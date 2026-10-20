param(
    [string]$LibDirectory = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\bin\x64\Release\net48\lib",
    [string]$Text = "沙河批量打开器朗读测试",
    [string]$ResultPath = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\tolk-native-probe-result.json",
    [string]$LogPath = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\tolk-native-probe.log"
)

$source = @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

public sealed class TolkProbeResult
{
    public string DllPath { get; set; }
    public bool Is64BitProcess { get; set; }
    public bool IsLoaded { get; set; }
    public string DetectedScreenReader { get; set; }
    public bool OutputResult { get; set; }
    public bool Unloaded { get; set; }
    public bool Freed { get; set; }
}

public static class TolkNativeProbe
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void BoolSetter([MarshalAs(UnmanagedType.I1)] bool value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void VoidCall();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool BoolCall();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private delegate IntPtr DetectCall();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool OutputCall(
        [MarshalAs(UnmanagedType.LPWStr)] string text,
        [MarshalAs(UnmanagedType.I1)] bool interrupt);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string path);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr module, string name);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr module);

    public static TolkProbeResult Run(string dllPath, string text)
    {
        IntPtr module = LoadLibrary(dllPath);
        if (module == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "LoadLibrary failed");
        }

        VoidCall unload = null;
        bool unloaded = false;
        try
        {
            BoolSetter trySapi = GetDelegate<BoolSetter>(module, "Tolk_TrySAPI");
            BoolSetter preferSapi = GetDelegate<BoolSetter>(module, "Tolk_PreferSAPI");
            VoidCall load = GetDelegate<VoidCall>(module, "Tolk_Load");
            BoolCall isLoaded = GetDelegate<BoolCall>(module, "Tolk_IsLoaded");
            DetectCall detect = GetDelegate<DetectCall>(module, "Tolk_DetectScreenReader");
            OutputCall output = GetDelegate<OutputCall>(module, "Tolk_Output");
            unload = GetDelegate<VoidCall>(module, "Tolk_Unload");

            trySapi(true);
            preferSapi(false);
            load();
            bool loaded = isLoaded();
            IntPtr detected = loaded ? detect() : IntPtr.Zero;
            string reader = detected == IntPtr.Zero
                ? string.Empty
                : Marshal.PtrToStringUni(detected) ?? string.Empty;
            bool outputResult = loaded && output(text, true);
            unload();
            unloaded = true;
            bool freed = FreeLibrary(module);
            module = IntPtr.Zero;
            return new TolkProbeResult
            {
                DllPath = dllPath,
                Is64BitProcess = Environment.Is64BitProcess,
                IsLoaded = loaded,
                DetectedScreenReader = reader,
                OutputResult = outputResult,
                Unloaded = unloaded,
                Freed = freed
            };
        }
        finally
        {
            if (module != IntPtr.Zero)
            {
                if (!unloaded && unload != null)
                {
                    try { unload(); } catch { }
                }
                FreeLibrary(module);
            }
        }
    }

    private static T GetDelegate<T>(IntPtr module, string name) where T : class
    {
        IntPtr address = GetProcAddress(module, name);
        if (address == IntPtr.Zero)
        {
            throw new MissingMethodException(name);
        }
        return (T)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
    }
}
'@

$result = $null
$errorText = ""
$returnCode = 1
try {
    Add-Type -TypeDefinition $source -Language CSharp
    $dllPath = Join-Path $LibDirectory "Tolk.dll"
    $result = [TolkNativeProbe]::Run($dllPath, $Text)
    if ($result.Is64BitProcess -and $result.IsLoaded -and $result.OutputResult -and $result.Unloaded -and $result.Freed) {
        $returnCode = 0
    }
}
catch {
    $errorText = $_.Exception.ToString()
}

$record = [ordered]@{
    command = "tolk-native-probe.ps1"
    returnCode = $returnCode
    libDirectory = $LibDirectory
    text = $Text
    is64BitProcess = if ($result) { $result.Is64BitProcess } else { [Environment]::Is64BitProcess }
    isLoaded = if ($result) { $result.IsLoaded } else { $false }
    detectedScreenReader = if ($result) { $result.DetectedScreenReader } else { "" }
    outputResult = if ($result) { $result.OutputResult } else { $false }
    unloaded = if ($result) { $result.Unloaded } else { $false }
    freed = if ($result) { $result.Freed } else { $false }
    error = $errorText
}
$json = $record | ConvertTo-Json -Depth 4
$json | Set-Content -LiteralPath $ResultPath -Encoding utf8
@(
    "returnCode: $returnCode",
    "libDirectory: $LibDirectory",
    "is64BitProcess: $($record.is64BitProcess)",
    "isLoaded: $($record.isLoaded)",
    "detectedScreenReader: $($record.detectedScreenReader)",
    "outputResult: $($record.outputResult)",
    "unloaded: $($record.unloaded)",
    "freed: $($record.freed)",
    "error: $errorText"
) | Set-Content -LiteralPath $LogPath -Encoding utf8
$json
exit $returnCode
