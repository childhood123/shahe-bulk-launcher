using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Dialogs.Settings;
using ShaheLauncher.Core.AppPaths;
using ShaheLauncher.Core.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("AppPaths")]
    [TestCategory("SettingsDialogs")]
    public sealed class PathHistoryLimitRuntimeContractTests
    {
        [TestMethod]
        public void HistoryServiceNormalizesZeroTo999AndRetains999Records()
        {
            var service = new AppPathHistoryService();
            int[,] cases =
            {
                { -1, 0 },
                { 0, 0 },
                { 20, 20 },
                { 21, 21 },
                { 999, 999 },
                { 1000, 999 }
            };

            for (int row = 0; row < cases.GetLength(0); row++)
            {
                int input = cases[row, 0];
                int expected = cases[row, 1];
                var state = new AppPathHistoryState
                {
                    AppHistory = Enumerable.Range(0, 1000)
                        .Select(index => new AppHistoryItem
                        {
                            Path = "C:\\App" + index + ".exe",
                            Note = "应用" + index
                        })
                        .ToList()
                };

                service.ApplyHistoryCount(state, input, null);

                Assert.AreEqual(expected, state.MaxHistoryItems, "输入=" + input);
                Assert.AreEqual(Math.Min(1000, expected), state.AppHistory.Count, "输入=" + input);
            }

            FieldInfo maximum = typeof(AppPathHistoryService).GetField(
                "MaximumHistoryItems", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(maximum, "路径记忆上限必须只有一个共享定义。");
            Assert.AreEqual(999, Convert.ToInt32(maximum.GetValue(null)));
        }

        [TestMethod]
        public void MainFormKeeps999AcrossLoadCommitAndManagerSave()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Assert.IsTrue(form.ConfigStore.SetJson("settings_json", new Dictionary<string, object>
                {
                    { "max_history_items", 999 },
                    { "app_history", new List<object>() }
                }));
                MigrationRuntimeTestHelpers.Invoke(form, "LoadApplicationSettings");
                Assert.AreEqual(999, GetSettingsState(form).MaxHistoryItems, "启动加载不得把 999 压回 20。");

                MigrationRuntimeTestHelpers.Invoke(form, "CommitAppPathHistoryState",
                    new AppPathHistoryState { MaxHistoryItems = 999 });
                Assert.AreEqual(999, GetSettingsState(form).MaxHistoryItems, "状态提交不得把 999 压回 20。");
                Assert.AreEqual(999, Convert.ToInt32(form.Settings["max_history_items"]));

                MigrationRuntimeTestHelpers.Invoke(
                    form,
                    "OnPathMemorySaved",
                    new List<IDictionary<string, object>>(),
                    new Dictionary<string, object>
                    {
                        { "max_history_items", 999 },
                        { "hide_path", false }
                    });
                Assert.AreEqual(999, GetSettingsState(form).MaxHistoryItems, "管理器保存不得把 999 压回 20。");

                Assert.IsTrue(form.ConfigStore.SetJson("settings_json", new Dictionary<string, object>
                {
                    { "max_history_items", "无效值" },
                    { "app_history", new List<object>() }
                }));
                MigrationRuntimeTestHelpers.Invoke(form, "LoadApplicationSettings");
                Assert.AreEqual(10, GetSettingsState(form).MaxHistoryItems, "无效启动值继续恢复默认 10。");
            });
        }

        [TestMethod]
        [TestCategory("SettingsService")]
        public void SettingsServiceKeeps999WhenApplyingOtherSettings()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var state = new LauncherSettingsState
                {
                    Settings = new Dictionary<string, object>(),
                    MaxHistoryItems = 10
                };
                var incoming = new Dictionary<string, object>
                {
                    { "max_history_items", 999 },
                    { "show_tray", false }
                };

                form.SettingsService.ApplySettings(
                    state,
                    incoming,
                    new LauncherSettingsCallbacks());

                Assert.AreEqual(999, state.MaxHistoryItems, "保存其他软件设置不得把 999 压回 20。");
                var persisted = (System.Collections.IDictionary)form.ConfigStore.GetJson(
                    "settings_json", null);
                Assert.IsNotNull(persisted);
                Assert.AreEqual(999L, Convert.ToInt64(persisted["max_history_items"]));
            });
        }

        [TestMethod]
        public void MainFormRendersTenHistoryItemsWithoutNullListViewItem()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                LauncherSettingsState state = GetSettingsState(form);
                state.MaxHistoryItems = 10;
                state.AppHistory = Enumerable.Range(0, 10)
                    .Select(index => (object)new AppHistoryItem
                    {
                        Path = "C:\\Gap09\\App" + index + ".exe",
                        Note = "应用" + index
                    })
                    .ToList();
                state.AppPath = string.Empty;
                state.AppDirectory = string.Empty;

                MigrationRuntimeTestHelpers.Invoke(form, "UpdateAppPathList");

                Assert.AreEqual(10, form.AppPathList.Items.Count);
            });
        }

        [TestMethod]
        public void MainFormInitializesTenPersistedHistoryItemsWithoutThreadException()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var history = Enumerable.Range(0, 10)
                    .Select(index => (object)new Dictionary<string, object>
                    {
                        { "path", "C:\\Gap09\\App" + index + ".exe" },
                        { "note", "应用" + index }
                    })
                    .ToList();
                Assert.IsTrue(form.ConfigStore.SetJson("settings_json", new Dictionary<string, object>
                {
                    { "max_history_items", 10 },
                    { "app_history", history },
                    { "sandbox_root", root },
                    { "sandboxie_path", string.Empty },
                    { "show_tray", false },
                    { "auto_run", false }
                }));
                form.FindSandboxiePathCallback = () => string.Empty;
                form.ShowMessageCallback = (text, title, icon) => { };
                form.InitializeApplication(false);
                Application.DoEvents();
                Assert.AreEqual(10, form.AppPathList.Items.Count);
            });
        }

        [TestMethod]
        public void BothCountDialogsExpose999WithoutChangingAccessibility()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                decimal quickMaximum = -1;
                string quickAccessibleName = string.Empty;
                using (var timer = new Timer { Interval = 25 })
                {
                    timer.Tick += (sender, eventArgs) =>
                    {
                        Form dialog = Application.OpenForms.Cast<Form>()
                            .FirstOrDefault(candidate =>
                                !ReferenceEquals(candidate, form)
                                && candidate.Text == AppPathHistoryService.HistoryCountDialogTitle);
                        if (dialog == null)
                        {
                            return;
                        }

                        NumericUpDown input = Descendants(dialog).OfType<NumericUpDown>().Single();
                        quickMaximum = input.Maximum;
                        quickAccessibleName = input.AccessibleName;
                        input.Value = input.Maximum;
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Close();
                    };
                    timer.Start();
                    int selected = Convert.ToInt32(MigrationRuntimeTestHelpers.Invoke(
                        form, "PromptAppPathHistoryCount", 999));
                    timer.Stop();

                    Assert.AreEqual(999m, quickMaximum);
                    Assert.AreEqual(999, selected);
                    Assert.AreEqual("路径记忆数量", quickAccessibleName);
                }

                var settings = new Dictionary<string, object>
                {
                    { "max_history_items", 999 },
                    { "hide_path", false }
                };
                IDictionary<string, object> saved = null;
                using (var dialog = new PathMemoryManagerDialog(
                    form,
                    new List<IDictionary<string, object>>(),
                    settings,
                    (history, values) => saved = values))
                {
                    Assert.AreEqual(0m, dialog.HistoryCountInput.Minimum);
                    Assert.AreEqual(999m, dialog.HistoryCountInput.Maximum);
                    Assert.AreEqual(999m, dialog.HistoryCountInput.Value);
                    Assert.AreEqual("路径记忆数量", dialog.HistoryCountInput.AccessibleName);
                    Assert.AreEqual(0, dialog.HistoryCountInput.TabIndex);
                    Assert.AreEqual(
                        "(0=不记录, 最大999)",
                        Descendants(dialog).OfType<Label>()
                            .Single(label => label.Text.Contains("0=不记录")).Text);

                    dialog.SaveChanges();
                }

                Assert.IsNotNull(saved);
                Assert.AreEqual(999, Convert.ToInt32(saved["max_history_items"]));
            });
        }

        private static LauncherSettingsState GetSettingsState(MainForm form)
        {
            FieldInfo field = typeof(MainForm).GetField(
                "_settingsState", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            return (LauncherSettingsState)field.GetValue(form);
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (Control descendant in Descendants(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
