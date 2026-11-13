using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Processes;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("ProcessActions")]
    [TestCategory("Speech")]
    public sealed class ProcessAssistResultsContractTests
    {
        private const int TargetPid = int.MaxValue;
        private const int SecondPid = int.MaxValue - 1;
        private const int UnrelatedPid = int.MaxValue - 2;

        [DataTestMethod]
        [DataRow(false, 0.98, 5, 100)]
        [DataRow(false, 0.02, -5, 0)]
        [DataRow(true, 0.50, 5, 55)]
        public void VolumeSpeaksFinalLevelIncludingClamping(bool all, double initial, int step, int expected)
        {
            using (var run = new ActionRun())
            {
                var session = Audio(TargetPid, initial);
                var unrelated = Audio(UnrelatedPid, 0.35);
                run.Callbacks.GetAudioSessions = () => new[] { session, unrelated };

                run.Execute(() => all
                    ? run.Service.OnAdjustVolumeAllSandboxes(step, run.State, run.Callbacks)
                    : run.Service.OnAdjustVolumeCurrentSandbox("BoxA", step, run.State, run.Callbacks));

                Assert.AreEqual(expected / 100.0, session.MasterVolume, 0.0001);
                Assert.AreEqual(0.35, unrelated.MasterVolume);
                Assert.AreEqual(Subject(all) + "音量调到" + expected + "%", run.Speech(Event("volume", all)));
                if (!all)
                {
                    CollectionAssert.Contains(run.Statuses.ToArray(), ProcessActionService.FormatVolumeCompletedStatus(
                        "用户A", step > 0, step, expected));
                }
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void DifferentSessionsAreReportedAsAverageEvenForOnePid(bool all)
        {
            using (var run = new ActionRun())
            {
                var quiet = Audio(TargetPid, 0.20);
                var loud = Audio(TargetPid, 0.80);
                run.Callbacks.GetAudioSessions = () => new[] { quiet, loud };

                run.Execute(() => all
                    ? run.Service.OnAdjustVolumeAllSandboxes(5, run.State, run.Callbacks)
                    : run.Service.OnAdjustVolumeCurrentSandbox("BoxA", 5, run.State, run.Callbacks));

                Assert.AreEqual(0.25, quiet.MasterVolume, 0.0001);
                Assert.AreEqual(0.85, loud.MasterVolume, 0.0001);
                Assert.AreEqual(Subject(all) + "平均音量55%", run.Speech(Event("volume", all)));
            }
        }

        [DataTestMethod]
        [DataRow(false, true)]
        [DataRow(true, false)]
        public void VolumeWriteFailuresSpeakPartialResultOrFailure(bool all, bool partial)
        {
            using (var run = new ActionRun())
            {
                run.State.RememberVolume = true;
                var changed = Audio(TargetPid, 0.50);
                var failed = Audio(TargetPid, 0.80);
                failed.WriteMasterVolume = value => { throw new InvalidOperationException("isolated audio failure"); };
                run.Callbacks.GetAudioSessions = () => partial ? new[] { changed, failed } : new[] { failed };

                run.Execute(() => all
                    ? run.Service.OnAdjustVolumeAllSandboxes(5, run.State, run.Callbacks)
                    : run.Service.OnAdjustVolumeCurrentSandbox("BoxA", 5, run.State, run.Callbacks));

                Assert.AreEqual(0.80, failed.MasterVolume);
                string spoken = run.Speech(Event("volume", all));
                StringAssert.Contains(spoken, Subject(all));
                StringAssert.Contains(spoken, partial ? "部分" : "失败");
                if (partial)
                {
                    Assert.AreEqual(0.55, changed.MasterVolume, 0.0001);
                    StringAssert.Contains(spoken, "55%");
                }
                else
                {
                    Assert.IsFalse(spoken.Contains("调到"));
                }
                Assert.AreEqual(0, run.State.SandboxVolumeMemory.Count);
            }
        }

        [TestMethod]
        public void SuccessfulVolumeChangeStillSpeaksWhenRememberedVolumeCannotBeSaved()
        {
            using (var run = new ActionRun())
            {
                run.State.RememberVolume = true;
                run.Callbacks.ResolveVolumeMemoryKey = name => name;
                run.Callbacks.SaveSettings = () =>
                {
                    throw new InvalidOperationException("isolated settings failure");
                };
                var session = Audio(TargetPid, 0.50);
                run.Callbacks.GetAudioSessions = () => new[] { session };

                run.Execute(() => run.Service.OnAdjustVolumeCurrentSandbox(
                    "BoxA", 5, run.State, run.Callbacks));

                Assert.AreEqual(0.55, session.MasterVolume, 0.0001);
                Assert.AreEqual("用户A音量调到55%", run.Speech("volume_current"));
                Assert.IsTrue(run.Statuses.Any(text => text.Contains("保存") || text.Contains("设置")),
                    "保存失败应保留独立状态信息");
            }
        }

        [DataTestMethod]
        [DataRow(false, false, "success")]
        [DataRow(false, true, "success")]
        [DataRow(true, false, "success")]
        [DataRow(true, true, "success")]
        [DataRow(false, false, "partial")]
        [DataRow(true, true, "failed")]
        public void MuteSpeaksTheActualToggleOutcome(bool all, bool initiallyMuted, string outcome)
        {
            using (var run = new ActionRun())
            {
                var changed = Audio(TargetPid, 0.50);
                var failed = Audio(TargetPid, 0.50);
                changed.IsMuted = failed.IsMuted = initiallyMuted;
                failed.WriteMute = value => { throw new InvalidOperationException("isolated mute failure"); };
                run.Callbacks.GetAudioSessions = () => outcome == "success" ? new[] { changed }
                    : outcome == "partial" ? new[] { changed, failed } : new[] { failed };

                run.Execute(() => all ? run.Service.OnMuteAllSandboxes(run.State, run.Callbacks)
                    : run.Service.OnMuteCurrentSandbox("BoxA", run.State, run.Callbacks));

                string action = initiallyMuted ? "取消静音" : "静音";
                string spoken = run.Speech(Event("mute", all));
                StringAssert.Contains(spoken, Subject(all));
                StringAssert.Contains(spoken, action);
                if (outcome == "success")
                {
                    Assert.AreEqual(Subject(all) + "已" + action, spoken);
                    Assert.AreEqual(!initiallyMuted, changed.IsMuted);
                }
                else
                {
                    StringAssert.Contains(spoken, outcome == "partial" ? "部分" : "失败");
                    Assert.AreEqual(initiallyMuted, failed.IsMuted);
                    if (outcome == "partial") Assert.AreEqual(!initiallyMuted, changed.IsMuted);
                }
                Assert.AreEqual(run.CallerThread, run.SpeechThread, "Mute completion must stay on the posted UI callback.");
            }
        }

        [DataTestMethod]
        [DataRow("mute", false)]
        [DataRow("mute", true)]
        [DataRow("volume", false)]
        [DataRow("volume", true)]
        [DataRow("switch_output", false)]
        [DataRow("switch_output", true)]
        public void EmptyTargetsProduceOneBriefNoTargetResult(string action, bool all)
        {
            using (var run = new ActionRun())
            {
                run.Callbacks.GetSandboxPids = name => new int[0];
                run.Callbacks.GetAllSandboxPids = () => new int[0];
                run.Execute(() => RunAudioAction(run, action, all), action != "switch_output" || all);

                string spoken = run.Speech(Event(action, all));
                StringAssert.Contains(spoken, all ? "进程" : "用户A");
                Assert.IsTrue(spoken.Contains("没有") || spoken.Contains("未找到"), spoken);
                Assert.IsFalse(spoken.Contains("成功") || spoken.Contains("已切换") || spoken.Contains("调到"));
            }
        }

        [DataTestMethod]
        [DataRow("mute", false)]
        [DataRow("volume", true)]
        public void RunningPidWithoutAudioSessionDoesNotAnnounceAudioSuccess(string action, bool all)
        {
            using (var run = new ActionRun())
            {
                run.Callbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                run.Execute(() => RunAudioAction(run, action, all));

                string spoken = run.Speech(Event(action, all));
                StringAssert.Contains(spoken, Subject(all));
                Assert.IsTrue(spoken.Contains("没有") || spoken.Contains("未找到"), spoken);
            }
        }

        [DataTestMethod]
        [DataRow(false, "success")]
        [DataRow(true, "partial")]
        [DataRow(false, "failed")]
        [DataRow(true, "no-device")]
        public void OutputDeviceSpeaksSuccessfulPartialFailedOrUnavailableResult(bool all, string outcome)
        {
            using (var run = new ActionRun())
            {
                run.Callbacks.GetSandboxPids = name => new[] { TargetPid, SecondPid };
                run.Callbacks.GetAllSandboxPids = () => new[] { TargetPid, SecondPid };
                var routed = new List<int>();
                run.Callbacks.SetOutputDevice = (pid, device) =>
                {
                    bool success = outcome == "success" || (outcome == "partial" && pid == TargetPid);
                    if (success) routed.Add(pid);
                    return success;
                };
                if (outcome == "no-device") run.Callbacks.ListOutputDevices = () => new ProcessAudioDevice[0];

                run.Execute(() => RunAudioAction(run, "switch_output", all));

                string spoken = run.Speech(Event("switch_output", all));
                StringAssert.Contains(spoken, all && outcome == "partial" ? "部分进程" : Subject(all));
                if (outcome == "success" || outcome == "partial")
                {
                    StringAssert.Contains(spoken, "耳机");
                    CollectionAssert.AreEquivalent(outcome == "success" ? new[] { TargetPid, SecondPid }
                        : new[] { TargetPid }, routed.ToArray());
                    if (outcome == "partial") StringAssert.Contains(spoken, "部分");
                }
                else
                {
                    Assert.AreEqual(0, routed.Count);
                    Assert.IsFalse(spoken.Contains("已切换"));
                    if (outcome == "failed") StringAssert.Contains(spoken, "失败");
                    else Assert.IsTrue(spoken.Contains("没有") || spoken.Contains("未找到"), spoken);
                }
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void CancellingDeviceSelectionDoesNotRouteOrSpeakSuccess(bool all)
        {
            using (var run = new ActionRun())
            {
                int routes = 0;
                run.Callbacks.ChooseOutputDevice = (title, devices) => null;
                run.Callbacks.SetOutputDevice = (pid, device) => { routes++; return true; };
                Assert.IsTrue(RunAudioAction(run, "switch_output", all));
                // Cancellation has no final status; wait for the existing worker gate to settle.
                FieldInfo busy = typeof(ProcessActionService).GetField("_volumeTaskBusy",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(busy);
                Assert.IsTrue(SpinWait.SpinUntil(() =>
                {
                    run.DrainUi();
                    return (int)busy.GetValue(run.Service) == 0;
                }, 5000), "Cancelled device worker did not finish.");
                run.DrainUi();
                Assert.AreEqual(0, routes);
                Assert.AreEqual(0, run.Spoken.Count);
            }
        }

        [DataTestMethod]
        [DataRow(false, "success")]
        [DataRow(true, "partial")]
        [DataRow(false, "failed")]
        [DataRow(true, "no-target")]
        public void CloseSpeechMatchesObservedRemainingProcesses(bool all, string outcome)
        {
            using (var run = new ActionRun())
            {
                var alive = new HashSet<int>(outcome == "no-target" ? new int[0] : new[] { TargetPid, SecondPid });
                run.Callbacks.GetSandboxPids = name => alive.ToArray();
                run.Callbacks.GetAllSandboxPids = () => alive.ToArray();
                run.Callbacks.PidExists = pid => alive.Contains(pid);
                run.Callbacks.RunProcessCommand = args =>
                {
                    if (outcome == "success") alive.Clear();
                    else if (outcome == "partial") alive.Remove(TargetPid);
                };

                run.Execute(() => all ? run.Service.OnCloseAllSandboxes(run.Callbacks)
                    : run.Service.OnCloseCurrentSandbox("BoxA", run.Callbacks));

                string spoken = run.Speech(Event("close", all));
                StringAssert.Contains(spoken, all ? "进程" : "用户A");
                if (outcome == "success")
                {
                    Assert.AreEqual(0, alive.Count);
                    Assert.AreEqual("已关闭用户A进程", spoken);
                    CollectionAssert.Contains(run.Statuses.ToArray(), "已关闭当前打开的进程: 用户A");
                }
                else if (outcome == "partial")
                {
                    CollectionAssert.AreEquivalent(new[] { SecondPid }, alive.ToArray());
                    StringAssert.Contains(spoken, "部分");
                    Assert.IsFalse(spoken.Contains("已关闭全部进程"), "closed_some must not claim all processes closed.");
                }
                else if (outcome == "failed")
                {
                    Assert.AreEqual(2, alive.Count);
                    Assert.IsTrue(spoken.Contains("失败") || spoken.Contains("未关闭"), spoken);
                }
                else
                {
                    Assert.IsTrue(spoken.Contains("没有") || spoken.Contains("未找到"), spoken);
                    Assert.IsFalse(spoken.Contains("已关闭"));
                }
                Assert.AreEqual(run.CallerThread, run.SpeechThread, "Close feedback must stay on the posted UI callback.");
            }
        }

        [DataTestMethod]
        [DataRow("success")]
        [DataRow("failed")]
        [DataRow("no-target")]
        [DataRow("no-window")]
        public void LocateSpeaksOnlyTheActualTargetAndResult(string outcome)
        {
            using (var run = new ActionRun())
            {
                int activations = 0;
                run.Callbacks.EnumerateWindows = pids => outcome == "no-window"
                    ? new ProcessWindowCandidate[0]
                    : new[] { new ProcessWindowCandidate { Pid = TargetPid, Hwnd = 1001, Title = "用户A" } };
                if (outcome == "no-target") run.Callbacks.GetSandboxPids = name => new int[0];
                run.Callbacks.ActivateWindow = hwnd =>
                {
                    activations++;
                    if (outcome == "failed") throw new InvalidOperationException("isolated activation failure");
                    return true;
                };

                Assert.AreEqual(outcome == "success", run.Service.OnLocateCurrentSandbox("BoxA", run.Callbacks));

                string spoken = run.Speech("locate_current");
                StringAssert.Contains(spoken, "用户A");
                if (outcome == "success")
                {
                    Assert.AreEqual("已定位到用户A进程", spoken);
                    Assert.AreEqual(1, activations);
                    CollectionAssert.Contains(run.Statuses.ToArray(), "已定位到当前打开的进程: 用户A");
                }
                else
                {
                    Assert.IsFalse(spoken.Contains("已定位"));
                    if (outcome == "failed") StringAssert.Contains(spoken, "失败");
                    else
                    {
                        Assert.AreEqual(0, activations);
                        Assert.IsTrue(spoken.Contains("没有") || spoken.Contains("未找到"), spoken);
                    }
                }
            }
        }

        private static ProcessAudioSession Audio(int pid, double volume)
        {
            return new ProcessAudioSession { Pid = pid, HasVolume = true, MasterVolume = volume };
        }

        private static string Subject(bool all) { return all ? "全部进程" : "用户A"; }
        private static string Event(string action, bool all) { return action + (all ? "_all" : "_current"); }

        private static bool RunAudioAction(ActionRun run, string action, bool all)
        {
            switch (action)
            {
                case "mute": return all ? run.Service.OnMuteAllSandboxes(run.State, run.Callbacks)
                    : run.Service.OnMuteCurrentSandbox("BoxA", run.State, run.Callbacks);
                case "volume": return all ? run.Service.OnAdjustVolumeAllSandboxes(5, run.State, run.Callbacks)
                    : run.Service.OnAdjustVolumeCurrentSandbox("BoxA", 5, run.State, run.Callbacks);
                case "switch_output": return all ? run.Service.OnSwitchOutputDeviceAllSandboxes(run.Callbacks)
                    : run.Service.OnSwitchOutputDeviceCurrentSandbox("BoxA", run.Callbacks);
                default: throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        private sealed class ActionRun : IDisposable
        {
            private readonly ConcurrentQueue<Action> _ui = new ConcurrentQueue<Action>();
            public readonly ProcessActionService Service = new ProcessActionService();
            public readonly ProcessActionState State = new ProcessActionState { RememberVolume = false };
            public readonly ConcurrentQueue<string> Statuses = new ConcurrentQueue<string>();
            public readonly ConcurrentQueue<Tuple<string, string>> Spoken = new ConcurrentQueue<Tuple<string, string>>();
            public readonly int CallerThread = Thread.CurrentThread.ManagedThreadId;
            public readonly ProcessActionCallbacks Callbacks;
            public int SpeechThread;

            public ActionRun()
            {
                Callbacks = new ProcessActionCallbacks
                {
                    ResolveSandboxNames = name => Tuple.Create("BoxA", "用户A"),
                    GetSandboxPids = name => new[] { TargetPid },
                    GetAllSandboxPids = () => new[] { TargetPid },
                    GetAudioSessions = () => new[] { Audio(TargetPid, 0.5) },
                    ListOutputDevices = () => new[] { new ProcessAudioDevice { Id = "test-headphones", Name = "耳机" } },
                    ChooseOutputDevice = (title, devices) => devices[0],
                    SetOutputDevice = (pid, device) => true,
                    GetSandboxieStartExecutable = () => string.Empty,
                    GetOfficialSandboxNames = () => new string[0],
                    ScanSandboxNames = () => new string[0],
                    PostToUi = action => _ui.Enqueue(action),
                    SetStatus = text => Statuses.Enqueue(text),
                    Speak = (text, key) =>
                    {
                        SpeechThread = Thread.CurrentThread.ManagedThreadId;
                        Spoken.Enqueue(Tuple.Create(key, text));
                    },
                    IsWindowClosing = () => false
                };
            }

            public void Execute(Func<bool> action, bool accepted = true)
            {
                Assert.AreEqual(accepted, action());
                Assert.IsTrue(SpinWait.SpinUntil(() =>
                {
                    DrainUi();
                    return Statuses.Any(text => !text.StartsWith("正在", StringComparison.Ordinal));
                }, 5000), "Operation did not report its final status.");
            }

            public void DrainUi()
            {
                Action action;
                while (_ui.TryDequeue(out action)) action();
            }

            public string Speech(string eventKey)
            {
                Tuple<string, string>[] spoken = Spoken.ToArray();
                Assert.AreEqual(1, spoken.Length, "Expected one completed-operation announcement: "
                    + string.Join(" | ", spoken.Select(item => item.Item1 + ":" + item.Item2)));
                Assert.AreEqual(eventKey, spoken[0].Item1);
                return spoken[0].Item2;
            }

            public void Dispose() { Service.Dispose(); }
        }
    }
}
