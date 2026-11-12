using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.Core.SandboxAdmin
{
    public sealed class SandboxAdminHistoryEntry
    {
        public SandboxAdminHistoryEntry()
        {
            Path = string.Empty;
            Note = string.Empty;
        }

        public string Path { get; set; }
        public string Note { get; set; }
    }

    public sealed class SandboxAdminIniSection
    {
        public SandboxAdminIniSection()
        {
            Name = string.Empty;
            Block = string.Empty;
        }

        public string Name { get; set; }
        public string Block { get; set; }
    }

    public sealed class SandboxIniSplitResult
    {
        public SandboxIniSplitResult()
        {
            Preamble = string.Empty;
            Sections = new List<SandboxAdminIniSection>();
        }

        public string Preamble { get; set; }
        public IList<SandboxAdminIniSection> Sections { get; set; }
    }

    public sealed class SandboxScheduleJob
    {
        public SandboxScheduleJob()
        {
            Description = string.Empty;
            SandboxTargets = new List<string>();
        }

        public string Description { get; set; }
        public IList<string> SandboxTargets { get; set; }
    }

    public sealed class SandboxAdminState
    {
        public SandboxAdminState()
        {
            SandboxGroups = new Dictionary<string, IList<string>>();
            PinnedSandboxes = new List<string>();
            SandboxVolumeMemory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            CachedMuteStateBySandbox = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            ScheduleJobs = new Dictionary<string, SandboxScheduleJob>(StringComparer.OrdinalIgnoreCase);
            SandboxRoot = string.Empty;
        }

        public IDictionary<string, IList<string>> SandboxGroups { get; set; }
        public IList<string> PinnedSandboxes { get; set; }
        public IDictionary<string, int> SandboxVolumeMemory { get; set; }
        public IDictionary<string, bool> CachedMuteStateBySandbox { get; set; }
        public bool? CachedMuteStateAll { get; set; }
        public IDictionary<string, SandboxScheduleJob> ScheduleJobs { get; set; }
        public string SandboxRoot { get; set; }
    }

    public sealed class SandboxAdminCallbacks
    {
        public Func<IEnumerable<string>> GetExistingSandboxDisplayNames { get; set; }
        public Func<IEnumerable<string>> GetExistingSandboxNames { get; set; }
        public Func<string, string> NormalizeSandboxName { get; set; }
        public Func<string, string> GetSandboxDisplayName { get; set; }
        public Func<string, string> GetVolumeMemoryKey { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action SaveSettings { get; set; }
        public Action<double> SuppressListRefreshStatus { get; set; }
        public Action RefreshSandboxList { get; set; }
        public Action<string, bool> CancelScheduleTask { get; set; }
        public Func<string, IEnumerable<int>> GetSandboxPids { get; set; }
        public Func<string, bool> TerminateSandbox { get; set; }
        public Func<IEnumerable<int>, IList<int>> GetAlivePids { get; set; }
        public Action<IEnumerable<int>> TerminatePids { get; set; }
    }

    public sealed class SandboxAliasResult
    {
        public SandboxAliasResult()
        {
            Error = string.Empty;
        }

        public bool Changed { get; set; }
        public string Error { get; set; }
    }

    public sealed class SandboxRegistryCleanupResult
    {
        public SandboxRegistryCleanupResult()
        {
            FailedPaths = new List<string>();
        }

        public int Cleaned { get; set; }
        public IList<string> FailedPaths { get; set; }
    }

    public sealed class SandboxAdminService
    {
        public const string BatchCreateDialogTitle = "批量创建沙盒用户";
        public const string BatchCreatePromptText = "请输入批量创建规则：";
        public const string BatchRenameDialogTitle = "批量重命名沙盒用户";
        public const string BatchRenamePromptText = "将按当前勾选顺序依次重命名，请设置新命名规则：";
        public const string NameValidationFailedTitle = "名称校验失败";

        private static readonly Regex IniHeaderPattern = new Regex(
            @"(?m)^[ \t]*\[(?<name>[^\]\r\n]+)\][^\r\n]*(?:\r?\n)?",
            RegexOptions.CultureInvariant);
        private static readonly Regex IniSettingPattern = new Regex(
            @"^([ \t]*)(?<name>[^=\r\n]+)([ \t]*=)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex SectionHeaderPattern = new Regex(
            @"(?m)^([ \t]*)\[[^\]\r\n]+\]([^\r\n]*)(\r?\n)?",
            RegexOptions.CultureInvariant);

        public IDictionary<string, object> NormalizeSandboxGroups(object groups, SandboxAdminCallbacks callbacks)
        {
            var normalized = new Dictionary<string, object>();
            ISet<string> displays;
            ISet<string> internals;
            GetValidSandboxLookups(callbacks, out displays, out internals);
            bool filter = displays.Count != 0 || internals.Count != 0;
            IDictionary dictionary = groups as IDictionary;
            if (dictionary == null)
            {
                return normalized;
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                string groupName = Text(entry.Key).Trim();
                if (groupName.Length == 0)
                {
                    continue;
                }

                var cleaned = new List<string>();
                foreach (object item in Values(entry.Value))
                {
                    string itemName = Text(item).Trim();
                    if (itemName.Length == 0)
                    {
                        continue;
                    }

                    string normalizedName = itemName;
                    if (filter)
                    {
                        string internalName = InvokeNormalize(callbacks, itemName);
                        if (!displays.Contains(itemName.ToLowerInvariant())
                            && !internals.Contains(internalName.ToLowerInvariant()))
                        {
                            continue;
                        }

                        string displayName = InvokeDisplay(callbacks, internalName);
                        normalizedName = displayName.Length == 0 ? itemName : displayName;
                    }

                    if (normalizedName.Length != 0 && !cleaned.Contains(normalizedName))
                    {
                        cleaned.Add(normalizedName);
                    }
                }

                if (cleaned.Count != 0)
                {
                    normalized[groupName] = cleaned;
                }
            }

            return normalized;
        }

        public IList<string> NormalizeSandboxNameList(object values, SandboxAdminCallbacks callbacks)
        {
            var normalized = new List<string>();
            ISet<string> displays;
            ISet<string> internals;
            GetValidSandboxLookups(callbacks, out displays, out internals);
            bool filter = displays.Count != 0 || internals.Count != 0;
            foreach (object item in Values(values))
            {
                string name = Text(item).Trim();
                if (name.Length == 0)
                {
                    continue;
                }

                string normalizedName = name;
                if (filter)
                {
                    string internalName = InvokeNormalize(callbacks, name);
                    if (!displays.Contains(name.ToLowerInvariant())
                        && !internals.Contains(internalName.ToLowerInvariant()))
                    {
                        continue;
                    }

                    string displayName = InvokeDisplay(callbacks, internalName);
                    normalizedName = displayName.Length == 0 ? name : displayName;
                }

                if (normalizedName.Length != 0 && !normalized.Contains(normalizedName))
                {
                    normalized.Add(normalizedName);
                }
            }

            return normalized;
        }

        public IDictionary<string, int> NormalizeVolumeMemory(object volumeMemory, SandboxAdminCallbacks callbacks)
        {
            var normalized = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            IDictionary dictionary = volumeMemory as IDictionary;
            if (dictionary == null)
            {
                return normalized;
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                string name = InvokeVolumeKey(callbacks, Text(entry.Key));
                if (name.Length == 0)
                {
                    continue;
                }

                double level;
                if (!TryDouble(entry.Value, out level))
                {
                    continue;
                }

                int value = Math.Max(0, Math.Min(100, (int)Math.Round(level)));
                normalized[name] = value;
            }

            return normalized;
        }

        public IList<SandboxAdminHistoryEntry> NormalizeAppHistory(object appHistory, int? limit)
        {
            var normalized = new List<SandboxAdminHistoryEntry>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (object item in Values(appHistory))
            {
                string path = string.Empty;
                string note = string.Empty;
                IDictionary dictionary = item as IDictionary;
                if (dictionary != null)
                {
                    path = Text(GetDictionaryValue(dictionary, "path")).Trim();
                    note = Text(GetDictionaryValue(dictionary, "note")).Trim();
                }
                else if (item is string)
                {
                    path = Text(item).Trim();
                }

                if (path.Length == 0)
                {
                    continue;
                }

                string key;
                try
                {
                    key = Path.GetFullPath(path);
                }
                catch
                {
                    key = path;
                }

                if (!seen.Add(key))
                {
                    continue;
                }

                normalized.Add(new SandboxAdminHistoryEntry
                {
                    Path = path,
                    Note = note.Length == 0 ? Path.GetFileName(path) : note
                });
            }

            if (limit.HasValue)
            {
                return normalized.Take(Math.Max(0, limit.Value)).ToList();
            }

            return normalized;
        }

        public IList<string> GetGroupNames(SandboxAdminState state)
        {
            return (state?.SandboxGroups?.Keys ?? Enumerable.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        public IList<string> GetGroupMembers(SandboxAdminState state, string groupName)
        {
            IList<string> members;
            return state != null && state.SandboxGroups != null
                && state.SandboxGroups.TryGetValue(Text(groupName), out members)
                ? new List<string>(members ?? new List<string>())
                : new List<string>();
        }

        public IList<string> GetPinnedSandboxes(SandboxAdminState state)
        {
            return new List<string>(state?.PinnedSandboxes ?? new List<string>());
        }

        public bool IsSandboxPinned(SandboxAdminState state, string sandboxName)
        {
            string name = Text(sandboxName).Trim();
            return name.Length != 0 && (state?.PinnedSandboxes ?? new List<string>()).Contains(name);
        }

        public bool TogglePinSandbox(SandboxAdminState state, string sandboxName, SandboxAdminCallbacks callbacks)
        {
            string name = Text(sandboxName).Trim();
            if (state == null || name.Length == 0)
            {
                return false;
            }

            if (state.PinnedSandboxes.Contains(name))
            {
                state.PinnedSandboxes = state.PinnedSandboxes.Where(value => value != name).ToList();
                callbacks?.SetStatus?.Invoke("已取消置顶: " + name);
            }
            else
            {
                state.PinnedSandboxes = new[] { name }
                    .Concat(state.PinnedSandboxes.Where(value => value != name))
                    .ToList();
                callbacks?.SetStatus?.Invoke("已置顶: " + name);
            }

            callbacks?.SaveSettings?.Invoke();
            callbacks?.SuppressListRefreshStatus?.Invoke(2.0);
            callbacks?.RefreshSandboxList?.Invoke();
            return true;
        }

        public ISet<string> GetGroupedMembers(SandboxAdminState state)
        {
            var grouped = new HashSet<string>(StringComparer.Ordinal);
            if (state?.SandboxGroups == null)
            {
                return grouped;
            }

            foreach (IList<string> members in state.SandboxGroups.Values)
            {
                foreach (string name in members ?? new List<string>())
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        grouped.Add(name);
                    }
                }
            }

            return grouped;
        }

        public IList<string> BuildBatchSandboxNames(string prefix, int start, int count, int digits)
        {
            string prefixValue = Text(prefix).Trim();
            int startValue = Math.Max(0, start);
            int countValue = Math.Max(0, count);
            int digitsValue = Math.Max(0, digits);
            var names = new List<string>();
            for (int index = 0; index < countValue; index++)
            {
                int number = startValue + index;
                string numberText = digitsValue > 0
                    ? number.ToString().PadLeft(digitsValue, '0')
                    : number.ToString();
                names.Add(prefixValue + numberText);
            }

            return names;
        }

        public IList<string> ValidateBatchSandboxNames(IEnumerable<string> names)
        {
            var errors = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new List<string>();
            foreach (string rawName in names ?? Enumerable.Empty<string>())
            {
                string name = Text(rawName).Trim();
                string validationError = LauncherConstants.GetSandboxDisplayNameValidationError(name);
                if (validationError.Length != 0)
                {
                    errors.Add(validationError + ": " + (name.Length == 0 ? "(空)" : name));
                    continue;
                }

                if (!seen.Add(name) && !duplicates.Contains(name))
                {
                    duplicates.Add(name);
                }
            }

            if (duplicates.Count != 0)
            {
                errors.Add("名称重复: " + string.Join("、", duplicates));
            }

            return errors;
        }

        public static string FormatBatchCreateStatus(int created, int skipped, int failed)
        {
            var parts = new List<string> { "已创建 " + Math.Max(0, created) + " 个沙盒用户" };
            if (skipped > 0) parts.Add("跳过 " + skipped + " 个已存在");
            if (failed > 0) parts.Add("失败 " + failed + " 个");
            return string.Join("，", parts);
        }

        public static string FormatBatchRenameStatus(int count)
        {
            return "已批量重命名 " + Math.Max(0, count) + " 个沙盒用户";
        }

        public static string FormatBatchDeleteStatus(int deleted, int failed, int blockedRunning)
        {
            if (deleted <= 0)
            {
                return "未删除任何沙盒用户";
            }

            if (failed > 0 || blockedRunning > 0)
            {
                return "已删除 " + deleted + " 个，失败 " + Math.Max(0, failed)
                    + " 个，跳过运行中 " + Math.Max(0, blockedRunning) + " 个";
            }

            return "已批量删除 " + deleted + " 个沙盒用户";
        }

        public IList<string> GetSandboxieIniCandidates(string windowsDirectory, string sandboxiePath)
        {
            string winDir = Text(windowsDirectory).Trim();
            if (winDir.Length == 0)
            {
                winDir = @"C:\Windows";
            }

            var directories = new List<string> { winDir };
            string installPath = Text(sandboxiePath).Trim();
            if (installPath.Length != 0 && !directories.Contains(installPath, StringComparer.OrdinalIgnoreCase))
            {
                directories.Add(installPath);
            }

            var candidates = new List<string>();
            foreach (string filename in new[] { "Sandboxie.ini", "Sandboxie-Plus.ini", "SandboxiePlus.ini" })
            {
                foreach (string directory in directories)
                {
                    string candidate = Path.Combine(directory, filename);
                    if (!candidates.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            return candidates;
        }

        public string GetSandboxieIniPath(string windowsDirectory, string sandboxiePath)
        {
            IList<string> candidates = GetSandboxieIniCandidates(windowsDirectory, sandboxiePath);
            string existing = candidates.FirstOrDefault(File.Exists);
            return existing ?? (candidates.Count == 0 ? string.Empty : candidates[0]);
        }

        public Tuple<string, string> ReadTextWithEncoding(string path)
        {
            return IniTextFile.Read(Text(path));
        }

        public void WriteTextWithEncoding(string path, string text, string encodingName)
        {
            IniTextFile.Write(Text(path), Text(text), encodingName);
        }

        public string BackupSandboxieIniBeforeWrite(string iniPath)
        {
            string path = Text(iniPath);
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(
                Path.GetDirectoryName(path) ?? string.Empty,
                Path.GetFileName(path) + ".沙河批量打开器备份_" + timestamp + ".bak");
            File.Copy(path, backupPath, true);
            return backupPath;
        }

        public SandboxIniSplitResult SplitIniSections(string text)
        {
            string content = Text(text);
            MatchCollection matches = IniHeaderPattern.Matches(content);
            var result = new SandboxIniSplitResult();
            if (matches.Count == 0)
            {
                result.Preamble = content;
                return result;
            }

            result.Preamble = content.Substring(0, matches[0].Index);
            for (int index = 0; index < matches.Count; index++)
            {
                Match match = matches[index];
                int end = index + 1 < matches.Count ? matches[index + 1].Index : content.Length;
                result.Sections.Add(new SandboxAdminIniSection
                {
                    Name = Text(match.Groups["name"].Value).Trim(),
                    Block = content.Substring(match.Index, end - match.Index)
                });
            }

            return result;
        }

        public string ReplaceSectionHeaderName(string block, string newName)
        {
            return SectionHeaderPattern.Replace(
                Text(block),
                match => match.Groups[1].Value + "[" + Text(newName) + "]"
                    + match.Groups[2].Value + match.Groups[3].Value,
                1);
        }

        public Tuple<string, bool> SetIniSettingInBlock(
            string block,
            string settingName,
            string value,
            string lineSeparator)
        {
            string setting = Text(settingName).Trim();
            if (setting.Length == 0)
            {
                return Tuple.Create(Text(block), false);
            }

            string text = Text(block);
            string separator = lineSeparator == "\r\n" ? "\r\n" : "\n";
            var lines = SplitLinesKeepingEndings(text);
            string escaped = Regex.Escape(setting);
            var pattern = new Regex("^[ \\t]*" + escaped + "[ \\t]*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (string.IsNullOrEmpty(value))
            {
                bool removed = false;
                lines = lines.Where(line =>
                {
                    if (pattern.IsMatch(line))
                    {
                        removed = true;
                        return false;
                    }
                    return true;
                }).ToList();
                return Tuple.Create(string.Concat(lines), removed);
            }

            string newLine = setting + "=" + value + separator;
            bool found = false;
            bool changed = false;
            for (int index = 0; index < lines.Count; index++)
            {
                if (!pattern.IsMatch(lines[index]))
                {
                    continue;
                }

                found = true;
                if (lines[index] != newLine)
                {
                    lines[index] = newLine;
                    changed = true;
                }
            }

            if (!found)
            {
                lines.Insert(lines.Count == 0 ? 0 : 1, newLine);
                changed = true;
            }

            return Tuple.Create(string.Concat(lines), changed);
        }

        public string BuildIniSectionBlock(string sandboxName, string lineSeparator)
        {
            string separator = lineSeparator == "\r\n" ? "\r\n" : "\n";
            return "[" + Text(sandboxName) + "]" + separator
                + "Enabled=y" + separator
                + "ConfigLevel=9" + separator + separator;
        }

        public IList<string> ListIniSectionNames(string iniPath, bool sandboxOnly)
        {
            Tuple<string, string> loaded = ReadTextWithEncoding(iniPath);
            SandboxIniSplitResult split = SplitIniSections(loaded.Item1);
            return split.Sections
                .Where(section => !sandboxOnly || IsSandboxConfigSection(section.Name, section.Block))
                .Select(section => section.Name)
                .ToList();
        }

        public Tuple<bool, string> ApplyIniSectionChanges(
            string iniPath,
            IEnumerable<string> addNames,
            IEnumerable<string> removeNames,
            IDictionary<string, string> renameMap)
        {
            string path = Text(iniPath);
            IList<string> adds = (addNames ?? Enumerable.Empty<string>())
                .Select(Text).Select(value => value.Trim())
                .Where(IsEditableSandboxIniSectionName).ToList();
            ISet<string> removes = new HashSet<string>(
                (removeNames ?? Enumerable.Empty<string>())
                    .Select(Text).Select(value => value.Trim())
                    .Where(IsEditableSandboxIniSectionName), StringComparer.OrdinalIgnoreCase);
            var renames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> item in renameMap ?? new Dictionary<string, string>())
            {
                string oldName = Text(item.Key).Trim();
                string newName = Text(item.Value).Trim();
                if (IsEditableSandboxIniSectionName(oldName) && IsEditableSandboxIniSectionName(newName))
                {
                    renames[oldName] = newName;
                }
            }

            Tuple<string, string> loaded = ReadTextWithEncoding(path);
            string text = loaded.Item1;
            string separator = text.Contains("\r\n") ? "\r\n" : "\n";
            SandboxIniSplitResult split = SplitIniSections(text);
            bool changed = false;
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rebuilt = new List<SandboxAdminIniSection>();
            foreach (SandboxAdminIniSection section in split.Sections)
            {
                string key = section.Name.ToLowerInvariant();
                if (!IsEditableSandboxIniSectionName(section.Name))
                {
                    rebuilt.Add(section);
                    used.Add(key);
                    continue;
                }
                if (removes.Contains(section.Name))
                {
                    changed = true;
                    continue;
                }

                string target = renames.ContainsKey(section.Name) ? renames[section.Name] : section.Name;
                if (used.Contains(target))
                {
                    changed = true;
                    continue;
                }
                string block = section.Block;
                if (!string.Equals(target, section.Name, StringComparison.Ordinal))
                {
                    block = ReplaceSectionHeaderName(block, target);
                    changed = true;
                }
                rebuilt.Add(new SandboxAdminIniSection { Name = target, Block = block });
                used.Add(target);
            }

            foreach (string name in adds)
            {
                if (used.Contains(name))
                {
                    continue;
                }
                rebuilt.Add(new SandboxAdminIniSection { Name = name, Block = BuildIniSectionBlock(name, separator) });
                used.Add(name);
                changed = true;
            }

            string body = string.Concat(rebuilt.Select(section =>
                section.Block.EndsWith("\r\n", StringComparison.Ordinal)
                    || section.Block.EndsWith("\n", StringComparison.Ordinal)
                    ? section.Block
                    : section.Block + separator));
            string newText = split.Preamble + body;
            if (!changed && File.Exists(path))
            {
                return Tuple.Create(false, string.Empty);
            }
            if (newText == text && File.Exists(path))
            {
                return Tuple.Create(changed, string.Empty);
            }

            try
            {
                BackupSandboxieIniBeforeWrite(path);
            }
            catch
            {
            }
            WriteTextWithEncoding(path, newText, loaded.Item2);
            return Tuple.Create(true, string.Empty);
        }

        public Tuple<string, IList<string>> CleanUserSettingsBlock(string block, string lineSeparator)
        {
            string separator = lineSeparator == "\r\n" ? "\r\n" : "\n";
            var removeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sbiectrl_windowcoords", "enabled", "configlevel", "boxalias", "boxaliasdisabled"
            };
            var lines = SplitLinesKeepingEndings(Text(block));
            var kept = new List<string>();
            var removed = new List<string>();
            foreach (string line in lines)
            {
                string stripped = line.Trim();
                int separatorIndex = stripped.IndexOf('=');
                if (stripped.Length != 0 && !stripped.StartsWith("#", StringComparison.Ordinal) && separatorIndex >= 0)
                {
                    string key = stripped.Substring(0, separatorIndex).Trim().ToLowerInvariant();
                    if (removeNames.Contains(key))
                    {
                        removed.Add(key);
                        continue;
                    }
                }
                kept.Add(line);
            }

            if (kept.Count != 0
                && !kept[kept.Count - 1].EndsWith("\r\n", StringComparison.Ordinal)
                && !kept[kept.Count - 1].EndsWith("\n", StringComparison.Ordinal))
            {
                kept[kept.Count - 1] += separator;
            }

            return Tuple.Create(string.Concat(kept), (IList<string>)removed);
        }

        public Tuple<bool, string> RepairSandboxieControlUserSettings(
            string iniPath,
            Func<bool> reloadConfiguration)
        {
            Tuple<string, string> loaded = ReadTextWithEncoding(iniPath);
            if (loaded.Item1.Length == 0)
            {
                return Tuple.Create(false, "未找到 Sandboxie.ini 或配置为空");
            }

            string separator = loaded.Item1.Contains("\r\n") ? "\r\n" : "\n";
            SandboxIniSplitResult split = SplitIniSections(loaded.Item1);
            if (split.Sections.Count == 0)
            {
                return Tuple.Create(false, "Sandboxie.ini 中未找到配置段");
            }

            bool changed = false;
            int removedCount = 0;
            var touchedSections = new List<string>();
            var rebuilt = new List<string>();
            foreach (SandboxAdminIniSection section in split.Sections)
            {
                string block = section.Block;
                if (section.Name.StartsWith("UserSettings_", StringComparison.OrdinalIgnoreCase))
                {
                    Tuple<string, IList<string>> cleaned = CleanUserSettingsBlock(block, separator);
                    if (cleaned.Item2.Count != 0)
                    {
                        changed = true;
                        removedCount += cleaned.Item2.Count;
                        touchedSections.Add(section.Name);
                        block = cleaned.Item1;
                    }
                }

                if (!block.EndsWith("\r\n", StringComparison.Ordinal)
                    && !block.EndsWith("\n", StringComparison.Ordinal))
                {
                    block += separator;
                }
                rebuilt.Add(block);
            }

            if (!changed)
            {
                return Tuple.Create(false, "未发现需要清理的 Sandboxie Control 用户设置");
            }

            string backupPath = string.Empty;
            try
            {
                backupPath = BackupSandboxieIniBeforeWrite(iniPath);
            }
            catch
            {
            }
            WriteTextWithEncoding(iniPath, split.Preamble + string.Concat(rebuilt), loaded.Item2);
            try
            {
                reloadConfiguration?.Invoke();
            }
            catch
            {
            }

            string detail = "已清理 " + touchedSections.Count + " 个 UserSettings 段中的 "
                + removedCount + " 条异常/易冲突记录";
            if (backupPath.Length != 0)
            {
                detail += "\n已备份原配置：" + backupPath;
            }
            return Tuple.Create(true, detail);
        }

        public bool RenameSandboxReferences(
            SandboxAdminState state,
            IDictionary<string, string> renameMap)
        {
            if (state == null || renameMap == null || renameMap.Count == 0)
            {
                return false;
            }

            bool changed = false;
            foreach (KeyValuePair<string, IList<string>> group in state.SandboxGroups.ToList())
            {
                var members = new List<string>();
                foreach (string member in group.Value ?? new List<string>())
                {
                    string target = renameMap.ContainsKey(member) ? renameMap[member] : member;
                    if (!string.IsNullOrEmpty(target) && !members.Contains(target))
                    {
                        members.Add(target);
                    }
                }
                if (!members.SequenceEqual(group.Value ?? new List<string>()))
                {
                    state.SandboxGroups[group.Key] = members;
                    changed = true;
                }
            }

            var pinned = new List<string>();
            foreach (string name in state.PinnedSandboxes)
            {
                string target = renameMap.ContainsKey(name) ? renameMap[name] : name;
                if (!string.IsNullOrEmpty(target) && !pinned.Contains(target))
                {
                    pinned.Add(target);
                }
            }
            if (!pinned.SequenceEqual(state.PinnedSandboxes))
            {
                state.PinnedSandboxes = pinned;
                changed = true;
            }

            var volume = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, int> item in state.SandboxVolumeMemory)
            {
                string target = renameMap.ContainsKey(item.Key) ? renameMap[item.Key] : item.Key;
                if (!string.IsNullOrEmpty(target))
                {
                    volume[target] = item.Value;
                }
            }
            if (!DictionaryEquals(state.SandboxVolumeMemory, volume))
            {
                state.SandboxVolumeMemory = volume;
                changed = true;
            }

            var mute = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, bool> item in state.CachedMuteStateBySandbox)
            {
                string target = renameMap.ContainsKey(item.Key) ? renameMap[item.Key] : item.Key;
                if (!string.IsNullOrEmpty(target))
                {
                    mute[target] = item.Value;
                }
            }
            if (!DictionaryEquals(state.CachedMuteStateBySandbox, mute))
            {
                state.CachedMuteStateBySandbox = mute;
                changed = true;
            }
            state.CachedMuteStateAll = null;
            return changed;
        }

        public bool RenameSandboxReferencesByDisplay(
            SandboxAdminState state,
            IDictionary<string, string> displayRenameMap)
        {
            IDictionary<string, string> lookup = BuildCaseInsensitiveMap(displayRenameMap);
            if (state == null || lookup.Count == 0)
            {
                return false;
            }

            bool changed = false;
            foreach (KeyValuePair<string, IList<string>> group in state.SandboxGroups.ToList())
            {
                var members = new List<string>();
                foreach (string member in group.Value ?? new List<string>())
                {
                    string target = lookup.ContainsKey(Text(member).Trim().ToLowerInvariant())
                        ? lookup[Text(member).Trim().ToLowerInvariant()]
                        : member;
                    if (!string.IsNullOrEmpty(target) && !members.Contains(target))
                    {
                        members.Add(target);
                    }
                }
                if (!members.SequenceEqual(group.Value ?? new List<string>()))
                {
                    state.SandboxGroups[group.Key] = members;
                    changed = true;
                }
            }

            var pinned = state.PinnedSandboxes.Select(name =>
                lookup.ContainsKey(Text(name).Trim().ToLowerInvariant())
                    ? lookup[Text(name).Trim().ToLowerInvariant()]
                    : name).Where(name => !string.IsNullOrEmpty(name)).Distinct().ToList();
            if (!pinned.SequenceEqual(state.PinnedSandboxes))
            {
                state.PinnedSandboxes = pinned;
                changed = true;
            }

            var volume = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, int> item in state.SandboxVolumeMemory)
            {
                string target = lookup.ContainsKey(Text(item.Key).Trim().ToLowerInvariant())
                    ? lookup[Text(item.Key).Trim().ToLowerInvariant()]
                    : item.Key;
                if (!string.IsNullOrEmpty(target)) volume[target] = item.Value;
            }
            if (!DictionaryEquals(state.SandboxVolumeMemory, volume))
            {
                state.SandboxVolumeMemory = volume;
                changed = true;
            }
            return changed;
        }

        public bool DeleteSandboxReferences(
            SandboxAdminState state,
            IEnumerable<string> namesToDelete)
        {
            if (state == null)
            {
                return false;
            }

            ISet<string> deleteSet = new HashSet<string>(
                (namesToDelete ?? Enumerable.Empty<string>()).Select(Text).Select(value => value.Trim())
                    .Where(value => value.Length != 0), StringComparer.Ordinal);
            if (deleteSet.Count == 0)
            {
                return false;
            }
            ISet<string> lookup = new HashSet<string>(deleteSet.Select(value => value.ToLowerInvariant()));
            bool changed = false;

            foreach (KeyValuePair<string, IList<string>> group in state.SandboxGroups.ToList())
            {
                IList<string> members = group.Value ?? new List<string>();
                var remaining = members.Where(member => !deleteSet.Contains(member)
                    && !lookup.Contains(Text(member).Trim().ToLowerInvariant())).ToList();
                if (!remaining.SequenceEqual(members))
                {
                    if (remaining.Count == 0) state.SandboxGroups.Remove(group.Key);
                    else state.SandboxGroups[group.Key] = remaining;
                    changed = true;
                }
            }

            var pinned = state.PinnedSandboxes.Where(name => !deleteSet.Contains(name)
                && !lookup.Contains(Text(name).Trim().ToLowerInvariant())).ToList();
            if (!pinned.SequenceEqual(state.PinnedSandboxes))
            {
                state.PinnedSandboxes = pinned;
                changed = true;
            }
            foreach (string key in state.SandboxVolumeMemory.Keys.ToList())
            {
                if (deleteSet.Contains(key) || lookup.Contains(Text(key).Trim().ToLowerInvariant()))
                {
                    state.SandboxVolumeMemory.Remove(key);
                    changed = true;
                }
            }
            foreach (string key in state.CachedMuteStateBySandbox.Keys.ToList())
            {
                if (deleteSet.Contains(key) || lookup.Contains(Text(key).Trim().ToLowerInvariant()))
                {
                    state.CachedMuteStateBySandbox.Remove(key);
                    changed = true;
                }
            }
            state.CachedMuteStateAll = null;
            return changed;
        }

        public bool DeleteSandboxReferencesByDisplay(
            SandboxAdminState state,
            IEnumerable<string> namesToDelete)
        {
            if (state == null)
            {
                return false;
            }

            ISet<string> lookup = new HashSet<string>(
                (namesToDelete ?? Enumerable.Empty<string>()).Select(Text).Select(value => value.Trim().ToLowerInvariant())
                    .Where(value => value.Length != 0));
            if (lookup.Count == 0)
            {
                return false;
            }
            bool changed = false;
            foreach (KeyValuePair<string, IList<string>> group in state.SandboxGroups.ToList())
            {
                IList<string> members = group.Value ?? new List<string>();
                var remaining = members.Where(member => !lookup.Contains(Text(member).Trim().ToLowerInvariant())).ToList();
                if (!remaining.SequenceEqual(members))
                {
                    if (remaining.Count == 0) state.SandboxGroups.Remove(group.Key);
                    else state.SandboxGroups[group.Key] = remaining;
                    changed = true;
                }
            }

            var pinned = state.PinnedSandboxes.Where(name => !lookup.Contains(Text(name).Trim().ToLowerInvariant())).ToList();
            if (!pinned.SequenceEqual(state.PinnedSandboxes))
            {
                state.PinnedSandboxes = pinned;
                changed = true;
            }
            foreach (string key in state.SandboxVolumeMemory.Keys.ToList())
            {
                if (lookup.Contains(Text(key).Trim().ToLowerInvariant()))
                {
                    state.SandboxVolumeMemory.Remove(key);
                    changed = true;
                }
            }
            return changed;
        }

        public bool RenameScheduleJobReferencesByDisplay(
            SandboxAdminState state,
            IDictionary<string, string> displayRenameMap)
        {
            IDictionary<string, string> lookup = BuildCaseInsensitiveMap(displayRenameMap);
            if (state?.ScheduleJobs == null || lookup.Count == 0)
            {
                return false;
            }

            bool changed = false;
            foreach (SandboxScheduleJob job in state.ScheduleJobs.Values)
            {
                if (job == null || job.SandboxTargets == null)
                {
                    continue;
                }
                var targets = job.SandboxTargets.Select(name =>
                    lookup.ContainsKey(Text(name).Trim().ToLowerInvariant())
                        ? lookup[Text(name).Trim().ToLowerInvariant()]
                        : name).Where(name => !string.IsNullOrEmpty(name)).Distinct().ToList();
                if (!targets.SequenceEqual(job.SandboxTargets))
                {
                    job.SandboxTargets = targets;
                    changed = true;
                }
                UpdateScheduleDescription(job);
            }
            return changed;
        }

        public bool DeleteScheduleJobReferencesByDisplay(
            SandboxAdminState state,
            IEnumerable<string> namesToDelete,
            SandboxAdminCallbacks callbacks)
        {
            if (state?.ScheduleJobs == null)
            {
                return false;
            }
            ISet<string> lookup = new HashSet<string>(
                (namesToDelete ?? Enumerable.Empty<string>()).Select(Text).Select(value => value.Trim().ToLowerInvariant())
                    .Where(value => value.Length != 0));
            if (lookup.Count == 0)
            {
                return false;
            }

            bool changed = false;
            var empty = new List<string>();
            foreach (KeyValuePair<string, SandboxScheduleJob> item in state.ScheduleJobs)
            {
                SandboxScheduleJob job = item.Value;
                if (job == null || job.SandboxTargets == null)
                {
                    continue;
                }
                var targets = job.SandboxTargets
                    .Where(name => !lookup.Contains(Text(name).Trim().ToLowerInvariant())).ToList();
                if (!targets.SequenceEqual(job.SandboxTargets))
                {
                    job.SandboxTargets = targets;
                    changed = true;
                }
                if (targets.Count == 0)
                {
                    empty.Add(item.Key);
                }
                else
                {
                    UpdateScheduleDescription(job);
                }
            }
            foreach (string key in empty)
            {
                state.ScheduleJobs.Remove(key);
                callbacks?.CancelScheduleTask?.Invoke(key, false);
            }
            return changed;
        }

        public Tuple<bool, string> RenameSandboxDirectories(
            string sandboxRoot,
            IDictionary<string, string> renameMap)
        {
            if (renameMap == null || renameMap.Count == 0)
            {
                return Tuple.Create(true, string.Empty);
            }

            string root = Text(sandboxRoot).Trim();
            Directory.CreateDirectory(root);
            var temporary = new Dictionary<string, string>(StringComparer.Ordinal);
            var finalized = new List<Tuple<string, string>>();
            string seed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            try
            {
                int index = 0;
                foreach (KeyValuePair<string, string> item in renameMap)
                {
                    string oldName = Text(item.Key).Trim();
                    string temporaryName = "__sbx_tmp_" + seed + "_" + index++;
                    string temporaryPath = Path.Combine(root, temporaryName);
                    int suffix = 0;
                    while (Directory.Exists(temporaryPath))
                    {
                        temporaryName = "__sbx_tmp_" + seed + "_" + (index - 1) + "_" + (++suffix);
                        temporaryPath = Path.Combine(root, temporaryName);
                    }

                    string oldPath = Path.Combine(root, oldName);
                    if (!Directory.Exists(oldPath))
                    {
                        throw new DirectoryNotFoundException("未找到沙盒目录: " + oldName);
                    }
                    Directory.Move(oldPath, temporaryPath);
                    temporary[oldName] = temporaryName;
                }

                foreach (KeyValuePair<string, string> item in renameMap)
                {
                    string oldName = Text(item.Key).Trim();
                    string newName = Text(item.Value).Trim();
                    Directory.Move(Path.Combine(root, temporary[oldName]), Path.Combine(root, newName));
                    finalized.Add(Tuple.Create(oldName, newName));
                }
                return Tuple.Create(true, string.Empty);
            }
            catch (Exception error)
            {
                foreach (Tuple<string, string> item in Enumerable.Reverse(finalized))
                {
                    string newPath = Path.Combine(root, item.Item2);
                    string oldPath = Path.Combine(root, item.Item1);
                    if (Directory.Exists(newPath) && !Directory.Exists(oldPath))
                    {
                        try { Directory.Move(newPath, oldPath); } catch { }
                    }
                }
                foreach (KeyValuePair<string, string> item in temporary)
                {
                    string temporaryPath = Path.Combine(root, item.Value);
                    string oldPath = Path.Combine(root, item.Key);
                    if (Directory.Exists(temporaryPath) && !Directory.Exists(oldPath))
                    {
                        try { Directory.Move(temporaryPath, oldPath); } catch { }
                    }
                }
                return Tuple.Create(false, error.Message);
            }
        }

        public bool SafeDeleteDirectory(string directoryPath)
        {
            string path = Text(directoryPath).Trim();
            if (!Directory.Exists(path))
            {
                return true;
            }
            try
            {
                DeleteDirectoryWithReadonlyFiles(path);
                return !Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        private static void DeleteDirectoryWithReadonlyFiles(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new IOException("删除目录不能为链接: " + path);
            foreach (string file in Directory.GetFiles(path))
            {
                FileAttributes fileAttributes = File.GetAttributes(file);
                if ((fileAttributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("删除文件不能为链接: " + file);
                if ((fileAttributes & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(file, fileAttributes & ~FileAttributes.ReadOnly);
                File.Delete(file);
            }
            foreach (string child in Directory.GetDirectories(path)) DeleteDirectoryWithReadonlyFiles(child);
            if ((attributes & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            Directory.Delete(path);
        }

        public SandboxAliasResult ApplySandboxAliases(
            IDictionary<string, string> aliasMap,
            Func<string, string, bool> setAlias)
        {
            var result = new SandboxAliasResult();
            var changes = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> item in aliasMap ?? new Dictionary<string, string>())
            {
                string name = Text(item.Key).Trim();
                if (name.Length != 0)
                {
                    changes[name] = Text(item.Value).Trim();
                }
            }
            if (changes.Count == 0)
            {
                return result;
            }

            var failed = new List<string>();
            foreach (KeyValuePair<string, string> item in changes)
            {
                bool success = false;
                try
                {
                    success = setAlias != null && setAlias(item.Key, item.Value);
                }
                catch
                {
                }
                if (success)
                {
                    result.Changed = true;
                }
                else
                {
                    failed.Add(item.Key);
                }
            }

            if (failed.Count != 0)
            {
                string preview = string.Join("、", failed.Take(8));
                if (failed.Count > 8)
                {
                    preview += " 等 " + failed.Count + " 个沙盒";
                }
                result.Error = "以下沙盒别名更新失败：" + preview;
            }
            return result;
        }

        public IList<string> BuildSandboxRegistryPaths(IEnumerable<string> sandboxNames)
        {
            var bases = new[]
            {
                @"HKCU\Software\Sandboxie\Boxes",
                @"HKCU\Software\Sandboxie-Plus\Boxes",
                @"HKCU\Software\SandboxiePlus\Boxes",
                @"HKLM\SOFTWARE\Sandboxie\Boxes",
                @"HKLM\SOFTWARE\Sandboxie-Plus\Boxes",
                @"HKLM\SOFTWARE\SandboxiePlus\Boxes",
                @"HKLM\SOFTWARE\WOW6432Node\Sandboxie\Boxes",
                @"HKLM\SOFTWARE\WOW6432Node\Sandboxie-Plus\Boxes",
                @"HKLM\SOFTWARE\WOW6432Node\SandboxiePlus\Boxes"
            };
            var paths = new List<string>();
            foreach (string name in sandboxNames ?? Enumerable.Empty<string>())
            {
                string value = Text(name).Trim();
                if (value.Length == 0)
                {
                    continue;
                }
                paths.AddRange(bases.Select(root => root + "\\" + value));
            }
            return paths;
        }

        public SandboxRegistryCleanupResult CleanupSandboxRegistryEntries(IEnumerable<string> sandboxNames)
        {
            return CleanupSandboxRegistryEntries(sandboxNames, DeleteRegistryPath);
        }

        public SandboxRegistryCleanupResult CleanupSandboxRegistryEntries(
            IEnumerable<string> sandboxNames,
            Func<string, bool> deletePath)
        {
            var result = new SandboxRegistryCleanupResult();
            foreach (string path in BuildSandboxRegistryPaths(sandboxNames))
            {
                bool success = false;
                try
                {
                    success = deletePath != null && deletePath(path);
                }
                catch
                {
                }
                if (success) result.Cleaned++;
                else result.FailedPaths.Add(path);
            }
            return result;
        }

        public bool EnsureSandboxStoppedBeforeDelete(
            string sandboxName,
            double timeoutSeconds,
            SandboxAdminCallbacks callbacks)
        {
            string name = Text(sandboxName).Trim();
            if (name.Length == 0 || callbacks?.GetSandboxPids == null)
            {
                return false;
            }

            IList<int> tracked;
            try
            {
                IEnumerable<int> queried = callbacks.GetSandboxPids(name);
                if (queried == null) return false;
                tracked = NormalizePids(queried);
            }
            catch
            {
                return false;
            }
            if (tracked.Count == 0)
            {
                return true;
            }

            try { callbacks?.TerminateSandbox?.Invoke(name); } catch { }
            IList<int> remaining = WaitForPidsExit(tracked, timeoutSeconds, callbacks?.GetAlivePids);
            if (remaining.Count == 0)
            {
                return true;
            }

            try { callbacks?.TerminatePids?.Invoke(remaining); } catch { }
            remaining = WaitForPidsExit(remaining, 3.0, callbacks?.GetAlivePids);
            return remaining.Count == 0;
        }

        private bool DeleteRegistryPath(string path)
        {
            string value = Text(path);
            int separator = value.IndexOf('\\');
            if (separator <= 0 || separator == value.Length - 1)
            {
                return true;
            }

            string hiveName = value.Substring(0, separator);
            string subkey = value.Substring(separator + 1);
            Microsoft.Win32.RegistryKey hive = string.Equals(hiveName, "HKCU", StringComparison.OrdinalIgnoreCase)
                ? Microsoft.Win32.Registry.CurrentUser
                : Microsoft.Win32.Registry.LocalMachine;
            return DeleteRegistryTree(hive, subkey);
        }

        private bool DeleteRegistryTree(Microsoft.Win32.RegistryKey hive, string subkey)
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key = hive.OpenSubKey(subkey, true))
                {
                    if (key == null)
                    {
                        return true;
                    }
                    foreach (string child in key.GetSubKeyNames())
                    {
                        DeleteRegistryTree(key, child);
                    }
                }
                string parent = Path.GetDirectoryName(subkey)?.Replace('/', '\\');
                string leaf = Path.GetFileName(subkey);
                using (Microsoft.Win32.RegistryKey parentKey = string.IsNullOrEmpty(parent)
                    ? hive
                    : hive.OpenSubKey(parent, true))
                {
                    if (parentKey != null && !string.IsNullOrEmpty(leaf))
                    {
                        parentKey.DeleteSubKeyTree(leaf, false);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private IList<int> WaitForPidsExit(
            IEnumerable<int> pids,
            double timeoutSeconds,
            Func<IEnumerable<int>, IList<int>> getAlivePids)
        {
            IList<int> tracked = NormalizePids(pids);
            if (tracked.Count == 0 || getAlivePids == null)
            {
                return tracked;
            }

            double seconds = double.IsNaN(timeoutSeconds) || double.IsInfinity(timeoutSeconds)
                ? 1.0
                : Math.Max(1.0, timeoutSeconds);
            DateTime deadline = DateTime.UtcNow.AddSeconds(seconds);
            IList<int> remaining = tracked;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    IList<int> queried = getAlivePids(tracked);
                    if (queried == null) return tracked;
                    remaining = NormalizePids(queried);
                }
                catch
                {
                    remaining = tracked;
                }
                if (remaining.Count == 0)
                {
                    return remaining;
                }
                Thread.Sleep(150);
            }
            try
            {
                IList<int> queried = getAlivePids(tracked);
                remaining = queried == null ? tracked : NormalizePids(queried);
            }
            catch
            {
            }
            return remaining;
        }

        private static bool IsEditableSandboxIniSectionName(string name)
        {
            string value = Text(name).Trim();
            return value.Length != 0
                && !IsReservedSandboxIniSectionName(value)
                && LauncherConstants.IsValidInternalSandboxName(value);
        }

        private static bool IsReservedSandboxIniSectionName(string name)
        {
            string value = Text(name).Trim();
            return string.Equals(value, "GlobalSettings", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("UserSettings_", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSandboxConfigSection(string name, string block)
        {
            if (!IsEditableSandboxIniSectionName(name))
            {
                return false;
            }
            string value = GetIniSettingValue(block, "Enabled");
            return value.Equals("y", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("1", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetIniSettingValue(string block, string settingName)
        {
            string target = Text(settingName).Trim();
            foreach (string rawLine in SplitLinesKeepingEndings(Text(block)))
            {
                string line = rawLine.Trim();
                int separator = line.IndexOf('=');
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) || separator < 0)
                {
                    continue;
                }
                if (line.Substring(0, separator).Trim().Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(separator + 1).Trim();
                }
            }
            return string.Empty;
        }

        private static void GetValidSandboxLookups(
            SandboxAdminCallbacks callbacks,
            out ISet<string> displayNames,
            out ISet<string> internalNames)
        {
            displayNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string name in callbacks?.GetExistingSandboxDisplayNames?.Invoke()
                    ?? Enumerable.Empty<string>())
                {
                    string value = Text(name).Trim();
                    if (value.Length != 0) displayNames.Add(value);
                }
            }
            catch
            {
            }
            try
            {
                foreach (string name in callbacks?.GetExistingSandboxNames?.Invoke()
                    ?? Enumerable.Empty<string>())
                {
                    string value = Text(name).Trim();
                    if (value.Length != 0) internalNames.Add(value);
                }
            }
            catch
            {
            }
        }

        private static string InvokeNormalize(SandboxAdminCallbacks callbacks, string name)
        {
            try
            {
                return Text(callbacks?.NormalizeSandboxName == null ? name : callbacks.NormalizeSandboxName(name)).Trim();
            }
            catch
            {
                return name;
            }
        }

        private static string InvokeDisplay(SandboxAdminCallbacks callbacks, string name)
        {
            try
            {
                return Text(callbacks?.GetSandboxDisplayName == null ? string.Empty : callbacks.GetSandboxDisplayName(name)).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string InvokeVolumeKey(SandboxAdminCallbacks callbacks, string name)
        {
            try
            {
                string value = callbacks?.GetVolumeMemoryKey == null ? name : callbacks.GetVolumeMemoryKey(name);
                return Text(value).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static IDictionary<string, string> BuildCaseInsensitiveMap(IDictionary<string, string> source)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> item in source ?? new Dictionary<string, string>())
            {
                string oldName = Text(item.Key).Trim();
                string newName = Text(item.Value).Trim();
                if (oldName.Length != 0 && newName.Length != 0)
                {
                    result[oldName] = newName;
                }
            }
            return result;
        }

        private static void UpdateScheduleDescription(SandboxScheduleJob job)
        {
            if (job == null || job.SandboxTargets == null || job.SandboxTargets.Count == 0)
            {
                return;
            }
            if (Text(job.Description).Contains("定时启动"))
            {
                job.Description = "定时启动 " + job.SandboxTargets.Count + " 个沙盒";
            }
            else if (Text(job.Description).Contains("定时关闭"))
            {
                job.Description = "定时关闭 " + job.SandboxTargets.Count + " 个沙盒";
            }
        }

        private static bool DictionaryEquals<T>(IDictionary<string, T> left, IDictionary<string, T> right)
        {
            if (left == null || right == null || left.Count != right.Count)
            {
                return left == null && right == null;
            }
            foreach (KeyValuePair<string, T> item in left)
            {
                T value;
                if (!right.TryGetValue(item.Key, out value) || !EqualityComparer<T>.Default.Equals(item.Value, value))
                {
                    return false;
                }
            }
            return true;
        }

        private static IList<int> NormalizePids(IEnumerable<int> values)
        {
            return (values ?? Enumerable.Empty<int>()).Where(value => value > 0).Distinct().OrderBy(value => value).ToList();
        }

        private static bool TryDouble(object value, out double result)
        {
            result = 0;
            try
            {
                result = Convert.ToDouble(value);
                return !double.IsNaN(result) && !double.IsInfinity(result);
            }
            catch
            {
                return false;
            }
        }

        private static object GetDictionaryValue(IDictionary dictionary, string key)
        {
            foreach (DictionaryEntry item in dictionary)
            {
                if (string.Equals(Text(item.Key), key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }
            return null;
        }

        private static IEnumerable<object> Values(object value)
        {
            if (value == null)
            {
                yield break;
            }
            if (value is string)
            {
                yield return value;
                yield break;
            }
            IEnumerable sequence = value as IEnumerable;
            if (sequence == null)
            {
                yield break;
            }
            foreach (object item in sequence)
            {
                yield return item;
            }
        }

        private static IList<string> SplitLinesKeepingEndings(string text)
        {
            string value = Text(text);
            var lines = new List<string>();
            int start = 0;
            while (start < value.Length)
            {
                int newline = value.IndexOf('\n', start);
                if (newline < 0)
                {
                    lines.Add(value.Substring(start));
                    break;
                }
                lines.Add(value.Substring(start, newline - start + 1));
                start = newline + 1;
            }
            if (value.Length == 0)
            {
                return lines;
            }
            return lines;
        }

        private static string Text(object value)
        {
            return value == null ? string.Empty : Convert.ToString(value) ?? string.Empty;
        }
    }
}
