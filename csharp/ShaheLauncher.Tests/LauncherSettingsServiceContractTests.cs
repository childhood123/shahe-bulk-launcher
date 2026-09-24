using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("SettingsService")]
    public sealed class LauncherSettingsServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.Settings.LauncherSettingsService";
        private const string StateTypeName = "ShaheLauncher.Core.Settings.LauncherSettingsState";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Settings.LauncherSettingsCallbacks";
        private const string StoreTypeName = "ShaheLauncher.Core.Configuration.ConfigStore";
        private const string DefaultUpdateUrl = "https://lqh.viyf.net/shahe-bulk-launcher/update.json";

        [TestMethod]
        public void CreatesExactDefaultsAndIndependentNestedCollections()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                var defaults = (IDictionary<string, object>)ReflectionTestHelpers.InvokeInstance(
                    service, "CreateDefaultSettings");
                Assert.AreEqual(23, defaults.Count);
                Assert.AreEqual(10, defaults["max_history_items"]);
                Assert.AreEqual(false, defaults["hide_path"]);
                Assert.AreEqual(5, defaults["launch_interval"]);
                Assert.AreEqual(1, defaults["launch_concurrency_limit"]);
                Assert.AreEqual(true, defaults["show_tray"]);
                Assert.AreEqual(true, defaults["list_use_left_right_nav"]);
                Assert.AreEqual(false, defaults["auto_minimize_to_tray_after_launch"]);
                Assert.AreEqual(true, defaults["remember_volume"]);
                Assert.AreEqual("detailed", defaults["log_output_mode"]);
                Assert.AreEqual(false, defaults["hotkeys_enabled"]);
                Assert.AreEqual(string.Empty, defaults["speech_interface_dll"]);
                Assert.IsInstanceOfType(defaults["app_history"], typeof(IList));
                Assert.IsInstanceOfType(defaults["hotkeys"], typeof(IDictionary));
                Assert.IsInstanceOfType(defaults["sandbox_groups"], typeof(IDictionary));
                Assert.IsInstanceOfType(defaults["pinned_sandboxes"], typeof(IList));
                Assert.IsInstanceOfType(defaults["sandbox_volume_memory"], typeof(IDictionary));

                IDictionary<string, object> hints = AsDictionary(defaults["speech_assist_hints"]);
                Assert.AreEqual(true, hints["enabled"]);
                Assert.AreEqual("静音当前打开的进程", hints["mute_current"]);
                Assert.AreEqual("静音全部打开的进程", hints["mute_all"]);
                hints["mute_current"] = "changed";
                var second = (IDictionary<string, object>)ReflectionTestHelpers.InvokeInstance(
                    service, "CreateDefaultSettings");
                Assert.AreEqual(
                    "静音当前打开的进程",
                    AsDictionary(second["speech_assist_hints"])["mute_current"]);

                Assert.AreEqual("重置设置", GetStaticString(type, "ResetConfirmationTitle"));
                Assert.AreEqual(
                    "确定要将全部设置重置为默认值吗？\n该操作会覆盖当前设置。",
                    GetStaticString(type, "ResetConfirmationText"));
                Assert.AreEqual("手动校正 Sandboxie 安装路径", GetStaticString(type, "ManualSandboxiePathDialogTitle"));
                Assert.AreEqual("选择沙盒用户数据目录", GetStaticString(type, "SandboxRootDialogTitle"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void NormalizesAndPersistsUpdateManifestUrlLikePython()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                SetProperty(type, service, "UpdateManifestUrl", " http://invalid/update.json ");
                Assert.AreEqual(DefaultUpdateUrl, ReflectionTestHelpers.InvokeInstance(service, "GetUpdateManifestUrl"));
                Assert.AreEqual(DefaultUpdateUrl, ReflectionTestHelpers.InvokeInstance(service, "SetUpdateManifestUrl", "ftp://invalid"));
                Assert.AreEqual(
                    DefaultUpdateUrl,
                    ReflectionTestHelpers.InvokeInstance(store, "GetSecretText", "update_manifest_url", "fallback"));

                const string custom = "HTTPS://example.test/update.json";
                Assert.AreEqual(custom, ReflectionTestHelpers.InvokeInstance(
                    service, "SetUpdateManifestUrl", "  " + custom + "  "));
                Assert.AreEqual(custom, GetProperty<string>(type, service, "UpdateManifestUrl"));
                Assert.AreEqual(
                    custom,
                    ReflectionTestHelpers.InvokeInstance(store, "GetSecretText", "update_manifest_url", "fallback"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void LoadsStoredSettingsWithNestedSpeechMergeAndLegacyKeyRemoval()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                ReflectionTestHelpers.InvokeInstance(
                    store,
                    "SetJson",
                    "settings_json",
                    Map(
                        "max_history_items", 19,
                        "log_output_mode", "关闭日志输出",
                        "speech_assist_hints", Map("enabled", "关闭", "mute_current", "自定义提示"),
                        "auto_update_on_startup", true,
                        "auto_check_update_on_startup", true,
                        "update_manifest_url", "https://plain.invalid"));
                const string secret = "https://secret.example/update.json";
                ReflectionTestHelpers.InvokeInstance(store, "SetSecretText", "update_manifest_url", secret);

                IDictionary<string, object> settings = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(service, "LoadSettings"));
                Assert.AreEqual(19L, Convert.ToInt64(settings["max_history_items"]));
                Assert.AreEqual(true, settings["show_tray"]);
                Assert.AreEqual("off", settings["log_output_mode"]);
                Assert.IsFalse(settings.ContainsKey("auto_update_on_startup"));
                Assert.IsFalse(settings.ContainsKey("auto_check_update_on_startup"));
                Assert.IsFalse(settings.ContainsKey("update_manifest_url"));
                IDictionary<string, object> hints = AsDictionary(settings["speech_assist_hints"]);
                Assert.AreEqual("关闭", hints["enabled"]);
                Assert.AreEqual("自定义提示", hints["mute_current"]);
                Assert.AreEqual("静音全部打开的进程", hints["mute_all"]);
                Assert.AreEqual(secret, GetProperty<string>(type, service, "UpdateManifestUrl"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void MigratesLegacyJsonAndFallsBackFromMalformedData()
        {
            string root = CreateTemporaryDirectory();
            string malformedRoot = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                File.WriteAllText(
                    Path.Combine(root, "app_config.dat"),
                    "{\"hide_path\":true,\"log_output_mode\":\"关闭日志输出\",\"speech_assist_hints\":{\"mute_all\":\"旧提示\"},\"update_manifest_url\":\"https://plain.invalid\"}",
                    new UTF8Encoding(false));
                object store;
                object service = CreateService(type, root, out store);
                IDictionary<string, object> loaded = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(service, "LoadSettings"));
                Assert.AreEqual(true, loaded["hide_path"]);
                Assert.AreEqual("off", loaded["log_output_mode"]);
                Assert.AreEqual("旧提示", AsDictionary(loaded["speech_assist_hints"])["mute_all"]);
                Assert.IsFalse(loaded.ContainsKey("update_manifest_url"));
                Assert.IsTrue(Directory.Exists(Path.Combine(root, "Configuration")));
                IDictionary migrated = (IDictionary)ReflectionTestHelpers.InvokeInstance(
                    store, "GetJson", "settings_json", null);
                Assert.AreEqual(true, migrated["hide_path"]);
                Assert.AreEqual(
                    DefaultUpdateUrl,
                    ReflectionTestHelpers.InvokeInstance(store, "GetSecretText", "update_manifest_url", "fallback"));

                File.WriteAllText(Path.Combine(malformedRoot, "app_config.dat"), "{broken", new UTF8Encoding(false));
                object malformedStore;
                object malformedService = CreateService(type, malformedRoot, out malformedStore);
                ReflectionTestHelpers.InvokeInstance(
                    malformedStore, "SetSecretText", "update_manifest_url", "http://invalid");
                IDictionary<string, object> fallback = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(malformedService, "LoadSettings"));
                Assert.AreEqual(false, fallback["hide_path"]);
                Assert.AreEqual("detailed", fallback["log_output_mode"]);
                Assert.AreEqual(DefaultUpdateUrl, GetProperty<string>(type, malformedService, "UpdateManifestUrl"));
                Assert.AreEqual(
                    DefaultUpdateUrl,
                    ReflectionTestHelpers.InvokeInstance(
                        malformedStore, "GetSecretText", "update_manifest_url", "fallback"));
            }
            finally
            {
                Directory.Delete(root, true);
                Directory.Delete(malformedRoot, true);
            }
        }

        [TestMethod]
        public void SaveSettingsWritesRuntimeStateAndRemovesLegacyKeys()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                Type stateType = RequireType(StateTypeName);
                object state = Activator.CreateInstance(stateType);
                SetProperty(stateType, state, "Settings", Map(
                    "keep", 7,
                    "auto_update_on_startup", true,
                    "auto_check_update_on_startup", true,
                    "update_manifest_url", "https://plain.invalid"));
                SetProperty(stateType, state, "AppHistory", new List<object> { Map("path", "raw") });
                SetProperty(stateType, state, "MaxHistoryItems", 4);
                SetProperty(stateType, state, "HidePath", true);
                SetProperty(stateType, state, "SandboxiePath", "C:\\Sandboxie");
                SetProperty(stateType, state, "SandboxRoot", "D:\\SandboxRoot");
                SetProperty(stateType, state, "LaunchInterval", 7);
                SetProperty(stateType, state, "LaunchConcurrencyLimit", 3);
                SetProperty(stateType, state, "DynamicLaunchDelayByLoad", true);
                SetProperty(stateType, state, "ShowTray", false);
                SetProperty(stateType, state, "AutoRun", true);
                SetProperty(stateType, state, "AutoSelectRangeOnCheck", true);
                SetProperty(stateType, state, "ListUseLeftRightNavigation", false);
                SetProperty(stateType, state, "SkipRunningSandboxOnLaunch", true);
                SetProperty(stateType, state, "RememberVolume", false);
                SetProperty(stateType, state, "LogOutputMode", "关闭日志输出");
                SetProperty(stateType, state, "HotkeysEnabled", true);
                SetProperty(stateType, state, "Hotkeys", new Dictionary<string, string> { { "launch", "Ctrl+Enter" } });
                SetProperty(stateType, state, "SandboxGroups", Map("常用", new[] { "用户1" }));
                SetProperty(stateType, state, "PinnedSandboxes", new List<string> { "用户1" });
                SetProperty(stateType, state, "SandboxVolumeMemory", Map("用户1", 0.5));
                SetProperty(stateType, state, "SpeechInterfaceDll", "D:\\reader.dll");
                SetProperty(stateType, state, "SpeechAssistHints", Map("enabled", true));
                SetProperty(
                    type,
                    service,
                    "NormalizeAppHistory",
                    new Func<object, int, IList<object>>((value, limit) =>
                        new List<object> { Map("path", "normalized", "limit", limit) }));
                SetProperty(type, service, "UpdateManifestUrl", "https://example.test/update.json");

                ReflectionTestHelpers.InvokeInstance(service, "SaveSettings", state);
                IDictionary<string, object> runtimeSettings = GetProperty<IDictionary<string, object>>(
                    stateType, state, "Settings");
                Assert.IsFalse(runtimeSettings.ContainsKey("auto_update_on_startup"));
                Assert.IsFalse(runtimeSettings.ContainsKey("auto_check_update_on_startup"));
                Assert.IsFalse(runtimeSettings.ContainsKey("update_manifest_url"));
                Assert.AreEqual(false, runtimeSettings["auto_minimize_to_tray_after_launch"]);
                Assert.AreEqual("off", runtimeSettings["log_output_mode"]);
                Assert.AreEqual("normalized", AsDictionary(((IList<object>)runtimeSettings["app_history"])[0])["path"]);

                IDictionary persisted = (IDictionary)ReflectionTestHelpers.InvokeInstance(
                    store, "GetJson", "settings_json", null);
                Assert.AreEqual(4L, Convert.ToInt64(persisted["max_history_items"]));
                Assert.AreEqual(false, persisted["show_tray"]);
                Assert.AreEqual("off", persisted["log_output_mode"]);
                Assert.AreEqual(
                    "https://example.test/update.json",
                    ReflectionTestHelpers.InvokeInstance(store, "GetSecretText", "update_manifest_url", "fallback"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ApplySettingsNormalizesStateAndRunsCallbacksInPythonOrder()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                object state = Activator.CreateInstance(stateType);
                SetProperty(stateType, state, "Settings", Map());
                SetProperty(stateType, state, "ShowTray", false);
                SetProperty(type, service, "NormalizeHotkeys", new Func<object, IDictionary<string, string>>(value =>
                    new Dictionary<string, string> { { "normalized", "Ctrl+K" } }));
                SetProperty(type, service, "NormalizeSandboxGroups", new Func<object, IDictionary<string, object>>(value =>
                    Map("常用", new[] { "用户1" })));
                SetProperty(type, service, "NormalizeSandboxNames", new Func<object, IList<string>>(value =>
                    new List<string> { "用户1" }));
                SetProperty(type, service, "NormalizeVolumeMemory", new Func<object, IDictionary<string, object>>(value =>
                    Map("用户1", 0.5)));
                SetProperty(type, service, "NormalizeSpeechInterface", new Func<object, string>(value => string.Empty));

                var calls = new List<string>();
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "CreateTrayIcon", new Action(() => calls.Add("tray:create")));
                SetProperty(callbacksType, callbacks, "DisableTrayDisplay", new Action(() => calls.Add("tray:disable")));
                SetProperty(callbacksType, callbacks, "SetSandboxRoot", new Action<string>(value => calls.Add("list:root")));
                SetProperty(callbacksType, callbacks, "SetRangeAutoCheckEnabled", new Action<bool>(value => calls.Add("list:range")));
                SetProperty(callbacksType, callbacks, "SetLeftRightNavigationEnabled", new Action<bool>(value => calls.Add("list:nav")));
                SetProperty(callbacksType, callbacks, "RefreshSandboxList", new Action(() => calls.Add("list:refresh")));
                SetProperty(callbacksType, callbacks, "UpdateAppPathList", new Action(() =>
                {
                    calls.Add("apps:update");
                    throw new InvalidOperationException("ignored like Python");
                }));
                SetProperty(callbacksType, callbacks, "ApplyHotkeys", new Action(() => calls.Add("hotkeys")));
                SetProperty(callbacksType, callbacks, "CreateMenuBar", new Action(() => calls.Add("menu")));
                SetProperty(callbacksType, callbacks, "ApplyAutoRun", new Action(() =>
                {
                    IDictionary saved = (IDictionary)ReflectionTestHelpers.InvokeInstance(
                        store, "GetJson", "settings_json", null);
                    Assert.AreEqual(true, saved["hotkeys_enabled"]);
                    calls.Add("auto-run");
                }));
                SetProperty(callbacksType, callbacks, "ScheduleProcessMenuRefresh", new Action<int>(delay => calls.Add("schedule:" + delay)));
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(text => calls.Add("status:" + text)));

                string expectedRoot = Path.GetFullPath(Path.Combine(root, ".", "SandboxRoot"));
                IDictionary<string, object> incoming = Map(
                    "max_history_items", 99,
                    "hide_path", "开启",
                    "log_output_mode", "关闭日志输出",
                    "app_history", new List<object> { Map("path", "A.exe") },
                    "launch_interval", -4,
                    "launch_concurrency_limit", 99,
                    "dynamic_launch_delay_by_load", "yes",
                    "auto_run", "1",
                    "auto_select_range_on_check", "on",
                    "list_use_left_right_nav", "关闭",
                    "skip_running_sandbox_on_launch", "是",
                    "remember_volume", 0,
                    "hotkeys_enabled", "true",
                    "hotkeys", Map("raw", "bad"),
                    "sandbox_root", expectedRoot,
                    "sandbox_groups", Map("raw", new string[0]),
                    "pinned_sandboxes", new[] { "raw" },
                    "sandbox_volume_memory", Map("raw", 1),
                    "speech_interface_dll", "bad.dll",
                    "speech_assist_hints", Map("enabled", "关闭", "mute_current", " ", "mute_all", null),
                    "show_tray", true,
                    "auto_update_on_startup", true,
                    "auto_check_update_on_startup", true);

                ReflectionTestHelpers.InvokeInstance(service, "ApplySettings", state, incoming, callbacks);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "tray:create", "list:root", "list:range", "list:nav", "list:refresh",
                        "apps:update", "hotkeys", "menu", "auto-run", "schedule:1", "status:设置已保存"
                    },
                    calls.ToArray());
                Assert.AreEqual(20, GetProperty<int>(stateType, state, "MaxHistoryItems"));
                Assert.AreEqual(true, GetProperty<bool>(stateType, state, "HidePath"));
                Assert.AreEqual("off", GetProperty<string>(stateType, state, "LogOutputMode"));
                Assert.AreEqual(0, GetProperty<int>(stateType, state, "LaunchInterval"));
                Assert.AreEqual(32, GetProperty<int>(stateType, state, "LaunchConcurrencyLimit"));
                Assert.AreEqual(true, GetProperty<bool>(stateType, state, "DynamicLaunchDelayByLoad"));
                Assert.AreEqual(true, GetProperty<bool>(stateType, state, "AutoRun"));
                Assert.AreEqual(true, GetProperty<bool>(stateType, state, "AutoSelectRangeOnCheck"));
                Assert.AreEqual(false, GetProperty<bool>(stateType, state, "ListUseLeftRightNavigation"));
                Assert.AreEqual(false, GetProperty<bool>(stateType, state, "MinimizeToTrayAfterLaunch"));
                Assert.AreEqual(true, GetProperty<bool>(stateType, state, "SkipRunningSandboxOnLaunch"));
                Assert.AreEqual(false, GetProperty<bool>(stateType, state, "RememberVolume"));
                Assert.AreEqual(true, GetProperty<bool>(stateType, state, "HotkeysEnabled"));
                Assert.AreEqual(expectedRoot, GetProperty<string>(stateType, state, "SandboxRoot"));
                Assert.AreEqual(string.Empty, GetProperty<string>(stateType, state, "SpeechInterfaceDll"));
                IDictionary<string, object> hints = GetProperty<IDictionary<string, object>>(
                    stateType, state, "SpeechAssistHints");
                Assert.AreEqual(false, hints["enabled"]);
                Assert.AreEqual("静音当前打开的进程", hints["mute_current"]);
                Assert.AreEqual("静音全部打开的进程", hints["mute_all"]);
                IDictionary<string, object> applied = GetProperty<IDictionary<string, object>>(
                    stateType, state, "Settings");
                Assert.IsFalse(applied.ContainsKey("auto_update_on_startup"));
                Assert.IsFalse(applied.ContainsKey("auto_check_update_on_startup"));
                Assert.AreEqual(string.Empty, applied["speech_interface_dll"]);
                Assert.AreEqual(false, applied["auto_minimize_to_tray_after_launch"]);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ResetSettingsUsesExactTextAndClearsPathsBeforeApplyingDefaults()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                object state = Activator.CreateInstance(stateType);
                SetProperty(stateType, state, "Settings", Map());
                SetProperty(stateType, state, "AppHistory", new List<object> { Map("path", "old") });
                SetProperty(stateType, state, "AppPath", "old.exe");
                SetProperty(stateType, state, "AppDirectory", "C:\\old");
                SetProperty(stateType, state, "SandboxiePath", "C:\\Sandboxie");
                SetProperty(stateType, state, "ShowTray", true);
                var calls = new List<string>();
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "ClearAppPath", new Action(() => calls.Add("apps:clear")));
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(text => calls.Add("status:" + text)));

                ReflectionTestHelpers.InvokeInstance(service, "ResetSettings", state, callbacks);
                CollectionAssert.AreEqual(
                    new[] { "apps:clear", "status:设置已保存", "status:已重置所有设置为默认值" },
                    calls.ToArray());
                Assert.AreEqual(0, GetProperty<IList<object>>(stateType, state, "AppHistory").Count);
                Assert.AreEqual(10, GetProperty<int>(stateType, state, "MaxHistoryItems"));
                Assert.AreEqual(false, GetProperty<bool>(stateType, state, "HidePath"));
                Assert.AreEqual(string.Empty, GetProperty<string>(stateType, state, "AppPath"));
                Assert.AreEqual(string.Empty, GetProperty<string>(stateType, state, "AppDirectory"));
                Assert.AreEqual(string.Empty, GetProperty<string>(stateType, state, "SandboxiePath"));
                Assert.AreEqual(
                    "已重置所有设置为默认值",
                    GetStaticString(type, "ResetCompletedStatus"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void AppliesSandboxRootAndSandboxiePathOnlyWhenValid()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = RequireType(ServiceTypeName);
                object store;
                object service = CreateService(type, root, out store);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                object state = Activator.CreateInstance(stateType);
                SetProperty(stateType, state, "Settings", ReflectionTestHelpers.InvokeInstance(
                    service, "CreateDefaultSettings"));
                var calls = new List<string>();
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "SetSandboxRoot", new Action<string>(value => calls.Add("root:" + value)));
                SetProperty(callbacksType, callbacks, "RefreshSandboxList", new Action(() => calls.Add("refresh")));

                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                    service, "TryApplySandboxRoot", state, " ", true, callbacks));
                string sandboxRoot = Path.Combine(root, "SandboxData", "..");
                string normalizedRoot = Path.GetFullPath(sandboxRoot);
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                    service, "TryApplySandboxRoot", state, sandboxRoot, true, callbacks));
                CollectionAssert.AreEqual(new[] { "root:" + normalizedRoot, "refresh" }, calls.ToArray());
                Assert.AreEqual(normalizedRoot, GetProperty<string>(stateType, state, "SandboxRoot"));

                string invalid = Path.Combine(root, "InvalidSandboxie");
                Directory.CreateDirectory(invalid);
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                    service, "TryApplySandboxiePath", state, invalid));
                string valid = Path.Combine(root, "ValidSandboxie");
                Directory.CreateDirectory(valid);
                File.WriteAllText(Path.Combine(valid, "Start.exe"), string.Empty);
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                    service, "TryApplySandboxiePath", state, "  " + valid + "  "));
                Assert.AreEqual(Path.GetFullPath(valid), GetProperty<string>(stateType, state, "SandboxiePath"));
                Assert.AreEqual(
                    "已校正 Sandboxie 安装路径: " + Path.GetFullPath(valid),
                    ReflectionTestHelpers.InvokeStatic(type, "GetSandboxiePathSuccessStatus", Path.GetFullPath(valid)));
                Assert.AreEqual(
                    "已切换沙盒数据目录: " + normalizedRoot,
                    ReflectionTestHelpers.InvokeStatic(type, "GetSandboxRootSuccessStatus", normalizedRoot));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static object CreateService(Type serviceType, string root, out object store)
        {
            string configurationDirectory = Path.Combine(root, "Configuration");
            store = CreateStore(Path.Combine(configurationDirectory, "shahe_config.ini"));
            return Activator.CreateInstance(
                serviceType,
                store,
                Path.Combine(root, "app_config.dat"),
                configurationDirectory);
        }

        private static object CreateStore(string path)
        {
            Type type = RequireType(StoreTypeName);
            ConstructorInfo constructor = type.GetConstructors().Single(candidate => candidate.GetParameters().Length == 3);
            return constructor.Invoke(new object[] { path, "settings-service-test", new Func<long>(() => 1786118400L) });
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static string GetStaticString(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be public static");
            return (string)field.GetValue(null);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }

        private static Dictionary<string, object> Map(params object[] values)
        {
            var result = new Dictionary<string, object>();
            for (int index = 0; index + 1 < values.Length; index += 2)
            {
                result[(string)values[index]] = values[index + 1];
            }
            return result;
        }

        private static IDictionary<string, object> AsDictionary(object value)
        {
            var dictionary = value as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }
            var json = value as JObject;
            Assert.IsNotNull(json, "value must be a string/object dictionary");
            return json.ToObject<Dictionary<string, object>>();
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
