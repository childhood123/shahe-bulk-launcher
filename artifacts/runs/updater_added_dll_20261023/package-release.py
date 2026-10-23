import hashlib
import json
import shutil
import zipfile
from pathlib import Path

run = Path(__file__).resolve().parent
repo = run.parents[2]
staging = run / "package"
if staging.exists():
    raise RuntimeError("Use a fresh package directory; do not overwrite earlier evidence")
staging.mkdir()
for source in (run / "publish").rglob("*"):
    if not source.is_file() or source.suffix.lower() == ".pdb":
        continue
    relative = source.relative_to(run / "publish")
    if relative.parts[0] in ("Configuration", "Configurations", "log", "tenp"):
        raise RuntimeError("Publish output contains user data: " + str(relative))
    target = staging / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, target)
for name in ("updater.exe", "updater.exe.config"):
    shutil.copy2(run / "bin" / "x64" / "Release" / "net48" / name, staging / name)
(staging / "Configurations").mkdir()
assert not any((staging / "Configurations").iterdir())
assert len(list((staging / "打赏").glob("*.jpg"))) == 2
assert len(list((staging / "lib").iterdir())) == 7
inventory = {p.relative_to(staging).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest().upper()
             for p in staging.rglob("*") if p.is_file()}
assert not any("test" in name.lower() or "AuditRunner" in name for name in inventory)
archive = run / "shahe-bulk-launcher-4.0.47-win-x64.zip"
with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as target:
    target.writestr("Configurations/", b"")
    for name in sorted(inventory):
        target.write(staging / name, name)
with zipfile.ZipFile(archive) as target:
    assert target.read("Configurations/") == b""
    assert not any(name.startswith("Configurations/") and name != "Configurations/" for name in target.namelist())
    assert set(target.namelist()) == set(inventory) | {"Configurations/"}
result = dict(returnCode=0, version="4.0.47", package=str(archive), size=archive.stat().st_size,
              sha256=hashlib.sha256(archive.read_bytes()).hexdigest().upper(), configurationsEmpty=True,
              inventory=inventory, entries=len(inventory)+1)
(run / "package-result.json").write_text(json.dumps(result, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
(run / "expected-inventory.json").write_text(json.dumps(inventory, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
with (repo / "PROJECT_STRUCTURE.md").open("a", encoding="utf-8", newline="\n") as doc:
    doc.write("\n- UPDATE-DLL-01 完整发布包组装/校验返回码 0；Configurations 目录存在且为空，打赏原图及 lib 运行文件齐全，无个人配置、测试或探针。文件清单/大小/SHA-256 见 artifacts/runs/updater_added_dll_20261023/package-result.json，脚本 package-release.py。\n")
print(json.dumps({k:v for k,v in result.items() if k!="inventory"}, ensure_ascii=True))
