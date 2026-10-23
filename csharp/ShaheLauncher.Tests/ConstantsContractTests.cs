using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Constants")]
    public sealed class ConstantsContractTests
    {
        private const string ConstantsTypeName = "ShaheLauncher.Core.Constants.LauncherConstants";
        private static readonly object EnvironmentLock = new object();

        [TestMethod]
        public void ExposesExactScalarConstantsAndChoices()
        {
            Type type = RequireConstantsType();

            Assert.AreEqual(@"C:\Sandbox\Administrator", GetField(type, "OldDefaultSandboxRoot"));
            Assert.AreEqual("4.0.47", GetField(type, "AppVersion"));
            Assert.AreEqual("https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/raw/main/update.json", GetField(type, "UpdateManifestUrl"));
            Assert.AreEqual("off", GetField(type, "LogOutputOff"));
            Assert.AreEqual("detailed", GetField(type, "LogOutputDetailed"));

            CollectionAssert.AreEqual(
                new[] { "off=关闭日志输出", "detailed=详细日志输出" },
                ReadPairs(GetField(type, "LogOutputChoices")));
        }

        [TestMethod]
        public void ExposesExactFixedHotkeyMaps()
        {
            Type type = RequireConstantsType();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "select_all=Ctrl+A",
                    "open_app=Ctrl+O",
                    "paste_app=Ctrl+V",
                    "refresh=F5",
                    "minimize_to_tray=Esc",
                    "exit_app=Alt+F4"
                },
                ReadPairs(GetField(type, "FixedHotkeyTextByAction")));
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "select_all=全选",
                    "open_app=选择应用",
                    "paste_app=粘贴 .exe 或文件路径",
                    "refresh=刷新沙盒用户",
                    "minimize_to_tray=最小化到托盘",
                    "exit_app=退出软件"
                },
                ReadPairs(GetField(type, "FixedHotkeyLabelByAction")));
        }

        [TestMethod]
        public void NormalizesLogOutputModeExactlyLikePython()
        {
            Assert.AreEqual("off", Invoke("NormalizeLogOutputMode", "关闭日志输出"));
            Assert.AreEqual("off", Invoke("NormalizeLogOutputMode", " off "));
            Assert.AreEqual("detailed", Invoke("NormalizeLogOutputMode", "详细日志输出"));
            Assert.AreEqual("detailed", Invoke("NormalizeLogOutputMode", (object)null));
            Assert.AreEqual("detailed", Invoke("NormalizeLogOutputMode", "unexpected"));
        }

        [TestMethod]
        public void BuildsDefaultSandboxRootFromEnvironment()
        {
            lock (EnvironmentLock)
            {
                string oldDrive = Environment.GetEnvironmentVariable("SystemDrive");
                string oldUserName = Environment.GetEnvironmentVariable("USERNAME");
                string oldUser = Environment.GetEnvironmentVariable("USER");
                try
                {
                    Environment.SetEnvironmentVariable("SystemDrive", "D:\\");
                    Environment.SetEnvironmentVariable("USERNAME", " TestUser ");
                    Environment.SetEnvironmentVariable("USER", "IgnoredUser");
                    Assert.AreEqual(@"D:\Sandbox\TestUser", Invoke("GetDefaultSandboxRoot"));

                    Environment.SetEnvironmentVariable("SystemDrive", null);
                    Environment.SetEnvironmentVariable("USERNAME", " ");
                    Environment.SetEnvironmentVariable("USER", null);
                    Assert.AreEqual(@"C:\Sandbox\Administrator", Invoke("GetDefaultSandboxRoot"));
                }
                finally
                {
                    Environment.SetEnvironmentVariable("SystemDrive", oldDrive);
                    Environment.SetEnvironmentVariable("USERNAME", oldUserName);
                    Environment.SetEnvironmentVariable("USER", oldUser);
                }
            }
        }

        [TestMethod]
        public void CalculatesLaunchConcurrencyMaximumAndConservativeDefault()
        {
            int expectedMaximum = Math.Max(1, Math.Min(32, Environment.ProcessorCount));
            int expectedDefault = Math.Min(
                expectedMaximum,
                Environment.ProcessorCount < 4 ? 2 : 4);
            Assert.AreEqual(expectedMaximum, Invoke("GetLaunchConcurrencyMaximum"));
            Assert.AreEqual(expectedDefault, Invoke("GetDefaultLaunchConcurrencyLimit"));
        }

        [TestMethod]
        public void NormalizesSandboxRootPaths()
        {
            Assert.AreEqual(string.Empty, Invoke("NormalizeSandboxRootPath", "  "));
            string relative = Path.Combine("relative", "box");
            Assert.AreEqual(Path.GetFullPath(relative), Invoke("NormalizeSandboxRootPath", relative));
        }

        [TestMethod]
        public void ValidatesInternalSandboxNamesWithExactMessages()
        {
            Assert.AreEqual("内部沙盒名不能为空", Invoke("GetInternalSandboxNameValidationError", "  "));
            Assert.AreEqual("内部沙盒名不能超过 32 个字符", Invoke("GetInternalSandboxNameValidationError", new string('A', 33)));
            Assert.AreEqual("内部沙盒名不能使用 Sandboxie 保留分节名", Invoke("GetInternalSandboxNameValidationError", "GlobalSettings"));
            Assert.AreEqual("内部沙盒名不能使用 Sandboxie 保留分节名", Invoke("GetInternalSandboxNameValidationError", "UserSettings_ABC"));
            Assert.AreEqual("内部沙盒名不能使用 Windows 设备保留名", Invoke("GetInternalSandboxNameValidationError", "COM1"));
            Assert.AreEqual("内部沙盒名只能使用英文字母、数字和下划线", Invoke("GetInternalSandboxNameValidationError", "测试"));
            Assert.AreEqual("内部沙盒名只能使用英文字母、数字和下划线", Invoke("GetInternalSandboxNameValidationError", string.Concat(Enumerable.Repeat("😀", 20))));
            Assert.AreEqual(string.Empty, Invoke("GetInternalSandboxNameValidationError", "Box_01"));
            Assert.AreEqual(true, Invoke("IsValidInternalSandboxName", "Box_01"));
            Assert.AreEqual(false, Invoke("IsValidInternalSandboxName", "Box-01"));
        }

        [TestMethod]
        public void ValidatesDisplayNamesWithExactMessages()
        {
            Assert.AreEqual("沙盒名不能为空", Invoke("GetSandboxDisplayNameValidationError", (object)null));
            Assert.AreEqual("沙盒名过长", Invoke("GetSandboxDisplayNameValidationError", new string('名', 241)));
            Assert.AreEqual("沙盒名过长", Invoke("GetSandboxDisplayNameValidationError", string.Concat(Enumerable.Repeat("😀", 241))));
            Assert.AreEqual("沙盒名不能包含英文逗号、括号或控制字符", Invoke("GetSandboxDisplayNameValidationError", "Box,1"));
            Assert.AreEqual(string.Empty, Invoke("GetSandboxDisplayNameValidationError", "沙盒用户 1"));
        }

        [TestMethod]
        public void BuildsDefaultDisplayPrefixExactlyLikePython()
        {
            Assert.AreEqual("沙盒用户", Invoke("MakeDefaultSandboxDisplayPrefix", string.Empty, string.Empty));
            Assert.AreEqual("盒", Invoke("MakeDefaultSandboxDisplayPrefix", " 盒,()12 ", "备用"));
            Assert.AreEqual("备用", Invoke("MakeDefaultSandboxDisplayPrefix", "123", "备用"));

            string longPrefix = string.Concat(Enumerable.Repeat("😀", 81));
            string expected = string.Concat(Enumerable.Repeat("😀", 80));
            Assert.AreEqual(expected, Invoke("MakeDefaultSandboxDisplayPrefix", longPrefix, "备用"));
        }

        private static Type RequireConstantsType()
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(ConstantsTypeName, false);
            Assert.IsNotNull(type, ConstantsTypeName + " must exist");
            return type;
        }

        private static object GetField(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be a public static field");
            return field.GetValue(null);
        }

        private static object Invoke(string name, params object[] args)
        {
            Type type = RequireConstantsType();
            MethodInfo method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(candidate => candidate.Name == name && candidate.GetParameters().Length == args.Length);
            Assert.IsNotNull(method, name + " must be a public static method");
            return method.Invoke(null, args);
        }

        private static string[] ReadPairs(object source)
        {
            var result = new List<string>();
            foreach (object item in (IEnumerable)source)
            {
                Type itemType = item.GetType();
                object key = itemType.GetProperty("Key").GetValue(item, null);
                object value = itemType.GetProperty("Value").GetValue(item, null);
                result.Add(key + "=" + value);
            }

            return result.ToArray();
        }
    }
}
