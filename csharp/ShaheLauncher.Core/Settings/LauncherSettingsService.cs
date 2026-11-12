using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaheLauncher.Core.AppPaths;
using ShaheLauncher.Core.Common;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.Core.Settings
{
    public sealed class SettingsSaveException : IOException
    {
        public SettingsSaveException(Exception inner = null)
            : base("设置保存失败，请检查配置文件是否可写。", inner) { }
    }

    public sealed class LauncherSettingsState
    {
        public LauncherSettingsState()
        {
            Settings = new Dictionary<string, object>();
            AppHistory = new List<object>();
            MaxHistoryItems = 10;
            SandboxRoot = LauncherConstants.GetDefaultSandboxRoot();
            LaunchInterval = 5;
            LaunchConcurrencyLimit = LauncherConstants.GetDefaultLaunchConcurrencyLimit();
            ShowTray = true;
            ListUseLeftRightNavigation = true;
            RememberVolume = true;
            LogOutputMode = LauncherConstants.LogOutputOff;
            Hotkeys = new Dictionary<string, string>();
            SandboxGroups = new Dictionary<string, object>();
            PinnedSandboxes = new List<string>();
            SandboxVolumeMemory = new Dictionary<string, object>();
            SpeechAssistHints = new Dictionary<string, object>
            {
                { "enabled", true },
                { "mute_current", "静音当前打开的进程" },
                { "mute_all", "静音全部打开的进程" }
            };
        }

        public IDictionary<string, object> Settings { get; set; }
        public IList<object> AppHistory { get; set; }
        public int MaxHistoryItems { get; set; }
        public bool HidePath { get; set; }
        public string AppPath { get; set; }
        public string AppDirectory { get; set; }
        public string SandboxiePath { get; set; }
        public string SandboxRoot { get; set; }
        public int LaunchInterval { get; set; }
        public int LaunchConcurrencyLimit { get; set; }
        public bool ShowTray { get; set; }
        public bool AutoRun { get; set; }
        public bool AutoSelectRangeOnCheck { get; set; }
        public bool ListUseLeftRightNavigation { get; set; }
        public bool MinimizeToTrayAfterLaunch { get; set; }
        public bool SkipRunningSandboxOnLaunch { get; set; }
        public bool RememberVolume { get; set; }
        public string LogOutputMode { get; set; }
        public bool HotkeysEnabled { get; set; }
        public IDictionary<string, string> Hotkeys { get; set; }
        public IDictionary<string, object> SandboxGroups { get; set; }
        public IList<string> PinnedSandboxes { get; set; }
        public IDictionary<string, object> SandboxVolumeMemory { get; set; }
        public string SpeechInterfaceDll { get; set; }
        public IDictionary<string, object> SpeechAssistHints { get; set; }
    }

    public sealed class LauncherSettingsCallbacks
    {
        public Action ClearAppPath { get; set; }
        public Action CreateTrayIcon { get; set; }
        public Action DisableTrayDisplay { get; set; }
        public Action<string> SetSandboxRoot { get; set; }
        public Action<bool> SetRangeAutoCheckEnabled { get; set; }
        public Action<bool> SetLeftRightNavigationEnabled { get; set; }
        public Action RefreshSandboxList { get; set; }
        public Action UpdateAppPathList { get; set; }
        public Action ApplyHotkeys { get; set; }
        public Action CreateMenuBar { get; set; }
        public Action ApplyAutoRun { get; set; }
        public Action<int> ScheduleProcessMenuRefresh { get; set; }
        public Action<string> SetStatus { get; set; }
    }

    public sealed class LauncherSettingsService
    {
        public const string ResetConfirmationTitle = "重置设置";
        public const string ResetConfirmationText = "确定要将全部设置重置为默认值吗？\n该操作会覆盖当前设置。";
        public const string ResetCompletedStatus = "已重置所有设置为默认值";
        public const string SettingsSavedStatus = "设置已保存";
        public const string ManualSandboxiePathDialogTitle = "手动校正 Sandboxie 安装路径";
        public const string DefaultSandboxiePath = @"C:\Program Files\Sandboxie";
        public const string SandboxiePathSavedMessage = "安装路径已保存。";
        public const string SandboxiePathSavedTitle = "校正完成";
        public const string SandboxiePathInvalidStatus = "该目录下未找到 Start.exe，未修改安装路径";
        public const string SandboxiePathInvalidMessage = "该目录不是有效的 Sandboxie 安装目录。";
        public const string SandboxiePathInvalidTitle = "校正失败";
        public const string SandboxRootDialogTitle = "选择沙盒用户数据目录";
        public const string SandboxRootInvalidStatus = "沙盒数据目录无效，未修改";

        private readonly ConfigStore _configStore;
        private readonly string _legacyConfigFile;
        private readonly string _configurationDirectory;

        public LauncherSettingsService(
            ConfigStore configStore,
            string legacyConfigFile,
            string configurationDirectory)
        {
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _legacyConfigFile = legacyConfigFile ?? string.Empty;
            _configurationDirectory = configurationDirectory ?? string.Empty;
            UpdateManifestUrl = string.Empty;
        }

        public string UpdateManifestUrl { get; set; }
        public Func<object, int, IList<object>> NormalizeAppHistory { get; set; }
        public Func<object, IDictionary<string, string>> NormalizeHotkeys { get; set; }
        public Func<object, IDictionary<string, object>> NormalizeSandboxGroups { get; set; }
        public Func<object, IList<string>> NormalizeSandboxNames { get; set; }
        public Func<object, IDictionary<string, object>> NormalizeVolumeMemory { get; set; }
        public Func<object, string> NormalizeSpeechInterface { get; set; }

        public IDictionary<string, object> CreateDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                { "max_history_items", 10 },
                { "hide_path", false },
                { "app_history", new List<object>() },
                { "sandboxie_path", string.Empty },
                { "sandbox_root", LauncherConstants.GetDefaultSandboxRoot() },
                { "launch_interval", 5 },
                { "launch_concurrency_limit", LauncherConstants.GetDefaultLaunchConcurrencyLimit() },
                { "show_tray", true },
                { "auto_run", false },
                { "auto_select_range_on_check", false },
                { "list_use_left_right_nav", true },
                { "auto_minimize_to_tray_after_launch", false },
                { "skip_running_sandbox_on_launch", false },
                { "remember_volume", true },
                { "log_output_mode", LauncherConstants.LogOutputOff },
                { "hotkeys_enabled", false },
                { "hotkeys", new Dictionary<string, string>() },
                { "sandbox_groups", new Dictionary<string, object>() },
                { "pinned_sandboxes", new List<string>() },
                { "sandbox_volume_memory", new Dictionary<string, object>() },
                { "speech_interface_dll", string.Empty },
                {
                    "speech_assist_hints",
                    new Dictionary<string, object>
                    {
                        { "enabled", true },
                        { "mute_current", "静音当前打开的进程" },
                        { "mute_all", "静音全部打开的进程" }
                    }
                }
            };
        }

        public string GetUpdateManifestUrl()
        {
            string value = StringValue(UpdateManifestUrl).Trim();
            return value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(value, "https://lqh.viyf.net/shahe-bulk-launcher/update.json", StringComparison.OrdinalIgnoreCase)
                ? value
                : LauncherConstants.UpdateManifestUrl;
        }

        public string SetUpdateManifestUrl(string value)
        {
            UpdateManifestUrl = StringValue(value).Trim();
            string normalized = GetUpdateManifestUrl();
            UpdateManifestUrl = normalized;
            try
            {
                _configStore.SetSecretText("update_manifest_url", normalized);
            }
            catch
            {
            }
            return normalized;
        }

        public IDictionary<string, object> LoadSettings()
        {
            IDictionary<string, object> defaults = CreateDefaultSettings();
            try
            {
                IDictionary<string, object> stored = CopyDictionary(
                    _configStore.GetJson("settings_json", null));
                if (stored != null)
                {
                    IDictionary<string, object> merged = MergeSettings(defaults, stored);
                    UpdateManifestUrl = _configStore.GetSecretText(
                        "update_manifest_url", LauncherConstants.UpdateManifestUrl)
                        ?? LauncherConstants.UpdateManifestUrl;
                    if (UpdateManifestUrl.Length == 0)
                    {
                        UpdateManifestUrl = LauncherConstants.UpdateManifestUrl;
                    }
                    return merged;
                }

                if (_legacyConfigFile.Length != 0 && File.Exists(_legacyConfigFile))
                {
                    IDictionary<string, object> legacy = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        File.ReadAllText(_legacyConfigFile));
                    if (legacy != null)
                    {
                        IDictionary<string, object> merged = MergeSettings(defaults, legacy);
                        try
                        {
                            _configStore.SetJson("settings_json", merged);
                        }
                        catch
                        {
                        }
                        try
                        {
                            if (_configurationDirectory.Length != 0)
                            {
                                Directory.CreateDirectory(_configurationDirectory);
                            }
                        }
                        catch
                        {
                        }
                        UpdateManifestUrl = _configStore.GetSecretText(
                            "update_manifest_url", LauncherConstants.UpdateManifestUrl)
                            ?? LauncherConstants.UpdateManifestUrl;
                        if (UpdateManifestUrl.Length == 0)
                        {
                            UpdateManifestUrl = LauncherConstants.UpdateManifestUrl;
                        }
                        SetUpdateManifestUrl(UpdateManifestUrl);
                        return merged;
                    }
                }
            }
            catch
            {
            }

            UpdateManifestUrl = _configStore.GetSecretText(
                "update_manifest_url", LauncherConstants.UpdateManifestUrl)
                ?? LauncherConstants.UpdateManifestUrl;
            if (UpdateManifestUrl.Length == 0)
            {
                UpdateManifestUrl = LauncherConstants.UpdateManifestUrl;
            }
            SetUpdateManifestUrl(UpdateManifestUrl);
            defaults["log_output_mode"] = LauncherConstants.NormalizeLogOutputMode(
                StringValue(defaults["log_output_mode"]));
            return defaults;
        }

        public void SaveSettings(LauncherSettingsState state)
        {
            if (state == null)
            {
                return;
            }

            try
            {
                IDictionary<string, object> settings = state.Settings
                    ?? new Dictionary<string, object>();
                state.Settings = settings;
                IList<object> history = NormalizeAppHistory == null
                    ? DefaultNormalizeAppHistory(state.AppHistory, state.MaxHistoryItems)
                    : NormalizeAppHistory(state.AppHistory, state.MaxHistoryItems);
                settings["app_history"] = history ?? new List<object>();
                settings["max_history_items"] = state.MaxHistoryItems;
                settings["hide_path"] = state.HidePath;
                settings["sandboxie_path"] = state.SandboxiePath ?? string.Empty;
                settings["sandbox_root"] = state.SandboxRoot ?? string.Empty;
                settings["launch_interval"] = state.LaunchInterval;
                settings["launch_concurrency_limit"] = state.LaunchConcurrencyLimit;
                settings.Remove("dynamic_launch_delay_by_load");
                settings["show_tray"] = state.ShowTray;
                settings["auto_run"] = state.AutoRun;
                settings.Remove("auto_update_on_startup");
                settings.Remove("auto_check_update_on_startup");
                settings["auto_select_range_on_check"] = state.AutoSelectRangeOnCheck;
                settings["list_use_left_right_nav"] = state.ListUseLeftRightNavigation;
                settings["auto_minimize_to_tray_after_launch"] = false;
                settings["skip_running_sandbox_on_launch"] = state.SkipRunningSandboxOnLaunch;
                settings["remember_volume"] = state.RememberVolume;
                state.LogOutputMode = LauncherConstants.NormalizeLogOutputMode(state.LogOutputMode);
                settings["log_output_mode"] = state.LogOutputMode;
                settings["hotkeys_enabled"] = state.HotkeysEnabled;
                settings["hotkeys"] = state.Hotkeys ?? new Dictionary<string, string>();
                settings["sandbox_groups"] = state.SandboxGroups ?? new Dictionary<string, object>();
                settings["pinned_sandboxes"] = state.PinnedSandboxes ?? new List<string>();
                settings["sandbox_volume_memory"] = state.SandboxVolumeMemory
                    ?? new Dictionary<string, object>();
                settings["speech_interface_dll"] = state.SpeechInterfaceDll ?? string.Empty;
                settings["speech_assist_hints"] = state.SpeechAssistHints
                    ?? new Dictionary<string, object>();
                settings.Remove("auto_update_on_startup");
                settings.Remove("auto_check_update_on_startup");
                settings.Remove("update_manifest_url");
                if (!_configStore.SetJson("settings_json", settings)) throw new SettingsSaveException();
                SetUpdateManifestUrl(GetUpdateManifestUrl());
            }
            catch (SettingsSaveException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new SettingsSaveException(error);
            }
        }

        public void ApplySettings(
            LauncherSettingsState state,
            IDictionary<string, object> settings,
            LauncherSettingsCallbacks callbacks)
        {
            if (state == null)
            {
                return;
            }

            IDictionary<string, object> data = settings == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(settings);
            LauncherSettingsCallbacks effects = callbacks ?? new LauncherSettingsCallbacks();
            state.Settings = data;
            state.MaxHistoryItems = CommonUtilities.ParseIntSetting(
                Unwrap(GetValue(data, "max_history_items", state.MaxHistoryItems)),
                state.MaxHistoryItems,
                0,
                AppPathHistoryService.MaximumHistoryItems);
            state.HidePath = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "hide_path", state.HidePath)), state.HidePath);
            state.LogOutputMode = LauncherConstants.NormalizeLogOutputMode(
                StringValue(GetValue(data, "log_output_mode", state.LogOutputMode)));
            data["log_output_mode"] = state.LogOutputMode;
            IList<object> incomingHistory = CopyObjectList(
                GetValue(data, "app_history", state.AppHistory));
            if (incomingHistory != null)
            {
                state.AppHistory = incomingHistory;
            }

            state.LaunchInterval = CommonUtilities.ParseIntSetting(
                Unwrap(GetValue(data, "launch_interval", 5)), 5, 0, 15);
            state.LaunchConcurrencyLimit = CommonUtilities.ParseIntSetting(
                Unwrap(GetValue(data, "launch_concurrency_limit", LauncherConstants.GetDefaultLaunchConcurrencyLimit())),
                state.LaunchConcurrencyLimit,
                1,
                LauncherConstants.GetLaunchConcurrencyMaximum());
            data.Remove("dynamic_launch_delay_by_load");
            state.AutoRun = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "auto_run", false)), false);
            data.Remove("auto_update_on_startup");
            data.Remove("auto_check_update_on_startup");
            state.AutoSelectRangeOnCheck = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "auto_select_range_on_check", false)), false);
            state.ListUseLeftRightNavigation = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "list_use_left_right_nav", state.ListUseLeftRightNavigation)),
                state.ListUseLeftRightNavigation);
            state.MinimizeToTrayAfterLaunch = false;
            state.SkipRunningSandboxOnLaunch = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "skip_running_sandbox_on_launch", false)), false);
            state.RememberVolume = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "remember_volume", state.RememberVolume)), state.RememberVolume);
            state.HotkeysEnabled = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "hotkeys_enabled", state.HotkeysEnabled)), state.HotkeysEnabled);
            object hotkeysValue = GetValue(data, "hotkeys", state.Hotkeys);
            state.Hotkeys = NormalizeHotkeys == null
                ? DefaultNormalizeHotkeys(hotkeysValue)
                : NormalizeHotkeys(hotkeysValue);

            string sandboxRoot = LauncherConstants.NormalizeSandboxRootPath(
                StringValue(GetValue(data, "sandbox_root", state.SandboxRoot)));
            if (sandboxRoot.Length == 0)
            {
                sandboxRoot = LauncherConstants.GetDefaultSandboxRoot();
            }
            state.SandboxRoot = sandboxRoot;
            data["sandbox_root"] = sandboxRoot;
            object groupsValue = GetValue(data, "sandbox_groups", new Dictionary<string, object>());
            state.SandboxGroups = NormalizeSandboxGroups == null
                ? DefaultNormalizeDictionary(groupsValue)
                : NormalizeSandboxGroups(groupsValue);
            object pinnedValue = GetValue(data, "pinned_sandboxes", state.PinnedSandboxes);
            state.PinnedSandboxes = NormalizeSandboxNames == null
                ? DefaultNormalizeNames(pinnedValue)
                : NormalizeSandboxNames(pinnedValue);
            object volumeValue = GetValue(data, "sandbox_volume_memory", state.SandboxVolumeMemory);
            state.SandboxVolumeMemory = NormalizeVolumeMemory == null
                ? DefaultNormalizeDictionary(volumeValue)
                : NormalizeVolumeMemory(volumeValue);

            object speechValue = GetValue(data, "speech_interface_dll", state.SpeechInterfaceDll);
            string rawSpeech = StringValue(speechValue).Trim();
            state.SpeechInterfaceDll = NormalizeSpeechInterface == null
                ? rawSpeech
                : NormalizeSpeechInterface(speechValue) ?? string.Empty;
            if (rawSpeech.Length != 0 && state.SpeechInterfaceDll.Length == 0)
            {
                data["speech_interface_dll"] = string.Empty;
            }

            IDictionary<string, object> hints = CopyDictionary(
                GetValue(data, "speech_assist_hints", state.SpeechAssistHints))
                ?? new Dictionary<string, object>();
            hints["enabled"] = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(hints, "enabled", true)), true);
            if (StringValue(GetValue(hints, "mute_current", string.Empty)).Trim().Length == 0)
            {
                hints["mute_current"] = "静音当前打开的进程";
            }
            if (StringValue(GetValue(hints, "mute_all", string.Empty)).Trim().Length == 0)
            {
                hints["mute_all"] = "静音全部打开的进程";
            }
            state.SpeechAssistHints = hints;

            bool newShowTray = CommonUtilities.ParseBoolSetting(
                Unwrap(GetValue(data, "show_tray", state.ShowTray)), state.ShowTray);
            if (newShowTray != state.ShowTray)
            {
                state.ShowTray = newShowTray;
                if (state.ShowTray)
                {
                    effects.CreateTrayIcon?.Invoke();
                }
                else
                {
                    effects.DisableTrayDisplay?.Invoke();
                }
            }

            effects.SetSandboxRoot?.Invoke(state.SandboxRoot);
            effects.SetRangeAutoCheckEnabled?.Invoke(state.AutoSelectRangeOnCheck);
            effects.SetLeftRightNavigationEnabled?.Invoke(state.ListUseLeftRightNavigation);
            effects.RefreshSandboxList?.Invoke();
            try
            {
                effects.UpdateAppPathList?.Invoke();
            }
            catch
            {
            }
            effects.ApplyHotkeys?.Invoke();
            effects.CreateMenuBar?.Invoke();
            SaveSettings(state);
            effects.ApplyAutoRun?.Invoke();
            effects.ScheduleProcessMenuRefresh?.Invoke(1);
            effects.SetStatus?.Invoke(SettingsSavedStatus);
        }

        public void ResetSettings(
            LauncherSettingsState state,
            LauncherSettingsCallbacks callbacks)
        {
            if (state == null)
            {
                return;
            }

            LauncherSettingsCallbacks effects = callbacks ?? new LauncherSettingsCallbacks();
            IDictionary<string, object> defaults = CreateDefaultSettings();
            state.AppHistory = CopyObjectList(defaults["app_history"]) ?? new List<object>();
            state.MaxHistoryItems = CommonUtilities.ParseIntSetting(
                defaults["max_history_items"],
                10,
                0,
                AppPathHistoryService.MaximumHistoryItems);
            state.HidePath = CommonUtilities.ParseBoolSetting(defaults["hide_path"], false);
            state.AppPath = string.Empty;
            state.AppDirectory = string.Empty;
            state.SandboxiePath = StringValue(defaults["sandboxie_path"]).Trim();
            state.SandboxRoot = LauncherConstants.NormalizeSandboxRootPath(
                StringValue(defaults["sandbox_root"]));
            if (state.SandboxRoot.Length == 0)
            {
                state.SandboxRoot = LauncherConstants.GetDefaultSandboxRoot();
            }
            try
            {
                effects.ClearAppPath?.Invoke();
            }
            catch
            {
            }
            ApplySettings(state, defaults, effects);
            effects.SetStatus?.Invoke(ResetCompletedStatus);
        }

        public bool TryApplySandboxRoot(
            LauncherSettingsState state,
            string newRoot,
            bool refresh,
            LauncherSettingsCallbacks callbacks)
        {
            if (state == null)
            {
                return false;
            }

            string value = LauncherConstants.NormalizeSandboxRootPath(newRoot);
            if (value.Length == 0)
            {
                return false;
            }

            state.SandboxRoot = value;
            LauncherSettingsCallbacks effects = callbacks ?? new LauncherSettingsCallbacks();
            effects.SetSandboxRoot?.Invoke(value);
            if (refresh)
            {
                effects.RefreshSandboxList?.Invoke();
            }
            SaveSettings(state);
            return true;
        }

        public bool TryApplySandboxiePath(
            LauncherSettingsState state,
            string newPath)
        {
            if (state == null)
            {
                return false;
            }

            string value = StringValue(newPath).Trim();
            if (value.Length == 0)
            {
                return false;
            }
            try
            {
                value = Path.GetFullPath(value);
            }
            catch
            {
            }
            if (!File.Exists(Path.Combine(value, "Start.exe")))
            {
                return false;
            }

            state.SandboxiePath = value;
            SaveSettings(state);
            return true;
        }

        public static string GetSandboxiePathSuccessStatus(string path)
        {
            return "已校正 Sandboxie 安装路径: " + StringValue(path);
        }

        public static string GetSandboxRootSuccessStatus(string path)
        {
            return "已切换沙盒数据目录: " + StringValue(path);
        }

        private static IDictionary<string, object> MergeSettings(
            IDictionary<string, object> defaults,
            IDictionary<string, object> loaded)
        {
            var merged = new Dictionary<string, object>(defaults);
            foreach (KeyValuePair<string, object> item in loaded)
            {
                merged[item.Key] = RestoreJsonValue(item.Value);
            }

            IDictionary<string, object> hints = CopyDictionary(
                GetValue(defaults, "speech_assist_hints", null))
                ?? new Dictionary<string, object>();
            IDictionary<string, object> loadedHints = CopyDictionary(
                GetValue(merged, "speech_assist_hints", null));
            if (loadedHints != null)
            {
                foreach (KeyValuePair<string, object> item in loadedHints)
                {
                    hints[item.Key] = item.Value;
                }
            }
            merged["speech_assist_hints"] = hints;
            merged.Remove("auto_update_on_startup");
            merged.Remove("auto_check_update_on_startup");
            merged.Remove("update_manifest_url");
            merged["log_output_mode"] = LauncherConstants.NormalizeLogOutputMode(
                StringValue(GetValue(merged, "log_output_mode", LauncherConstants.LogOutputOff)));
            return merged;
        }

        private static object RestoreJsonValue(object value)
        {
            JObject jsonObject = value as JObject;
            if (jsonObject != null)
            {
                return jsonObject.Properties().ToDictionary(
                    property => property.Name,
                    property => RestoreJsonValue(property.Value),
                    StringComparer.Ordinal);
            }

            JArray jsonArray = value as JArray;
            if (jsonArray != null)
            {
                return jsonArray.Select(item => RestoreJsonValue(item)).ToList();
            }

            return Unwrap(value);
        }

        private static object GetValue(
            IDictionary<string, object> values,
            string key,
            object defaultValue)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value : defaultValue;
        }

        private static IDictionary<string, object> CopyDictionary(object value)
        {
            var generic = value as IDictionary<string, object>;
            if (generic != null)
            {
                return new Dictionary<string, object>(generic);
            }

            var json = value as JObject;
            if (json != null)
            {
                return json.ToObject<Dictionary<string, object>>();
            }

            var dictionary = value as IDictionary;
            if (dictionary == null)
            {
                return null;
            }

            var result = new Dictionary<string, object>();
            foreach (DictionaryEntry item in dictionary)
            {
                string key = StringValue(item.Key);
                if (key.Length != 0)
                {
                    result[key] = item.Value;
                }
            }
            return result;
        }

        private static IList<object> CopyObjectList(object value)
        {
            var generic = value as IList<object>;
            if (generic != null)
            {
                return new List<object>(generic);
            }

            var json = value as JArray;
            if (json != null)
            {
                return json.ToObject<List<object>>();
            }

            var list = value as IList;
            if (list == null)
            {
                return null;
            }

            var result = new List<object>();
            foreach (object item in list)
            {
                result.Add(item);
            }
            return result;
        }

        private static IList<object> DefaultNormalizeAppHistory(object value, int limit)
        {
            IList<object> items = CopyObjectList(value) ?? new List<object>();
            return items.Take(Math.Max(0, limit)).ToList();
        }

        private static IDictionary<string, string> DefaultNormalizeHotkeys(object value)
        {
            IDictionary<string, object> source = CopyDictionary(value);
            var result = new Dictionary<string, string>();
            if (source == null)
            {
                return result;
            }
            foreach (KeyValuePair<string, object> item in source)
            {
                result[item.Key] = StringValue(item.Value).Trim();
            }
            return result;
        }

        private static IDictionary<string, object> DefaultNormalizeDictionary(object value)
        {
            return CopyDictionary(value) ?? new Dictionary<string, object>();
        }

        private static IList<string> DefaultNormalizeNames(object value)
        {
            var result = new List<string>();
            var sequence = value as IEnumerable;
            if (sequence == null || value is string)
            {
                return result;
            }
            foreach (object item in sequence)
            {
                string name = StringValue(item).Trim();
                if (name.Length != 0)
                {
                    result.Add(name);
                }
            }
            return result;
        }

        private static object Unwrap(object value)
        {
            var json = value as JValue;
            return json == null ? value : json.Value;
        }

        private static string StringValue(object value)
        {
            object raw = Unwrap(value);
            return raw == null ? string.Empty : Convert.ToString(raw) ?? string.Empty;
        }
    }
}
