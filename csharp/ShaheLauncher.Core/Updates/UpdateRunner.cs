using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace ShaheLauncher.Core.Updates
{
    public static class UpdateRunner
    {
        public static string LastError { get; private set; } = string.Empty;

        public static int Run(string[] args)
        {
            return Run(args, DownloadPackage, RestartApplication);
        }

        public static int Run(
            string[] args,
            Action<string, string> downloadPackage,
            Action<string> restartApplication)
        {
            LastError = string.Empty;
            string workDirectory = string.Empty;
            string backupDirectory = string.Empty;
            bool preserveBackup = false;
            try
            {
                UpdateInstallArguments arguments = UpdateInstaller.ParseArguments(args);
                if (arguments.RequestFile.Length == 0
                    || arguments.AppDirectory.Length == 0
                    || arguments.ExecutablePath.Length == 0)
                {
                    return Fail(2, "更新参数不完整");
                }

                UpdateInstallRequest request = UpdateInstaller.LoadRequest(arguments.RequestFile);
                if (request.Url.Length == 0 || request.Sha256.Length == 0 || request.Size <= 0)
                {
                    return Fail(2, "更新请求缺少下载地址、文件大小或 SHA-256");
                }

                if (!WaitForProcessExit(arguments.ProcessId, 30000))
                {
                    return Fail(3, "等待主程序退出超时");
                }

                workDirectory = Path.Combine(Path.GetTempPath(), "ShaheUpdater_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(workDirectory);
                string packagePath = Path.Combine(workDirectory, "update.package");
                downloadPackage(request.Url, packagePath);
                if (!ValidatePackage(packagePath, request.Size, request.Sha256))
                {
                    return Fail(4, "更新包大小或 SHA-256 校验失败");
                }

                if (request.Signature.Length != 0)
                {
                    UpdateSecurity.VerifyUpdateSignature(packagePath, request.Signature);
                }

                string zipPath = packagePath;
                if (request.Encrypted)
                {
                    zipPath = Path.Combine(workDirectory, "update.zip");
                    UpdateSecurity.DecryptUpdatePackage(packagePath, zipPath);
                }

                string packageDirectory = Path.Combine(workDirectory, "package");
                ExtractPackage(zipPath, packageDirectory);
                backupDirectory = Path.Combine(workDirectory, "backup");
                string mainExecutableName = Path.GetFileName(arguments.ExecutablePath);
                preserveBackup = true;
                if (!InstallPackage(packageDirectory, arguments.AppDirectory, backupDirectory, mainExecutableName))
                {
                    return Fail(5, LastError + (Directory.Exists(backupDirectory)
                        ? "\n恢复备份已保留: " + backupDirectory
                        : string.Empty));
                }

                if (request.Restart)
                {
                    restartApplication(arguments.ExecutablePath);
                }

                preserveBackup = false;
                return 0;
            }
            catch (Exception error)
            {
                return Fail(1, "自动更新失败: " + error.Message
                    + (preserveBackup && Directory.Exists(backupDirectory)
                        ? "\n恢复备份已保留: " + backupDirectory
                        : string.Empty));
            }
            finally
            {
                try
                {
                    if (!preserveBackup && workDirectory.Length != 0 && Directory.Exists(workDirectory))
                    {
                        Directory.Delete(workDirectory, true);
                    }
                }
                catch
                {
                }
            }
        }

        public static bool ValidatePackage(string packagePath, long expectedSize, string expectedSha256)
        {
            string hash = (expectedSha256 ?? string.Empty).Trim();
            return File.Exists(packagePath)
                && expectedSize > 0
                && new FileInfo(packagePath).Length == expectedSize
                && hash.Length != 0
                && UpdateInstaller.ValidateSha256(packagePath, hash);
        }

        public static void DownloadPackage(string url, string targetPath)
        {
            using (var client = new UpdateHttpClient(new System.Net.Http.HttpClientHandler()))
            {
                byte[] data = client.DownloadBytes(new Uri(url, UriKind.Absolute), 30, false);
                File.WriteAllBytes(targetPath, data ?? new byte[0]);
            }
        }

        public static void ExtractPackage(string packagePath, string targetDirectory)
        {
            if (!File.Exists(packagePath))
            {
                throw new FileNotFoundException("更新包不存在", packagePath);
            }

            string root = Path.GetFullPath(targetDirectory);
            Directory.CreateDirectory(root);
            using (ZipArchive archive = ZipFile.OpenRead(packagePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.Length == 0)
                    {
                        continue;
                    }

                    string destination = Path.GetFullPath(Path.Combine(root, entry.FullName));
                    if (!destination.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("更新包含有不安全路径: " + entry.FullName);
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    entry.ExtractToFile(destination, true);
                }
            }
        }

        public static bool InstallPackage(
            string sourceDirectory,
            string appDirectory,
            string backupDirectory,
            string mainExecutableName)
        {
            LastError = string.Empty;
            if (!Directory.Exists(sourceDirectory)
                || string.IsNullOrEmpty(mainExecutableName)
                || !File.Exists(Path.Combine(sourceDirectory, mainExecutableName)))
            {
                LastError = "更新包缺少主程序";
                return false;
            }

            bool backupCompleted = false;
            var attemptedFiles = new List<string>();
            try
            {
                if (Directory.Exists(backupDirectory))
                {
                    Directory.Delete(backupDirectory, true);
                }

                CopyTree(appDirectory, backupDirectory);
                backupCompleted = true;
                CopyTree(sourceDirectory, appDirectory, attemptedFiles);
                return true;
            }
            catch (Exception error)
            {
                LastError = (backupCompleted ? "安装更新失败: " : "备份安装目录失败: ") + error.Message;
                if (backupCompleted)
                {
                    string restoreError = RestoreTree(backupDirectory, appDirectory, attemptedFiles);
                    LastError += restoreError.Length == 0
                        ? "\n已恢复原文件。"
                        : "\n部分文件恢复失败: " + restoreError;
                }
                return false;
            }
        }

        public static bool WaitForProcessExit(int processId, int timeoutMilliseconds)
        {
            if (processId <= 0)
            {
                return true;
            }

            DateTime deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(0, timeoutMilliseconds));
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    using (Process process = Process.GetProcessById(processId))
                    {
                        if (process.HasExited)
                        {
                            return true;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        public static string[] BuildRestartCommand(string executablePath)
        {
            return string.IsNullOrEmpty(executablePath) ? new string[0] : new[] { executablePath };
        }

        private static int Fail(int exitCode, string message)
        {
            LastError = message ?? string.Empty;
            return exitCode;
        }

        private static void RestartApplication(string executablePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true
            });
        }

        private static void CopyTree(string source, string target, ICollection<string> attemptedFiles = null)
        {
            if (!Directory.Exists(source))
            {
                return;
            }

            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
            {
                string destination = Path.Combine(target, Path.GetFileName(file));
                attemptedFiles?.Add(destination);
                File.Copy(file, destination, true);
            }

            foreach (string directory in Directory.GetDirectories(source))
            {
                CopyTree(directory, Path.Combine(target, Path.GetFileName(directory)), attemptedFiles);
            }
        }

        private static string RestoreTree(string backup, string target, IEnumerable<string> attemptedFiles)
        {
            if (!Directory.Exists(backup))
            {
                return "备份目录不存在: " + backup;
            }

            string firstError = string.Empty;
            foreach (string file in attemptedFiles)
            {
                try
                {
                    string relativePath = file.Substring(target.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string savedFile = Path.Combine(backup, relativePath);
                    if (File.Exists(savedFile))
                    {
                        if (File.Exists(file)
                            && new FileInfo(file).Length == new FileInfo(savedFile).Length
                            && string.Equals(UpdateInstaller.ComputeSha256(file),
                                UpdateInstaller.ComputeSha256(savedFile), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        File.Copy(savedFile, file, true);
                    }
                    else if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception error)
                {
                    if (firstError.Length == 0)
                    {
                        firstError = file + ": " + error.Message;
                    }
                }
            }

            return firstError;
        }
    }
}
