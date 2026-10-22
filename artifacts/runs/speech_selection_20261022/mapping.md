# 手动读屏接口真实路由

用户已明确要求修复先前识别的读屏下拉框仅保存问题，作为原 Python 遗留缺陷的独立 C# 功能修复。UI 中文文案、自动/NVDA/争渡/保益顺序、配置键均不改变。

入口 SpeechSettingsDialog.ConfirmSettings → SpeechService.SaveSpeechInterfaceDll → 保存设置/释放旧后端 → SpeakAssistText → EnsureAssistSpeechRuntime。自动模式继续 Tolk；手动模式绑定已有 nvdaControllerClient64.dll、ZDSRAPI_x64.dll、byctrl-x64.dll，不添加下载依赖。

底层函数签名参照本地已构建版本 Tolk 的对应驱动源码（artifacts/probes/tolk-downloaded/boz700908-tolk-add14ac/src），并通过上游源码核对。只使用公开导出 API；C# 复用已有 SpeechAssistBackend 委托模型、加载/卸载和串行发声锁。

验证覆盖四种路由、切换/保存/自动恢复、缺失 DLL 时临时自动回退且保留保存偏好、关闭后不再重建、后台请求二次检查提示开关；实际环境只对当前可用读屏采集输出/返回值证据，未安装读屏作为环境限制记录。

用户追加要求：三个支持的读屏均未运行时回退 Windows 系统语音。自动模式沿用 Tolk 的系统回退；手动选择时若未检测到这三类读屏进程则暂用 Tolk Windows 优先模式，保存的接口不改变；目标读屏启动后，下次输出重新连接所选接口。

用户进一步澄清：手动接口始终是固定保存偏好；所选读屏未运行/接口不可用时临时回退自动检测（其他读屏或 Windows），选择本身不改；所选读屏恢复后再回到它。点确定不得直接重置为自动。以下测试以此最新规则为准，先前“仅全部未运行才回退”的设计已被细化。
