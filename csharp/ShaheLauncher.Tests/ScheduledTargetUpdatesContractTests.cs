using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Scheduling;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Scheduling")]
    public sealed class ScheduledTargetUpdatesContractTests
    {
        [DataTestMethod]
        [DataRow(true, "rename")]
        [DataRow(false, "rename")]
        [DataRow(true, "partial-delete")]
        [DataRow(false, "partial-delete")]
        [DataRow(true, "all-delete")]
        [DataRow(false, "all-delete")]
        [DataRow(true, "cancel")]
        [DataRow(false, "cancel")]
        [DataRow(true, "retry-rename")]
        [DataRow(true, "retry-delete")]
        [DataRow(true, "retry-all-delete")]
        public void ScheduledActionsUseUpdatedMainWindowTargets(bool launch, string change)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var timers = new List<Action>();
                var disposableTimers = new List<TrackingTimer>();
                var actions = new List<string>();
                form.LaunchCallbacks.ScheduleDelayed = (delay, action) =>
                {
                    timers.Add(action);
                    var timer = new TrackingTimer();
                    disposableTimers.Add(timer);
                    return timer;
                };
                form.LaunchCallbacks.LaunchSelectedSandboxes = (targets, apps) => actions.AddRange(targets);
                form.LaunchCallbacks.CloseSandboxProcesses = target => { actions.Add(target); return "closed"; };
                form.LaunchCallbacks.RefreshProcessUi = force => { };
                form.LaunchCallbacks.DeleteActiveLaunchReferencesByDisplay = targets => { };
                string key = launch
                    ? form.LaunchScheduler.ScheduleSelectedLaunch(form.LaunchState, new[] { "A", "B" },
                        new[] { new LaunchAppTarget { Path = "isolated.exe" } }, 1, form.LaunchCallbacks)
                    : form.LaunchScheduler.ScheduleSelectedClose(form.LaunchState, new[] { "A", "B" }, 1, form.LaunchCallbacks);
                CollectionAssert.AreEqual(new[] { "A", "B" }, form.LaunchState.Jobs[key].SandboxTargets.ToArray());
                if (change.StartsWith("retry-", StringComparison.Ordinal))
                {
                    form.LaunchState.IsLaunching = true;
                    timers[0]();
                    Assert.AreEqual(0, actions.Count);
                    Assert.AreEqual(2, timers.Count);
                    form.LaunchState.IsLaunching = false;
                }

                string[] expected;
                if (change.EndsWith("rename", StringComparison.Ordinal))
                {
                    Assert.AreEqual(true, MigrationRuntimeTestHelpers.Invoke(form,
                        "RenameSandboxRefsInScheduledJobsByDisplay", new Dictionary<string, string> { { "A", "C" } }));
                    expected = new[] { "C", "B" };
                }
                else if (change == "cancel")
                {
                    Assert.IsTrue(form.LaunchScheduler.CancelScheduleTask(form.LaunchState, key, false, form.LaunchCallbacks));
                    expected = new string[0];
                }
                else
                {
                    bool all = change.EndsWith("all-delete", StringComparison.Ordinal);
                    Assert.AreEqual(true, MigrationRuntimeTestHelpers.Invoke(form,
                        "DeleteSandboxRefsInScheduledJobsByDisplay", (object)(all ? new[] { "A", "B" } : new[] { "A" })));
                    expected = all ? new string[0] : new[] { "B" };
                }
                if (expected.Length != 0)
                    CollectionAssert.AreEqual(expected, form.LaunchState.Jobs[key].SandboxTargets.ToArray());
                else
                    Assert.IsTrue(disposableTimers[disposableTimers.Count - 1].Disposed);

                // Fire even a canceled timer's queued callback to test the identity guard.
                timers[timers.Count - 1]();
                CollectionAssert.AreEqual(expected, actions.ToArray());
                Assert.IsFalse(form.LaunchState.Jobs.ContainsKey(key));
            });
        }

        private sealed class TrackingTimer : IDisposable
        {
            internal bool Disposed { get; private set; }
            public void Dispose() { Disposed = true; }
        }
    }
}
