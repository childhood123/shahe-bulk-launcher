using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Processes;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("ProcessActions")]
    public sealed class CloseProcessFeedbackRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void CloseCompletionPreservesSpeechAndRefreshScope(bool all, bool hasTargets)
        {
            using (var service = new ProcessActionService())
            using (var posted = new BlockingCollection<Action>())
            {
                var events = new List<string>();
                var callbacks = new ProcessActionCallbacks
                {
                    ResolveSandboxNames = name => Tuple.Create("Box1", "用户一"),
                    GetSandboxPids = name => hasTargets ? new[] { int.MaxValue } : new int[0],
                    GetAllSandboxPids = () => hasTargets ? new[] { int.MaxValue } : new int[0],
                    GetSandboxieStartExecutable = () => @"C:\test-only\Start.exe",
                    GetOfficialSandboxNames = () => new[] { "Box1" },
                    RunSandboxieStart = args => Tuple.Create(0, ""),
                    PidExists = pid => false,
                    RunProcessCommand = args => Assert.Fail("No PID fallback should be necessary"),
                    PostToUi = action => posted.Add(action),
                    SetStatus = text => events.Add("status:" + text),
                    Speak = (text, key) => events.Add("speak:" + key + ":" + text),
                    RefreshProcessUi = full => events.Add("refresh:" + full),
                    RefreshVisibleStates = () => { events.Add("states"); return true; }
                };
                Assert.IsTrue(all ? service.OnCloseAllSandboxes(callbacks)
                    : service.OnCloseCurrentSandbox("Box1", callbacks));
                Action complete;
                Assert.IsTrue(posted.TryTake(out complete, 5000), "Close worker did not post its result");
                events.Clear();
                complete();
                string status = all
                    ? (hasTargets ? "已关闭全部打开进程" : "当前没有可关闭的全部打开进程")
                    : (hasTargets ? "已关闭当前打开的进程: 用户一" : "当前用户 用户一 没有可关闭的已开进程");
                var expected = new List<string> { "status:" + status };
                if (hasTargets)
                {
                    expected.Add(all ? "speak:close_all:已关闭全部进程" : "speak:close_current:已关闭用户一进程");
                }
                expected.Add("refresh:" + hasTargets);
                expected.Add("states");
                CollectionAssert.AreEqual(expected, events);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void CurrentPartialOrFailedCloseDoesNotAnnounceSuccess(bool closesOne)
        {
            using (var service = new ProcessActionService())
            using (var posted = new BlockingCollection<Action>())
            {
                var alive = new HashSet<int> { int.MaxValue, int.MaxValue - 1 };
                var events = new List<string>();
                var callbacks = new ProcessActionCallbacks
                {
                    ResolveSandboxNames = name => Tuple.Create("Box1", "用户一"),
                    GetSandboxPids = name => new[] { int.MaxValue, int.MaxValue - 1 },
                    GetSandboxieStartExecutable = () => @"C:\test-only\Start.exe",
                    RunSandboxieStart = args => Tuple.Create(1, "isolated failure"),
                    PidExists = pid => alive.Contains(pid),
                    RunProcessCommand = args => { if (closesOne) alive.Remove(int.MaxValue); },
                    PostToUi = action => posted.Add(action),
                    SetStatus = text => events.Add(text),
                    Speak = (text, key) => events.Add("unexpected speech"),
                    RefreshProcessUi = full => events.Add("refresh:" + full),
                    RefreshVisibleStates = () => { events.Add("states"); return true; }
                };
                Assert.IsTrue(service.OnCloseCurrentSandbox("Box1", callbacks));
                Action complete;
                Assert.IsTrue(posted.TryTake(out complete, 5000));
                events.Clear();
                complete();
                CollectionAssert.AreEqual(new[] {
                    closesOne ? "已关闭部分进程，仍有进程未关闭: 用户一" : "未关闭任何进程: 用户一",
                    "refresh:True", "states" }, events);
            }
        }

        [DataTestMethod]
        [DataRow("closed_some", 2, true, true)]
        [DataRow("partially_closed", 0, true, false)]
        [DataRow("none_closed", 0, false, false)]
        public void CloseAllPartialFeedbackMatchesPython(string status, int closed, bool any, bool speaks)
        {
            using (var service = new ProcessActionService())
            {
                var spoken = new List<string>();
                bool? refresh = null;
                var callbacks = new ProcessActionCallbacks
                {
                    PostToUi = action => action(),
                    Speak = (text, key) => spoken.Add(key + ":" + text),
                    RefreshProcessUi = full => refresh = full
                };
                typeof(ProcessActionService).GetMethod("PostCloseStatus",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(service, new object[] { callbacks, status, closed, any });
                CollectionAssert.AreEqual(speaks ? new[] { "close_all:已关闭全部进程" } : new string[0], spoken);
                Assert.AreEqual(true, refresh);
            }
        }
    }
}
