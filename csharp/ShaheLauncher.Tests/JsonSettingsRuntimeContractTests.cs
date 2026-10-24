using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaheLauncher.App;
using ShaheLauncher.Core.AppPaths;
using ShaheLauncher.Core.Runtime;
using ShaheLauncher.Core.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class JsonSettingsRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow("history", false)]
        [DataRow("groups", false)]
        [DataRow("volume", false)]
        [DataRow("hotkeys", false)]
        [DataRow("pinned", false)]
        [DataRow("speech", false)]
        [DataRow("history", true)]
        [DataRow("groups", true)]
        [DataRow("volume", true)]
        [DataRow("hotkeys", true)]
        [DataRow("pinned", true)]
        [DataRow("speech", true)]
        public void ReloadsPythonJsonFieldsIntoRuntime(string field, bool legacy)
        {
            WithForm((root, form) =>
            {
                IDictionary<string, object> settings = SeedSettings(root);
                if (legacy)
                {
                    File.WriteAllText(form.LegacyConfigPath, JsonConvert.SerializeObject(settings));
                }
                else
                {
                    Assert.IsTrue(form.ConfigStore.SetJson("settings_json", settings));
                }

                Invoke(form, "LoadApplicationSettings");
                LauncherSettingsState state = GetState(form);
                switch (field)
                {
                    case "history":
                        AssertHistory(root, state);
                        break;
                    case "groups":
                        Assert.IsTrue(state.SandboxGroups.ContainsKey("常用"), "分组必须从持久化 JSON 恢复");
                        CollectionAssert.AreEqual(new[] { "用户A", "用户B" },
                            ((IEnumerable)state.SandboxGroups["常用"]).Cast<object>().Select(Convert.ToString).ToArray());
                        CollectionAssert.AreEqual(new[] { "用户A", "用户B" },
                            form.SandboxAdminState.SandboxGroups["常用"].ToArray());
                        break;
                    case "volume":
                        Assert.IsTrue(state.SandboxVolumeMemory.ContainsKey("用户A"), "音量记忆必须从持久化 JSON 恢复");
                        Assert.AreEqual(35, state.SandboxVolumeMemory["用户A"]);
                        Assert.AreEqual(35, form.ProcessState.SandboxVolumeMemory["用户A"]);
                        break;
                    case "hotkeys":
                        Assert.IsTrue(state.Hotkeys.ContainsKey("launch_selected"), "自定义快捷键必须从持久化 JSON 恢复");
                        Assert.AreEqual("Ctrl+Shift+F8", state.Hotkeys["launch_selected"]);
                        Assert.IsTrue(state.HotkeysEnabled);
                        break;
                    case "pinned":
                        CollectionAssert.AreEqual(new[] { "用户B" }, state.PinnedSandboxes.ToArray());
                        break;
                    case "speech":
                        Assert.AreEqual(false, state.SpeechAssistHints["enabled"]);
                        Assert.AreEqual("自定义静音提示", state.SpeechAssistHints["mute_current"]);
                        Assert.AreEqual("静音全部打开的进程", state.SpeechAssistHints["mute_all"]);
                        break;
                }
            });
        }

        [TestMethod]
        public void SavingTypedHistoryKeepsPythonKeysAndSurvivesNextLoad()
        {
            WithForm((root, form) =>
            {
                var state = new LauncherSettingsState
                {
                    AppHistory = new List<object>
                    {
                        new AppHistoryItem { Path = Path.Combine(root, "应用.exe"), Note = "工作应用" }
                    }
                };
                form.SettingsService.SaveSettings(state);
                var saved = (IDictionary<string, object>)form.ConfigStore.GetJson("settings_json", null);
                var entry = JArray.FromObject(saved["app_history"])[0];
                Assert.AreEqual(Path.Combine(root, "应用.exe"), (string)entry["path"], "保存键必须与 Python 的 path 一致");
                Assert.AreEqual("工作应用", (string)entry["note"]);
                Assert.IsNull(entry["Path"]);
                Assert.IsNull(entry["Note"]);
                Invoke(form, "LoadApplicationSettings");
                AssertHistory(root, GetState(form));
            });
        }

        [TestMethod]
        public void LoadingAndSavingAgainPreservesNestedSettingsAcrossInstances()
        {
            WithForm((root, first) =>
            {
                Assert.IsTrue(first.ConfigStore.SetJson("settings_json", SeedSettings(root)));
                Invoke(first, "LoadApplicationSettings");
                Invoke(first, "SaveSettings");
                using (MainForm second = CreateForm(root))
                {
                    Invoke(second, "LoadApplicationSettings");
                    LauncherSettingsState state = GetState(second);
                    AssertHistory(root, state);
                    CollectionAssert.AreEqual(new[] { "用户A", "用户B" },
                        second.SandboxAdminState.SandboxGroups["常用"].ToArray());
                    Assert.AreEqual(35, second.ProcessState.SandboxVolumeMemory["用户A"]);
                    Assert.AreEqual("Ctrl+Shift+F8", state.Hotkeys["launch_selected"]);
                    Assert.AreEqual("自定义静音提示", state.SpeechAssistHints["mute_current"]);
                }
            });
        }

        private static IDictionary<string, object> SeedSettings(string root)
        {
            return new Dictionary<string, object>
            {
                { "app_history", new[] { new Dictionary<string, object>
                    { { "path", Path.Combine(root, "应用.exe") }, { "note", "工作应用" } } } },
                { "sandbox_groups", new Dictionary<string, object> { { "常用", new[] { "用户A", "用户B" } } } },
                { "pinned_sandboxes", new[] { "用户B" } },
                { "sandbox_volume_memory", new Dictionary<string, object> { { "用户A", 35 } } },
                { "hotkeys_enabled", true },
                { "hotkeys", new Dictionary<string, object> { { "launch_selected", "Ctrl+Shift+F8" } } },
                { "speech_assist_hints", new Dictionary<string, object>
                    { { "enabled", false }, { "mute_current", "自定义静音提示" } } }
            };
        }

        private static void AssertHistory(string root, LauncherSettingsState state)
        {
            Assert.AreEqual(1, state.AppHistory.Count, "路径历史必须从持久化 JSON 恢复");
            AppHistoryItem item = (AppHistoryItem)state.AppHistory[0];
            Assert.AreEqual(Path.Combine(root, "应用.exe"), item.Path);
            Assert.AreEqual("工作应用", item.Note);
        }

        private static LauncherSettingsState GetState(MainForm form)
        {
            return (LauncherSettingsState)typeof(MainForm).GetField(
                "_settingsState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
        }

        private static void Invoke(MainForm form, string method)
        {
            typeof(MainForm).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
        }

        private static MainForm CreateForm(string root)
        {
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            return new MainForm(new LauncherRuntime(false, executable, executable, () => DateTime.Now));
        }

        private static void WithForm(Action<string, MainForm> action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                string root = Path.Combine(Path.GetTempPath(), "shahe_json_settings_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                try
                {
                    using (MainForm form = CreateForm(root)) action(root, form);
                }
                catch (Exception caught) { error = caught; }
                finally { Directory.Delete(root, true); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(10000), "隔离 JSON 配置测试超时");
            if (error != null) throw new AssertFailedException("STA action failed: " + error, error);
        }
    }
}
