using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.App.UI.Dialogs.Settings;
using ShaheLauncher.Core.Processes;
using ShaheLauncher.Core.Settings;
using ShaheLauncher.Core.Speech;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Speech")]
    [DoNotParallelize]
    public sealed class AssistSpeechMenuRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow("menu", true)]
        [DataRow("hotkey", true)]
        [DataRow("menu", false)]
        [DataRow("hotkey", false)]
        public void VolumeMenuAndHotkeyReachRealOperationAndEnabledAssistBackend(string trigger, bool enabled)
        {
            WithSpeechForm(enabled, (form, speech) =>
            {
                var session = new ProcessAudioSession { Pid = 101, HasVolume = true, MasterVolume = 0.50 };
                form.ProcessCallbacks.GetAudioSessions = () => new[] { session };

                Trigger(form, trigger, "提高当前打开的进程音量（+5%）", Keys.Control | Keys.Up);

                WaitFor(() => Math.Abs(session.MasterVolume - 0.55) < 0.0001,
                    "菜单/快捷键必须经过真实 ProcessActionService 修改音量");
                if (enabled)
                {
                    WaitFor(() => speech.Messages.Length != 0, "操作完成后应进入辅助朗读后端");
                    string result = speech.Messages.Single();
                    StringAssert.Contains(result, "用户A");
                    StringAssert.Contains(result, "55%");
                    Assert.IsTrue(result.Length <= 30, result);
                }
                else
                {
                    AssertSilent(speech);
                }
            });
        }

        [DataTestMethod]
        [DataRow("menu", true)]
        [DataRow("hotkey", true)]
        [DataRow("menu", false)]
        [DataRow("hotkey", false)]
        public void PinAndUnpinSpeakOnlyTheSavedObjectResult(string trigger, bool enabled)
        {
            WithSpeechForm(enabled, (form, speech) =>
            {
                Trigger(form, trigger, "置顶当前用户", Keys.Control | Keys.Shift | Keys.P);
                Assert.IsTrue(form.SandboxAdminService.IsSandboxPinned(form.SandboxAdminState, "用户A"));
                CollectionAssert.Contains(JArray.FromObject(form.SettingsService.LoadSettings()["pinned_sandboxes"])
                    .Values<string>().ToArray(), "用户A", "置顶实际写入隔离配置");
                if (enabled)
                {
                    WaitFor(() => speech.Messages.Length == 1, "置顶保存成功后应附加朗读");
                    CollectionAssert.AreEqual(new[] { "已置顶用户A" }, speech.Messages);
                }
                else AssertSilent(speech);

                SelectFirstUser(form);
                Trigger(form, trigger, "取消置顶当前用户", Keys.Control | Keys.Shift | Keys.P);
                Assert.IsFalse(form.SandboxAdminService.IsSandboxPinned(form.SandboxAdminState, "用户A"));
                if (enabled)
                {
                    WaitFor(() => speech.Messages.Length == 2, "取消置顶保存成功后应附加朗读");
                    CollectionAssert.AreEqual(new[] { "已置顶用户A", "已取消置顶用户A" }, speech.Messages);
                }
                else AssertSilent(speech);
            });
        }

        [DataTestMethod]
        [DataRow("menu")]
        [DataRow("hotkey")]
        public void PinSaveFailureDoesNotSpeakSuccess(string trigger)
        {
            WithSpeechForm(true, (form, speech) =>
            {
                int rejectedSaves = 0;
                form.SandboxAdminCallbacks.SaveSettings = () =>
                {
                    rejectedSaves++;
                    throw new SettingsSaveException();
                };
                Action<string, string, MessageBoxIcon> original = DialogMessageService.Handler;
                DialogMessageService.Handler = (text, title, icon) => { };
                try
                {
                    Trigger(form, trigger, "置顶当前用户", Keys.Control | Keys.Shift | Keys.P);
                    Assert.AreEqual(1, rejectedSaves);
                    PumpFor(200);
                    Assert.IsFalse(speech.Messages.Any(text => text.Contains("已置顶") || text.Contains("已取消置顶")),
                        "保存失败不能播报置顶成功");
                }
                finally { DialogMessageService.Handler = original; }
            });
        }

        [DataTestMethod]
        [DataRow("menu")]
        [DataRow("hotkey")]
        public void SelectAllAndNoneChangeChecksWithoutSupplementarySpeech(string trigger)
        {
            WithSpeechForm(true, (form, speech) =>
            {
                Trigger(form, trigger, "全选", Keys.Control | Keys.A);
                Assert.AreEqual(2, form.SandboxList.GetCheckedCount());
                Trigger(form, trigger, "全都不选", Keys.Control | Keys.A);
                Assert.AreEqual(0, form.SandboxList.GetCheckedCount());
                AssertSilent(speech);
            });
        }

        [DataTestMethod]
        [DataRow("menu", true)]
        [DataRow("hotkey", true)]
        [DataRow("list-key", true)]
        [DataRow("menu", false)]
        [DataRow("hotkey", false)]
        public void ManualUserRefreshSpeaksOnceAfterListChanges(string trigger, bool enabled)
        {
            WithSpeechForm(enabled, (form, speech) =>
            {
                Directory.CreateDirectory(Path.Combine(form.SandboxList.GetSandboxRoot(), "用户C"));
                if (trigger == "list-key")
                {
                    MigrationRuntimeTestHelpers.Invoke(form.SandboxList, "OnKeyDown", new KeyEventArgs(Keys.F5));
                }
                else Trigger(form, trigger, "刷新沙盒用户", Keys.F5);

                Assert.AreEqual(3, form.SandboxList.Items.Count, "应执行真实列表扫描与更新");
                if (enabled)
                {
                    WaitFor(() => speech.Messages.Length != 0, "手动刷新完成应有简短结果朗读");
                    string result = speech.Messages.Single();
                    StringAssert.Contains(result, "已刷新");
                    StringAssert.Contains(result, "用户");
                    Assert.IsTrue(result.Length <= 25, result);
                }
                else AssertSilent(speech);
            });
        }

        [DataTestMethod]
        [DataRow("menu", false)]
        [DataRow("hotkey", false)]
        [DataRow("menu", true)]
        [DataRow("hotkey", true)]
        public void ManualProcessRefreshWaitsForQueryAndDistinguishesFailure(string trigger, bool fail)
        {
            WithSpeechForm(true, (form, speech) =>
            {
                using (var entered = new ManualResetEventSlim(false))
                using (var release = new ManualResetEventSlim(false))
                {
                    form.SandboxList.RunningStatesProvider = names =>
                    {
                        entered.Set();
                        if (!release.Wait(4000)) throw new TimeoutException("test query gate");
                        if (fail) throw new IOException("isolated query failure");
                        return names.ToDictionary(name => name, name => false);
                    };
                    try
                    {
                        Trigger(form, trigger, "刷新进程状态", Keys.Control | Keys.F5);
                        WaitFor(() => entered.IsSet, "必须进入实际异步运行状态查询");
                        Assert.AreEqual(0, speech.Messages.Length, "查询未完成时不能提前播报成功");
                    }
                    finally { release.Set(); }

                    string expected = fail ? "刷新进程状态失败" : "已刷新进程状态";
                    WaitFor(() => speech.Messages.Length != 0, "后台完成结果应进入辅助朗读接口");
                    CollectionAssert.AreEqual(new[] { expected }, speech.Messages);
                    if (fail)
                    {
                        Assert.AreNotEqual("已刷新进程状态", form.StatusLabel.Text,
                            "查询异常不能作为成功完成显示");
                    }
                    else
                    {
                        Assert.IsTrue(form.SandboxList.Items[0].Text.Contains("空闲中"));
                    }
                }
            });
        }

        [DataTestMethod]
        [DataRow("menu")]
        [DataRow("hotkey")]
        public void DisabledAssistKeepsManualProcessRefreshSilent(string trigger)
        {
            WithSpeechForm(false, (form, speech) =>
            {
                form.SandboxList.RunningStatesProvider = names => names.ToDictionary(name => name, name => false);
                Trigger(form, trigger, "刷新进程状态", Keys.Control | Keys.F5);
                WaitFor(() => form.SandboxList.Items[0].Text.Contains("空闲中"), "运行状态应完成更新");
                AssertSilent(speech);
            });
        }

        [TestMethod]
        public void StartupAndAutomaticRefreshAndStatusChangesStaySilent()
        {
            WithSpeechForm(true, (form, speech) =>
            {
                form.RefreshSandboxListOnStartup();
                form.SandboxList.FastRefreshSandboxList();
                form.SandboxList.RunningStatesProvider = names => names.ToDictionary(name => name, name => true);
                Assert.IsTrue(form.SandboxList.RefreshVisibleRunningStates());
                WaitFor(() => form.SandboxList.Items[0].Text.Contains("运行中"), "自动刷新应正常完成");
                form.StatusLabel.SetLabel("正在准备启动沙盒...");
                form.StatusLabel.SetLabel("启动扫描进度 1/2");
                AssertSilent(speech);
            });
        }

        [TestMethod]
        public void ExistingSpeechSettingsCheckboxPersistsAndChangesMenuAndHotkeySpeechImmediately()
        {
            WithSpeechForm(true, (form, speech) =>
            {
                var session = new ProcessAudioSession { Pid = 101, HasVolume = true, MasterVolume = 0.50 };
                form.ProcessCallbacks.GetAudioSessions = () => new[] { session };
                SaveCheckboxThroughDialog(form, false);
                Trigger(form, "hotkey", "提高当前打开的进程音量（+5%）", Keys.Control | Keys.Up);
                WaitFor(() => Math.Abs(session.MasterVolume - 0.55) < 0.0001, "关闭朗读不应关闭实际操作");
                AssertSilent(speech);

                SaveCheckboxThroughDialog(form, true);
                Trigger(form, "menu", "提高当前打开的进程音量（+5%）", Keys.Control | Keys.Up);
                WaitFor(() => Math.Abs(session.MasterVolume - 0.60) < 0.0001, "开启朗读后仍应执行真实操作");
                WaitFor(() => speech.Messages.Length == 1, "无需重启或重载设置即可恢复辅助朗读");
                StringAssert.Contains(speech.Messages.Single(), "用户A");
                StringAssert.Contains(speech.Messages.Single(), "60%");
            });
        }

        [TestMethod]
        public void CurrentUserHotkeyWithoutSelectionSpeaksBriefMissingTargetInsteadOfSuccess()
        {
            WithSpeechForm(true, (form, speech) =>
            {
                form.SandboxList.Items.Clear();
                int queries = 0;
                form.ProcessCallbacks.GetSandboxPids = name => { queries++; return new[] { 101 }; };
                Trigger(form, "hotkey", string.Empty, Keys.Control | Keys.Up);
                Assert.AreEqual(0, queries, "无选中用户时不能猜测目标进程");
                WaitFor(() => speech.Messages.Length == 1, "快捷键无目标也应提供简短辅助反馈");
                string result = speech.Messages.Single();
                StringAssert.Contains(result, "用户");
                Assert.IsTrue(result.Contains("选择") || result.Contains("选中"), result);
                Assert.IsTrue(result.Length <= 25, result);
                Assert.IsFalse(result.Contains("音量调到"), result);
            });
        }

        private static void SaveCheckboxThroughDialog(MainForm form, bool enabled)
        {
            bool opened = false;
            form.SpeechService.Callbacks.ShowSettings = request =>
            {
                opened = true;
                using (var dialog = new SpeechSettingsDialog(form, request.SpeechConfig, request.SaveSpeechConfig,
                    request.ReaderIniSettings, request.SaveReaderIniSettings, null, request.InterfaceDllChoices,
                    request.SelectedInterfaceDll, request.SaveInterfaceDll, request.AssistHints, request.SaveAssistHints))
                {
                    Assert.AreEqual(!enabled, dialog.AssistEnabledCheckBox.Checked, "对话框应显示当前运行开关状态");
                    dialog.AssistEnabledCheckBox.Checked = enabled;
                    Assert.IsTrue(dialog.ConfirmSettings());
                }
                return true;
            };
            Assert.IsTrue(form.SpeechService.OnSpeechSettings());
            Assert.IsTrue(opened);
            Assert.AreEqual(enabled, (bool)form.SpeechService.SpeechAssistHints["enabled"]);
            Assert.AreEqual(enabled,
                JObject.FromObject(form.SettingsService.LoadSettings()["speech_assist_hints"])["enabled"].Value<bool>(),
                "必须由既有对话框保存回调持久化 speech_assist_hints");
        }

        private static void WithSpeechForm(bool enabled, Action<MainForm, RecordingSpeech> test)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                typeof(MainForm).GetField("_hotkeysEnabled", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(form, true);
                typeof(MainForm).GetField("_hotkeys", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(form, new Dictionary<string, string>
                    {
                        { "volume_current_up", "Ctrl+Up" },
                        { "refresh_process_state", "Ctrl+F5" },
                        { "pin_current_user", "Ctrl+Shift+P" }
                    });
                // Hidden native handles and the normal UI message queue; no desktop input or Sandboxie calls.
                Assert.AreNotEqual(IntPtr.Zero, form.Handle);
                Assert.AreNotEqual(IntPtr.Zero, form.SandboxList.Handle);
                string sandboxRoot = Path.Combine(root, "sandboxes");
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "用户A"));
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "用户B"));
                form.SandboxList.SetSandboxRoot(sandboxRoot);
                form.SandboxList.SandboxDisplayNameScanner = null;
                form.SandboxList.DisplayNameResolver = name => name;
                form.SandboxList.RunningStatesProvider = null;
                form.SandboxList.PinnedSandboxesProvider = () => form.SandboxAdminState.PinnedSandboxes;
                form.SandboxList.PinStateProvider = name => form.SandboxAdminService.IsSandboxPinned(form.SandboxAdminState, name);
                form.RefreshSandboxListCallback = form.SandboxList.RefreshSandboxList;
                form.SandboxAdminCallbacks.RefreshSandboxList = form.SandboxList.RefreshSandboxList;
                form.ProcessCallbacks.ResolveSandboxNames = name => Tuple.Create(name, name);
                form.ProcessCallbacks.ResolveVolumeMemoryKey = name => name;
                form.ProcessCallbacks.GetSandboxPids = name => new[] { name == "用户A" ? 101 : 102 };
                form.ProcessCallbacks.GetAllSandboxPids = () => new[] { 101, 102 };
                form.ProcessCallbacks.ScanSandboxNames = () => new[] { "用户A", "用户B" };
                form.ProcessCallbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                form.ProcessCallbacks.ListOutputDevices = () => new ProcessAudioDevice[0];
                form.SandboxList.ReplaceItemTexts(new[] { "用户A 【运行中】", "用户B 【运行中】" }, new int[0]);
                SelectFirstUser(form);
                using (var speech = new RecordingSpeech(root, form))
                {
                    form.SpeechService.SaveSpeechAssistHints(new Dictionary<string, object> { { "enabled", enabled } });
                    test(form, speech);
                }
            });
        }

        private static void SelectFirstUser(MainForm form)
        {
            foreach (ListViewItem item in form.SandboxList.Items) item.Selected = false;
            ListViewItem user = form.SandboxList.Items.Cast<ListViewItem>()
                .First(item => item.Text.StartsWith("用户A", StringComparison.Ordinal));
            user.Selected = true;
            user.Focused = true;
        }

        private static void Trigger(MainForm form, string trigger, string menuText, Keys key)
        {
            if (trigger == "menu")
            {
                using (ContextMenu menu = form.SandboxList.BuildContextMenu(form.SandboxList.SelectedIndices[0]))
                {
                    MenuItem item = menu.MenuItems.Cast<MenuItem>().Single(entry => entry.Text == menuText);
                    Assert.IsTrue(item.Enabled, menuText);
                    item.PerformClick();
                }
                return;
            }

            // Exercise normal WinForms dispatch, preserving the real action and completion callbacks.
            MethodInfo dispatch = typeof(MainForm).GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] arguments = { new Message(), key };
            Assert.IsTrue((bool)dispatch.Invoke(form, arguments), "快捷键应已分派: " + key);
        }

        private static void WaitFor(Func<bool> condition, string message)
        {
            var timer = Stopwatch.StartNew();
            while (!condition() && timer.ElapsedMilliseconds < 3000)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
            Application.DoEvents();
            Assert.IsTrue(condition(), message);
        }

        private static void PumpFor(int milliseconds)
        {
            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
        }

        private static void AssertSilent(RecordingSpeech speech)
        {
            PumpFor(200);
            CollectionAssert.AreEqual(new string[0], speech.Messages, "不应新增辅助朗读");
        }

        private sealed class RecordingSpeech : IDisposable
        {
            private readonly MainForm _form;
            private readonly string _originalPath;
            private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();

            internal RecordingSpeech(string root, MainForm form)
            {
                _form = form;
                _originalPath = Environment.GetEnvironmentVariable("PATH");
                string lib = Path.Combine(root, "lib");
                Directory.CreateDirectory(lib);
                File.WriteAllBytes(Path.Combine(lib, "Tolk.dll"), new byte[] { 0 });
                form.SpeechService.Callbacks.EnumerateProcessNames = () => new string[0];
                form.SpeechService.Callbacks.LoadAssistBackend = path => new SpeechAssistBackend
                {
                    Load = () => { },
                    IsLoaded = () => true,
                    Silence = () => { },
                    Output = (text, interrupt) => _messages.Enqueue(text),
                    Unload = () => { }
                };
                Assert.IsTrue(form.SpeechService.EnsureAssistSpeechRuntime(), "隔离辅助接口必须实际加载成功");
            }

            internal string[] Messages => _messages.ToArray();

            public void Dispose()
            {
                _form.SpeechService.ShutdownAssistSpeechRuntime();
                Environment.SetEnvironmentVariable("PATH", _originalPath);
            }
        }
    }
}
