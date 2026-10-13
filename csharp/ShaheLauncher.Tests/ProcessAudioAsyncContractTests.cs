using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Processes;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("AudioBackend")]
    [TestCategory("ProcessActions")]
    public sealed class ProcessAudioAsyncContractTests
    {
        [TestMethod]
        public void OutputDeviceEnumerationDoesNotBlockTheCallingThread()
        {
            var service = new ProcessActionService();
            using (var enumerationStarted = new ManualResetEventSlim(false))
            using (var releaseEnumeration = new ManualResetEventSlim(false))
            using (var callReturned = new ManualResetEventSlim(false))
            using (var routeCompleted = new ManualResetEventSlim(false))
            {
                int enumerationThread = 0;
                int choiceThread = 0;
                var callbacks = new ProcessActionCallbacks
                {
                    GetSandboxPids = _ => new[] { Process.GetCurrentProcess().Id },
                    ResolveSandboxNames = _ => Tuple.Create("BoxA", "BoxA"),
                    ListOutputDevices = () =>
                    {
                        enumerationThread = Thread.CurrentThread.ManagedThreadId;
                        enumerationStarted.Set();
                        releaseEnumeration.Wait(2000);
                        return new[]
                        {
                            new ProcessAudioDevice
                            {
                                Id = "device-1",
                                Name = "设备一",
                                IsDefault = true
                            }
                        };
                    },
                    ChooseOutputDevice = (_, devices) =>
                    {
                        choiceThread = Thread.CurrentThread.ManagedThreadId;
                        return devices[0];
                    },
                    SetOutputDevice = (_, __) =>
                    {
                        routeCompleted.Set();
                        return true;
                    },
                    PostToUi = action => action(),
                    SetStatus = _ => { },
                    RefreshProcessUi = _ => { },
                    Speak = (_, __) => { },
                    IsWindowClosing = () => false
                };

                bool returned = false;
                var caller = new Thread(() =>
                {
                    returned = service.OnSwitchOutputDeviceCurrentSandbox("BoxA", callbacks);
                    callReturned.Set();
                });
                caller.Start();
                try
                {
                    Assert.IsTrue(callReturned.Wait(500),
                        "device enumeration must not block the action caller");
                    Assert.IsTrue(returned);
                    Assert.IsTrue(enumerationStarted.Wait(1000));
                    Assert.AreNotEqual(caller.ManagedThreadId, enumerationThread);
                    releaseEnumeration.Set();
                    Assert.IsTrue(routeCompleted.Wait(2000));
                    Assert.AreNotEqual(caller.ManagedThreadId, choiceThread);
                }
                finally
                {
                    releaseEnumeration.Set();
                    caller.Join(3000);
                }
            }
        }

        [TestMethod]
        public void OutputDeviceChoiceWaitsForTheUserWithoutAFixedDeadline()
        {
            string root = FindRepositoryRoot();
            string source = File.ReadAllText(Path.Combine(
                root,
                "csharp",
                "ShaheLauncher.Core",
                "Processes",
                "ProcessActionService.cs"));
            int start = source.IndexOf(
                "private ProcessAudioDevice PromptOutputDeviceChoiceFromWorker",
                StringComparison.Ordinal);
            int end = source.IndexOf("public long PromptWindowChoice", start, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0 && end > start, "output-device choice worker must exist");
            string method = source.Substring(start, end - start);
            StringAssert.Contains(method, "while (!choiceCompleted.Wait(100))");
            StringAssert.Contains(method, "IsClosing(callbacks)");
            Assert.IsFalse(
                method.Contains("DateTime.UtcNow"),
                "published builds must not discard a valid device choice because the user needed more than 30 seconds");
        }

        private static string FindRepositoryRoot()
        {
            foreach (string start in new[] { Environment.CurrentDirectory, AppDomain.CurrentDomain.BaseDirectory })
            {
                DirectoryInfo directory = new DirectoryInfo(start);
                while (directory != null)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "PROJECT_STRUCTURE.md")))
                    {
                        return directory.FullName;
                    }
                    directory = directory.Parent;
                }
            }
            Assert.Fail("Repository root not found");
            return string.Empty;
        }
    }
}
