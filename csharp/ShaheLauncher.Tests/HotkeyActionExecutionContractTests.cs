using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Dialogs.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("SettingsDialogs")]
    public sealed class HotkeyActionExecutionContractTests
    {
        [TestMethod]
        public void EveryActionRowDispatchesOnceAfterDialogDisposalWithoutSavingEdits()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                var actions = form.HotkeyController.ActionSpecs.Select(spec => spec.Action).ToArray();
                var dispatched = new List<string>();
                HotkeySettingsDialog activeDialog = null;
                form.ApplicationActionCallback = action =>
                {
                    Assert.IsTrue(activeDialog.IsDisposed, "Dispatch must happen after the hotkey modal is disposed.");
                    dispatched.Add(action);
                };
                int initialSettingsCount = form.Settings.Count;
                for (int index = 0; index < actions.Length; index++)
                {
                    int selectedIndex = index;
                    Exception callbackError = null;
                    using (var timer = new Timer { Interval = 10 })
                    {
                        timer.Tick += (sender, args) =>
                        {
                            activeDialog = Application.OpenForms.OfType<HotkeySettingsDialog>().SingleOrDefault();
                            if (activeDialog == null) return;
                            timer.Stop();
                            try
                            {
                                activeDialog.SelectAction(selectedIndex);
                                activeDialog.HotkeyTextBox.Text = "Ctrl+Q";
                                activeDialog.HotkeysEnabledCheckBox.Checked = true;
                                activeDialog.ActiveControl = activeDialog.ActionList;
                                Assert.AreEqual(selectedIndex, activeDialog.ActionList.SelectedIndex);
                                Assert.AreEqual(selectedIndex, dispatched.Count, "Selection alone must not execute.");
                                Assert.IsTrue(ProcessKey(activeDialog, Keys.Enter));
                            }
                            catch (Exception error)
                            {
                                callbackError = error;
                                activeDialog.DialogResult = DialogResult.Cancel;
                                activeDialog.Close();
                            }
                        };
                        timer.Start();
                        MigrationRuntimeTestHelpers.Invoke(form, "ShowHotkeySettingsDialog");
                    }
                    if (callbackError != null) throw callbackError;
                    Application.DoEvents();
                    Assert.AreEqual(index + 1, dispatched.Count, actions[index] + " should execute exactly once.");
                    Assert.AreEqual(actions[index], dispatched[index]);
                    Assert.AreEqual(initialSettingsCount, form.Settings.Count, "Execution must not save hotkey edits.");
                    Assert.IsFalse(File.Exists(form.ConfigurationStorePath), "No settings file should be written by execution.");
                }
                CollectionAssert.AreEqual(actions, dispatched.ToArray());
                Console.WriteLine("All production action rows covered: " + actions.Length);
            });
        }

        [TestMethod]
        public void EnterInHotkeyEditorCapturesBindingWithoutExecutingOrConfirming()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                using (var dialog = CreateDialog(form, false))
                {
                    dialog.Show();
                    dialog.ActiveControl = dialog.HotkeyTextBox;
                    Assert.IsTrue(ProcessKey(dialog, Keys.Enter));
                    Assert.AreEqual("Enter", dialog.HotkeyTextBox.Text);
                    Assert.AreEqual(DialogResult.None, dialog.DialogResult);
                    Assert.AreEqual(string.Empty, RequestedAction(dialog));
                    Assert.IsTrue(ProcessKey(dialog, Keys.Control | Keys.Enter));
                    Assert.AreEqual("Ctrl+Enter", dialog.HotkeyTextBox.Text);
                    Assert.AreEqual(string.Empty, RequestedAction(dialog));
                }
            });
        }

        [TestMethod]
        public void SelectionModifiedEnterAndCancelDoNotRequestExecution()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                using (var dialog = CreateDialog(form, false))
                {
                    dialog.Show();
                    dialog.SelectAction(1);
                    dialog.ActiveControl = dialog.ActionList;
                    Assert.AreEqual(string.Empty, RequestedAction(dialog));
                    ProcessKey(dialog, Keys.Control | Keys.Enter);
                    Assert.AreEqual(string.Empty, RequestedAction(dialog));
                    Assert.AreEqual(DialogResult.None, dialog.DialogResult);
                    Assert.IsTrue(ProcessKey(dialog, Keys.Escape));
                    Assert.AreEqual(string.Empty, RequestedAction(dialog));
                }
            });
        }

        [TestMethod]
        public void EmptyActionSelectionDoesNotConfirmOrExecute()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                using (var dialog = CreateDialog(form, true))
                {
                    dialog.Show();
                    dialog.ActiveControl = dialog.ActionList;
                    Assert.IsTrue(ProcessKey(dialog, Keys.Enter));
                    Assert.AreEqual(DialogResult.None, dialog.DialogResult);
                    Assert.AreEqual(string.Empty, RequestedAction(dialog));
                }
            });
        }

        private static HotkeySettingsDialog CreateDialog(MainForm form, bool empty)
        {
            var specs = form.HotkeyController.ActionSpecs.Take(empty ? 0 : 2)
                .Select(spec => (IDictionary<string, object>)new Dictionary<string, object>
                {
                    { "action", spec.Action }, { "label", spec.Label }
                });
            return new HotkeySettingsDialog(form, specs, new Dictionary<string, string>(),
                new Dictionary<string, string>(), form.HotkeyController.ParseHotkeyText,
                form.HotkeyController.FormatHotkeyText, false, null);
        }

        private static bool ProcessKey(HotkeySettingsDialog dialog, Keys keys)
        {
            var message = Message.Create(dialog.Handle, 0x100, new IntPtr((int)(keys & Keys.KeyCode)), new IntPtr(1));
            return (bool)typeof(HotkeySettingsDialog).GetMethod("ProcessCmdKey",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog, new object[] { message, keys });
        }

        private static string RequestedAction(HotkeySettingsDialog dialog)
        {
            var property = typeof(HotkeySettingsDialog).GetProperty("RequestedAction");
            Assert.IsNotNull(property, "Selected action must be exposed to the existing main form dispatcher.");
            return (string)property.GetValue(dialog);
        }
    }
}
