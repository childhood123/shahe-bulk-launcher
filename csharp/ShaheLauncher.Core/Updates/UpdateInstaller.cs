using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShaheLauncher.Core.Updates
{
    public sealed class UpdateInstallArguments
    {
        public UpdateInstallArguments()
        {
            AppDirectory = string.Empty;
            ExecutablePath = string.Empty;
            RequestFile = string.Empty;
        }

        public string AppDirectory { get; set; }
        public string ExecutablePath { get; set; }
        public int ProcessId { get; set; }
        public string RequestFile { get; set; }
    }

    public sealed class UpdateInstallRequest
    {
        public UpdateInstallRequest()
        {
            Url = string.Empty;
            PackageKind = string.Empty;
            CurrentVersion = string.Empty;
            TargetVersion = string.Empty;
            Sha256 = string.Empty;
            Signature = string.Empty;
        }

        public string Url { get; set; }
        public string PackageKind { get; set; }
        public string CurrentVersion { get; set; }
        public string TargetVersion { get; set; }
        public string Sha256 { get; set; }
        public string Signature { get; set; }
        public bool Encrypted { get; set; }
        public long Size { get; set; }
        public bool Restart { get; set; }
        public int DownloadWorkers { get; set; }
    }

    public static class UpdateInstaller
    {
        public static UpdateInstallArguments ParseArguments(string[] args)
        {
            var result = new UpdateInstallArguments();
            string[] values = args ?? new string[0];
            for (int index = 0; index + 1 < values.Length; index += 2)
            {
                string key = values[index] ?? string.Empty;
                string value = values[index + 1] ?? string.Empty;
                switch (key)
                {
                    case "--app-dir":
                        result.AppDirectory = value;
                        break;
                    case "--exe":
                        result.ExecutablePath = value;
                        break;
                    case "--pid":
                        int processId;
                        result.ProcessId = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out processId)
                            ? processId
                            : 0;
                        break;
                    case "--request-file":
                        result.RequestFile = value;
                        break;
                }
            }

            return result;
        }

        public static UpdateInstallRequest LoadRequest(string requestFile)
        {
            IDictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                File.ReadAllText(requestFile));
            if (data == null)
            {
                throw new InvalidDataException("更新请求文件格式不正确");
            }

            return new UpdateInstallRequest
            {
                Url = Text(GetValue(data, "url", string.Empty)).Trim(),
                PackageKind = Text(GetValue(data, "package_kind", string.Empty)).Trim(),
                CurrentVersion = Text(GetValue(data, "current_version", string.Empty)).Trim(),
                TargetVersion = Text(GetValue(data, "target_version", string.Empty)).Trim(),
                Sha256 = Text(GetValue(data, "sha256", string.Empty)).Trim(),
                Signature = Text(GetValue(data, "signature", string.Empty)).Trim(),
                Encrypted = ParseBool(GetValue(data, "encrypted", false)),
                Size = NonNegativeLong(GetValue(data, "size", 0)),
                Restart = ParseBool(GetValue(data, "restart", true)),
                DownloadWorkers = Math.Max(1, (int)NonNegativeLong(GetValue(data, "download_workers", 4)))
            };
        }

        public static string ComputeSha256(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(filePath))
            {
                return string.Concat(sha256.ComputeHash(stream).Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
            }
        }

        public static bool ValidateSha256(string filePath, string expectedHash)
        {
            string expected = (expectedHash ?? string.Empty).Trim();
            return expected.Length == 0
                || string.Equals(ComputeSha256(filePath), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static object GetValue(IDictionary<string, object> values, string key, object fallback)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? Unwrap(value) : fallback;
        }

        private static object Unwrap(object value)
        {
            JValue json = value as JValue;
            return json == null ? value : json.Value;
        }

        private static string Text(object value)
        {
            object raw = Unwrap(value);
            return raw == null ? string.Empty : Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static long NonNegativeLong(object value)
        {
            long number;
            return long.TryParse(Text(value).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
                ? Math.Max(0L, number)
                : 0L;
        }

        private static bool ParseBool(object value)
        {
            string text = Text(value).Trim().ToLowerInvariant();
            if (text == "true" || text == "1" || text == "yes" || text == "是")
            {
                return true;
            }

            if (text == "false" || text == "0" || text == "no" || text == "否" || text.Length == 0)
            {
                return false;
            }

            bool parsed;
            return bool.TryParse(text, out parsed) && parsed;
        }
    }
}
