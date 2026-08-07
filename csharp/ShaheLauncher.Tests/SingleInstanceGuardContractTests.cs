using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("SingleInstance")]
    public sealed class SingleInstanceGuardContractTests
    {
        private const string TypeName = "ShaheLauncher.App.Infrastructure.SingleInstanceGuard";

        [TestMethod]
        public void NormalizesNameAndPreservesNotificationText()
        {
            Type type = RequireType();
            using (var guard = (IDisposable)Activator.CreateInstance(type, new object[] { "  " }))
            {
                Assert.AreEqual("SandboxLauncher", type.GetProperty("Name").GetValue(guard));
            }

            Assert.AreEqual("沙盒批量打开器", GetConstant(type, "PrimaryWindowTitle"));
            Assert.AreEqual("沙河批量打开器", GetConstant(type, "LegacyWindowTitle"));
            Assert.AreEqual("沙河批量打开器正在运行中", GetConstant(type, "RunningMessage"));
            Assert.AreEqual("提示", GetConstant(type, "NotificationTitle"));
        }

        [TestMethod]
        public void GlobalMutexAllowsOnlyOneLiveGuardAndCanBeReleased()
        {
            Type type = RequireType();
            string name = "ShaheLauncherTests_" + Guid.NewGuid().ToString("N");
            var first = (IDisposable)Activator.CreateInstance(type, name);
            var second = (IDisposable)Activator.CreateInstance(type, name);
            try
            {
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(first, "Check"));
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(second, "Check"));
                ReflectionTestHelpers.InvokeInstance(first, "Release");

                using (var third = (IDisposable)Activator.CreateInstance(type, name))
                {
                    Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(third, "Check"));
                }
            }
            finally
            {
                first.Dispose();
                second.Dispose();
            }
        }

        [TestMethod]
        public void LockFileFallbackRejectsLivePidAndReplacesStaleOrMalformedPid()
        {
            string root = Path.Combine(Path.GetTempPath(), "shahe-single-instance-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                Type type = RequireType();
                ConstructorInfo constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(candidate => candidate.GetParameters().Length == 4);
                string name = "FallbackGuard";
                string lockPath = Path.Combine(root, name + ".lock");
                File.WriteAllText(lockPath, "4321", new UTF8Encoding(false));
                object liveGuard = constructor.Invoke(
                    new object[] { name, root, Process.GetCurrentProcess().Id, new Func<int, bool>(pid => pid == 4321) });
                Assert.AreEqual(false, InvokeLockFileCheck(liveGuard));
                Assert.AreEqual("4321", File.ReadAllText(lockPath, Encoding.UTF8));

                File.WriteAllText(lockPath, "invalid", new UTF8Encoding(false));
                object staleGuard = constructor.Invoke(
                    new object[] { name, root, Process.GetCurrentProcess().Id, new Func<int, bool>(pid => false) });
                Assert.AreEqual(true, InvokeLockFileCheck(staleGuard));
                byte[] lockBytes = File.ReadAllBytes(lockPath);
                Assert.AreEqual(Process.GetCurrentProcess().Id.ToString(), Encoding.UTF8.GetString(lockBytes));
                Assert.IsFalse(lockBytes.Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }));
                ReflectionTestHelpers.InvokeInstance(staleGuard, "Release");
                Assert.IsFalse(File.Exists(lockPath));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static Type RequireType()
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(TypeName, false);
            Assert.IsNotNull(type, TypeName + " must exist");
            return type;
        }

        private static string GetConstant(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be public const");
            return (string)field.GetRawConstantValue();
        }

        private static object InvokeLockFileCheck(object target)
        {
            MethodInfo method = target.GetType().GetMethod("CheckByLockFile", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "CheckByLockFile must exist");
            return method.Invoke(target, null);
        }
    }
}
