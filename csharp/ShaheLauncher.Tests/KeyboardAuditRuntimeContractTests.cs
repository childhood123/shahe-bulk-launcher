using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class KeyboardAuditRuntimeContractTests
    {
        [TestMethod]
        public void EnabledAltNavigationSkipsGroupsAndWrapsWithoutCheckingRows()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                SetField(form, "_hotkeysEnabled", true);
                var list = form.SandboxList;
                list.GroupNamesProvider = () => new[] { "常用" };
                list.Items.Add("用户甲 【空闲中】");
                list.Items.Add("[分组] 常用");
                list.Items.Add("用户乙 【空闲中】");
                list.SetSpecialItemIndexes(new[] { 1 });
                // Native item selection needs a handle, but no desktop focus/input is required.
                IntPtr handle = list.Handle;
                list.Items[1].Selected = true;
                list.Items[1].Focused = true;
                int launches = 0;
                form.LaunchSelectedCallback = () => launches++;

                Assert.IsTrue(Dispatch(form, Keys.Alt | Keys.Down, false));
                Assert.AreEqual(0, list.SelectedIndices[0]);
                Assert.IsTrue(Dispatch(form, Keys.Alt | Keys.Up, false));
                Assert.AreEqual(2, list.SelectedIndices[0]);
                Assert.IsTrue(Dispatch(form, Keys.Alt | Keys.Down, false));
                Assert.AreEqual(0, list.SelectedIndices[0]);
                Assert.AreEqual(0, list.CheckedItems.Count);
                Assert.AreEqual(0, launches);

                SetField(form, "_hotkeysEnabled", false);
                Assert.AreEqual(string.Empty, form.ResolveHotkeyAction(Keys.Alt | Keys.Down, false));
                Assert.AreEqual(string.Empty, form.ResolveHotkeyAction(Keys.Alt | Keys.Up, false));
                Assert.AreEqual("select_all", form.ResolveHotkeyAction(Keys.Control | Keys.A, false));
            });
        }

        [TestMethod]
        public void ProcessCmdKeyDistinguishesExtendedEnterBeforeDispatch()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                SetField(form, "_hotkeysEnabled", true);
                SetField(form, "_hotkeys", new Dictionary<string, string>
                {
                    { "select_next_user", "NumPadEnter" },
                    { "select_prev_user", "Enter" }
                });
                var actions = new List<string>();
                form.FixedHotkeyActionCallback = actions.Add;

                Assert.IsTrue(Dispatch(form, Keys.Enter, true), "extended Enter binding");
                Assert.IsTrue(Dispatch(form, Keys.Enter, false), "regular Enter binding");
                CollectionAssert.AreEqual(new[] { "select_next_user", "select_prev_user" }, actions);

                SetField(form, "_hotkeysEnabled", false);
                Assert.AreEqual(string.Empty, form.ResolveHotkeyAction(Keys.Enter, true));
                Assert.AreEqual(string.Empty, form.ResolveHotkeyAction(Keys.Enter, false));
            });
        }

        private static bool Dispatch(MainForm form, Keys key, bool extended)
        {
            // Synthetic message contract only: real keyboard delivery is a separate EXE audit.
            var message = Message.Create(IntPtr.Zero,
                (key & Keys.Alt) != 0 ? 0x104 : 0x100,
                new IntPtr((int)(key & Keys.KeyCode)),
                new IntPtr(0x001C0001 | (extended ? 0x01000000 : 0)));
            var method = typeof(MainForm).GetMethod("ProcessCmdKey",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)method.Invoke(form, new object[] { message, key });
        }

        private static void SetField(MainForm form, string name, object value)
        {
            typeof(MainForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(form, value);
        }
    }
}
