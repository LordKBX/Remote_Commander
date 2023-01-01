using Accord.Audio;
using Accord.DirectSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Server
{
    public partial class Program
    {
        private static Dictionary<string, FileInfo> SoundList = new Dictionary<string, FileInfo>();
        private static NetCoreAudio.Player player = null;
        private static bool playerPlayFinished = true;

        private static AudioCaptureDevice audioSource = null;
        private static Accord.Video.MJPEGStream videoSource = null;


        public static void PlaySound(string filePath)
        {
            try
            {
                Console.WriteLine("PlaySound(string '" + filePath + "')");
                Console.WriteLine("PlaySound(string '" + filePath + "')");
                if (player == null)
                {
                    player = new NetCoreAudio.Player();
                    player.PlaybackFinished += Player_PlaybackFinished;
                }
                if (playerPlayFinished == false) { player.Stop(); }
                playerPlayFinished = false;
                player.Play(filePath);
            }
            catch (Exception er)
            {
                Console.WriteLine("Extention_WinAudio error: " + er.Message);
                Console.WriteLine("Extention_WinAudio error: " + er.Message);
            }
        }

        private static void Player_PlaybackFinished(object sender, EventArgs e)
        {
            playerPlayFinished = true;
        }

        public static void SoundRecordStart()
        {
            // Create default capture device
            audioSource = new AudioCaptureDevice();
            Accord.Video.MJPEGStream videoSource = new Accord.Video.MJPEGStream();

            // Specify capturing options
            audioSource.DesiredFrameSize = 4096;
            audioSource.SampleRate = 22050;
            audioSource.Format = SampleFormat.Format128BitComplex;

            // Specify the callback function which will be
            // called once a sample is completely available
            audioSource.NewFrame += audioSource_NewFrame;

            // Start capturing
            audioSource.Start();

        }

        // The callback function should determine what
        // should be done with the samples being caught
        private static void audioSource_NewFrame(object sender, Accord.Audio.NewFrameEventArgs eventArgs)
        {
            // Read current frame...
            Signal s = eventArgs.Signal;
            byte[] tab = s.RawData;


            // Process/play/record it
            // ...
        }

        public static JObject GetSoundInfo(bool full=true)
        {
            Dictionary<string, object> plug = PluginGet("VolControl");
            if (plug == null) { return null; }
            Assembly asem = (Assembly)plug["assembly"];
            Type t = (Type)plug["type"];
            object instance = (object)plug["instance"];

            MethodInfo m = t.GetMethod("IsMute");
            MethodInfo u = t.GetMethod("GetVolume");
            MethodInfo d1 = t.GetMethod("TryGetMediaInfo");
            MethodInfo d2 = t.GetMethod("TryGetMediaInfoFull");
            bool state = (bool)(m.Invoke(instance, new object[] { }));
            float curVol = (float)(u.Invoke(instance, new object[] { }));
            Task<string> mediaInfo = (full)?(Task<string>)(d2.Invoke(instance, new object[] { })): (Task<string>)(d1.Invoke(instance, new object[] { }));
            mediaInfo.Wait();
            JObject obf = new JObject();
            obf["function"] = "GetSoundInfo";
            obf["mute"] = state;
            obf["vol"] = curVol;
            obf["mediaInfo"] = mediaInfo.Result;
            return obf;
        }

        public static void ParseSoundInfo(string function, WebSocketService service, JToken ob)
        {
            Assembly asem = null;
            Type t = null;
            object instance = null;
            if (function == "MuteSound" || function == "VolUp" || function == "VolDown")
            {
                Dictionary<string, object> plug = PluginGet("VolControl");
                if (plug == null) { return; }
                asem = (Assembly)plug["assembly"];
                t = (Type)plug["type"];
                instance = (object)plug["instance"];
            }
            if (function == "MuteSound")
            {
                MethodInfo m = t.GetMethod("IsMute");
                MethodInfo u = t.GetMethod("SetMute");
                bool state = (bool)(m.Invoke(instance, new object[] { }));

                if (state == true) { u.Invoke(instance, new object[] { false }); } else { u.Invoke(instance, new object[] { true }); }
            }
            if (function == "VolUp")
            {
                MethodInfo m = t.GetMethod("GetVolume");
                MethodInfo u = t.GetMethod("SetVolume");
                float curVol = (float)(m.Invoke(instance, new object[] { }));

                float step = ((float)(ob["step"].Value<double>() / 100.0));
                Console.WriteLine("curVol = " + curVol.ToString());
                Console.WriteLine("step = " + step.ToString());

                if (curVol + step > ((float)(1.0))) { u.Invoke(instance, new object[] { (float)(1.00) }); }
                else { u.Invoke(instance, new object[] { curVol + step }); }
            }
            if (function == "VolDown")
            {
                MethodInfo m = t.GetMethod("GetVolume");
                MethodInfo u = t.GetMethod("SetVolume");
                float curVol = (float)(m.Invoke(instance, new object[] { }));

                float step = ((float)(ob["step"].Value<double>() / 100.0));
                Console.WriteLine("curVol = " + curVol.ToString());
                Console.WriteLine("step = " + step.ToString());

                if (curVol - step < ((float)(0.0))) { u.Invoke(instance, new object[] { (float)(0.00) }); }
                else { u.Invoke(instance, new object[] { curVol - step }); }
            }

            string soundInfo = JsonConvert.SerializeObject(GetSoundInfo());
            service.SendMessage(soundInfo);
        }
    }
}
