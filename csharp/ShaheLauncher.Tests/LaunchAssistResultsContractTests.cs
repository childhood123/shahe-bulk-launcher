using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Scheduling;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Scheduling")]
    [TestCategory("Speech")]
    public sealed class LaunchAssistResultsContractTests
    {
        [TestMethod]
        public void SuccessfulBatchSpeaksOneFinalUserCountAndKeepsProgressAndRefresh()
        {
            var scenario = new LaunchScenario();

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B" }, 1);
            Assert.AreEqual(2, result.Success);
            Assert.AreEqual(2, scenario.Starts.Count);
            Assert.IsTrue(scenario.Progress.Any(text => text.Contains("启动进度 2/2")));
            Assert.IsTrue(scenario.Logs.Any(text => text.Contains("启动任务结果:")));
            scenario.Complete(result);

            scenario.AssertOnlySpeech("已启动2个用户");
            CollectionAssert.Contains(scenario.Statuses.ToArray(),
                "启动完成：成功 2/2。按 F5 可刷新运行状态");
            Assert.AreEqual(3, scenario.RefreshCount);
        }

        [TestMethod]
        public void MultipleAppsCountDistinctStartedUsersInsteadOfSuccessfulAppTasks()
        {
            var scenario = new LaunchScenario();

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B" }, 2);
            Assert.AreEqual(4, result.Total);
            Assert.AreEqual(4, result.Success);
            Assert.AreEqual(4, scenario.Starts.Count);
            scenario.Complete(result);

            scenario.AssertOnlySpeech("已启动2个用户");
            CollectionAssert.Contains(scenario.Statuses.ToArray(),
                "启动完成：成功 4/4。按 F5 可刷新运行状态");
        }

        [TestMethod]
        public void PartialFailureReportsStartedUsersAndFailedTaskCountOnce()
        {
            var scenario = new LaunchScenario
            {
                Start = (name, path, directory) => name == "用户A"
            };

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B", "用户C" }, 1);
            Assert.AreEqual(1, result.Success);
            Assert.AreEqual(2, result.Failed);
            scenario.Complete(result);

            scenario.AssertOnlySpeech("已启动1个用户，失败2项");
            CollectionAssert.Contains(scenario.Statuses.ToArray(),
                "启动完成：成功 1/3，失败 2。按 F5 可刷新运行状态");
        }

        [TestMethod]
        public void OneUsersFailedAppDoesNotHideTheirSuccessfulAppOrMislabelTaskCount()
        {
            var scenario = new LaunchScenario
            {
                Start = (name, path, directory) => path.EndsWith("app1.exe", StringComparison.Ordinal)
            };

            LaunchBatchResult result = scenario.Run(new[] { "用户A" }, 2);
            Assert.AreEqual(1, result.Success);
            Assert.AreEqual(1, result.Failed);
            scenario.Complete(result);

            scenario.AssertOnlySpeech("已启动1个用户，失败1项");
        }

        [TestMethod]
        public void FailedAndThrowingStartsProduceOneShortFailureWithoutSuccess()
        {
            var scenario = new LaunchScenario
            {
                Start = (name, path, directory) =>
                {
                    if (name == "用户B") throw new InvalidOperationException("isolated launch failure");
                    return false;
                }
            };

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B" }, 1);
            Assert.AreEqual(0, result.Success);
            Assert.AreEqual(2, result.Failed);
            Assert.IsTrue(scenario.Logs.Any(text => text.Contains("isolated launch failure")));
            scenario.Complete(result);

            scenario.AssertOnlySpeech("启动失败，共2项");
            CollectionAssert.Contains(scenario.Statuses.ToArray(),
                "启动完成：成功 0/2，失败 2。按 F5 可刷新运行状态");
        }

        [TestMethod]
        public void AlreadyRunningUsersAreReportedAsSkippedWithoutCallingStart()
        {
            var scenario = new LaunchScenario();
            scenario.State.SkipRunningSandboxOnLaunch = true;
            scenario.Callbacks.GetSandboxPids = name => new[] { 101 };

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B" }, 2);
            Assert.AreEqual(4, result.Skipped);
            Assert.AreEqual(0, result.Success);
            Assert.AreEqual(0, scenario.Starts.Count);
            scenario.Complete(result);

            scenario.AssertOnlySpeech("所选用户已运行，无需启动");
        }

        [TestMethod]
        public void SkippedUsersAreNotCountedAsNewlyStartedUsers()
        {
            var scenario = new LaunchScenario();
            scenario.State.SkipRunningSandboxOnLaunch = true;
            scenario.Callbacks.GetSandboxPids = name => name == "用户A" ? new[] { 101 } : new int[0];

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B" }, 2);
            Assert.AreEqual(2, result.Skipped);
            Assert.AreEqual(2, result.Success);
            CollectionAssert.AreEquivalent(new[] { "用户B", "用户B" }, scenario.Starts.ToArray());
            scenario.Complete(result);

            scenario.AssertOnlySpeech("已启动1个用户");
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void CancellationBeforeOrAfterOneStartNeverAnnouncesSuccess(bool afterOneStart)
        {
            var scenario = new LaunchScenario();
            int cancelled = afterOneStart ? 0 : 1;
            scenario.State.LaunchConcurrencyLimit = 1;
            scenario.Callbacks.IsLaunchSessionCancelled = id => Volatile.Read(ref cancelled) != 0;
            scenario.Start = (name, path, directory) =>
            {
                Interlocked.Exchange(ref cancelled, 1);
                return true;
            };

            LaunchBatchResult result = scenario.Run(new[] { "用户A", "用户B" }, 1);
            Assert.IsTrue(result.Cancelled);
            Assert.AreEqual(afterOneStart ? 1 : 0, result.Success);
            Assert.AreEqual(afterOneStart ? 1 : 0, scenario.Starts.Count);
            scenario.Complete(result);

            Assert.AreEqual(0, scenario.Speech.Count);
        }

        [TestMethod]
        public void CancellationWhileCompletionWaitsForUiDoesNotAnnounceSuccess()
        {
            var scenario = new LaunchScenario();
            bool cancelled = false;
            scenario.Callbacks.IsLaunchSessionCancelled = id => cancelled;
            scenario.Callbacks.ClearLaunchCancellation = () => cancelled = false;
            LaunchBatchResult result = scenario.Run(new[] { "用户A" }, 1);
            Assert.AreEqual(1, result.Success);
            Assert.IsFalse(result.Cancelled);

            // The worker result exists, but its UI completion has not run yet.
            cancelled = true;
            scenario.Complete(result);

            Assert.AreEqual(0, scenario.Speech.Count);
        }

        [TestMethod]
        public void ClosingWindowSuppressesFinalSpeechAndScheduledRefreshes()
        {
            var scenario = new LaunchScenario();
            LaunchBatchResult result = scenario.Run(new[] { "用户A" }, 1);
            Assert.AreEqual(1, result.Success);
            scenario.Callbacks.IsWindowClosing = () => true;

            scenario.Complete(result);

            Assert.AreEqual(0, scenario.Speech.Count);
            Assert.AreEqual(0, scenario.RefreshCount);
        }

        [TestMethod]
        public void CallbackFailureAfterSuccessfulStartStillCompletesTheLaunchResult()
        {
            var scenario = new LaunchScenario();
            scenario.State.RememberVolume = true;
            scenario.State.SandboxVolumeMemory["用户A"] = 55;
            scenario.Callbacks.RestoreSavedVolume = name => { };
            scenario.Callbacks.GetVolumeMemoryKey = name =>
            {
                throw new InvalidOperationException("isolated memory key failure");
            };

            Task<LaunchBatchResult> task = scenario.RunTask(new[] { "用户A" }, 1);
            Assert.IsTrue(task.Wait(3000), "非启动回调异常不能让启动任务悬挂");
            LaunchBatchResult result = task.Result;
            Assert.AreEqual(1, result.Success);
            Assert.IsFalse(scenario.State.IsLaunching);
            Assert.IsNull(scenario.State.ActiveLaunchTargets);
            scenario.AssertOnlySpeech("已启动1个用户");
            CollectionAssert.Contains(scenario.Statuses.ToArray(),
                "启动完成：成功 1/1。按 F5 可刷新运行状态");
        }

        private sealed class LaunchScenario
        {
            private readonly LaunchScheduler _scheduler = new LaunchScheduler();
            private readonly List<Action> _refreshActions = new List<Action>();

            public readonly LaunchSchedulerState State = new LaunchSchedulerState
            {
                LaunchInterval = 0,
                LaunchConcurrencyLimit = 2
            };
            public readonly LaunchSchedulerCallbacks Callbacks;
            public readonly ConcurrentQueue<string> Speech = new ConcurrentQueue<string>();
            public readonly ConcurrentQueue<string> Starts = new ConcurrentQueue<string>();
            public readonly ConcurrentQueue<string> Statuses = new ConcurrentQueue<string>();
            public readonly ConcurrentQueue<string> Progress = new ConcurrentQueue<string>();
            public readonly ConcurrentQueue<string> Logs = new ConcurrentQueue<string>();
            public Func<string, string, string, bool> Start = (name, path, directory) => true;
            public int RefreshCount;

            public LaunchScenario()
            {
                Callbacks = new LaunchSchedulerCallbacks
                {
                    StartAppInSandbox = (name, path, directory) =>
                    {
                        Starts.Enqueue(name);
                        return Start(name, path, directory);
                    },
                    SetStatus = Statuses.Enqueue,
                    SetSafeStatus = Statuses.Enqueue,
                    SetLaunchStatusIfActive = (id, text) => Progress.Enqueue(text),
                    Log = Logs.Enqueue,
                    Speak = (text, key) => Speech.Enqueue(text + "|" + key),
                    DelayMilliseconds = milliseconds => { },
                    ScheduleDelayed = (milliseconds, action) =>
                    {
                        _refreshActions.Add(action);
                        return null;
                    },
                    RefreshVisibleSandboxStates = () => RefreshCount++
                };
            }

            public LaunchBatchResult Run(string[] users, int appCount)
            {
                LaunchAppTarget[] apps = Enumerable.Range(1, appCount)
                    .Select(index => new LaunchAppTarget
                    {
                        Path = @"C:\IsolatedApps\app" + index + ".exe",
                        Directory = @"C:\IsolatedApps"
                    }).ToArray();
                Assert.IsTrue(_scheduler.BeginLaunch(State, users, apps, Callbacks));
                LaunchBatchResult result = _scheduler.RunLaunchBatch(
                    State, State.LaunchSessionId, users, Callbacks);
                Assert.AreEqual(0, Speech.Count,
                    "Preparation, per-item progress and background completion must not speak before the final result.");
                return result;
            }

            public Task<LaunchBatchResult> RunTask(string[] users, int appCount)
            {
                LaunchAppTarget[] apps = Enumerable.Range(1, appCount)
                    .Select(index => new LaunchAppTarget
                    {
                        Path = @"C:\IsolatedApps\app" + index + ".exe",
                        Directory = @"C:\IsolatedApps"
                    }).ToArray();
                Assert.IsTrue(_scheduler.BeginLaunch(State, users, apps, Callbacks));
                return _scheduler.RunLaunchTaskAsync(State, State.LaunchSessionId, users, Callbacks);
            }

            public void Complete(LaunchBatchResult result)
            {
                _scheduler.CompleteLaunch(State, result, Callbacks);
                Assert.IsFalse(State.IsLaunching);
                Assert.IsNull(State.ActiveLaunchTargets);
                foreach (Action refresh in _refreshActions) refresh();
            }

            public void AssertOnlySpeech(string expected)
            {
                CollectionAssert.AreEqual(new[] { expected + "|launch_done" }, Speech.ToArray());
            }
        }
    }
}
