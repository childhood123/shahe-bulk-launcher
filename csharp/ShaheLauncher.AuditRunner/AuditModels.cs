using System;
using System.Collections.Generic;

namespace ShaheLauncher.AuditRunner
{
    public enum AuditMode
    {
        Contract,
        Preflight,
        DialogGate,
        Lifecycle,
        VerifyClean
    }

    public enum AuditStatus
    {
        Passed,
        Failed,
        Inconclusive
    }

    public enum AuditFailureKind
    {
        FocusNotConfirmed,
        IndexDrift,
        WindowInvalid,
        Timeout,
        MenuNotUnique,
        CleanupIncomplete,
        ProductFailure
    }

    public sealed class AuditOptions
    {
        public AuditMode Mode { get; set; }
        public bool Execute { get; set; }
        public string BaselinePath { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string ConfigPath { get; set; } = string.Empty;
        public string SandboxieIniPath { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public string StatePath { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public string ExpectedTargetSha256 { get; set; } = string.Empty;
        public string ExpectedConfigSha256 { get; set; } = string.Empty;
        public string ExpectedIniSha256 { get; set; } = string.Empty;
    }

    public sealed class MenuCommandInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public sealed class OwnershipSnapshot
    {
        public bool IniReadable { get; set; }
        public IList<string> Sections { get; set; } = new List<string>();
        public IDictionary<string, string> AliasesBySection { get; set; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public IList<string> Directories { get; set; } = new List<string>();
    }

    public sealed class OwnershipClaim
    {
        public string InternalName { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string DirectoryName { get; set; } = string.Empty;
    }

    public sealed class AuditStepResult
    {
        public AuditStatus Status { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    public sealed class LifecycleState
    {
        public string BaselineId { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public IList<string> CompletedStages { get; set; } = new List<string>();
        public IList<OwnershipClaim> OwnershipClaims { get; set; } = new List<OwnershipClaim>();
        public string GroupName { get; set; } = string.Empty;
        public IList<string> GroupMembers { get; set; } = new List<string>();
        public string ConfigurationBaselineBase64 { get; set; } = string.Empty;
        public string ExpectedConfigurationSha256 { get; set; } = string.Empty;
    }

    public sealed class ProcessObservation
    {
        public int ProcessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public sealed class PreflightReport
    {
        public string Kind { get; set; } = "auditrunner-preflight";
        public string Timestamp { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string BaselineId { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string TargetSha256 { get; set; } = string.Empty;
        public string ConfigPath { get; set; } = string.Empty;
        public string ConfigSha256 { get; set; } = string.Empty;
        public string SandboxieIniPath { get; set; } = string.Empty;
        public string SandboxieIniSha256 { get; set; } = string.Empty;
        public string SandboxiePath { get; set; } = string.Empty;
        public string SandboxRoot { get; set; } = string.Empty;
        public string ServiceStatus { get; set; } = string.Empty;
        public bool IniReadable { get; set; }
        public IList<string> EnabledBoxes { get; set; } = new List<string>();
        public IList<string> IniSections { get; set; } = new List<string>();
        public IDictionary<string, string> AliasesBySection { get; set; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public IList<string> SandboxDirectories { get; set; } = new List<string>();
        public IList<string> ShaheAuditSections { get; set; } = new List<string>();
        public IList<string> ShaheAuditDirectories { get; set; } = new List<string>();
        public IDictionary<string, int> ProcessCounts { get; set; }
            = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public IList<ProcessObservation> RelevantProcesses { get; set; } = new List<ProcessObservation>();
        public IDictionary<string, bool> Checks { get; set; }
            = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public IList<string> Errors { get; set; } = new List<string>();
    }
}
