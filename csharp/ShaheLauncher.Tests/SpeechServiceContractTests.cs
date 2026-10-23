using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Speech;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Speech")]
    public sealed class SpeechServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.Speech.SpeechService";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Speech.SpeechServiceCallbacks";
        private const string BackendTypeName = "ShaheLauncher.Core.Speech.SpeechAssistBackend";
        private static object CapturedSettingsRequest;

        [TestMethod]
        public void SanitizesSpeechConfigAndReaderIniDefaultsLikePython()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            IDictionary<string, object> config = Map(
                "DefaultEngine", "VTTS",
                "TTSInfos", Map(
                    "  Engine A  ", Map("Speed", "250", "Volume", -1, "Pitch", "bad", "Name", " Voice "),
                    "VTTS", Map("Speed", 10),
                    "", Map("Speed", 20)));

            IDictionary<string, object> cleaned = AsDictionary(
                ReflectionTestHelpers.InvokeStatic(serviceType, "SanitizeSpeechConfig", config));
            Assert.AreEqual("Engine A", cleaned["DefaultEngine"]);
            IDictionary<string, object> infos = AsDictionary(cleaned["TTSInfos"]);
            Assert.AreEqual(1, infos.Count);
            IDictionary<string, object> engine = AsDictionary(infos["Engine A"]);
            Assert.AreEqual(200, engine["Speed"]);
            Assert.AreEqual(0, engine["Volume"]);
            Assert.AreEqual(50, engine["Pitch"]);
            Assert.AreEqual("Voice", engine["Name"]);

            IDictionary<string, object> reader = AsDictionary(
                ReflectionTestHelpers.InvokeStatic(
                    serviceType,
                    "NormalizeReaderIniSettings",
                    Map(
                        "zdsr", Map("type", " 0 ", "channelName", " channel "),
                        "boy", Map("Param1", "TRUE", "Param3", "no"))));
            IDictionary<string, object> zdsr = AsDictionary(reader["zdsr"]);
            IDictionary<string, object> boy = AsDictionary(reader["boy"]);
            Assert.AreEqual("0", zdsr["type"]);
            Assert.AreEqual("channel", zdsr["channelName"]);
            Assert.AreEqual("", zdsr["keyDownInterrupt"]);
            Assert.AreEqual("false", boy["Param3"]);
            Assert.AreEqual(
                "false",
                boy["Param2"],
                string.Join("|", boy.Select(item => item.Key + "=" + (item.Value ?? "<null>"))));
        }

        [TestMethod]
        public void ResolvesConfigurationAndTolkResourcesWithConfigurationWriteBoundary()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-path-");
            try
            {
                string configDirectory = Path.Combine(root, "Configurations");
                string internalDirectory = Path.Combine(root, "internal");
                Directory.CreateDirectory(internalDirectory);
                string sourceConfig = Path.Combine(internalDirectory, "SpeechConfig.json");
                string tolkPath = Path.Combine(internalDirectory, "Tolk.dll");
                File.WriteAllText(sourceConfig, "{}");
                File.WriteAllText(tolkPath, "placeholder");
                object service = CreateService(serviceType, root, configDirectory, Activator.CreateInstance(callbacksType));

                Assert.AreEqual(
                    Path.Combine(configDirectory, "SpeechConfig.json"),
                    ReflectionTestHelpers.InvokeInstance(service, "GetSpeechConfigPath"));
                Assert.AreEqual(
                    Path.GetFullPath(configDirectory),
                    ReflectionTestHelpers.InvokeInstance(service, "GetSpeechRuntimeDirectory"));
                Assert.IsTrue(Directory.Exists(configDirectory));
                Assert.AreEqual(
                    Path.GetFullPath(tolkPath),
                    ReflectionTestHelpers.InvokeInstance(service, "GetTolkDllPath"));
                Assert.AreEqual(
                    Path.GetFullPath(internalDirectory),
                    ReflectionTestHelpers.InvokeInstance(service, "GetAssistSpeechEngineDirectory"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ReadsAndWritesReaderIniSettingsUnderConfigurationDirectory()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-ini-");
            try
            {
                string configDirectory = Path.Combine(root, "Configurations");
                Directory.CreateDirectory(configDirectory);
                File.WriteAllText(
                    Path.Combine(configDirectory, "ZDSRAPI.ini"),
                    "[settings]\r\ntype = 0\r\nchannelName = channel\r\nkeyDownInterrupt = 1\r\nalwaysInterrupt = 0\r\n");
                File.WriteAllText(
                    Path.Combine(configDirectory, "boyctrl.ini"),
                    "[Config]\r\nParam1 = TRUE\r\nParam2 = no\r\nParam3 = true\r\nParam4 = false\r\n");
                object service = CreateService(serviceType, root, configDirectory, Activator.CreateInstance(callbacksType));

                IDictionary<string, object> loaded = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(service, "LoadReaderIniSettings"));
                IDictionary<string, object> zdsr = AsDictionary(loaded["zdsr"]);
                IDictionary<string, object> boy = AsDictionary(loaded["boy"]);
                Assert.AreEqual("0", zdsr["type"]);
                Assert.AreEqual("channel", zdsr["channelName"]);
                Assert.AreEqual("true", boy["Param1"]);
                Assert.AreEqual("false", boy["Param2"]);

                bool saved = (bool)ReflectionTestHelpers.InvokeInstance(
                    service,
                    "SaveReaderIniSettings",
                    Map(
                        "zdsr", Map("type", " 1 ", "channelName", " new-channel "),
                        "boy", Map("Param1", "false", "Param2", "TRUE", "Param3", "bad", "Param4", "true")));
                Assert.IsTrue(saved);
                string zdsrText = File.ReadAllText(Path.Combine(configDirectory, "ZDSRAPI.ini"));
                string boyText = File.ReadAllText(Path.Combine(configDirectory, "boyctrl.ini"));
                StringAssert.Contains(zdsrText, "type = 1");
                StringAssert.Contains(zdsrText, "channelName = new-channel");
                StringAssert.Contains(boyText, "Param2 = true");
                StringAssert.Contains(boyText, "Param3 = false");
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void IgnoresReaderIniOutsideConfigurationDirectory()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-lib-ini-");
            try
            {
                string lib = Path.Combine(root, "lib");
                Directory.CreateDirectory(lib);
                File.WriteAllText(Path.Combine(lib, "ZDSRAPI.ini"),
                    "[settings]\r\ntype = 1\r\nchannelName = lib-channel\r\n");
                object service = CreateService(serviceType, root, Path.Combine(root, "Configurations"),
                    Activator.CreateInstance(callbacksType));

                IDictionary<string, object> loaded = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(service, "LoadReaderIniSettings"));
                IDictionary<string, object> zdsr = AsDictionary(loaded["zdsr"]);
                Assert.AreEqual(string.Empty, zdsr["type"]);
                Assert.AreEqual(string.Empty, zdsr["channelName"]);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void AddsLibDirectoryToPathBeforeLoadingAssistBackend()
        {
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-lib-path-");
            string oldPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                string lib = Path.Combine(root, "lib");
                Directory.CreateDirectory(lib);
                File.WriteAllText(Path.Combine(lib, "Tolk.dll"), "placeholder");
                string observedPath = string.Empty;
                var backend = new SpeechAssistBackend
                {
                    Load = () => { },
                    IsLoaded = () => true,
                    Unload = () => { }
                };
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "LoadAssistBackend",
                    new Func<string, SpeechAssistBackend>(path =>
                    {
                        observedPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                        return backend;
                    }));
                object service = Activator.CreateInstance(
                    typeof(SpeechService),
                    root,
                    Path.Combine(root, "Configurations"),
                    callbacks);

                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(service, "EnsureAssistSpeechRuntime"));
                Assert.IsTrue(observedPath.StartsWith(lib + Path.PathSeparator, StringComparison.OrdinalIgnoreCase));
                ((SpeechService)service).ShutdownAssistSpeechRuntime();
            }
            finally
            {
                Environment.SetEnvironmentVariable("PATH", oldPath);
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void DetectsReadersMatchesEnginesAndKeepsInterfaceChoices()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object callbacks = Activator.CreateInstance(callbacksType);
            SetProperty(callbacksType, callbacks, "EnumerateProcessNames",
                new Func<IEnumerable<string>>(() => new[] { "other.exe", "ZDSRAPI.exe" }));
            object service = CreateService(serviceType, CreateTemporaryDirectory("shahe-speech-detect-"),
                CreateTemporaryDirectory("shahe-speech-config-"), callbacks);
            try
            {
                IDictionary<string, object> detected = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(
                        service,
                        "DetectScreenReaderAndMatchEngine",
                        (object)new[] { "普通引擎", "争渡中文引擎" }));
                Assert.AreEqual("zdsr", detected["reader_key"]);
                Assert.AreEqual("争渡读屏", detected["reader_name"]);
                Assert.AreEqual("zdsrapi.exe", detected["process_name"]);
                Assert.AreEqual("争渡中文引擎", detected["engine_name"]);
                Assert.AreEqual("zdsr", ReflectionTestHelpers.InvokeInstance(
                    service, "GetPreferredSpeechInterfaceDll", (object)new[] { "nvda", "zdsr" }));

                IList<object> choices = ToObjectList(
                    ReflectionTestHelpers.InvokeInstance(service, "ScanReaderInterfaceDlls"));
                Assert.AreEqual(3, choices.Count);
                Assert.AreEqual("NVDA", AsDictionary(choices[0])["label"]);
                Assert.AreEqual("zdsr", AsDictionary(choices[1])["path"]);
                Assert.AreEqual("boy", AsDictionary(choices[2])["path"]);
            }
            finally
            {
                Directory.Delete(Convert.ToString(GetProperty(serviceType, service, "AppDirectory")), true);
                Directory.Delete(Convert.ToString(GetProperty(serviceType, service, "ConfigurationDirectory")), true);
            }
        }

        [TestMethod]
        public void NormalizesOnlyAllowedSpeechInterfaceValues()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-speech-interface-app-"),
                CreateTemporaryDirectory("shahe-speech-interface-config-"),
                Activator.CreateInstance(callbacksType));
            Assert.AreEqual("nvda", ReflectionTestHelpers.InvokeInstance(service, "NormalizeSpeechInterfaceValue", "NVDACONTROLLERCLIENT64.DLL"));
            Assert.AreEqual("zdsr", ReflectionTestHelpers.InvokeInstance(service, "NormalizeSpeechInterfaceValue", "D:\\reader\\ZDSRAPI_X64.DLL"));
            Assert.AreEqual("boy", ReflectionTestHelpers.InvokeInstance(service, "NormalizeSpeechInterfaceValue", "保益"));
            Assert.AreEqual("", ReflectionTestHelpers.InvokeInstance(service, "NormalizeSpeechInterfaceValue", "D:\\reader\\unknown.dll"));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "IsAllowedSpeechInterfaceDllPath", "boyctrl.dll"));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(service, "IsAllowedSpeechInterfaceDllPath", "unknown.dll"));
        }

        [TestMethod]
        public void LoadsAndSavesSpeechConfigInConfigurationDirectoryWithOriginalStatusText()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-config-");
            try
            {
                string configDirectory = Path.Combine(root, "Configurations");
                Directory.CreateDirectory(configDirectory);
                File.WriteAllText(
                    Path.Combine(configDirectory, "SpeechConfig.json"),
                    "{\"DefaultEngine\":\"None\",\"TTSInfos\":{\" Engine \":{\"Speed\":201,\"Volume\":20,\"Pitch\":40,\"Name\":\" Voice \"},\"VTTS\":{}}}");
                object callbacks = Activator.CreateInstance(callbacksType);
                var statuses = new List<string>();
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
                object service = CreateService(serviceType, root, configDirectory, callbacks);

                IDictionary<string, object> loaded = AsDictionary(
                    ReflectionTestHelpers.InvokeInstance(service, "LoadSpeechConfig"));
                Assert.AreEqual("Engine", loaded["DefaultEngine"]);
                Assert.IsTrue(File.Exists(Path.Combine(configDirectory, "SpeechConfig.json")));
                IDictionary<string, object> saved = Map(
                    "DefaultEngine", "Engine",
                    "TTSInfos", Map("Engine", Map("Speed", 90, "Volume", 80, "Pitch", 70, "Name", "Saved")));
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "SaveSpeechConfig", saved));
                StringAssert.Contains(File.ReadAllText(Path.Combine(configDirectory, "SpeechConfig.json")), "Saved");
                CollectionAssert.Contains(statuses, "朗读设置已保存");
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void SavesInterfaceAndAssistHintsAndPrefixesEnabledStatusText()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object callbacks = Activator.CreateInstance(callbacksType);
            int saves = 0;
            SetProperty(callbacksType, callbacks, "SaveSettings", new Action(() => saves++));
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-speech-hints-app-"),
                CreateTemporaryDirectory("shahe-speech-hints-config-"),
                callbacks);

            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "SaveSpeechInterfaceDll", "争渡"));
            Assert.AreEqual("zdsr", GetProperty(serviceType, service, "SpeechInterfaceDll"));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service,
                "SaveSpeechAssistHints",
                Map("enabled", "是", "mute_current", "当前静音", "mute_all", "")));
            IDictionary<string, object> hints = AsDictionary(GetProperty(serviceType, service, "SpeechAssistHints"));
            Assert.AreEqual(true, hints["enabled"]);
            Assert.AreEqual("当前静音", hints["mute_current"]);
            Assert.AreEqual("静音全部打开的进程", hints["mute_all"]);
            Assert.AreEqual("当前静音，已完成", ReflectionTestHelpers.InvokeInstance(service, "WithAssistHint", "mute_current", "已完成"));
            Assert.AreEqual(2, saves);
        }

        [TestMethod]
        public void SpeaksAssistTextOnWorkerThreadAndShutdownDisablesRuntime()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            Type backendType = ReflectionTestHelpers.RequireType(BackendTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-runtime-");
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "internal"));
                File.WriteAllText(Path.Combine(root, "internal", "Tolk.dll"), "placeholder");
                object callbacks = Activator.CreateInstance(callbacksType);
                object backend = Activator.CreateInstance(backendType);
                var calls = new List<string>();
                var done = new ManualResetEventSlim(false);
                int callerThread = Thread.CurrentThread.ManagedThreadId;
                int outputThread = 0;
                SetProperty(backendType, backend, "TrySapi", new Action<bool>(value => calls.Add("try:" + value)));
                SetProperty(backendType, backend, "PreferSapi", new Action<bool>(value => calls.Add("prefer:" + value)));
                SetProperty(backendType, backend, "Load", new Action(() => calls.Add("load")));
                SetProperty(backendType, backend, "IsLoaded", new Func<bool>(() => true));
                SetProperty(backendType, backend, "Silence", new Action(() => calls.Add("silence")));
                SetProperty(backendType, backend, "Output", new Action<string, bool>((text, interrupt) =>
                {
                    outputThread = Thread.CurrentThread.ManagedThreadId;
                    calls.Add("output:" + text + ":" + interrupt);
                    done.Set();
                }));
                SetProperty(backendType, backend, "Unload", new Action(() => calls.Add("unload")));
                SetBackendLoader(callbacksType, callbacks, backend);
                SetProperty(callbacksType, callbacks, "IsWindowClosing", new Func<bool>(() => false));
                SetProperty(callbacksType, callbacks, "IsDestroyStarted", new Func<bool>(() => false));
                object service = CreateService(serviceType, root, Path.Combine(root, "Configurations"), callbacks);

                ReflectionTestHelpers.InvokeInstance(service, "SpeakAssistText", "  测试朗读  ", true);
                Assert.IsTrue(done.Wait(3000));
                Assert.AreNotEqual(callerThread, outputThread);
                CollectionAssert.AreEqual(
                    new[] { "try:True", "prefer:False", "load", "silence", "output:测试朗读:True" },
                    calls.ToArray());

                ReflectionTestHelpers.InvokeInstance(service, "ShutdownAssistSpeechRuntime");
                CollectionAssert.AreEqual(
                    new[] { "try:True", "prefer:False", "load", "silence", "output:测试朗读:True", "unload" },
                    calls.ToArray());
                Assert.AreEqual(true, GetProperty(serviceType, service, "AssistSpeechDisabled"));
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(service, "EnsureAssistSpeechRuntime"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void FiltersUnknownAssistEventsAndSuppressesDuplicateSpeechBriefly()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            Type backendType = ReflectionTestHelpers.RequireType(BackendTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-dedupe-");
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "internal"));
                File.WriteAllText(Path.Combine(root, "internal", "Tolk.dll"), "placeholder");
                object callbacks = Activator.CreateInstance(callbacksType);
                object backend = Activator.CreateInstance(backendType);
                var outputs = new List<string>();
                var done = new ManualResetEventSlim(false);
                SetProperty(backendType, backend, "TrySapi", new Action<bool>(_ => { }));
                SetProperty(backendType, backend, "Load", new Action(() => { }));
                SetProperty(backendType, backend, "IsLoaded", new Func<bool>(() => true));
                SetProperty(backendType, backend, "Silence", new Action(() => { }));
                SetProperty(backendType, backend, "Output", new Action<string, bool>((text, _) =>
                {
                    lock (outputs)
                    {
                        outputs.Add(text);
                    }
                    done.Set();
                }));
                SetBackendLoader(callbacksType, callbacks, backend);
                SetProperty(callbacksType, callbacks, "IsWindowClosing", new Func<bool>(() => false));
                SetProperty(callbacksType, callbacks, "IsDestroyStarted", new Func<bool>(() => false));
                object service = CreateService(serviceType, root, Path.Combine(root, "Configurations"), callbacks);

                ReflectionTestHelpers.InvokeInstance(service, "SpeakProcessAssist", "重复文案", true, "unknown");
                ReflectionTestHelpers.InvokeInstance(service, "SpeakProcessAssist", "重复文案", true, "mute_current");
                ReflectionTestHelpers.InvokeInstance(service, "SpeakProcessAssist", "重复文案", true, "mute_current");
                Assert.IsTrue(done.Wait(3000));
                Thread.Sleep(80);
                lock (outputs)
                {
                    CollectionAssert.AreEqual(new[] { "重复文案" }, outputs.ToArray());
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void OpensSpeechSettingsWithOriginalStatusWarningAndCallbackPayload()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-speech-settings-");
            try
            {
                object callbacks = Activator.CreateInstance(callbacksType);
                var statuses = new List<string>();
                var warnings = new List<string>();
                int menuCreates = 0;
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
                SetProperty(callbacksType, callbacks, "ShowWarning",
                    new Action<string, string>((message, title) => warnings.Add(title + ":" + message)));
                SetProperty(callbacksType, callbacks, "CreateMenuBar", new Action(() => menuCreates++));
                CapturedSettingsRequest = null;
                SetProperty(callbacksType, callbacks, "ShowSettings", CreateShowSettingsDelegate(callbacksType));
                object service = CreateService(
                    serviceType,
                    root,
                    Path.Combine(root, "Configurations"),
                    callbacks);

                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "OnSpeechSettings"));
                CollectionAssert.Contains(statuses, "未检测到读屏辅助接口，请检查 internal 目录");
                CollectionAssert.Contains(warnings, "朗读设置提示:未检测到读屏辅助接口。\n请确认 internal 目录中的读屏辅助文件完整。");
                Assert.AreEqual(1, menuCreates);
                Assert.IsNotNull(CapturedSettingsRequest);
                Type requestType = ReflectionTestHelpers.RequireType("ShaheLauncher.Core.Speech.SpeechSettingsRequest");
                Assert.AreEqual(0, AsDictionary(GetProperty(requestType, CapturedSettingsRequest, "SpeechConfig")).Count);
                Assert.AreEqual(3, ToObjectList(GetProperty(requestType, CapturedSettingsRequest, "InterfaceDllChoices")).Count);
                Assert.AreEqual("", GetProperty(requestType, CapturedSettingsRequest, "SelectedInterfaceDll"));
            }
            finally
            {
                CapturedSettingsRequest = null;
                Directory.Delete(root, true);
            }
        }

        private static object CreateService(Type serviceType, string appDirectory, string configurationDirectory, object callbacks)
        {
            return Activator.CreateInstance(serviceType, appDirectory, configurationDirectory, callbacks);
        }

        private static object CreateBackendLoaderDelegate(Type callbacksType, object backend)
        {
            PropertyInfo property = callbacksType.GetProperty("LoadAssistBackend");
            Assert.IsNotNull(property);
            Type delegateType = property.PropertyType;
            Type[] genericArguments = delegateType.GetGenericArguments();
            ParameterExpression parameter = Expression.Parameter(genericArguments[0], "path");
            UnaryExpression body = Expression.Convert(Expression.Constant(backend), genericArguments[1]);
            return Expression.Lambda(delegateType, body, parameter).Compile();
        }

        private static void SetBackendLoader(Type callbacksType, object callbacks, object backend)
        {
            SetProperty(callbacksType, callbacks, "LoadAssistBackend", CreateBackendLoaderDelegate(callbacksType, backend));
        }

        private static object CreateShowSettingsDelegate(Type callbacksType)
        {
            PropertyInfo property = callbacksType.GetProperty("ShowSettings");
            Assert.IsNotNull(property);
            Type delegateType = property.PropertyType;
            Type requestType = delegateType.GetGenericArguments()[0];
            ParameterExpression parameter = Expression.Parameter(requestType, "request");
            MethodInfo capture = typeof(SpeechServiceContractTests).GetMethod(
                nameof(CaptureSettingsRequest),
                BindingFlags.NonPublic | BindingFlags.Static);
            Expression captureCall = Expression.Call(
                capture,
                Expression.Convert(parameter, typeof(object)));
            Expression body = Expression.Block(captureCall, Expression.Constant(true));
            return Expression.Lambda(delegateType, body, parameter).Compile();
        }

        private static void CaptureSettingsRequest(object request)
        {
            CapturedSettingsRequest = request;
        }

        private static IDictionary<string, object> Map(params object[] values)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            for (int index = 0; index + 1 < values.Length; index += 2)
            {
                result[Convert.ToString(values[index])] = values[index + 1];
            }
            return result;
        }

        private static IDictionary<string, object> AsDictionary(object value)
        {
            return (IDictionary<string, object>)value;
        }

        private static IList<object> ToObjectList(object value)
        {
            return ((IEnumerable)value).Cast<object>().ToList();
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value, null);
        }

        private static object GetProperty(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return property.GetValue(target, null);
        }

        private static string CreateTemporaryDirectory(string prefix)
        {
            string path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
