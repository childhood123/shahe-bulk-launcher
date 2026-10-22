using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaheLauncher.Core.Common;

namespace ShaheLauncher.Core.Speech
{
    public sealed class SpeechAssistBackend
    {
        public Action<bool> TrySapi { get; set; }
        public Action<bool> PreferSapi { get; set; }
        public Func<string> DetectScreenReader { get; set; }
        public Action Load { get; set; }
        public Func<bool> IsLoaded { get; set; }
        public Action Silence { get; set; }
        public Action<string, bool> Output { get; set; }
        public Action Unload { get; set; }
    }

    public sealed class SpeechSettingsRequest
    {
        public SpeechSettingsRequest()
        {
            SpeechConfig = new Dictionary<string, object>();
            ReaderIniSettings = new Dictionary<string, object>();
            InterfaceDllChoices = new List<object>();
            SelectedInterfaceDll = string.Empty;
            AssistHints = new Dictionary<string, object>();
        }

        public IDictionary<string, object> SpeechConfig { get; set; }
        public IDictionary<string, object> ReaderIniSettings { get; set; }
        public IList<object> InterfaceDllChoices { get; set; }
        public string SelectedInterfaceDll { get; set; }
        public IDictionary<string, object> AssistHints { get; set; }
        public Func<IDictionary<string, object>, bool> SaveReaderIniSettings { get; set; }
        public Func<string, bool> SaveInterfaceDll { get; set; }
        public Func<IDictionary<string, object>, bool> SaveAssistHints { get; set; }
        public Func<IDictionary<string, object>, bool> SaveSpeechConfig { get; set; }
    }

    public sealed class SpeechServiceCallbacks
    {
        public Func<IEnumerable<string>> EnumerateProcessNames { get; set; }
        public Func<string, SpeechAssistBackend> LoadAssistBackend { get; set; }
        public Func<bool> IsWindowClosing { get; set; }
        public Func<bool> IsDestroyStarted { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action<string, string> ShowWarning { get; set; }
        public Func<SpeechSettingsRequest, bool> ShowSettings { get; set; }
        public Action CreateMenuBar { get; set; }
        public Action SaveSettings { get; set; }
    }

    public sealed class SpeechService : IDisposable
    {
        private static readonly HashSet<string> BlockedEngines = new HashSet<string>(StringComparer.Ordinal)
        {
            "VTTS", "None", "_None", "无语音输出_None", "无语音输出"
        };

        private static readonly HashSet<string> AllowedAssistEventKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "mute_current",
            "mute_all",
            "volume_current",
            "volume_all",
            "switch_output_current",
            "switch_output_all",
            "close_current",
            "close_all",
            "locate_current",
            "launch_done"
        };

        private readonly object _assistSpeechInitLock = new object();
        private readonly object _assistSpeechCallLock = new object();
        private readonly object _assistDedupLock = new object();
        private SpeechAssistBackend _assistSpeechRuntime;
        private bool _assistSpeechShutdown;
        private string _assistSpeechRoute = string.Empty;
        private string _assistLastText = string.Empty;
        private DateTime _assistLastTimeUtc = DateTime.MinValue;

        public SpeechService(
            string appDirectory,
            string configurationDirectory,
            SpeechServiceCallbacks callbacks)
        {
            AppDirectory = GetFullPathSafely(appDirectory);
            ConfigurationDirectory = GetFullPathSafely(configurationDirectory);
            Callbacks = callbacks ?? new SpeechServiceCallbacks();
            SpeechInterfaceDll = string.Empty;
            SpeechAssistHints = DefaultAssistHints();
        }

        public string AppDirectory { get; private set; }
        public string ConfigurationDirectory { get; private set; }
        public SpeechServiceCallbacks Callbacks { get; private set; }
        public string SpeechInterfaceDll { get; private set; }
        public IDictionary<string, object> SpeechAssistHints { get; private set; }
        public bool DestroyStarted { get; set; }
        public bool AssistSpeechDisabled { get; private set; }

        public string GetSpeechSourceConfigPath()
        {
            string[] candidates =
            {
                Path.Combine(ConfigurationDirectory, "SpeechConfig.json"),
                Path.Combine(AppDirectory, "SpeechConfig.json"),
                Path.Combine(AppDirectory, "internal", "SpeechConfig.json"),
                Path.Combine(AppDirectory, "Speech", "SpeechConfig.json"),
                Path.Combine(AppDirectory, "py_work", "游戏", "哲隐语音组件", "Speech", "SpeechConfig.json")
            };
            return FirstExistingFile(candidates);
        }

        public bool EnsureWritableDirectory(string folderPath)
        {
            try
            {
                string path = Text(folderPath).Trim();
                if (path.Length == 0)
                {
                    return false;
                }

                Directory.CreateDirectory(path);
                string testPath = Path.Combine(path, ".write_test.tmp");
                File.WriteAllText(testPath, "ok", new UTF8Encoding(false));
                File.Delete(testPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetSpeechRuntimeDirectory()
        {
            EnsureWritableDirectory(ConfigurationDirectory);
            return ConfigurationDirectory;
        }

        public string GetTolkDllPath()
        {
            return FirstExistingFile(new[]
            {
                Path.Combine(AppDirectory, "lib", "Tolk.dll"),
                Path.Combine(AppDirectory, "internal", "Tolk.dll"),
                Path.Combine(AppDirectory, "Tolk.dll")
            });
        }

        public string GetReaderIniPath(string filename, bool forWrite)
        {
            string name = Text(filename).Trim();
            if (name.Length == 0)
            {
                return string.Empty;
            }

            string configPath = Path.Combine(ConfigurationDirectory, name);
            if (forWrite)
            {
                EnsureWritableDirectory(ConfigurationDirectory);
                return configPath;
            }

            string existing = FirstExistingFile(new[]
            {
                configPath,
                Path.Combine(AppDirectory, "internal", name),
                Path.Combine(AppDirectory, "lib", name),
                Path.Combine(AppDirectory, name)
            });
            if (existing.Length != 0)
            {
                return existing;
            }

            EnsureWritableDirectory(ConfigurationDirectory);
            return configPath;
        }

        public IDictionary<string, object> LoadReaderIniSettings()
        {
            IDictionary<string, object> result = DefaultReaderIniSettings();
            string zdsrPath = GetReaderIniPath("ZDSRAPI.ini", false);
            IDictionary<string, string> zdsr = ReadIniSection(zdsrPath, "settings");
            if (zdsr != null)
            {
                IDictionary<string, object> target = (IDictionary<string, object>)result["zdsr"];
                foreach (string key in new[] { "type", "channelName", "keyDownInterrupt", "alwaysInterrupt" })
                {
                    string value;
                    if (zdsr.TryGetValue(key, out value))
                    {
                        target[key] = Text(value).Trim();
                    }
                }
            }

            string boyPath = GetReaderIniPath("boyctrl.ini", false);
            IDictionary<string, string> boy = ReadIniSection(boyPath, "Config");
            if (boy != null)
            {
                IDictionary<string, object> target = (IDictionary<string, object>)result["boy"];
                foreach (string key in new[] { "Param1", "Param2", "Param3", "Param4" })
                {
                    string value;
                    if (boy.TryGetValue(key, out value))
                    {
                        target[key] = string.Equals(Text(value).Trim(), "true", StringComparison.OrdinalIgnoreCase)
                            ? "true"
                            : "false";
                    }
                }
            }
            return result;
        }

        public bool SaveReaderIniSettings(IDictionary<string, object> settings)
        {
            try
            {
                IDictionary<string, object> data = NormalizeReaderIniSettings(settings);
                IDictionary<string, object> zdsr = AsDictionary(data["zdsr"]);
                IDictionary<string, object> boy = AsDictionary(data["boy"]);
                WriteIniSection(
                    GetReaderIniPath("ZDSRAPI.ini", true),
                    "settings",
                    new[] { "type", "channelName", "keyDownInterrupt", "alwaysInterrupt" },
                    zdsr);
                WriteIniSection(
                    GetReaderIniPath("boyctrl.ini", true),
                    "Config",
                    new[] { "Param1", "Param2", "Param3", "Param4" },
                    boy);
                return true;
            }
            catch (Exception error)
            {
                Callbacks.SetStatus?.Invoke("读屏配置保存失败: " + error.Message);
                return false;
            }
        }

        private IList<string> ReadScreenReaderProcessNames()
        {
            var names = new List<string>();
            try
            {
                IEnumerable<string> supplied = Callbacks.EnumerateProcessNames != null
                    ? Callbacks.EnumerateProcessNames()
                    : null;
                if (supplied != null)
                {
                    names.AddRange(supplied.Select(Text).Where(value => value.Length != 0).Select(value => value.ToLowerInvariant()));
                }
                else
                {
                    foreach (Process process in Process.GetProcesses())
                    {
                        try
                        {
                            string name = Text(process.ProcessName).Trim();
                            if (name.Length != 0)
                            {
                                names.Add(name.ToLowerInvariant());
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch
            {
                names.Clear();
            }

            return names;
        }

        public IDictionary<string, object> DetectScreenReaderAndMatchEngine(IEnumerable<string> engineNames)
        {
            IList<string> names = ReadScreenReaderProcessNames();
            string readerKey = string.Empty;
            string readerName = string.Empty;
            string processName = string.Empty;
            var patterns = new[]
            {
                Tuple.Create("zdsr", "争渡读屏", new[] { "zdsr", "zdsrapi", "saapi" }),
                Tuple.Create("boy", "保益读屏", new[] { "boyctrl", "boy" }),
                Tuple.Create("nvda", "NVDA", new[] { "nvda" })
            };
            foreach (var pattern in patterns)
            {
                string matched = names.FirstOrDefault(name => pattern.Item3.Any(name.Contains));
                if (matched == null)
                {
                    continue;
                }

                readerKey = pattern.Item1;
                readerName = pattern.Item2;
                processName = matched;
                break;
            }

            string engineName = string.Empty;
            if (readerKey.Length != 0 && engineNames != null)
            {
                IDictionary<string, string[]> keywordMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    { "zdsr", new[] { "争渡", "zdsr" } },
                    { "boy", new[] { "保益", "boy" } },
                    { "nvda", new[] { "nvda" } }
                };
                string[] keys = keywordMap[readerKey];
                foreach (string engine in engineNames)
                {
                    string text = Text(engine).Trim();
                    string lower = text.ToLowerInvariant();
                    if (keys.Any(key => lower.Contains(key.ToLowerInvariant()) || text.Contains(key)))
                    {
                        engineName = text;
                        break;
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "reader_key", readerKey },
                { "reader_name", readerName },
                { "process_name", processName },
                { "engine_name", engineName }
            };
        }

        public IList<object> ScanReaderInterfaceDlls()
        {
            return new List<object>
            {
                new Dictionary<string, object> { { "label", "NVDA" }, { "path", "nvda" } },
                new Dictionary<string, object> { { "label", "争渡" }, { "path", "zdsr" } },
                new Dictionary<string, object> { { "label", "保益" }, { "path", "boy" } }
            };
        }

        public string NormalizeSpeechInterfaceValue(object value)
        {
            string text = Text(value).Trim();
            string lowerText = text.ToLowerInvariant();
            string basename = string.Empty;
            try
            {
                basename = Path.GetFileName(text).ToLowerInvariant();
            }
            catch
            {
            }

            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "nvda", "nvda" },
                { "nvdacontrollerclient.dll", "nvda" },
                { "nvdacontrollerclient64.dll", "nvda" },
                { "zdsr", "zdsr" },
                { "zdsrapi.dll", "zdsr" },
                { "zdsrapi_x64.dll", "zdsr" },
                { "争渡", "zdsr" },
                { "boy", "boy" },
                { "boyctrl.dll", "boy" },
                { "boyctrl-x64.dll", "boy" },
                { "byctrl.dll", "boy" },
                { "byctrl-x64.dll", "boy" },
                { "保益", "boy" }
            };
            string normalized;
            return mapping.TryGetValue(lowerText, out normalized)
                ? normalized
                : mapping.TryGetValue(basename, out normalized) ? normalized : string.Empty;
        }

        public bool IsAllowedSpeechInterfaceDllPath(object value)
        {
            return NormalizeSpeechInterfaceValue(value).Length != 0;
        }

        public string GetPreferredSpeechInterfaceDll(IEnumerable<string> choices)
        {
            IDictionary<string, object> detected = DetectScreenReaderAndMatchEngine(new string[0]);
            string key = Text(detected["reader_key"]).Trim().ToLowerInvariant();
            return new[] { "nvda", "zdsr", "boy" }.Contains(key, StringComparer.OrdinalIgnoreCase)
                ? key
                : string.Empty;
        }

        public string GetAssistSpeechEngineDirectory()
        {
            string tolkPath = GetTolkDllPath();
            if (tolkPath.Length != 0)
            {
                string directory = Path.GetDirectoryName(tolkPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    return directory;
                }
            }

            string libDirectory = Path.Combine(AppDirectory, "lib");
            if (Directory.Exists(libDirectory))
            {
                return libDirectory;
            }
            string internalDirectory = Path.Combine(AppDirectory, "internal");
            return Directory.Exists(internalDirectory) ? internalDirectory : AppDirectory;
        }

        private string GetAssistSpeechRoute()
        {
            if (SpeechInterfaceDll.Length == 0) return string.Empty;
            IList<string> names = ReadScreenReaderProcessNames();
            if (names.Any(name => MatchesReaderProcess(name, SpeechInterfaceDll))) return SpeechInterfaceDll;
            return names.Any(name => MatchesReaderProcess(name, "nvda")
                || MatchesReaderProcess(name, "zdsr") || MatchesReaderProcess(name, "boy"))
                ? string.Empty : "windows";
        }

        private static bool MatchesReaderProcess(string name, string reader)
        {
            switch (reader)
            {
                case "nvda": return name.Contains("nvda");
                case "zdsr": return name.Contains("zdsr") || name.Contains("zdsrapi") || name.Contains("saapi");
                case "boy": return name.Contains("boyctrl") || name.Contains("boy");
                default: return false;
            }
        }

        private string GetAssistSpeechLibraryPath()
        {
            return GetAssistSpeechLibraryPathForRoute(GetAssistSpeechRoute());
        }

        private string GetAssistSpeechLibraryPathForRoute(string route)
        {
            string filename;
            switch (route)
            {
                case "nvda": filename = "nvdaControllerClient64.dll"; break;
                case "zdsr": filename = "ZDSRAPI_x64.dll"; break;
                case "boy": filename = "byctrl-x64.dll"; break;
                default: return GetTolkDllPath();
            }
            return new[]
            {
                Path.Combine(AppDirectory, "lib", filename),
                Path.Combine(AppDirectory, "internal", filename),
                Path.Combine(AppDirectory, filename)
            }.FirstOrDefault(File.Exists) ?? string.Empty;
        }

        public bool EnsureAssistSpeechRuntime()
        {
            lock (_assistSpeechCallLock)
            {
                if (_assistSpeechShutdown) return false;
                string route = GetAssistSpeechRoute();
                lock (_assistSpeechInitLock)
                {
                    if (_assistSpeechRuntime != null && route == _assistSpeechRoute)
                    {
                        if (route.Length == 0 || route == "windows") return true;
                        try { if (_assistSpeechRuntime.IsLoaded()) return true; }
                        catch { }
                    }
                    if (AssistSpeechDisabled && route == _assistSpeechRoute) return false;
                    SpeechAssistBackend previous = _assistSpeechRuntime;
                    _assistSpeechRuntime = null;
                    AssistSpeechDisabled = false;
                    try { previous?.Unload?.Invoke(); }
                    catch { }
                    if (TryLoadAssistSpeechRuntime(route)) return true;
                    if (route == "nvda" || route == "zdsr" || route == "boy")
                    {
                        // Keep the saved reader; this fallback affects only the current runtime.
                        if (TryLoadAssistSpeechRuntime(string.Empty)) return true;
                    }
                    AssistSpeechDisabled = SpeechInterfaceDll.Length == 0;
                    return false;
                }
            }
        }

        private bool TryLoadAssistSpeechRuntime(string route)
        {
            _assistSpeechRoute = route;
            string libraryPath = GetAssistSpeechLibraryPathForRoute(route);
            if (libraryPath.Length == 0) return false;
            PrependSpeechEngineDirectoryToPath(Path.GetDirectoryName(libraryPath));
            SpeechAssistBackend backend = null;
            try
            {
                backend = Callbacks.LoadAssistBackend != null
                    ? Callbacks.LoadAssistBackend(libraryPath)
                    : route.Length == 0 || route == "windows"
                        ? CreateNativeBackend(libraryPath)
                        : CreateReaderBackend(libraryPath, route);
                if (backend == null || backend.Load == null || backend.IsLoaded == null)
                    throw new InvalidOperationException("Tolk 运行时接口不完整");
                backend.TrySapi?.Invoke(true);
                backend.PreferSapi?.Invoke(route == "windows");
                backend.Load();
                if (!backend.IsLoaded()) throw new InvalidOperationException("Tolk 初始化失败");
                _assistSpeechRuntime = backend;
                return true;
            }
            catch
            {
                try { backend?.Unload?.Invoke(); }
                catch { }
                return false;
            }
        }

        public void SpeakAssistText(string text, bool interrupt)
        {
            string content = Text(text).Trim();
            if (content.Length == 0 || DestroyStarted || IsWindowClosing())
            {
                return;
            }
            if (!CommonUtilities.ParseBoolSetting(GetValue(SpeechAssistHints, "enabled", false), false))
            {
                return;
            }

            Task.Run(delegate
            {
                lock (_assistSpeechCallLock)
                {
                    if (IsWindowClosing()
                        || !CommonUtilities.ParseBoolSetting(GetValue(SpeechAssistHints, "enabled", false), false)
                        || !EnsureAssistSpeechRuntime())
                    {
                        return;
                    }

                    SpeechAssistBackend runtime = _assistSpeechRuntime;
                    if (runtime == null)
                    {
                        return;
                    }
                    try
                    {
                        if (interrupt)
                        {
                            try
                            {
                                runtime.Silence?.Invoke();
                            }
                            catch
                            {
                            }
                        }
                        runtime.Output?.Invoke(content, interrupt);
                    }
                    catch
                    {
                    }
                }
            });
        }

        public void ShutdownAssistSpeechRuntime()
        {
            lock (_assistSpeechCallLock)
            {
                SpeechAssistBackend backend;
                lock (_assistSpeechInitLock)
                {
                    backend = _assistSpeechRuntime;
                    _assistSpeechRuntime = null;
                    AssistSpeechDisabled = true;
                    _assistSpeechShutdown = true;
                }

                try
                {
                    backend?.Unload?.Invoke();
                }
                catch
                {
                }
            }
        }

        public bool SaveSpeechInterfaceDll(string dllPath)
        {
            string selected = NormalizeSpeechInterfaceValue(dllPath);
            lock (_assistSpeechCallLock)
            {
                SpeechAssistBackend previous = null;
                lock (_assistSpeechInitLock)
                {
                    if (selected != SpeechInterfaceDll || (AssistSpeechDisabled && !_assistSpeechShutdown))
                    {
                        previous = _assistSpeechRuntime;
                        _assistSpeechRuntime = null;
                        AssistSpeechDisabled = _assistSpeechShutdown;
                    }
                    SpeechInterfaceDll = selected;
                }
                try { previous?.Unload?.Invoke(); }
                catch { }
            }
            Callbacks.SaveSettings?.Invoke();
            return true;
        }

        public bool SaveSpeechAssistHints(IDictionary<string, object> hints)
        {
            IDictionary<string, object> source = hints ?? new Dictionary<string, object>();
            bool enabled = CommonUtilities.ParseBoolSetting(
                GetValue(source, "enabled", GetValue(SpeechAssistHints, "enabled", false)), false);
            string muteCurrent = Text(GetValue(source, "mute_current", string.Empty)).Trim();
            string muteAll = Text(GetValue(source, "mute_all", string.Empty)).Trim();
            if (muteCurrent.Length == 0)
            {
                muteCurrent = Text(GetValue(SpeechAssistHints, "mute_current", string.Empty)).Trim();
                if (muteCurrent.Length == 0)
                {
                    muteCurrent = "静音当前打开的进程";
                }
            }
            if (muteAll.Length == 0)
            {
                muteAll = Text(GetValue(SpeechAssistHints, "mute_all", string.Empty)).Trim();
                if (muteAll.Length == 0)
                {
                    muteAll = "静音全部打开的进程";
                }
            }
            SpeechAssistHints = new Dictionary<string, object>
            {
                { "enabled", enabled },
                { "mute_current", muteCurrent },
                { "mute_all", muteAll }
            };
            Callbacks.SaveSettings?.Invoke();
            return true;
        }

        public string WithAssistHint(string hintKey, string statusText)
        {
            string text = Text(statusText).Trim();
            if (!CommonUtilities.ParseBoolSetting(GetValue(SpeechAssistHints, "enabled", false), false))
            {
                return text;
            }

            string hint = Text(GetValue(SpeechAssistHints, Text(hintKey).Trim(), string.Empty)).Trim();
            return hint.Length == 0 ? text : hint + "，" + text;
        }

        public void SpeakProcessAssist(string text, bool interrupt, string eventKey)
        {
            string key = Text(eventKey).Trim().ToLowerInvariant();
            if (key.Length != 0 && !AllowedAssistEventKeys.Contains(key))
            {
                return;
            }

            string content = Text(text).Trim();
            if (content.Length == 0)
            {
                return;
            }

            lock (_assistDedupLock)
            {
                DateTime now = DateTime.UtcNow;
                if (string.Equals(content, _assistLastText, StringComparison.Ordinal)
                    && (now - _assistLastTimeUtc).TotalSeconds < 0.45)
                {
                    return;
                }
                _assistLastText = content;
                _assistLastTimeUtc = now;
            }
            SpeakAssistText(content, interrupt);
        }

        public string GetSpeechConfigPath()
        {
            string runtimeDirectory = GetSpeechRuntimeDirectory();
            if (runtimeDirectory.Length == 0)
            {
                return string.Empty;
            }

            string configPath = Path.Combine(runtimeDirectory, "SpeechConfig.json");
            if (File.Exists(configPath))
            {
                return configPath;
            }

            string sourcePath = GetSpeechSourceConfigPath();
            if (sourcePath.Length != 0)
            {
                try
                {
                    IDictionary<string, object> source = ReadJsonDictionary(sourcePath);
                    WriteJson(configPath, SanitizeSpeechConfig(source));
                    return configPath;
                }
                catch
                {
                }
            }

            try
            {
                WriteJson(configPath, new Dictionary<string, object>
                {
                    { "DefaultEngine", string.Empty },
                    { "TTSInfos", new Dictionary<string, object>() }
                });
                return configPath;
            }
            catch
            {
                return string.Empty;
            }
        }

        public IDictionary<string, object> LoadSpeechConfig()
        {
            string configPath = GetSpeechConfigPath();
            if (configPath.Length == 0)
            {
                return EmptySpeechConfig();
            }

            IDictionary<string, object> raw;
            try
            {
                raw = ReadJsonDictionary(configPath);
            }
            catch
            {
                raw = EmptySpeechConfig();
            }

            IDictionary<string, object> cleaned = SanitizeSpeechConfig(raw);
            try
            {
                if (!string.Equals(
                    JsonConvert.SerializeObject(raw),
                    JsonConvert.SerializeObject(cleaned),
                    StringComparison.Ordinal))
                {
                    WriteJson(configPath, cleaned);
                }
            }
            catch
            {
            }
            return cleaned;
        }

        public bool SaveSpeechConfig(IDictionary<string, object> config)
        {
            string configPath = GetSpeechConfigPath();
            if (configPath.Length == 0)
            {
                Callbacks.SetStatus?.Invoke("朗读设置保存失败：无法创建配置目录");
                return false;
            }

            try
            {
                WriteJson(configPath, SanitizeSpeechConfig(config));
                Callbacks.SetStatus?.Invoke("朗读设置已保存");
                return true;
            }
            catch (Exception error)
            {
                Callbacks.SetStatus?.Invoke("朗读设置保存失败: " + error.Message);
                return false;
            }
        }

        public bool OnSpeechSettings()
        {
            string libraryPath = GetAssistSpeechLibraryPath();
            if (libraryPath.Length != 0)
            {
                Callbacks.SetStatus?.Invoke("已检测到读屏辅助接口");
            }
            else
            {
                Callbacks.SetStatus?.Invoke("未检测到读屏辅助接口，请检查 internal 目录");
                Callbacks.ShowWarning?.Invoke(
                    "未检测到读屏辅助接口。\n请确认 internal 目录中的读屏辅助文件完整。",
                    "朗读设置提示");
            }

            var request = new SpeechSettingsRequest
            {
                SpeechConfig = new Dictionary<string, object>(),
                ReaderIniSettings = LoadReaderIniSettings(),
                InterfaceDllChoices = ScanReaderInterfaceDlls(),
                SelectedInterfaceDll = NormalizeSpeechInterfaceValue(SpeechInterfaceDll),
                AssistHints = new Dictionary<string, object>(SpeechAssistHints ?? DefaultAssistHints()),
                SaveReaderIniSettings = SaveReaderIniSettings,
                SaveInterfaceDll = SaveSpeechInterfaceDll,
                SaveAssistHints = SaveSpeechAssistHints,
                SaveSpeechConfig = config => true
            };
            bool accepted = Callbacks.ShowSettings != null && Callbacks.ShowSettings(request);
            if (accepted)
            {
                Callbacks.CreateMenuBar?.Invoke();
            }
            return accepted;
        }

        public void Dispose()
        {
            ShutdownAssistSpeechRuntime();
        }

        public static IDictionary<string, object> SanitizeSpeechConfig(IDictionary<string, object> config)
        {
            IDictionary<string, object> result = CopyDictionary(config) ?? new Dictionary<string, object>();
            IDictionary<string, object> sourceInfos = CopyDictionary(GetValue(result, "TTSInfos", null));
            var cleanedInfos = new Dictionary<string, object>(StringComparer.Ordinal);
            string firstEngine = string.Empty;
            if (sourceInfos != null)
            {
                foreach (KeyValuePair<string, object> pair in sourceInfos)
                {
                    string engineName = Text(pair.Key).Trim();
                    if (engineName.Length == 0 || BlockedEngines.Contains(engineName))
                    {
                        continue;
                    }

                    IDictionary<string, object> item = CopyDictionary(pair.Value)
                        ?? new Dictionary<string, object>();
                    var cleaned = new Dictionary<string, object>
                    {
                        { "Speed", CommonUtilities.ParseIntSetting(Unwrap(GetValue(item, "Speed", 100)), 100, 0, 200) },
                        { "Volume", CommonUtilities.ParseIntSetting(Unwrap(GetValue(item, "Volume", 100)), 100, 0, 100) },
                        { "Pitch", CommonUtilities.ParseIntSetting(Unwrap(GetValue(item, "Pitch", 50)), 50, 0, 100) },
                        { "Name", Text(GetValue(item, "Name", string.Empty)).Trim() }
                    };
                    cleanedInfos[engineName] = cleaned;
                    if (firstEngine.Length == 0)
                    {
                        firstEngine = engineName;
                    }
                }
            }

            result["TTSInfos"] = cleanedInfos;
            string defaultEngine = Text(GetValue(result, "DefaultEngine", string.Empty)).Trim();
            if (BlockedEngines.Contains(defaultEngine) || !cleanedInfos.ContainsKey(defaultEngine))
            {
                defaultEngine = string.Empty;
            }
            if (defaultEngine.Length == 0)
            {
                defaultEngine = firstEngine;
            }
            result["DefaultEngine"] = defaultEngine;
            return result;
        }

        public static IDictionary<string, object> NormalizeReaderIniSettings(
            IDictionary<string, object> settings)
        {
            IDictionary<string, object> source = settings ?? new Dictionary<string, object>();
            IDictionary<string, object> zdsrSource = CopyDictionary(GetValue(source, "zdsr", null))
                ?? new Dictionary<string, object>();
            IDictionary<string, object> boySource = CopyDictionary(GetValue(source, "boy", null))
                ?? new Dictionary<string, object>();
            return new Dictionary<string, object>
            {
                {
                    "zdsr",
                    new Dictionary<string, object>
                    {
                        { "type", Text(GetValue(zdsrSource, "type", string.Empty)).Trim() },
                        { "channelName", Text(GetValue(zdsrSource, "channelName", string.Empty)).Trim() },
                        { "keyDownInterrupt", Text(GetValue(zdsrSource, "keyDownInterrupt", string.Empty)).Trim() },
                        { "alwaysInterrupt", Text(GetValue(zdsrSource, "alwaysInterrupt", string.Empty)).Trim() }
                    }
                },
                {
                    "boy",
                    new Dictionary<string, object>
                    {
                        { "Param1", PythonTrue(GetValue(boySource, "Param1", "false"), "false") },
                        { "Param2", PythonTrue(GetValue(boySource, "Param2", "false"), "false") },
                        { "Param3", PythonTrue(GetValue(boySource, "Param3", "true"), "true") },
                        { "Param4", PythonTrue(GetValue(boySource, "Param4", "true"), "true") }
                    }
                }
            };
        }

        private bool IsWindowClosing()
        {
            return Callbacks.IsWindowClosing != null && Callbacks.IsWindowClosing();
        }

        private IDictionary<string, object> DefaultReaderIniSettings()
        {
            return new Dictionary<string, object>
            {
                {
                    "zdsr",
                    new Dictionary<string, object>
                    {
                        { "type", string.Empty },
                        { "channelName", string.Empty },
                        { "keyDownInterrupt", string.Empty },
                        { "alwaysInterrupt", string.Empty }
                    }
                },
                {
                    "boy",
                    new Dictionary<string, object>
                    {
                        { "Param1", "false" },
                        { "Param2", "false" },
                        { "Param3", "true" },
                        { "Param4", "true" }
                    }
                }
            };
        }

        private static IDictionary<string, object> DefaultAssistHints()
        {
            return new Dictionary<string, object>
            {
                { "enabled", true },
                { "mute_current", "静音当前打开的进程" },
                { "mute_all", "静音全部打开的进程" }
            };
        }

        private static IDictionary<string, object> EmptySpeechConfig()
        {
            return new Dictionary<string, object>
            {
                { "DefaultEngine", string.Empty },
                { "TTSInfos", new Dictionary<string, object>() }
            };
        }

        private static IDictionary<string, string> ReadIniSection(string path, string sectionName)
        {
            if (!File.Exists(Text(path)))
            {
                return null;
            }

            try
            {
                var result = new Dictionary<string, string>(StringComparer.Ordinal);
                bool inSection = false;
                foreach (string rawLine in File.ReadAllLines(Text(path), Encoding.UTF8))
                {
                    string line = rawLine.Trim();
                    if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                    {
                        inSection = string.Equals(
                            line.Substring(1, line.Length - 2).Trim(),
                            sectionName,
                            StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (!inSection || line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal)
                        || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex <= 0)
                    {
                        continue;
                    }
                    string key = line.Substring(0, equalsIndex).Trim();
                    string value = line.Substring(equalsIndex + 1).Trim();
                    if (key.Length != 0)
                    {
                        result[key] = value;
                    }
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static void WriteIniSection(
            string path,
            string sectionName,
            IEnumerable<string> keys,
            IDictionary<string, object> values)
        {
            string parent = Path.GetDirectoryName(Text(path));
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }

            var builder = new StringBuilder();
            builder.Append('[').Append(sectionName).Append(']').Append(Environment.NewLine);
            foreach (string key in keys)
            {
                builder.Append(key)
                    .Append(" = ")
                    .Append(Text(GetValue(values, key, string.Empty)))
                    .Append(Environment.NewLine);
            }
            File.WriteAllText(Text(path), builder.ToString(), new UTF8Encoding(false));
        }

        private static IDictionary<string, object> ReadJsonDictionary(string path)
        {
            string text = File.ReadAllText(Text(path), Encoding.UTF8);
            IDictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);
            return dictionary ?? new Dictionary<string, object>();
        }

        private static void WriteJson(string path, IDictionary<string, object> value)
        {
            string parent = Path.GetDirectoryName(Text(path));
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }
            File.WriteAllText(
                Text(path),
                JsonConvert.SerializeObject(value ?? new Dictionary<string, object>(), Formatting.Indented),
                new UTF8Encoding(false));
        }

        private static string FirstExistingFile(IEnumerable<string> candidates)
        {
            foreach (string candidate in candidates ?? Enumerable.Empty<string>())
            {
                try
                {
                    if (File.Exists(Text(candidate)))
                    {
                        return Path.GetFullPath(Text(candidate));
                    }
                }
                catch
                {
                }
            }
            return string.Empty;
        }

        private static string PythonTrue(object value, string defaultValue)
        {
            return string.Equals(Text(value).Trim(), "true", StringComparison.OrdinalIgnoreCase)
                ? "true"
                : "false";
        }

        private static IDictionary<string, object> CopyDictionary(object value)
        {
            if (value == null)
            {
                return null;
            }
            IDictionary<string, object> generic = value as IDictionary<string, object>;
            if (generic != null)
            {
                return new Dictionary<string, object>(generic, StringComparer.Ordinal);
            }
            JObject json = value as JObject;
            if (json != null)
            {
                return json.ToObject<Dictionary<string, object>>();
            }
            IDictionary dictionary = value as IDictionary;
            if (dictionary == null)
            {
                return null;
            }
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in dictionary)
            {
                string key = Text(entry.Key);
                if (key.Length != 0)
                {
                    result[key] = entry.Value;
                }
            }
            return result;
        }

        private static IDictionary<string, object> AsDictionary(object value)
        {
            return CopyDictionary(value) ?? new Dictionary<string, object>();
        }

        private static object GetValue(IDictionary<string, object> values, string key, object defaultValue)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value : defaultValue;
        }

        private static object Unwrap(object value)
        {
            JValue json = value as JValue;
            return json == null ? value : json.Value;
        }

        private static string Text(object value)
        {
            object raw = Unwrap(value);
            return raw == null ? string.Empty : Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string GetFullPathSafely(string path)
        {
            string value = Text(path).Trim();
            if (value.Length == 0)
            {
                return string.Empty;
            }
            try
            {
                return Path.GetFullPath(value);
            }
            catch
            {
                return value;
            }
        }

        private static void PrependSpeechEngineDirectoryToPath(string directory)
        {
            string value = Text(directory).Trim();
            if (value.Length == 0 || !Directory.Exists(value))
            {
                return;
            }

            try
            {
                value = Path.GetFullPath(value);
                string current = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                bool present = current
                    .Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                    .Any(item => string.Equals(
                        GetFullPathSafely(item.Trim()), value,
                        StringComparison.OrdinalIgnoreCase));
                if (!present)
                {
                    Environment.SetEnvironmentVariable("PATH", value + Path.PathSeparator + current);
                }
            }
            catch
            {
            }
        }

        private static SpeechAssistBackend CreateReaderBackend(string dllPath, string reader)
        {
            IntPtr module = NativeMethods.LoadLibrary(dllPath);
            if (module == IntPtr.Zero) throw new InvalidOperationException("无法加载读屏辅助接口");
            try
            {
                var backend = new SpeechAssistBackend();
                Action uninitialize = null;
                bool initialized = false;
                switch (reader)
                {
                    case "nvda":
                        ReaderStatusDelegate running = LoadDelegate<ReaderStatusDelegate>(module, "nvdaController_testIfRunning");
                        ReaderTextDelegate speakNvda = LoadDelegate<ReaderTextDelegate>(module, "nvdaController_speakText");
                        ReaderStatusDelegate stopNvda = LoadDelegate<ReaderStatusDelegate>(module, "nvdaController_cancelSpeech");
                        backend.Load = () => { };
                        backend.IsLoaded = () => running() == 0;
                        backend.DetectScreenReader = () => running() == 0 ? "NVDA" : string.Empty;
                        backend.Silence = () => CheckReaderResult(stopNvda(), "nvdaController_cancelSpeech");
                        backend.Output = (text, interrupt) => CheckReaderResult(speakNvda(text), "nvdaController_speakText");
                        break;
                    case "zdsr":
                        ZdsrInitializeDelegate initializeZdsr = LoadDelegate<ZdsrInitializeDelegate>(module, "InitTTS");
                        ReaderStatusDelegate stateZdsr = LoadDelegate<ReaderStatusDelegate>(module, "GetSpeakState");
                        ZdsrSpeakDelegate speakZdsr = LoadDelegate<ZdsrSpeakDelegate>(module, "Speak");
                        ReaderVoidDelegate stopZdsr = LoadDelegate<ReaderVoidDelegate>(module, "StopSpeak");
                        backend.Load = () => CheckReaderResult(initializeZdsr(0, null, true), "InitTTS");
                        backend.IsLoaded = () => { int state = stateZdsr(); return state == 3 || state == 4; };
                        backend.DetectScreenReader = () => backend.IsLoaded() ? "ZDSR" : string.Empty;
                        backend.Silence = () => stopZdsr();
                        backend.Output = (text, interrupt) => CheckReaderResult(speakZdsr(text, interrupt), "Speak");
                        break;
                    case "boy":
                        ReaderTextDelegate initializeBoy = LoadDelegate<ReaderTextDelegate>(module, "BoyCtrlInitialize");
                        ReaderVoidDelegate uninitializeBoy = LoadDelegate<ReaderVoidDelegate>(module, "BoyCtrlUninitialize");
                        ReaderBoolDelegate runningBoy = LoadDelegate<ReaderBoolDelegate>(module, "BoyCtrlIsReaderRunning");
                        BoySpeakDelegate speakBoy = LoadDelegate<BoySpeakDelegate>(module, "BoyCtrlSpeak");
                        ReaderStatusDelegate stopBoy = LoadDelegate<ReaderStatusDelegate>(module, "BoyCtrlStopSpeaking");
                        backend.Load = () => { CheckReaderResult(initializeBoy(null), "BoyCtrlInitialize"); initialized = true; };
                        backend.IsLoaded = () => runningBoy();
                        backend.DetectScreenReader = () => runningBoy() ? "BoyPCReader" : string.Empty;
                        backend.Silence = () => CheckReaderResult(stopBoy(), "BoyCtrlStopSpeaking");
                        backend.Output = (text, interrupt) => CheckReaderResult(speakBoy(text, !interrupt, BoySpeechCompleted), "BoyCtrlSpeak");
                        uninitialize = () => { if (initialized) uninitializeBoy(); };
                        break;
                    default:
                        throw new InvalidOperationException("Unknown screen reader: " + reader);
                }
                int released = 0;
                backend.Unload = () =>
                {
                    if (Interlocked.Exchange(ref released, 1) != 0) return;
                    try { uninitialize?.Invoke(); }
                    finally { NativeMethods.FreeLibrary(module); }
                };
                return backend;
            }
            catch
            {
                NativeMethods.FreeLibrary(module);
                throw;
            }
        }

        private static void CheckReaderResult(int result, string operation)
        {
            if (result != 0) throw new InvalidOperationException(operation + " returned " + result);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReaderStatusDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void ReaderVoidDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int ReaderTextDelegate([MarshalAs(UnmanagedType.LPWStr)] string text);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool ReaderBoolDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int ZdsrInitializeDelegate(int type, [MarshalAs(UnmanagedType.LPWStr)] string channel,
            [MarshalAs(UnmanagedType.Bool)] bool keyDownInterrupt);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int ZdsrSpeakDelegate([MarshalAs(UnmanagedType.LPWStr)] string text,
            [MarshalAs(UnmanagedType.Bool)] bool interrupt);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void BoySpeakCompletedDelegate(int reason);

        private static readonly BoySpeakCompletedDelegate BoySpeechCompleted = reason => { };

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int BoySpeakDelegate([MarshalAs(UnmanagedType.LPWStr)] string text,
            [MarshalAs(UnmanagedType.I1)] bool append, BoySpeakCompletedDelegate completion);

        private static SpeechAssistBackend CreateNativeBackend(string dllPath)
        {
            IntPtr module = NativeMethods.LoadLibrary(dllPath);
            if (module == IntPtr.Zero)
            {
                throw new InvalidOperationException("无法加载读屏辅助接口");
            }

            try
            {
                TolkTrySapiDelegate trySapi = LoadDelegate<TolkTrySapiDelegate>(module, "Tolk_TrySAPI");
                TolkPreferSapiDelegate preferSapi = LoadDelegate<TolkPreferSapiDelegate>(module, "Tolk_PreferSAPI");
                TolkDetectScreenReaderDelegate detectScreenReader = LoadDelegate<TolkDetectScreenReaderDelegate>(module, "Tolk_DetectScreenReader");
                TolkLoadDelegate load = LoadDelegate<TolkLoadDelegate>(module, "Tolk_Load");
                TolkIsLoadedDelegate isLoaded = LoadDelegate<TolkIsLoadedDelegate>(module, "Tolk_IsLoaded");
                TolkSilenceDelegate silence = LoadDelegate<TolkSilenceDelegate>(module, "Tolk_Silence");
                TolkOutputDelegate output = LoadDelegate<TolkOutputDelegate>(module, "Tolk_Output");
                TolkUnloadDelegate unload = LoadDelegate<TolkUnloadDelegate>(module, "Tolk_Unload");
                int released = 0;
                return new SpeechAssistBackend
                {
                    TrySapi = value => trySapi(value),
                    PreferSapi = value => preferSapi(value),
                    DetectScreenReader = () =>
                    {
                        IntPtr value = detectScreenReader();
                        return value == IntPtr.Zero
                            ? string.Empty
                            : Marshal.PtrToStringUni(value) ?? string.Empty;
                    },
                    Load = () => load(),
                    IsLoaded = () => isLoaded(),
                    Silence = () => silence(),
                    Output = (text, interrupt) => output(text, interrupt),
                    Unload = () =>
                    {
                        if (Interlocked.Exchange(ref released, 1) != 0)
                        {
                            return;
                        }
                        try
                        {
                            unload();
                        }
                        finally
                        {
                            NativeMethods.FreeLibrary(module);
                        }
                    }
                };
            }
            catch
            {
                NativeMethods.FreeLibrary(module);
                throw;
            }
        }

        private static TDelegate LoadDelegate<TDelegate>(IntPtr module, string name)
        {
            IntPtr address = NativeMethods.GetProcAddress(module, name);
            if (address == IntPtr.Zero)
            {
                throw new MissingMethodException(name);
            }
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TolkTrySapiDelegate([MarshalAs(UnmanagedType.I1)] bool useSapi);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TolkPreferSapiDelegate([MarshalAs(UnmanagedType.I1)] bool preferSapi);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate IntPtr TolkDetectScreenReaderDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TolkLoadDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool TolkIsLoadedDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool TolkSilenceDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool TolkOutputDelegate([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TolkUnloadDelegate();

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeLibrary(IntPtr module);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr module, string name);
        }
    }
}
