using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Accessibility;

// Input structure, foreground gate and MSAA calibration reused from AUDIT-04.
// Runs one non-destructive scenario against an isolated production EXE.
internal static class HotkeyEnterRealProbe
{
    delegate bool EnumProc(IntPtr h,IntPtr p);
    delegate IntPtr HookProc(int code,IntPtr message,IntPtr data);
    [StructLayout(LayoutKind.Sequential)] struct KeyInput { public ushort Vk,Scan;public uint Flags,Time;public UIntPtr Extra; }
    [StructLayout(LayoutKind.Sequential)] struct MouseInput { public int X,Y;public uint Data,Flags,Time;public UIntPtr Extra; }
    [StructLayout(LayoutKind.Explicit,Size=32)] struct Union { [FieldOffset(0)]public KeyInput Key;[FieldOffset(0)]public MouseInput Mouse; }
    [StructLayout(LayoutKind.Sequential)] struct Input { public uint Type;public Union Value; }
    [StructLayout(LayoutKind.Sequential)] struct LowKey { public uint Vk,Scan,Flags,Time;public UIntPtr Extra; }
    [StructLayout(LayoutKind.Sequential)] struct Point { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] struct Gui {public uint Size,Flags;public IntPtr Active,Focus,Capture,MenuOwner,MoveSize,Caret;public int L,T,R,B;}
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb,IntPtr p);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr h,EnumProc cb,IntPtr p);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr h,StringBuilder s,int count);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr h,StringBuilder s,int count);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] static extern bool IsWindowEnabled(IntPtr h);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] static extern bool GetGUIThreadInfo(uint thread,ref Gui info);
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] static extern uint SendInput(uint count,Input[] input,int size);
    [DllImport("user32.dll")] static extern uint MapVirtualKey(uint key,uint mode);
    [DllImport("user32.dll")] static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(Point point);
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr h,int msg,IntPtr wp,IntPtr lp);
    [DllImport("user32.dll")] static extern IntPtr GetMenu(IntPtr h);
    [DllImport("user32.dll")] static extern IntPtr GetSubMenu(IntPtr h,int index);
    [DllImport("user32.dll")] static extern int GetMenuItemCount(IntPtr h);
    [DllImport("user32.dll")] static extern uint GetMenuItemID(IntPtr h,int index);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetMenuString(IntPtr h,uint index,StringBuilder text,int count,uint flags);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h,int msg,IntPtr wp,IntPtr lp);
    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int id,HookProc cb,IntPtr module,uint thread);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr message,IntPtr data);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern IntPtr GetModuleHandle(string name);
    [DllImport("oleacc.dll")] static extern int AccessibleObjectFromWindow(IntPtr h,uint id,ref Guid iid,[MarshalAs(UnmanagedType.Interface)] out IAccessible a);
    const ulong Marker=0xE0102026;
    static bool AllKeysTargeted;static int Pid,ForeignKeys;static string Exe,Output,Doc;static readonly List<object> Events=new List<object>();
    static readonly HookProc Hook=OnKey;
    static readonly JavaScriptSerializer Json=new JavaScriptSerializer();
    static IntPtr OnKey(int code,IntPtr message,IntPtr data) {
        if(code>=0){var key=(LowKey)Marshal.PtrToStructure(data,typeof(LowKey));uint owner;GetWindowThreadProcessId(GetForegroundWindow(),out owner);
            if(key.Extra.ToUInt64()==Marker){if(owner!=Pid)AllKeysTargeted=false;Events.Add(new {message=message.ToInt32(),vk=key.Vk,scan=key.Scan,flags=key.Flags,pid=owner,target=owner==Pid});}else ForeignKeys++;}
        return CallNextHookEx(IntPtr.Zero,code,message,data);
    }
    static string Text(IntPtr h,bool cls=false){var b=new StringBuilder(512);if(cls)GetClassName(h,b,b.Capacity);else GetWindowText(h,b,b.Capacity);return b.ToString();}
    static IntPtr Window(string title){IntPtr found=IntPtr.Zero;int count=0;EnumWindows((h,p)=>{uint owner;GetWindowThreadProcessId(h,out owner);if(owner==Pid&&IsWindowVisible(h)&&Text(h)==title){found=h;count++;}return true;},IntPtr.Zero);return count==1?found:IntPtr.Zero;}
    static IntPtr WaitWindow(string title){var watch=Stopwatch.StartNew();IntPtr h;do{h=Window(title);if(h!=IntPtr.Zero)return h;Thread.Sleep(40);}while(watch.ElapsedMilliseconds<4000);throw new InvalidOperationException("Window not observed: "+title);}
    static IntPtr Focus(IntPtr h){uint owner;uint thread=GetWindowThreadProcessId(h,out owner);var g=new Gui{Size=(uint)Marshal.SizeOf(typeof(Gui))};return GetGUIThreadInfo(thread,ref g)?g.Focus:IntPtr.Zero;}
    static void Foreground(IntPtr h){if(!IsWindowVisible(h)||!IsWindowEnabled(h))throw new InvalidOperationException("Input window unavailable");SetForegroundWindow(h);Thread.Sleep(120);if(GetForegroundWindow()!=h)throw new InvalidOperationException("Foreground not confirmed");}
    static IAccessible Accessible(IntPtr h){var id=new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");IAccessible a;if(AccessibleObjectFromWindow(h,0xFFFFFFFC,ref id,out a)<0)throw new InvalidOperationException("MSAA unavailable");return a;}
    static Input Key(ushort vk,bool up){return new Input{Type=1,Value=new Union{Key=new KeyInput{Vk=vk,Scan=(ushort)MapVirtualKey(vk,0),Flags=(up?2U:0U)|((vk>=0x21&&vk<=0x28)?1U:0U),Extra=new UIntPtr(Marker)}}};}
    static void Record(string step,object detail){File.WriteAllText(Path.Combine(Output,step+".json"),Json.Serialize(new{step,pid=Pid,executable=Exe,time=DateTimeOffset.Now.ToString("o"),detail}),new UTF8Encoding(false));File.AppendAllText(Doc,"\n- HOTKEY-ENTER-01 真实 EXE 步骤 "+step+" 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/"+Path.GetFileName(Output)+"/"+step+".json；完整结论以 final-result.json 为准。\n",new UTF8Encoding(false));}
    static void Send(IntPtr window,string step,params ushort[] keys){
        Foreground(window);foreach(int mod in new[]{16,17,18,91,92})if((GetAsyncKeyState(mod)&0x8000)!=0)throw new InvalidOperationException("Physical modifier held");
        Events.Clear();ForeignKeys=0;AllKeysTargeted=true;var inputs=new List<Input>();foreach(var key in keys)inputs.Add(Key(key,false));for(int i=keys.Length-1;i>=0;i--)inputs.Add(Key(keys[i],true));
        IntPtr hook=SetWindowsHookEx(13,Hook,GetModuleHandle(null),0);if(hook==IntPtr.Zero)throw new InvalidOperationException("Hook unavailable");uint sent=0;
        try{sent=SendInput((uint)inputs.Count,inputs.ToArray(),Marshal.SizeOf(typeof(Input)));var w=Stopwatch.StartNew();while(w.ElapsedMilliseconds<350){Application.DoEvents();Thread.Sleep(10);}}
        finally{UnhookWindowsHookEx(hook);}
        Record(step,new {window=window.ToInt64(),requested=inputs.Count,sent,foreignKeys=ForeignKeys,events=Events.ToArray()});
        if(sent!=inputs.Count||Events.Count!=inputs.Count||ForeignKeys!=0||!AllKeysTargeted)throw new InvalidOperationException("Input incomplete/interfered");
    }
    static int MenuCommand(IntPtr menu,string label){for(int i=0;i<GetMenuItemCount(menu);i++){var b=new StringBuilder(256);GetMenuString(menu,(uint)i,b,b.Capacity,0x400);if(b.ToString().Split('	')[0]==label)return (int)GetMenuItemID(menu,i);var sub=GetSubMenu(menu,i);if(sub!=IntPtr.Zero){int found=MenuCommand(sub,label);if(found>=0)return found;}}return -1;}
    static string Hash(string path){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
    [STAThread]static int Main(string[] args){
        SetProcessDPIAware();
        Exe=Path.GetFullPath(args[0]);Output=Path.GetFullPath(args[1]);Doc=Path.GetFullPath(args[2]);Directory.CreateDirectory(Output);
        if(Exe.IndexOf(@"\artifacts\runs\hotkey_enter_action_20261023\app\",StringComparison.OrdinalIgnoreCase)<0)return 2;
        var result=new Dictionary<string,object>();Process app=null;int code=3;bool forced=false;
        try{
            app=Process.Start(new ProcessStartInfo(Exe){WorkingDirectory=Path.GetDirectoryName(Exe),UseShellExecute=false});Pid=app.Id;app.WaitForInputIdle(8000);Thread.Sleep(2500);
            if(!string.Equals(app.MainModule.FileName,Exe,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Ownership mismatch");
            IntPtr main=WaitWindow("沙河批量打开器");string config=Path.Combine(Path.GetDirectoryName(Exe),"Configuration","shahe_config.ini");string before=Hash(config);
            Record("startup",new{main=main.ToInt64(),configHash=before,exeHash=Hash(Exe)});
            int menuCommand=MenuCommand(GetMenu(main),"热键设置");if(menuCommand<0||!PostMessage(main,0x111,new IntPtr(menuCommand),IntPtr.Zero))throw new InvalidOperationException("Hotkey menu command unavailable");
            Record("open-hotkeys",new{method="existing native menu command",menuCommand,label="热键设置"});
            IntPtr dialog=WaitWindow("快捷键设置"),list=IntPtr.Zero;
            EnumChildWindows(dialog,(h,p)=>{if(Text(h,true).IndexOf("LISTBOX",StringComparison.OrdinalIgnoreCase)>=0)list=h;return true;},IntPtr.Zero);
            if(list==IntPtr.Zero)throw new InvalidOperationException("Action list absent");
            var a=Accessible(list);int child=0,count=0,x=0,y=0,width=0,height=0;
            try{for(int i=1;i<=a.accChildCount;i++){if(a.get_accName(i)=="设置"){child=i;count++;}}if(count!=1)throw new InvalidOperationException("Settings action ambiguous");a.accLocation(out x,out y,out width,out height,child);}
            finally{Marshal.ReleaseComObject(a);}
            Foreground(dialog);
            if(Focus(dialog)!=list)throw new InvalidOperationException("Hotkey action list focus absent");
            Send(dialog,"select-first",36);
            for(int i=1;i<child;i++)Send(dialog,"select-next-"+i,40);
            int selected=SendMessage(list,0x188,IntPtr.Zero,IntPtr.Zero).ToInt32();
            Record("selected-action",new{list=list.ToInt64(),label="设置",index=child-1,selected,focus=Focus(dialog).ToInt64(),bounds=new{x,y,width,height},noActionYet=Window("设置")==IntPtr.Zero});
            if(selected!=child-1||Focus(dialog)!=list||Window("设置")!=IntPtr.Zero)throw new InvalidOperationException("Selection/focus not calibrated or action ran before Enter");
            Send(dialog,"execute-enter",13);
            IntPtr settings=WaitWindow("设置");bool hotkeysClosed=Window("快捷键设置")==IntPtr.Zero;
            Record("settings-opened",new{settings=settings.ToInt64(),hotkeysClosed,configUnchanged=Hash(config)==before,mainDisabled=!IsWindowEnabled(main)});
            if(!hotkeysClosed||Hash(config)!=before)throw new InvalidOperationException("Modal handoff/config preservation failed");
            Send(settings,"cancel-settings",27);Thread.Sleep(150);
            bool returned=Window("设置")==IntPtr.Zero&&IsWindowVisible(main)&&IsWindowEnabled(main)&&GetForegroundWindow()==main;
            Record("returned-main",new{returned,focus=Focus(main).ToInt64(),configUnchanged=Hash(config)==before});
            if(!returned||Hash(config)!=before)throw new InvalidOperationException("Cancel return/config preservation failed");
            result["passed"]=1;code=0;
        }catch(Exception e){result["error"]=e.ToString();result["passed"]=0;}
        finally{if(app!=null&&!app.HasExited&&string.Equals(app.MainModule.FileName,Exe,StringComparison.OrdinalIgnoreCase)){foreach(string title in new[]{"快捷键设置","设置"}){var owned=Window(title);if(owned!=IntPtr.Zero)PostMessage(owned,0x10,IntPtr.Zero,IntPtr.Zero);}Thread.Sleep(150);app.CloseMainWindow();if(!app.WaitForExit(3000)){app.Kill();app.WaitForExit(2000);forced=true;}}if(app!=null){result["exited"]=app.HasExited;if(app.HasExited)result["exitCode"]=app.ExitCode;}result["forcedCleanup"]=forced;if(forced)code=3;}
        result["pid"]=Pid;result["executable"]=Exe;result["returnCode"]=code;result["failed"]=0;result["inconclusive"]=code==0?0:1;
        File.WriteAllText(Path.Combine(Output,"final-result.json"),Json.Serialize(result),new UTF8Encoding(false));
        File.AppendAllText(Doc,"\n- HOTKEY-ENTER-01 真实 EXE 最终返回码 "+code+"；完整命令、键事件、窗口/索引和清理见 artifacts/runs/hotkey_enter_action_20261023/"+Path.GetFileName(Output)+"/final-result.json。\n",new UTF8Encoding(false));
        return code;
    }
}
