using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

internal static class GitCodeUpgradeProbe
{
    delegate bool EnumProc(IntPtr handle, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr handle, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr handle, StringBuilder text, int length);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr handle);
    [DllImport("user32.dll")] static extern bool IsWindowEnabled(IntPtr handle);
    [DllImport("user32.dll")] static extern IntPtr GetMenu(IntPtr handle);
    [DllImport("user32.dll")] static extern IntPtr GetSubMenu(IntPtr handle, int index);
    [DllImport("user32.dll")] static extern int GetMenuItemCount(IntPtr menu);
    [DllImport("user32.dll")] static extern uint GetMenuItemID(IntPtr menu, int index);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetMenuString(IntPtr menu, uint index, StringBuilder text, int count, uint flags);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr handle, int message, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] static extern IntPtr GetDlgItem(IntPtr handle, int id);
    [DllImport("oleacc.dll")] static extern int AccessibleObjectFromWindow(IntPtr handle, uint objectId, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out Accessibility.IAccessible accessible);
    static void InvokeButton(IntPtr handle)
    {
        Guid iid = typeof(Accessibility.IAccessible).GUID; Accessibility.IAccessible accessible;
        int code = AccessibleObjectFromWindow(handle, 0xFFFFFFFC, ref iid, out accessible);
        Require(code == 0 && accessible != null, "Button accessibility unavailable");
        try { accessible.accDoDefaultAction(0); } finally { Marshal.ReleaseComObject(accessible); }
    }
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
    static string App, Output, Doc; static int TargetPid;
    static string Text(IntPtr handle) { var text = new StringBuilder(4096); GetWindowText(handle, text, text.Capacity); return text.ToString(); }
    static string Contents(IntPtr handle) { var text = new StringBuilder(Text(handle)); EnumChildWindows(handle, (child, data) => { text.AppendLine(Text(child)); return true; }, IntPtr.Zero); return text.ToString(); }
    static IntPtr Window(string title) { IntPtr found = IntPtr.Zero; EnumWindows((handle, data) => { uint pid; GetWindowThreadProcessId(handle, out pid); if (pid == TargetPid && IsWindowVisible(handle) && Text(handle) == title) found = handle; return true; }, IntPtr.Zero); return found; }
    static IntPtr WaitWindow(string title, int milliseconds) { var watch = Stopwatch.StartNew(); do { var window = Window(title); if (window != IntPtr.Zero) return window; Thread.Sleep(80); } while (watch.ElapsedMilliseconds < milliseconds); throw new InvalidOperationException("Window missing: " + title); }
    static int FindMenu(IntPtr menu, string label) { for (int i = 0; i < GetMenuItemCount(menu); i++) { var text = new StringBuilder(256); GetMenuString(menu, (uint)i, text, text.Capacity, 0x400); if (text.ToString().Split('\t')[0] == label) return (int)GetMenuItemID(menu, i); var sub = GetSubMenu(menu, i); if (sub != IntPtr.Zero) { int command = FindMenu(sub, label); if (command >= 0) return command; } } return -1; }
    static void Menu(IntPtr main, string label) { if (!IsWindowEnabled(main)) throw new InvalidOperationException("Main window disabled"); int command = FindMenu(GetMenu(main), label); if (command < 0 || !PostMessage(main, 0x111, new IntPtr(command), IntPtr.Zero)) throw new InvalidOperationException("Menu not available: " + label); }
    static string Hash(string path) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static void Record(string step, object detail) { File.WriteAllText(Path.Combine(Output, step + ".json"), Json.Serialize(new { step, time = DateTimeOffset.Now.ToString("o"), pid = TargetPid, executable = App, detail }), new UTF8Encoding(false)); File.AppendAllText(Doc, "\n- UPDATE-DLL-01 GitCode 真实升级步骤 " + step + " 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/" + Path.GetFileName(Output) + "/" + step + ".json，完整结论以 final-result.json 为准。\n", new UTF8Encoding(false)); }
    static Process FindOwned(string name, string exactPath, string prefix) { foreach (var process in Process.GetProcessesByName(name)) { try { string path = process.MainModule.FileName; if ((exactPath != null && string.Equals(path, exactPath, StringComparison.OrdinalIgnoreCase)) || (prefix != null && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))) { var handle = process.Handle; return process; } } catch { } process.Dispose(); } return null; }
    static bool Cleanup(Process process) { if (process == null || process.HasExited) return false; process.CloseMainWindow(); if (process.WaitForExit(2500)) return false; process.Kill(); process.WaitForExit(2000); return true; }
    [STAThread] static int Main(string[] args)
    {
        App = Path.GetFullPath(args[0]); Output = Path.GetFullPath(args[1]); Doc = Path.GetFullPath(args[2]);
        Require(App.IndexOf(@"\artifacts\runs\updater_added_dll_20261023\old-app\", StringComparison.OrdinalIgnoreCase) >= 0, "Unexpected fixture path");
        Directory.CreateDirectory(Output); string appDir = Path.GetDirectoryName(App);
        var result = new Dictionary<string, object>(); Process old = null, helper = null, restarted = null; int code = 3; bool forced = false;
        try
        {
            Require(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(App)).Length == 0, "Another main application is already running; leave it untouched");
            Require(!Directory.Exists(Path.Combine(appDir, "打赏")), "Old fixture already contains donation resources");
            Require(!File.Exists(Path.Combine(appDir, "lib", "byctrl-x64.dll")), "Optional new-DLL fixture already present");
            old = Process.Start(new ProcessStartInfo(App) { WorkingDirectory = appDir, UseShellExecute = false }); TargetPid = old.Id; old.WaitForInputIdle(8000);
            var main = WaitWindow("沙河批量打开器", 10000); Thread.Sleep(1500);
            string oldConfig = Path.Combine(appDir, "Configuration", "shahe_config.ini"); string configHash = Hash(oldConfig); string oldCoreHash = Hash(Path.Combine(appDir, "ShaheLauncher.Core.dll"));
            Record("old-startup", new { main = main.ToInt64(), configHash, oldCoreHash, missingOptionalDll = true, missingDonation = true });
            IntPtr downloadButton = IntPtr.Zero; var detection = Stopwatch.StartNew();
            do
            {
                EnumChildWindows(main, (handle, data) => { if (Text(handle) == "下载更新" && IsWindowVisible(handle) && IsWindowEnabled(handle)) downloadButton = handle; return true; }, IntPtr.Zero);
                if (downloadButton != IntPtr.Zero) break; Thread.Sleep(80);
            } while (detection.ElapsedMilliseconds < 15000);
            string notice = Contents(main);
            Require(downloadButton != IntPtr.Zero && notice.Contains("4.0.47"), "Startup update notice/button missing: " + notice);
            Record("update-detected", new { method = "production startup check and download button", text = notice, button = downloadButton.ToInt64() });
            InvokeButton(downloadButton);
            var installConfirmation = WaitWindow("下载更新", 5000);
            string confirmationText = Contents(installConfirmation);
            Require(confirmationText.Contains("4.0.47") && confirmationText.Contains("是否下载并安装更新"), "Unexpected old-version installation confirmation");
            Record("install-confirmation", new { text = confirmationText });
            InvokeButton(GetDlgItem(installConfirmation, 6));
            var watch = Stopwatch.StartNew(); string helperPrefix = Path.Combine(appDir, "tenp") + Path.DirectorySeparatorChar;
            do { if (helper == null) helper = FindOwned("updater", null, helperPrefix); if (old.HasExited && helper != null) break; Thread.Sleep(30); } while (watch.ElapsedMilliseconds < 15000);
            Require(old.HasExited, "Old main did not exit"); Require(old.ExitCode == 0, "Old main failed to exit normally"); Require(helper != null, "Production updater was not observed");
            result["oldPid"] = old.Id; result["oldExitCode"] = old.ExitCode; result["updaterPid"] = helper.Id;
            Record("handoff", new { oldPid = old.Id, oldExitCode = old.ExitCode, updaterPid = helper.Id, helperDirectory = helperPrefix });
            Require(helper.WaitForExit(45000), "Updater timed out"); result["updaterExitCode"] = helper.ExitCode; Require(helper.ExitCode == 0, "Updater returned failure");
            watch.Restart(); do { restarted = FindOwned(Path.GetFileNameWithoutExtension(App), App, null); if (restarted != null) break; Thread.Sleep(80); } while (watch.ElapsedMilliseconds < 12000);
            Require(restarted != null && restarted.Id != old.Id, "Updated main restart was not observed"); TargetPid = restarted.Id; restarted.WaitForInputIdle(8000); main = WaitWindow("沙河批量打开器", 10000); Thread.Sleep(1000);
            string newConfig = Path.Combine(appDir, "Configurations", "shahe_config.ini");
            Require(File.Exists(newConfig) && Hash(newConfig) == configHash, "User configuration changed during folder migration");
            Require(!Directory.Exists(Path.Combine(appDir, "Configuration")), "Previous configuration folder was not migrated");
            Require(Hash(Path.Combine(appDir, "ShaheLauncher.Core.dll")) != oldCoreHash, "Core DLL was not replaced");
            var expected = Json.Deserialize<Dictionary<string, string>>(File.ReadAllText(args[3])); var installed = new Dictionary<string, string>();
            foreach (var item in expected) { string actual = Hash(Path.Combine(appDir, item.Key)); Require(actual == item.Value, "Payload mismatch: " + item.Key); installed[item.Key] = actual; }
            Record("installed", new { newPid = restarted.Id, main = main.ToInt64(), configurationPreserved = true, configurationDirectory = Path.GetDirectoryName(newConfig), files = installed });
            Menu(main, "检查更新"); var latest = WaitWindow("检查更新", 15000); string latestText = Contents(latest);
            Require(latestText.Contains("已是最新版本") && latestText.Contains("当前版本：4.0.47"), "Installed version verification failed: " + latestText);
            Record("latest-version", new { text = latestText }); InvokeButton(GetDlgItem(latest, 1)); Thread.Sleep(200);
            foreach (string title in new[] { "支付宝打赏", "微信打赏" })
            {
                Menu(main, title); var dialog = WaitWindow(title, 5000); string controls = Contents(dialog);
                Require(controls.Contains("复制二维码") && controls.Contains("关闭"), "Donation image did not finish loading: " + controls);
                Record(title.StartsWith("支付宝") ? "alipay-image" : "wechat-image", new { title, window = dialog.ToInt64(), imageLoaded = true, controls });
                PostMessage(dialog, 0x10, IntPtr.Zero, IntPtr.Zero); Thread.Sleep(200); Require(IsWindowEnabled(main), "Donation dialog did not return to main");
            }
            restarted.CloseMainWindow(); Require(restarted.WaitForExit(5000) && restarted.ExitCode == 0, "Updated application did not exit normally");
            result["newPid"] = restarted.Id; result["newExitCode"] = restarted.ExitCode; result["configurationPreserved"] = true; result["donationImagesLoaded"] = true; result["newDllInstalled"] = true; result["payloadHashesMatch"] = true; code = 0;
        }
        catch (Exception error) { result["error"] = error.ToString(); var windows = new List<string>(); EnumWindows((handle, data) => { uint owner; GetWindowThreadProcessId(handle, out owner); if (owner == TargetPid && IsWindowVisible(handle)) windows.Add(Contents(handle)); return true; }, IntPtr.Zero); result["windowsAtFailure"] = windows; }
        finally { forced |= Cleanup(restarted); forced |= Cleanup(old); forced |= Cleanup(helper); }
        if (forced) code = 3;
        result["returnCode"] = code; result["passed"] = code == 0 ? 1 : 0; result["failed"] = 0; result["inconclusive"] = code == 0 ? 0 : 1; result["forcedCleanup"] = forced;
        File.WriteAllText(Path.Combine(Output, "final-result.json"), Json.Serialize(result), new UTF8Encoding(false));
        File.AppendAllText(Doc, "\n- UPDATE-DLL-01 GitCode 真实升级完整场景返回码 " + code + "；结果 artifacts/runs/updater_added_dll_20261023/" + Path.GetFileName(Output) + "/final-result.json。\n", new UTF8Encoding(false));
        return code;
    }
}
