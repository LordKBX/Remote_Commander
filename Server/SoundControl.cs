using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public partial class Program
    {

        private static JObject GetSoundInfo()
        {
            Dictionary<string, object> plug = PluginGet("VolControl");
            if (plug == null) { return null; }
            Assembly asem = (Assembly)plug["assembly"];
            Type t = (Type)plug["type"];
            object instance = (object)plug["instance"];

            MethodInfo m = t.GetMethod("IsMute");
            MethodInfo u = t.GetMethod("GetVolume");
            bool state = (bool)(m.Invoke(instance, new object[] { }));
            float curVol = (float)(u.Invoke(instance, new object[] { }));
            JObject obf = new JObject();
            obf["function"] = "GetSoundInfo";
            obf["mute"] = state;
            obf["vol"] = curVol;
            return obf;
        }

        public static void ParseSoundInfo(string function, ref UdpClient newsock, IPEndPoint sender, JToken ob)
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

            byte[] data = new byte[51200];
            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetSoundInfo()));
            newsock.Send(data, data.Length, sender);
        }

    }
}
