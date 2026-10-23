using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ShaheLauncher.Core.Common
{
    public static class CommonUtilities
    {
        private const string ConfigStoreName = "shahe_config.ini";

        public static bool ParseBoolSetting(object value, bool defaultValue)
        {
            string text = value as string;
            if (text != null)
            {
                switch (text.Trim().ToLowerInvariant())
                {
                    case "1":
                    case "true":
                    case "yes":
                    case "y":
                    case "on":
                    case "是":
                    case "开":
                    case "开启":
                        return true;
                    case "0":
                    case "false":
                    case "no":
                    case "n":
                    case "off":
                    case "否":
                    case "关":
                    case "关闭":
                    case "":
                        return false;
                    default:
                        return defaultValue;
                }
            }

            if (value == null)
            {
                return defaultValue;
            }

            if (value is bool)
            {
                return (bool)value;
            }

            if (value is ICollection)
            {
                return ((ICollection)value).Count != 0;
            }

            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0.0;
            }
            catch
            {
                return true;
            }
        }

        public static int ParseIntSetting(object value, int defaultValue, object minimum, object maximum)
        {
            int number;
            try
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value is double || value is float)
                {
                    double floatingValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    if (double.IsNaN(floatingValue) || double.IsInfinity(floatingValue))
                    {
                        throw new OverflowException();
                    }

                    number = (int)floatingValue;
                }
                else if (value is decimal)
                {
                    number = (int)(decimal)value;
                }
                else
                {
                    number = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                number = defaultValue;
            }

            if (minimum != null)
            {
                number = Math.Max(Convert.ToInt32(minimum, CultureInfo.InvariantCulture), number);
            }

            if (maximum != null)
            {
                number = Math.Min(Convert.ToInt32(maximum, CultureInfo.InvariantCulture), number);
            }

            return number;
        }

        public static int[] ParseVersionParts(string versionText)
        {
            string[] rawParts = Regex.Split((versionText ?? string.Empty).Trim(), @"[^\d]+");
            var parts = new System.Collections.Generic.List<int>();
            foreach (string rawPart in rawParts)
            {
                if (rawPart.Length == 0)
                {
                    continue;
                }

                int number;
                parts.Add(int.TryParse(rawPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out number) ? number : 0);
            }

            return parts.Count == 0 ? new[] { 0 } : parts.ToArray();
        }

        public static int CompareVersions(string left, string right)
        {
            int[] leftParts = ParseVersionParts(left);
            int[] rightParts = ParseVersionParts(right);
            int length = Math.Max(leftParts.Length, rightParts.Length);
            for (int index = 0; index < length; index++)
            {
                int leftValue = index < leftParts.Length ? leftParts[index] : 0;
                int rightValue = index < rightParts.Length ? rightParts[index] : 0;
                if (leftValue != rightValue)
                {
                    return leftValue > rightValue ? 1 : -1;
                }
            }

            return 0;
        }

        public static string FormatSize(object numberOfBytes)
        {
            double size;
            try
            {
                size = Convert.ToDouble(numberOfBytes, CultureInfo.InvariantCulture);
            }
            catch
            {
                size = 0.0;
            }

            foreach (string unit in new[] { "B", "KB", "MB", "GB" })
            {
                if (size < 1024.0 || unit == "GB")
                {
                    return unit == "B"
                        ? ((int)size).ToString(CultureInfo.InvariantCulture) + " B"
                        : size.ToString("0.0", CultureInfo.InvariantCulture) + " " + unit;
                }

                size /= 1024.0;
            }

            return "0 B";
        }

        public static bool IsAllowedUpdateBodyStatus(object status, string url)
        {
            int statusCode;
            try
            {
                statusCode = Convert.ToInt32(status, CultureInfo.InvariantCulture);
            }
            catch
            {
                return false;
            }

            string allowedUrl = (url ?? string.Empty).Trim();
            return allowedUrl.Length == 0
                ? statusCode == 404
                : statusCode == 404 && allowedUrl.StartsWith("https://lqh.viyf.net/", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetConfigurationDirectory(string baseDirectory)
        {
            return Path.Combine(GetBaseDirectory(baseDirectory), "Configurations");
        }

        public static string PrepareConfigurationDirectory(string baseDirectory)
        {
            string directory = GetConfigurationDirectory(baseDirectory);
            string previous = Path.Combine(GetBaseDirectory(baseDirectory), "Configuration");
            if (Directory.Exists(previous)
                && (!Directory.Exists(directory) || Directory.GetFileSystemEntries(directory).Length == 0))
            {
                if (Directory.Exists(directory)) Directory.Delete(directory);
                Directory.Move(previous, directory);
            }
            Directory.CreateDirectory(directory);
            return directory;
        }

        public static string GetConfigurationStorePath(string baseDirectory)
        {
            return Path.Combine(GetConfigurationDirectory(baseDirectory), ConfigStoreName);
        }

        public static string GetUpdateTempDirectory(string baseDirectory)
        {
            return Path.Combine(GetBaseDirectory(baseDirectory), "tenp");
        }

        public static ProcessStartInfo CreateHiddenProcessStartInfo(string fileName, string arguments)
        {
            return new ProcessStartInfo
            {
                FileName = fileName ?? string.Empty,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        private static string GetBaseDirectory(string baseDirectory)
        {
            string root = (baseDirectory ?? string.Empty).Trim();
            return root.Length == 0 ? AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar) : root;
        }
    }
}
