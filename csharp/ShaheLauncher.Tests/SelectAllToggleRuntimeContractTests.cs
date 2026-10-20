using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class SelectAllToggleRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow("file-menu")]
        [DataRow("ctrl-a")]
        public void FileMenuAndCtrlAToggleCheckableRowsLikePython(string trigger)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                AddRows(form);
                form.SandboxList.Items[0].Checked = true;

                Trigger(form, trigger);

                AssertChecked(form, true, false, true);

                Trigger(form, trigger);

                AssertChecked(form, false, false, false);
            });
        }

        [DataTestMethod]
        [DataRow("file-menu")]
        [DataRow("ctrl-a")]
        public void EmptyListToggleDoesNotAnnounceASelectionChange(string trigger)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                int updates = 0;
                form.SandboxToggledCallback = count => updates++;
                form.InitializeUi();

                Trigger(form, trigger);
                Trigger(form, trigger);

                Assert.AreEqual(0, updates);
            });
        }

        [TestMethod]
        public void ContextMenuSelectAllRemainsOneWay()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                AddRows(form);
                form.SandboxList.CheckAll();

                using (ContextMenu menu = form.SandboxList.BuildContextMenu(0))
                {
                    MenuItem selectAll = menu.MenuItems.Cast<MenuItem>()
                        .Single(item => item.Text == "全选");
                    selectAll.PerformClick();
                    selectAll.PerformClick();
                }

                AssertChecked(form, true, false, true);
            });
        }

        private static void AddRows(MainForm form)
        {
            form.SandboxList.Items.Add("用户1 【空闲中】");
            form.SandboxList.Items.Add("[分组] 常用");
            form.SandboxList.Items.Add("用户2 【空闲中】");
            form.SandboxList.SetSpecialItemIndexes(new[] { 1 });
        }

        private static void Trigger(MainForm form, string trigger)
        {
            if (trigger == "file-menu")
            {
                MenuItem item = FindMenuItem(form.Menu.MenuItems, "全选");
                Assert.IsNotNull(item);
                item.PerformClick();
                return;
            }

            MethodInfo processCmdKey = typeof(MainForm).GetMethod(
                "ProcessCmdKey",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(processCmdKey);
            object[] arguments = { new Message(), Keys.Control | Keys.A };
            Assert.IsTrue((bool)processCmdKey.Invoke(form, arguments));
        }

        private static MenuItem FindMenuItem(Menu.MenuItemCollection items, string visibleText)
        {
            foreach (MenuItem item in items)
            {
                string text = (item.Text ?? string.Empty).Split('\t')[0].Replace("&", string.Empty);
                if (text == visibleText)
                {
                    return item;
                }

                MenuItem child = FindMenuItem(item.MenuItems, visibleText);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private static void AssertChecked(MainForm form, bool first, bool group, bool second)
        {
            Assert.AreEqual(first, form.SandboxList.Items[0].Checked);
            Assert.AreEqual(group, form.SandboxList.Items[1].Checked);
            Assert.AreEqual(second, form.SandboxList.Items[2].Checked);
        }
    }
}
