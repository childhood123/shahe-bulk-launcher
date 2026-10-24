using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace ShaheLauncher.AuditRunner
{
    public static class AuditSafety
    {
        private static readonly string[] LifecycleStages =
        {
            "batch-create", "create-group", "launch-group", "schedule-launch",
            "schedule-close", "cancel-schedules", "batch-rename", "batch-delete"
        };

        public static int FindUniqueMenuCommand(
            IEnumerable<MenuCommandInfo> commands,
            string expectedTitle)
        {
            MenuCommandInfo[] matches = (commands ?? Enumerable.Empty<MenuCommandInfo>())
                .Where(command => command != null
                    && string.Equals(command.Title, expectedTitle, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException("Menu title must match exactly once: " + expectedTitle);
            }
            return matches[0].Id;
        }

        public static string FindUniqueCurrentName(
            IEnumerable<string> names,
            string expectedName)
        {
            string[] matches = (names ?? Enumerable.Empty<string>())
                .Where(name => string.Equals(name, expectedName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException("Current resource name must match exactly once: " + expectedName);
            }
            return matches[0];
        }

        public static IList<OwnershipClaim> SelectOwnedResources(
            OwnershipSnapshot baseline,
            OwnershipSnapshot current,
            IEnumerable<OwnershipClaim> claims)
        {
            return SelectOwnedResources(baseline, current, claims, false);
        }

        private static IList<OwnershipClaim> SelectOwnedResources(
            OwnershipSnapshot baseline,
            OwnershipSnapshot current,
            IEnumerable<OwnershipClaim> claims,
            bool allowMissing)
        {
            if (baseline == null || current == null || !baseline.IniReadable || !current.IniReadable)
            {
                throw new InvalidOperationException("Sandboxie.ini is missing or unreadable.");
            }

            IList<OwnershipClaim> requested = (claims ?? Enumerable.Empty<OwnershipClaim>()).ToList();
            if (requested.Count == 0
                || requested.Any(claim => claim == null
                    || string.IsNullOrWhiteSpace(claim.InternalName)
                    || string.IsNullOrWhiteSpace(claim.Alias)
                    || string.IsNullOrWhiteSpace(claim.DirectoryName)))
            {
                throw new InvalidOperationException("Ownership claims are incomplete.");
            }
            if (requested.Select(claim => claim.InternalName).Distinct(StringComparer.OrdinalIgnoreCase).Count() != requested.Count
                || requested.Select(claim => claim.Alias).Distinct(StringComparer.Ordinal).Count() != requested.Count
                || requested.Select(claim => claim.DirectoryName).Distinct(StringComparer.OrdinalIgnoreCase).Count() != requested.Count)
            {
                throw new InvalidOperationException("Ownership claims are not unique.");
            }

            var baselineDirectories = new HashSet<string>(
                baseline.Directories ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase);
            var currentDirectories = new HashSet<string>(
                current.Directories ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase);
            IDictionary<string, string> baselineAliases = baseline.AliasesBySection
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            IDictionary<string, string> currentAliases = current.AliasesBySection
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var baselineSections = new HashSet<string>(
                (baseline.Sections ?? new List<string>()).Concat(baselineAliases.Keys),
                StringComparer.OrdinalIgnoreCase);
            var currentSections = new HashSet<string>(
                (current.Sections ?? new List<string>()).Concat(currentAliases.Keys),
                StringComparer.OrdinalIgnoreCase);

            foreach (OwnershipClaim claim in requested)
            {
                if (baselineSections.Contains(claim.InternalName)
                    || baselineDirectories.Contains(claim.DirectoryName)
                    || baselineAliases.Values.Any(value => string.Equals(value, claim.Alias, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException("Claim overlaps the preflight baseline: " + claim.InternalName);
                }

                if (allowMissing
                    && !currentSections.Contains(claim.InternalName)
                    && !currentDirectories.Contains(claim.DirectoryName)
                    && !currentAliases.Values.Contains(claim.Alias, StringComparer.Ordinal))
                {
                    continue;
                }

                string currentAlias;
                if (!currentSections.Contains(claim.InternalName)
                    || !currentAliases.TryGetValue(claim.InternalName, out currentAlias)
                    || !string.Equals(currentAlias, claim.Alias, StringComparison.Ordinal)
                    || (!allowMissing && !currentDirectories.Contains(claim.DirectoryName)))
                {
                    throw new InvalidOperationException("Ownership changed or is incomplete: " + claim.InternalName);
                }
                if (currentAliases.Count(pair => string.Equals(pair.Value, claim.Alias, StringComparison.Ordinal)) != 1)
                {
                    throw new InvalidOperationException("BoxAlias ownership is not unique: " + claim.Alias);
                }
            }

            return requested;
        }

        public static void RenameOwnedAliases(
            IList<OwnershipClaim> claims,
            IList<string> aliases,
            Func<string, string, int> setAlias,
            Action reload,
            Action commit)
        {
            if (claims == null || aliases == null || claims.Count != aliases.Count
                || claims.Count == 0 || claims.Any(claim => claim == null)
                || setAlias == null || reload == null || commit == null)
            {
                throw new InvalidOperationException("Alias rename inputs are incomplete.");
            }
            string[] originals = claims.Select(claim => claim.Alias).ToArray();
            int attempted = 0;
            try
            {
                for (int index = 0; index < claims.Count; index++)
                {
                    attempted = index + 1;
                    if (setAlias(claims[index].InternalName, aliases[index]) != 0)
                    {
                        throw new InvalidOperationException("BoxAlias rename failed: " + claims[index].InternalName);
                    }
                }
                reload();
                commit();
            }
            catch (Exception failure)
            {
                var rollbackErrors = new List<string>();
                for (int index = attempted - 1; index >= 0; index--)
                {
                    try
                    {
                        if (setAlias(claims[index].InternalName, originals[index]) != 0)
                        {
                            throw new InvalidOperationException("BoxAlias restore failed: " + claims[index].InternalName);
                        }
                        claims[index].Alias = originals[index];
                    }
                    catch (Exception error) { rollbackErrors.Add(error.Message); }
                }
                try { reload(); }
                catch (Exception error) { rollbackErrors.Add(error.Message); }
                if (rollbackErrors.Count != 0)
                {
                    throw new InvalidOperationException(failure.Message + "; Alias rollback: "
                        + string.Join("; ", rollbackErrors), failure);
                }
                throw;
            }
        }

        public static IList<OwnershipClaim> SelectOwnedResourcesForCleanup(
            OwnershipSnapshot baseline,
            OwnershipSnapshot current,
            IEnumerable<OwnershipClaim> claims)
        {
            return SelectOwnedResources(baseline, current, claims, true);
        }

        public static AuditStepResult VerifyOwnedResourcesAbsent(
            OwnershipSnapshot baseline,
            OwnershipSnapshot current,
            IEnumerable<OwnershipClaim> claims)
        {
            try
            {
                IList<OwnershipClaim> owned = SelectOwnedResourcesForCleanup(baseline, current, claims);
                bool remains = owned.Any(claim =>
                    current.Sections.Contains(claim.InternalName, StringComparer.OrdinalIgnoreCase)
                    || current.Directories.Contains(claim.DirectoryName, StringComparer.OrdinalIgnoreCase)
                    || current.AliasesBySection.ContainsKey(claim.InternalName)
                    || current.AliasesBySection.Values.Contains(claim.Alias, StringComparer.Ordinal));
                return remains ? Inconclusive("Exact owned resource remains.")
                    : new AuditStepResult { Status = AuditStatus.Passed };
            }
            catch (Exception exception) { return Inconclusive(exception.Message); }
        }

        public static void ConfirmOwnedLaunchStart(
            IEnumerable<OwnershipClaim> claims,
            Func<string, IEnumerable<int>> readPids,
            Action launchStarting)
        {
            foreach (OwnershipClaim claim in claims)
            {
                if (readPids(claim.InternalName).Any())
                    throw new InvalidOperationException("Owned sandbox was already running: " + claim.Alias);
            }
            launchStarting();
        }

        public static AuditStepResult DeleteOwnedResource(
            OwnershipClaim claim,
            Func<string, int> deleteSandboxSilent,
            Func<string, bool> directoryExists,
            Action<string> deleteIniSection,
            Action validateOwnership = null)
        {
            if (claim == null || deleteSandboxSilent == null || directoryExists == null || deleteIniSection == null)
            {
                throw new InvalidOperationException("Cleanup inputs are incomplete.");
            }
            try
            {
                validateOwnership?.Invoke();
                int returnCode = deleteSandboxSilent(claim.InternalName);
                if (returnCode != 0)
                {
                    return Inconclusive("delete_sandbox_silent returned " + returnCode);
                }
                if (directoryExists(claim.DirectoryName))
                {
                    return Inconclusive("Owned sandbox directory still exists.");
                }
                validateOwnership?.Invoke();
                deleteIniSection(claim.InternalName);
                return new AuditStepResult { Status = AuditStatus.Passed };
            }
            catch (Exception exception)
            {
                return Inconclusive(exception.Message);
            }
        }

        public static AuditStatus ClassifyFailure(AuditFailureKind kind)
        {
            return kind == AuditFailureKind.ProductFailure
                ? AuditStatus.Failed
                : AuditStatus.Inconclusive;
        }

        public static IDictionary<string, object> AddOwnedGroupSettings(
            IDictionary<string, object> settings,
            string groupName,
            IEnumerable<string> members)
        {
            if (settings == null || string.IsNullOrWhiteSpace(groupName))
            {
                throw new InvalidOperationException("Group settings are incomplete.");
            }

            string suffix = "_Group";
            if (!groupName.StartsWith("ShaheAudit_", StringComparison.Ordinal)
                || !groupName.EndsWith(suffix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Owned group name is invalid.");
            }

            string prefix = groupName.Substring(0, groupName.Length - suffix.Length);
            string[] exactMembers = (members ?? Enumerable.Empty<string>()).ToArray();
            if (exactMembers.Length != 2
                || !exactMembers.SequenceEqual(
                    new[] { prefix + "_Alias-01", prefix + "_Alias-02" },
                    StringComparer.Ordinal))
            {
                throw new InvalidOperationException("Owned group members are not exact.");
            }

            var groups = ReadSandboxGroups(settings);
            if (groups.ContainsKey(groupName))
            {
                throw new InvalidOperationException("Owned group already exists.");
            }
            groups[groupName] = exactMembers.ToList();

            var updated = new Dictionary<string, object>(settings, StringComparer.Ordinal);
            updated["sandbox_groups"] = groups.ToDictionary(
                item => item.Key,
                item => (object)new List<string>(item.Value),
                StringComparer.Ordinal);
            return updated;
        }

        public static AuditStepResult RestoreOwnedFile(
            string path,
            byte[] baselineBytes,
            string baselineSha256,
            string expectedCurrentSha256)
        {
            string temporaryPath = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(path)
                    || baselineBytes == null
                    || !File.Exists(path)
                    || !string.Equals(HashBytes(baselineBytes), baselineSha256, StringComparison.OrdinalIgnoreCase))
                {
                    return Inconclusive("Owned file hash verification failed; restore was refused.");
                }

                string currentHash = HashFile(path);
                if (string.Equals(currentHash, baselineSha256, StringComparison.OrdinalIgnoreCase))
                {
                    return new AuditStepResult { Status = AuditStatus.Passed };
                }
                if (!string.Equals(currentHash, expectedCurrentSha256, StringComparison.OrdinalIgnoreCase))
                {
                    return Inconclusive("Owned file hash verification failed; restore was refused.");
                }

                string fullPath = Path.GetFullPath(path);
                temporaryPath = fullPath + ".ShaheAuditRestore-" + Guid.NewGuid().ToString("N") + ".tmp";
                File.WriteAllBytes(temporaryPath, baselineBytes);
                File.Replace(temporaryPath, fullPath, null);
                return string.Equals(HashFile(fullPath), baselineSha256, StringComparison.OrdinalIgnoreCase)
                    ? new AuditStepResult { Status = AuditStatus.Passed }
                    : Inconclusive("Owned file restore hash did not match the baseline.");
            }
            catch (Exception exception)
            {
                return Inconclusive(exception.Message);
            }
            finally
            {
                if (temporaryPath.Length != 0 && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static IList<OwnershipClaim> SelectOwnedGroupClaims(LifecycleState state)
        {
            if (state == null
                || string.IsNullOrWhiteSpace(state.Prefix)
                || !string.Equals(state.GroupName, state.Prefix + "_Group", StringComparison.Ordinal)
                || state.GroupMembers == null
                || state.OwnershipClaims == null)
            {
                throw new InvalidOperationException("Owned group state is incomplete.");
            }

            bool renamed = (state.CompletedStages ?? new List<string>())
                .Contains("batch-rename", StringComparer.Ordinal);
            string[] expectedMembers = renamed
                ? new[] { state.Prefix + "_Renamed 01", state.Prefix + "_Renamed 02" }
                : new[] { state.Prefix + "_Alias-01", state.Prefix + "_Alias-02" };
            if (!state.GroupMembers.SequenceEqual(expectedMembers, StringComparer.Ordinal)
                || state.OwnershipClaims.Count != expectedMembers.Length)
            {
                throw new InvalidOperationException("Owned group members changed.");
            }

            var byAlias = state.OwnershipClaims.ToDictionary(claim => claim.Alias, StringComparer.Ordinal);
            if (byAlias.Count != expectedMembers.Length)
            {
                throw new InvalidOperationException("Owned group claims are not unique.");
            }
            try
            {
                return expectedMembers.Select(alias => byAlias[alias]).ToList();
            }
            catch (KeyNotFoundException exception)
            {
                throw new InvalidOperationException("Owned group claim is missing.", exception);
            }
        }

        public static void ValidateLifecycleStage(
            LifecycleState state,
            string baselineId,
            string prefix,
            string stage)
        {
            if (state == null
                || !string.Equals(state.BaselineId, baselineId, StringComparison.Ordinal)
                || !string.Equals(state.Prefix, prefix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Lifecycle state does not match the baseline and prefix.");
            }

            IList<string> completed = state.CompletedStages ?? new List<string>();
            if (completed.Count > LifecycleStages.Length
                || !completed.SequenceEqual(LifecycleStages.Take(completed.Count), StringComparer.Ordinal))
            {
                throw new InvalidOperationException("Lifecycle state contains an invalid stage history.");
            }
            if (completed.Count == LifecycleStages.Length
                || !string.Equals(stage, LifecycleStages[completed.Count], StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Lifecycle stage is out of order or already completed.");
            }
        }

        public static LifecycleState LoadOrCreateLifecycleState(
            string path,
            string baselineId,
            string prefix,
            string stage)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Lifecycle state path is required.");
            }

            string fullPath = Path.GetFullPath(path);
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            LifecycleState state;
            if (File.Exists(fullPath))
            {
                try
                {
                    state = serializer.Deserialize<LifecycleState>(File.ReadAllText(fullPath));
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Lifecycle state is unreadable.", exception);
                }
            }
            else
            {
                if (!string.Equals(stage, "batch-create", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Only batch-create may initialize lifecycle state.");
                }
                state = new LifecycleState { BaselineId = baselineId, Prefix = prefix };
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(serializer.Serialize(state));
                }
            }

            ValidateLifecycleStage(state, baselineId, prefix, stage);
            return state;
        }

        public static LifecycleState LoadLifecycleStateForCleanup(
            string path,
            string baselineId,
            string prefix)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new InvalidOperationException("Cleanup lifecycle state is missing.");
            }

            LifecycleState state;
            try
            {
                state = new JavaScriptSerializer { MaxJsonLength = int.MaxValue }
                    .Deserialize<LifecycleState>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Cleanup lifecycle state is unreadable.", exception);
            }

            IList<string> completed = state == null ? null : state.CompletedStages;
            if (state == null
                || !string.Equals(state.BaselineId, baselineId, StringComparison.Ordinal)
                || !string.Equals(state.Prefix, prefix, StringComparison.Ordinal)
                || completed == null
                || completed.Count > LifecycleStages.Length
                || !completed.SequenceEqual(LifecycleStages.Take(completed.Count), StringComparer.Ordinal)
                || state.OwnershipClaims == null
                || state.OwnershipClaims.Count == 0)
            {
                throw new InvalidOperationException("Cleanup lifecycle state is not bound or has no ownership claims.");
            }
            return state;
        }

        internal static IDictionary<string, IList<string>> ReadSandboxGroups(
            IDictionary<string, object> settings)
        {
            object value;
            if (settings == null || !settings.TryGetValue("sandbox_groups", out value) || value == null)
            {
                return new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            }

            Dictionary<string, List<string>> deserialized = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(
                JsonConvert.SerializeObject(value));
            return (deserialized ?? new Dictionary<string, List<string>>())
                .ToDictionary(
                    item => item.Key,
                    item => (IList<string>)new List<string>(item.Value ?? new List<string>()),
                    StringComparer.Ordinal);
        }

        private static string HashFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (SHA256 algorithm = SHA256.Create())
            {
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static string HashBytes(byte[] value)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                return BitConverter.ToString(algorithm.ComputeHash(value)).Replace("-", string.Empty);
            }
        }

        private static AuditStepResult Inconclusive(string detail)
        {
            return new AuditStepResult { Status = AuditStatus.Inconclusive, Detail = detail ?? string.Empty };
        }
    }
}
