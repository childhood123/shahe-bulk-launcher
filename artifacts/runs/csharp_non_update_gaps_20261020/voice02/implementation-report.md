# VOICE-02 结果

SpeechService 现在从 `Configuration` 写入、从发布 `lib` 读取 `ZDSRAPI.ini`/`byctrl.conf` 兼容默认值；加载 Tolk 前将实际 Tolk 所在目录加入 PATH，使 ZDSR、保益和 NVDA 伴随 DLL 按发布目录解析。

定向 2/2、Speech/Application/Updates 受影响 167/167、C# 全量 343/343、Python 29/29、厂商二进制探针 3/3、compileall 返回 0、Release x64 0 警告/0 错误、差异检查返回 0。真实读屏输出、设备配置和干净发布目录仍需 AUDIT-04/AUDIT-07。
