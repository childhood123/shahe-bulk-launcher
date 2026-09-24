using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("SandboxAdmin")]
    public sealed class SandboxAdminServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.SandboxAdmin.SandboxAdminService";
        private const string StateTypeName = "ShaheLauncher.Core.SandboxAdmin.SandboxAdminState";
        private const string CallbacksTypeName = "ShaheLauncher.Core.SandboxAdmin.SandboxAdminCallbacks";
        private const string ScheduleJobTypeName = "ShaheLauncher.Core.SandboxAdmin.SandboxScheduleJob";

        [TestMethod]
        public void NormalizesGroupsNamesVolumeAndHistoryLikePython()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object service = Activator.CreateInstance(serviceType);
            object callbacks = Activator.CreateInstance(callbacksType);
            SetProperty(callbacksType, callbacks, "GetExistingSandboxDisplayNames",
                new Func<IEnumerable<string>>(() => new[] { "工作盒" }));
            SetProperty(callbacksType, callbacks, "GetExistingSandboxNames",
                new Func<IEnumerable<string>>(() => new[] { "Box_A" }));
            SetProperty(callbacksType, callbacks, "NormalizeSandboxName",
                new Func<string, string>(name => string.Equals(name, "工作盒", StringComparison.OrdinalIgnoreCase) ? "Box_A" : name));
            SetProperty(callbacksType, callbacks, "GetSandboxDisplayName",
                new Func<string, string>(name => string.Equals(name, "Box_A", StringComparison.OrdinalIgnoreCase) ? "工作盒" : name));
            SetProperty(callbacksType, callbacks, "GetVolumeMemoryKey",
                new Func<string, string>(name => string.Equals(name, "Box_A", StringComparison.OrdinalIgnoreCase) ? "工作盒" : name));

            var groups = new Dictionary<string, object>
            {
                { " 组一 ", new object[] { "工作盒", "Box_A", "不存在", "工作盒" } },
                { "空组", new object[0] },
                { " ", new[] { "工作盒" } }
            };
            IDictionary normalizedGroups = (IDictionary)ReflectionTestHelpers.InvokeInstance(
                service, "NormalizeSandboxGroups", groups, callbacks);
            Assert.AreEqual(1, normalizedGroups.Count);
            CollectionAssert.AreEqual(new[] { "工作盒" }, ToStringArray(normalizedGroups["组一"]));
            CollectionAssert.AreEqual(
                new[] { "工作盒" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(
                    service, "NormalizeSandboxNameList", new object[] { "Box_A", "工作盒", "不存在", "" }, callbacks)));

            var volume = new Dictionary<string, object>
            {
                { "Box_A", 130.0 }, { "工作盒", 20.4 }, { "", 50 }, { "Bad", "invalid" }
            };
            IDictionary normalizedVolume = (IDictionary)ReflectionTestHelpers.InvokeInstance(
                service, "NormalizeVolumeMemory", volume, callbacks);
            Assert.AreEqual(1, normalizedVolume.Count);
            Assert.AreEqual(20, normalizedVolume["工作盒"]);

            var history = new object[]
            {
                new Dictionary<string, object> { { "path", @"C:\Apps\A.exe" }, { "note", "启动器" } },
                @"C:\Apps\A.exe",
                @"C:\Apps\B.exe",
                new Dictionary<string, object> { { "path", "" } }
            };
            IList normalizedHistory = (IList)ReflectionTestHelpers.InvokeInstance(
                service, "NormalizeAppHistory", history, 2);
            Assert.AreEqual(2, normalizedHistory.Count);
            Assert.AreEqual(@"C:\Apps\A.exe", GetProperty<string>(normalizedHistory[0].GetType(), normalizedHistory[0], "Path"));
            Assert.AreEqual("启动器", GetProperty<string>(normalizedHistory[0].GetType(), normalizedHistory[0], "Note"));
            Assert.AreEqual("B.exe", GetProperty<string>(normalizedHistory[1].GetType(), normalizedHistory[1], "Note"));
        }

        [TestMethod]
        public void PreservesGroupPinStateAndExplicitSideEffects()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object service = Activator.CreateInstance(serviceType);
            object state = Activator.CreateInstance(stateType);
            object callbacks = Activator.CreateInstance(callbacksType);
            SetProperty(stateType, state, "SandboxGroups", new Dictionary<string, IList<string>>
            {
                { "组二", new List<string> { "用户B", "用户C" } },
                { "组一", new List<string> { "用户A", "用户B" } }
            });
            SetProperty(stateType, state, "PinnedSandboxes", new List<string> { "用户A", "用户B" });
            var calls = new List<string>();
            SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(value => calls.Add("status:" + value)));
            SetProperty(callbacksType, callbacks, "SaveSettings", new Action(() => calls.Add("save")));
            SetProperty(callbacksType, callbacks, "SuppressListRefreshStatus",
                new Action<double>(value => calls.Add("suppress:" + value.ToString("0.0"))));
            SetProperty(callbacksType, callbacks, "RefreshSandboxList", new Action(() => calls.Add("refresh")));

            CollectionAssert.AreEqual(new[] { "组一", "组二" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(service, "GetGroupNames", state)));
            CollectionAssert.AreEqual(new[] { "用户A", "用户B" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(service, "GetGroupMembers", state, "组一")));
            CollectionAssert.AreEquivalent(new[] { "用户A", "用户B", "用户C" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(service, "GetGroupedMembers", state)));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "IsSandboxPinned", state, "用户B"));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "TogglePinSandbox", state, "用户B", callbacks));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "TogglePinSandbox", state, "用户C", callbacks));
            CollectionAssert.AreEqual(new[] { "用户C", "用户A" },
                ToStringArray(GetProperty<object>(stateType, state, "PinnedSandboxes")));
            CollectionAssert.AreEqual(
                new[]
                {
                    "status:已取消置顶: 用户B", "save", "suppress:2.0", "refresh",
                    "status:已置顶: 用户C", "save", "suppress:2.0", "refresh"
                }, calls.ToArray());
        }

        [TestMethod]
        public void PreservesBatchNamingValidationTextAndResultStatuses()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            Assert.AreEqual("批量创建沙盒用户", GetStaticString(serviceType, "BatchCreateDialogTitle"));
            Assert.AreEqual("请输入批量创建规则：", GetStaticString(serviceType, "BatchCreatePromptText"));
            Assert.AreEqual("批量重命名沙盒用户", GetStaticString(serviceType, "BatchRenameDialogTitle"));
            Assert.AreEqual("将按当前勾选顺序依次重命名，请设置新命名规则：", GetStaticString(serviceType, "BatchRenamePromptText"));
            Assert.AreEqual("名称校验失败", GetStaticString(serviceType, "NameValidationFailedTitle"));
            CollectionAssert.AreEqual(new[] { "用户01", "用户02", "用户03" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(service, "BuildBatchSandboxNames", " 用户 ", 1, 3, 2)));
            string[] errors = ToStringArray(ReflectionTestHelpers.InvokeInstance(
                service, "ValidateBatchSandboxNames", (object)new[] { "User", "user", "" }));
            CollectionAssert.Contains(errors, "沙盒名不能为空: (空)");
            CollectionAssert.Contains(errors, "名称重复: user");
            Assert.AreEqual("已创建 2 个沙盒用户，跳过 1 个已存在，失败 3 个",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatBatchCreateStatus", 2, 1, 3));
            Assert.AreEqual("已批量重命名 4 个沙盒用户",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatBatchRenameStatus", 4));
            Assert.AreEqual("已删除 2 个，失败 1 个，跳过运行中 1 个",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatBatchDeleteStatus", 2, 1, 1));
            Assert.AreEqual("未删除任何沙盒用户",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatBatchDeleteStatus", 0, 0, 0));
        }

        [TestMethod]
        public void PreservesIniSectionAndUserSettingsTransforms()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            const string source = "GlobalSettings=y\r\n\r\n[BoxA]\r\nEnabled=y\r\nConfigLevel=8\r\n\r\n[BoxB]\r\nEnabled=n\r\n";
            object split = ReflectionTestHelpers.InvokeInstance(service, "SplitIniSections", source);
            Assert.AreEqual("GlobalSettings=y\r\n\r\n", GetProperty<string>(split.GetType(), split, "Preamble"));
            IList sections = (IList)GetProperty<object>(split.GetType(), split, "Sections");
            Assert.AreEqual(2, sections.Count);
            Assert.AreEqual("BoxA", GetProperty<string>(sections[0].GetType(), sections[0], "Name"));

            string renamed = (string)ReflectionTestHelpers.InvokeInstance(
                service, "ReplaceSectionHeaderName", GetProperty<string>(sections[0].GetType(), sections[0], "Block"), "BoxRenamed");
            Assert.IsTrue(renamed.StartsWith("[BoxRenamed]\r\n", StringComparison.Ordinal));
            object updated = ReflectionTestHelpers.InvokeInstance(
                service, "SetIniSettingInBlock", renamed, "ConfigLevel", "9", "\r\n");
            Assert.AreEqual(true, GetTupleItem<bool>(updated, "Item2"));
            StringAssert.Contains(GetTupleItem<string>(updated, "Item1"), "ConfigLevel=9\r\n");
            object removed = ReflectionTestHelpers.InvokeInstance(
                service, "SetIniSettingInBlock", GetTupleItem<string>(updated, "Item1"), "Enabled", null, "\r\n");
            Assert.AreEqual(true, GetTupleItem<bool>(removed, "Item2"));
            Assert.IsFalse(GetTupleItem<string>(removed, "Item1").Contains("Enabled="));
            Assert.AreEqual("[BoxC]\r\nEnabled=y\r\nConfigLevel=9\r\n\r\n",
                ReflectionTestHelpers.InvokeInstance(service, "BuildIniSectionBlock", "BoxC", "\r\n"));

            object cleaned = ReflectionTestHelpers.InvokeInstance(
                service,
                "CleanUserSettingsBlock",
                "[UserSettings_A]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=ok\r\n",
                "\r\n");
            Assert.AreEqual("[UserSettings_A]\r\nKeep=ok\r\n", GetTupleItem<string>(cleaned, "Item1"));
            CollectionAssert.AreEquivalent(
                new[] { "sbiectrl_windowcoords", "enabled" },
                ToStringArray(GetTupleItem<object>(cleaned, "Item2")));
        }

        [TestMethod]
        public void AppliesIniChangesRepairsUserSettingsAndKeepsBackups()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string path = Path.Combine(root, "Sandboxie.ini");
                File.WriteAllText(
                    path,
                    "[GlobalSettings]\r\nValue=1\r\n\r\n[BoxA]\r\nEnabled=y\r\n\r\n[BoxB]\r\nEnabled=y\r\n",
                    new UTF8Encoding(false));
                Type serviceType = RequireType(ServiceTypeName);
                object service = Activator.CreateInstance(serviceType);
                object changed = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "ApplyIniSectionChanges",
                    path,
                    new[] { "BoxC" },
                    new[] { "BoxB" },
                    new Dictionary<string, string> { { "BoxA", "BoxRenamed" } });
                Assert.AreEqual(true, GetTupleItem<bool>(changed, "Item1"));
                Assert.AreEqual(string.Empty, GetTupleItem<string>(changed, "Item2"));
                string content = File.ReadAllText(path);
                StringAssert.Contains(content, "[GlobalSettings]");
                StringAssert.Contains(content, "[BoxRenamed]");
                StringAssert.Contains(content, "[BoxC]\r\nEnabled=y\r\nConfigLevel=9\r\n\r\n");
                Assert.IsFalse(content.Contains("[BoxB]"));
                CollectionAssert.AreEqual(new[] { "BoxRenamed", "BoxC" },
                    ToStringArray(ReflectionTestHelpers.InvokeInstance(service, "ListIniSectionNames", path, true)));
                Assert.AreEqual(1, Directory.GetFiles(root, "Sandboxie.ini.沙河批量打开器备份_*.bak").Length);

                File.AppendAllText(
                    path,
                    "[UserSettings_Test]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=ok\r\n",
                    new UTF8Encoding(false));
                int reloads = 0;
                object repair = ReflectionTestHelpers.InvokeInstance(
                    service, "RepairSandboxieControlUserSettings", path, new Func<bool>(() => { reloads++; return true; }));
                Assert.AreEqual(true, GetTupleItem<bool>(repair, "Item1"));
                StringAssert.StartsWith(
                    GetTupleItem<string>(repair, "Item2"),
                    "已清理 1 个 UserSettings 段中的 2 条异常/易冲突记录");
                Assert.AreEqual(1, reloads);
                content = File.ReadAllText(path);
                Assert.IsFalse(content.Contains("SbieCtrl_WindowCoords="));
                Assert.IsFalse(content.Contains("[UserSettings_Test]\r\nEnabled="));
                StringAssert.Contains(content, "Keep=ok");
                Assert.IsTrue(Directory.GetFiles(root, "Sandboxie.ini.沙河批量打开器备份_*.bak").Length >= 1);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void RenamesAndDeletesSettingsAndScheduledJobReferences()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            Type scheduleJobType = RequireType(ScheduleJobTypeName);
            object service = Activator.CreateInstance(serviceType);
            object state = Activator.CreateInstance(stateType);
            object callbacks = Activator.CreateInstance(callbacksType);
            SetProperty(stateType, state, "SandboxGroups", new Dictionary<string, IList<string>>
            {
                { "组一", new List<string> { "用户A", "用户B", "用户A" } }
            });
            SetProperty(stateType, state, "PinnedSandboxes", new List<string> { "用户A", "用户B" });
            SetProperty(stateType, state, "SandboxVolumeMemory",
                new Dictionary<string, int> { { "用户A", 10 }, { "用户B", 20 } });
            SetProperty(stateType, state, "CachedMuteStateBySandbox",
                new Dictionary<string, bool> { { "用户A", true }, { "用户B", false } });
            IDictionary jobs = CreateScheduleJobs(scheduleJobType,
                Tuple.Create("launch", "定时启动 2 个沙盒", new[] { "用户A", "用户B" }),
                Tuple.Create("close", "定时关闭 1 个沙盒", new[] { "用户B" }));
            SetProperty(stateType, state, "ScheduleJobs", jobs);

            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "RenameSandboxReferencesByDisplay", state,
                new Dictionary<string, string> { { "用户a", "新用户A" } }));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "RenameScheduleJobReferencesByDisplay", state,
                new Dictionary<string, string> { { "用户a", "新用户A" } }));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "RenameSandboxReferences", state,
                new Dictionary<string, string> { { "新用户A", "BoxA2" } }));
            CollectionAssert.AreEqual(new[] { "BoxA2", "用户B" },
                ToStringArray(((IDictionary)GetProperty<object>(stateType, state, "SandboxGroups"))["组一"]));
            CollectionAssert.AreEqual(new[] { "BoxA2", "用户B" },
                ToStringArray(GetProperty<object>(stateType, state, "PinnedSandboxes")));
            IDictionary mute = (IDictionary)GetProperty<object>(stateType, state, "CachedMuteStateBySandbox");
            Assert.AreEqual(true, mute["用户A"]);
            Assert.IsFalse(mute.Contains("BoxA2"));
            Assert.AreEqual(null, GetProperty<object>(stateType, state, "CachedMuteStateAll"));

            var cancelled = new List<string>();
            SetProperty(callbacksType, callbacks, "CancelScheduleTask",
                new Action<string, bool>((key, update) => cancelled.Add(key + ":" + update)));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "DeleteSandboxReferencesByDisplay", state, new[] { "用户B" }));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "DeleteScheduleJobReferencesByDisplay", state, new[] { "用户B" }, callbacks));
            CollectionAssert.AreEqual(new[] { "close:False" }, cancelled.ToArray());
            IDictionary remainingJobs = (IDictionary)GetProperty<object>(stateType, state, "ScheduleJobs");
            object launchJob = remainingJobs["launch"];
            CollectionAssert.AreEqual(new[] { "新用户A" },
                ToStringArray(GetProperty<object>(launchJob.GetType(), launchJob, "SandboxTargets")));
            Assert.AreEqual("定时启动 1 个沙盒", GetProperty<string>(launchJob.GetType(), launchJob, "Description"));
        }

        [TestMethod]
        public void RenamesDirectoriesWithRollbackAndBuildsRegistryCleanupPlan()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "OldA"));
                Directory.CreateDirectory(Path.Combine(root, "OldB"));
                Type serviceType = RequireType(ServiceTypeName);
                object service = Activator.CreateInstance(serviceType);
                object renamed = ReflectionTestHelpers.InvokeInstance(
                    service, "RenameSandboxDirectories", root,
                    new Dictionary<string, string> { { "OldA", "NewA" }, { "OldB", "NewB" } });
                Assert.AreEqual(true, GetTupleItem<bool>(renamed, "Item1"));
                Assert.IsTrue(Directory.Exists(Path.Combine(root, "NewA")));
                Assert.IsTrue(Directory.Exists(Path.Combine(root, "NewB")));

                object failed = ReflectionTestHelpers.InvokeInstance(
                    service, "RenameSandboxDirectories", root,
                    new Dictionary<string, string> { { "NewA", "FinalA" }, { "Missing", "X" } });
                Assert.AreEqual(false, GetTupleItem<bool>(failed, "Item1"));
                StringAssert.Contains(GetTupleItem<string>(failed, "Item2"), "未找到沙盒目录: Missing");
                Assert.IsTrue(Directory.Exists(Path.Combine(root, "NewA")));
                Assert.IsFalse(Directory.Exists(Path.Combine(root, "FinalA")));
                Assert.AreEqual(true,
                    ReflectionTestHelpers.InvokeInstance(service, "SafeDeleteDirectory", Path.Combine(root, "NewB")));

                string[] registryPaths = ToStringArray(ReflectionTestHelpers.InvokeInstance(
                    service, "BuildSandboxRegistryPaths", (object)new[] { "BoxA" }));
                Assert.AreEqual(9, registryPaths.Length);
                CollectionAssert.Contains(registryPaths, @"HKCU\Software\Sandboxie\Boxes\BoxA");
                CollectionAssert.Contains(registryPaths, @"HKLM\SOFTWARE\WOW6432Node\SandboxiePlus\Boxes\BoxA");
                object cleanup = ReflectionTestHelpers.InvokeInstance(
                    service, "CleanupSandboxRegistryEntries", (object)new[] { "BoxA" },
                    new Func<string, bool>(path => !path.Contains("WOW6432Node\\SandboxiePlus")));
                Assert.AreEqual(8, GetProperty<int>(cleanup.GetType(), cleanup, "Cleaned"));
                CollectionAssert.AreEqual(
                    new[] { @"HKLM\SOFTWARE\WOW6432Node\SandboxiePlus\Boxes\BoxA" },
                    ToStringArray(GetProperty<object>(cleanup.GetType(), cleanup, "FailedPaths")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void AppliesAliasesAndStopsSandboxBeforeDeleteThroughCallbacks()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object service = Activator.CreateInstance(serviceType);
            object callbacks = Activator.CreateInstance(callbacksType);
            var calls = new List<string>();
            bool forced = false;
            SetProperty(callbacksType, callbacks, "GetSandboxPids",
                new Func<string, IEnumerable<int>>(name => new[] { 12, 12, 5 }));
            SetProperty(callbacksType, callbacks, "TerminateSandbox",
                new Func<string, bool>(name => { calls.Add("terminate:" + name); return true; }));
            SetProperty(callbacksType, callbacks, "GetAlivePids",
                new Func<IEnumerable<int>, IList<int>>(values => forced
                    ? new List<int>()
                    : values.Distinct().OrderBy(value => value).ToList()));
            SetProperty(callbacksType, callbacks, "TerminatePids",
                new Action<IEnumerable<int>>(values => { calls.Add("kill:" + string.Join(",", values)); forced = true; }));

            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "EnsureSandboxStoppedBeforeDelete", "BoxA", 0.01, callbacks));
            CollectionAssert.AreEqual(new[] { "terminate:BoxA", "kill:5,12" }, calls.ToArray());

            object aliasResult = ReflectionTestHelpers.InvokeInstance(
                service, "ApplySandboxAliases",
                new Dictionary<string, string> { { "BoxA", "工作盒" }, { "BoxB", "" } },
                new Func<string, string, bool>((name, alias) => name == "BoxA"));
            Assert.AreEqual(true, GetProperty<bool>(aliasResult.GetType(), aliasResult, "Changed"));
            Assert.AreEqual("以下沙盒别名更新失败：BoxB",
                GetProperty<string>(aliasResult.GetType(), aliasResult, "Error"));
        }

        private static IDictionary CreateScheduleJobs(
            Type scheduleJobType,
            params Tuple<string, string, string[]>[] values)
        {
            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), scheduleJobType);
            IDictionary result = (IDictionary)Activator.CreateInstance(dictionaryType);
            foreach (Tuple<string, string, string[]> value in values)
            {
                object job = Activator.CreateInstance(scheduleJobType);
                SetProperty(scheduleJobType, job, "Description", value.Item2);
                SetProperty(scheduleJobType, job, "SandboxTargets", new List<string>(value.Item3));
                result[value.Item1] = job;
            }
            return result;
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-sandbox-admin-" + Guid.NewGuid().ToString("N"));
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

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }

        private static T GetTupleItem<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must exist");
            return (T)property.GetValue(target);
        }

        private static string[] ToStringArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(item => Convert.ToString(item)).ToArray();
        }
    }
}
