using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Speech;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Speech"), DoNotParallelize]
    public sealed class AssistSpeechQueueContractTests
    {
        [DataTestMethod]
        [DataRow("pin_current", "用户A已置顶")]
        [DataRow("refresh_users", "已刷新用户列表")]
        [DataRow("refresh_process", "已刷新进程状态")]
        public void CompletedMenuResultsReachTheSelectedSpeechBackend(string key, string message)
        {
            WithService((service, spoken) =>
            {
                using (var done = new ManualResetEventSlim())
                {
                    service.Callbacks.LoadAssistBackend = path => Backend((text, interrupt) =>
                    {
                        spoken.Add(text);
                        done.Set();
                    });
                    service.SpeakProcessAssist(message, true, key);
                    Assert.IsTrue(done.Wait(3000), "完成结果未到达辅助朗读接口: " + key);
                    CollectionAssert.AreEqual(new[] { message }, spoken.ToArray());
                }
            });
        }

        [TestMethod]
        public void DisabledMessageDoesNotSuppressTheSameResultAfterEnabling()
        {
            WithService((service, spoken) =>
            {
                using (var done = new ManualResetEventSlim())
                {
                    service.Callbacks.LoadAssistBackend = path => Backend((text, interrupt) =>
                    {
                        spoken.Add(text);
                        done.Set();
                    });
                    service.SaveSpeechAssistHints(Hints(false));
                    service.SpeakProcessAssist("用户A已静音", true, "mute_current");
                    service.SaveSpeechAssistHints(Hints(true));
                    service.SpeakProcessAssist("用户A已静音", true, "mute_current");
                    Assert.IsTrue(done.Wait(3000), "重新启用后，同一结果不应被关闭时的去重记录丢弃");
                    CollectionAssert.AreEqual(new[] { "用户A已静音" }, spoken.ToArray());
                }
            });
        }

        [TestMethod]
        public void RapidVolumeChangesDiscardQueuedOldLevelsAndKeepOtherResultsInOrder()
        {
            WithService((service, spoken) =>
            {
                ObserveQueuedRequests(service, () =>
                {
                    service.SpeakProcessAssist("用户A音量调到55%", true, "volume_current");
                    service.SpeakProcessAssist("用户A音量调到60%", true, "volume_current");
                    service.SpeakProcessAssist("用户A音量调到65%", true, "volume_current");
                    service.SpeakProcessAssist("用户A已静音", true, "mute_current");
                });
                CollectionAssert.AreEqual(new[] { "用户A音量调到65%", "用户A已静音" }, spoken.ToArray());
            });
        }

        [TestMethod]
        public void DisablingThenReenablingDoesNotReviveQueuedOldResults()
        {
            WithService((service, spoken) =>
            {
                ObserveQueuedRequests(service, () =>
                {
                    service.SpeakProcessAssist("用户A已静音", true, "mute_current");
                    service.SaveSpeechAssistHints(Hints(false));
                    service.SaveSpeechAssistHints(Hints(true));
                    service.SpeakProcessAssist("用户B已静音", true, "mute_current");
                });
                CollectionAssert.AreEqual(new[] { "用户B已静音" }, spoken.ToArray());
            });
        }

        [TestMethod]
        public void DisablingBeforeQueuedOutputSuppressesItWithoutLoadingAnInterface()
        {
            WithService((service, spoken) =>
            {
                int loads = 0;
                service.Callbacks.LoadAssistBackend = path =>
                {
                    loads++;
                    return Backend((text, interrupt) => spoken.Add(text));
                };
                ObserveQueuedRequests(service, () =>
                {
                    service.SpeakProcessAssist("用户A已静音", true, "mute_current");
                    service.SaveSpeechAssistHints(Hints(false));
                });
                Assert.AreEqual(0, loads);
                Assert.AreEqual(0, spoken.Count);
            });
        }

        [DataTestMethod]
        [DataRow("disable")]
        [DataRow("toggle")]
        [DataRow("volume")]
        [DataRow("closing")]
        public void AResultInvalidatedWhileTheInterfaceLoadsIsNeverOutput(string change)
        {
            WithService((service, spoken) =>
            {
                int closing = 0;
                using (var loading = new ManualResetEventSlim())
                using (var release = new ManualResetEventSlim())
                using (var lastResult = new ManualResetEventSlim())
                {
                    service.Callbacks.IsWindowClosing = () => Volatile.Read(ref closing) != 0;
                    service.Callbacks.LoadAssistBackend = path => new SpeechAssistBackend
                    {
                        Load = () => { loading.Set(); if (!release.Wait(5000)) throw new TimeoutException(); },
                        IsLoaded = () => true,
                        Output = (text, interrupt) => { spoken.Add(text); lastResult.Set(); }
                    };
                    service.SpeakProcessAssist("用户A音量调到55%", true, "volume_current");
                    try
                    {
                        Assert.IsTrue(loading.Wait(3000), "backend load did not start");
                        if (change == "closing") Interlocked.Exchange(ref closing, 1);
                        else if (change == "volume")
                            service.SpeakProcessAssist("用户A音量调到65%", true, "volume_current");
                        else
                        {
                            service.SaveSpeechAssistHints(Hints(false));
                            if (change == "toggle") service.SaveSpeechAssistHints(Hints(true));
                        }
                    }
                    finally { release.Set(); }

                    WaitForQueue(service);
                    if (change == "volume")
                    {
                        Assert.IsTrue(lastResult.Wait(3000));
                        CollectionAssert.AreEqual(new[] { "用户A音量调到65%" }, spoken.ToArray());
                    }
                    else Assert.AreEqual(0, spoken.Count, "Invalidation during native load must suppress the pending result");
                }
            });
        }

        private static void ObserveQueuedRequests(SpeechService service, Action enqueue)
        {
            // Hold the existing native-call lock to model an interface which is busy speaking.
            object callLock = typeof(SpeechService).GetField("_assistSpeechCallLock",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service);
            lock (callLock) enqueue();
            WaitForQueue(service);
        }

        private static void WaitForQueue(SpeechService service)
        {
            var pending = (Task)typeof(SpeechService).GetField("_assistSpeechTask",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service);
            Assert.IsTrue(pending.Wait(5000), "辅助朗读队列未完成");
        }

        private static IDictionary<string, object> Hints(bool enabled)
        {
            return new Dictionary<string, object> { { "enabled", enabled } };
        }

        private static SpeechAssistBackend Backend(Action<string, bool> output)
        {
            return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true, Output = output };
        }

        private static void WithService(Action<SpeechService, List<string>> action)
        {
            string root = Path.Combine(Path.GetTempPath(), "shahe-assist-results-" + Guid.NewGuid().ToString("N"));
            string oldPath = Environment.GetEnvironmentVariable("PATH");
            Directory.CreateDirectory(Path.Combine(root, "lib"));
            File.WriteAllText(Path.Combine(root, "lib", "Tolk.dll"), "test backend boundary");
            var spoken = new List<string>();
            try
            {
                using (var service = new SpeechService(root, Path.Combine(root, "Configurations"),
                    new SpeechServiceCallbacks
                    {
                        EnumerateProcessNames = () => new string[0],
                        LoadAssistBackend = path => Backend((text, interrupt) => spoken.Add(text))
                    })) action(service, spoken);
            }
            finally
            {
                Environment.SetEnvironmentVariable("PATH", oldPath);
                Directory.Delete(root, true);
            }
        }
    }
}
