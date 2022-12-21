using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server
{
    class MacrosProcessing
    {        
        public static void Run(Execute exe, JObject MacroList, string CalledMacro, string CalledSound, UdpClient newsock, IPEndPoint sender) {
            Debug.WriteLine("MacrosProcessing.Run: " + CalledMacro + " " + CalledSound);
            if (CalledMacro != "")
            {
                Debug.WriteLine("IN MACRO");
                string[] tabGivenMacro = CalledMacro.Split('/');
                foreach (JToken section in MacroList["sections"].ToList<JToken>())
                {
                    //Debug.WriteLine("section Compare: " + tabGivenMacro[0] + " " + section["name"].Value<string>());
                    if (section["name"].Value<string>() == tabGivenMacro[0])
                    {
                        //Debug.WriteLine("section data = " + JsonConvert.SerializeObject(section));
                        if (section["macros"].Value<JObject>().ContainsKey(tabGivenMacro[1]) == true)
                        {
                            JObject ob2 = section["macros"][tabGivenMacro[1]].Value<JObject>();
                            if (CalledSound != "") {
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true) { CalledSound = CalledSound.Replace("/", "\\"); }
                                ob2["sound"] = Program.soundDir + CalledSound;
                            }
                            string st = JsonConvert.SerializeObject(ob2);
                            //Debug.WriteLine("macro data = " + st);
                            Dictionary<string, object> options = new Dictionary<string, object>() {
                            { "exe", exe },
                            { "macro", ob2 },
                            { "newsock", newsock },
                            { "sender", sender }
                        };
                            if (st.Contains("Sleep") == true && st.Contains("delay") == true)
                            {
                                Thread objThread = new Thread(new ParameterizedThreadStart(RunPart2));
                                //Make the thread as background thread.
                                objThread.IsBackground = true;
                                //Set the Priority of the thread.
                                objThread.Priority = ThreadPriority.AboveNormal;
                                //Start the thread.
                                objThread.Start(options);
                            }
                            else
                            {
                                RunPart2(options);
                            }
                        }
                    }
                }
            }
            else
            {
                if (CalledSound != "")
                {
                    JObject options = new JObject();
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true) { CalledSound = CalledSound.Replace("/", "\\"); }
                    options["url"] = Program.soundDir + CalledSound;
                    PlaySound(options);
                }
            }
        }

        private static void RunPart2(object tab) {
            Debug.WriteLine("RunPart2()");
            Dictionary<string, object> values = (Dictionary<string, object>)tab;
            Execute exe = (Execute)values["exe"];
            JObject Macro = (JObject)values["macro"];
            UdpClient newsock = (UdpClient)values["newsock"];
            IPEndPoint sender = (IPEndPoint)values["sender"];

            try
            {
                string sound = Macro["sound"].Value<string>();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true) { sound = sound.Replace("/", "\\"); }
                Debug.WriteLine("sound = "+sound);
                if (sound != "") {
                    JObject a = new JObject();
                    a["url"] = sound;
                    PlaySound(a);
                }
            }
            catch (Exception) { }

            List<JToken> actions = Macro["actions"].ToList<JToken>();
            foreach (JToken action in actions)
            {
                Debug.WriteLine(JsonConvert.SerializeObject(Macro));
                string type = "";
                try { type = action["type"].Value<string>(); } catch (Exception) { }
                Console.WriteLine("type = " + type);
                if (type == "PlaySound") { PlaySound(action); }
                if (type == "Execute") { ExecuteCommand(exe, action); }
                if (type == "KeyBoardInput") { ExecuteKeyboardInput(action); }
                if (type == "GetSoundInfo" || type == "MuteSound" || type == "VolUp" || type == "VolDown")
                {
                    JToken tok = action.Value<JToken>();
                    Program.ParseSoundInfo(type, ref newsock, sender, tok);
                }
                if (type == "Sleep") { try { Thread.Sleep(action["delay"].Value<int>()); } catch (Exception) { } }
            }
        }

        private static void PlaySound(object ob) {
            JToken ob2 = (JToken)ob;
            string url = "";
            try { url = ob2["url"].Value<string>(); } catch (Exception) { }
            Debug.WriteLine("url = " + url);

            Dictionary<string, object> plug = Server.Program.PluginGet("Audio");
            if (plug == null) { return; }
            try
            {
                Assembly asem = (Assembly)plug["assembly"];
                Type t = (Type)plug["type"];
                object instance = (object)plug["instance"];

                MethodInfo m = t.GetMethod("PlaySound");
                Debug.WriteLine("m.Invoke(instance, new object[] { url });");
                m.Invoke(instance, new object[] { url });
            }
            catch (Exception) { }
        }

        private static void ExecuteCommand(Execute exe, JToken ob2) {
            string executable = "";
            try { executable = ob2["executable"].Value<string>(); } catch (Exception) { }
            if (executable.StartsWith(".\\") == true) { executable.Replace(".\\", AppDomain.CurrentDomain.BaseDirectory); }
            //Console.WriteLine("executable = " + executable);
            string eparams = "";
            try { eparams = ob2["params"].Value<string>(); } catch (Exception) { }
            //Console.WriteLine("params = " + eparams);
            exe.ExecuteCommandAsync(executable, eparams);
        }
        
        private static void ExecuteKeyboardInput(JToken ob2) {
            string key = null;
            bool ctrl = false;
            bool alt = false;
            bool maj = false;
            bool win = false;
            try { key = ob2["key"].Value<string>(); } catch (Exception) { }
            
            try { ctrl = ob2["ctrl"].Value<bool>(); } catch (Exception) { }
            try { alt = ob2["alt"].Value<bool>(); } catch (Exception) { }
            try { maj = ob2["maj"].Value<bool>(); } catch (Exception) { }
            try { win = ob2["win"].Value<bool>(); } catch (Exception) { }
            
            Console.WriteLine("key = " + key);
            try
            {
                Dictionary<string, object> plug = Server.Program.PluginGet("Keyboard");
                if (plug == null) { return; }

                Assembly asem = (Assembly)plug["assembly"];
                Type t = (Type)plug["type"];
                object instance = (object)plug["instance"];

                // Call the method
                if (key == null)
                {
                    MethodInfo m = t.GetMethod("SendSpe");
                    FieldInfo u = t.GetField("VK_NULL");
                    m.Invoke(instance, new object[] { u.GetValue(0), alt, ctrl, maj, win });
                }
                else if (key.StartsWith("MEDIA_") == true)
                {
                    MethodInfo m = t.GetMethod("SendSpe");
                    //Console.WriteLine("Test MEDIA_ OK");
                    FieldInfo u = null;
                    if (key == "MEDIA_PLAY_PAUSE") { u = t.GetField("VK_MEDIA_PLAY_PAUSE"); }
                    if (key == "MEDIA_PREV_TRACK") { u = t.GetField("VK_MEDIA_PREV_TRACK"); }
                    if (key == "MEDIA_NEXT_TRACK") { u = t.GetField("VK_MEDIA_NEXT_TRACK"); }
                    m.Invoke(instance, new object[] { u.GetValue(0), alt, ctrl, maj, win });
                }
                else
                {
                    MethodInfo m = t.GetMethod("Send");
                    foreach (char ch in key.ToCharArray())
                    {
                        m.Invoke(instance, new object[] { ch, alt, ctrl, maj, win });
                    }
                }
            } catch (Exception) { }
        }
    }
}
