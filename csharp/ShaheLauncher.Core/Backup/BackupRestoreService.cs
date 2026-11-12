using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ShaheLauncher.Core.Configuration;

namespace ShaheLauncher.Core.Backup
{
    public sealed class BackupRestoreCallbacks
    {
        public Action<string> SetStatus { get; set; }
        public Action<int, string> UpdateProgress { get; set; }
        public Func<bool> IsSandboxieServiceRunning { get; set; }
        public Action StopSandboxieServices { get; set; }
        public Action ReloadSandboxieConfig { get; set; }
        public Action RestartSandboxieServices { get; set; }
        public Action<double> SuppressListRefreshStatus { get; set; }
        public Action RefreshSandboxList { get; set; }
    }

    public sealed class BackupRestoreResult
    {
        public BackupRestoreResult()
        {
            Error = string.Empty;
            Status = string.Empty;
            ControlExecutable = string.Empty;
        }

        public bool Success { get; set; }
        public string Error { get; set; }
        public string Status { get; set; }
        public string ControlExecutable { get; set; }
    }

    public sealed class BackupRestoreService
    {
        public const string BackupFileExtension = ".sbx";
        public const string BackupArchivePrefix = "SandboxBackup_";
        public const string BackupInProgressStatus = "正在备份...";
        public const string BackupCompletedStatus = "备份完成";
        public const string BackupFailedStatus = "备份失败";
        public const string RestoreCompletedStatus = "还原完成";
        public const string RestoreFailedStatus = "还原失败";
        public const string RestartPromptText = "还原已完成。\n是否重启本程序？";

        private static readonly Regex IniHeaderPattern = new Regex(
            @"(?m)^[ \t]*\[(?<name>[^\]\r\n]+)\][^\r\n]*(?:\r?\n)?",
            RegexOptions.CultureInvariant);

        public static void SafeExtractZip(string zipPath, string extractDirectory)
        {
            string root = Path.GetFullPath(Text(extractDirectory));
            Directory.CreateDirectory(root);
            var entries = new List<Tuple<ZipArchiveEntry, string, bool>>();

            using (ZipArchive archive = ZipFile.OpenRead(Text(zipPath)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string name = Text(entry.FullName).Replace('\\', '/');
                    string[] parts = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0 || name.StartsWith("/", StringComparison.Ordinal)
                        || parts.Any(part => part == ".." || part.IndexOf(':') >= 0)
                        || IsSymbolicLink(entry))
                    {
                        throw new InvalidDataException("备份文件包含不安全路径：" + entry.FullName);
                    }

                    string target = Path.GetFullPath(Path.Combine(
                        new[] { root }.Concat(parts.Select(part => part.Replace('/', Path.DirectorySeparatorChar))).ToArray()));
                    if (!IsUnderRoot(root, target))
                    {
                        throw new InvalidDataException("备份文件包含不安全路径：" + entry.FullName);
                    }

                    entries.Add(Tuple.Create(
                        entry,
                        target,
                        name.EndsWith("/", StringComparison.Ordinal)));
                }

                foreach (Tuple<ZipArchiveEntry, string, bool> item in entries)
                {
                    if (item.Item3)
                    {
                        Directory.CreateDirectory(item.Item2);
                        continue;
                    }

                    string parent = Path.GetDirectoryName(item.Item2);
                    if (!string.IsNullOrEmpty(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }
                    using (Stream source = item.Item1.Open())
                    using (FileStream destination = File.Create(item.Item2))
                    {
                        source.CopyTo(destination);
                    }
                }
            }
        }

        public string CreateBackupArchive(
            string saveDirectory,
            string sandboxieIniPath,
            string sandboxUsersRoot,
            IEnumerable<string> selectedUsers,
            DateTime timestamp)
        {
            string saveRoot = Path.GetFullPath(Text(saveDirectory));
            Directory.CreateDirectory(saveRoot);
            string stamp = timestamp.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string finalPath = Path.Combine(saveRoot, BackupArchivePrefix + stamp + BackupFileExtension);
            string temporaryPath = Path.Combine(saveRoot, "temp_" + stamp + ".zip");
            try
            {
                using (FileStream stream = File.Create(temporaryPath))
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    string iniPath = Text(sandboxieIniPath).Trim();
                    if (File.Exists(iniPath))
                    {
                        archive.CreateEntryFromFile(iniPath, "Sandboxie.ini");
                    }

                    string usersRoot = Path.GetFullPath(Text(sandboxUsersRoot));
                    foreach (string user in CleanNames(selectedUsers))
                    {
                        string userPath = Path.GetFullPath(Path.Combine(usersRoot, user));
                        if (!Directory.Exists(userPath) || !IsDirectChild(usersRoot, userPath))
                        {
                            continue;
                        }
                        AddDirectoryToArchive(archive, userPath, "SandboxUsers/" + user);
                    }
                }

                File.Move(temporaryPath, finalPath);
                return finalPath;
            }
            finally
            {
                TryDeleteFile(temporaryPath);
            }
        }

        public Task<string> CreateBackupArchiveAsync(
            string saveDirectory,
            string sandboxieIniPath,
            string sandboxUsersRoot,
            IEnumerable<string> selectedUsers,
            DateTime timestamp,
            BackupRestoreCallbacks callbacks)
        {
            return Task.Run(delegate
            {
                callbacks?.SetStatus?.Invoke(BackupInProgressStatus);
                try
                {
                    string path = CreateBackupArchive(
                        saveDirectory,
                        sandboxieIniPath,
                        sandboxUsersRoot,
                        selectedUsers,
                        timestamp);
                    callbacks?.SetStatus?.Invoke(BackupCompletedStatus);
                    return path;
                }
                catch
                {
                    callbacks?.SetStatus?.Invoke(BackupFailedStatus);
                    throw;
                }
            });
        }

        public void MergeIniSections(
            string backupIniPath,
            string targetIniPath,
            IEnumerable<string> sectionsToMerge)
        {
            IList<string> wanted = CleanNames(sectionsToMerge)
                .Select(value => value.ToLowerInvariant())
                .ToList();
            if (wanted.Count == 0 || !File.Exists(Text(backupIniPath)))
            {
                return;
            }

            string backupText = IniTextFile.Read(Text(backupIniPath)).Item1;
            Tuple<string, string> loadedTarget = IniTextFile.Read(Text(targetIniPath));
            string targetText = loadedTarget.Item1;
            string lineSeparator = targetText.IndexOf("\r\n", StringComparison.Ordinal) >= 0
                || backupText.IndexOf("\r\n", StringComparison.Ordinal) >= 0
                ? "\r\n"
                : "\n";
            IniDocument backup = SplitIni(backupText);
            IniDocument target = SplitIni(targetText);
            var backupMap = backup.Sections
                .Where(section => section.Name.Length != 0)
                .ToDictionary(section => section.Name.ToLowerInvariant(), section => section.Block);
            var replaced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var merged = new List<IniSection>();
            foreach (IniSection section in target.Sections)
            {
                string key = section.Name.ToLowerInvariant();
                string block;
                if (wanted.Contains(key) && backupMap.TryGetValue(key, out block))
                {
                    merged.Add(new IniSection(section.Name, block));
                    replaced.Add(key);
                }
                else
                {
                    merged.Add(section);
                }
            }

            foreach (string key in wanted)
            {
                if (replaced.Contains(key))
                {
                    continue;
                }
                string block;
                if (backupMap.TryGetValue(key, out block))
                {
                    merged.Add(new IniSection(key, block));
                    replaced.Add(key);
                }
            }

            var builder = new StringBuilder(target.Preamble);
            foreach (IniSection section in merged)
            {
                string block = section.Block;
                if (block.Length != 0 && !block.EndsWith("\n", StringComparison.Ordinal)
                    && !block.EndsWith("\r", StringComparison.Ordinal))
                {
                    block += lineSeparator;
                }
                builder.Append(block);
            }
            IniTextFile.Write(Text(targetIniPath), builder.ToString(), loadedTarget.Item2);
        }

        public Task<BackupRestoreResult> RestoreSelectedUsersAsync(
            string extractedRoot,
            string sandboxUsersRoot,
            string targetIniPath,
            IEnumerable<string> selectedUsers,
            BackupRestoreCallbacks callbacks)
        {
            return Task.Run(() => RestoreSelectedUsers(
                extractedRoot,
                sandboxUsersRoot,
                targetIniPath,
                selectedUsers,
                callbacks));
        }

        public string FindSandboxieControlExecutable(
            string sandboxiePath,
            IEnumerable<string> fallbackRoots)
        {
            var roots = new List<string>();
            string preferredRoot = Text(sandboxiePath).Trim();
            if (preferredRoot.Length != 0)
            {
                roots.Add(preferredRoot);
            }
            roots.AddRange((fallbackRoots ?? Enumerable.Empty<string>())
                .Select(Text).Select(value => value.Trim())
                .Where(value => value.Length != 0));
            foreach (string root in roots)
            {
                foreach (string filename in new[] { "SandMan.exe", "SbieCtrl.exe", "Sandboxie.exe" })
                {
                    string candidate = Path.Combine(root, filename);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }
            return string.Empty;
        }

        public static string FormatRestoreSuccessMessage(bool controlFound)
        {
            return controlFound
                ? "还原成功！\n是否立即启动 Sandboxie Control？"
                : "还原成功！\n未找到 Sandboxie Control，请手动启动。";
        }

        public static string FormatRestoreFailureMessage(string error)
        {
            return "还原失败：" + Text(error);
        }

        public static IList<string> BuildRestartCommand(
            string runtimeExecutable,
            string currentExecutablePath,
            bool packaged)
        {
            string entryPath = Text(currentExecutablePath).Trim();
            if (packaged)
            {
                return new List<string> { entryPath };
            }
            return new List<string> { Text(runtimeExecutable).Trim(), entryPath };
        }

        private BackupRestoreResult RestoreSelectedUsers(
            string extractedRoot,
            string sandboxUsersRoot,
            string targetIniPath,
            IEnumerable<string> selectedUsers,
            BackupRestoreCallbacks callbacks)
        {
            var result = new BackupRestoreResult();
            var replacements = new List<RestoreDirectory>();
            string stagingRoot = null;
            string preparedIni = null;
            string previousIni = null;
            bool createdIni = false;
            bool resumeService = false;
            bool reloadAttempted = false;
            bool rollbackComplete = true;
            try
            {
                string sourceRoot = Path.Combine(Path.GetFullPath(Text(extractedRoot)), "SandboxUsers");
                string destinationRoot = Path.GetFullPath(Text(sandboxUsersRoot));
                IList<string> users = CleanNames(selectedUsers).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (string user in users)
                {
                    string source = Path.GetFullPath(Path.Combine(sourceRoot, user));
                    string destination = Path.GetFullPath(Path.Combine(destinationRoot, user));
                    if (Path.IsPathRooted(user) || user.IndexOfAny(new[] { '/', '\\', ':' }) >= 0
                        || !IsDirectChild(sourceRoot, source) || !IsDirectChild(destinationRoot, destination))
                    {
                        throw new InvalidDataException("无效的备份沙盒用户: " + user);
                    }
                    if (!Directory.Exists(source))
                    {
                        throw new DirectoryNotFoundException("未找到备份沙盒用户: " + user);
                    }
                    if (IsUnderRoot(destination, Path.GetFullPath(targetIniPath)))
                    {
                        throw new InvalidDataException("配置文件不能位于待还原用户目录中。");
                    }
                }

                bool wasRunning = callbacks?.IsSandboxieServiceRunning?.Invoke() ?? true;
                callbacks?.UpdateProgress?.Invoke(10, "清理旧数据...");
                stagingRoot = Path.Combine(destinationRoot, ".shahe-restore-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(stagingRoot);
                foreach (string user in users)
                {
                    var replacement = new RestoreDirectory
                    {
                        Destination = Path.Combine(destinationRoot, user),
                        Prepared = Path.Combine(stagingRoot, "new-" + replacements.Count),
                        Previous = Path.Combine(stagingRoot, "old-" + replacements.Count)
                    };
                    CopyDirectory(Path.Combine(sourceRoot, user), replacement.Prepared);
                    replacements.Add(replacement);
                }
                string backupIni = Path.Combine(Path.GetFullPath(Text(extractedRoot)), "Sandboxie.ini");
                if (File.Exists(backupIni))
                {
                    string iniDirectory = Path.GetDirectoryName(Path.GetFullPath(targetIniPath));
                    Directory.CreateDirectory(iniDirectory);
                    preparedIni = Path.Combine(iniDirectory, ".shahe-restore-" + Guid.NewGuid().ToString("N") + ".ini");
                    previousIni = preparedIni + ".previous";
                    if (File.Exists(targetIniPath)) File.Copy(targetIniPath, preparedIni);
                    MergeIniSections(backupIni, preparedIni, users);
                }

                if (wasRunning)
                {
                    // A stop callback can stop the service and then throw.
                    resumeService = true;
                    callbacks?.StopSandboxieServices?.Invoke();
                }
                callbacks?.UpdateProgress?.Invoke(30, "还原用户数据...");
                foreach (RestoreDirectory replacement in replacements)
                {
                    if (Directory.Exists(replacement.Destination))
                    {
                        CheckRestoreDirectory(replacement.Destination);
                        Directory.Move(replacement.Destination, replacement.Previous);
                        replacement.OldMoved = true;
                    }
                    Directory.Move(replacement.Prepared, replacement.Destination);
                    replacement.NewMoved = true;
                }
                callbacks?.UpdateProgress?.Invoke(70, "合并配置文件...");
                if (preparedIni != null)
                {
                    if (File.Exists(targetIniPath)) File.Replace(preparedIni, targetIniPath, previousIni);
                    else
                    {
                        File.Move(preparedIni, targetIniPath);
                        createdIni = true;
                    }
                }

                if (wasRunning)
                {
                    callbacks?.UpdateProgress?.Invoke(90, "重载配置...");
                    reloadAttempted = true;
                    callbacks?.ReloadSandboxieConfig?.Invoke();
                    callbacks?.RestartSandboxieServices?.Invoke();
                    resumeService = false;
                }
                result.Success = true;
            }
            catch (Exception error)
            {
                result.Error = error.Message;
                if (reloadAttempted && resumeService)
                {
                    // Start.exe /reload may have started the service before failing.
                    try { callbacks?.StopSandboxieServices?.Invoke(); }
                    catch (Exception stopError)
                    {
                        rollbackComplete = false;
                        result.Error += "\n回退前停止服务失败，保留副本 " + stagingRoot
                            + " / " + previousIni + ": " + stopError.Message;
                    }
                }
                bool canRollback = rollbackComplete;
                // Never discard the old copies when rollback itself is blocked.
                foreach (RestoreDirectory replacement in replacements.AsEnumerable().Reverse())
                {
                    if (!canRollback) break;
                    try
                    {
                        if (replacement.NewMoved) DeleteRestoreDirectory(replacement.Destination);
                        if (replacement.OldMoved) Directory.Move(replacement.Previous, replacement.Destination);
                    }
                    catch (Exception rollbackError)
                    {
                        rollbackComplete = false;
                        result.Error += "\n数据回退失败，保留副本 " + replacement.Previous + ": " + rollbackError.Message;
                    }
                }
                try
                {
                    if (canRollback && previousIni != null && File.Exists(previousIni))
                    {
                        if (File.Exists(targetIniPath)) File.Replace(previousIni, targetIniPath, null);
                        else File.Move(previousIni, targetIniPath);
                    }
                    else if (canRollback && createdIni) File.Delete(targetIniPath);
                }
                catch (Exception rollbackError)
                {
                    rollbackComplete = false;
                    result.Error += "\n配置回退失败，保留副本 " + previousIni + ": " + rollbackError.Message;
                }
                if (resumeService && rollbackComplete)
                {
                    if (reloadAttempted)
                    {
                        try { callbacks?.ReloadSandboxieConfig?.Invoke(); }
                        catch (Exception reloadError) { result.Error += "\n重新加载原配置失败: " + reloadError.Message; }
                    }
                    try { callbacks?.RestartSandboxieServices?.Invoke(); }
                    catch (Exception restartError) { result.Error += "\n恢复服务失败: " + restartError.Message; }
                }
                else if (resumeService)
                {
                    result.Error += "\n回退未完成，未启动服务，请使用保留副本恢复数据和配置。";
                }
            }
            finally
            {
                if (rollbackComplete)
                {
                    try
                    {
                        if (stagingRoot != null) DeleteRestoreDirectory(stagingRoot);
                        if (preparedIni != null) TryDeleteFile(preparedIni);
                        if (previousIni != null) TryDeleteFile(previousIni);
                    }
                    catch (Exception cleanupError)
                    {
                        result.Error += "\n清理还原临时目录失败 " + stagingRoot + ": " + cleanupError.Message;
                        result.Success = false;
                    }
                }
            }

            result.Status = result.Success ? RestoreCompletedStatus : RestoreFailedStatus;
            if (result.Success)
            {
                callbacks?.UpdateProgress?.Invoke(100, "完成");
                callbacks?.SuppressListRefreshStatus?.Invoke(2.0);
                callbacks?.RefreshSandboxList?.Invoke();
            }
            callbacks?.SetStatus?.Invoke(result.Status);
            return result;
        }

        private sealed class RestoreDirectory
        {
            public string Destination;
            public string Prepared;
            public string Previous;
            public bool OldMoved;
            public bool NewMoved;
        }

        private static void CheckRestoreDirectory(string directory)
        {
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("还原目录不能包含链接: " + directory);
            foreach (string file in Directory.GetFiles(directory))
            {
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("还原文件不能为链接: " + file);
                using (File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None)) { }
            }
            foreach (string child in Directory.GetDirectories(directory)) CheckRestoreDirectory(child);
        }

        private static void DeleteRestoreDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("还原清理目录不能为链接: " + directory);
            foreach (string file in Directory.GetFiles(directory))
            {
                File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
                File.Delete(file);
            }
            foreach (string child in Directory.GetDirectories(directory)) DeleteRestoreDirectory(child);
            Directory.Delete(directory);
        }

        private static void AddDirectoryToArchive(ZipArchive archive, string directoryPath, string archiveRoot)
        {
            DirectoryInfo root = new DirectoryInfo(directoryPath);
            AddEmptyDirectoryEntry(archive, archiveRoot + "/", root);
            foreach (FileInfo file in root.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string relative = file.FullName.Substring(root.FullName.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '/');
                archive.CreateEntryFromFile(file.FullName, archiveRoot + "/" + relative);
            }

            foreach (DirectoryInfo directory in root.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                if (!directory.EnumerateFileSystemInfos().Any())
                {
                    string relative = directory.FullName.Substring(root.FullName.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    AddEmptyDirectoryEntry(archive, archiveRoot + "/" + relative + "/", directory);
                }
            }
        }

        private static void AddEmptyDirectoryEntry(ZipArchive archive, string name, DirectoryInfo directory)
        {
            if (!directory.EnumerateFileSystemInfos().Any())
            {
                archive.CreateEntry(name.EndsWith("/", StringComparison.Ordinal) ? name : name + "/");
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("还原目录不能包含链接: " + source);
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
            {
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("还原文件不能为链接: " + file);
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }
            foreach (string directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
            }
        }

        private static IniDocument SplitIni(string text)
        {
            string content = Text(text);
            MatchCollection matches = IniHeaderPattern.Matches(content);
            if (matches.Count == 0)
            {
                return new IniDocument(content, new List<IniSection>());
            }
            string preamble = content.Substring(0, matches[0].Index);
            var sections = new List<IniSection>();
            for (int index = 0; index < matches.Count; index++)
            {
                Match match = matches[index];
                int end = index + 1 < matches.Count ? matches[index + 1].Index : content.Length;
                sections.Add(new IniSection(
                    Text(match.Groups["name"].Value).Trim(),
                    content.Substring(match.Index, end - match.Index)));
            }
            return new IniDocument(preamble, sections);
        }

        private static IList<string> CleanNames(IEnumerable<string> values)
        {
            var names = new List<string>();
            foreach (string value in values ?? Enumerable.Empty<string>())
            {
                string name = Text(value).Trim();
                if (name.Length != 0 && !names.Contains(name, StringComparer.Ordinal))
                {
                    names.Add(name);
                }
            }
            return names;
        }

        private static bool IsDirectChild(string root, string path)
        {
            string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(Path.GetDirectoryName(normalizedPath), normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUnderRoot(string root, string path)
        {
            string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSymbolicLink(ZipArchiveEntry entry)
        {
            PropertyInfo property = typeof(ZipArchiveEntry).GetProperty("ExternalAttributes");
            if (property == null)
            {
                return false;
            }
            try
            {
                int attributes = Convert.ToInt32(property.GetValue(entry, null), CultureInfo.InvariantCulture);
                int mode = (attributes >> 16) & 0xFFFF;
                return (mode & 0xF000) == 0xA000;
            }
            catch
            {
                return false;
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    FileAttributes attributes = File.GetAttributes(path);
                    if ((attributes & FileAttributes.ReparsePoint) != 0) return;
                    if ((attributes & FileAttributes.ReadOnly) != 0)
                        File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }

        private static string Text(object value)
        {
            return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private sealed class IniDocument
        {
            public IniDocument(string preamble, IList<IniSection> sections)
            {
                Preamble = preamble;
                Sections = sections;
            }

            public string Preamble { get; private set; }
            public IList<IniSection> Sections { get; private set; }
        }

        private sealed class IniSection
        {
            public IniSection(string name, string block)
            {
                Name = name;
                Block = block;
            }

            public string Name { get; private set; }
            public string Block { get; private set; }
        }
    }
}
