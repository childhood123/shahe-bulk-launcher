using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.Core.Sandboxie
{
    public sealed class SandboxieIniSection
    {
        public SandboxieIniSection()
        {
            Name = string.Empty;
            Block = string.Empty;
        }

        public string Name { get; set; }
        public string Block { get; set; }
    }

    public sealed class SandboxieDisplayMaps
    {
        public SandboxieDisplayMaps()
        {
            DisplayToName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            NameToDisplay = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, string> DisplayToName { get; set; }
        public IDictionary<string, string> NameToDisplay { get; set; }
    }

    public sealed class SandboxieCommand
    {
        public SandboxieCommand()
        {
            Executable = string.Empty;
            Arguments = new List<string>();
        }

        public string Executable { get; set; }
        public IList<string> Arguments { get; set; }
        public bool IsValid { get; set; }
    }

    public sealed class SandboxieCallbacks
    {
        public Func<IList<string>, Tuple<int, string>> RunSbieIni { get; set; }
        public Func<IList<string>, Tuple<int, string>> RunStart { get; set; }
        public Func<IList<string>, string, Tuple<bool, string>> DispatchStart { get; set; }
        public Func<IList<string>> ListSectionNames { get; set; }
        public Func<IList<string>> ListOfficialSandboxNames { get; set; }
        public Action ReloadConfiguration { get; set; }
        public Action RefreshDisplayMaps { get; set; }
        public Action RefreshSandboxList { get; set; }
        public Action<string> EnsureSandboxDirectory { get; set; }
        public Action<string> SetStatus { get; set; }
    }

    public sealed class SandboxieOperationResult
    {
        public SandboxieOperationResult()
        {
            InternalName = string.Empty;
            Detail = string.Empty;
        }

        public bool Success { get; set; }
        public string InternalName { get; set; }
        public string Detail { get; set; }
    }

    public sealed class SandboxieLaunchResult
    {
        public SandboxieLaunchResult()
        {
            Output = string.Empty;
            Status = string.Empty;
        }

        public bool Success { get; set; }
        public string Output { get; set; }
        public string Status { get; set; }
    }

    public sealed class SandboxieClient
    {
        public const string FirstScanStatusText = "正在扫描 C/D 盘的 Sandboxie 安装路径...";
        public const string FirstScanSuccessPrefix = "已扫描到 Sandboxie 安装位置：\n";
        public const string FirstScanSuccessTitle = "扫描成功";
        public const string FirstScanMissingText = "未找到 Sandboxie 安装路径，请在“校正”菜单中手动校正 Sandboxie 安装路径。";
        public const string FirstScanResultTitle = "扫描结果";
        public const string RepairConfirmationText =
            "该操作会备份 Sandboxie.ini，然后清理 UserSettings 段中的 SbieCtrl_WindowCoords 窗口坐标记录。\n\n" +
            "建议先关闭 Sandboxie Control 再执行。\n\n" +
            "不会删除沙盒用户，也不会修改 GlobalSettings 或沙盒配置。是否继续？";
        public const string RepairDialogTitle = "修复 Sandboxie Control 用户设置";
        public const string RepairFailedStatus = "修复 Sandboxie Control 用户设置失败";
        public const string RepairFailedTitle = "修复失败";
        public const string RepairCompletedStatus = "已修复 Sandboxie Control 用户设置";
        public const string RepairCompletedTitle = "修复完成";
        public const string RepairNotNeededStatus = "未发现需要修复的 Sandboxie Control 用户设置";
        public const string RepairNotNeededTitle = "无需修复";

        private static readonly Regex InvalidBoxOutputName =
            new Regex("[\\\\/:*?\"<>|\\[\\]]", RegexOptions.CultureInvariant);
        private static readonly Regex IntegerPattern =
            new Regex(@"\b\d+\b", RegexOptions.CultureInvariant);
        private static readonly Regex InvalidInternalCharacters =
            new Regex(@"[^A-Za-z0-9_]", RegexOptions.CultureInvariant);
        private static readonly Regex RepeatedUnderscores =
            new Regex(@"_+", RegexOptions.CultureInvariant);
        private static readonly Regex TrailingDigits =
            new Regex(@"\d+$", RegexOptions.CultureInvariant);

        public SandboxieClient()
        {
            SandboxiePath = string.Empty;
            SandboxRoot = string.Empty;
        }

        public string SandboxiePath { get; set; }
        public string SandboxRoot { get; set; }

        public bool IsValidInstallPath(string path)
        {
            string value = Text(path).Trim();
            return value.Length != 0 && File.Exists(Path.Combine(value, "Start.exe"));
        }

        public string GetStartExecutable(string sandboxiePath)
        {
            string root = Text(sandboxiePath).Trim();
            if (root.Length == 0)
            {
                return string.Empty;
            }

            string path = Path.Combine(root, "Start.exe");
            return File.Exists(path) ? path : string.Empty;
        }

        public string GetSbieIniExecutable(string sandboxiePath)
        {
            string root = Text(sandboxiePath).Trim();
            if (root.Length == 0)
            {
                return string.Empty;
            }

            foreach (string name in new[] { "SbieIni.exe", "sbieini.exe" })
            {
                string path = Path.Combine(root, name);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        public string GetSbieDllPath(string sandboxiePath)
        {
            string root = Text(sandboxiePath).Trim();
            if (root.Length == 0)
            {
                return string.Empty;
            }

            foreach (string name in new[] { "SbieDll.dll", "SbieDll64.dll" })
            {
                string path = Path.Combine(root, name);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        public IList<string> ParseEnabledSandboxNames(string output, int exitCode)
        {
            var names = new List<string>();
            if (exitCode != 0)
            {
                return names;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string rawLine in SplitLines(output))
            {
                string name = Text(rawLine).Trim().Trim('"');
                if (name.StartsWith("[", StringComparison.Ordinal)
                    && name.EndsWith("]", StringComparison.Ordinal))
                {
                    name = name.Substring(1, name.Length - 2).Trim();
                }

                if (name.Length == 0
                    || IsReservedIniSectionName(name)
                    || InvalidBoxOutputName.IsMatch(name)
                    || !seen.Add(name))
                {
                    continue;
                }

                names.Add(name);
            }

            return names;
        }

        public ISet<string> GetOfficialSandboxNameSet(IEnumerable<string> names)
        {
            return new HashSet<string>(
                (names ?? Enumerable.Empty<string>())
                    .Select(name => Text(name).Trim())
                    .Where(name => name.Length != 0),
                StringComparer.OrdinalIgnoreCase);
        }

        public bool SandboxExistsInOfficialConfig(string sandboxName, IEnumerable<string> officialNames)
        {
            string name = Text(sandboxName).Trim();
            ISet<string> names = GetOfficialSandboxNameSet(officialNames);
            return name.Length != 0 && names.Count != 0 && names.Contains(name);
        }

        public string GetIniSettingValue(string block, string settingName)
        {
            string target = Text(settingName).Trim();
            if (target.Length == 0)
            {
                return string.Empty;
            }

            foreach (string rawLine in SplitLines(block))
            {
                string line = Text(rawLine).Trim();
                int separator = line.IndexOf('=');
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) || separator < 0)
                {
                    continue;
                }

                string key = line.Substring(0, separator).Trim();
                if (string.Equals(key, target, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(separator + 1).Trim();
                }
            }

            return string.Empty;
        }

        public bool IsReservedIniSectionName(string sectionName)
        {
            string name = Text(sectionName).Trim();
            return string.Equals(name, "globalsettings", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("usersettings_", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsEditableIniSectionName(string sectionName)
        {
            string name = Text(sectionName).Trim();
            return name.Length != 0
                && !IsReservedIniSectionName(name)
                && LauncherConstants.IsValidInternalSandboxName(name);
        }

        public bool IsSandboxConfigSection(
            string sectionName,
            string block,
            IEnumerable<string> enabledNames)
        {
            string name = Text(sectionName).Trim();
            if (!IsEditableIniSectionName(name))
            {
                return false;
            }

            ISet<string> enabled = GetOfficialSandboxNameSet(enabledNames);
            if (enabled.Count != 0 && enabled.Contains(name))
            {
                return true;
            }

            string setting = GetIniSettingValue(block, "Enabled");
            return string.Equals(setting, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(setting, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(setting, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(setting, "1", StringComparison.OrdinalIgnoreCase);
        }

        public IDictionary<string, string> BuildAliasMap(IEnumerable<SandboxieIniSection> sections)
        {
            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (SandboxieIniSection section in sections ?? Enumerable.Empty<SandboxieIniSection>())
            {
                if (section == null || !IsSandboxConfigSection(section.Name, section.Block, null))
                {
                    continue;
                }

                string alias = GetIniSettingValue(section.Block, "BoxAlias");
                if (alias.Length == 0)
                {
                    alias = GetIniSettingValue(section.Block, "BoxAliasDisabled");
                }

                string name = Text(section.Name).Trim();
                if (name.Length != 0 && alias.Length != 0)
                {
                    aliases[name.ToLowerInvariant()] = alias.Trim();
                }
            }

            return aliases;
        }

        public IList<string> SelectInternalSandboxNames(
            IEnumerable<SandboxieIniSection> sections,
            IEnumerable<string> enabledNames,
            IEnumerable<string> directoryNames)
        {
            ISet<string> enabled = GetOfficialSandboxNameSet(enabledNames);
            var fromSections = new List<string>();
            foreach (SandboxieIniSection section in sections ?? Enumerable.Empty<SandboxieIniSection>())
            {
                if (section != null && IsSandboxConfigSection(section.Name, section.Block, enabled))
                {
                    fromSections.Add(Text(section.Name).Trim());
                }
            }

            IList<string> selected = fromSections.Count != 0
                ? (IList<string>)fromSections
                : enabled.Count != 0
                    ? enabledNames.Select(name => Text(name).Trim()).Where(name => name.Length != 0).ToList()
                    : (directoryNames ?? Enumerable.Empty<string>())
                        .Select(name => Text(name).Trim())
                        .Where(LauncherConstants.IsValidInternalSandboxName)
                        .ToList();
            return DistinctNames(selected);
        }

        public string DisplayNameForSandbox(string internalName, IDictionary<string, string> aliasMap)
        {
            string name = Text(internalName).Trim();
            if (name.Length == 0)
            {
                return string.Empty;
            }

            string alias;
            if (aliasMap != null && aliasMap.TryGetValue(name.ToLowerInvariant(), out alias) && Text(alias).Length != 0)
            {
                return alias;
            }

            return name.Replace('_', ' ');
        }

        public SandboxieDisplayMaps BuildDisplayMaps(
            IEnumerable<string> internalNames,
            IDictionary<string, string> aliasMap)
        {
            var maps = new SandboxieDisplayMaps();
            var usedDisplay = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string internalName in DistinctNames(internalNames))
            {
                string displayName = DisplayNameForSandbox(internalName, aliasMap);
                if (!usedDisplay.Add(displayName))
                {
                    displayName = displayName + " [" + internalName + "]";
                    usedDisplay.Add(displayName);
                }

                maps.DisplayToName[displayName.ToLowerInvariant()] = internalName;
                maps.DisplayToName[internalName.ToLowerInvariant()] = internalName;
                maps.NameToDisplay[internalName.ToLowerInvariant()] = displayName;
            }

            return maps;
        }

        public IList<string> ScanDisplayNames(SandboxieDisplayMaps maps)
        {
            return (maps?.NameToDisplay?.Values ?? Enumerable.Empty<string>())
                .Where(value => Text(value).Trim().Length != 0)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public string ResolveInternalName(string sandboxName, SandboxieDisplayMaps maps)
        {
            string text = Text(sandboxName).Trim();
            if (text.Length == 0)
            {
                return string.Empty;
            }

            string mapped;
            return maps?.DisplayToName != null && maps.DisplayToName.TryGetValue(text.ToLowerInvariant(), out mapped)
                ? mapped
                : text;
        }

        public string GetDisplayName(string sandboxName, SandboxieDisplayMaps maps)
        {
            string internalName = ResolveInternalName(sandboxName, maps);
            if (internalName.Length == 0)
            {
                return string.Empty;
            }

            string display;
            return maps?.NameToDisplay != null && maps.NameToDisplay.TryGetValue(internalName.ToLowerInvariant(), out display)
                ? display
                : internalName.Replace('_', ' ');
        }

        public Tuple<string, string> GetInternalAndDisplayName(string sandboxName, SandboxieDisplayMaps maps)
        {
            string displayName = Text(sandboxName).Trim();
            string internalName = NormalizeSandboxName(sandboxName, maps);
            if (internalName.Length != 0)
            {
                string resolved = GetDisplayName(internalName, maps);
                if (resolved.Length != 0)
                {
                    displayName = resolved;
                }
                else if (displayName.Length == 0)
                {
                    displayName = internalName;
                }
            }

            return Tuple.Create(internalName, displayName.Length == 0 ? internalName : displayName);
        }

        public string GetVolumeMemoryKey(string sandboxName, SandboxieDisplayMaps maps)
        {
            Tuple<string, string> names = GetInternalAndDisplayName(sandboxName, maps);
            string key = Text(names.Item2).Trim();
            return key.Length != 0 ? key : Text(sandboxName).Trim();
        }

        public string DisplayNameToInternalCandidate(string displayName)
        {
            return Text(displayName).Trim().Replace(" ", "_");
        }

        public string GenerateAvailableInternalSandboxName(
            string displayName,
            IEnumerable<string> existingNames,
            IEnumerable<string> reservedNames)
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in (existingNames ?? Enumerable.Empty<string>())
                .Concat(reservedNames ?? Enumerable.Empty<string>()))
            {
                string value = Text(name).Trim();
                if (value.Length != 0)
                {
                    existing.Add(value);
                }
            }

            string preferred = DisplayNameToInternalCandidate(displayName);
            if (LauncherConstants.IsValidInternalSandboxName(preferred) && !existing.Contains(preferred))
            {
                return preferred;
            }

            string baseName = InvalidInternalCharacters.Replace(preferred, string.Empty);
            baseName = RepeatedUnderscores.Replace(baseName, "_").Trim('_');
            baseName = TrailingDigits.Replace(baseName, string.Empty).Trim('_');
            if (baseName.Length == 0)
            {
                baseName = "Box";
            }
            if (baseName.Length > 24)
            {
                baseName = baseName.Substring(0, 24);
            }
            if (baseName.Length == 0)
            {
                baseName = "Box";
            }

            for (int index = 1; index < 1000000; index++)
            {
                string suffix = index.ToString();
                string candidateBase = baseName;
                if (candidateBase.Length + suffix.Length > 32)
                {
                    candidateBase = candidateBase.Substring(0, 32 - suffix.Length);
                }
                string candidate = candidateBase + suffix;
                if (!existing.Contains(candidate) && LauncherConstants.IsValidInternalSandboxName(candidate))
                {
                    return candidate;
                }
            }

            return "Box" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public SandboxieOperationResult CreateSandboxViaOfficial(
            string sandboxName,
            IEnumerable<string> existingNames,
            IEnumerable<string> reservedNames,
            SandboxieCallbacks callbacks)
        {
            string displayName = Text(sandboxName).Trim();
            string validationError = LauncherConstants.GetSandboxDisplayNameValidationError(displayName);
            if (validationError.Length != 0)
            {
                return OperationFailure(validationError);
            }

            string internalName = GenerateAvailableInternalSandboxName(displayName, existingNames, reservedNames);
            validationError = LauncherConstants.GetInternalSandboxNameValidationError(internalName);
            if (validationError.Length != 0)
            {
                return OperationFailure(validationError);
            }

            string alias = DisplayNameForSandbox(internalName, null) == displayName ? string.Empty : displayName;
            Tuple<int, string> enabled = InvokeCommand(callbacks?.RunSbieIni, new[] { "set", "/drv", internalName, "Enabled", "y" });
            if (enabled == null || enabled.Item1 != 0)
            {
                string detail = enabled == null ? string.Empty : Text(enabled.Item2).Trim();
                return OperationFailure(detail.Length == 0 ? "官方创建命令未成功" : detail);
            }

            InvokeCommand(callbacks?.RunSbieIni, new[] { "set", "/drv", internalName, "ConfigLevel", "9" });
            if (alias.Length != 0)
            {
                InvokeCommand(callbacks?.RunSbieIni, new[] { "set", "/drv", internalName, "BoxAlias", alias });
            }
            callbacks?.ReloadConfiguration?.Invoke();
            if (callbacks?.EnsureSandboxDirectory != null)
            {
                callbacks.EnsureSandboxDirectory(internalName);
            }
            else if (Text(SandboxRoot).Trim().Length != 0)
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(SandboxRoot, internalName));
                }
                catch
                {
                }
            }
            callbacks?.RefreshDisplayMaps?.Invoke();
            return new SandboxieOperationResult { Success = true, InternalName = internalName };
        }

        public SandboxieOperationResult DeleteSandboxViaOfficial(
            string sandboxName,
            SandboxieDisplayMaps maps,
            IEnumerable<string> officialNames,
            SandboxieCallbacks callbacks)
        {
            string name = NormalizeSandboxName(sandboxName, maps);
            if (name.Length == 0)
            {
                return OperationFailure("沙盒名为空");
            }

            string validationError = LauncherConstants.GetInternalSandboxNameValidationError(name);
            if (validationError.Length != 0)
            {
                return OperationFailure(validationError);
            }

            ISet<string> official = GetOfficialSandboxNameSet(officialNames);
            ISet<string> beforeSections = GetOfficialSandboxNameSet(callbacks?.ListSectionNames?.Invoke());
            bool targetExists = official.Contains(name) || beforeSections.Contains(name);
            if ((official.Count != 0 || beforeSections.Count != 0) && !targetExists)
            {
                return OperationFailure("沙盒不存在");
            }

            if (official.Count == 0 || official.Contains(name))
            {
                foreach (IList<string> arguments in new[]
                {
                    (IList<string>)new List<string> { "/box:" + name, "delete_sandbox_silent" },
                    new List<string> { "/box:" + name, "delete_sandbox" }
                })
                {
                    Tuple<int, string> result = InvokeCommand(callbacks?.RunStart, arguments);
                    if (result != null && result.Item1 == 0)
                    {
                        break;
                    }
                }
            }

            InvokeCommand(callbacks?.RunSbieIni, new[] { "set", "/drv", name, "*", string.Empty });
            callbacks?.ReloadConfiguration?.Invoke();
            ISet<string> afterSections = GetOfficialSandboxNameSet(callbacks?.ListSectionNames?.Invoke());
            if (callbacks?.ListSectionNames != null && !afterSections.Contains(name))
            {
                return new SandboxieOperationResult { Success = true, InternalName = name };
            }

            return OperationFailure("官方删除命令未成功", name);
        }

        public string NormalizeSandboxName(string sandboxName, SandboxieDisplayMaps maps)
        {
            string name = Text(sandboxName).Trim();
            if (name.IndexOf('\n') >= 0 || name.IndexOf('\r') >= 0)
            {
                name = SplitLines(name).Select(part => part.Trim()).FirstOrDefault(part => part.Length != 0) ?? string.Empty;
            }

            return ResolveInternalName(name, maps).Trim();
        }

        public SandboxieCommand BuildStartCommand(
            string startExecutable,
            IEnumerable<string> arguments,
            bool autoSilent)
        {
            var normalized = new List<string>();
            foreach (string argument in arguments ?? Enumerable.Empty<string>())
            {
                string value = Text(argument).Trim();
                if (value.Length == 0 || value == "?" || value == "/?" || value == "-?" || value == "--help")
                {
                    continue;
                }
                normalized.Add(value);
            }

            string executable = Text(startExecutable).Trim();
            if (normalized.Count == 0)
            {
                return new SandboxieCommand
                {
                    Executable = executable,
                    Arguments = normalized,
                    IsValid = false
                };
            }

            var lower = new HashSet<string>(normalized, StringComparer.OrdinalIgnoreCase);
            bool isQueryOrControl = lower.Contains("/listpids")
                || lower.Contains("/terminate")
                || lower.Contains("/terminate_all")
                || lower.Contains("/reload");
            if (autoSilent && !isQueryOrControl && !lower.Contains("/silent"))
            {
                int insertAt = normalized.Count != 0 && normalized[0].StartsWith("/box:", StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 0;
                normalized.Insert(insertAt, "/silent");
            }

            return new SandboxieCommand
            {
                Executable = executable,
                Arguments = normalized,
                IsValid = executable.Length != 0 && normalized.Count != 0
            };
        }

        public SandboxieCommand BuildSbieIniCommand(string executable, IEnumerable<string> arguments)
        {
            IList<string> normalized = (arguments ?? Enumerable.Empty<string>())
                .Select(argument => Text(argument).Trim())
                .Where(argument => argument.Length != 0)
                .ToList();
            string path = Text(executable).Trim();
            return new SandboxieCommand
            {
                Executable = path,
                Arguments = normalized,
                IsValid = path.Length != 0
            };
        }

        public string FormatProcessArguments(IEnumerable<string> arguments)
        {
            return FormatProcessArgumentsCore(arguments);
        }

        public Tuple<int, string> RunSbieIni(IEnumerable<string> arguments, int timeoutSeconds, string workingDirectory)
        {
            SandboxieCommand command = BuildSbieIniCommand(GetSbieIniExecutable(SandboxiePath), arguments);
            return RunProcess(command, timeoutSeconds, workingDirectory, true);
        }

        public Tuple<int, string> RunSandboxieStart(
            IEnumerable<string> arguments,
            int timeoutSeconds,
            string workingDirectory,
            bool hideWindow,
            bool autoSilent)
        {
            SandboxieCommand command = BuildStartCommand(GetStartExecutable(SandboxiePath), arguments, autoSilent);
            return RunProcess(command, timeoutSeconds, workingDirectory, hideWindow);
        }

        public Tuple<bool, string> DispatchSandboxieStartDetached(
            IEnumerable<string> arguments,
            string workingDirectory,
            bool hideWindow,
            bool autoSilent)
        {
            SandboxieCommand command = BuildStartCommand(GetStartExecutable(SandboxiePath), arguments, autoSilent);
            if (!command.IsValid)
            {
                return Tuple.Create(false, string.Empty);
            }

            var output = new StringBuilder();
            var process = new Process
            {
                StartInfo = CreateProcessStartInfo(command, workingDirectory, hideWindow, true),
                EnableRaisingEvents = false
            };
            process.OutputDataReceived += (sender, eventArgs) => AppendOutput(output, eventArgs.Data);
            process.ErrorDataReceived += (sender, eventArgs) => AppendOutput(output, eventArgs.Data);
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                Thread.Sleep(80);
                if (process.HasExited)
                {
                    process.WaitForExit();
                    int exitCode = process.ExitCode;
                    string detail = output.ToString().Trim();
                    process.Dispose();
                    if (exitCode != 0)
                    {
                        if (detail.Length == 0)
                        {
                            string boxArgument = command.Arguments.FirstOrDefault(
                                argument => argument.StartsWith("/box:", StringComparison.OrdinalIgnoreCase));
                            detail = "Sandboxie Start.exe 立即退出，退出码 " + exitCode;
                            if (Text(boxArgument).Length != 0)
                            {
                                detail += "\n/box:" + boxArgument.Substring(5);
                            }
                        }
                        return Tuple.Create(false, detail);
                    }

                    return Tuple.Create(true, string.Empty);
                }

                Task.Run(() =>
                {
                    try
                    {
                        process.WaitForExit();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        process.Dispose();
                    }
                });
                return Tuple.Create(true, string.Empty);
            }
            catch (Exception error)
            {
                process.Dispose();
                return Tuple.Create(false, Text(error.Message));
            }
        }

        public bool IsInvalidSandboxNameOutput(string outputText)
        {
            string text = Text(outputText).ToLowerInvariant();
            return text.Length != 0 && new[]
            {
                "invalid sandbox name",
                "invalid box",
                "invalid sandbox",
                "无效的沙盒名参数",
                "无效沙盒名",
                "沙盒名参数无效",
                "参数: defaultbox"
            }.Any(text.Contains);
        }

        public IList<int> ParseListPidsOutput(string output)
        {
            IList<int> values = IntegerPattern.Matches(Text(output))
                .Cast<Match>()
                .Select(match => ParsePositiveInt(match.Value))
                .Where(value => value >= 0)
                .ToList();
            if (values.Count >= 2 && values[0] == values.Count - 1)
            {
                values.RemoveAt(0);
            }

            var result = new List<int>();
            var seen = new HashSet<int>();
            foreach (int value in values)
            {
                if (value > 0 && seen.Add(value))
                {
                    result.Add(value);
                }
            }
            return result;
        }

        public IList<int> GetSandboxPids(
            string sandboxName,
            SandboxieDisplayMaps maps,
            IEnumerable<string> officialNames,
            IDictionary<string, IList<int>> processMap,
            Func<string, IList<int>> fallbackQuery)
        {
            string name = NormalizeSandboxName(sandboxName, maps);
            if (!LauncherConstants.IsValidInternalSandboxName(name))
            {
                return new List<int>();
            }

            ISet<string> official = GetOfficialSandboxNameSet(officialNames);
            if (official.Count != 0 && !official.Contains(name))
            {
                return new List<int>();
            }

            IList<int> apiPids;
            if (processMap != null && processMap.TryGetValue(name.ToLowerInvariant(), out apiPids))
            {
                IList<int> normalized = NormalizePids(apiPids);
                if (normalized.Count != 0)
                {
                    return normalized;
                }
            }

            return fallbackQuery == null ? new List<int>() : NormalizePids(fallbackQuery(name));
        }

        public IDictionary<string, bool> GetRunningSandboxStates(
            IEnumerable<string> sandboxNames,
            SandboxieDisplayMaps maps,
            IDictionary<string, IList<int>> processMap,
            Func<string, IList<int>> fallbackQuery)
        {
            var states = new Dictionary<string, bool>();
            foreach (string rawName in sandboxNames ?? Enumerable.Empty<string>())
            {
                string displayName = Text(rawName).Trim();
                if (displayName.Length == 0)
                {
                    continue;
                }
                states[displayName] = false;
            }

            foreach (string displayName in states.Keys.ToList())
            {
                string internalName = NormalizeSandboxName(displayName, maps);
                if (!LauncherConstants.IsValidInternalSandboxName(internalName))
                {
                    continue;
                }

                if (processMap != null)
                {
                    IList<int> pids;
                    states[displayName] = processMap.TryGetValue(internalName.ToLowerInvariant(), out pids)
                        && NormalizePids(pids).Count != 0;
                }
                else
                {
                    states[displayName] = fallbackQuery != null && NormalizePids(fallbackQuery(internalName)).Count != 0;
                }
            }

            return states;
        }

        public IList<int> GetAllSandboxPids(
            IEnumerable<string> sandboxNames,
            SandboxieDisplayMaps maps,
            Func<string, IList<int>> queryPerBox)
        {
            var result = new HashSet<int>();
            var seenBoxes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string sandboxName in sandboxNames ?? Enumerable.Empty<string>())
            {
                string box = NormalizeSandboxName(sandboxName, maps);
                if (!LauncherConstants.IsValidInternalSandboxName(box) || !seenBoxes.Add(box))
                {
                    continue;
                }

                if (queryPerBox != null)
                {
                    foreach (int pid in NormalizePids(queryPerBox(box)))
                    {
                        result.Add(pid);
                    }
                }
            }

            return result.OrderBy(pid => pid).ToList();
        }

        public IDictionary<string, IList<int>> GetSandboxProcessMapViaApi()
        {
            string dllPath = GetSbieDllPath(SandboxiePath);
            if (dllPath.Length == 0)
            {
                return null;
            }

            IntPtr module = LoadLibrary(dllPath);
            if (module == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                IntPtr enumAddress = GetProcAddress(module, "SbieApi_EnumProcessEx");
                IntPtr queryAddress = GetProcAddress(module, "SbieApi_QueryProcess");
                if (enumAddress == IntPtr.Zero || queryAddress == IntPtr.Zero)
                {
                    return null;
                }

                var enumProcess = (EnumProcessExDelegate)Marshal.GetDelegateForFunctionPointer(
                    enumAddress,
                    typeof(EnumProcessExDelegate));
                var queryProcess = (QueryProcessDelegate)Marshal.GetDelegateForFunctionPointer(
                    queryAddress,
                    typeof(QueryProcessDelegate));
                uint count = 0;
                if (enumProcess(null, true, uint.MaxValue, IntPtr.Zero, ref count) != 0)
                {
                    return null;
                }
                if (count == 0)
                {
                    return new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase);
                }
                if (count > 100000)
                {
                    return null;
                }

                IntPtr buffer = Marshal.AllocHGlobal(checked((int)count * sizeof(uint)));
                try
                {
                    if (enumProcess(null, true, uint.MaxValue, buffer, ref count) != 0)
                    {
                        return null;
                    }

                    var map = new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase);
                    for (int index = 0; index < count; index++)
                    {
                        int pid = Marshal.ReadInt32(buffer, index * sizeof(uint));
                        if (pid <= 0)
                        {
                            continue;
                        }

                        var boxName = new StringBuilder(128);
                        var imageName = new StringBuilder(260);
                        var sidText = new StringBuilder(260);
                        uint sessionId = 0;
                        if (queryProcess(new IntPtr(pid), boxName, imageName, sidText, ref sessionId) != 0)
                        {
                            continue;
                        }

                        string name = boxName.ToString().Trim();
                        if (name.Length == 0)
                        {
                            continue;
                        }

                        IList<int> pids;
                        if (!map.TryGetValue(name.ToLowerInvariant(), out pids))
                        {
                            pids = new List<int>();
                            map[name.ToLowerInvariant()] = pids;
                        }
                        pids.Add(pid);
                    }
                    return map;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                FreeLibrary(module);
            }
        }

        public SandboxieLaunchResult StartAppInSandbox(
            string sandboxName,
            string appExecutable,
            string appDirectory,
            SandboxieDisplayMaps maps,
            IEnumerable<string> officialNames,
            SandboxieCallbacks callbacks)
        {
            string boxName = NormalizeSandboxName(sandboxName, maps);
            if (boxName.Length == 0)
            {
                return new SandboxieLaunchResult();
            }

            string displayName = GetDisplayName(boxName, maps);
            if (displayName.Length == 0)
            {
                displayName = Text(sandboxName).Trim();
            }
            if (displayName.Length == 0)
            {
                displayName = boxName;
            }

            string validationError = LauncherConstants.GetInternalSandboxNameValidationError(boxName);
            if (validationError.Length != 0)
            {
                return LaunchFailure(callbacks, "启动失败：" + validationError + "（" + displayName + "）", string.Empty);
            }

            ISet<string> official = GetOfficialSandboxNameSet(officialNames);
            if (official.Count != 0 && !official.Contains(boxName))
            {
                return DisabledBoxFailure(displayName, callbacks);
            }

            string appPath = Text(appExecutable).Trim();
            if (appPath.Length == 0 || !File.Exists(appPath) || GetStartExecutable(SandboxiePath).Length == 0)
            {
                return new SandboxieLaunchResult();
            }

            string workingDirectory = Text(appDirectory).Trim();
            if (workingDirectory.Length == 0 || !Directory.Exists(workingDirectory))
            {
                workingDirectory = null;
            }

            var arguments = new List<string> { "/box:" + boxName, appPath };
            Tuple<bool, string> dispatched = Dispatch(callbacks, arguments, workingDirectory);
            if (dispatched.Item1)
            {
                return new SandboxieLaunchResult { Success = true };
            }

            string output = Text(dispatched.Item2);
            if (IsInvalidSandboxNameOutput(output))
            {
                try
                {
                    callbacks?.ReloadConfiguration?.Invoke();
                }
                catch
                {
                }
                dispatched = Dispatch(callbacks, arguments, workingDirectory);
                if (dispatched.Item1)
                {
                    return new SandboxieLaunchResult { Success = true };
                }
                output = Text(dispatched.Item2);

                IEnumerable<string> refreshedNames = callbacks?.ListOfficialSandboxNames?.Invoke() ?? officialNames;
                official = GetOfficialSandboxNameSet(refreshedNames);
                if (official.Count != 0 && !official.Contains(boxName))
                {
                    return DisabledBoxFailure(displayName, callbacks);
                }
            }

            if (IsInvalidSandboxNameOutput(output))
            {
                return LaunchFailure(callbacks, "启动失败：无效的沙盒名参数 " + displayName, output);
            }

            string detail = SplitLines(output).Select(line => line.Trim()).FirstOrDefault(line => line.Length != 0) ?? string.Empty;
            return detail.Length == 0
                ? new SandboxieLaunchResult { Output = output }
                : LaunchFailure(callbacks, "启动失败：" + displayName + "，" + detail, output);
        }

        public string GetSandboxiePathFromRegistry()
        {
            string[] registryPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Sandboxie",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Sandboxie",
                @"SOFTWARE\Sandboxie",
                @"SOFTWARE\WOW6432Node\Sandboxie"
            };
            string[] valueNames = { "InstallLocation", "Install_Dir" };
            foreach (string registryPath in registryPaths)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                    {
                        if (key == null)
                        {
                            continue;
                        }

                        foreach (string valueName in valueNames)
                        {
                            string value = Text(key.GetValue(valueName)).Trim().Trim('"');
                            if (IsValidInstallPath(value))
                            {
                                return value;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return string.Empty;
        }

        public string FindSandboxiePath(
            IEnumerable<string> registryCandidates,
            IEnumerable<string> standardCandidates,
            IEnumerable<string> wildcardMatches)
        {
            foreach (IEnumerable<string> candidates in new[]
            {
                registryCandidates ?? Enumerable.Empty<string>(),
                standardCandidates ?? Enumerable.Empty<string>(),
                wildcardMatches ?? Enumerable.Empty<string>()
            })
            {
                foreach (string candidate in candidates)
                {
                    string path = Text(candidate).Trim().Trim('"');
                    if (IsValidInstallPath(path))
                    {
                        return path;
                    }
                }
            }

            return string.Empty;
        }

        public string FindSandboxiePath()
        {
            string registry = GetSandboxiePathFromRegistry();
            var standard = new List<string>();
            var wildcard = new List<string>();
            for (char driveLetter = 'C'; driveLetter <= 'Z'; driveLetter++)
            {
                string root = driveLetter + @":\";
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (string baseDirectory in new[] { "Program Files", "Program Files (x86)", string.Empty })
                {
                    string basePath = baseDirectory.Length == 0 ? root : Path.Combine(root, baseDirectory);
                    standard.Add(Path.Combine(basePath, "Sandboxie"));
                    standard.Add(Path.Combine(basePath, "Sandboxie-Plus"));
                }
            }

            for (char driveLetter = 'C'; driveLetter <= 'Z'; driveLetter++)
            {
                string root = driveLetter + @":\";
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (string baseDirectory in new[] { Path.Combine(root, "Program Files"), Path.Combine(root, "Program Files (x86)"), root })
                {
                    try
                    {
                        foreach (string directory in Directory.GetDirectories(baseDirectory, "*Sandboxie*", SearchOption.TopDirectoryOnly))
                        {
                            wildcard.Add(directory);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return FindSandboxiePath(
                registry.Length == 0 ? new string[0] : new[] { registry },
                standard,
                wildcard);
        }

        public IList<string> ListEnabledSandboxesFromSbieIni()
        {
            Tuple<int, string> result = RunSbieIni(new[] { "queryex", "/boxes", "*" }, 8, null);
            return result == null ? new List<string>() : ParseEnabledSandboxNames(result.Item2, result.Item1);
        }

        private Tuple<bool, string> Dispatch(
            SandboxieCallbacks callbacks,
            IList<string> arguments,
            string workingDirectory)
        {
            return callbacks?.DispatchStart != null
                ? callbacks.DispatchStart(arguments, workingDirectory)
                : DispatchSandboxieStartDetached(arguments, workingDirectory, false, false);
        }

        private SandboxieLaunchResult DisabledBoxFailure(string displayName, SandboxieCallbacks callbacks)
        {
            callbacks?.RefreshDisplayMaps?.Invoke();
            callbacks?.RefreshSandboxList?.Invoke();
            return LaunchFailure(
                callbacks,
                "启动失败：Sandboxie 未启用沙盒 " + displayName + "，已刷新列表",
                string.Empty);
        }

        private static SandboxieLaunchResult LaunchFailure(
            SandboxieCallbacks callbacks,
            string status,
            string output)
        {
            callbacks?.SetStatus?.Invoke(status);
            return new SandboxieLaunchResult { Status = status, Output = Text(output) };
        }

        private static SandboxieOperationResult OperationFailure(string detail, string internalName = "")
        {
            return new SandboxieOperationResult
            {
                InternalName = Text(internalName),
                Detail = Text(detail)
            };
        }

        private static Tuple<int, string> InvokeCommand(
            Func<IList<string>, Tuple<int, string>> callback,
            IEnumerable<string> arguments)
        {
            return callback?.Invoke(arguments.ToList());
        }

        private static Tuple<int, string> RunProcess(
            SandboxieCommand command,
            int timeoutSeconds,
            string workingDirectory,
            bool hideWindow)
        {
            if (command == null || !command.IsValid)
            {
                return null;
            }

            using (var process = new Process
            {
                StartInfo = CreateProcessStartInfo(command, workingDirectory, hideWindow, true)
            })
            {
                try
                {
                    process.Start();
                    Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
                    Task<string> standardError = process.StandardError.ReadToEndAsync();
                    int timeoutMilliseconds = Math.Max(1, timeoutSeconds) * 1000;
                    if (!process.WaitForExit(timeoutMilliseconds))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                        }
                        return null;
                    }
                    process.WaitForExit();
                    string output = JoinOutput(standardOutput.Result, standardError.Result);
                    return Tuple.Create(process.ExitCode, output);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static ProcessStartInfo CreateProcessStartInfo(
            SandboxieCommand command,
            string workingDirectory,
            bool hideWindow,
            bool redirectOutput)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command.Executable,
                Arguments = FormatProcessArgumentsCore(command.Arguments),
                UseShellExecute = false,
                CreateNoWindow = hideWindow,
                WindowStyle = hideWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                RedirectStandardInput = true,
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectOutput,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            string directory = Text(workingDirectory).Trim();
            if (directory.Length != 0)
            {
                startInfo.WorkingDirectory = directory;
            }
            return startInfo;
        }

        private static string QuoteArgument(string value)
        {
            string text = Text(value);
            if (text.Length != 0 && !text.Any(character => char.IsWhiteSpace(character) || character == '"'))
            {
                return text;
            }

            var quoted = new StringBuilder("\"");
            int backslashes = 0;
            foreach (char character in text)
            {
                if (character == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (character == '"')
                {
                    quoted.Append('\\', (backslashes * 2) + 1);
                    quoted.Append('"');
                    backslashes = 0;
                    continue;
                }

                quoted.Append('\\', backslashes);
                backslashes = 0;
                quoted.Append(character);
            }

            quoted.Append('\\', backslashes * 2);
            quoted.Append('"');
            return quoted.ToString();
        }

        private static string FormatProcessArgumentsCore(IEnumerable<string> arguments)
        {
            return string.Join(" ", (arguments ?? Enumerable.Empty<string>()).Select(QuoteArgument));
        }

        private static string JoinOutput(string standardOutput, string standardError)
        {
            return string.Join(
                "\n",
                new[] { Text(standardOutput), Text(standardError) }.Where(part => part.Length != 0));
        }

        private static void AppendOutput(StringBuilder output, string line)
        {
            if (line == null)
            {
                return;
            }
            lock (output)
            {
                if (output.Length != 0)
                {
                    output.AppendLine();
                }
                output.Append(line);
            }
        }

        private static IList<string> DistinctNames(IEnumerable<string> names)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in names ?? Enumerable.Empty<string>())
            {
                string value = Text(name).Trim();
                if (value.Length != 0 && seen.Add(value))
                {
                    result.Add(value);
                }
            }
            return result;
        }

        private static IList<int> NormalizePids(IEnumerable<int> values)
        {
            return (values ?? Enumerable.Empty<int>())
                .Where(value => value > 0)
                .Distinct()
                .OrderBy(value => value)
                .ToList();
        }

        private static int ParsePositiveInt(string value)
        {
            int result;
            return int.TryParse(value, out result) ? result : -1;
        }

        private static IEnumerable<string> SplitLines(string value)
        {
            return Text(value).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static string Text(object value)
        {
            return Convert.ToString(value) ?? string.Empty;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr module, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr module);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int EnumProcessExDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string boxName,
            [MarshalAs(UnmanagedType.Bool)] bool allSessions,
            uint sessionId,
            IntPtr processIds,
            ref uint processCount);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int QueryProcessDelegate(
            IntPtr processId,
            StringBuilder boxName,
            StringBuilder imageName,
            StringBuilder sidText,
            ref uint sessionId);
    }
}
