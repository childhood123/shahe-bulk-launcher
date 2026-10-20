using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ShaheLauncher.Core.AppPaths
{
    public sealed class AppHistoryItem
    {
        public AppHistoryItem()
        {
            Path = string.Empty;
            Note = string.Empty;
        }

        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
    }

    public sealed class AppPathHistoryState
    {
        public AppPathHistoryState()
        {
            AppHistory = new List<AppHistoryItem>();
            MaxHistoryItems = 10;
            AppPath = string.Empty;
            AppDirectory = string.Empty;
            ComboItemPaths = new List<string>();
        }

        public IList<AppHistoryItem> AppHistory { get; set; }
        public int MaxHistoryItems { get; set; }
        public bool HidePath { get; set; }
        public string AppPath { get; set; }
        public string AppDirectory { get; set; }
        public IList<string> ComboItemPaths { get; set; }
    }

    public sealed class AppPathHistoryCallbacks
    {
        public Action SaveSettings { get; set; }
        public Action RefreshComboBox { get; set; }
        public Action<string> SetCurrentValue { get; set; }
        public Action FocusAppPathList { get; set; }
        public Action<string> SetStatus { get; set; }
    }

    public sealed class AppPathHistoryResolution
    {
        public AppPathHistoryResolution()
        {
            Path = string.Empty;
            Note = string.Empty;
        }

        public string Path { get; set; }
        public string Note { get; set; }
    }

    public sealed class AppPathHistoryView
    {
        public AppPathHistoryView()
        {
            ItemTexts = new List<string>();
            ItemPaths = new List<string>();
            CheckedIndices = new List<int>();
            SelectedIndex = -1;
            SelectedText = string.Empty;
        }

        public IList<string> ItemTexts { get; set; }
        public IList<string> ItemPaths { get; set; }
        public IList<int> CheckedIndices { get; set; }
        public int SelectedIndex { get; set; }
        public string SelectedText { get; set; }
    }

    public sealed class AppPathHistoryService
    {
        public const string OpenAppMenuText = "选择应用";
        public const string PasteAppMenuText = "粘贴 .exe 或文件路径";
        public const string EditNoteMenuText = "路径记忆备注";
        public const string AdjustCountMenuText = "调整记忆数";
        public const string EditNotePrompt = "请输入路径备注:";
        public const string HistoryCountPrompt = "请输入路径记忆数量:";
        public const string HistoryCountDialogTitle = "调整记忆数";
        public const string HistoryCountLabel = "路径记忆数量";
        public const string HidePathMenuText = "隐藏路径";
        public const string ShowPathMenuText = "显示路径";
        public const string DeleteCurrentMenuText = "删除当前记忆";
        public const string ClearMemoryMenuText = "清空所有路径记忆";
        public const string CopyPathMenuText = "复制完整路径";
        public const string ClearMemoryConfirmationText = "确定要清空所有路径记忆吗？";
        public const string DeleteCurrentConfirmationText = "确定要删除当前路径记忆吗？";
        public const string PathCopiedStatus = "路径已复制到剪贴板";
        public const string CopyPathFailedStatus = "复制路径失败";
        public const string CannotAccessClipboardStatus = "无法访问剪贴板";
        public const string InvalidClipboardStatus = "剪贴板没有包含有效的数据";
        public const string MemoryClearedStatus = "路径记忆已清空";
        public const string CurrentDeletedStatus = "当前路径已从记忆列表中删除";
        public const string CurrentNotInMemoryStatus = "当前路径不在记忆列表中";
        public const string PathHiddenStatus = "已隐藏完整路径";
        public const string PathShownStatus = "已显示完整路径";
        public const string PastedAppStatusPrefix = "已从剪贴板粘贴应用";
        public const string MemorySavedStatus = "路径记忆设置已保存";
        public const string NoteUpdatedStatus = "备注已更新";

        private static readonly Regex QuotedExePattern = new Regex(
            "[\\\"']([^\\\"']+?\\.exe)[\\\"']",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex DriveExePattern = new Regex(
            "[A-Za-z]:[\\\\/][^<>\\r\\n\\\"|]+?\\.exe",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex UncExePattern = new Regex(
            "\\\\\\\\[^<>\\r\\n\\\"|]+?\\.exe",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public bool AddToHistory(
            AppPathHistoryState state,
            string appPath,
            bool refresh,
            AppPathHistoryCallbacks callbacks)
        {
            if (state == null)
            {
                return false;
            }

            string parsedPath = ExtractExePathFromText(appPath);
            if (parsedPath.Length == 0 || state.MaxHistoryItems == 0)
            {
                return false;
            }

            if (!File.Exists(parsedPath))
            {
                return false;
            }

            if (state.AppHistory == null)
            {
                state.AppHistory = new List<AppHistoryItem>();
            }

            bool changed = false;
            AppHistoryItem existing = state.AppHistory.FirstOrDefault(item =>
                item != null && string.Equals(item.Path, parsedPath, StringComparison.Ordinal));
            if (existing != null)
            {
                state.AppHistory.Remove(existing);
                state.AppHistory.Insert(0, existing);
                changed = true;
            }
            else
            {
                state.AppHistory.Insert(0, new AppHistoryItem
                {
                    Path = parsedPath,
                    Note = System.IO.Path.GetFileName(parsedPath)
                });
                changed = true;
            }

            int limit = Math.Max(0, state.MaxHistoryItems);
            if (state.AppHistory.Count > limit)
            {
                while (state.AppHistory.Count > limit)
                {
                    state.AppHistory.RemoveAt(state.AppHistory.Count - 1);
                }
                changed = true;
            }

            if (changed)
            {
                callbacks?.SaveSettings?.Invoke();
                if (refresh)
                {
                    callbacks?.RefreshComboBox?.Invoke();
                }
            }

            return changed;
        }

        public static string FormatDisplayText(AppHistoryItem item, bool hidePath)
        {
            if (item == null)
            {
                return string.Empty;
            }

            string note = item.Note ?? string.Empty;
            return hidePath
                ? note
                : note + " - \"" + (item.Path ?? string.Empty) + "\"";
        }

        public AppHistoryItem GetAppHistoryItemByIndex(AppPathHistoryState state, object index)
        {
            int targetIndex;
            if (state == null || !TryConvertIndex(index, out targetIndex)
                || state.AppHistory == null
                || targetIndex < 0
                || targetIndex >= state.AppHistory.Count)
            {
                return null;
            }

            return state.AppHistory[targetIndex];
        }

        public AppHistoryItem GetAppHistoryItemByPath(AppPathHistoryState state, string appPath)
        {
            int index = FindAppHistoryIndexByPath(state, appPath);
            return index < 0 ? null : state.AppHistory[index];
        }

        public string GetAppListPathByIndex(AppPathHistoryState state, object index)
        {
            int targetIndex;
            if (state == null || !TryConvertIndex(index, out targetIndex)
                || state.ComboItemPaths == null
                || targetIndex < 0
                || targetIndex >= state.ComboItemPaths.Count)
            {
                return string.Empty;
            }

            return (state.ComboItemPaths[targetIndex] ?? string.Empty).Trim();
        }

        public int FindAppHistoryIndexByPath(AppPathHistoryState state, string appPath)
        {
            string targetPath = (appPath ?? string.Empty).Trim();
            if (state == null || targetPath.Length == 0 || state.AppHistory == null)
            {
                return -1;
            }

            string targetKey = NormalizePathKey(targetPath);
            for (int index = 0; index < state.AppHistory.Count; index++)
            {
                AppHistoryItem item = state.AppHistory[index];
                if (item != null && NormalizePathKey(item.Path) == targetKey)
                {
                    return index;
                }
            }

            return -1;
        }

        public AppPathHistoryResolution ExtractPathAndNote(
            AppPathHistoryState state,
            string text,
            object preferredIndex)
        {
            string preferredPath = GetAppListPathByIndex(state, preferredIndex);
            if (preferredPath.Length != 0)
            {
                AppHistoryItem item = GetAppHistoryItemByPath(state, preferredPath);
                return item == null
                    ? new AppPathHistoryResolution
                    {
                        Path = preferredPath,
                        Note = System.IO.Path.GetFileName(preferredPath)
                    }
                    : new AppPathHistoryResolution { Path = item.Path, Note = item.Note };
            }

            string currentText = text ?? string.Empty;
            if (currentText.Contains(" - \"") && currentText.EndsWith("\"", StringComparison.Ordinal))
            {
                int separator = currentText.LastIndexOf(" - \"", StringComparison.Ordinal);
                if (separator >= 0)
                {
                    return new AppPathHistoryResolution
                    {
                        Path = currentText.Substring(separator + 4, currentText.Length - separator - 5),
                        Note = currentText.Substring(0, separator)
                    };
                }
            }

            if (state?.AppHistory != null)
            {
                foreach (AppHistoryItem item in state.AppHistory)
                {
                    if (item != null && FormatDisplayText(item, state.HidePath) == currentText)
                    {
                        return new AppPathHistoryResolution { Path = item.Path, Note = item.Note };
                    }
                }
            }

            return new AppPathHistoryResolution
            {
                Path = currentText,
                Note = System.IO.Path.GetFileName(currentText)
            };
        }

        public AppHistoryItem ResolveAppHistoryEntry(
            AppPathHistoryState state,
            string text,
            object preferredIndex)
        {
            AppHistoryItem preferred = GetAppHistoryItemByIndex(state, preferredIndex);
            if (preferred != null)
            {
                return preferred;
            }

            AppPathHistoryResolution resolution = ExtractPathAndNote(state, text, preferredIndex);
            if (resolution.Path.Length == 0)
            {
                return null;
            }

            AppHistoryItem existing = GetAppHistoryItemByPath(state, resolution.Path);
            return existing ?? new AppHistoryItem
            {
                Path = resolution.Path,
                Note = resolution.Note.Length == 0 ? System.IO.Path.GetFileName(resolution.Path) : resolution.Note
            };
        }

        public AppPathHistoryView UpdateComboBox(
            AppPathHistoryState state,
            string currentText,
            object currentIndex,
            IEnumerable<string> checkedPaths)
        {
            var view = new AppPathHistoryView();
            if (state == null)
            {
                return view;
            }

            if (state.AppHistory == null)
            {
                state.AppHistory = new List<AppHistoryItem>();
            }

            view.ItemTexts = state.AppHistory
                .Where(item => item != null)
                .Select(item => FormatDisplayText(item, state.HidePath))
                .ToList();
            view.ItemPaths = state.AppHistory
                .Where(item => item != null)
                .Select(item => (item.Path ?? string.Empty).Trim())
                .ToList();
            state.ComboItemPaths = view.ItemPaths.ToList();

            int selectedIndex = -1;
            int preferred;
            if (TryConvertIndex(currentIndex, out preferred) && preferred >= 0 && preferred < state.AppHistory.Count)
            {
                selectedIndex = preferred;
            }
            else
            {
                string text = currentText ?? string.Empty;
                int byPath = FindAppHistoryIndexByPath(state, ExtractPathAndNote(state, text, -1).Path);
                if (byPath >= 0)
                {
                    selectedIndex = byPath;
                }
                else
                {
                    selectedIndex = view.ItemTexts.IndexOf(text);
                }
            }

            if (selectedIndex < 0 && state.AppHistory.Count != 0)
            {
                selectedIndex = 0;
            }

            view.SelectedIndex = selectedIndex;
            view.SelectedText = selectedIndex >= 0 && selectedIndex < view.ItemTexts.Count
                ? view.ItemTexts[selectedIndex]
                : string.Empty;
            if (selectedIndex >= 0 && selectedIndex < state.AppHistory.Count)
            {
                state.AppPath = state.AppHistory[selectedIndex].Path ?? string.Empty;
                state.AppDirectory = System.IO.Path.GetDirectoryName(state.AppPath) ?? string.Empty;
            }
            else
            {
                state.AppPath = string.Empty;
                state.AppDirectory = string.Empty;
            }

            var checkedKeys = new HashSet<string>(
                (checkedPaths ?? Enumerable.Empty<string>())
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(NormalizePathKey));
            view.CheckedIndices = view.ItemPaths
                .Select((path, index) => new { path, index })
                .Where(item => checkedKeys.Contains(NormalizePathKey(item.path)))
                .Select(item => item.index)
                .ToList();
            return view;
        }

        public string ResolvePastedAppPath(IEnumerable<string> fileNames, string text)
        {
            foreach (string fileName in fileNames ?? Enumerable.Empty<string>())
            {
                string path = ExtractExePathFromText(fileName);
                if (path.Length != 0)
                {
                    return path;
                }
            }

            return ExtractExePathFromText(text);
        }

        public bool SetCurrentAppPath(
            AppPathHistoryState state,
            string appPath,
            string statusPrefix,
            AppPathHistoryCallbacks callbacks)
        {
            string path = ExtractExePathFromText(appPath);
            if (state == null || path.Length == 0)
            {
                return false;
            }

            state.AppPath = path;
            state.AppDirectory = System.IO.Path.GetDirectoryName(path) ?? string.Empty;
            AddToHistory(state, path, true, callbacks);
            int index = FindAppHistoryIndexByPath(state, path);
            string display = index >= 0
                ? FormatDisplayText(state.AppHistory[index], state.HidePath)
                : path;
            callbacks?.SetCurrentValue?.Invoke(display);
            callbacks?.FocusAppPathList?.Invoke();
            callbacks?.SetStatus?.Invoke(
                (statusPrefix ?? "已选择") + ": " + System.IO.Path.GetFileName(path));
            return true;
        }

        public void ApplyHistoryCount(
            AppPathHistoryState state,
            int newCount,
            AppPathHistoryCallbacks callbacks)
        {
            if (state == null)
            {
                return;
            }

            state.MaxHistoryItems = Math.Max(0, Math.Min(20, newCount));
            callbacks?.SaveSettings?.Invoke();
            if (state.AppHistory != null && state.AppHistory.Count > state.MaxHistoryItems)
            {
                while (state.AppHistory.Count > state.MaxHistoryItems)
                {
                    state.AppHistory.RemoveAt(state.AppHistory.Count - 1);
                }
                callbacks?.SaveSettings?.Invoke();
                callbacks?.RefreshComboBox?.Invoke();
            }
            callbacks?.SetStatus?.Invoke("路径记忆数量已设置为: " + state.MaxHistoryItems);
        }

        public void ClearMemory(
            AppPathHistoryState state,
            bool confirmed,
            AppPathHistoryCallbacks callbacks)
        {
            if (state == null || !confirmed)
            {
                return;
            }

            state.AppHistory?.Clear();
            callbacks?.SaveSettings?.Invoke();
            callbacks?.RefreshComboBox?.Invoke();
            state.AppPath = string.Empty;
            state.AppDirectory = string.Empty;
            callbacks?.SetCurrentValue?.Invoke(string.Empty);
            callbacks?.SetStatus?.Invoke(MemoryClearedStatus);
        }

        public bool DeleteCurrent(
            AppPathHistoryState state,
            string currentText,
            object currentIndex,
            bool confirmed,
            AppPathHistoryCallbacks callbacks)
        {
            if (state == null || !confirmed)
            {
                return false;
            }

            AppPathHistoryResolution resolution = ExtractPathAndNote(state, currentText, currentIndex);
            if (resolution.Path.Length == 0 || state.AppHistory == null)
            {
                callbacks?.SetStatus?.Invoke(CurrentNotInMemoryStatus);
                return false;
            }

            AppHistoryItem item = state.AppHistory.FirstOrDefault(candidate =>
                candidate != null && string.Equals(candidate.Path, resolution.Path, StringComparison.Ordinal));
            if (item == null)
            {
                callbacks?.SetStatus?.Invoke(CurrentNotInMemoryStatus);
                return false;
            }

            state.AppHistory.Remove(item);
            callbacks?.SaveSettings?.Invoke();
            callbacks?.RefreshComboBox?.Invoke();
            callbacks?.SetStatus?.Invoke(CurrentDeletedStatus);
            return true;
        }

        public void TogglePathDisplay(
            AppPathHistoryState state,
            AppPathHistoryCallbacks callbacks)
        {
            if (state == null)
            {
                return;
            }

            state.HidePath = !state.HidePath;
            callbacks?.SaveSettings?.Invoke();
            callbacks?.RefreshComboBox?.Invoke();
            callbacks?.SetStatus?.Invoke(state.HidePath ? PathHiddenStatus : PathShownStatus);
        }

        public static string ExtractExePathFromText(string text)
        {
            string raw = (text ?? string.Empty).Trim();
            if (raw.Length == 0)
            {
                return string.Empty;
            }

            var candidates = new List<string> { raw };
            if (raw.Contains("\r") || raw.Contains("\n"))
            {
                candidates.AddRange(Regex.Split(raw, @"[\r\n]+").Where(part => part.Trim().Length != 0));
            }
            candidates.AddRange(QuotedExePattern.Matches(raw).Cast<Match>().Select(match => match.Groups[1].Value));
            candidates.AddRange(DriveExePattern.Matches(raw).Cast<Match>().Select(match => match.Value));
            candidates.AddRange(UncExePattern.Matches(raw).Cast<Match>().Select(match => match.Value));

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string candidate in candidates)
            {
                string value = (candidate ?? string.Empty).Trim().Trim('\ufeff').Trim('\0');
                if (value.Length == 0)
                {
                    continue;
                }

                if (value.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    value = Uri.UnescapeDataString(value.Substring(7));
                    if (value.StartsWith("/") && value.Length >= 3 && value[2] == ':')
                    {
                        value = value.Substring(1);
                    }
                    value = value.Replace('/', '\\');
                }

                if ((value.StartsWith("\"") && value.EndsWith("\""))
                    || (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2).Trim();
                }
                if (value.Length == 0 || value.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = Environment.ExpandEnvironmentVariables(value);
                if (value.StartsWith("~" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    || value == "~")
                {
                    string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    value = Path.Combine(profile, value.Length == 1 ? string.Empty : value.Substring(2));
                }
                if (value.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    value = value.Replace('/', '\\');
                }
                else if (Regex.IsMatch(value, @"^[A-Za-z]:/"))
                {
                    value = value.Replace('/', '\\');
                }

                int exeIndex = value.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                if (exeIndex >= 0)
                {
                    value = value.Substring(0, exeIndex + 4);
                }

                try
                {
                    value = Path.GetFullPath(value);
                }
                catch
                {
                }

                string key = value.ToUpperInvariant();
                if (!seen.Add(key))
                {
                    continue;
                }

                if (File.Exists(value) && value.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string NormalizePathKey(string path)
        {
            string value = (path ?? string.Empty).Trim();
            try
            {
                value = Path.GetFullPath(value);
            }
            catch
            {
            }

            return value.ToUpperInvariant();
        }

        private static bool TryConvertIndex(object value, out int index)
        {
            try
            {
                index = Convert.ToInt32(value);
                return true;
            }
            catch
            {
                index = -1;
                return false;
            }
        }

        private static string GetFileName(string path)
        {
            try
            {
                return System.IO.Path.GetFileName(path ?? string.Empty);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

}
