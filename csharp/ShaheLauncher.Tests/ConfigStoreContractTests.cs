using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Common")]
    public sealed class ConfigStoreContractTests
    {
        private const string CryptoTypeName = "ShaheLauncher.Core.Configuration.ConfigCrypto";
        private const string StoreTypeName = "ShaheLauncher.Core.Configuration.ConfigStore";

        [TestMethod]
        public void ConfigCryptoMatchesDeterministicPythonAesGcmVector()
        {
            Type type = ReflectionTestHelpers.RequireType(CryptoTypeName);
            const string fingerprint = "MachineGuid|HOST|User|Domain|CPU";
            byte[] nonce = Enumerable.Range(0, 12).Select(value => (byte)value).ToArray();
            const string vector = "AAECAwQFBgcICQoLKpHL8pdFcd92RYQ5c/vGtZB3qoOK2STzzA==";

            Assert.AreEqual(vector, ReflectionTestHelpers.InvokeStatic(type, "EncryptConfigValue", "测试值", fingerprint, nonce));
            Assert.AreEqual("测试值", ReflectionTestHelpers.InvokeStatic(type, "DecryptConfigValue", vector, fingerprint));
            Assert.AreEqual(string.Empty, ReflectionTestHelpers.InvokeStatic(type, "DecryptConfigValue", "invalid", fingerprint));
            Assert.AreEqual(string.Empty, ReflectionTestHelpers.InvokeStatic(type, "EncryptConfigValue", string.Empty, fingerprint, nonce));
        }

        [TestMethod]
        public void ConfigStoreWritesPythonCompatibleIniAndReadsSettings()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string path = Path.Combine(root, "shahe_config.ini");
                object store = CreateStore(path, "test-fingerprint", 1786118400L);
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(store, "SetText", "plain", "值", false));
                Assert.AreEqual(
                    true,
                    ReflectionTestHelpers.InvokeInstance(
                        store,
                        "SetJson",
                        "settings_json",
                        new Dictionary<string, object>
                        {
                            { "hide_path", false },
                            { "max_history_items", 10 },
                            { "name", "测试" }
                        }));

                string expected =
                    "[config]\r\n" +
                    "plain=值\r\n" +
                    "plain_updated_at=1786118400\r\n\r\n" +
                    "[settings]\r\n" +
                    "hide_path=false\r\n" +
                    "max_history_items=10\r\n" +
                    "name=\"测试\"\r\n" +
                    "updated_at=1786118400\r\n\r\n";
                Assert.AreEqual(expected, File.ReadAllText(path, Encoding.UTF8));
                Assert.AreEqual("值", ReflectionTestHelpers.InvokeInstance(store, "GetText", "plain", "fallback"));
                Assert.AreEqual("fallback", ReflectionTestHelpers.InvokeInstance(store, "GetText", "Plain", "fallback"));

                IDictionary settings = (IDictionary)ReflectionTestHelpers.InvokeInstance(store, "GetJson", "settings_json", null);
                Assert.AreEqual(false, settings["hide_path"]);
                Assert.AreEqual(10L, Convert.ToInt64(settings["max_history_items"]));
                Assert.AreEqual("测试", settings["name"]);
                Assert.AreEqual(1786118400L, Convert.ToInt64(settings["updated_at"]));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ConfigStoreRoundTripsSecretsAndRecoversFromMalformedIni()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string path = Path.Combine(root, "shahe_config.ini");
                object store = CreateStore(path, "test-fingerprint", 123L);
                Assert.AreEqual(
                    true,
                    ReflectionTestHelpers.InvokeInstance(
                        store,
                        "SetSecretText",
                        "update_manifest_url",
                        "https://example.test/update.json"));
                Assert.AreEqual(
                    "https://example.test/update.json",
                    ReflectionTestHelpers.InvokeInstance(store, "GetSecretText", "update_manifest_url", "fallback"));

                string source = File.ReadAllText(path, Encoding.UTF8);
                StringAssert.Contains(source, "update_manifest_url_encrypted=");
                Assert.IsFalse(source.Contains("https://example.test/update.json"));

                File.WriteAllText(
                    path,
                    "[config]\r\nduplicate=1\r\nduplicate=2\r\n",
                    new UTF8Encoding(false));
                Assert.AreEqual(
                    "fallback",
                    ReflectionTestHelpers.InvokeInstance(store, "GetText", "duplicate", "fallback"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static object CreateStore(string path, string fingerprint, long timestamp)
        {
            Type type = ReflectionTestHelpers.RequireType(StoreTypeName);
            ConstructorInfo constructor = type.GetConstructors().SingleOrDefault(candidate => candidate.GetParameters().Length == 3);
            Assert.IsNotNull(constructor, "ConfigStore must expose the three-argument constructor");
            return constructor.Invoke(new object[] { path, fingerprint, new Func<long>(() => timestamp) });
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-csharp-common-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
