using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using HeyRed.Mime;



namespace Server
{
    public partial class Program
    {
        private static bool unload = false;
        private static bool IsDebug = false;
        private static int listhendPort;//valeur par defaut 25000
        private static Execute exe;
        private static string inviteKey;
        private static string secureKey;
        private static string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static string extentionsDir = AppDomain.CurrentDomain.BaseDirectory+"\\Extentions\\";
        private static string imagesDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Images\\";
        public static string soundDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Sounds\\";
        private static Dictionary<string, Dictionary<string, object>> extentionsInfosList = new Dictionary<string, Dictionary<string, object>>();

        private static List<JObject> listSocket;

        private static FileSystemWatcher watcher;
        private static JObject MacroList;
        private static JObject GridsList;
        private static int lastUpdate;
        public static UdpClient newsock;
        
        private static void updateBaseDir() {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string osSep = "/";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true) { osSep = "\\"; }
            DirectoryInfo[] ldir = dir.GetDirectories();
            foreach (DirectoryInfo di in ldir) {
                Debug.WriteLine(di.Name);
            }
            if (Directory.Exists(dir.FullName + osSep + "Extentions") == true)
            {
                baseDir = dir.FullName;
                extentionsDir = baseDir + osSep + "Extentions" + osSep;
                imagesDir = baseDir + osSep + "Images" + osSep;
                soundDir = baseDir + osSep + "Sounds" + osSep;
                return;
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (dir.Parent == null) { return; }
                    dir = dir.Parent;
                    ldir = dir.GetDirectories();
                    foreach (DirectoryInfo di in ldir)
                    {
                        if (di.Name == "Extentions") {
                            baseDir = dir.FullName;
                            extentionsDir = baseDir + osSep + "Extentions" + osSep;
                            imagesDir = baseDir + osSep + "Images" + osSep;
                            soundDir = baseDir + osSep + "Sounds" + osSep;
                            return;
                        }
                        Debug.WriteLine(di.Name);
                    }
                }
            }
            //Environment.Exit(1);
        }

        [Conditional("DEBUG")]
        private static void isDebugF() { IsDebug = true; }

        public static void Main(string[] args)
        {
            updateBaseDir();
            Debug.WriteLine("baseDir = " + baseDir);
            Debug.WriteLine("extentionsDir = " + extentionsDir);
            Debug.WriteLine("imagesDir = " + imagesDir);
            Debug.WriteLine("soundDir = " + soundDir);
            isDebugF();
            PluginLoadAll();
            if (IsDebug == false) {
                Dictionary<string, object> plug = Server.Program.PluginGet("TrayIcon");
                if (plug == null) { return; }
                try
                {
                    Assembly asem = (Assembly)plug["assembly"];
                    Type t = (Type)plug["type"];
                    object instance = (object)plug["instance"];

                    MethodInfo m = t.GetMethod("HideConsole");
                    Debug.WriteLine("m.Invoke(instance, new object[] {});");
                    m.Invoke(instance, new object[] { });
                }
                catch (Exception error) { }
            }
            
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            listSocket = new List<JObject>();
            exe = new Execute();
            LoadRemoteParams();

            watcher = new FileSystemWatcher(baseDir);
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
            watcher.Filter = "*.json";
            watcher.Changed += OnLoadRemoteParamsChanged;
            watcher.Created += OnLoadRemoteParamsChanged;
            //watcher.Deleted += OnLoadRemoteParamsChanged;
            watcher.Renamed += OnLoadRemoteParamsChanged;
            watcher.EnableRaisingEvents = true;

            
            try
            {
                Thread objThread = new Thread(TimerLoop);
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start();
            }
            catch (ThreadStartException objException) { }
            catch (ThreadAbortException objException) { }
            catch (Exception objException) { }
            

            byte[] data = new byte[51200];
            listhendPort = 25000;
            //listhendPort = FreeTcpPort();
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, listhendPort);
            newsock = new UdpClient(ipep);
            
            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            while(unload == false)
            {
                try {
                    data = newsock.Receive(ref sender);
                    Console.WriteLine("Message received from {0}:", sender.ToString());
                    string text = Encoding.UTF8.GetString(data, 0, data.Length);
                    try { Console.WriteLine(text); } catch (Exception error) { Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length)); }
                    try {
                        JObject ob = JObject.Parse(text);
                        string function = "";
                        try { function = ob.GetValue("function").Value<string>(); } catch (Exception error) { }
                        string macro = null;
                        try { macro = ob.GetValue("macro").Value<string>(); } catch (Exception error) { }

                        if (function != "")
                        {
                            if (function == "Pong")
                            {
                                for(int i=0; i < listSocket.Count; i++) {
                                    if (listSocket[i]["addr"].Value<string>() == sender.Address.ToString() && listSocket[i]["port"].Value<int>() == sender.Port) {
                                        listSocket[i]["last"] = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                                    }
                                }
                                continue;
                            }
                            if (function == "GetInfo")
                            {
                                JObject sock = new JObject();
                                sock["addr"] = sender.Address.ToString();
                                sock["port"] = sender.Port;
                                sock["last"] = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                                listSocket.Add(sock);
                                data = Encoding.UTF8.GetBytes(GetInfo());
                                newsock.Send(data, data.Length, sender);
                            }
                           
                            if (function == "GetGrids" || function == "ForceReload")
                            {
                                if (function == "ForceReload") { LoadRemoteParams(); }
                                string gri = JsonConvert.SerializeObject(GridsList.GetValue("grids"));
                                data = Encoding.UTF8.GetBytes("{\"function\":\"SendGrids\", \"grids\":" + gri + "}");
                            }
                           
                            if (function == "GetImage")
                            {
                                string filePath = imagesDir + ob["reference"].Value<string>();
                                if (File.Exists(filePath) == true)
                                {
                                    using (Image image = Image.FromFile(filePath))
                                    {
                                        using (MemoryStream m = new MemoryStream())
                                        {
                                            image.Save(m, image.RawFormat);
                                            byte[] imageBytes = m.ToArray();
                                            string base64String = Convert.ToBase64String(imageBytes);
                                            data = Encoding.UTF8.GetBytes("{\"function\":\"RetGetImage\", \"reference\":\"" + ob["reference"].Value<string>() + "\", \"result\":\"data:"+ MimeTypesMap.GetMimeType(filePath) + ";base64," + base64String + "\"}");
                                        }
                                    }
                                }
                                else
                                {
                                    data = Encoding.UTF8.GetBytes("{\"function\":\"RetGetImage\", \"reference\":\"" + ob["reference"].Value<string>() + "\", \"result\":\"ERROR\"}");
                                }
                            }

                            if (function == "GetSoundInfo" || function == "MuteSound" || function == "VolUp" || function == "VolDown")
                            {
                                JToken tok = ob.Value<JToken>();
                                ParseSoundInfo(function, ref newsock, sender, tok);
                            }

                            newsock.Send(data, data.Length, sender);
                        }
                        if (macro != null)
                        {
                            string sound = null;
                            try { sound = ob.GetValue("sound").Value<string>(); } catch (Exception error) { }
                            MacrosProcessing.Run(exe, MacroList, macro, sound, newsock, sender);
                        }


                    } catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); Debug.WriteLine(Encoding.UTF8.GetString(data, 0, data.Length)); }
                    
                }
                catch (Exception error) { Debug.WriteLine(error.StackTrace); }
            }
        }

        private static void LoadRemoteParams(bool update = false) {
            string macrofile = System.IO.File.ReadAllText(baseDir+"\\macros.json").Replace("\\n", " ");
            string gridsfile = System.IO.File.ReadAllText(baseDir+"\\grids.json").Replace("\\n", " ");
            MacroList = JObject.Parse(macrofile);
            GridsList = JObject.Parse(gridsfile);
            lastUpdate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            if (update == true) {
                byte[] data = new byte[51200];
                string gri = JsonConvert.SerializeObject(GridsList.GetValue("grids"));
                data = Encoding.UTF8.GetBytes("{\"function\":\"SendGrids\", \"grids\":" + gri + "}");
                foreach (JObject ob in listSocket)
                {
                    newsock.Send(data, data.Length, ob["addr"].Value<string>(), ob["port"].Value<int>());
                }
            }
        }

        private static void OnLoadRemoteParamsChanged(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            LoadRemoteParams(true);
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            unload = true;
            PluginDisposeAll();
            try { Program.exe.KillAll(); } catch (Exception error) { }
        }

        private static string GetInfo() {
            JObject ob = new JObject();
            ob["function"] = "GetInfo";
            ob["application"] = "LordKBX_Remote_Controller";
            ob["version"] = "1.0";
            ob["hostName"] = System.Environment.MachineName;
            ob["port"] = listhendPort;
            ob["inviteKey"] = inviteKey;
            ob["lastUpdate"] = lastUpdate;
            ob["time"] = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return JsonConvert.SerializeObject(ob);
        }
        private static void TimerLoop()
        {
            byte[] data = new byte[1024];
            List<string> addrl = new List<string>();
            List<int> purgeList = new List<int>();
            int currentTime = 0;
            int cptLoop = 100;
            while (unload == false)
            {
                currentTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                addrl.Clear();
                purgeList.Clear();
                if (cptLoop >= 100) { cptLoop = 0; inviteKey = Crytography.Encrypt(DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongTimeString()); }
                data = Encoding.UTF8.GetBytes(GetInfo());
                //newsock.Send(data, data.Length, "192.168.1.255", 48000);

                for (int i = 0; i < listSocket.Count; i++)
                {
                    if (listSocket[i]["last"].Value<int>() + 30 < currentTime) { purgeList.Add(i); }
                }
                for (int i = purgeList.Count-1; i > -1; i--)
                {
                    listSocket.RemoveAt(purgeList[i]);
                }
                data = Encoding.UTF8.GetBytes("{\"function\":\"Ping\"}");
                foreach (JObject ob in listSocket) {
                    newsock.Send(data, data.Length, ob["addr"].Value<string>(), ob["port"].Value<int>());

                    IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ob["addr"].Value<string>()), ob["port"].Value<int>());
                    JToken tok = new JObject();
                    ParseSoundInfo("GetSoundInfo", ref newsock, sender, tok);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(5000);
                //Thread.Sleep(1000);
                cptLoop += 1;
            }
        }
    }
}
