"""Run only the supplied child under Windows native debugging; record fatal faults."""
import ctypes as c
from ctypes import wintypes as w
import json
from pathlib import Path
import struct
import subprocess
import sys
import time

assert c.sizeof(c.c_void_p) == 8, 'x64 Python is required'
k = c.WinDLL('kernel32', use_last_error=True)
class Startup(c.Structure):
    _fields_ = [('cb', w.DWORD), ('reserved', w.LPWSTR), ('desktop', w.LPWSTR), ('title', w.LPWSTR),
                ('x', w.DWORD), ('y', w.DWORD), ('xs', w.DWORD), ('ys', w.DWORD),
                ('xc', w.DWORD), ('yc', w.DWORD), ('fill', w.DWORD), ('flags', w.DWORD),
                ('show', w.WORD), ('reserved2', w.WORD), ('bytes', c.c_void_p),
                ('stdin', w.HANDLE), ('stdout', w.HANDLE), ('stderr', w.HANDLE)]
class ProcessInfo(c.Structure):
    _fields_ = [('process', w.HANDLE), ('thread', w.HANDLE), ('pid', w.DWORD), ('tid', w.DWORD)]
assert c.sizeof(Startup) == 104 and c.sizeof(ProcessInfo) == 24
k.CreateProcessW.argtypes = [w.LPCWSTR, w.LPWSTR, c.c_void_p, c.c_void_p, w.BOOL, w.DWORD, c.c_void_p, w.LPCWSTR, c.POINTER(Startup), c.POINTER(ProcessInfo)]
k.CreateProcessW.restype = w.BOOL
k.WaitForDebugEvent.argtypes = [c.c_void_p, w.DWORD]
k.WaitForDebugEvent.restype = w.BOOL
k.ContinueDebugEvent.argtypes = [w.DWORD, w.DWORD, w.DWORD]
k.GetFinalPathNameByHandleW.argtypes = [w.HANDLE, w.LPWSTR, w.DWORD, w.DWORD]
k.CloseHandle.argtypes = [w.HANDLE]
k.TerminateProcess.argtypes = [w.HANDLE, w.UINT]
output = Path(sys.argv[1]).resolve()
argv = sys.argv[2:]
startup, process = Startup(), ProcessInfo()
startup.cb = c.sizeof(startup)
command = c.create_unicode_buffer(subprocess.list2cmdline(argv))
if not k.CreateProcessW(None, command, None, None, False, 0x08000002, None, None, c.byref(startup), c.byref(process)):
    raise c.WinError(c.get_last_error())
report = {'command': argv, 'pid': process.pid, 'fatalExceptions': [], 'accessViolations': [], 'status': 'running', 'exitCode': None}
modules, unloaded = {}, []
event = c.create_string_buffer(176)
def image_size(path):
    try:
        with open(path, 'rb') as f:
            f.seek(60)
            pe = struct.unpack('<I', f.read(4))[0]
            f.seek(pe + 24 + 56)
            return struct.unpack('<I', f.read(4))[0]
    except OSError:
        return 0

def module_at(address, values):
    return next((dict(m, offset=hex(address-m['base'])) for m in values if m['base'] <= address < m['base']+m['size']), None)
try:
    deadline = time.monotonic() + 90
    while time.monotonic() < deadline:
        if not k.WaitForDebugEvent(event, 500):
            if c.get_last_error() == 121:
                continue
            raise c.WinError(c.get_last_error())
        raw = event.raw
        kind, pid, tid = struct.unpack_from('<III', raw)
        decision = 0x00010002
        if kind in (3, 6):
            handle = struct.unpack_from('<Q', raw, 16)[0]
            base = struct.unpack_from('<Q', raw, 40 if kind == 3 else 24)[0]
            path = c.create_unicode_buffer(32768)
            if handle and k.GetFinalPathNameByHandleW(handle, path, len(path), 0):
                modules[base] = {'base': base, 'path': path.value, 'size': image_size(path.value)}
            if handle:
                k.CloseHandle(handle)
        elif kind == 7:
            base = struct.unpack_from('<Q', raw, 16)[0]
            if base in modules:
                unloaded.append(modules.pop(base))
        elif kind == 1:
            code = struct.unpack_from('<I', raw, 16)[0]
            address = struct.unpack_from('<Q', raw, 32)[0]
            first = struct.unpack_from('<I', raw, 168)[0]
            detail = {'code': hex(code), 'address': hex(address), 'threadId': tid, 'firstChance': first,
                      'module': module_at(address, modules.values()), 'unloadedModule': module_at(address, reversed(unloaded))}
            if code == 0xc0000005:
                detail['operation'], detail['target'] = struct.unpack_from('<QQ', raw, 48)
                report['accessViolations'].append(detail)
            if not first:
                report['fatalExceptions'].append(detail)
            decision = 0x00010002 if code == 0x80000003 else 0x80010001
        elif kind == 5:
            report['exitCode'] = struct.unpack_from('<I', raw, 16)[0]
            report['status'] = 'passed' if report['exitCode'] == 0 else 'failed'
        k.ContinueDebugEvent(pid, tid, decision)
        if kind == 5:
            break
    else:
        report['status'] = 'timeout'
        k.TerminateProcess(process.process, 124)
finally:
    k.CloseHandle(process.thread)
    k.CloseHandle(process.process)
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report, ensure_ascii=False))
sys.exit(0 if report['status'] == 'passed' else 1)
