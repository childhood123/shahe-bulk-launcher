using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Constants;
using ShaheLauncher.Core.SandboxAdmin;
using ShaheLauncher.Core.Sandboxie;
using ShaheLauncher.Core.Scheduling;

namespace ShaheLauncher.AuditRunner
{
    public sealed class SandboxieLifecycleAudit
    {
        public int Run(AuditOptions options)
        {
            if (options == null)
            {
                throw new InvalidOperationException("Audit options are required.");
            }
            switch (options.Mode)
            {
                case AuditMode.Contract:
                    return RunContract(options);
                case AuditMode.Preflight:
                    return RunPreflight(options);
                case AuditMode.DialogGate:
                    return RunDialogGate(options);
                case AuditMode.Lifecycle:
                    return RunLifecycle(options);
                case AuditMode.VerifyClean:
                    return RunVerifyClean(options);
                default:
                    return WriteUnsupported(options);
            }
        }

        private static int RunContract(AuditOptions options)
        {
            var checks = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            try
            {
                AuditOptions lifecycle = AuditCommandLine.Parse(new[]
                {
                    "lifecycle", "--execute", "--baseline", "baseline.json",
                    "--prefix", "ShaheAudit_Contract", "--stage", "batch-create",
                    "--state", "state.json"
                });
                checks["lifecycleGate"] = lifecycle.Execute
                    && lifecycle.BaselinePath == "baseline.json"
                    && lifecycle.Prefix == "ShaheAudit_Contract"
                    && lifecycle.Stage == "batch-create"
                    && lifecycle.StatePath == "state.json";
                checks["defaultWriteRefusal"] = IsRejected(() => AuditCommandLine.Parse(new[] { "lifecycle" }));
                checks["dynamicMenuLookup"] = AuditSafety.FindUniqueMenuCommand(
                    new[] { new MenuCommandInfo { Id = 719, Title = "退出软件\tAlt+F4" } },
                    "退出软件\tAlt+F4") == 719;

                var claim = new OwnershipClaim
                {
                    InternalName = "Box901",
                    Alias = "ShaheAudit_Contract_01",
                    DirectoryName = "Box901"
                };
                IList<OwnershipClaim> owned = AuditSafety.SelectOwnedResources(
                    new OwnershipSnapshot { IniReadable = true },
                    new OwnershipSnapshot
                    {
                        IniReadable = true,
                        AliasesBySection = new Dictionary<string, string> { { "Box901", claim.Alias } },
                        Directories = new List<string> { "Box901" }
                    },
                    new[] { claim });
                checks["uniqueOwnership"] = owned.Count == 1;

                var order = new List<string>();
                AuditStepResult deleted = AuditSafety.DeleteOwnedResource(
                    claim,
                    name => { order.Add("delete"); return 0; },
                    name => { order.Add("directory"); return false; },
                    name => order.Add("section"));
                checks["deleteOrder"] = deleted.Status == AuditStatus.Passed
                    && order.SequenceEqual(new[] { "delete", "directory", "section" });
                checks["failureClassification"] = AuditSafety.ClassifyFailure(AuditFailureKind.Timeout)
                    == AuditStatus.Inconclusive;
            }
            catch
            {
                checks["contractException"] = false;
            }

            bool passed = checks.Count != 0 && checks.Values.All(value => value);
            string path = WriteJson(options.OutputDirectory, "auditrunner-contract", new Dictionary<string, object>
            {
                { "kind", "auditrunner-contract" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "failed" },
                { "checks", checks }
            });
            Console.WriteLine(path);
            return passed ? 0 : 1;
        }

        private static int RunPreflight(AuditOptions options)
        {
            RequireFileOption(options.TargetPath, "--target");
            RequireFileOption(options.ConfigPath, "--config");
            RequireFileOption(options.SandboxieIniPath, "--sandboxie-ini");
            var report = new PreflightReport
            {
                Timestamp = Timestamp(),
                BaselineId = Guid.NewGuid().ToString("N"),
                TargetPath = Path.GetFullPath(options.TargetPath),
                ConfigPath = Path.GetFullPath(options.ConfigPath),
                SandboxieIniPath = Path.GetFullPath(options.SandboxieIniPath)
            };

            report.TargetSha256 = HashFile(report.TargetPath, report.Errors);
            report.ConfigSha256 = HashFile(report.ConfigPath, report.Errors);
            report.SandboxieIniSha256 = HashFile(report.SandboxieIniPath, report.Errors);
            LoadConfiguration(report);
            LoadSandboxieState(report);
            LoadProcesses(report);
            report.ServiceStatus = GetServiceStatus("SbieSvc", report.Errors);
            SetChecks(report, options);
            report.Status = report.Checks.Values.All(value => value) && report.Errors.Count == 0
                ? "passed"
                : "blocked";

            string path = WriteJson(options.OutputDirectory, "auditrunner-preflight", report);
            Console.WriteLine(path);
            return report.Status == "passed" ? 0 : 3;
        }

        private static int RunVerifyClean(AuditOptions options)
        {
            RequireFileOption(options.TargetPath, "--target");
            RequireFileOption(options.ConfigPath, "--config");
            RequireFileOption(options.SandboxieIniPath, "--sandboxie-ini");
            var report = new PreflightReport
            {
                Kind = "auditrunner-verify-clean",
                Timestamp = Timestamp(),
                TargetPath = Path.GetFullPath(options.TargetPath),
                ConfigPath = Path.GetFullPath(options.ConfigPath),
                SandboxieIniPath = Path.GetFullPath(options.SandboxieIniPath)
            };

            report.TargetSha256 = HashFile(report.TargetPath, report.Errors);
            report.ConfigSha256 = HashFile(report.ConfigPath, report.Errors);
            report.SandboxieIniSha256 = HashFile(report.SandboxieIniPath, report.Errors);
            LoadConfiguration(report);
            LoadSandboxieState(report);
            LoadProcesses(report);
            report.ServiceStatus = GetServiceStatus("SbieSvc", report.Errors);
            SetChecks(report, options);
            report.Checks["ownedResources"] = false;
            try
            {
                PreflightReport baseline = LoadLifecycleBaseline(options);
                LifecycleState state = AuditSafety.LoadLifecycleStateForCleanup(
                    options.StatePath, baseline.BaselineId, options.Prefix);
                if (!string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(report.TargetPath, baseline.TargetPath, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(report.ConfigPath, baseline.ConfigPath, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(report.SandboxieIniPath, baseline.SandboxieIniPath, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(report.SandboxRoot, baseline.SandboxRoot, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(options.ExpectedTargetSha256, baseline.TargetSha256, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(options.ExpectedConfigSha256, baseline.ConfigSha256, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(options.ExpectedIniSha256, baseline.SandboxieIniSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Verify-clean inputs do not match the original baseline.");
                }
                AuditStepResult absent = AuditSafety.VerifyOwnedResourcesAbsent(
                    OwnershipFromPreflight(baseline), OwnershipFromPreflight(report), state.OwnershipClaims);
                report.Checks["ownedResources"] = absent.Status == AuditStatus.Passed;
                if (absent.Status != AuditStatus.Passed) report.Errors.Add(absent.Detail);
            }
            catch (Exception exception) { report.Errors.Add("Exact ownership verification: " + exception.Message); }
            report.Status = report.Checks.Values.All(value => value) && report.Errors.Count == 0
                ? "passed"
                : "blocked";

            string path = WriteJson(options.OutputDirectory, "auditrunner-verify-clean", report);
            Console.WriteLine(path);
            return report.Status == "passed" ? 0 : 3;
        }

        private static int RunDialogGate(AuditOptions options)
        {
            RequireFileOption(options.BaselinePath, "--baseline");
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            PreflightReport baseline = serializer.Deserialize<PreflightReport>(File.ReadAllText(options.BaselinePath));
            if (baseline == null || !string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("dialog-gate requires a passed preflight baseline.");
            }

            var result = new Dictionary<string, object>();
            var errors = new List<string>();
            var dialogs = new List<DialogObservation>();
            var commandIds = new Dictionary<string, int>();
            Process process = null;
            IntPtr mainWindow = IntPtr.Zero;
            bool dynamicExit = false;
            bool forcedStop = false;
            try
            {
                if (!string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(HashFile(baseline.ConfigPath, errors), baseline.ConfigSha256, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(HashFile(baseline.SandboxieIniPath, errors), baseline.SandboxieIniSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Baseline hashes changed before dialog-gate.");
                }

                process = Process.Start(new ProcessStartInfo
                {
                    FileName = baseline.TargetPath,
                    WorkingDirectory = Path.GetDirectoryName(baseline.TargetPath) ?? Environment.CurrentDirectory,
                    UseShellExecute = false
                });
                if (process == null)
                {
                    throw new InvalidOperationException("Target process did not start.");
                }
                mainWindow = Win32Automation.WaitForMainWindow(process, 15000);
                if (mainWindow == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Main window was not ready.");
                }

                IList<MenuCommandInfo> commands = Win32Automation.GetMenuCommands(mainWindow);
                foreach (string title in new[]
                {
                    "批量创建沙盒用户", "批量重命名沙盒用户", "批量删除沙盒用户",
                    "新建分组", "启动分组", "删除分组", "定时启动选中沙盒",
                    "定时关闭选中沙盒", "取消全部定时任务", "全选\tCtrl+A", "退出软件\tAlt+F4"
                })
                {
                    commandIds[title] = AuditSafety.FindUniqueMenuCommand(commands, title);
                }

                dialogs.Add(Win32Automation.OpenAndCancelDialog(
                    process.Id, mainWindow, ResolveCommand(mainWindow, commandIds, "批量创建沙盒用户"), "批量创建沙盒用户", 10000));
                if (!Win32Automation.WaitForSandboxItemCount(mainWindow, 2, 10000))
                {
                    throw new InvalidOperationException("Sandbox list did not load at least two entries.");
                }
                if (!Win32Automation.SendCommand(
                    mainWindow,
                    ResolveCommand(mainWindow, commandIds, "全选\tCtrl+A")))
                {
                    throw new InvalidOperationException("Could not invoke the dynamic select-all command.");
                }
                dialogs.Add(Win32Automation.OpenAndCancelDialog(
                    process.Id, mainWindow, ResolveCommand(mainWindow, commandIds, "新建分组"), "新建分组", 10000));
                dialogs.Add(Win32Automation.OpenAndCancelDialog(
                    process.Id, mainWindow, ResolveCommand(mainWindow, commandIds, "批量重命名沙盒用户"), "批量重命名沙盒用户", 10000));
                dialogs.Add(Win32Automation.OpenAndCancelDialog(
                    process.Id, mainWindow, ResolveCommand(mainWindow, commandIds, "批量删除沙盒用户"), "确认批量删除", 10000));
                dialogs.Add(Win32Automation.OpenAndCancelDialog(
                    process.Id, mainWindow, ResolveCommand(mainWindow, commandIds, "定时关闭选中沙盒"), "定时关闭沙盒", 10000));
                bool dialogChecks = DialogPassed(dialogs[0], 3, 1, 2)
                    && DialogPassed(dialogs[1], 1, 0, 2)
                    && DialogPassed(dialogs[2], 3, 0, 2)
                    && DialogPassed(dialogs[3], 0, 0, 2)
                    && DialogPassed(dialogs[4], 1, 0, 2);
                if (!dialogChecks)
                {
                    errors.Add("One or more dialog structure/focus/cancel checks were inconclusive.");
                }

                dynamicExit = Win32Automation.PostCommand(
                        mainWindow,
                        ResolveCommand(mainWindow, commandIds, "退出软件\tAlt+F4"))
                    && Win32Automation.WaitForExit(process, 10000);
                if (!dynamicExit)
                {
                    errors.Add("Dynamic exit command did not close the owned target process.");
                }
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
            }
            finally
            {
                if (process != null && !process.HasExited)
                {
                    Win32Automation.CloseWindow(mainWindow);
                    Win32Automation.WaitForExit(process, 3000);
                }
                if (process != null && !process.HasExited)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                        forcedStop = true;
                    }
                    catch (Exception exception)
                    {
                        errors.Add("Owned target cleanup: " + exception.Message);
                    }
                }
            }

            bool hashesRestored = string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase)
                && string.Equals(HashFile(baseline.ConfigPath, errors), baseline.ConfigSha256, StringComparison.OrdinalIgnoreCase)
                && string.Equals(HashFile(baseline.SandboxieIniPath, errors), baseline.SandboxieIniSha256, StringComparison.OrdinalIgnoreCase);
            if (!hashesRestored)
            {
                errors.Add("One or more baseline hashes changed during dialog-gate.");
            }

            bool passed = errors.Count == 0 && dynamicExit && !forcedStop;
            result["kind"] = "auditrunner-dialog-gate";
            result["timestamp"] = Timestamp();
            result["status"] = passed ? "passed" : "inconclusive";
            result["baselineId"] = baseline.BaselineId;
            result["targetProcessId"] = process == null ? 0 : process.Id;
            result["mainWindow"] = "0x" + mainWindow.ToInt64().ToString("X");
            result["commandIds"] = commandIds;
            result["dialogs"] = dialogs;
            result["menuOnlyChecks"] = new[] { "启动分组", "删除分组", "定时启动选中沙盒", "取消全部定时任务" };
            result["dynamicExit"] = dynamicExit;
            result["forcedStop"] = forcedStop;
            result["hashesRestored"] = hashesRestored;
            result["errors"] = errors;
            string output = WriteJson(options.OutputDirectory, "auditrunner-dialog-gate", result);
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycle(AuditOptions options)
        {
            if (string.Equals(options.Stage, "cleanup", StringComparison.Ordinal))
            {
                return RunLifecycleCleanup(options);
            }
            if (string.Equals(options.Stage, "create-group", StringComparison.Ordinal))
            {
                return RunLifecycleCreateGroup(options);
            }
            if (string.Equals(options.Stage, "launch-group", StringComparison.Ordinal))
            {
                return RunLifecycleLaunchGroup(options);
            }
            if (string.Equals(options.Stage, "schedule-launch", StringComparison.Ordinal))
            {
                return RunLifecycleScheduleLaunch(options);
            }
            if (string.Equals(options.Stage, "schedule-close", StringComparison.Ordinal))
            {
                return RunLifecycleScheduleClose(options);
            }
            if (string.Equals(options.Stage, "cancel-schedules", StringComparison.Ordinal))
            {
                return RunLifecycleCancelSchedules(options);
            }
            if (string.Equals(options.Stage, "batch-rename", StringComparison.Ordinal))
            {
                return RunLifecycleBatchRename(options);
            }
            if (string.Equals(options.Stage, "batch-delete", StringComparison.Ordinal))
            {
                return RunLifecycleBatchDelete(options);
            }
            if (!string.Equals(options.Stage, "batch-create", StringComparison.Ordinal))
            {
                return WriteUnsupported(options);
            }

            RequireFileOption(options.BaselinePath, "--baseline");
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            PreflightReport baseline = serializer.Deserialize<PreflightReport>(File.ReadAllText(options.BaselinePath));
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            var claims = new List<OwnershipClaim>();
            LifecycleState state = null;
            Process process = null;
            IntPtr mainWindow = IntPtr.Zero;
            DialogObservation dialog = null;
            var dialogs = new List<DialogObservation>();
            int commandId = 0;
            bool dynamicExit = false;
            bool forcedStop = false;
            bool passed = false;
            IList<string> mainWindowTexts = new List<string>();
            OwnershipSnapshot baselineOwnership = null;
            string[] expectedAliases = BuildBatchCreateAliases(options.Prefix);
            var officialCreateResults = new List<IDictionary<string, object>>();
            try
            {
                ValidateLifecycleBaseline(baseline, expectedAliases, errors);
                baselineOwnership = OwnershipFromPreflight(baseline);
                EnsureLifecycleProcessGate(baseline.TargetPath);
                EnsureNoAuditResources(ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot));
                state = AuditSafety.LoadOrCreateLifecycleState(
                    options.StatePath,
                    baseline.BaselineId,
                    options.Prefix,
                    options.Stage);

                var reservedInternalNames = new List<string>(baselineOwnership.Sections);
                foreach (string alias in expectedAliases)
                {
                    SandboxieOperationResult created = CreateOwnedSandboxViaOfficial(
                        baseline,
                        baselineOwnership,
                        reservedInternalNames,
                        alias);
                    officialCreateResults.Add(new Dictionary<string, object>
                    {
                        { "alias", alias },
                        { "success", created != null && created.Success },
                        { "internalName", created == null ? string.Empty : created.InternalName },
                        { "detail", created == null ? "No creation result." : created.Detail }
                    });
                    if (created == null || !created.Success || string.IsNullOrWhiteSpace(created.InternalName))
                    {
                        throw new InvalidOperationException(
                            "Official Sandboxie creation failed for " + alias + ": "
                            + (created == null ? "no result" : created.Detail));
                    }
                    reservedInternalNames.Add(created.InternalName.Trim());
                    claims.AddRange(WaitForOwnedClaims(
                        baselineOwnership,
                        baseline.SandboxieIniPath,
                        baseline.SandboxRoot,
                        new[] { alias },
                        GetBatchCreateObservationTimeoutMilliseconds(1)));
                }

                state.OwnershipClaims = claims;
                state.CompletedStages.Add("batch-create");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
            }
            finally
            {
                CloseOwnedTarget(process, mainWindow, ref dynamicExit, ref forcedStop, errors);
                if (!passed && baselineOwnership != null)
                {
                    try
                    {
                        claims = FindAvailableOwnedClaims(
                            baselineOwnership,
                            baseline.SandboxieIniPath,
                            baseline.SandboxRoot,
                            expectedAliases).ToList();
                    }
                    catch (Exception exception)
                    {
                        errors.Add("Owned cleanup selection: " + exception.Message);
                    }
                    cleanup.AddRange(CleanupOwnedClaims(baseline, baselineOwnership, claims));
                }
            }

            var result = new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-batch-create" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "baselineId", baseline == null ? string.Empty : baseline.BaselineId },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "statePath", Path.GetFullPath(options.StatePath) },
                { "targetProcessId", process == null ? 0 : process.Id },
                { "mainWindow", "0x" + mainWindow.ToInt64().ToString("X") },
                { "commandId", commandId },
                { "dialog", dialog },
                { "dialogs", dialogs },
                { "officialCreateResults", officialCreateResults },
                { "mainWindowTextsAfterSubmit", mainWindowTexts },
                { "expectedAliases", expectedAliases },
                { "ownershipClaims", claims },
                { "dynamicExit", dynamicExit },
                { "forcedStop", forcedStop },
                { "cleanup", cleanup },
                { "errors", errors }
            };
            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-batch-create", result);
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static string[] BuildBatchCreateAliases(string prefix)
        {
            return new SandboxAdminService()
                .BuildBatchSandboxNames(prefix + "_Alias-", 1, 2, 2)
                .ToArray();
        }

        private static SandboxieOperationResult CreateOwnedSandboxViaOfficial(
            PreflightReport baseline,
            OwnershipSnapshot baselineOwnership,
            IList<string> reservedInternalNames,
            string alias)
        {
            var client = new SandboxieClient
            {
                SandboxiePath = baseline.SandboxiePath,
                SandboxRoot = baseline.SandboxRoot
            };
            return client.CreateSandboxViaOfficial(
                alias,
                baselineOwnership.Sections,
                reservedInternalNames,
                new SandboxieCallbacks
                {
                    RunSbieIni = arguments => client.RunSbieIni(arguments, 8, null),
                    ReloadConfiguration = () => client.RunSandboxieStart(
                        new[] { "/reload" }, 8, null, true, false),
                    EnsureSandboxDirectory = name => Directory.CreateDirectory(
                        Path.Combine(baseline.SandboxRoot, name))
                });
        }

        private static int RunLifecycleCreateGroup(AuditOptions options)
        {
            RequireFileOption(options.BaselinePath, "--baseline");
            RequireFileOption(options.StatePath, "--state");
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            PreflightReport baseline = serializer.Deserialize<PreflightReport>(File.ReadAllText(options.BaselinePath));
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            LifecycleState state = null;
            bool passed = false;
            bool configurationChanged = false;
            byte[] configurationBaseline = null;
            string expectedConfigurationSha256 = string.Empty;
            string groupName = options.Prefix + "_Group";
            try
            {
                if (baseline == null
                    || !string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase)
                    || baseline.Checks == null
                    || baseline.Checks.Any(check => !check.Value))
                {
                    throw new InvalidOperationException("Create-group requires the original passed preflight baseline.");
                }
                RequireFileOption(baseline.TargetPath, "baseline target");
                RequireFileOption(baseline.ConfigPath, "baseline config");
                RequireFileOption(baseline.SandboxieIniPath, "baseline Sandboxie.ini");
                if (!string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(HashFile(baseline.ConfigPath, errors), baseline.ConfigSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Target or configuration hash changed before create-group.");
                }

                EnsureLifecycleProcessGate(baseline.TargetPath);
                state = AuditSafety.LoadOrCreateLifecycleState(
                    options.StatePath,
                    baseline.BaselineId,
                    options.Prefix,
                    options.Stage);
                string[] expectedAliases = BuildBatchCreateAliases(options.Prefix);
                OwnershipSnapshot current = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                IList<OwnershipClaim> owned = AuditSafety.SelectOwnedResources(
                    OwnershipFromPreflight(baseline),
                    current,
                    state.OwnershipClaims);
                if (!owned.Select(claim => claim.Alias).SequenceEqual(expectedAliases, StringComparer.Ordinal))
                {
                    throw new InvalidOperationException("Create-group ownership members changed.");
                }
                EnsureOnlyOwnedAuditResources(current, owned);

                configurationBaseline = File.ReadAllBytes(baseline.ConfigPath);
                var store = new ConfigStore(
                    baseline.ConfigPath,
                    Environment.UserName,
                    () => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                var settings = store.GetJson("settings_json", new Dictionary<string, object>())
                    as IDictionary<string, object>;
                IDictionary<string, object> updated = AuditSafety.AddOwnedGroupSettings(
                    settings,
                    groupName,
                    expectedAliases);
                if (!store.SetJson("settings_json", updated))
                {
                    throw new InvalidOperationException("Configuration store refused the owned group.");
                }
                configurationChanged = true;
                expectedConfigurationSha256 = HashFile(baseline.ConfigPath, errors);

                var verifiedSettings = store.GetJson("settings_json", null) as IDictionary<string, object>;
                IDictionary<string, IList<string>> verifiedGroups = AuditSafety.ReadSandboxGroups(verifiedSettings);
                IList<string> verifiedMembers;
                if (!verifiedGroups.TryGetValue(groupName, out verifiedMembers)
                    || !verifiedMembers.SequenceEqual(expectedAliases, StringComparer.Ordinal))
                {
                    throw new InvalidOperationException("Owned group did not persist exact members.");
                }

                state.GroupName = groupName;
                state.GroupMembers = expectedAliases.ToList();
                state.ConfigurationBaselineBase64 = Convert.ToBase64String(configurationBaseline);
                state.ExpectedConfigurationSha256 = expectedConfigurationSha256;
                state.CompletedStages.Add("create-group");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
            }
            finally
            {
                if (!passed)
                {
                    if (configurationChanged)
                    {
                        AuditStepResult restored = AuditSafety.RestoreOwnedFile(
                            baseline.ConfigPath,
                            configurationBaseline,
                            baseline.ConfigSha256,
                            expectedConfigurationSha256);
                        cleanup.Add(StepRow("configuration", restored));
                    }
                    if (state != null && state.OwnershipClaims != null && state.OwnershipClaims.Count != 0)
                    {
                        cleanup.AddRange(CleanupOwnedClaims(
                            baseline,
                            OwnershipFromPreflight(baseline),
                            state.OwnershipClaims));
                    }
                }
            }

            var result = new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-create-group" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "baselineId", baseline == null ? string.Empty : baseline.BaselineId },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "statePath", Path.GetFullPath(options.StatePath) },
                { "groupName", groupName },
                { "groupMembers", state == null ? new List<string>() : state.GroupMembers },
                { "configurationSha256", expectedConfigurationSha256 },
                { "cleanup", cleanup },
                { "errors", errors }
            };
            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-create-group", result);
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycleLaunchGroup(AuditOptions options)
        {
            RequireFileOption(options.BaselinePath, "--baseline");
            RequireFileOption(options.StatePath, "--state");
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            PreflightReport baseline = serializer.Deserialize<PreflightReport>(File.ReadAllText(options.BaselinePath));
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            var processCleanup = new List<IDictionary<string, object>>();
            var observedPids = new Dictionary<string, IList<int>>(StringComparer.Ordinal);
            LifecycleState state = null;
            IList<OwnershipClaim> groupClaims = new List<OwnershipClaim>();
            LaunchBatchResult launch = null;
            bool launchAttempted = false;
            bool passed = false;
            try
            {
                if (baseline == null || !string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Launch-group requires the original passed preflight baseline.");
                }
                if (!string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Target hash changed before launch-group.");
                }

                EnsureLifecycleProcessGate(baseline.TargetPath);
                state = AuditSafety.LoadOrCreateLifecycleState(
                    options.StatePath,
                    baseline.BaselineId,
                    options.Prefix,
                    options.Stage);
                if (!string.Equals(
                    HashFile(baseline.ConfigPath, errors),
                    state.ExpectedConfigurationSha256,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Owned configuration changed before launch-group.");
                }

                groupClaims = AuditSafety.SelectOwnedGroupClaims(state);
                OwnershipSnapshot current = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                AuditSafety.SelectOwnedResources(OwnershipFromPreflight(baseline), current, groupClaims);
                EnsureOnlyOwnedAuditResources(current, groupClaims);

                var client = new SandboxieClient
                {
                    SandboxiePath = baseline.SandboxiePath,
                    SandboxRoot = baseline.SandboxRoot
                };
                SandboxieDisplayMaps maps = client.BuildDisplayMaps(current.Sections, current.AliasesBySection);
                var scheduler = new LaunchScheduler();
                IList<string> resolved = scheduler.ResolveLaunchEntries(
                    state.GroupMembers,
                    client.ScanDisplayNames(maps),
                    current.Sections,
                    name => client.NormalizeSandboxName(name, maps),
                    name => client.GetDisplayName(name, maps));
                if (!resolved.SequenceEqual(state.GroupMembers, StringComparer.Ordinal))
                {
                    throw new InvalidOperationException("Owned group did not resolve to its exact members.");
                }
                foreach (OwnershipClaim claim in groupClaims)
                {
                    if (QuerySandboxPids(client, claim.InternalName).Count != 0)
                    {
                        throw new InvalidOperationException("Owned sandbox was already running: " + claim.Alias);
                    }
                }

                var schedulerState = new LaunchSchedulerState
                {
                    LaunchInterval = 0,
                    LaunchConcurrencyLimit = 2
                };
                var appTargets = new[]
                {
                    new LaunchAppTarget
                    {
                        Path = baseline.TargetPath,
                        Directory = Path.GetDirectoryName(baseline.TargetPath) ?? string.Empty
                    }
                };
                var callbacks = new LaunchSchedulerCallbacks
                {
                    BeginLaunchSession = () => 1,
                    IsLaunchSessionCancelled = sessionId => false,
                    StartAppInSandbox = (sandbox, app, directory) => client.StartAppInSandbox(
                        sandbox,
                        app,
                        directory,
                        maps,
                        current.Sections,
                        new SandboxieCallbacks
                        {
                            ReloadConfiguration = () => client.RunSandboxieStart(
                                new[] { "/reload" }, 8, null, true, false),
                            ListOfficialSandboxNames = () => ReadOwnershipSnapshot(
                                baseline.SandboxieIniPath,
                                baseline.SandboxRoot).Sections
                        }).Success
                };
                if (!scheduler.BeginLaunch(schedulerState, resolved, appTargets, callbacks))
                {
                    throw new InvalidOperationException("Production launch scheduler refused the owned group.");
                }
                launchAttempted = true;
                launch = scheduler.RunLaunchBatch(
                    schedulerState,
                    schedulerState.LaunchSessionId,
                    resolved,
                    callbacks);
                if (launch.Success != 2 || launch.Total != 2 || launch.Failed != 0 || launch.Skipped != 0)
                {
                    throw new InvalidOperationException("Production group launch did not dispatch both owned members.");
                }

                foreach (OwnershipClaim claim in groupClaims)
                {
                    observedPids[claim.Alias] = WaitForSandboxPids(client, claim.InternalName, 15000);
                }
                bool processesClean;
                processCleanup.AddRange(TerminateOwnedSandboxProcesses(baseline, client, groupClaims, out processesClean));
                if (!processesClean)
                {
                    throw new InvalidOperationException("Owned launch processes did not close completely.");
                }

                state.CompletedStages.Add("launch-group");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                if (launchAttempted && state != null && groupClaims.Count != 0)
                {
                    try
                    {
                        var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
                        bool ignored;
                        processCleanup.AddRange(TerminateOwnedSandboxProcesses(
                            baseline, client,
                            groupClaims,
                            out ignored));
                        AuditStepResult restored = AuditSafety.RestoreOwnedFile(
                            baseline.ConfigPath,
                            Convert.FromBase64String(state.ConfigurationBaselineBase64),
                            baseline.ConfigSha256,
                            state.ExpectedConfigurationSha256);
                        cleanup.Add(StepRow("configuration", restored));
                        cleanup.AddRange(CleanupOwnedClaims(
                            baseline,
                            OwnershipFromPreflight(baseline),
                            state.OwnershipClaims));
                    }
                    catch (Exception cleanupException)
                    {
                        errors.Add("Controlled cleanup: " + cleanupException.Message);
                    }
                }
            }

            var result = new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-launch-group" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "baselineId", baseline == null ? string.Empty : baseline.BaselineId },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "groupName", state == null ? string.Empty : state.GroupName },
                { "groupMembers", state == null ? new List<string>() : state.GroupMembers },
                { "application", baseline == null ? string.Empty : baseline.TargetPath },
                { "launch", launch },
                { "observedPids", observedPids },
                { "processCleanup", processCleanup },
                { "cleanup", cleanup },
                { "errors", errors }
            };
            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-launch-group", result);
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static bool StopAuditTimer(Timer timer, int timeoutMilliseconds)
        {
            if (timer == null) return true;
            var stopped = new ManualResetEvent(false);
            if (!timer.Dispose(stopped))
            {
                stopped.Dispose();
                return false;
            }
            if (stopped.WaitOne(timeoutMilliseconds))
            {
                stopped.Dispose();
                return true;
            }
            ThreadPool.RegisterWaitForSingleObject(stopped,
                (state, timedOut) => ((ManualResetEvent)state).Dispose(), stopped, Timeout.Infinite, true);
            return false;
        }

        private static int RunLifecycleScheduleLaunch(AuditOptions options)
        {
            PreflightReport baseline = LoadLifecycleBaseline(options);
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            var processCleanup = new List<IDictionary<string, object>>();
            var observedPids = new Dictionary<string, IList<int>>(StringComparer.Ordinal);
            LifecycleState state = null;
            LaunchBatchResult launch = null;
            string jobKey = string.Empty;
            bool launchAttempted = false;
            bool callbacksStopped = true;
            Timer auditTimer = null;
            bool passed = false;
            try
            {
                OwnershipSnapshot current;
                IList<OwnershipClaim> claims;
                LoadOwnedLifecycleStage(options, baseline, errors, out state, out current, out claims);
                var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
                var scheduler = new LaunchScheduler();
                var scheduleState = new LaunchSchedulerState();
                var completed = new TaskCompletionSource<bool>();
                string callbackError = string.Empty;
                try
                {
                    var appTargets = BuildAuditTargetApplications(baseline);
                    jobKey = scheduler.ScheduleSelectedLaunch(
                        scheduleState,
                        state.GroupMembers,
                        appTargets,
                        1,
                        new LaunchSchedulerCallbacks
                        {
                            ScheduleDelayed = (delay, action) =>
                            {
                                auditTimer = new Timer(_ => action(), null, delay, Timeout.Infinite);
                                return auditTimer;
                            },
                            LaunchSelectedSandboxes = (sandboxes, applications) =>
                            {
                                try
                                {
                                    launch = StartOwnedGroup(
                                        baseline,
                                        state,
                                        current,
                                        client,
                                        sandboxes,
                                        appTargets, () => launchAttempted = true);
                                }
                                catch (Exception exception)
                                {
                                    callbackError = exception.Message;
                                }
                                finally
                                {
                                    completed.TrySetResult(true);
                                }
                            }
                        });
                    if (jobKey.Length == 0 || !completed.Task.Wait(TimeSpan.FromSeconds(12)))
                    {
                        throw new InvalidOperationException("Scheduled launch did not execute within the bounded wait.");
                    }
                    if (callbackError.Length != 0)
                    {
                        throw new InvalidOperationException(callbackError);
                    }
                    if (launch == null || launch.Success != 2 || launch.Total != 2 || launch.Failed != 0 || launch.Skipped != 0)
                    {
                        throw new InvalidOperationException("Scheduled launch did not start both owned members.");
                    }
                    foreach (OwnershipClaim claim in claims)
                    {
                        observedPids[claim.Alias] = WaitForSandboxPids(client, claim.InternalName, 15000);
                    }
                    bool processesClean;
                    processCleanup.AddRange(TerminateOwnedSandboxProcesses(baseline, client, claims, out processesClean));
                    if (!processesClean)
                    {
                        throw new InvalidOperationException("Scheduled launch processes did not close completely.");
                    }
                }
                finally
                {
                    callbacksStopped = StopAuditTimer(auditTimer, 15000);
                    if (!callbacksStopped)
                        throw new InvalidOperationException("Scheduled callback is still active; destructive cleanup was deferred.");
                }

                state.CompletedStages.Add("schedule-launch");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                if (callbacksStopped) CleanupFailedOwnedStage(baseline, state, launchAttempted, cleanup, errors);
            }

            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-schedule-launch", new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-schedule-launch" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "jobKey", jobKey },
                { "launch", launch },
                { "observedPids", observedPids },
                { "processCleanup", processCleanup },
                { "cleanup", cleanup },
                { "errors", errors }
            });
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycleScheduleClose(AuditOptions options)
        {
            PreflightReport baseline = LoadLifecycleBaseline(options);
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            var processCleanup = new List<IDictionary<string, object>>();
            var observedPids = new Dictionary<string, IList<int>>(StringComparer.Ordinal);
            LifecycleState state = null;
            LaunchBatchResult launch = null;
            string jobKey = string.Empty;
            bool launchAttempted = false;
            bool callbacksStopped = true;
            Timer auditTimer = null;
            bool passed = false;
            try
            {
                OwnershipSnapshot current;
                IList<OwnershipClaim> claims;
                LoadOwnedLifecycleStage(options, baseline, errors, out state, out current, out claims);
                var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
                launch = StartOwnedGroup(
                    baseline,
                    state,
                    current,
                    client,
                    state.GroupMembers,
                    BuildAuditTargetApplications(baseline), () => launchAttempted = true);
                if (launch.Success != 2 || launch.Total != 2 || launch.Failed != 0 || launch.Skipped != 0)
                {
                    throw new InvalidOperationException("Schedule-close setup did not start both owned members.");
                }
                foreach (OwnershipClaim claim in claims)
                {
                    observedPids[claim.Alias] = WaitForSandboxPids(client, claim.InternalName, 15000);
                }

                var scheduler = new LaunchScheduler();
                var scheduleState = new LaunchSchedulerState();
                var completed = new TaskCompletionSource<bool>();
                int closed = 0;
                try
                {
                    jobKey = scheduler.ScheduleSelectedClose(
                        scheduleState,
                        state.GroupMembers,
                        1,
                        new LaunchSchedulerCallbacks
                        {
                            ScheduleDelayed = (delay, action) =>
                            {
                                auditTimer = new Timer(_ => action(), null, delay, Timeout.Infinite);
                                return auditTimer;
                            },
                            CloseSandboxProcesses = alias =>
                            {
                                OwnershipClaim claim = claims.Single(item => string.Equals(item.Alias, alias, StringComparison.Ordinal));
                                bool clean;
                                IDictionary<string, object> row = TerminateOwnedSandboxProcess(baseline, client, claim, out clean);
                                lock (processCleanup)
                                {
                                    processCleanup.Add(row);
                                    closed++;
                                    if (closed == claims.Count)
                                    {
                                        completed.TrySetResult(true);
                                    }
                                }
                                return clean ? "all_closed" : "partially_closed";
                            }
                        });
                    if (jobKey.Length == 0 || !completed.Task.Wait(TimeSpan.FromSeconds(15)))
                    {
                        throw new InvalidOperationException("Scheduled close did not execute within the bounded wait.");
                    }
                    if (processCleanup.Count != claims.Count
                        || processCleanup.Any(row => !string.Equals(Convert.ToString(row["status"]), "passed", StringComparison.Ordinal)))
                    {
                        throw new InvalidOperationException("Scheduled close did not close both owned sandboxes.");
                    }
                }
                finally
                {
                    callbacksStopped = StopAuditTimer(auditTimer, 15000);
                    if (!callbacksStopped)
                        throw new InvalidOperationException("Scheduled callback is still active; destructive cleanup was deferred.");
                }

                state.CompletedStages.Add("schedule-close");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                if (callbacksStopped) CleanupFailedOwnedStage(baseline, state, launchAttempted, cleanup, errors);
            }

            IList<IDictionary<string, object>> processCleanupSnapshot;
            lock (processCleanup) { processCleanupSnapshot = processCleanup.ToList(); }
            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-schedule-close", new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-schedule-close" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "jobKey", jobKey },
                { "setupLaunch", launch },
                { "observedPids", observedPids },
                { "processCleanup", processCleanupSnapshot },
                { "cleanup", cleanup },
                { "errors", errors }
            });
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycleCancelSchedules(AuditOptions options)
        {
            PreflightReport baseline = LoadLifecycleBaseline(options);
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            LifecycleState state = null;
            string launchJob = string.Empty;
            string closeJob = string.Empty;
            int cancelled = 0;
            bool passed = false;
            try
            {
                OwnershipSnapshot current;
                IList<OwnershipClaim> claims;
                LoadOwnedLifecycleStage(options, baseline, errors, out state, out current, out claims);
                var scheduler = new LaunchScheduler();
                var scheduleState = new LaunchSchedulerState();
                int executed = 0;
                var callbacks = new LaunchSchedulerCallbacks
                {
                    LaunchSelectedSandboxes = (sandboxes, applications) => Interlocked.Increment(ref executed),
                    CloseSandboxProcesses = alias =>
                    {
                        Interlocked.Increment(ref executed);
                        return "all_closed";
                    }
                };
                launchJob = scheduler.ScheduleSelectedLaunch(
                    scheduleState,
                    state.GroupMembers,
                    BuildAuditTargetApplications(baseline),
                    30,
                    callbacks);
                closeJob = scheduler.ScheduleSelectedClose(
                    scheduleState,
                    state.GroupMembers,
                    30,
                    callbacks);
                if (launchJob.Length == 0 || closeJob.Length == 0 || scheduleState.Jobs.Count != 2)
                {
                    throw new InvalidOperationException("Pending schedule setup was incomplete.");
                }
                cancelled = scheduler.CancelAllSchedules(scheduleState, true, callbacks);
                Thread.Sleep(100);
                if (cancelled != 2 || scheduleState.Jobs.Count != 0 || executed != 0)
                {
                    throw new InvalidOperationException("Pending schedules were not cancelled exactly.");
                }

                state.CompletedStages.Add("cancel-schedules");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
            }

            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-cancel-schedules", new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-cancel-schedules" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "launchJob", launchJob },
                { "closeJob", closeJob },
                { "cancelled", cancelled },
                { "cleanup", cleanup },
                { "errors", errors }
            });
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycleBatchRename(AuditOptions options)
        {
            PreflightReport baseline = LoadLifecycleBaseline(options);
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            LifecycleState state = null;
            IList<OwnershipClaim> claims = new List<OwnershipClaim>();
            string[] originalAliases = null;
            byte[] configurationBefore = null;
            string configurationBeforeHash = string.Empty;
            string configurationAfterHash = string.Empty;
            bool configurationChanged = false;
            var observedAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool passed = false;
            try
            {
                OwnershipSnapshot current;
                LoadOwnedLifecycleStage(options, baseline, errors, out state, out current, out claims);
                originalAliases = claims.Select(claim => claim.Alias).ToArray();
                configurationBeforeHash = state.ExpectedConfigurationSha256;
                var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
                foreach (OwnershipClaim claim in claims)
                {
                    if (QuerySandboxPids(client, claim.InternalName).Count != 0)
                    {
                        throw new InvalidOperationException("Owned sandbox is still running: " + claim.Alias);
                    }
                }

                string[] renamedAliases =
                {
                    options.Prefix + "_Renamed 01",
                    options.Prefix + "_Renamed 02"
                };
                AuditSafety.RenameOwnedAliases(
                    claims,
                    renamedAliases,
                    (name, alias) =>
                    {
                        int index = claims.ToList().FindIndex(claim => claim.InternalName == name);
                        OwnershipSnapshot latest = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                        string observed;
                        if (index < 0 || !latest.AliasesBySection.TryGetValue(name, out observed)
                            || (observed != originalAliases[index] && observed != renamedAliases[index]))
                        {
                            throw new InvalidOperationException("Alias ownership changed; write refused: " + name);
                        }
                        AuditSafety.SelectOwnedResources(OwnershipFromPreflight(baseline), latest,
                            new[] { new OwnershipClaim { InternalName = name, Alias = observed,
                                DirectoryName = claims[index].DirectoryName } });
                        Tuple<int, string> command = client.RunSbieIni(
                            new[] { "set", "/drv", name, "BoxAlias", alias }, 8, null);
                        if (command == null || command.Item1 != 0) return command == null ? -1 : command.Item1;
                        OwnershipSnapshot after = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                        string actual;
                        if (!after.AliasesBySection.TryGetValue(name, out actual) || actual != alias)
                        {
                            throw new InvalidOperationException("Alias write was not observed: " + name);
                        }
                        return 0;
                    },
                    () =>
                    {
                        Tuple<int, string> reload = client.RunSandboxieStart(new[] { "/reload" }, 8, null, true, false);
                        if (reload == null || reload.Item1 != 0) throw new InvalidOperationException("Sandboxie reload failed.");
                    },
                    () =>
                    {
                        configurationBefore = File.ReadAllBytes(baseline.ConfigPath);
                        var store = new ConfigStore(
                            baseline.ConfigPath,
                            Environment.UserName,
                            () => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        var settings = store.GetJson("settings_json", new Dictionary<string, object>())
                            as IDictionary<string, object> ?? new Dictionary<string, object>();
                        IDictionary<string, IList<string>> groups = AuditSafety.ReadSandboxGroups(settings);
                        IList<string> members;
                        if (!groups.TryGetValue(state.GroupName, out members)
                            || !members.SequenceEqual(state.GroupMembers, StringComparer.Ordinal))
                        {
                            throw new InvalidOperationException("Owned group changed before batch rename.");
                        }
                        groups[state.GroupName] = renamedAliases.ToList();
                        var updated = new Dictionary<string, object>(settings, StringComparer.Ordinal)
                        {
                            ["sandbox_groups"] = groups.ToDictionary(
                                item => item.Key,
                                item => (object)item.Value.ToList(),
                                StringComparer.Ordinal)
                        };
                        if (!store.SetJson("settings_json", updated))
                        {
                            throw new InvalidOperationException("Configuration store refused renamed owned group.");
                        }
                        configurationChanged = true;
                        configurationAfterHash = HashFile(baseline.ConfigPath, errors);

                        OwnershipSnapshot verified = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                        for (int index = 0; index < claims.Count; index++)
                        {
                            string alias;
                            if (!verified.AliasesBySection.TryGetValue(claims[index].InternalName, out alias)
                                || !string.Equals(alias, renamedAliases[index], StringComparison.Ordinal))
                            {
                                throw new InvalidOperationException("Renamed alias was not observed: " + claims[index].InternalName);
                            }
                            claims[index].Alias = renamedAliases[index];
                        }
                        state.GroupMembers = renamedAliases.ToList();
                        state.ExpectedConfigurationSha256 = configurationAfterHash;
                        state.CompletedStages.Add("batch-rename");
                        WriteLifecycleState(options.StatePath, state);
                    });
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                if (configurationChanged && configurationBefore != null && state != null)
                {
                    cleanup.Add(StepRow("configuration", AuditSafety.RestoreOwnedFile(
                        baseline.ConfigPath,
                        configurationBefore,
                        configurationBeforeHash,
                        configurationAfterHash)));
                }
                try
                {
                    OwnershipSnapshot actual = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                    foreach (OwnershipClaim claim in claims)
                    {
                        string alias;
                        observedAliases[claim.InternalName] = actual.AliasesBySection.TryGetValue(claim.InternalName, out alias)
                            ? alias : "<missing>";
                    }
                }
                catch (Exception readError) { errors.Add("Recovery observation: " + readError.Message); }
                if (state != null && originalAliases != null)
                {
                    state.GroupMembers = originalAliases.ToList();
                    state.ExpectedConfigurationSha256 = configurationBeforeHash;
                    state.CompletedStages.Remove("batch-rename");
                }
            }

            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-batch-rename", new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-batch-rename" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "claims", state == null ? new List<OwnershipClaim>() : state.OwnershipClaims },
                { "groupMembers", state == null ? new List<string>() : state.GroupMembers },
                { "observedAliasesAfterFailure", observedAliases },
                { "cleanup", cleanup },
                { "errors", errors }
            });
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycleBatchDelete(AuditOptions options)
        {
            PreflightReport baseline = LoadLifecycleBaseline(options);
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            LifecycleState state = null;
            byte[] configurationBefore = null;
            string configurationBeforeHash = string.Empty;
            string configurationAfterHash = string.Empty;
            bool configurationChanged = false;
            bool passed = false;
            try
            {
                OwnershipSnapshot current;
                IList<OwnershipClaim> claims;
                LoadOwnedLifecycleStage(options, baseline, errors, out state, out current, out claims);
                var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
                foreach (OwnershipClaim claim in claims)
                {
                    if (QuerySandboxPids(client, claim.InternalName).Count != 0)
                    {
                        throw new InvalidOperationException("Owned sandbox is still running: " + claim.Alias);
                    }
                }

                var store = new ConfigStore(
                    baseline.ConfigPath,
                    Environment.UserName,
                    () => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                var settings = store.GetJson("settings_json", new Dictionary<string, object>())
                    as IDictionary<string, object> ?? new Dictionary<string, object>();
                IDictionary<string, IList<string>> groups = AuditSafety.ReadSandboxGroups(settings);
                IList<string> members;
                if (!groups.TryGetValue(state.GroupName, out members)
                    || !members.SequenceEqual(state.GroupMembers, StringComparer.Ordinal))
                {
                    throw new InvalidOperationException("Owned group changed before batch delete.");
                }
                configurationBefore = File.ReadAllBytes(baseline.ConfigPath);
                configurationBeforeHash = state.ExpectedConfigurationSha256;

                cleanup.AddRange(CleanupOwnedClaims(
                    baseline,
                    OwnershipFromPreflight(baseline),
                    claims));
                if (cleanup.Count != claims.Count
                    || cleanup.Any(row => !string.Equals(Convert.ToString(row["status"]), "passed", StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException("Batch delete did not remove every owned sandbox.");
                }

                groups.Remove(state.GroupName);
                var updated = new Dictionary<string, object>(settings, StringComparer.Ordinal)
                {
                    ["sandbox_groups"] = groups.ToDictionary(
                        item => item.Key,
                        item => (object)item.Value.ToList(),
                        StringComparer.Ordinal)
                };
                if (!store.SetJson("settings_json", updated))
                {
                    throw new InvalidOperationException("Configuration store refused owned group deletion.");
                }
                configurationChanged = true;
                configurationAfterHash = HashFile(baseline.ConfigPath, errors);
                state.ExpectedConfigurationSha256 = configurationAfterHash;
                state.CompletedStages.Add("batch-delete");
                WriteLifecycleState(options.StatePath, state);
                passed = true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                if (configurationChanged)
                {
                    cleanup.Add(StepRow("configuration", AuditSafety.RestoreOwnedFile(
                        baseline.ConfigPath, configurationBefore, configurationBeforeHash, configurationAfterHash)));
                }
            }

            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-batch-delete", new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-batch-delete" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "cleanup", cleanup },
                { "errors", errors }
            });
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static int RunLifecycleCleanup(AuditOptions options)
        {
            RequireFileOption(options.BaselinePath, "--baseline");
            RequireFileOption(options.StatePath, "--state");
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            PreflightReport baseline = serializer.Deserialize<PreflightReport>(File.ReadAllText(options.BaselinePath));
            var errors = new List<string>();
            var cleanup = new List<IDictionary<string, object>>();
            LifecycleState state = null;
            bool passed = false;
            try
            {
                if (baseline == null
                    || !string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Cleanup requires the original passed preflight baseline.");
                }
                RequireFileOption(baseline.TargetPath, "baseline target");
                RequireFileOption(baseline.ConfigPath, "baseline config");
                RequireFileOption(baseline.SandboxieIniPath, "baseline Sandboxie.ini");
                if (!string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Target hash changed before cleanup.");
                }

                state = AuditSafety.LoadLifecycleStateForCleanup(
                    options.StatePath,
                    baseline.BaselineId,
                    options.Prefix);
                EnsureCleanupProcessGate(baseline, state);
                bool configurationRestored;
                if (string.IsNullOrWhiteSpace(state.ConfigurationBaselineBase64))
                {
                    configurationRestored = string.Equals(
                        HashFile(baseline.ConfigPath, errors),
                        baseline.ConfigSha256,
                        StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    AuditStepResult restored = AuditSafety.RestoreOwnedFile(
                        baseline.ConfigPath,
                        Convert.FromBase64String(state.ConfigurationBaselineBase64),
                        baseline.ConfigSha256,
                        state.ExpectedConfigurationSha256);
                    cleanup.Add(StepRow("configuration", restored));
                    configurationRestored = restored.Status == AuditStatus.Passed;
                }
                cleanup.AddRange(CleanupOwnedClaims(
                    baseline,
                    OwnershipFromPreflight(baseline),
                    state.OwnershipClaims));
                passed = configurationRestored
                    && cleanup.Count == state.OwnershipClaims.Count
                        + (string.IsNullOrWhiteSpace(state.ConfigurationBaselineBase64) ? 0 : 1)
                    && cleanup.All(row => string.Equals(
                        Convert.ToString(row["status"]),
                        "passed",
                        StringComparison.Ordinal));
                if (!passed)
                {
                    errors.Add("One or more exact owned resources were not cleaned.");
                }
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
            }

            var result = new Dictionary<string, object>
            {
                { "kind", "auditrunner-lifecycle-cleanup" },
                { "timestamp", Timestamp() },
                { "status", passed ? "passed" : "inconclusive" },
                { "baselineId", baseline == null ? string.Empty : baseline.BaselineId },
                { "prefix", options.Prefix },
                { "stage", options.Stage },
                { "statePath", Path.GetFullPath(options.StatePath) },
                { "claims", state == null ? new List<OwnershipClaim>() : state.OwnershipClaims },
                { "cleanup", cleanup },
                { "errors", errors }
            };
            string output = WriteJson(options.OutputDirectory, "auditrunner-lifecycle-cleanup", result);
            Console.WriteLine(output);
            return passed ? 0 : 3;
        }

        private static IDictionary<string, object> StepRow(string resource, AuditStepResult result)
        {
            return new Dictionary<string, object>
            {
                { "resource", resource },
                { "status", result.Status.ToString().ToLowerInvariant() },
                { "detail", result.Detail }
            };
        }

        private static PreflightReport LoadLifecycleBaseline(AuditOptions options)
        {
            RequireFileOption(options.BaselinePath, "--baseline");
            RequireFileOption(options.StatePath, "--state");
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue }
                .Deserialize<PreflightReport>(File.ReadAllText(options.BaselinePath));
        }

        private static void LoadOwnedLifecycleStage(
            AuditOptions options,
            PreflightReport baseline,
            IList<string> errors,
            out LifecycleState state,
            out OwnershipSnapshot current,
            out IList<OwnershipClaim> claims)
        {
            if (baseline == null || !string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Lifecycle stage requires the original passed preflight baseline.");
            }
            if (!string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Target hash changed before lifecycle stage.");
            }
            EnsureLifecycleProcessGate(baseline.TargetPath);
            state = AuditSafety.LoadOrCreateLifecycleState(
                options.StatePath,
                baseline.BaselineId,
                options.Prefix,
                options.Stage);
            if (!string.Equals(
                HashFile(baseline.ConfigPath, errors),
                state.ExpectedConfigurationSha256,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Owned configuration changed before lifecycle stage.");
            }
            claims = AuditSafety.SelectOwnedGroupClaims(state);
            current = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
            AuditSafety.SelectOwnedResources(OwnershipFromPreflight(baseline), current, claims);
            EnsureOnlyOwnedAuditResources(current, claims);
        }

        private static IList<LaunchAppTarget> BuildAuditTargetApplications(PreflightReport baseline)
        {
            return new List<LaunchAppTarget>
            {
                new LaunchAppTarget
                {
                    Path = baseline.TargetPath,
                    Directory = Path.GetDirectoryName(baseline.TargetPath) ?? string.Empty
                }
            };
        }

        private static LaunchBatchResult StartOwnedGroup(
            PreflightReport baseline,
            LifecycleState state,
            OwnershipSnapshot current,
            SandboxieClient client,
            IEnumerable<string> sandboxes,
            IEnumerable<LaunchAppTarget> applications,
            Action launchStarting)
        {
            SandboxieDisplayMaps maps = client.BuildDisplayMaps(current.Sections, current.AliasesBySection);
            var scheduler = new LaunchScheduler();
            IList<string> resolved = scheduler.ResolveLaunchEntries(
                sandboxes,
                client.ScanDisplayNames(maps),
                current.Sections,
                name => client.NormalizeSandboxName(name, maps),
                name => client.GetDisplayName(name, maps));
            if (!resolved.SequenceEqual(state.GroupMembers, StringComparer.Ordinal))
            {
                throw new InvalidOperationException("Owned group did not resolve to its exact members.");
            }

            var schedulerState = new LaunchSchedulerState
            {
                LaunchInterval = 0,
                LaunchConcurrencyLimit = 2
            };
            var callbacks = new LaunchSchedulerCallbacks
            {
                BeginLaunchSession = () => 1,
                IsLaunchSessionCancelled = sessionId => false,
                StartAppInSandbox = (sandbox, app, directory) => client.StartAppInSandbox(
                    sandbox,
                    app,
                    directory,
                    maps,
                    current.Sections,
                    new SandboxieCallbacks
                    {
                        ReloadConfiguration = () => client.RunSandboxieStart(
                            new[] { "/reload" }, 8, null, true, false),
                        ListOfficialSandboxNames = () => ReadOwnershipSnapshot(
                            baseline.SandboxieIniPath,
                            baseline.SandboxRoot).Sections
                    }).Success
            };
            if (!scheduler.BeginLaunch(schedulerState, resolved, applications, callbacks))
            {
                throw new InvalidOperationException("Production launch scheduler refused the owned group.");
            }
            AuditSafety.ConfirmOwnedLaunchStart(AuditSafety.SelectOwnedGroupClaims(state),
                name => QuerySandboxPids(client, name), launchStarting);
            return scheduler.RunLaunchBatch(
                schedulerState,
                schedulerState.LaunchSessionId,
                resolved,
                callbacks);
        }

        private static void CleanupFailedOwnedStage(
            PreflightReport baseline,
            LifecycleState state,
            bool launchAttempted,
            List<IDictionary<string, object>> cleanup,
            IList<string> errors)
        {
            if (!launchAttempted || baseline == null || state == null || state.OwnershipClaims == null || state.OwnershipClaims.Count == 0)
            {
                return;
            }
            try
            {
                AuditSafety.SelectOwnedResources(OwnershipFromPreflight(baseline),
                    ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot), state.OwnershipClaims);
                var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
                bool processesClean;
                cleanup.AddRange(TerminateOwnedSandboxProcesses(baseline, client, state.OwnershipClaims, out processesClean));
                if (!processesClean) return;
                EnsureCleanupProcessGate(baseline, state);
                AuditStepResult restored = AuditSafety.RestoreOwnedFile(
                    baseline.ConfigPath,
                    Convert.FromBase64String(state.ConfigurationBaselineBase64),
                    baseline.ConfigSha256,
                    state.ExpectedConfigurationSha256);
                cleanup.Add(StepRow("configuration", restored));
                cleanup.AddRange(CleanupOwnedClaims(
                    baseline,
                    OwnershipFromPreflight(baseline),
                    state.OwnershipClaims));
            }
            catch (Exception exception)
            {
                errors.Add("Controlled cleanup: " + exception.Message);
            }
        }

        private static IList<int> QuerySandboxPids(SandboxieClient client, string internalName)
        {
            Tuple<int, string> result = client.RunSandboxieStart(
                new[] { "/box:" + internalName, "/listpids" },
                8,
                null,
                true,
                false);
            if (result == null || result.Item1 != 0)
            {
                throw new InvalidOperationException("Owned process query failed: " + internalName);
            }
            return client.ParseListPidsOutput(result.Item2);
        }

        private static IList<int> WaitForSandboxPids(
            SandboxieClient client,
            string internalName,
            int timeoutMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                IList<int> pids = QuerySandboxPids(client, internalName);
                if (pids.Count != 0)
                {
                    return pids;
                }
                Thread.Sleep(100);
            }
            throw new InvalidOperationException("No process was observed in owned sandbox: " + internalName);
        }

        private static IEnumerable<IDictionary<string, object>> TerminateOwnedSandboxProcesses(
            PreflightReport baseline,
            SandboxieClient client,
            IEnumerable<OwnershipClaim> claims,
            out bool passed)
        {
            var rows = new List<IDictionary<string, object>>();
            passed = true;
            foreach (OwnershipClaim claim in claims ?? Enumerable.Empty<OwnershipClaim>())
            {
                bool clean;
                IDictionary<string, object> row = TerminateOwnedSandboxProcess(baseline, client, claim, out clean);
                passed &= clean;
                rows.Add(row);
            }
            return rows;
        }

        private static IDictionary<string, object> TerminateOwnedSandboxProcess(
            PreflightReport baseline,
            SandboxieClient client,
            OwnershipClaim claim,
            out bool passed)
        {
            AuditSafety.SelectOwnedResources(
                OwnershipFromPreflight(baseline),
                ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot),
                new[] { claim });
            Tuple<int, string> result = client.RunSandboxieStart(
                new[] { "/box:" + claim.InternalName, "/terminate_all" },
                12,
                null,
                true,
                false);
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < 10000 && QuerySandboxPids(client, claim.InternalName).Count != 0)
            {
                Thread.Sleep(100);
            }
            passed = result != null
                && result.Item1 == 0
                && QuerySandboxPids(client, claim.InternalName).Count == 0;
            return new Dictionary<string, object>
            {
                { "alias", claim.Alias },
                { "internalName", claim.InternalName },
                { "returnCode", result == null ? -1 : result.Item1 },
                { "status", passed ? "passed" : "inconclusive" }
            };
        }

        private static void ValidateLifecycleBaseline(
            PreflightReport baseline,
            IEnumerable<string> expectedAliases,
            IList<string> errors)
        {
            if (baseline == null
                || !string.Equals(baseline.Status, "passed", StringComparison.OrdinalIgnoreCase)
                || baseline.Checks == null
                || baseline.Checks.Any(check => !check.Value))
            {
                throw new InvalidOperationException("Lifecycle requires a passed preflight baseline.");
            }
            RequireFileOption(baseline.TargetPath, "baseline target");
            RequireFileOption(baseline.ConfigPath, "baseline config");
            RequireFileOption(baseline.SandboxieIniPath, "baseline Sandboxie.ini");
            if (!string.Equals(HashFile(baseline.TargetPath, errors), baseline.TargetSha256, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(HashFile(baseline.ConfigPath, errors), baseline.ConfigSha256, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(HashFile(baseline.SandboxieIniPath, errors), baseline.SandboxieIniSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("One or more baseline hashes changed before lifecycle.");
            }
            if ((expectedAliases ?? Enumerable.Empty<string>()).Any(LauncherConstants.IsValidInternalSandboxName))
            {
                throw new InvalidOperationException("Lifecycle prefix must force an exact BoxAlias mapping.");
            }
        }

        private static void EnsureLifecycleProcessGate(string targetPath)
        {
            var report = new PreflightReport { TargetPath = targetPath };
            LoadProcesses(report);
            foreach (string key in new[]
            {
                "target", "Start.exe", "powershell.exe", "pwsh.exe", "AuditRunner.exe",
                "SbieCtrl.exe", "SandboxieOther"
            })
            {
                int count;
                if (!report.ProcessCounts.TryGetValue(key, out count) || count != 0)
                {
                    throw new InvalidOperationException("Lifecycle process gate is not clean: " + key);
                }
            }
            var serviceErrors = new List<string>();
            if (!string.Equals(GetServiceStatus("SbieSvc", serviceErrors), "Running", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("SbieSvc is not running.");
            }
        }

        private static void EnsureCleanupProcessGate(PreflightReport baseline, LifecycleState state)
        {
            var report = new PreflightReport { TargetPath = baseline.TargetPath };
            LoadProcesses(report);
            foreach (string key in new[]
            {
                "target", "Start.exe", "powershell.exe", "pwsh.exe", "AuditRunner.exe"
            })
            {
                int count;
                if (!report.ProcessCounts.TryGetValue(key, out count) || count != 0)
                {
                    throw new InvalidOperationException("Cleanup process gate is not clean: " + key);
                }
            }

            var serviceErrors = new List<string>();
            if (!string.Equals(GetServiceStatus("SbieSvc", serviceErrors), "Running", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("SbieSvc is not running.");
            }

            OwnershipSnapshot current = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
            AuditSafety.SelectOwnedResourcesForCleanup(OwnershipFromPreflight(baseline), current, state.OwnershipClaims);
            var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
            foreach (OwnershipClaim claim in state.OwnershipClaims ?? new List<OwnershipClaim>())
            {
                if (current.Sections.Contains(claim.InternalName, StringComparer.OrdinalIgnoreCase)
                    && QuerySandboxPids(client, claim.InternalName).Count != 0)
                {
                    throw new InvalidOperationException("Owned sandbox still has running processes: " + claim.Alias);
                }
            }
        }

        private static OwnershipSnapshot OwnershipFromPreflight(PreflightReport report)
        {
            return new OwnershipSnapshot
            {
                IniReadable = report.IniReadable,
                Sections = (report.IniSections ?? new List<string>()).ToList(),
                AliasesBySection = new Dictionary<string, string>(
                    report.AliasesBySection ?? new Dictionary<string, string>(),
                    StringComparer.OrdinalIgnoreCase),
                Directories = (report.SandboxDirectories ?? new List<string>()).ToList()
            };
        }

        private static OwnershipSnapshot ReadOwnershipSnapshot(string iniPath, string sandboxRoot)
        {
            var snapshot = new OwnershipSnapshot();
            if (!File.Exists(iniPath))
            {
                return snapshot;
            }
            var admin = new SandboxAdminService();
            Tuple<string, string> loaded = admin.ReadTextWithEncoding(iniPath);
            SandboxIniSplitResult split = admin.SplitIniSections(loaded.Item1);
            if (split.Sections.Count == 0)
            {
                return snapshot;
            }
            snapshot.IniReadable = true;
            snapshot.Sections = split.Sections.Select(section => section.Name).ToList();
            var client = new SandboxieClient();
            snapshot.AliasesBySection = client.BuildAliasMap(split.Sections.Select(section => new SandboxieIniSection
            {
                Name = section.Name,
                Block = section.Block
            }).ToList());
            snapshot.Directories = Directory.Exists(sandboxRoot)
                ? Directory.GetDirectories(sandboxRoot).Select(Path.GetFileName).ToList()
                : new List<string>();
            return snapshot;
        }

        private static void EnsureNoAuditResources(OwnershipSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IniReadable)
            {
                throw new InvalidOperationException("Sandboxie.ini is missing or unreadable.");
            }
            bool found = snapshot.Sections.Any(name => name.StartsWith("ShaheAudit_", StringComparison.OrdinalIgnoreCase))
                || snapshot.AliasesBySection.Values.Any(alias => alias.StartsWith("ShaheAudit_", StringComparison.Ordinal))
                || snapshot.Directories.Any(name => name.StartsWith("ShaheAudit_", StringComparison.OrdinalIgnoreCase));
            if (found)
            {
                throw new InvalidOperationException("Existing ShaheAudit_* resources block lifecycle.");
            }
        }

        private static void EnsureOnlyOwnedAuditResources(
            OwnershipSnapshot snapshot,
            IEnumerable<OwnershipClaim> claims)
        {
            IList<OwnershipClaim> owned = (claims ?? Enumerable.Empty<OwnershipClaim>()).ToList();
            var sections = new HashSet<string>(owned.Select(claim => claim.InternalName), StringComparer.OrdinalIgnoreCase);
            var aliases = new HashSet<string>(owned.Select(claim => claim.Alias), StringComparer.Ordinal);
            var directories = new HashSet<string>(owned.Select(claim => claim.DirectoryName), StringComparer.OrdinalIgnoreCase);
            if (snapshot.Sections.Any(name => name.StartsWith("ShaheAudit_", StringComparison.OrdinalIgnoreCase) && !sections.Contains(name))
                || snapshot.AliasesBySection.Values.Any(alias => alias.StartsWith("ShaheAudit_", StringComparison.Ordinal) && !aliases.Contains(alias))
                || snapshot.Directories.Any(name => name.StartsWith("ShaheAudit_", StringComparison.OrdinalIgnoreCase) && !directories.Contains(name)))
            {
                throw new InvalidOperationException("Unknown ShaheAudit_* resources block lifecycle.");
            }
        }

        private static IList<OwnershipClaim> WaitForOwnedClaims(
            OwnershipSnapshot baseline,
            string iniPath,
            string sandboxRoot,
            IEnumerable<string> aliases,
            int timeoutMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            Exception last = null;
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                try
                {
                    IList<OwnershipClaim> claims = FindAvailableOwnedClaims(
                        baseline, iniPath, sandboxRoot, aliases);
                    if (claims.Count == aliases.Count())
                    {
                        return claims;
                    }
                }
                catch (Exception exception)
                {
                    last = exception;
                }
                Thread.Sleep(100);
            }
            throw new InvalidOperationException(
                last == null ? "Owned Sandboxie resources were not observed." : last.Message,
                last);
        }

        private static int GetBatchCreateObservationTimeoutMilliseconds(int sandboxCount)
        {
            return Math.Max(30000, checked(Math.Max(1, sandboxCount) * 30000));
        }

        private static IList<OwnershipClaim> FindAvailableOwnedClaims(
            OwnershipSnapshot baseline,
            string iniPath,
            string sandboxRoot,
            IEnumerable<string> aliases)
        {
            OwnershipSnapshot current = ReadOwnershipSnapshot(iniPath, sandboxRoot);
            var candidates = new List<OwnershipClaim>();
            foreach (string alias in aliases ?? Enumerable.Empty<string>())
            {
                string[] matches = current.AliasesBySection
                    .Where(item => string.Equals(item.Value, alias, StringComparison.Ordinal))
                    .Select(item => item.Key)
                    .ToArray();
                if (matches.Length > 1)
                {
                    throw new InvalidOperationException("BoxAlias ownership is not unique: " + alias);
                }
                if (matches.Length == 1)
                {
                    string sectionName = AuditSafety.FindUniqueCurrentName(current.Sections, matches[0]);
                    string directoryName = AuditSafety.FindUniqueCurrentName(current.Directories, matches[0]);
                    candidates.Add(new OwnershipClaim
                    {
                        InternalName = sectionName,
                        Alias = alias,
                        DirectoryName = directoryName
                    });
                }
            }
            return candidates.Count == 0
                ? candidates
                : AuditSafety.SelectOwnedResources(baseline, current, candidates);
        }

        private static void WriteLifecycleState(string path, LifecycleState state)
        {
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            string fullPath = Path.GetFullPath(path);
            string temporary = fullPath + ".ShaheAuditWrite-" + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, serializer.Serialize(state), new UTF8Encoding(false));
                if (File.Exists(fullPath)) File.Replace(temporary, fullPath, null);
                else File.Move(temporary, fullPath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static void CloseOwnedTarget(
            Process process,
            IntPtr mainWindow,
            ref bool dynamicExit,
            ref bool forcedStop,
            IList<string> errors)
        {
            if (process == null || process.HasExited)
            {
                return;
            }
            try
            {
                dynamicExit = Win32Automation.PostCommand(
                        mainWindow,
                        AuditSafety.FindUniqueMenuCommand(
                            Win32Automation.GetMenuCommands(mainWindow),
                            "退出软件\tAlt+F4"))
                    && Win32Automation.WaitForExit(process, 5000);
            }
            catch (Exception exception)
            {
                errors.Add("Dynamic target cleanup: " + exception.Message);
            }
            if (!process.HasExited)
            {
                Win32Automation.CloseWindow(mainWindow);
                Win32Automation.WaitForExit(process, 3000);
            }
            if (!process.HasExited)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(5000);
                    forcedStop = true;
                }
                catch (Exception exception)
                {
                    errors.Add("Owned target cleanup: " + exception.Message);
                }
            }
        }

        private static IEnumerable<IDictionary<string, object>> CleanupOwnedClaims(
            PreflightReport baseline,
            OwnershipSnapshot baselineOwnership,
            IEnumerable<OwnershipClaim> claims)
        {
            var rows = new List<IDictionary<string, object>>();
            IList<OwnershipClaim> requested = (claims ?? Enumerable.Empty<OwnershipClaim>()).ToList();
            if (requested.Count == 0)
            {
                return rows;
            }
            OwnershipSnapshot current = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
            IList<OwnershipClaim> owned;
            try
            {
                owned = AuditSafety.SelectOwnedResourcesForCleanup(baselineOwnership, current, requested);
            }
            catch (Exception exception)
            {
                rows.Add(new Dictionary<string, object> { { "status", "inconclusive" }, { "detail", exception.Message } });
                return rows;
            }

            var client = new SandboxieClient { SandboxiePath = baseline.SandboxiePath };
            foreach (OwnershipClaim claim in owned)
            {
                Action validate = () =>
                {
                    current = ReadOwnershipSnapshot(baseline.SandboxieIniPath, baseline.SandboxRoot);
                    AuditSafety.SelectOwnedResourcesForCleanup(baselineOwnership, current, new[] { claim });
                    if (current.Sections.Contains(claim.InternalName, StringComparer.OrdinalIgnoreCase)
                        && QuerySandboxPids(client, claim.InternalName).Count != 0)
                        throw new InvalidOperationException("Owned sandbox is running; deletion refused: " + claim.InternalName);
                };
                try { validate(); }
                catch (Exception exception)
                {
                    rows.Add(StepRow(claim.Alias, new AuditStepResult { Status = AuditStatus.Inconclusive, Detail = exception.Message }));
                    continue;
                }
                if (!current.Sections.Contains(claim.InternalName, StringComparer.OrdinalIgnoreCase)
                    && !current.AliasesBySection.ContainsKey(claim.InternalName))
                {
                    rows.Add(StepRow(claim.Alias, new AuditStepResult { Status = AuditStatus.Passed,
                        Detail = "Owned resource is already absent." }));
                    continue;
                }
                AuditStepResult deleted = AuditSafety.DeleteOwnedResource(
                    claim,
                    name =>
                    {
                        if (!current.Directories.Contains(claim.DirectoryName, StringComparer.OrdinalIgnoreCase)) return 0;
                        Tuple<int, string> command = client.RunSandboxieStart(
                            new[] { "/box:" + name, "delete_sandbox_silent" },
                            12,
                            null,
                            true,
                            false);
                        return command == null ? -1 : command.Item1;
                    },
                    name => !WaitForDirectoryMissing(Path.Combine(baseline.SandboxRoot, name), 8000),
                    name =>
                    {
                        Tuple<int, string> command = client.RunSbieIni(
                            new[] { "set", "/drv", name, "*", string.Empty },
                            8,
                            null);
                        if (command == null || command.Item1 != 0)
                        {
                            throw new InvalidOperationException("Sandboxie.ini section deletion failed: " + name);
                        }
                    }, validate);
                rows.Add(new Dictionary<string, object>
                {
                    { "internalName", claim.InternalName },
                    { "alias", claim.Alias },
                    { "status", deleted.Status.ToString().ToLowerInvariant() },
                    { "detail", deleted.Detail }
                });
            }
            return rows;
        }

        private static bool WaitForDirectoryMissing(string path, int timeoutMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            while (Directory.Exists(path) && timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                Thread.Sleep(100);
            }
            return !Directory.Exists(path);
        }

        private static bool DialogPassed(DialogObservation dialog, int edits, int combos, int buttons)
        {
            return dialog != null
                && dialog.Handle.Length != 0
                && dialog.Foreground
                && dialog.Stable
                && dialog.Closed
                && dialog.EditCount >= edits
                && dialog.ComboBoxCount >= combos
                && dialog.ButtonCount >= buttons;
        }

        private static int ResolveCommand(
            IntPtr mainWindow,
            IDictionary<string, int> commandIds,
            string title)
        {
            int commandId = AuditSafety.FindUniqueMenuCommand(
                Win32Automation.GetMenuCommands(mainWindow),
                title);
            commandIds[title] = commandId;
            return commandId;
        }

        private static void LoadConfiguration(PreflightReport report)
        {
            try
            {
                var store = new ConfigStore(report.ConfigPath, string.Empty, () => 0L);
                var fallback = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                var settings = store.GetJson("settings_json", fallback) as IDictionary<string, object> ?? fallback;
                report.SandboxiePath = Value(settings, "sandboxie_path");
                report.SandboxRoot = LauncherConstants.NormalizeSandboxRootPath(Value(settings, "sandbox_root"));
                if (report.SandboxRoot.Length == 0)
                {
                    report.SandboxRoot = LauncherConstants.GetDefaultSandboxRoot();
                }
                var client = new SandboxieClient { SandboxiePath = report.SandboxiePath };
                if (!client.IsValidInstallPath(report.SandboxiePath))
                {
                    report.SandboxiePath = client.FindSandboxiePath();
                }
            }
            catch (Exception exception)
            {
                report.Errors.Add("config: " + exception.Message);
            }
        }

        private static void LoadSandboxieState(PreflightReport report)
        {
            try
            {
                var admin = new SandboxAdminService();
                Tuple<string, string> loaded = admin.ReadTextWithEncoding(report.SandboxieIniPath);
                if (loaded == null || loaded.Item1.Length == 0)
                {
                    report.Errors.Add("Sandboxie.ini is empty or unreadable.");
                    return;
                }
                SandboxIniSplitResult split = admin.SplitIniSections(loaded.Item1);
                report.IniReadable = split.Sections.Count != 0;
                report.IniSections = split.Sections.Select(section => section.Name).ToList();

                var client = new SandboxieClient { SandboxiePath = report.SandboxiePath };
                Tuple<int, string> enabled = client.RunSbieIni(new[] { "queryex", "/boxes", "*" }, 8, null);
                if (enabled == null || enabled.Item1 != 0)
                {
                    report.Errors.Add("SbieIni queryex /boxes * failed.");
                }
                else
                {
                    report.EnabledBoxes = client.ParseEnabledSandboxNames(enabled.Item2, enabled.Item1);
                }

                IList<SandboxieIniSection> sections = split.Sections.Select(section => new SandboxieIniSection
                {
                    Name = section.Name,
                    Block = section.Block
                }).ToList();
                report.AliasesBySection = client.BuildAliasMap(sections);
                report.SandboxDirectories = Directory.Exists(report.SandboxRoot)
                    ? Directory.GetDirectories(report.SandboxRoot).Select(Path.GetFileName).OrderBy(name => name).ToList()
                    : new List<string>();
                report.ShaheAuditSections = report.IniSections
                    .Where(name => name.StartsWith("ShaheAudit_", StringComparison.OrdinalIgnoreCase)
                        || (report.AliasesBySection.ContainsKey(name)
                            && report.AliasesBySection[name].StartsWith("ShaheAudit_", StringComparison.Ordinal)))
                    .OrderBy(name => name)
                    .ToList();
                report.ShaheAuditDirectories = report.SandboxDirectories
                    .Where(name => name.StartsWith("ShaheAudit_", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(name => name)
                    .ToList();
            }
            catch (Exception exception)
            {
                report.Errors.Add("sandboxie: " + exception.Message);
            }
        }

        private static void LoadProcesses(PreflightReport report)
        {
            int currentId = Process.GetCurrentProcess().Id;
            var launcherChain = new HashSet<int>(GetAncestorProcessIds());
            var observations = new List<ProcessObservation>();
            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    if (launcherChain.Contains(process.Id))
                    {
                        continue;
                    }
                    string name = process.ProcessName ?? string.Empty;
                    string path = ProcessPath(process);
                    bool relevant = IsRelevantProcess(name, path, report.TargetPath);
                    if (relevant)
                    {
                        observations.Add(new ProcessObservation
                        {
                            ProcessId = process.Id,
                            Name = name,
                            Path = path
                        });
                    }
                }
            }
            report.RelevantProcesses = observations.OrderBy(item => item.ProcessId).ToList();
            report.ProcessCounts["target"] = observations.Count(item => SamePath(item.Path, report.TargetPath));
            report.ProcessCounts["Start.exe"] = CountName(observations, "Start");
            report.ProcessCounts["powershell.exe"] = CountName(observations, "powershell");
            report.ProcessCounts["pwsh.exe"] = CountName(observations, "pwsh");
            report.ProcessCounts["AuditRunner.exe"] = observations.Count(item =>
                string.Equals(item.Name, "ShaheLauncher.AuditRunner", StringComparison.OrdinalIgnoreCase)
                && item.ProcessId != currentId);
            report.ProcessCounts["SbieCtrl.exe"] = CountName(observations, "SbieCtrl");
            report.ProcessCounts["SandboxieOther"] = observations.Count(item => IsSandboxieName(item.Name)
                && !string.Equals(item.Name, "SbieSvc", StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<int> GetAncestorProcessIds()
        {
            var parents = new Dictionary<int, int>();
            IntPtr snapshot = CreateToolhelp32Snapshot(2, 0);
            if (snapshot != new IntPtr(-1))
            {
                try
                {
                    var entry = new ProcessEntry32 { Size = (uint)Marshal.SizeOf(typeof(ProcessEntry32)) };
                    if (Process32First(snapshot, ref entry))
                    {
                        do
                        {
                            parents[unchecked((int)entry.ProcessId)] = unchecked((int)entry.ParentProcessId);
                            entry.Size = (uint)Marshal.SizeOf(typeof(ProcessEntry32));
                        }
                        while (Process32Next(snapshot, ref entry));
                    }
                }
                finally
                {
                    CloseHandle(snapshot);
                }
            }

            var result = new List<int>();
            int current = Process.GetCurrentProcess().Id;
            while (current > 0 && !result.Contains(current))
            {
                result.Add(current);
                int parent;
                if (!parents.TryGetValue(current, out parent) || parent <= 0)
                {
                    break;
                }
                current = parent;
            }
            return result;
        }

        private static void SetChecks(PreflightReport report, AuditOptions options)
        {
            report.Checks["targetHash"] = HashMatches(report.TargetSha256, options.ExpectedTargetSha256);
            report.Checks["configHash"] = HashMatches(report.ConfigSha256, options.ExpectedConfigSha256);
            report.Checks["iniHash"] = HashMatches(report.SandboxieIniSha256, options.ExpectedIniSha256);
            report.Checks["iniReadable"] = report.IniReadable;
            report.Checks["sandboxiePath"] = Directory.Exists(report.SandboxiePath);
            report.Checks["sandboxRoot"] = Directory.Exists(report.SandboxRoot);
            report.Checks["serviceRunning"] = string.Equals(report.ServiceStatus, "Running", StringComparison.OrdinalIgnoreCase);
            foreach (string key in new[]
            {
                "target", "Start.exe", "powershell.exe", "pwsh.exe", "AuditRunner.exe",
                "SbieCtrl.exe", "SandboxieOther"
            })
            {
                report.Checks[key] = report.ProcessCounts.ContainsKey(key) && report.ProcessCounts[key] == 0;
            }
            report.Checks["shaheAuditSections"] = report.ShaheAuditSections.Count == 0;
            report.Checks["shaheAuditDirectories"] = report.ShaheAuditDirectories.Count == 0;
        }

        private static string WriteJson(string outputDirectory, string prefix, object value)
        {
            string directory = string.IsNullOrWhiteSpace(outputDirectory)
                ? Environment.CurrentDirectory
                : Path.GetFullPath(outputDirectory);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, prefix + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "-result.json");
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            File.WriteAllText(path, serializer.Serialize(value), new UTF8Encoding(false));
            return path;
        }

        private static int WriteUnsupported(AuditOptions options)
        {
            string path = WriteJson(options.OutputDirectory, "auditrunner-unsupported", new Dictionary<string, object>
            {
                { "kind", "auditrunner-" + options.Mode.ToString().ToLowerInvariant() },
                { "timestamp", Timestamp() },
                { "status", "inconclusive" },
                { "detail", "Mode is not implemented yet." }
            });
            Console.WriteLine(path);
            return 3;
        }

        private static bool IsRejected(Action action)
        {
            try { action(); return false; }
            catch (InvalidOperationException) { return true; }
        }

        private static void RequireFileOption(string path, string option)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new InvalidOperationException(option + " must name an existing file.");
            }
        }

        private static string HashFile(string path, IList<string> errors)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                using (SHA256 hash = SHA256.Create())
                {
                    return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
            catch (Exception exception)
            {
                errors.Add("hash " + path + ": " + exception.Message);
                return string.Empty;
            }
        }

        private static string GetServiceStatus(string serviceName, IList<string> errors)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    return service.Status.ToString();
                }
            }
            catch (Exception exception)
            {
                errors.Add("service " + serviceName + ": " + exception.Message);
                return "Unavailable";
            }
        }

        private static string ProcessPath(Process process)
        {
            try { return process.MainModule == null ? string.Empty : process.MainModule.FileName ?? string.Empty; }
            catch { return string.Empty; }
        }

        private static bool IsRelevantProcess(string name, string path, string targetPath)
        {
            return SamePath(path, targetPath)
                || string.Equals(name, "Start", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "powershell", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "pwsh", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "ShaheLauncher.AuditRunner", StringComparison.OrdinalIgnoreCase)
                || IsSandboxieName(name);
        }

        private static bool IsSandboxieName(string name)
        {
            return (name ?? string.Empty).IndexOf("sbie", StringComparison.OrdinalIgnoreCase) >= 0
                || (name ?? string.Empty).IndexOf("sandboxie", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int CountName(IEnumerable<ProcessObservation> processes, string name)
        {
            return processes.Count(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static bool SamePath(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }
            try
            {
                return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool HashMatches(string actual, string expected)
        {
            return !string.IsNullOrWhiteSpace(expected)
                && string.Equals(actual, expected.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ProcessEntry32
        {
            public uint Size;
            public uint Usage;
            public uint ProcessId;
            public IntPtr DefaultHeapId;
            public uint ModuleId;
            public uint Threads;
            public uint ParentProcessId;
            public int BasePriority;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ExecutableFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry32 entry);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry32 entry);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        private static string Value(IDictionary<string, object> values, string key)
        {
            object value;
            return values != null && values.TryGetValue(key, out value)
                ? Convert.ToString(value) ?? string.Empty
                : string.Empty;
        }

        private static string Timestamp()
        {
            return DateTimeOffset.Now.ToString("o");
        }
    }
}
