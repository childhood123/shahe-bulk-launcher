using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Processes;
using ShaheLauncher.Core.Runtime;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("AudioBackend")]
    [TestCategory("ProcessActions")]
    public sealed class WindowsProcessAudioBackendContractTests
    {
        private const string BackendTypeName = "ShaheLauncher.Core.Processes.WindowsProcessAudioBackend";

        [TestMethod]
        public void MainFormBindsProcessCallbacksToTheWindowsAudioBackend()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    PropertyInfo property = typeof(MainForm).GetProperty(
                        "ProcessAudioBackend",
                        BindingFlags.Public | BindingFlags.Instance);
                    Assert.IsNotNull(property, "MainForm must compose the Windows audio backend");

                    object backend = property.GetValue(form);
                    Assert.IsNotNull(backend);
                    Assert.AreEqual(BackendTypeName, backend.GetType().FullName);
                    AssertCallbackTarget(form.ProcessCallbacks.GetAudioSessions, backend, "GetAudioSessions");
                    AssertCallbackTarget(form.ProcessCallbacks.ListOutputDevices, backend, "ListOutputDevices");
                    AssertCallbackTarget(form.ProcessCallbacks.SetOutputDevice, backend, "SetOutputDevice");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void MapsPidSessionsToReadableAndWritableVolumeDelegates()
        {
            Type backendType = RequireBackendType();
            MethodInfo factory = RequireMethod(backendType, "CreateSession");
            bool muted = false;
            double volume = 0.25;

            var session = (ProcessAudioSession)factory.Invoke(null, new object[]
            {
                42,
                new Func<bool>(() => muted),
                new Action<bool>(value => muted = value),
                new Func<double>(() => volume),
                new Action<double>(value => volume = value)
            });

            Assert.AreEqual(42, session.Pid);
            Assert.IsTrue(session.HasVolume);
            Assert.AreEqual(false, session.GetMute());
            Assert.AreEqual(0.25, session.GetMasterVolume(), 0.0001);
            session.SetMute(true);
            session.SetMasterVolume(0.75);
            Assert.IsTrue(muted);
            Assert.AreEqual(0.75, volume, 0.0001);
        }

        [TestMethod]
        public void NormalizesOutputDevicesAndWindowsAudioPolicyIdentifiers()
        {
            Type backendType = RequireBackendType();
            MethodInfo deviceFactory = RequireMethod(backendType, "CreateDevice");
            var device = (ProcessAudioDevice)deviceFactory.Invoke(
                null,
                new object[] { "device-id", "扬声器", true });
            Assert.AreEqual("device-id", device.Id);
            Assert.AreEqual("扬声器", device.Name);
            Assert.AreEqual("扬声器（默认）", device.Label);
            Assert.IsTrue(device.IsDefault);

            MethodInfo pack = RequireMethod(backendType, "PackRenderDeviceId");
            Assert.AreEqual(
                @"\\?\SWD#MMDEVAPI#device-id#{e6327cad-dcec-4949-ae8a-991e976a79d2}",
                pack.Invoke(null, new object[] { "device-id" }));

            MethodInfo resolveIid = RequireMethod(backendType, "ResolvePolicyConfigInterfaceId");
            Assert.AreEqual(Guid.Empty, resolveIid.Invoke(null, new object[] { 17133 }));
            Assert.AreEqual(
                new Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258"),
                resolveIid.Invoke(null, new object[] { 17134 }));
            Assert.AreEqual(
                new Guid("ab3d4648-e242-459f-b02f-541c70306324"),
                resolveIid.Invoke(null, new object[] { 21390 }));
        }

        [TestMethod]
        public void NativeBackendReturnsSafeCollectionsAndRejectsInvalidRoutes()
        {
            Type backendType = RequireBackendType();
            object backend = Activator.CreateInstance(backendType);
            var sessions = ((IEnumerable<ProcessAudioSession>)RequireMethod(
                backendType, "GetAudioSessions").Invoke(backend, null)).ToList();
            var devices = ((IEnumerable<ProcessAudioDevice>)RequireMethod(
                backendType, "ListOutputDevices").Invoke(backend, null)).ToList();

            Assert.IsTrue(sessions.All(session => session != null
                && session.Pid > 0
                && session.HasVolume
                && session.ReadMute != null
                && session.WriteMute != null
                && session.ReadMasterVolume != null
                && session.WriteMasterVolume != null));
            Assert.IsTrue(devices.All(device => device != null
                && !string.IsNullOrWhiteSpace(device.Id)
                && !string.IsNullOrWhiteSpace(device.Name)
                && !string.IsNullOrWhiteSpace(device.Label)));
            Assert.IsTrue(devices.Count(device => device.IsDefault) <= 1);

            MethodInfo setOutputDevice = RequireMethod(backendType, "SetOutputDevice");
            Assert.AreEqual(false, setOutputDevice.Invoke(backend, new object[] { -1, "device-id" }));
            Assert.AreEqual(false, setOutputDevice.Invoke(
                backend,
                new object[] { Process.GetCurrentProcess().Id, string.Empty }));
        }

        private static Type RequireBackendType()
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(BackendTypeName, false);
            Assert.IsNotNull(type, BackendTypeName + " must exist");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string name)
        {
            MethodInfo method = type.GetMethod(
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            Assert.IsNotNull(method, name + " must exist");
            return method;
        }

        private static void AssertCallbackTarget(Delegate callback, object backend, string methodName)
        {
            Assert.IsNotNull(callback, methodName + " callback must exist");
            Assert.AreSame(backend, callback.Target, methodName + " must target the composed backend");
            Assert.AreEqual(methodName, callback.Method.Name);
        }

        private static MainForm CreateForm(string root)
        {
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            return new MainForm(new LauncherRuntime(
                false,
                executable,
                executable,
                () => new DateTime(2026, 10, 12, 12, 0, 0)));
        }

        private static void RunInSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception caught) { error = caught; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
            {
                throw new AssertFailedException("STA action failed: " + error, error);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "shahe_audio_backend_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Destroy(MainForm form)
        {
            if (form == null)
            {
                return;
            }

            try { form.Destroy(); }
            catch
            {
                try { form.Dispose(); }
                catch { }
            }
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }
    }
}
