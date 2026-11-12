using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaheLauncher.Core.Common;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.Core.Updates
{
    public sealed class UpdateServiceCallbacks
    {
        public Func<string> GetManifestUrl { get; set; }
        public Action<string> Log { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action<Action> PostToUi { get; set; }
        public Func<int> GetProcessId { get; set; }
        public Func<long> GetUnixTimeSeconds { get; set; }
        public Func<string, byte[]> GetEmbeddedQrBytes { get; set; }
        public Action<UpdateDisplayResult> ShowUpdateResult { get; set; }
    }

    public sealed class UpdatePackageInfo
    {
        public UpdatePackageInfo()
        {
            Kind = string.Empty;
            KindLabel = string.Empty;
            Url = string.Empty;
            Sha256 = string.Empty;
            Signature = string.Empty;
            Version = string.Empty;
        }

        public string Kind { get; set; }
        public string KindLabel { get; set; }
        public string Url { get; set; }
        public string Sha256 { get; set; }
        public string Signature { get; set; }
        public bool Encrypted { get; set; }
        public long Size { get; set; }
        public string Version { get; set; }
    }

    public sealed class UpdateCommandPreparation
    {
        public UpdateCommandPreparation()
        {
            Error = string.Empty;
        }

        public IList<string> Command { get; set; }
        public string Error { get; set; }
    }

    public sealed class UpdateDisplayResult
    {
        public UpdateDisplayResult()
        {
            LatestVersion = string.Empty;
            StatusText = string.Empty;
            MessageTitle = string.Empty;
            MessageText = string.Empty;
            PromptText = string.Empty;
            ErrorText = string.Empty;
            ReleaseNotes = string.Empty;
        }

        public string ReleaseNotes { get; set; }
        public bool IsManualCheck { get; set; }
        public string LatestVersion { get; set; }
        public bool HasUpdate { get; set; }
        public bool ForceUpdate { get; set; }
        public bool ShouldPrompt { get; set; }
        public bool ShouldShowNotice { get; set; }
        public bool ShouldShowMessage { get; set; }
        public string StatusText { get; set; }
        public string MessageTitle { get; set; }
        public string MessageText { get; set; }
        public string PromptText { get; set; }
        public string ErrorText { get; set; }
        public UpdatePackageInfo Package { get; set; }
    }

    public sealed class UpdateService : IDisposable
    {
        private readonly bool _isPackagedApp;
        private readonly string _currentExecutablePath;
        private readonly UpdateHttpClient _httpClient;
        private readonly UpdateServiceCallbacks _callbacks;

        public UpdateService(
            string appDirectory,
            bool isPackagedApp,
            string currentExecutablePath,
            HttpMessageHandler handler,
            UpdateServiceCallbacks callbacks)
        {
            AppDirectory = FullPath(appDirectory);
            _isPackagedApp = isPackagedApp;
            _currentExecutablePath = FullPath(currentExecutablePath);
            _httpClient = new UpdateHttpClient(handler ?? new HttpClientHandler());
            _callbacks = callbacks ?? new UpdateServiceCallbacks();
        }

        public string AppDirectory { get; private set; }

        public static string GetWechatQrTitle()
        {
            return "微信打赏";
        }

        public static string GetAlipayQrTitle()
        {
            return "支付宝打赏";
        }

        public UpdatePackageInfo NormalizeUpdatePackage(
            IDictionary<string, object> package,
            string kind,
            string latestVersion)
        {
            if (package == null)
            {
                return null;
            }

            string url = Text(GetValue(package, "url", string.Empty)).Trim();
            Uri parsedUrl;
            if (!Uri.TryCreate(url, UriKind.Absolute, out parsedUrl)
                || !string.Equals(parsedUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new UpdatePackageInfo
            {
                Kind = Text(kind).Trim(),
                KindLabel = string.Equals(Text(kind).Trim(), "full", StringComparison.OrdinalIgnoreCase)
                    ? "完整更新包"
                    : "增量补丁",
                Url = url,
                Sha256 = Text(GetValue(package, "sha256", GetValue(package, "hash", string.Empty))).Trim(),
                Signature = Text(GetValue(package, "signature", GetValue(package, "sig", string.Empty))).Trim(),
                Encrypted = CommonUtilities.ParseBoolSetting(
                    GetValue(package, "encrypted", GetValue(package, "is_encrypted", false)), false),
                Size = NonNegativeLong(GetValue(package, "size", GetValue(package, "bytes", 0))),
                Version = Text(GetValue(package, "to", GetValue(package, "to_version", latestVersion))).Trim()
            };
        }

        public UpdatePackageInfo SelectUpdatePackage(
            IDictionary<string, object> manifest,
            string latestVersion)
        {
            foreach (object item in ToObjectList(GetValue(manifest, "packages", null)))
            {
                IDictionary<string, object> package = CopyDictionary(item);
                if (package == null)
                {
                    continue;
                }

                string kind = Text(GetValue(package, "kind", "full")).Trim();
                if (!string.Equals(kind, "full", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                UpdatePackageInfo selected = NormalizeUpdatePackage(package, "full", latestVersion);
                if (selected != null && selected.Sha256.Length != 0 && selected.Size > 0)
                {
                    return selected;
                }
            }

            IDictionary<string, object> single = CopyDictionary(GetValue(manifest, "package", null));
            if (single != null
                && string.Equals(Text(GetValue(single, "kind", "full")).Trim(), "full", StringComparison.OrdinalIgnoreCase))
            {
                UpdatePackageInfo selected = NormalizeUpdatePackage(single, "full", latestVersion);
                if (selected != null && selected.Sha256.Length != 0 && selected.Size > 0)
                {
                    return selected;
                }
            }

            IList<object> patches = ToObjectList(GetValue(manifest, "patches", null));
            foreach (object item in patches)
            {
                IDictionary<string, object> patch = CopyDictionary(item);
                if (patch == null)
                {
                    continue;
                }

                string fromVersion = Text(GetValue(patch, "from", GetValue(patch, "from_version", string.Empty))).Trim();
                string toVersion = Text(GetValue(patch, "to", GetValue(patch, "to_version", latestVersion))).Trim();
                if (!string.Equals(fromVersion, LauncherConstants.AppVersion, StringComparison.Ordinal)
                    || toVersion.Length != 0
                    && CommonUtilities.CompareVersions(toVersion, LauncherConstants.AppVersion) <= 0)
                {
                    continue;
                }

                UpdatePackageInfo selected = NormalizeUpdatePackage(patch, "patch", latestVersion);
                if (selected != null)
                {
                    return selected;
                }
            }

            return null;
        }

        public string FormatUpdateNotes(object releaseNotes)
        {
            IList<object> list = ToObjectList(releaseNotes);
            if (list.Count != 0)
            {
                var lines = new List<string>();
                foreach (object item in list)
                {
                    string line = Text(item).Trim();
                    if (line.Length != 0)
                    {
                        lines.Add("- " + line);
                    }

                    if (lines.Count >= 8)
                    {
                        break;
                    }
                }
                return string.Join("\n", lines);
            }

            return Text(releaseNotes).Trim();
        }

        public string FormatUpdatePackageSize(object sizeValue)
        {
            long size = NonNegativeLong(sizeValue);
            if (size <= 0)
            {
                return string.Empty;
            }

            double value = size;
            foreach (string unit in new[] { "B", "KB", "MB", "GB" })
            {
                if (value < 1024.0 || unit == "GB")
                {
                    return unit == "B"
                        ? ((long)value).ToString(CultureInfo.InvariantCulture) + " B"
                        : value.ToString("0.0", CultureInfo.InvariantCulture) + " " + unit;
                }

                value /= 1024.0;
            }

            return size.ToString(CultureInfo.InvariantCulture) + " B";
        }

        public IDictionary<string, object> FetchUpdateManifest(long unixTimestamp)
        {
            _callbacks.Log?.Invoke("获取更新清单开始");
            string baseUrl = GetManifestUrl();
            string requestUrl = baseUrl + "?t=" + unixTimestamp.ToString(CultureInfo.InvariantCulture);
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Cache-Control", "no-cache, no-store, max-age=0" },
                { "Pragma", "no-cache" },
                { "User-Agent", "SandboxBatchLauncher/" + LauncherConstants.AppVersion },
                { "Accept", "application/json" }
            };
            byte[] body = _httpClient.DownloadBytes(
                new Uri(requestUrl, UriKind.Absolute),
                8,
                true,
                headers);
            string text = new UTF8Encoding(false).GetString(body ?? new byte[0]).TrimStart('\uFEFF');
            IDictionary<string, object> manifest;
            try
            {
                manifest = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);
            }
            catch (Exception error)
            {
                throw new InvalidDataException("更新清单格式不正确", error);
            }

            if (manifest == null)
            {
                throw new InvalidDataException("更新清单格式不正确");
            }

            _callbacks.Log?.Invoke("更新清单下载与解析完成");
            manifest["_manifest_url"] = baseUrl;
            return manifest;
        }

        public UpdateCommandPreparation PrepareUpdaterCommand(
            UpdatePackageInfo packageInfo,
            string latestVersion)
        {
            if (!_isPackagedApp)
            {
                return Failure("自动更新只能在打包后的发布版中使用。");
            }

            string updaterPath = ResolveUpdaterExecutable();
            if (updaterPath.Length == 0)
            {
                return Failure("未找到主程序，自动更新无法继续。");
            }

            int processId = _callbacks.GetProcessId == null
                ? Process.GetCurrentProcess().Id
                : _callbacks.GetProcessId();
            long timestamp = _callbacks.GetUnixTimeSeconds == null
                ? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                : _callbacks.GetUnixTimeSeconds();
            string tempDirectory = CommonUtilities.GetUpdateTempDirectory(AppDirectory);
            string helperDirectory = Path.Combine(
                tempDirectory,
                "updater_helper_" + processId.ToString(CultureInfo.InvariantCulture) + "_" + timestamp.ToString(CultureInfo.InvariantCulture));

            try
            {
                _callbacks.Log?.Invoke("准备内置更新临时副本");
                Directory.CreateDirectory(helperDirectory);
                string temporaryUpdater = Path.Combine(helperDirectory, Path.GetFileName(updaterPath));
                File.Copy(updaterPath, temporaryUpdater, true);
                if (File.Exists(updaterPath + ".config"))
                {
                    File.Copy(updaterPath + ".config", temporaryUpdater + ".config", true);
                }
                foreach (string library in Directory.GetFiles(AppDirectory, "*.dll"))
                {
                    _callbacks.Log?.Invoke("复制更新依赖: " + Path.GetFileName(library));
                    File.Copy(library, Path.Combine(helperDirectory, Path.GetFileName(library)), true);
                }

                string internalSource = Path.Combine(AppDirectory, "internal");
                if (Directory.Exists(internalSource))
                {
                    CopyDirectory(internalSource, Path.Combine(helperDirectory, "internal"));
                }

                Directory.CreateDirectory(tempDirectory);
                string requestFile = Path.Combine(
                    tempDirectory,
                    "update_request_" + processId.ToString(CultureInfo.InvariantCulture) + "_" + timestamp.ToString(CultureInfo.InvariantCulture) + ".json");
                var request = new Dictionary<string, object>
                {
                    { "url", packageInfo == null ? string.Empty : packageInfo.Url },
                    { "package_kind", packageInfo == null ? string.Empty : packageInfo.Kind },
                    { "current_version", LauncherConstants.AppVersion },
                    { "target_version", Text(latestVersion).Trim() },
                    { "sha256", packageInfo == null ? string.Empty : packageInfo.Sha256 },
                    { "signature", packageInfo == null ? string.Empty : packageInfo.Signature },
                    { "encrypted", packageInfo != null && packageInfo.Encrypted },
                    { "size", (packageInfo == null ? 0L : packageInfo.Size).ToString(CultureInfo.InvariantCulture) },
                    { "restart", true },
                    { "download_workers", "4" }
                };
                _callbacks.Log?.Invoke("写入更新请求");
                File.WriteAllText(
                    requestFile,
                    JsonConvert.SerializeObject(request, Formatting.Indented),
                    new UTF8Encoding(false));

                return new UpdateCommandPreparation
                {
                    Command = new List<string>
                    {
                        temporaryUpdater,
                        "--install-update",
                        "--app-dir", AppDirectory,
                        "--exe", _currentExecutablePath,
                        "--pid", processId.ToString(CultureInfo.InvariantCulture),
                        "--request-file", requestFile
                    },
                    Error = string.Empty
                };
            }
            catch (Exception error)
            {
                _callbacks.Log?.Invoke("创建内置更新临时副本失败: " + error);
                return Failure("创建临时更新程序失败: " + error.Message);
            }
        }

        public string ResolveUpdaterExecutable()
        {
            string candidate = _currentExecutablePath;
            return File.Exists(candidate) ? Path.GetFullPath(candidate) : string.Empty;
        }

        public UpdateDisplayResult BuildUpdateDisplayResult(
            IDictionary<string, object> manifest,
            bool silentWhenLatest,
            bool inlineWhenUpdate)
        {
            string latestVersion = Text(GetValue(manifest, "latest_version", string.Empty));
            if (latestVersion.Length == 0)
                latestVersion = Text(GetValue(manifest, "version", string.Empty));
            latestVersion = latestVersion.Trim();
            var result = new UpdateDisplayResult
            {
                LatestVersion = latestVersion,
                ReleaseNotes = FormatUpdateNotes(GetValue(manifest, "release_notes", string.Empty)),
                IsManualCheck = !silentWhenLatest,
                MessageTitle = "检查更新"
            };
            if (latestVersion.Length == 0)
            {
                if (!silentWhenLatest) result.ErrorText = "更新清单缺少 latest_version";
                return result;
            }

            result.ForceUpdate = CommonUtilities.ParseBoolSetting(GetValue(manifest, "force_update", false), false);
            result.HasUpdate = CommonUtilities.CompareVersions(latestVersion, LauncherConstants.AppVersion) > 0;
            if (!result.HasUpdate)
            {
                if (silentWhenLatest)
                {
                    return result;
                }

                result.ShouldShowMessage = true;
                result.StatusText = "沙河批量打开器 已是最新版本: " + LauncherConstants.AppVersion;
                result.MessageText = "沙河批量打开器 已是最新版本。\n\n当前版本：" + LauncherConstants.AppVersion;
                return result;
            }

            result.Package = SelectUpdatePackage(manifest, latestVersion);
            if (result.Package == null)
            {
                result.ShouldShowMessage = true;
                result.StatusText = "发现新版本 " + latestVersion + "，但没有适用于当前版本的增量补丁";
                result.MessageText = "发现新版本：" + latestVersion + "\n但没有适用于当前版本的增量补丁。";
                return result;
            }

            result.ShouldPrompt = true;
            result.PromptText = "发现新版本" + latestVersion;
            result.StatusText = "发现新版本 " + latestVersion + "，可点击“立即更新”";
            result.ShouldShowNotice = true;
            return result;
        }

        public void CheckUpdateWorker(bool silentWhenLatest, bool inlineWhenUpdate)
        {
            Task.Run(delegate
            {
                try
                {
                    _callbacks.Log?.Invoke("检查更新开始: 当前版本=" + LauncherConstants.AppVersion);
                    long timestamp = _callbacks.GetUnixTimeSeconds == null
                        ? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        : _callbacks.GetUnixTimeSeconds();
                    IDictionary<string, object> manifest = FetchUpdateManifest(timestamp);
                    _callbacks.Log?.Invoke("检查更新成功");
                    UpdateDisplayResult display = BuildUpdateDisplayResult(manifest, silentWhenLatest, inlineWhenUpdate);
                    _callbacks.Log?.Invoke("版本比较完成: 最新版本=" + display.LatestVersion + "，发现更新=" + display.HasUpdate);
                    PostToUi(delegate
                    {
                        if (_callbacks.ShowUpdateResult != null) _callbacks.ShowUpdateResult(display);
                        else _callbacks.SetStatus?.Invoke(display.StatusText);
                    });
                }
                catch (Exception error)
                {
                    _callbacks.Log?.Invoke("检查更新失败: " + error);
                    if (!silentWhenLatest)
                    {
                        PostToUi(delegate
                        {
                            string status = "检查更新失败: " + error.Message;
                            _callbacks.SetStatus?.Invoke(status);
                            _callbacks.ShowUpdateResult?.Invoke(new UpdateDisplayResult
                            {
                                MessageTitle = "检查更新",
                                ErrorText = error.Message,
                                StatusText = status
                            });
                        });
                    }
                }
            });
        }

        public string ResolveQrImagePath(string filename)
        {
            string name = Text(filename).Trim();
            if (name.Length == 0)
            {
                return string.Empty;
            }

            foreach (string candidate in new[]
            {
                Path.Combine(AppDirectory, "打赏", name),
                Path.Combine(AppDirectory, name),
                Path.Combine(AppDirectory, "internal", name)
            })
            {
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }

            return string.Empty;
        }

        public byte[] GetEmbeddedQrBytes(string filename)
        {
            byte[] data = _callbacks.GetEmbeddedQrBytes == null
                ? null
                : _callbacks.GetEmbeddedQrBytes(Text(filename).Trim());
            return data == null ? new byte[0] : data.ToArray();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private string GetManifestUrl()
        {
            string value = _callbacks.GetManifestUrl == null ? string.Empty : _callbacks.GetManifestUrl();
            return Text(value).Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Text(value).Trim()
                : LauncherConstants.UpdateManifestUrl;
        }

        private void PostToUi(Action action)
        {
            if (_callbacks.PostToUi != null)
            {
                _callbacks.PostToUi(action);
                return;
            }

            action();
        }

        private static UpdateCommandPreparation Failure(string error)
        {
            return new UpdateCommandPreparation { Command = null, Error = error };
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }

            foreach (string directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
            }
        }

        private static long NonNegativeLong(object value)
        {
            long number;
            return long.TryParse(Text(value).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
                ? Math.Max(0L, number)
                : 0L;
        }

        private static IDictionary<string, object> CopyDictionary(object value)
        {
            if (value == null)
            {
                return null;
            }

            IDictionary<string, object> generic = value as IDictionary<string, object>;
            if (generic != null)
            {
                return new Dictionary<string, object>(generic, StringComparer.Ordinal);
            }

            JObject json = value as JObject;
            if (json != null)
            {
                return json.ToObject<Dictionary<string, object>>();
            }

            IDictionary dictionary = value as IDictionary;
            if (dictionary == null)
            {
                return null;
            }

            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in dictionary)
            {
                result[Text(entry.Key)] = entry.Value;
            }
            return result;
        }

        private static IList<object> ToObjectList(object value)
        {
            if (value == null)
            {
                return new List<object>();
            }

            JArray json = value as JArray;
            if (json != null)
            {
                return json.Cast<JToken>().Select(item => (object)item).ToList();
            }

            IEnumerable enumerable = value as IEnumerable;
            return enumerable == null
                ? new List<object>()
                : enumerable.Cast<object>().ToList();
        }

        private static object GetValue(IDictionary<string, object> values, string key, object defaultValue)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? Unwrap(value) : defaultValue;
        }

        private static object Unwrap(object value)
        {
            JValue json = value as JValue;
            return json == null ? value : json.Value;
        }

        private static string Text(object value)
        {
            object raw = Unwrap(value);
            return raw == null ? string.Empty : Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string FullPath(string path)
        {
            string value = Text(path).Trim();
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
    }
}
