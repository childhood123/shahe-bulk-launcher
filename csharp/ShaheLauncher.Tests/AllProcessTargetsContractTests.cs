using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Processes;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("ProcessActions")]
    public sealed class AllProcessTargetsContractTests
    {
        private const int FirstPid = 900001;
        private const int SecondPid = 900002;
        private const int UnrelatedPid = 900003;

        [TestMethod]
        public void MergesPartialFastResultWithAllKnownUsersAndDeduplicatesPids()
        {
            var service = new ProcessActionService();
            IList<int> result = service.GetTargetPidsForAll(
                new[] { FirstPid, FirstPid, 900004, 0 },
                new[] { "BoxA", "BoxB" },
                name => name == "BoxA" ? new[] { FirstPid } : new[] { SecondPid, SecondPid });

            CollectionAssert.AreEqual(new[] { FirstPid, SecondPid, 900004 }, result.ToArray());
        }

        [TestMethod]
        public void KeepsFastPidsWhenPerUserLookupIsUnavailable()
        {
            var service = new ProcessActionService();
            CollectionAssert.AreEqual(new[] { FirstPid }, service.GetTargetPidsForAll(
                new[] { FirstPid, 0, -1 }, new[] { "BoxA" }, null).ToArray());
        }

        [TestMethod]
        public void KeepsKnownTargetsAndContinuesAfterOneUserLookupThrows()
        {
            var service = new ProcessActionService();
            IList<int> result = service.GetTargetPidsForAll(
                new[] { FirstPid }, new[] { "BoxA", "BoxB" }, name =>
                {
                    if (name == "BoxA") throw new InvalidOperationException("query failed");
                    return new[] { SecondPid };
                });
            CollectionAssert.AreEqual(new[] { FirstPid, SecondPid }, result.ToArray());
        }

        [TestMethod]
        public void EmptyFastResultStillCollectsEveryKnownUser()
        {
            var service = new ProcessActionService();
            CollectionAssert.AreEqual(new[] { FirstPid, SecondPid }, service.GetTargetPidsForAll(
                new int[0], new[] { "BoxA", "BoxB" },
                name => name == "BoxA" ? new[] { FirstPid } : new[] { SecondPid }).ToArray());
        }

        [DataTestMethod]
        [DataRow("mute")]
        [DataRow("volume-up")]
        [DataRow("volume-down")]
        [DataRow("output-device")]
        [DataRow("close")]
        public void AllActionsReachBothUsersWhenFastQueryOnlyReturnsOne(string action)
        {
            var service = new ProcessActionService();
            var state = new ProcessActionState { RememberVolume = false };
            var first = new ProcessAudioSession { Pid = FirstPid, HasVolume = true, MasterVolume = 0.5 };
            var second = new ProcessAudioSession { Pid = SecondPid, HasVolume = true, MasterVolume = 0.5 };
            var unrelated = new ProcessAudioSession { Pid = UnrelatedPid, HasVolume = true, MasterVolume = 0.5 };
            var alive = new HashSet<int> { FirstPid, SecondPid, UnrelatedPid };
            var routed = new List<int>();
            using (var done = new ManualResetEventSlim(false))
            {
                Func<string, IEnumerable<int>> perUser = name =>
                {
                    int pid = name == "BoxA" ? FirstPid : SecondPid;
                    return alive.Contains(pid) ? new[] { pid } : new int[0];
                };
                var callbacks = new ProcessActionCallbacks
                {
                    GetAllSandboxPids = () => alive.Contains(FirstPid) ? new[] { FirstPid } : new int[0],
                    ScanSandboxNames = () => new[] { "BoxA", "BoxB" },
                    GetSandboxPids = perUser,
                    GetAudioSessions = () => new[] { first, second, unrelated },
                    ResolveSandboxNames = name => Tuple.Create(name, name),
                    ListOutputDevices = () => new[] { new ProcessAudioDevice { Id = "output-1", Name = "设备" } },
                    ChooseOutputDevice = (title, devices) => devices[0],
                    SetOutputDevice = (pid, device) => { routed.Add(pid); return true; },
                    GetSandboxieStartExecutable = () => string.Empty,
                    GetOfficialSandboxNames = () => new string[0],
                    PidExists = pid => alive.Contains(pid),
                    RunProcessCommand = arguments =>
                    {
                        for (int i = 0; i + 1 < arguments.Count; i++)
                            if (arguments[i] == "/PID") alive.Remove(int.Parse(arguments[i + 1]));
                    },
                    PostToUi = callback => callback(),
                    IsWindowClosing = () => false,
                    SetStatus = text => { if (!text.StartsWith("正在", StringComparison.Ordinal)) done.Set(); }
                };

                bool accepted;
                switch (action)
                {
                    case "mute": accepted = service.OnMuteAllSandboxes(state, callbacks); break;
                    case "volume-up": accepted = service.OnAdjustVolumeAllSandboxes(5, state, callbacks); break;
                    case "volume-down": accepted = service.OnAdjustVolumeAllSandboxes(-5, state, callbacks); break;
                    case "output-device": accepted = service.OnSwitchOutputDeviceAllSandboxes(callbacks); break;
                    case "close": accepted = service.OnCloseAllSandboxes(callbacks); break;
                    default: throw new ArgumentOutOfRangeException(nameof(action));
                }
                Assert.IsTrue(accepted);
                Assert.IsTrue(done.Wait(5000), "operation did not report a final result");
                switch (action)
                {
                    case "mute":
                        Assert.IsTrue(first.IsMuted);
                        Assert.IsTrue(second.IsMuted, "the second user's audio was omitted");
                        break;
                    case "volume-up":
                    case "volume-down":
                        double expected = action == "volume-up" ? 0.55 : 0.45;
                        Assert.AreEqual(expected, first.MasterVolume, 0.0001);
                        Assert.AreEqual(expected, second.MasterVolume, 0.0001, "the second user's audio was omitted");
                        break;
                    case "output-device":
                        CollectionAssert.AreEqual(new[] { FirstPid, SecondPid }, routed.OrderBy(pid => pid).ToArray());
                        break;
                    case "close":
                        Assert.IsFalse(alive.Contains(FirstPid));
                        Assert.IsFalse(alive.Contains(SecondPid), "the second user remains running");
                        break;
                }
                Assert.AreEqual(0.5, unrelated.MasterVolume);
                Assert.IsFalse(unrelated.IsMuted);
                Assert.IsTrue(alive.Contains(UnrelatedPid));
            }
        }

        [TestMethod]
        public void EmptyAllDeviceTargetsPostTheResultToTheUiThread()
        {
            var service = new ProcessActionService();
            int callerThread = Thread.CurrentThread.ManagedThreadId;
            int resultThread = 0;
            var queued = new System.Collections.Concurrent.ConcurrentQueue<Action>();
            using (var done = new ManualResetEventSlim(false))
            {
                var callbacks = new ProcessActionCallbacks
                {
                    GetAllSandboxPids = () => new int[0],
                    PostToUi = callback => queued.Enqueue(callback),
                    SetStatus = text =>
                    {
                        if (text == "当前没有可切换设备的全部打开进程")
                        {
                            resultThread = Thread.CurrentThread.ManagedThreadId;
                            done.Set();
                        }
                    }
                };
                Assert.IsTrue(service.OnSwitchOutputDeviceAllSandboxes(callbacks));
                Assert.IsTrue(SpinWait.SpinUntil(() =>
                {
                    Action callback;
                    while (queued.TryDequeue(out callback)) callback();
                    return done.IsSet;
                }, 5000));
                Assert.AreEqual(callerThread, resultThread);
            }
        }

        [TestMethod]
        public void CollectsAllDeviceTargetsOffTheActionCallerThread()
        {
            var service = new ProcessActionService();
            int callerThread = Thread.CurrentThread.ManagedThreadId;
            int queryThread = 0;
            using (var done = new ManualResetEventSlim(false))
            {
                var callbacks = new ProcessActionCallbacks
                {
                    GetAllSandboxPids = () =>
                    {
                        queryThread = Thread.CurrentThread.ManagedThreadId;
                        return new[] { FirstPid };
                    },
                    ListOutputDevices = () => new[] { new ProcessAudioDevice { Id = "output", Name = "设备" } },
                    ChooseOutputDevice = (title, devices) => devices[0],
                    SetOutputDevice = (pid, device) => true,
                    PostToUi = callback => callback(),
                    IsWindowClosing = () => false,
                    SetStatus = text => { if (!text.StartsWith("正在", StringComparison.Ordinal)) done.Set(); }
                };
                Assert.IsTrue(service.OnSwitchOutputDeviceAllSandboxes(callbacks));
                Assert.IsTrue(done.Wait(5000));
                Assert.AreNotEqual(callerThread, queryThread, "completing process queries must not block the UI caller");
            }
        }
    }
}
