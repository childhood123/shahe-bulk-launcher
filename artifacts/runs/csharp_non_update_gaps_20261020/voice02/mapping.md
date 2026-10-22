# VOICE-02 读屏配置/运行时目录映射

Python `features/speech.py` 将可写设置保存到 `Configuration`，读取时兼容 `Configuration`、旧 `internal`/资源路径，并在加载 Tolk 前把读屏运行时目录加入 `PATH`。发布副本的 C# 资源位于 `lib`，其中包含 Tolk、厂商 DLL、`ZDSRAPI.ini` 和 `byctrl.conf`。
目标：把 `lib` 纳入配置读取回退，并在加载辅助后端前将实际 Tolk 所在目录加入 PATH；保留 Configuration 写入边界和现有接口。
RED 覆盖 lib INI 读取和加载后端观察到的 PATH；随后做定向、Speech/Tolk 受影响、全量、Python、compileall、Release、差异检查。真实厂商朗读仍需发布目录审计。
