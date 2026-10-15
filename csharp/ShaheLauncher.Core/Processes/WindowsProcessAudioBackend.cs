using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShaheLauncher.Core.Processes
{
    public sealed class WindowsProcessAudioBackend
    {
        private const int ClsContextAll = 23;
        private const int DeviceStateActive = 1;
        private const int RenderFlow = 0;
        private const int ConsoleRole = 0;
        private const int MultimediaRole = 1;
        private const int MinimumRoutingBuild = 17134;
        private const int PolicyConfig21H2Build = 21390;
        private const int SetPersistedDefaultEndpointVtableIndex = 25;
        private const int RpcChangedMode = unchecked((int)0x80010106);
        private const string PolicyConfigClassId = "Windows.Media.Internal.AudioPolicyConfig";
        private const string MmDeviceApiToken = @"\\?\SWD#MMDEVAPI#";
        private const string RenderInterfaceId = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";

        private static readonly Guid AudioSessionManager2Id =
            new Guid("77aa99a0-1bd6-484f-8bc7-2c654c9a9b6f");
        private static readonly Guid FriendlyNameFormatId =
            new Guid("a45c254e-df1c-4efd-8020-67d146a850e0");
        private static readonly Guid PolicyConfigDownlevelId =
            new Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258");
        private static readonly Guid PolicyConfig21H2Id =
            new Guid("ab3d4648-e242-459f-b02f-541c70306324");

        public IEnumerable<ProcessAudioSession> GetAudioSessions()
        {
            var result = new List<ProcessAudioSession>();
            if (!IsWindows())
            {
                return result;
            }

            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnumerator = null;
            try
            {
                using (ComInitialization.Enter())
                {
                    deviceEnumerator = (IMMDeviceEnumerator)Activator.CreateInstance(
                        typeof(MmDeviceEnumeratorComObject));
                    ThrowIfFailed(deviceEnumerator.GetDefaultAudioEndpoint(
                        RenderFlow,
                        MultimediaRole,
                        out device));

                    object managerObject;
                    Guid managerId = AudioSessionManager2Id;
                    ThrowIfFailed(device.Activate(
                        ref managerId,
                        ClsContextAll,
                        IntPtr.Zero,
                        out managerObject));
                    manager = (IAudioSessionManager2)managerObject;
                    ThrowIfFailed(manager.GetSessionEnumerator(out sessionEnumerator));

                    int count;
                    ThrowIfFailed(sessionEnumerator.GetCount(out count));
                    for (int index = 0; index < count; index++)
                    {
                        IAudioSessionControl control = null;
                        bool keepControlAlive = false;
                        try
                        {
                            ThrowIfFailed(sessionEnumerator.GetSession(index, out control));
                            var control2 = (IAudioSessionControl2)control;
                            uint rawPid;
                            ThrowIfFailed(control2.GetProcessId(out rawPid));
                            if (rawPid == 0 || rawPid > int.MaxValue || !IsProcessAlive(unchecked((int)rawPid)))
                            {
                                continue;
                            }

                            var volume = (ISimpleAudioVolume)control;
                            var handle = new SimpleAudioVolumeHandle(volume);
                            result.Add(CreateSession(
                                unchecked((int)rawPid),
                                handle.GetMute,
                                handle.SetMute,
                                handle.GetMasterVolume,
                                handle.SetMasterVolume));
                            keepControlAlive = true;
                        }
                        catch
                        {
                        }
                        finally
                        {
                            if (!keepControlAlive)
                            {
                                ReleaseComObject(control);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                ReleaseComObject(sessionEnumerator);
                ReleaseComObject(manager);
                ReleaseComObject(device);
                ReleaseComObject(deviceEnumerator);
            }

            return result;
        }

        public IEnumerable<ProcessAudioDevice> ListOutputDevices()
        {
            var result = new List<ProcessAudioDevice>();
            if (!IsWindows())
            {
                return result;
            }

            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDeviceCollection collection = null;
            try
            {
                using (ComInitialization.Enter())
                {
                    deviceEnumerator = (IMMDeviceEnumerator)Activator.CreateInstance(
                        typeof(MmDeviceEnumeratorComObject));
                    string defaultDeviceId = GetDefaultDeviceId(deviceEnumerator);
                    ThrowIfFailed(deviceEnumerator.EnumAudioEndpoints(
                        RenderFlow,
                        DeviceStateActive,
                        out collection));

                    uint count;
                    ThrowIfFailed(collection.GetCount(out count));
                    for (uint index = 0; index < count; index++)
                    {
                        IMMDevice device = null;
                        IPropertyStore properties = null;
                        try
                        {
                            ThrowIfFailed(collection.Item(index, out device));
                            string id;
                            ThrowIfFailed(device.GetId(out id));
                            if (string.IsNullOrWhiteSpace(id))
                            {
                                continue;
                            }

                            string name = string.Empty;
                            if (device.OpenPropertyStore(0, out properties) >= 0)
                            {
                                name = ReadFriendlyName(properties);
                            }
                            result.Add(CreateDevice(
                                id,
                                name,
                                string.Equals(id, defaultDeviceId, StringComparison.OrdinalIgnoreCase)));
                        }
                        catch
                        {
                        }
                        finally
                        {
                            ReleaseComObject(properties);
                            ReleaseComObject(device);
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                ReleaseComObject(collection);
                ReleaseComObject(deviceEnumerator);
            }

            return result;
        }

        public bool SetOutputDevice(int processId, string deviceId)
        {
            string id = (deviceId ?? string.Empty).Trim();
            if (!IsWindows() || processId <= 0 || id.Length == 0 || !IsProcessAlive(processId))
            {
                return false;
            }

            Guid policyConfigId = ResolvePolicyConfigInterfaceId(GetWindowsBuildForRouting());
            if (policyConfigId == Guid.Empty)
            {
                return false;
            }

            IntPtr classId = IntPtr.Zero;
            IntPtr packedDeviceId = IntPtr.Zero;
            IntPtr factory = IntPtr.Zero;
            try
            {
                using (ComInitialization.Enter())
                {
                    ThrowIfFailed(WindowsCreateString(
                        PolicyConfigClassId,
                        unchecked((uint)PolicyConfigClassId.Length),
                        out classId));
                    ThrowIfFailed(RoGetActivationFactory(classId, ref policyConfigId, out factory));

                    string packedId = PackRenderDeviceId(id);
                    ThrowIfFailed(WindowsCreateString(
                        packedId,
                        unchecked((uint)packedId.Length),
                        out packedDeviceId));
                    IntPtr vtable = Marshal.ReadIntPtr(factory);
                    IntPtr methodPointer = Marshal.ReadIntPtr(
                        vtable,
                        IntPtr.Size * SetPersistedDefaultEndpointVtableIndex);
                    var setEndpoint = (SetPersistedDefaultEndpointDelegate)
                        Marshal.GetDelegateForFunctionPointer(
                            methodPointer,
                            typeof(SetPersistedDefaultEndpointDelegate));

                    ThrowIfFailed(setEndpoint(
                        factory,
                        unchecked((uint)processId),
                        RenderFlow,
                        ConsoleRole,
                        packedDeviceId));
                    ThrowIfFailed(setEndpoint(
                        factory,
                        unchecked((uint)processId),
                        RenderFlow,
                        MultimediaRole,
                        packedDeviceId));
                    return true;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (packedDeviceId != IntPtr.Zero)
                {
                    WindowsDeleteString(packedDeviceId);
                }
                if (classId != IntPtr.Zero)
                {
                    WindowsDeleteString(classId);
                }
                if (factory != IntPtr.Zero)
                {
                    Marshal.Release(factory);
                }
            }
        }

        internal static ProcessAudioSession CreateSession(
            int pid,
            Func<bool> readMute,
            Action<bool> writeMute,
            Func<double> readMasterVolume,
            Action<double> writeMasterVolume)
        {
            return new ProcessAudioSession
            {
                Pid = pid,
                HasVolume = readMute != null
                    && writeMute != null
                    && readMasterVolume != null
                    && writeMasterVolume != null,
                IsMuted = readMute != null && readMute(),
                MasterVolume = readMasterVolume != null ? readMasterVolume() : 0.0,
                ReadMute = readMute,
                WriteMute = writeMute,
                ReadMasterVolume = readMasterVolume,
                WriteMasterVolume = writeMasterVolume
            };
        }

        internal static ProcessAudioDevice CreateDevice(string deviceId, string deviceName, bool isDefault)
        {
            string id = (deviceId ?? string.Empty).Trim();
            string name = (deviceName ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                name = id;
            }
            return new ProcessAudioDevice
            {
                Id = id,
                Name = name,
                Label = ProcessActionService.FormatOutputDeviceLabel(name, id, isDefault),
                IsDefault = isDefault
            };
        }

        internal static string PackRenderDeviceId(string deviceId)
        {
            string id = (deviceId ?? string.Empty).Trim();
            return id.Length == 0 ? string.Empty : MmDeviceApiToken + id + RenderInterfaceId;
        }

        internal static Guid ResolvePolicyConfigInterfaceId(int windowsBuild)
        {
            if (windowsBuild < MinimumRoutingBuild)
            {
                return Guid.Empty;
            }
            return windowsBuild >= PolicyConfig21H2Build
                ? PolicyConfig21H2Id
                : PolicyConfigDownlevelId;
        }

        internal static int GetWindowsBuildForRouting()
        {
            if (!IsWindows())
            {
                return 0;
            }

            var version = new WindowsVersionInfo
            {
                Size = Marshal.SizeOf(typeof(WindowsVersionInfo))
            };
            return RtlGetVersion(ref version) == 0
                ? version.BuildNumber
                : Environment.OSVersion.Version.Build;
        }

        private static string GetDefaultDeviceId(IMMDeviceEnumerator deviceEnumerator)
        {
            IMMDevice device = null;
            try
            {
                if (deviceEnumerator.GetDefaultAudioEndpoint(
                    RenderFlow,
                    MultimediaRole,
                    out device) < 0 || device == null)
                {
                    return string.Empty;
                }
                string id;
                return device.GetId(out id) >= 0 ? id ?? string.Empty : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                ReleaseComObject(device);
            }
        }

        private static string ReadFriendlyName(IPropertyStore properties)
        {
            var key = new PropertyKey
            {
                FormatId = FriendlyNameFormatId,
                PropertyId = 14
            };
            PropVariant value;
            if (properties.GetValue(ref key, out value) < 0)
            {
                return string.Empty;
            }
            try
            {
                if ((value.VarType == 31 || value.VarType == 8) && value.PointerValue != IntPtr.Zero)
                {
                    return Marshal.PtrToStringUni(value.PointerValue) ?? string.Empty;
                }
                return string.Empty;
            }
            finally
            {
                PropVariantClear(ref value);
            }
        }

        private static bool IsProcessAlive(int processId)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return !process.HasExited;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsWindows()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            return platform == PlatformID.Win32NT;
        }

        private static void ThrowIfFailed(int hresult)
        {
            if (hresult < 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        private static void ReleaseComObject(object value)
        {
            if (value == null || !Marshal.IsComObject(value))
            {
                return;
            }
            try
            {
                Marshal.FinalReleaseComObject(value);
            }
            catch
            {
            }
        }

        private sealed class SimpleAudioVolumeHandle
        {
            private readonly ISimpleAudioVolume _volume;

            public SimpleAudioVolumeHandle(ISimpleAudioVolume volume)
            {
                _volume = volume ?? throw new ArgumentNullException(nameof(volume));
            }

            public bool GetMute()
            {
                using (ComInitialization.Enter())
                {
                    bool value;
                    ThrowIfFailed(_volume.GetMute(out value));
                    return value;
                }
            }

            public void SetMute(bool value)
            {
                using (ComInitialization.Enter())
                {
                    Guid context = Guid.Empty;
                    ThrowIfFailed(_volume.SetMute(value, ref context));
                }
            }

            public double GetMasterVolume()
            {
                using (ComInitialization.Enter())
                {
                    float value;
                    ThrowIfFailed(_volume.GetMasterVolume(out value));
                    return value;
                }
            }

            public void SetMasterVolume(double value)
            {
                using (ComInitialization.Enter())
                {
                    Guid context = Guid.Empty;
                    float level = unchecked((float)Math.Max(0.0, Math.Min(1.0, value)));
                    ThrowIfFailed(_volume.SetMasterVolume(level, ref context));
                }
            }
        }

        private sealed class ComInitialization : IDisposable
        {
            private readonly bool _uninitialize;

            private ComInitialization(bool uninitialize)
            {
                _uninitialize = uninitialize;
            }

            public static ComInitialization Enter()
            {
                int result = CoInitializeEx(IntPtr.Zero, 0);
                if (result < 0 && result != RpcChangedMode)
                {
                    Marshal.ThrowExceptionForHR(result);
                }
                return new ComInitialization(result >= 0);
            }

            public void Dispose()
            {
                if (_uninitialize)
                {
                    CoUninitialize();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PropertyKey
        {
            public Guid FormatId;
            public int PropertyId;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PropVariant
        {
            [FieldOffset(0)]
            public ushort VarType;

            [FieldOffset(8)]
            public IntPtr PointerValue;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WindowsVersionInfo
        {
            public int Size;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string ServicePack;

            public ushort ServicePackMajor;
            public ushort ServicePackMinor;
            public ushort SuiteMask;
            public byte ProductType;
            public byte Reserved;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetPersistedDefaultEndpointDelegate(
            IntPtr factory,
            uint processId,
            int flow,
            int role,
            IntPtr deviceId);

        [ComImport]
        [Guid("bcde0395-e52f-467c-8e3d-c4579291692e")]
        private sealed class MmDeviceEnumeratorComObject
        {
        }

        [ComImport]
        [Guid("a95664d2-9614-4f35-a746-de8db63617e6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            [PreserveSig]
            int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);

            [PreserveSig]
            int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice device);

            [PreserveSig]
            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);

            [PreserveSig]
            int RegisterEndpointNotificationCallback(IntPtr client);

            [PreserveSig]
            int UnregisterEndpointNotificationCallback(IntPtr client);
        }

        [ComImport]
        [Guid("0bd7a1be-7a1a-44db-8397-cc5392387b5e")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
            [PreserveSig]
            int GetCount(out uint count);

            [PreserveSig]
            int Item(uint index, out IMMDevice device);
        }

        [ComImport]
        [Guid("d666063f-1587-4e43-81f1-b948e807363f")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate(
                ref Guid interfaceId,
                int classContext,
                IntPtr activationParameters,
                [MarshalAs(UnmanagedType.IUnknown)] out object instance);

            [PreserveSig]
            int OpenPropertyStore(int accessMode, out IPropertyStore properties);

            [PreserveSig]
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

            [PreserveSig]
            int GetState(out int state);
        }

        [ComImport]
        [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            [PreserveSig]
            int GetCount(out uint count);

            [PreserveSig]
            int GetAt(uint index, out PropertyKey key);

            [PreserveSig]
            int GetValue(ref PropertyKey key, out PropVariant value);

            [PreserveSig]
            int SetValue(ref PropertyKey key, ref PropVariant value);

            [PreserveSig]
            int Commit();
        }

        [ComImport]
        [Guid("77aa99a0-1bd6-484f-8bc7-2c654c9a9b6f")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager2
        {
            [PreserveSig]
            int GetAudioSessionControl(ref Guid sessionId, int streamFlags, out IAudioSessionControl control);

            [PreserveSig]
            int GetSimpleAudioVolume(ref Guid sessionId, int streamFlags, out ISimpleAudioVolume volume);

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator sessionEnumerator);

            [PreserveSig]
            int RegisterSessionNotification(IntPtr notification);

            [PreserveSig]
            int UnregisterSessionNotification(IntPtr notification);

            [PreserveSig]
            int RegisterDuckNotification([MarshalAs(UnmanagedType.LPWStr)] string sessionId, IntPtr notification);

            [PreserveSig]
            int UnregisterDuckNotification(IntPtr notification);
        }

        [ComImport]
        [Guid("e2f5bb11-0570-40ca-acdd-3aa01277dee8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int count);

            [PreserveSig]
            int GetSession(int index, out IAudioSessionControl control);
        }

        [ComImport]
        [Guid("f4b1a599-7266-4319-a8ca-e70acb11e8cd")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl
        {
        }

        [ComImport]
        [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl2
        {
            [PreserveSig]
            int GetState(out int state);

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string displayName);

            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string displayName, ref Guid eventContext);

            [PreserveSig]
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string iconPath);

            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string iconPath, ref Guid eventContext);

            [PreserveSig]
            int GetGroupingParam(out Guid groupingId);

            [PreserveSig]
            int SetGroupingParam(ref Guid groupingId, ref Guid eventContext);

            [PreserveSig]
            int RegisterAudioSessionNotification(IntPtr notification);

            [PreserveSig]
            int UnregisterAudioSessionNotification(IntPtr notification);

            [PreserveSig]
            int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string sessionIdentifier);

            [PreserveSig]
            int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string sessionInstanceIdentifier);

            [PreserveSig]
            int GetProcessId(out uint processId);

            [PreserveSig]
            int IsSystemSoundsSession();

            [PreserveSig]
            int SetDuckingPreference([MarshalAs(UnmanagedType.Bool)] bool optOut);
        }

        [ComImport]
        [Guid("87ce5498-68d6-44e5-9215-6da47ef883d8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float level, ref Guid eventContext);

            [PreserveSig]
            int GetMasterVolume(out float level);

            [PreserveSig]
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, ref Guid eventContext);

            [PreserveSig]
            int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr reserved, uint coInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant value);

        [DllImport("ntdll.dll")]
        private static extern int RtlGetVersion(ref WindowsVersionInfo versionInfo);

        [DllImport("combase.dll", CharSet = CharSet.Unicode)]
        private static extern int WindowsCreateString(
            string sourceString,
            uint length,
            out IntPtr hstring);

        [DllImport("combase.dll")]
        private static extern int WindowsDeleteString(IntPtr hstring);

        [DllImport("combase.dll")]
        private static extern int RoGetActivationFactory(
            IntPtr activatableClassId,
            ref Guid interfaceId,
            out IntPtr factory);
    }
}
