using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Controls;
using ShaheLauncher.Core.Runtime;
using ShaheLauncher.Core.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class SettingsRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SoftwareSettingsApplyNewValuesToRuntimeAndAutoRunWriter(bool enabled)
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.ScheduleProcessMenuLiveStateRefreshCallback = null;
                    form.SpeechCallback = (text, key) => { };
                    form.SandboxList.SandboxDisplayNameScanner = () => new[] { "用户A", "用户B", "用户C" };
                    form.SandboxList.GroupNamesProvider = () => new string[0];
                    form.SandboxList.GroupedMembersProvider = () => new string[0];
                    form.SandboxList.PinnedSandboxesProvider = () => new string[0];
                    form.SandboxList.RunningStatesProvider = null;
                    LauncherSettingsState state = GetSettingsState(form);
                    state.ShowTray = false;
                    form.ShowTray = false;
                    state.AutoRun = !enabled;
                    form.AutoRun = !enabled;
                    var autoRunWrites = new List<bool>();
                    form.AutoRunWriter = (key, path, value) => autoRunWrites.Add(value);
                    MethodInfo apply = typeof(MainForm).GetMethod("ApplySettingsFromDialog", BindingFlags.Instance | BindingFlags.NonPublic);
                    using (var dialog = new ShaheLauncher.App.UI.Dialogs.Settings.SettingsDialog(
                        form, form.SettingsService.CreateDefaultSettings(),
                        values => apply.Invoke(form, new object[] { values }), form.SettingsService.CreateDefaultSettings()))
                    {
                        dialog.LaunchIntervalInput.Value = enabled ? 7 : 0;
                        dialog.LaunchConcurrencyInput.SelectedItem = 1;
                        dialog.AutoRunCheckBox.Checked = enabled;
                        dialog.ShowTrayCheckBox.Checked = false;
                        dialog.AutoSelectRangeCheckBox.Checked = enabled;
                        dialog.SkipRunningCheckBox.Checked = enabled;
                        dialog.RememberVolumeCheckBox.Checked = enabled;
                        dialog.ListNavigationCheckBox.Checked = enabled;
                        dialog.LogOutputChoice.SelectedIndex = enabled ? 1 : 0;
                        Assert.IsTrue(dialog.SaveSettings());
                    }
                    Assert.AreEqual(enabled ? 7 : 0, form.LaunchState.LaunchInterval);
                    Assert.AreEqual(1, form.LaunchState.LaunchConcurrencyLimit);
                    Assert.AreEqual(enabled, form.LaunchState.SkipRunningSandboxOnLaunch);
                    Assert.AreEqual(enabled, form.LaunchState.RememberVolume);
                    Assert.AreEqual(enabled, form.ProcessState.RememberVolume);
                    Assert.AreEqual(enabled, GetPrivateBool(form.SandboxList, "_rangeAutoCheckEnabled"));
                    Assert.AreEqual(enabled, GetPrivateBool(form.SandboxList, "_leftRightNavigationEnabled"));
                    form.SandboxList.CheckItem(0, true);
                    form.SandboxList.CheckItem(2, true);
                    Assert.AreEqual(enabled, form.SandboxList.Items[1].Checked);
                    int logged = 0;
                    form.LogOperationCallback = message => logged++;
                    form.LogOperation("settings probe");
                    Assert.AreEqual(enabled ? 1 : 0, logged);
                    Assert.AreEqual(enabled, form.SettingsService.LoadSettings()["auto_run"]);
                    Assert.AreEqual(enabled, form.AutoRun);
                    CollectionAssert.AreEqual(new[] { enabled }, autoRunWrites.ToArray(),
                        "The current checkbox value must reach the auto-run writer during this same save.");
                }
                finally
                {
                    Destroy(form);
                    Directory.Delete(root, true);
                }
            });
        }

        [TestMethod]
        public void ResetSettingsRestoresSwitchesWithoutSynchronousProcessScan()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.ScheduleProcessMenuLiveStateRefreshCallback = null;

                    var autoRunWrites = new List<bool>();
                    form.AutoRunWriter = (key, path, enabled) => autoRunWrites.Add(enabled);
                    form.ConfirmMessageCallback = (text, title, buttons, icon) =>
                    {
                        Assert.AreEqual(LauncherSettingsService.ResetConfirmationText, text);
                        Assert.AreEqual(LauncherSettingsService.ResetConfirmationTitle, title);
                        Assert.AreEqual(MessageBoxButtons.YesNo, buttons);
                        Assert.AreEqual(MessageBoxIcon.Warning, icon);
                        return DialogResult.Yes;
                    };

                    foreach (string action in new[]
                    {
                        "toggle_auto_run",
                        "toggle_show_tray",
                        "toggle_auto_select_range",
                        "toggle_skip_running_sandbox_on_launch",
                        "toggle_remember_volume",
                        "toggle_list_left_right_nav"
                    })
                    {
                        ExecuteAction(form, action);
                    }

                    LauncherSettingsState state = GetSettingsState(form);
                    Assert.IsTrue(form.AutoRun);
                    Assert.IsFalse(form.ShowTray);
                    Assert.IsTrue(state.AutoSelectRangeOnCheck);
                    Assert.IsTrue(state.SkipRunningSandboxOnLaunch);
                    Assert.IsFalse(state.RememberVolume);
                    Assert.IsFalse(state.ListUseLeftRightNavigation);

                    form.AppPathList.SetValue(Path.Combine(root, "old.exe"));
                    form.SandboxList.SandboxDisplayNameScanner = () => new[] { "用户A" };
                    form.SandboxList.GroupNamesProvider = () => new[] { "常用" };
                    form.SandboxList.GroupedMembersProvider = () => new string[0];
                    form.SandboxList.PinnedSandboxesProvider = () => new string[0];
                    form.SandboxList.RunningStatesProvider = null;
                    form.SandboxList.ReplaceItemTexts(
                        new[] { CheckableListCtrl.FormatGroupItemText("常用") },
                        new[] { 0 });
                    int menuProcessScans = 0;
                    form.ProcessCallbacks.GetAllSandboxPids = () =>
                    {
                        menuProcessScans++;
                        return new int[0];
                    };

                    ExecuteAction(form, "reset_settings");

                    Assert.AreEqual(0, menuProcessScans,
                        "重置设置后的菜单重建不能在 UI 线程同步扫描全部沙盒 PID");
                    CollectionAssert.AreEqual(new[] { true, false }, autoRunWrites.ToArray());
                    Assert.IsFalse(form.AutoRun);
                    Assert.IsTrue(form.ShowTray);
                    Assert.IsFalse(state.AutoSelectRangeOnCheck);
                    Assert.IsFalse(state.SkipRunningSandboxOnLaunch);
                    Assert.IsTrue(state.RememberVolume);
                    Assert.IsTrue(state.ListUseLeftRightNavigation);
                    Assert.IsFalse(form.LaunchState.SkipRunningSandboxOnLaunch);
                    Assert.IsTrue(form.LaunchState.RememberVolume);
                    Assert.IsTrue(form.ProcessState.RememberVolume);
                    Assert.IsFalse(GetPrivateBool(form.SandboxList, "_rangeAutoCheckEnabled"));
                    Assert.IsTrue(GetPrivateBool(form.SandboxList, "_leftRightNavigationEnabled"));
                    Assert.AreEqual(string.Empty, form.AppPathList.GetValue());
                    Assert.AreEqual(false, form.Settings["auto_run"]);
                    Assert.AreEqual(true, form.Settings["show_tray"]);
                    Assert.AreEqual(false, form.Settings["auto_select_range_on_check"]);
                    Assert.AreEqual(false, form.Settings["skip_running_sandbox_on_launch"]);
                    Assert.AreEqual(true, form.Settings["remember_volume"]);
                    Assert.AreEqual(true, form.Settings["list_use_left_right_nav"]);
                    Assert.AreEqual(
                        LauncherSettingsService.ResetCompletedStatus,
                        form.StatusLabel.Text);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void ResetSettingsSynchronizesGroupsBeforeRefreshingSandboxList()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.AutoRunWriter = (key, path, enabled) => { };
                    form.ScheduleProcessMenuLiveStateRefreshCallback = null;
                    form.ConfirmMessageCallback = (text, title, buttons, icon) => DialogResult.Yes;
                    form.ProcessCallbacks.GetAllSandboxPids = () => new int[0];
                    form.SandboxList.SandboxDisplayNameScanner = () => new[] { "用户A" };
                    form.SandboxList.GroupNamesProvider = () => form.SandboxAdminState.SandboxGroups.Keys;
                    form.SandboxList.GroupedMembersProvider = () => new string[0];
                    form.SandboxList.PinnedSandboxesProvider = () => new string[0];
                    form.SandboxList.RunningStatesProvider = null;
                    form.SandboxAdminState.SandboxGroups["常用"] = new List<string> { "用户A" };
                    form.SandboxList.RefreshSandboxList();
                    Assert.AreEqual(2, form.SandboxList.Items.Count);
                    Assert.IsTrue(form.SandboxList.IsGroupIndex(0));

                    ExecuteAction(form, "reset_settings");

                    Assert.AreEqual(0, form.SandboxAdminState.SandboxGroups.Count);
                    Assert.AreEqual(1, form.SandboxList.Items.Count,
                        "Python updates sandbox_groups before refreshing the visible list");
                    Assert.IsFalse(form.SandboxList.IsGroupIndex(0));
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        private static void ExecuteAction(MainForm form, string action)
        {
            MethodInfo method = typeof(MainForm).GetMethod(
                "ExecuteApplicationAction",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            method.Invoke(form, new object[] { action });
        }

        private static LauncherSettingsState GetSettingsState(MainForm form)
        {
            FieldInfo field = typeof(MainForm).GetField(
                "_settingsState",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            return (LauncherSettingsState)field.GetValue(form);
        }

        private static bool GetPrivateBool(object target, string name)
        {
            FieldInfo field = target.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            return (bool)field.GetValue(target);
        }

        private static MainForm CreateForm(string root)
        {
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            return new MainForm(new LauncherRuntime(
                false,
                executable,
                executable,
                () => new DateTime(2026, 10, 12, 13, 0, 0)));
        }

        private static void RunInSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception caught) { error = caught; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
            {
                throw new AssertFailedException("STA action failed: " + error, error);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "shahe_settings_runtime_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Destroy(MainForm form)
        {
            if (form == null)
            {
                return;
            }

            try { form.Destroy(); }
            catch
            {
                try { form.Dispose(); }
                catch { }
            }
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }
    }
}
