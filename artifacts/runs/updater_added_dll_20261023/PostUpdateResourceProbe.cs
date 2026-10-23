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

internal static class PostUpdateResourceProbe
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
        App=Path.GetFullPath(args[0]); Output=Path.GetFullPath(args[1]); Doc=Path.GetFullPath(args[2]);
        Require(App.IndexOf(@"\artifacts\runs\updater_added_dll_20261023\old-app\",StringComparison.OrdinalIgnoreCase)>=0,"Unexpected path");
        Directory.CreateDirectory(Output); string appDir=Path.GetDirectoryName(App); Process app=null; int code=3; bool forced=false; var result=new Dictionary<string,object>();
        try {
            Require(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(App)).Length==0,"Existing main process must remain untouched");
            string config=Path.Combine(appDir,"Configurations","shahe_config.ini"), before=Hash(config);
            app=Process.Start(new ProcessStartInfo(App){WorkingDirectory=appDir,UseShellExecute=false});TargetPid=app.Id;app.WaitForInputIdle(8000);var main=WaitWindow("沙河批量打开器",10000);Thread.Sleep(1800);
            Record("installed-startup",new{main=main.ToInt64(),configHash=before});
            foreach(string title in new[]{"支付宝打赏","微信打赏"}) {
                Menu(main,title);var dialog=WaitWindow(title,5000);string controls=Contents(dialog);
                Require(controls.Contains("复制二维码")&&controls.Contains("关闭"),"Image failed to load: "+controls);
                Record(title.StartsWith("支付宝")?"alipay-image":"wechat-image",new{title,window=dialog.ToInt64(),imageLoaded=true,controls});
                PostMessage(dialog,0x10,IntPtr.Zero,IntPtr.Zero);Thread.Sleep(250);Require(IsWindowEnabled(main),"Donation dialog did not return");
            }
            Require(Hash(config)==before,"Configuration changed during resource validation");app.CloseMainWindow();Require(app.WaitForExit(5000)&&app.ExitCode==0,"Normal exit failed");
            result["pid"]=app.Id;result["exitCode"]=app.ExitCode;result["donationImagesLoaded"]=true;result["configurationPreserved"]=true;code=0;
        } catch(Exception error){result["error"]=error.ToString();}
        finally { forced=Cleanup(app); }
        if(forced)code=3; result["returnCode"]=code;result["passed"]=code==0?1:0;result["failed"]=0;result["inconclusive"]=code==0?0:1;result["forcedCleanup"]=forced;
        File.WriteAllText(Path.Combine(Output,"final-result.json"),Json.Serialize(result),new UTF8Encoding(false));
        File.AppendAllText(Doc,"\n- UPDATE-DLL-01 已升级副本打赏/正常退出验证返回码 "+code+"；结果 artifacts/runs/updater_added_dll_20261023/"+Path.GetFileName(Output)+"/final-result.json。未重复安装更新。\n",new UTF8Encoding(false));return code;
    }
}
