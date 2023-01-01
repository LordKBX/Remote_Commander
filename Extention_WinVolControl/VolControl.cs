using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Control;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks.Dataflow;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace Extention_WinVolControl
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

    public class VolControl
    {
        static IAudioEndpointVolume Vol()
        {
            var enumerator = new MMDeviceEnumeratorComObject() as IMMDeviceEnumerator;
            IMMDevice dev = null;
            Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(/*eRender*/ 0, /*eMultimedia*/ 1, out dev));
            IAudioEndpointVolume epv = null;
            var epvid = typeof(IAudioEndpointVolume).GUID;
            Marshal.ThrowExceptionForHR(dev.Activate(ref epvid, /*CLSCTX_ALL*/ 23, 0, out epv));
            return epv;
        }

        public static float GetVolume(){ float v = -1; Marshal.ThrowExceptionForHR(Vol().GetMasterVolumeLevelScalar(out v)); return v; }
        public static void SetVolume(float value) { Marshal.ThrowExceptionForHR(Vol().SetMasterVolumeLevelScalar(value, System.Guid.Empty)); }

        public static bool IsMute()
        {
            bool mute; Marshal.ThrowExceptionForHR(Vol().GetMute(out mute)); return mute;
        }
        public static void SetMute(bool state)
        {
            Marshal.ThrowExceptionForHR(Vol().SetMute(state, System.Guid.Empty));
        }

        public async static Task<string> TryGetMediaInfoFull()
        {
            JObject obf = new JObject();
            obf["Artist"] = null;
            obf["Title"] = null;
            obf["AlbumTitle"] = null;
            obf["Genres"] = null;
            obf["TrackNumber"] = 0;
            obf["AlbumTrackCount"] = 0;
            obf["Thumbnail"] = null;
            obf["error"] = null;

            try
            {
                GlobalSystemMediaTransportControlsSessionManager gsmtcsm = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await gsmtcsm.GetCurrentSession().TryGetMediaPropertiesAsync();
                obf["Artist"] = (mediaProperties.Artist== null || mediaProperties.Artist.Trim() == "")?mediaProperties.AlbumArtist: mediaProperties.Artist;
                obf["Title"] = mediaProperties.Title;
                obf["AlbumTitle"] = mediaProperties.AlbumTitle;
                obf["Genres"] = Join(mediaProperties.Genres);
                obf["TrackNumber"] = mediaProperties.TrackNumber;
                obf["AlbumTrackCount"] = mediaProperties.AlbumTrackCount;

                IRandomAccessStreamWithContentType tor = await mediaProperties.Thumbnail.OpenReadAsync();
                    IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)tor.Size);
                    await tor.ReadAsync(buffer, (uint)tor.Size, InputStreamOptions.None);

                    obf["Thumbnail"] = Convert.ToBase64String(buffer.ToArray());

                //Console.WriteLine("{0} - {1} + Thumbnail", mediaProperties.Artist, mediaProperties.Title);
            }
            catch(Exception err) { obf["error"] = err.StackTrace; }
            return JsonConvert.SerializeObject(obf);
        }

        public async static Task<string> TryGetMediaInfo()
        {
            JObject obf = new JObject();
            obf["Artist"] = null;
            obf["Title"] = null;
            obf["AlbumTitle"] = null;
            obf["Genres"] = null;
            obf["TrackNumber"] = 0;
            obf["AlbumTrackCount"] = 0;
            obf["Thumbnail"] = null;
            obf["error"] = null;

            try
            {
                GlobalSystemMediaTransportControlsSessionManager gsmtcsm = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await gsmtcsm.GetCurrentSession().TryGetMediaPropertiesAsync();
                obf["Artist"] = (mediaProperties.Artist== null || mediaProperties.Artist.Trim() == "")?mediaProperties.AlbumArtist: mediaProperties.Artist;
                obf["Title"] = mediaProperties.Title;
                obf["AlbumTitle"] = mediaProperties.AlbumTitle;
                obf["Genres"] = Join(mediaProperties.Genres);
                obf["TrackNumber"] = mediaProperties.TrackNumber;
                obf["AlbumTrackCount"] = mediaProperties.AlbumTrackCount;

                //Console.WriteLine("{0} - {1}", mediaProperties.Artist, mediaProperties.Title);
            }
            catch(Exception err) { obf["error"] = err.StackTrace; }
            return JsonConvert.SerializeObject(obf);
        }

        private static string Join(IReadOnlyList<string> list) {
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                if(sb.Length > 0) { sb.Append(";"); }
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }
    }
}

