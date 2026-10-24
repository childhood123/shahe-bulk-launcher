using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Sandboxie")]
    public sealed class SandboxieClientContractTests
    {
        private const string ClientTypeName = "ShaheLauncher.Core.Sandboxie.SandboxieClient";
        private const string SectionTypeName = "ShaheLauncher.Core.Sandboxie.SandboxieIniSection";
        private const string MapsTypeName = "ShaheLauncher.Core.Sandboxie.SandboxieDisplayMaps";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Sandboxie.SandboxieCallbacks";

        [TestMethod]
        public void PreservesPythonTextAndInstallationFilePriority()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type clientType = RequireType(ClientTypeName);
                object client = Activator.CreateInstance(clientType);
                Assert.AreEqual("正在扫描 C/D 盘的 Sandboxie 安装路径...", GetStaticString(clientType, "FirstScanStatusText"));
                Assert.AreEqual("已扫描到 Sandboxie 安装位置：\n", GetStaticString(clientType, "FirstScanSuccessPrefix"));
                Assert.AreEqual("扫描成功", GetStaticString(clientType, "FirstScanSuccessTitle"));
                Assert.AreEqual("未找到 Sandboxie 安装路径，请在“校正”菜单中手动校正 Sandboxie 安装路径。", GetStaticString(clientType, "FirstScanMissingText"));
                Assert.AreEqual("扫描结果", GetStaticString(clientType, "FirstScanResultTitle"));
                Assert.AreEqual(
                    "该操作会备份 Sandboxie.ini，然后清理 UserSettings 段中的 SbieCtrl_WindowCoords 窗口坐标记录。\n\n建议先关闭 Sandboxie Control 再执行。\n\n不会删除沙盒用户，也不会修改 GlobalSettings 或沙盒配置。是否继续？",
                    GetStaticString(clientType, "RepairConfirmationText"));
                Assert.AreEqual("修复 Sandboxie Control 用户设置", GetStaticString(clientType, "RepairDialogTitle"));
                Assert.AreEqual("修复 Sandboxie Control 用户设置失败", GetStaticString(clientType, "RepairFailedStatus"));
                Assert.AreEqual("已修复 Sandboxie Control 用户设置", GetStaticString(clientType, "RepairCompletedStatus"));
                Assert.AreEqual("未发现需要修复的 Sandboxie Control 用户设置", GetStaticString(clientType, "RepairNotNeededStatus"));

                File.WriteAllText(Path.Combine(root, "Start.exe"), string.Empty);
                File.WriteAllText(Path.Combine(root, "sbieini.exe"), string.Empty);
                File.WriteAllText(Path.Combine(root, "SbieDll64.dll"), string.Empty);
                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsValidInstallPath", "  " + root + "  "));
                Assert.AreEqual(Path.Combine(root, "Start.exe"), ReflectionTestHelpers.InvokeInstance(client, "GetStartExecutable", root));
                Assert.AreEqual(Path.Combine(root, "SbieIni.exe"), ReflectionTestHelpers.InvokeInstance(client, "GetSbieIniExecutable", root));
                Assert.AreEqual(Path.Combine(root, "SbieDll64.dll"), ReflectionTestHelpers.InvokeInstance(client, "GetSbieDllPath", root));

                File.WriteAllText(Path.Combine(root, "SbieIni.exe"), string.Empty);
                File.WriteAllText(Path.Combine(root, "SbieDll.dll"), string.Empty);
                Assert.AreEqual(Path.Combine(root, "SbieIni.exe"), ReflectionTestHelpers.InvokeInstance(client, "GetSbieIniExecutable", root));
                Assert.AreEqual(Path.Combine(root, "SbieDll.dll"), ReflectionTestHelpers.InvokeInstance(client, "GetSbieDllPath", root));
                Assert.IsFalse((bool)ReflectionTestHelpers.InvokeInstance(client, "IsValidInstallPath", Path.Combine(root, "missing")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ParsesOfficialBoxesAndIniSectionsWithPythonFiltering()
        {
            Type clientType = RequireType(ClientTypeName);
            Type sectionType = RequireType(SectionTypeName);
            object client = Activator.CreateInstance(clientType);
            string output = "[Box_A]\r\n\"BoxB\"\r\nGlobalSettings\r\nUserSettings_ABC\r\nBox/Bad\r\nbox_a\r\n";
            CollectionAssert.AreEqual(
                new[] { "Box_A", "BoxB" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(client, "ParseEnabledSandboxNames", output, 0)));
            CollectionAssert.AreEqual(
                new string[0],
                ToStringArray(ReflectionTestHelpers.InvokeInstance(client, "ParseEnabledSandboxNames", output, 1)));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "SandboxExistsInOfficialConfig", "box_a", new[] { "Box_A" }));
            Assert.IsFalse((bool)ReflectionTestHelpers.InvokeInstance(client, "SandboxExistsInOfficialConfig", "Box_A", new string[0]));
            Assert.AreEqual("yes", ReflectionTestHelpers.InvokeInstance(client, "GetIniSettingValue", "# comment\n Enabled = yes \nName=demo", "enabled"));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsReservedIniSectionName", "UserSettings_ABC"));
            Assert.IsFalse((bool)ReflectionTestHelpers.InvokeInstance(client, "IsEditableIniSectionName", "GlobalSettings"));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsSandboxConfigSection", "BoxA", "Enabled=y", new string[0]));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsSandboxConfigSection", "BoxB", "Enabled=n", new[] { "boxb" }));
            Assert.IsFalse((bool)ReflectionTestHelpers.InvokeInstance(client, "IsSandboxConfigSection", "BoxC", "Enabled=n", new string[0]));

            IList sections = CreateSections(
                sectionType,
                Tuple.Create("BoxA", "Enabled=y\nBoxAlias=工作盒"),
                Tuple.Create("BoxB", "Enabled=yes\nBoxAliasDisabled=旧别名"),
                Tuple.Create("Disabled", "Enabled=n\nBoxAlias=忽略"),
                Tuple.Create("GlobalSettings", "Enabled=y\nBoxAlias=忽略"));
            IDictionary aliases = (IDictionary)ReflectionTestHelpers.InvokeInstance(client, "BuildAliasMap", sections);
            Assert.AreEqual(2, aliases.Count);
            Assert.AreEqual("工作盒", aliases["boxa"]);
            Assert.AreEqual("旧别名", aliases["boxb"]);
            CollectionAssert.AreEqual(
                new[] { "BoxA", "BoxB" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(
                    client,
                    "SelectInternalSandboxNames",
                    sections,
                    new[] { "boxb" },
                    new[] { "DirectoryOnly" })));
            CollectionAssert.AreEqual(
                new[] { "EnabledOnly" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(
                    client,
                    "SelectInternalSandboxNames",
                    CreateSections(sectionType, Tuple.Create("Disabled", "Enabled=n")),
                    new[] { "EnabledOnly" },
                    new[] { "DirectoryOnly" })));
            CollectionAssert.AreEqual(
                new[] { "Directory_Only" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(
                    client,
                    "SelectInternalSandboxNames",
                    CreateSections(sectionType, Tuple.Create("Disabled", "Enabled=n")),
                    new string[0],
                    new[] { "Directory_Only", "Bad Name!" })));
        }

        [TestMethod]
        public void BuildsDisplayMapsAndGeneratesInternalNamesLikePython()
        {
            Type clientType = RequireType(ClientTypeName);
            Type mapsType = RequireType(MapsTypeName);
            object client = Activator.CreateInstance(clientType);
            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "box_a", "Alpha" },
                { "boxb", "Alpha" }
            };
            object maps = ReflectionTestHelpers.InvokeInstance(
                client,
                "BuildDisplayMaps",
                new[] { "Box_A", "BoxB", "Box_C", "box_a" },
                aliases);
            IDictionary displayToName = (IDictionary)GetProperty<object>(mapsType, maps, "DisplayToName");
            IDictionary nameToDisplay = (IDictionary)GetProperty<object>(mapsType, maps, "NameToDisplay");
            Assert.AreEqual("Box_A", displayToName["alpha"]);
            Assert.AreEqual("BoxB", displayToName["alpha [boxb]"]);
            Assert.AreEqual("Alpha", nameToDisplay["box_a"]);
            Assert.AreEqual("Alpha [BoxB]", nameToDisplay["boxb"]);
            Assert.AreEqual("Box C", nameToDisplay["box_c"]);
            Assert.AreEqual("BoxB", ReflectionTestHelpers.InvokeInstance(client, "ResolveInternalName", "Alpha [BoxB]", maps));
            Assert.AreEqual("Alpha [BoxB]", ReflectionTestHelpers.InvokeInstance(client, "GetDisplayName", "BoxB", maps));
            Assert.AreEqual("Alpha [BoxB]", ReflectionTestHelpers.InvokeInstance(client, "GetVolumeMemoryKey", "BoxB", maps));
            CollectionAssert.AreEqual(
                new[] { "Alpha", "Alpha [BoxB]", "Box C" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(client, "ScanDisplayNames", maps)));
            Assert.AreEqual("My_Box", ReflectionTestHelpers.InvokeInstance(client, "DisplayNameToInternalCandidate", " My Box "));
            Assert.AreEqual(
                "My_Box1",
                ReflectionTestHelpers.InvokeInstance(client, "GenerateAvailableInternalSandboxName", "My Box", new[] { "my_box" }, new string[0]));
            Assert.AreEqual(
                "Box2",
                ReflectionTestHelpers.InvokeInstance(client, "GenerateAvailableInternalSandboxName", "中文 名称", new[] { "box1" }, new string[0]));
        }

        [TestMethod]
        public void BuildsStartCommandsAndRecognizesInvalidSandboxOutput()
        {
            Type clientType = RequireType(ClientTypeName);
            object client = Activator.CreateInstance(clientType);
            object launchCommand = ReflectionTestHelpers.InvokeInstance(
                client,
                "BuildStartCommand",
                @"C:\Sandboxie\Start.exe",
                new[] { "", "/box:BoxA", "?", @"C:\Apps\target.exe" },
                true);
            Type commandType = launchCommand.GetType();
            Assert.IsTrue(GetProperty<bool>(commandType, launchCommand, "IsValid"));
            Assert.AreEqual(@"C:\Sandboxie\Start.exe", GetProperty<string>(commandType, launchCommand, "Executable"));
            CollectionAssert.AreEqual(
                new[] { "/box:BoxA", "/silent", @"C:\Apps\target.exe" },
                ToStringArray(GetProperty<object>(commandType, launchCommand, "Arguments")));

            object queryCommand = ReflectionTestHelpers.InvokeInstance(
                client,
                "BuildStartCommand",
                @"C:\Sandboxie\Start.exe",
                new[] { "/box:BoxA", "/listpids" },
                true);
            CollectionAssert.AreEqual(
                new[] { "/box:BoxA", "/listpids" },
                ToStringArray(GetProperty<object>(queryCommand.GetType(), queryCommand, "Arguments")));
            object emptyCommand = ReflectionTestHelpers.InvokeInstance(client, "BuildStartCommand", @"C:\Sandboxie\Start.exe", new[] { "?", " " }, true);
            Assert.IsFalse(GetProperty<bool>(emptyCommand.GetType(), emptyCommand, "IsValid"));
            Assert.AreEqual(
                "/box:BoxA \"C:\\Program Files\\Target App\\target.exe\"",
                ReflectionTestHelpers.InvokeInstance(
                    client,
                    "FormatProcessArguments",
                    (object)new[] { "/box:BoxA", @"C:\Program Files\Target App\target.exe" }));
            Assert.AreEqual(
                "\"C:\\Directory With Space\\\\\"",
                ReflectionTestHelpers.InvokeInstance(
                    client,
                    "FormatProcessArguments",
                    (object)new[] { "C:\\Directory With Space\\" }));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsInvalidSandboxNameOutput", "Invalid sandbox name: BoxA"));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsInvalidSandboxNameOutput", "无效的沙盒名参数: BoxA"));
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(client, "IsInvalidSandboxNameOutput", "参数: DefaultBox"));
            Assert.IsFalse((bool)ReflectionTestHelpers.InvokeInstance(client, "IsInvalidSandboxNameOutput", "access denied"));
        }

        [TestMethod]
        public void ParsesPidsAndUsesApiThenPerBoxFallback()
        {
            Type clientType = RequireType(ClientTypeName);
            Type mapsType = RequireType(MapsTypeName);
            object client = Activator.CreateInstance(clientType);
            object maps = ReflectionTestHelpers.InvokeInstance(client, "BuildDisplayMaps", new[] { "BoxA", "BoxB" }, new Dictionary<string, string>());
            CollectionAssert.AreEqual(
                new[] { 123, 456 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(client, "ParseListPidsOutput", "2\r\n123\r\nPID: 456")));

            var apiMap = new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase)
            {
                { "boxa", new List<int> { 9, 9, 3 } },
                { "boxb", new List<int>() }
            };
            var fallbackCalls = new List<string>();
            Func<string, IList<int>> fallback = name =>
            {
                fallbackCalls.Add(name);
                return new List<int> { 42, 42 };
            };
            CollectionAssert.AreEqual(
                new[] { 3, 9 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(client, "GetSandboxPids", "BoxA", maps, new[] { "boxa", "boxb" }, apiMap, fallback)));
            CollectionAssert.AreEqual(
                new[] { 42 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(client, "GetSandboxPids", "BoxB", maps, new[] { "boxa", "boxb" }, apiMap, fallback)));
            CollectionAssert.AreEqual(new[] { "BoxB" }, fallbackCalls.ToArray());
            CollectionAssert.AreEqual(
                new int[0],
                ToIntArray(ReflectionTestHelpers.InvokeInstance(client, "GetSandboxPids", "Missing", maps, new[] { "boxa", "boxb" }, apiMap, fallback)));

            IDictionary states = (IDictionary)ReflectionTestHelpers.InvokeInstance(
                client,
                "GetRunningSandboxStates",
                new[] { "BoxA", "BoxB", "Bad Name!" },
                maps,
                apiMap,
                fallback);
            Assert.AreEqual(true, states["BoxA"]);
            Assert.AreEqual(false, states["BoxB"]);
            Assert.AreEqual(false, states["Bad Name!"]);
            CollectionAssert.AreEqual(
                new[] { 7, 8 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(
                    client,
                    "GetAllSandboxPids",
                    new[] { "BoxA", "boxa", "BoxB" },
                    maps,
                    new Func<string, IList<int>>(name => name == "BoxA" ? new List<int> { 8, 7 } : new List<int> { 8 }))));
            Assert.IsNotNull(mapsType);
        }

        [TestMethod]
        public void RunsOfficialCreateAndDeleteCommandsInPythonOrder()
        {
            Type clientType = RequireType(ClientTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object client = Activator.CreateInstance(clientType);
            object callbacks = Activator.CreateInstance(callbacksType);
            var calls = new List<string>();
            SetProperty(callbacksType, callbacks, "RunSbieIni", new Func<IList<string>, Tuple<int, string>>(args =>
            {
                calls.Add("ini:" + string.Join("|", args));
                return Tuple.Create(0, string.Empty);
            }));
            SetProperty(callbacksType, callbacks, "RunStart", new Func<IList<string>, Tuple<int, string>>(args =>
            {
                calls.Add("start:" + string.Join("|", args));
                return Tuple.Create(0, string.Empty);
            }));
            int sectionRead = 0;
            SetProperty(callbacksType, callbacks, "ListSectionNames", new Func<IList<string>>(() =>
            {
                sectionRead++;
                return sectionRead == 1 ? new List<string> { "Box1" } : new List<string>();
            }));
            SetProperty(callbacksType, callbacks, "ReloadConfiguration", new Action(() => calls.Add("reload")));
            SetProperty(callbacksType, callbacks, "RefreshDisplayMaps", new Action(() => calls.Add("refresh")));
            SetProperty(callbacksType, callbacks, "EnsureSandboxDirectory", new Action<string>(name => calls.Add("mkdir:" + name)));

            object created = ReflectionTestHelpers.InvokeInstance(
                client,
                "CreateSandboxViaOfficial",
                "中文 沙盒",
                new string[0],
                new string[0],
                callbacks);
            Assert.IsTrue(GetProperty<bool>(created.GetType(), created, "Success"));
            Assert.AreEqual("Box1", GetProperty<string>(created.GetType(), created, "InternalName"));
            CollectionAssert.AreEqual(
                new[]
                {
                    "ini:set|/drv|Box1|Enabled|y",
                    "ini:set|/drv|Box1|ConfigLevel|9",
                    "ini:set|/drv|Box1|BoxAlias|中文 沙盒",
                    "reload",
                    "mkdir:Box1",
                    "refresh"
                },
                calls.ToArray());

            calls.Clear();
            object maps = ReflectionTestHelpers.InvokeInstance(client, "BuildDisplayMaps", new[] { "Box1" }, new Dictionary<string, string>());
            object deleted = ReflectionTestHelpers.InvokeInstance(
                client,
                "DeleteSandboxViaOfficial",
                "Box1",
                maps,
                new[] { "box1" },
                callbacks);
            Assert.IsTrue(GetProperty<bool>(deleted.GetType(), deleted, "Success"));
            CollectionAssert.AreEqual(
                new[]
                {
                    "start:/box:Box1|delete_sandbox_silent",
                    "ini:set|/drv|Box1|*|",
                    "reload"
                },
                calls.ToArray());
        }

        [TestMethod]
        public void LaunchesDetachedRetriesInvalidNameAndKeepsExactStatuses()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string startExe = Path.Combine(root, "Start.exe");
                string appDir = Path.Combine(root, "app");
                string appExe = Path.Combine(appDir, "target.exe");
                File.WriteAllText(startExe, string.Empty);
                Directory.CreateDirectory(appDir);
                File.WriteAllText(appExe, string.Empty);
                Type clientType = RequireType(ClientTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                object client = Activator.CreateInstance(clientType);
                SetProperty(clientType, client, "SandboxiePath", root);
                object maps = ReflectionTestHelpers.InvokeInstance(client, "BuildDisplayMaps", new[] { "BoxA" }, new Dictionary<string, string>());
                object callbacks = Activator.CreateInstance(callbacksType);
                var calls = new List<string>();
                int dispatchCount = 0;
                SetProperty(callbacksType, callbacks, "DispatchStart", new Func<IList<string>, string, Tuple<bool, string>>((args, cwd) =>
                {
                    dispatchCount++;
                    calls.Add("dispatch:" + string.Join("|", args) + ":" + (cwd ?? ""));
                    return dispatchCount == 1
                        ? Tuple.Create(false, "无效的沙盒名参数: BoxA")
                        : Tuple.Create(true, string.Empty);
                }));
                SetProperty(callbacksType, callbacks, "ReloadConfiguration", new Action(() => calls.Add("reload")));
                SetProperty(callbacksType, callbacks, "RefreshDisplayMaps", new Action(() => calls.Add("refresh")));
                SetProperty(callbacksType, callbacks, "RefreshSandboxList", new Action(() => calls.Add("list")));
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(status => calls.Add("status:" + status)));

                object launched = ReflectionTestHelpers.InvokeInstance(
                    client,
                    "StartAppInSandbox",
                    "BoxA",
                    appExe,
                    appDir,
                    maps,
                    new[] { "boxa" },
                    callbacks);
                Assert.IsTrue(GetProperty<bool>(launched.GetType(), launched, "Success"));
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "dispatch:/box:BoxA|" + appExe + ":" + appDir,
                        "reload",
                        "dispatch:/box:BoxA|" + appExe + ":" + appDir
                    },
                    calls.ToArray());

                calls.Clear();
                object missing = ReflectionTestHelpers.InvokeInstance(
                    client,
                    "StartAppInSandbox",
                    "BoxA",
                    appExe,
                    appDir,
                    maps,
                    new[] { "other" },
                    callbacks);
                Assert.IsFalse(GetProperty<bool>(missing.GetType(), missing, "Success"));
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "refresh",
                        "list",
                        "status:启动失败：Sandboxie 未启用沙盒 BoxA，已刷新列表"
                    },
                    calls.ToArray());
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void InvalidNameRetryContinuesWhenReloadThrows()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string startExe = Path.Combine(root, "Start.exe");
                string appDir = Path.Combine(root, "app");
                string appExe = Path.Combine(appDir, "target.exe");
                File.WriteAllText(startExe, string.Empty);
                Directory.CreateDirectory(appDir);
                File.WriteAllText(appExe, string.Empty);
                Type clientType = RequireType(ClientTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                object client = Activator.CreateInstance(clientType);
                SetProperty(clientType, client, "SandboxiePath", root);
                object maps = ReflectionTestHelpers.InvokeInstance(client, "BuildDisplayMaps", new[] { "BoxA" }, new Dictionary<string, string>());
                object callbacks = Activator.CreateInstance(callbacksType);
                int dispatchCount = 0;
                SetProperty(callbacksType, callbacks, "DispatchStart", new Func<IList<string>, string, Tuple<bool, string>>((args, cwd) =>
                {
                    dispatchCount++;
                    return dispatchCount == 1
                        ? Tuple.Create(false, "无效的沙盒名参数: BoxA")
                        : Tuple.Create(true, string.Empty);
                }));
                SetProperty(callbacksType, callbacks, "ReloadConfiguration", new Action(() =>
                {
                    throw new InvalidOperationException("reload failed");
                }));

                object launched = ReflectionTestHelpers.InvokeInstance(
                    client,
                    "StartAppInSandbox",
                    "BoxA",
                    appExe,
                    appDir,
                    maps,
                    new[] { "boxa" },
                    callbacks);
                Assert.IsTrue(GetProperty<bool>(launched.GetType(), launched, "Success"));
                Assert.AreEqual(2, dispatchCount);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void FindsInstallPathInRegistryStandardThenWildcardPriority()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string registry = Path.Combine(root, "registry");
                string standard = Path.Combine(root, "standard");
                string wildcard = Path.Combine(root, "wildcard");
                Directory.CreateDirectory(registry);
                Directory.CreateDirectory(standard);
                Directory.CreateDirectory(wildcard);
                File.WriteAllText(Path.Combine(registry, "Start.exe"), string.Empty);
                File.WriteAllText(Path.Combine(standard, "Start.exe"), string.Empty);
                File.WriteAllText(Path.Combine(wildcard, "Start.exe"), string.Empty);
                Type clientType = RequireType(ClientTypeName);
                object client = Activator.CreateInstance(clientType);
                Assert.AreEqual(
                    registry,
                    ReflectionTestHelpers.InvokeInstance(client, "FindSandboxiePath", new[] { registry }, new[] { standard }, new[] { wildcard }));
                File.Delete(Path.Combine(registry, "Start.exe"));
                Assert.AreEqual(
                    standard,
                    ReflectionTestHelpers.InvokeInstance(client, "FindSandboxiePath", new[] { registry }, new[] { standard }, new[] { wildcard }));
                File.Delete(Path.Combine(standard, "Start.exe"));
                Assert.AreEqual(
                    wildcard,
                    ReflectionTestHelpers.InvokeInstance(client, "FindSandboxiePath", new[] { registry }, new[] { standard }, new[] { wildcard }));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static IList CreateSections(Type sectionType, params Tuple<string, string>[] values)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(sectionType));
            foreach (Tuple<string, string> value in values)
            {
                object section = Activator.CreateInstance(sectionType);
                SetProperty(sectionType, section, "Name", value.Item1);
                SetProperty(sectionType, section, "Block", value.Item2);
                list.Add(section);
            }

            return list;
        }

        private static string[] ToStringArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(item => Convert.ToString(item)).ToArray();
        }

        private static int[] ToIntArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(Convert.ToInt32).ToArray();
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-sandboxie-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
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
    }
}
