using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ShaheLauncher.App.UI.Dialogs.Settings;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Speech;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Speech"), DoNotParallelize]
    public sealed class SpeechSelectionRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow("", "Tolk.dll")]
        [DataRow("nvda", "nvdaControllerClient64.dll")]
        [DataRow("zdsr", "ZDSRAPI_x64.dll")]
        [DataRow("boy", "byctrl-x64.dll")]
        public void SelectedReaderDeterminesTheLibraryUsedForSpeech(string selection, string expectedLibrary)
        {
            WithLibraries(root =>
            {
                string loaded = string.Empty;
                string spoken = string.Empty;
                using (var output = new ManualResetEventSlim())
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = () => new[] { "nvda", "zdsr", "boy" },
                    LoadAssistBackend = path =>
                    {
                        loaded = path;
                        return new SpeechAssistBackend
                        {
                            Load = () => { }, IsLoaded = () => true,
                            Output = (text, interrupt) => { spoken = text; output.Set(); }
                        };
                    }
                }))
                {
                    Assert.IsTrue(service.SaveSpeechInterfaceDll(selection));
                    service.SpeakAssistText("指定接口朗读测试", false);
                    Assert.IsTrue(output.Wait(TimeSpan.FromSeconds(3)));
                    Assert.AreEqual(Path.Combine(root, "lib", expectedLibrary), loaded);
                    Assert.AreEqual("指定接口朗读测试", spoken);
                }
            });
        }

        [TestMethod]
        public void ChangingSelectionReleasesOldBackendAndCanReturnToAutomaticMode()
        {
            WithLibraries(root =>
            {
                var loaded = new List<string>();
                var unloaded = new List<string>();
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = () => new[] { "nvda", "zdsr", "boy" },
                    LoadAssistBackend = path =>
                    {
                        string name = Path.GetFileName(path);
                        loaded.Add(name);
                        return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true,
                            Unload = () => unloaded.Add(name) };
                    }
                }))
                {
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    service.SaveSpeechInterfaceDll("zdsr");
                    CollectionAssert.AreEqual(new[] { "Tolk.dll" }, unloaded);
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    service.SaveSpeechInterfaceDll("zdsr");
                    Assert.AreEqual(1, unloaded.Count, "Saving the same ready reader must keep its connection.");
                    service.SaveSpeechInterfaceDll("");
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll", "ZDSRAPI_x64.dll", "Tolk.dll" }, loaded);
                    CollectionAssert.AreEqual(new[] { "Tolk.dll", "ZDSRAPI_x64.dll" }, unloaded);
                }
            });
        }

        [TestMethod]
        public void MissingSelectedLibraryUsesAutomaticWithoutLosingChoice()
        {
            WithLibraries(root =>
            {
                File.Delete(Path.Combine(root, "lib", "nvdaControllerClient64.dll"));
                var loaded = new List<string>();
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = () => new[] { "nvda", "zdsr", "boy" },
                    LoadAssistBackend = path =>
                    {
                        loaded.Add(Path.GetFileName(path));
                        return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true };
                    }
                }))
                {
                    service.SaveSpeechInterfaceDll("nvda");
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll" }, loaded);
                    Assert.AreEqual("nvda", service.SpeechInterfaceDll);
                    service.SaveSpeechInterfaceDll("");
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll", "Tolk.dll" }, loaded);
                    service.ShutdownAssistSpeechRuntime();
                    service.SaveSpeechInterfaceDll("zdsr");
                    Assert.IsFalse(service.EnsureAssistSpeechRuntime(), "Saving after shutdown must not reopen native handles.");
                }
            });
        }

        [TestMethod]
        public void QueuedSpeechRechecksEnabledSettingBeforeLoadingReader()
        {
            WithLibraries(root =>
            {
                int loads = 0;
                int closingChecks = 0;
                using (var workerEntered = new ManualResetEventSlim())
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    IsWindowClosing = () => { if (Interlocked.Increment(ref closingChecks) > 1) workerEntered.Set(); return false; },
                    EnumerateProcessNames = () => new[] { "nvda", "zdsr", "boy" },
                    LoadAssistBackend = path => { Interlocked.Increment(ref loads); return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true }; }
                }))
                {
                    object callLock = typeof(SpeechService).GetField("_assistSpeechCallLock", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service);
                    lock (callLock)
                    {
                        service.SpeakAssistText("关闭提示前排队的消息", false);
                        service.SaveSpeechAssistHints(new Dictionary<string, object> { ["enabled"] = false });
                    }
                    Assert.IsTrue(workerEntered.Wait(TimeSpan.FromSeconds(3)));
                    lock (callLock)
                        Assert.AreEqual(0, loads, "Queued messages must not reopen/output after hints are disabled.");
                }
            });
        }

        [DataTestMethod]
        [DataRow("nvda", "nvdaControllerClient64.dll")]
        [DataRow("zdsr", "ZDSRAPI_x64.dll")]
        [DataRow("boy", "byctrl-x64.dll")]
        public void NoReadersFallsBackToWindowsAndReturnsWhenSelectedReaderStarts(string selection, string library)
        {
            WithLibraries(root =>
            {
                string[] processes = new string[0];
                var loaded = new List<string>();
                var preferredWindows = new List<bool>();
                int unloaded = 0;
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = () => processes,
                    LoadAssistBackend = path =>
                    {
                        loaded.Add(Path.GetFileName(path));
                        return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true,
                            PreferSapi = value => preferredWindows.Add(value), Unload = () => unloaded++ };
                    }
                }))
                {
                    service.SaveSpeechInterfaceDll(selection);
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll" }, loaded);
                    Assert.AreEqual(true, preferredWindows[0], "No running readers must prefer Windows speech.");
                    processes = new[] { selection };
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll", library }, loaded);
                    Assert.AreEqual(1, unloaded);
                    processes = new string[0];
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll", library, "Tolk.dll" }, loaded);
                    Assert.AreEqual(2, unloaded);
                    Assert.AreEqual(selection, service.SpeechInterfaceDll, "Fallback must preserve the saved choice.");
                }
            });
        }

        [DataTestMethod]
        [DataRow("nvda", "nvdaControllerClient64.dll", "zdsr")]
        [DataRow("zdsr", "ZDSRAPI_x64.dll", "nvda")]
        [DataRow("boy", "byctrl-x64.dll", "nvda")]
        public void UnavailableSelectionUsesAutomaticTemporarilyWithoutChangingSavedChoice(string selected, string library, string other)
        {
            WithLibraries(root =>
            {
                string[] processes = new[] { other };
                var loaded = new List<string>();
                int saves = 0;
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = () => processes,
                    SaveSettings = () => saves++,
                    LoadAssistBackend = path =>
                    {
                        loaded.Add(Path.GetFileName(path));
                        return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true };
                    }
                }))
                {
                    service.SaveSpeechInterfaceDll(selected);
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll" }, loaded);
                    Assert.AreEqual(selected, service.SpeechInterfaceDll);
                    processes = new[] { other, selected };
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    CollectionAssert.AreEqual(new[] { "Tolk.dll", library }, loaded);
                    Assert.AreEqual(selected, service.SpeechInterfaceDll);
                    Assert.AreEqual(1, saves, "Runtime fallback/recovery must never save an automatic selection.");
                }
            });
        }

        [TestMethod]
        public void ReaderApiBecomingUnavailableFallsBackEvenIfProcessNameStillExists()
        {
            WithLibraries(root =>
            {
                bool available = true;
                var loaded = new List<string>();
                using (var service = new SpeechService(root, Path.Combine(root, "Configuration"), new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = () => new[] { "nvda", "zdsr" },
                    LoadAssistBackend = path =>
                    {
                        string name = Path.GetFileName(path);
                        loaded.Add(name);
                        return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => name == "Tolk.dll" || available };
                    }
                }))
                {
                    service.SaveSpeechInterfaceDll("nvda");
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    available = false;
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    Assert.AreEqual("Tolk.dll", loaded[loaded.Count - 1]);
                    Assert.AreEqual("nvda", service.SpeechInterfaceDll);
                    available = true;
                    Assert.IsTrue(service.EnsureAssistSpeechRuntime());
                    Assert.AreEqual("nvdaControllerClient64.dll", loaded[loaded.Count - 1]);
                }
            });
        }

        [TestMethod]
        public void ReaderSelectionFromSettingsDialogPersistsAndReloadsIntoRuntime()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string lib = Path.Combine(root, "lib");
                Directory.CreateDirectory(lib);
                File.WriteAllBytes(Path.Combine(lib, "Tolk.dll"), new byte[] { 0 });
                File.WriteAllBytes(Path.Combine(lib, "ZDSRAPI_x64.dll"), new byte[] { 0 });
                string loaded = string.Empty;
                form.SpeechService.Callbacks.EnumerateProcessNames = () => new[] { "zdsr" };
                form.SpeechService.Callbacks.LoadAssistBackend = path =>
                {
                    loaded = path;
                    return new SpeechAssistBackend { Load = () => { }, IsLoaded = () => true };
                };
                form.SpeechService.Callbacks.ShowSettings = request =>
                {
                    using (var dialog = new SpeechSettingsDialog(form, request.SpeechConfig, request.SaveSpeechConfig,
                        request.ReaderIniSettings, request.SaveReaderIniSettings, null, request.InterfaceDllChoices,
                        request.SelectedInterfaceDll, request.SaveInterfaceDll, request.AssistHints, request.SaveAssistHints))
                    {
                        Assert.AreEqual("朗读设置", dialog.Text);
                        dialog.InterfaceChoice.SelectedIndex = 2;
                        Assert.AreEqual("zdsr", dialog.GetSelectedInterfaceValue());
                        Assert.IsTrue(dialog.ConfirmSettings());
                    }
                    return true;
                };
                Assert.IsTrue(form.SpeechService.OnSpeechSettings());
                Assert.AreEqual("zdsr", form.SettingsService.LoadSettings()["speech_interface_dll"]);
                MigrationRuntimeTestHelpers.Invoke(form, "LoadApplicationSettings");
                Assert.AreEqual("zdsr", form.SpeechService.SpeechInterfaceDll);
                Assert.IsTrue(form.SpeechService.EnsureAssistSpeechRuntime());
                Assert.AreEqual(Path.Combine(lib, "ZDSRAPI_x64.dll"), loaded);
            });
        }

        [DataTestMethod]
        [DataRow("ReaderBoolDelegate", -1, UnmanagedType.I1)]
        [DataRow("ZdsrInitializeDelegate", 2, UnmanagedType.Bool)]
        [DataRow("ZdsrSpeakDelegate", 1, UnmanagedType.Bool)]
        [DataRow("BoySpeakDelegate", 1, UnmanagedType.I1)]
        public void NativeReaderBooleanWidthsMatchTheShippedApis(string typeName, int index, UnmanagedType width)
        {
            Type type = typeof(SpeechService).GetNestedType(typeName, BindingFlags.NonPublic);
            Assert.IsNotNull(type);
            Assert.AreEqual(CallingConvention.StdCall, type.GetCustomAttribute<UnmanagedFunctionPointerAttribute>().CallingConvention);
            MethodInfo invoke = type.GetMethod("Invoke");
            ParameterInfo parameter = index < 0 ? invoke.ReturnParameter : invoke.GetParameters()[index];
            Assert.AreEqual(width, parameter.GetCustomAttribute<MarshalAsAttribute>().Value);
        }

        private static void WithLibraries(Action<string> action)
        {
            string root = Path.Combine(Path.GetTempPath(), "ShaheSpeechSelection-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "lib"));
            try
            {
                foreach (string name in new[] { "Tolk.dll", "nvdaControllerClient64.dll", "ZDSRAPI_x64.dll", "byctrl-x64.dll" })
                    File.WriteAllBytes(Path.Combine(root, "lib", name), new byte[] { 0 });
                action(root);
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
