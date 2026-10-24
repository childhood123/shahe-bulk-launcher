using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Runtime;

namespace ShaheLauncher.Tests
{
    internal static class MigrationRuntimeTestHelpers
    {
        internal static object Invoke(object target, string method, params object[] arguments)
        {
            MethodInfo member = target.GetType().GetMethod(method,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(member, method);
            return member.Invoke(target, arguments);
        }

        internal static void WithForm(Action<string, MainForm> action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                string root = Path.Combine(Path.GetTempPath(), "shahe_migration_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                try
                {
                    string executable = Path.Combine(root, "沙河批量打开器.exe");
                    using (var form = new MainForm(new LauncherRuntime(false, executable, executable, () => DateTime.Now)))
                    {
                        form.AutoRunWriter = (key, path, enabled) => { };
                        form.StartupDelayCallback = (delay, callback) => null;
                        form.SpeechService.Callbacks.ShowWarning = (text, title) => { };
                        action(root, form);
                    }
                }
                catch (Exception caught) { error = caught; }
                finally { Directory.Delete(root, true); }
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(15000), "隔离 STA 测试超时");
            if (error != null) throw new AssertFailedException("STA action failed: " + error, error);
        }
    }
}
