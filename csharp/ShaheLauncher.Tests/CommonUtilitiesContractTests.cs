using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Common")]
    public sealed class CommonUtilitiesContractTests
    {
        private const string TypeName = "ShaheLauncher.Core.Common.CommonUtilities";

        [TestMethod]
        public void ParsesBooleanAndIntegerSettingsLikePython()
        {
            Type type = ReflectionTestHelpers.RequireType(TypeName);
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", null, true));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", null, false));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", string.Empty, true));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", "开启", false));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", "关闭", true));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", "maybe", true));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "ParseBoolSetting", "maybe", false));
            Assert.AreEqual(10, ReflectionTestHelpers.InvokeStatic(type, "ParseIntSetting", null, 10, null, null));
            Assert.AreEqual(10, ReflectionTestHelpers.InvokeStatic(type, "ParseIntSetting", double.NaN, 10, null, null));
            Assert.AreEqual(10, ReflectionTestHelpers.InvokeStatic(type, "ParseIntSetting", "bad", 10, null, null));
            Assert.AreEqual(1, ReflectionTestHelpers.InvokeStatic(type, "ParseIntSetting", "-4", 10, 1, 32));
            Assert.AreEqual(32, ReflectionTestHelpers.InvokeStatic(type, "ParseIntSetting", "99", 10, 1, 32));
            Assert.AreEqual(5, ReflectionTestHelpers.InvokeStatic(type, "ParseIntSetting", "5", 10, 1, 32));
        }

        [TestMethod]
        public void ParsesAndComparesVersionsLikePython()
        {
            Type type = ReflectionTestHelpers.RequireType(TypeName);
            CollectionAssert.AreEqual(
                new[] { 4, 0, 46, 2 },
                (int[])ReflectionTestHelpers.InvokeStatic(type, "ParseVersionParts", "v4.0.46-beta2"));
            CollectionAssert.AreEqual(
                new[] { 0 },
                (int[])ReflectionTestHelpers.InvokeStatic(type, "ParseVersionParts", string.Empty));
            CollectionAssert.AreEqual(
                new[] { 1, 2 },
                (int[])ReflectionTestHelpers.InvokeStatic(type, "ParseVersionParts", "1..2"));
            Assert.AreEqual(0, ReflectionTestHelpers.InvokeStatic(type, "CompareVersions", "1.2", "1.2.0"));
            Assert.AreEqual(1, ReflectionTestHelpers.InvokeStatic(type, "CompareVersions", "4.0.46", "4.0.45"));
            Assert.AreEqual(-1, ReflectionTestHelpers.InvokeStatic(type, "CompareVersions", "4.0.45", "4.0.46"));
        }

        [TestMethod]
        public void FormatsSizesAndUpdateStatusRulesLikePython()
        {
            Type type = ReflectionTestHelpers.RequireType(TypeName);
            Assert.AreEqual("0 B", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", 0));
            Assert.AreEqual("1023 B", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", 1023));
            Assert.AreEqual("1.0 KB", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", 1024));
            Assert.AreEqual("1.5 KB", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", 1536));
            Assert.AreEqual("1.0 MB", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", 1048576));
            Assert.AreEqual("-1 B", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", -1));
            Assert.AreEqual("0 B", ReflectionTestHelpers.InvokeStatic(type, "FormatSize", "bad"));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(type, "IsAllowedUpdateBodyStatus", 404, "https://lqh.viyf.net/path"));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "IsAllowedUpdateBodyStatus", 404, "http://lqh.viyf.net/path"));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "IsAllowedUpdateBodyStatus", 404, "https://example.com/path"));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(type, "IsAllowedUpdateBodyStatus", 404, string.Empty));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(type, "IsAllowedUpdateBodyStatus", "bad", string.Empty));
        }

        [TestMethod]
        public void BuildsPathsAndHiddenProcessStartInfo()
        {
            Type type = ReflectionTestHelpers.RequireType(TypeName);
            string root = Path.Combine(Path.GetTempPath(), "shahe-common-paths");
            Assert.AreEqual(
                Path.Combine(root, "Configuration"),
                ReflectionTestHelpers.InvokeStatic(type, "GetConfigurationDirectory", root));
            Assert.AreEqual(
                Path.Combine(root, "Configuration", "shahe_config.ini"),
                ReflectionTestHelpers.InvokeStatic(type, "GetConfigurationStorePath", root));
            Assert.AreEqual(
                Path.Combine(root, "tenp"),
                ReflectionTestHelpers.InvokeStatic(type, "GetUpdateTempDirectory", root));

            var startInfo = (ProcessStartInfo)ReflectionTestHelpers.InvokeStatic(
                type,
                "CreateHiddenProcessStartInfo",
                "tool.exe",
                "--flag value");
            Assert.AreEqual("tool.exe", startInfo.FileName);
            Assert.AreEqual("--flag value", startInfo.Arguments);
            Assert.IsFalse(startInfo.UseShellExecute);
            Assert.IsTrue(startInfo.CreateNoWindow);
            Assert.AreEqual(ProcessWindowStyle.Hidden, startInfo.WindowStyle);
        }
    }
}
