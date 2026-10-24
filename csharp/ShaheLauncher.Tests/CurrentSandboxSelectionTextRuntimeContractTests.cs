using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class CurrentSandboxSelectionTextRuntimeContractTests
    {
        [TestMethod]
        public void CurrentActionsUsePythonUnselectedUserText()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                int invoked = 0;
                form.MuteCurrentCallback = name => invoked++;
                form.VolumeCurrentCallback = (name, step) => invoked++;
                form.SwitchOutputCurrentCallback = name => invoked++;
                form.CloseCurrentCallback = name => invoked++;
                form.LocateCurrentCallback = name => invoked++;
                form.PinToggleCallback = name => invoked++;

                foreach (string action in new[]
                {
                    "mute_current", "volume_current_up", "volume_current_down",
                    "switch_output_current", "close_current_process",
                    "locate_current_process", "pin_current_user"
                })
                {
                    MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", action);
                    Assert.AreEqual("请先在用户列表中选中一个用户", form.StatusLabel.Text, action);
                }

                Assert.AreEqual(0, invoked);
            });
        }

        [TestMethod]
        public void LocateActionUsesFocusedUserBeforePreviouslySelectedUser()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.SandboxList.Items.Add("用户一 【运行中】");
                form.SandboxList.Items.Add("用户二 【运行中】");
                IntPtr handle = form.SandboxList.Handle;
                form.SandboxList.Items[0].Selected = true;
                form.SandboxList.Items[1].Focused = true;
                var located = new List<string>();
                form.LocateCurrentCallback = located.Add;

                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "locate_current_process");

                CollectionAssert.AreEqual(new[] { "用户二" }, located.ToArray());
                Assert.IsNotNull(form.ProcessCallbacks.EnumerateWindows,
                    "生产定位必须保留按当前用户 PID 枚举窗口的真实边界");
            });
        }

        [TestMethod]
        public void LocateActionUsesUserSelectedByAdjacentShortcut()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.SandboxList.Items.Add("用户一 【运行中】");
                form.SandboxList.Items.Add("用户二 【运行中】");
                IntPtr handle = form.SandboxList.Handle;
                form.SandboxList.Items[0].Selected = true;
                form.SandboxList.Items[0].Focused = true;
                var located = new List<string>();
                form.LocateCurrentCallback = located.Add;

                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "select_next_user");
                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "locate_current_process");
                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "select_prev_user");
                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "locate_current_process");

                CollectionAssert.AreEqual(new[] { "用户二", "用户一" }, located.ToArray());
            });
        }
    }
}
