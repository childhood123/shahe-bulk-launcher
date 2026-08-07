using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace ShaheLauncher.Core.Constants
{
    public static class LauncherConstants
    {
        public const string OldDefaultSandboxRoot = @"C:\Sandbox\Administrator";
        public const string AppVersion = "4.0.46";
        public const string UpdateManifestUrl = "https://lqh.viyf.net/shahe-bulk-launcher/update.json";
        public const string LogOutputOff = "off";
        public const string LogOutputDetailed = "detailed";

        public static readonly IReadOnlyList<KeyValuePair<string, string>> LogOutputChoices =
            new ReadOnlyCollection<KeyValuePair<string, string>>(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(LogOutputOff, "关闭日志输出"),
                    new KeyValuePair<string, string>(LogOutputDetailed, "详细日志输出")
                });

        public static readonly IReadOnlyDictionary<string, string> FixedHotkeyTextByAction =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "select_all", "Ctrl+A" },
                    { "open_app", "Ctrl+O" },
                    { "paste_app", "Ctrl+V" },
                    { "refresh", "F5" },
                    { "minimize_to_tray", "Esc" },
                    { "exit_app", "Alt+F4" }
                });

        public static readonly IReadOnlyDictionary<string, string> FixedHotkeyLabelByAction =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "select_all", "全选" },
                    { "open_app", "选择应用" },
                    { "paste_app", "粘贴 .exe 或文件路径" },
                    { "refresh", "刷新沙盒用户" },
                    { "minimize_to_tray", "最小化到托盘" },
                    { "exit_app", "退出软件" }
                });

        private static readonly Regex InternalSandboxNamePattern =
            new Regex(@"^[A-Za-z0-9_]{1,32}$", RegexOptions.CultureInvariant);

        private static readonly Regex WindowsDeviceNamePattern =
            new Regex(@"^(aux|clock\$|con|nul|prn|com[0-9]|lpt[0-9])$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex InvalidDisplayNameCharacters =
            new Regex("[,()\\r\\n\\t\\0]", RegexOptions.CultureInvariant);

        private static readonly Regex TrailingDigits =
            new Regex(@"\d+$", RegexOptions.CultureInvariant);

        public static string NormalizeLogOutputMode(string value)
        {
            string text = (value ?? string.Empty).Trim();
            if (text == "关闭日志输出" || text == LogOutputOff)
            {
                return LogOutputOff;
            }

            return LogOutputDetailed;
        }

        public static string GetDefaultSandboxRoot()
        {
            string drive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            drive = drive.TrimEnd('\\', '/');
            if (drive.Length == 0)
            {
                drive = "C:";
            }

            string userName = Environment.GetEnvironmentVariable("USERNAME");
            if (string.IsNullOrEmpty(userName))
            {
                userName = Environment.GetEnvironmentVariable("USER");
            }

            userName = (userName ?? "Administrator").Trim();
            if (userName.Length == 0)
            {
                userName = "Administrator";
            }

            return drive + @"\Sandbox\" + userName;
        }

        public static string NormalizeSandboxRootPath(string sandboxRoot)
        {
            string value = (sandboxRoot ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                return Path.GetFullPath(value);
            }
            catch
            {
                return value;
            }
        }

        public static string GetInternalSandboxNameValidationError(string sandboxName)
        {
            string name = (sandboxName ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return "内部沙盒名不能为空";
            }

            if (CountCodePoints(name) > 32)
            {
                return "内部沙盒名不能超过 32 个字符";
            }

            if (string.Equals(name, "globalsettings", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("usersettings_", StringComparison.OrdinalIgnoreCase))
            {
                return "内部沙盒名不能使用 Sandboxie 保留分节名";
            }

            if (WindowsDeviceNamePattern.IsMatch(name))
            {
                return "内部沙盒名不能使用 Windows 设备保留名";
            }

            if (!InternalSandboxNamePattern.IsMatch(name))
            {
                return "内部沙盒名只能使用英文字母、数字和下划线";
            }

            return string.Empty;
        }

        public static bool IsValidInternalSandboxName(string sandboxName)
        {
            return GetInternalSandboxNameValidationError(sandboxName).Length == 0;
        }

        public static string GetSandboxDisplayNameValidationError(string sandboxName)
        {
            string name = (sandboxName ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return "沙盒名不能为空";
            }

            if (CountCodePoints(name) > 240)
            {
                return "沙盒名过长";
            }

            if (InvalidDisplayNameCharacters.IsMatch(name))
            {
                return "沙盒名不能包含英文逗号、括号或控制字符";
            }

            return string.Empty;
        }

        public static string MakeDefaultSandboxDisplayPrefix(string prefix, string fallback)
        {
            string value = InvalidDisplayNameCharacters.Replace((prefix ?? string.Empty).Trim(), string.Empty);
            value = TrailingDigits.Replace(value, string.Empty);
            if (value.Length == 0)
            {
                value = string.IsNullOrEmpty(fallback) ? "沙盒用户" : fallback;
            }

            value = TruncateCodePoints(value, 80);
            return value.Length == 0 ? "沙盒用户" : value;
        }

        private static int CountCodePoints(string value)
        {
            int count = 0;
            for (int index = 0; index < value.Length; index++)
            {
                if (char.IsHighSurrogate(value[index])
                    && index + 1 < value.Length
                    && char.IsLowSurrogate(value[index + 1]))
                {
                    index++;
                }

                count++;
            }

            return count;
        }

        private static string TruncateCodePoints(string value, int maximum)
        {
            int count = 0;
            int index = 0;
            while (index < value.Length && count < maximum)
            {
                if (char.IsHighSurrogate(value[index])
                    && index + 1 < value.Length
                    && char.IsLowSurrogate(value[index + 1]))
                {
                    index += 2;
                }
                else
                {
                    index++;
                }

                count++;
            }

            return index == value.Length ? value : value.Substring(0, index);
        }
    }
}
