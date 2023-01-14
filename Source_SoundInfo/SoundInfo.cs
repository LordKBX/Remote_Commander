using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Source_SoundInfo
{
    public class ExtType
    {
        public static readonly string Type = "Source";
        public static readonly string Name = "SoundInfo";
        //return type json = json stringified
        public static readonly Dictionary<string, string> Interfaces = new Dictionary<string, string>() {
            { "GetVolume", "float" },
            { "IsMute", "bool" },
            { "SetVolume", "float" },
            { "SetMute", "bool" }
        };

    }

    public class Source
    {
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioEndpointVolume
        {
            // f(), g(), ... are unused COM method slots. Define these if you care
            int f(); int g(); int h(); int i();
            int SetMasterVolumeLevelScalar(float fLevel, System.Guid pguidEventContext);
            int j();
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int k(); int l(); int m(); int n();
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, System.Guid pguidEventContext);
            int GetMute(out bool pbMute);
        }
        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMMDevice
        {
            int Activate(ref System.Guid id, int clsCtx, int activationParams, out IAudioEndpointVolume aev);
        }
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMMDeviceEnumerator
        {
            int f(); // Unused
            int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
        }
        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class MMDeviceEnumeratorComObject { }

        private static IAudioEndpointVolume device = null;

        static IAudioEndpointVolume Vol()
        {
            try
            {
                IMMDeviceEnumerator enumerator = new MMDeviceEnumeratorComObject() as IMMDeviceEnumerator;
                IMMDevice dev = null;
                Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(/*eRender*/ 0, /*eMultimedia*/ 1, out dev));
                IAudioEndpointVolume epv = null;
                var epvid = typeof(IAudioEndpointVolume).GUID;
                Marshal.ThrowExceptionForHR(dev.Activate(ref epvid, /*CLSCTX_ALL*/ 23, 0, out epv));
                return epv;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        public static float GetVolume()
        {
            if (device == null) { device = Vol(); }
            if (device == null) { return -1; }
            float v = -1;
            try
            {
                Marshal.ThrowExceptionForHR(device.GetMasterVolumeLevelScalar(out v));
            }
            catch (Exception) { }
            return v;
        }
        public static float SetVolume(float value)
        {
            if (device == null) { device = Vol(); }
            if (device == null) { return 0; }
            try
            {
                Marshal.ThrowExceptionForHR(device.SetMasterVolumeLevelScalar(value, System.Guid.Empty));
                return value;
            }
            catch (Exception) { return GetVolume(); }
        }

        public static bool IsMute()
        {
            if (device == null) { device = Vol(); }
            if (device == null) { return false; }
            bool mute = false;
            try
            {
                Marshal.ThrowExceptionForHR(device.GetMute(out mute));
            }
            catch (Exception) { }
            return mute;
        }

        public static bool SetMute(bool state)
        {
            if (device == null) { device = Vol(); }
            if (device == null) { return false; }
            try
            {
                Marshal.ThrowExceptionForHR(device.SetMute(state, System.Guid.Empty));
                return state;
            }
            catch (Exception) { return IsMute(); }
        }
    }
}
