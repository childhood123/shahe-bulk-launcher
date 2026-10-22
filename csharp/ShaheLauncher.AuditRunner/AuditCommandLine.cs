using System;
using System.Collections.Generic;

namespace ShaheLauncher.AuditRunner
{
    public static class AuditCommandLine
    {
        public static AuditOptions Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new InvalidOperationException("Audit mode is required.");
            }

            var options = new AuditOptions { Mode = ParseMode(args[0]) };
            for (int index = 1; index < args.Length; index++)
            {
                string argument = args[index] ?? string.Empty;
                if (string.Equals(argument, "--execute", StringComparison.OrdinalIgnoreCase))
                {
                    options.Execute = true;
                    continue;
                }

                string value = RequireValue(args, ref index, argument);
                switch (argument.ToLowerInvariant())
                {
                    case "--baseline": options.BaselinePath = value; break;
                    case "--prefix": options.Prefix = value; break;
                    case "--target": options.TargetPath = value; break;
                    case "--config": options.ConfigPath = value; break;
                    case "--sandboxie-ini": options.SandboxieIniPath = value; break;
                    case "--output": options.OutputDirectory = value; break;
                    case "--state": options.StatePath = value; break;
                    case "--stage": options.Stage = value; break;
                    case "--expected-target-sha256": options.ExpectedTargetSha256 = value; break;
                    case "--expected-config-sha256": options.ExpectedConfigSha256 = value; break;
                    case "--expected-ini-sha256": options.ExpectedIniSha256 = value; break;
                    default: throw new InvalidOperationException("Unknown argument: " + argument);
                }
            }

            if (options.Mode == AuditMode.Lifecycle)
            {
                if (!options.Execute
                    || options.BaselinePath.Length == 0
                    || !IsAuditPrefix(options.Prefix)
                    || options.StatePath.Length == 0
                    || !IsLifecycleStage(options.Stage))
                {
                    throw new InvalidOperationException(
                        "Lifecycle requires --execute, --baseline, --state, a known --stage, and a unique ShaheAudit_* prefix.");
                }
            }
            else if (options.Execute)
            {
                throw new InvalidOperationException("--execute is valid only for lifecycle.");
            }

            return options;
        }

        private static AuditMode ParseMode(string value)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "contract": return AuditMode.Contract;
                case "preflight": return AuditMode.Preflight;
                case "dialog-gate": return AuditMode.DialogGate;
                case "lifecycle": return AuditMode.Lifecycle;
                case "verify-clean": return AuditMode.VerifyClean;
                default: throw new InvalidOperationException("Unknown audit mode: " + value);
            }
        }

        private static string RequireValue(string[] args, ref int index, string argument)
        {
            if (!argument.StartsWith("--", StringComparison.Ordinal)
                || index + 1 >= args.Length
                || (args[index + 1] ?? string.Empty).StartsWith("--", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Missing value for " + argument);
            }
            index++;
            return (args[index] ?? string.Empty).Trim();
        }

        private static bool IsAuditPrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || !value.StartsWith("ShaheAudit_", StringComparison.Ordinal)
                || value.Length <= "ShaheAudit_".Length)
            {
                return false;
            }
            foreach (char character in value)
            {
                if (!(char.IsLetterOrDigit(character) || character == '_'))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsLifecycleStage(string value)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "batch-create":
                case "create-group":
                case "launch-group":
                case "schedule-launch":
                case "schedule-close":
                case "cancel-schedules":
                case "batch-rename":
                case "batch-delete":
                case "cleanup":
                    return true;
                default:
                    return false;
            }
        }
    }
}
