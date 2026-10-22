using System;
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
    }
}
