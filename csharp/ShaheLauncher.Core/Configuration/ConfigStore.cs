using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ShaheLauncher.Core.Configuration
{
    public sealed class ConfigStore
    {
        private const string ConfigSection = "config";
        private const string SettingsSection = "settings";
        private readonly string _storePath;
        private readonly string _fingerprint;
        private readonly Func<long> _unixTime;

        public ConfigStore(string path, string fingerprint, Func<long> unixTime)
        {
            _storePath = (path ?? string.Empty).Trim();
            _fingerprint = fingerprint ?? string.Empty;
            _unixTime = unixTime ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public string GetText(string key, string defaultValue)
        {
            string name = (key ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return defaultValue;
            }

            try
            {
                IniDocument document = Load();
                IniSection section = document.GetSection(ConfigSection);
                string encryptedName = name.EndsWith("_encrypted", StringComparison.Ordinal)
                    ? name
                    : name + "_encrypted";
                string encrypted;
                if (section.TryGetValue(encryptedName, out encrypted))
                {
                    string decrypted = ConfigCrypto.DecryptConfigValue(encrypted, _fingerprint);
                    return decrypted.Length == 0 ? defaultValue : decrypted;
                }

                string value;
                return section.TryGetValue(name, out value) ? value : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool SetText(string key, string value, bool secret)
        {
            string name = (key ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return false;
            }

            try
            {
                string storageName = secret ? name + "_encrypted" : name;
                string removeName = secret ? name : name + "_encrypted";
                string text = secret ? Encrypt(value ?? string.Empty) : value ?? string.Empty;
                return WriteLocked(() =>
                {
                    IniDocument document = LoadSnapshot();
                    IniSection section = document.EnsureSection(ConfigSection);
                    section.Remove(removeName);
                    section.Set(storageName, text);
                    section.Set(storageName + "_updated_at", _unixTime().ToString());
                    Save(document);
                });
            }
            catch
            {
                return false;
            }
        }

        public string GetSecretText(string key, string defaultValue)
        {
            return GetText(key, defaultValue);
        }

        public bool SetSecretText(string key, string value)
        {
            return SetText(key, value, true);
        }

        public object GetJson(string key, object defaultValue)
        {
            if (string.Equals((key ?? string.Empty).Trim(), "settings_json", StringComparison.Ordinal))
            {
                try
                {
                    IniSection section = Load().GetSection(SettingsSection);
                    if (section != null && section.Entries.Count != 0)
                    {
                        var settings = new Dictionary<string, object>(StringComparer.Ordinal);
                        foreach (IniEntry entry in section.Entries)
                        {
                            if (entry.Name.EndsWith("_updated_at", StringComparison.Ordinal))
                            {
                                continue;
                            }

                            try
                            {
                                settings[entry.Name] = JsonConvert.DeserializeObject<object>(entry.Value);
                            }
                            catch
                            {
                                settings[entry.Name] = entry.Value;
                            }
                        }

                        return settings;
                    }
                }
                catch
                {
                }
            }

            string text = GetText(key, string.Empty);
            if (text.Length == 0)
            {
                return defaultValue;
            }

            try
            {
                return JsonConvert.DeserializeObject<object>(text);
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool SetJson(string key, object value)
        {
            if (!string.Equals((key ?? string.Empty).Trim(), "settings_json", StringComparison.Ordinal))
            {
                try
                {
                    return SetText(key, JsonConvert.SerializeObject(value, Formatting.Indented), false);
                }
                catch
                {
                    return false;
                }
            }

            IDictionary values = value as IDictionary;
            if (values == null)
            {
                return false;
            }

            return SetSettings(values);
        }

        private bool SetSettings(IDictionary values)
        {
            return WriteLocked(() => SaveSettingsSnapshot(values));
        }

        private void SaveSettingsSnapshot(IDictionary values)
        {
            IniDocument document = LoadSnapshot();
            document.RemoveSection(SettingsSection);
            IniSection settings = document.EnsureSection(SettingsSection);
            IniSection config = document.EnsureSection(ConfigSection);
            config.Remove("settings_json");
            config.Remove("settings_json_updated_at");

            var entries = new List<KeyValuePair<string, object>>();
            foreach (DictionaryEntry entry in values)
            {
                string name = Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty;
                if (name.Trim().Length != 0)
                    entries.Add(new KeyValuePair<string, object>(name, entry.Value));
            }
            foreach (KeyValuePair<string, object> entry in entries.OrderBy(item => item.Key, StringComparer.Ordinal))
                settings.Set(entry.Key, JsonConvert.SerializeObject(entry.Value, Formatting.None));
            settings.Set("updated_at", _unixTime().ToString(CultureInfo.InvariantCulture));
            Save(document);
        }

        private bool WriteLocked(Action write)
        {
            try
            {
                return WithStoreLock(() => { write(); return true; });
            }
            catch
            {
                return false;
            }
        }

        private IniDocument Load()
        {
            return WithStoreLock(LoadSnapshot);
        }

        private T WithStoreLock<T>(Func<T> action)
        {
            EnsureStorePath();
            string lockName;
            using (SHA256 hash = SHA256.Create())
            {
                byte[] path = Encoding.UTF8.GetBytes(Path.GetFullPath(_storePath).ToUpperInvariant());
                lockName = "Local\\ShaheLauncher.Config." + BitConverter.ToString(hash.ComputeHash(path)).Replace("-", string.Empty);
            }
            using (var gate = new Mutex(false, lockName))
            {
                bool acquired = false;
                try
                {
                    try { acquired = gate.WaitOne(TimeSpan.FromSeconds(10)); }
                    catch (AbandonedMutexException) { acquired = true; }
                    if (!acquired) throw new IOException("等待配置文件访问超时");
                    return action();
                }
                finally { if (acquired) gate.ReleaseMutex(); }
            }
        }

        private IniDocument LoadSnapshot()
        {
            EnsureStorePath();
            Directory.CreateDirectory(Path.GetDirectoryName(_storePath) ?? string.Empty);
            IniDocument document;
            if (File.Exists(_storePath))
            {
                using (var stream = new FileStream(_storePath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    document = IniDocument.Parse(reader.ReadToEnd().Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
            }
            else document = new IniDocument();

            document.EnsureSection(ConfigSection);
            return document;
        }

        private void Save(IniDocument document)
        {
            EnsureStorePath();
            Directory.CreateDirectory(Path.GetDirectoryName(_storePath) ?? string.Empty);
            document.EnsureSection(ConfigSection);
            string temporaryPath = _storePath + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, document.Serialize(), new UTF8Encoding(false));
                if (File.Exists(_storePath)) File.Replace(temporaryPath, _storePath, null);
                else File.Move(temporaryPath, _storePath);
            }
            finally
            {
                try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
                catch { }
            }
        }

        private void EnsureStorePath()
        {
            if (_storePath.Length == 0)
            {
                throw new InvalidOperationException("配置文件路径无效");
            }
        }

        private string Encrypt(string value)
        {
            byte[] nonce = new byte[12];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(nonce);
            }

            return ConfigCrypto.EncryptConfigValue(value, _fingerprint, nonce);
        }
    }

    internal sealed class IniEntry
    {
        public IniEntry(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }

        public string Value { get; set; }
    }

    internal sealed class IniSection
    {
        public IniSection(string name)
        {
            Name = name;
            Entries = new List<IniEntry>();
        }

        public string Name { get; private set; }

        public List<IniEntry> Entries { get; private set; }

        public bool TryGetValue(string name, out string value)
        {
            IniEntry entry = Entries.SingleOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));
            value = entry == null ? null : entry.Value;
            return entry != null;
        }

        public void Set(string name, string value)
        {
            IniEntry entry = Entries.SingleOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (entry == null)
            {
                Entries.Add(new IniEntry(name, value ?? string.Empty));
            }
            else
            {
                entry.Value = value ?? string.Empty;
            }
        }

        public void Remove(string name)
        {
            Entries.RemoveAll(item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }
    }

    internal sealed class IniDocument
    {
        private readonly List<IniSection> _sections = new List<IniSection>();

        public IniSection GetSection(string name)
        {
            return _sections.SingleOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        public IniSection EnsureSection(string name)
        {
            IniSection section = GetSection(name);
            if (section == null)
            {
                section = new IniSection(name);
                _sections.Add(section);
            }

            return section;
        }

        public void RemoveSection(string name)
        {
            _sections.RemoveAll(item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        public static IniDocument Parse(IEnumerable<string> lines)
        {
            var document = new IniDocument();
            IniSection currentSection = null;
            IniEntry currentEntry = null;
            foreach (string sourceLine in lines)
            {
                string line = sourceLine ?? string.Empty;
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith(";", StringComparison.Ordinal))
                {
                    continue;
                }

                if (char.IsWhiteSpace(line[0]) && currentEntry != null)
                {
                    currentEntry.Value += "\n" + trimmed;
                    continue;
                }

                if (trimmed.StartsWith("[", StringComparison.Ordinal))
                {
                    if (!trimmed.EndsWith("]", StringComparison.Ordinal) || trimmed.Length <= 2)
                    {
                        throw new FormatException("Invalid INI section");
                    }

                    string sectionName = trimmed.Substring(1, trimmed.Length - 2);
                    if (document.GetSection(sectionName) != null)
                    {
                        throw new FormatException("Duplicate INI section");
                    }

                    currentSection = document.EnsureSection(sectionName);
                    currentEntry = null;
                    continue;
                }

                if (currentSection == null)
                {
                    throw new FormatException("INI value has no section");
                }

                int equalsIndex = line.IndexOf('=');
                int colonIndex = line.IndexOf(':');
                int separatorIndex = equalsIndex < 0
                    ? colonIndex
                    : colonIndex < 0 ? equalsIndex : Math.Min(equalsIndex, colonIndex);
                if (separatorIndex < 0)
                {
                    throw new FormatException("Invalid INI value");
                }

                string key = line.Substring(0, separatorIndex).Trim();
                string existing;
                if (key.Length == 0 || currentSection.TryGetValue(key, out existing))
                {
                    throw new FormatException("Duplicate or empty INI key");
                }

                currentSection.Set(key, line.Substring(separatorIndex + 1).Trim());
                currentEntry = currentSection.Entries[currentSection.Entries.Count - 1];
            }

            return document;
        }

        public string Serialize()
        {
            var builder = new StringBuilder();
            foreach (IniSection section in _sections)
            {
                builder.Append('[').Append(section.Name).Append("]\r\n");
                foreach (IniEntry entry in section.Entries)
                {
                    builder.Append(entry.Name).Append('=').Append(entry.Value.Replace("\n", "\r\n\t")).Append("\r\n");
                }

                builder.Append("\r\n");
            }

            return builder.ToString();
        }
    }
}
