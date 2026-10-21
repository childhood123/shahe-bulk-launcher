using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Processes")]
    public sealed class ProcessRefreshFeedbackRuntimeContractTests
    {
        [TestMethod]
        public void ManualRefreshUsesRunningStateFeedbackAndCompletionCallback()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.ProcessCallbacks.RefreshVisibleStates = () => true;

                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "refresh_process_state");
                Assert.AreEqual("正在刷新进程状态", form.StatusLabel.Text);

                PropertyInfo completed = form.SandboxList.GetType().GetProperty(
                    "RunningStatesRefreshCompletedCallback",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.IsNotNull(completed, "运行状态刷新必须提供完成回调");
                Assert.IsNotNull(completed.GetValue(form.SandboxList));
                ((System.Action)completed.GetValue(form.SandboxList))();
                Assert.AreEqual("已刷新进程状态", form.StatusLabel.Text);
            });
        }

        [TestMethod]
        public void ManualRefreshReportsWhenNoVisibleStateCanBeRefreshed()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.ProcessCallbacks.RefreshVisibleStates = () => false;

                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "refresh_process_state");
                Assert.AreEqual("没有可刷新的进程状态", form.StatusLabel.Text);
            });
        }
    }
}
