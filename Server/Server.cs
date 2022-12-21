using System;
using System.Xml;
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
using System.Security.Principal;


namespace Server
{
    public partial class Program
    {
        private static bool unload = false;
        private static bool IsDebug = false;
        private static int listhendPort;//valeur par defaut 25000
        private static Execute exe;
        private static string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static string extentionsDir = AppDomain.CurrentDomain.BaseDirectory+"\\Extentions\\";
        private static string imagesDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Images\\";
        public static string soundDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Sounds\\";
        private static Dictionary<string, Dictionary<string, object>> extentionsInfosList = new Dictionary<string, Dictionary<string, object>>();
        private static Dictionary<string, Dictionary<string, object>> ImagesList = new Dictionary<string, Dictionary<string, object>>();

        private static Dictionary<string, JObject> listSocket;
        private static RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider(1024);

        private static FileSystemWatcher watcher;
        private static JObject MacroList;
        private static JObject GridsList;
        private static int lastUpdate;
        //public static UdpClient newsock = null;
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

        public static Double getUnixTimeStamp(bool InMilliseconds = false)
        {
            if (InMilliseconds == false) { return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; }
            else { return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds; }
        }

        [Conditional("DEBUG")]
        private static void isDebugF() { IsDebug = true; }

        public static void Main(string[] args)
        {
            isDebugF();
            if (IsDebug == false)
            {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    if (pricipal.IsInRole(WindowsBuiltInRole.Administrator) == false)
                    {
                        Environment.Exit(0);
                    }
                }
            }
            updateBaseDir();
            Debug.WriteLine("baseDir = " + baseDir);
            Debug.WriteLine("extentionsDir = " + extentionsDir);
            Debug.WriteLine("imagesDir = " + imagesDir);
            Debug.WriteLine("soundDir = " + soundDir);
            Crytography.Init();
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
                catch (Exception) { }
            }
            
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            listSocket = new Dictionary<string, JObject>();
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
            catch (ThreadStartException) { }
            catch (ThreadAbortException) { }
            catch (Exception) { }
            

            byte[] data = new byte[51200];
            listhendPort = 25000;
            //listhendPort = FreeTcpPort();
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, listhendPort);
            try { newsock = new UdpClient(ipep); }
            catch (Exception)
            {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("netstat", "-a -o -p UDP -n");
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                string result = proc.StandardOutput.ReadToEnd();

                //Debug.WriteLine(result);
                string[] rezs = result.Split("\r");
                foreach (string line in rezs)
                {
                    if(line.Contains("UDP") == true && line.Contains("0.0.0.0:25000") == true) {
                        string ml = line;
                        Debug.WriteLine(line);
                        while (ml.Contains("  ")) {
                            ml = ml.Replace("  ", " ");
                        }
                        Debug.WriteLine(ml);
                        string[] tab = ml.Split(" ");
                        Debug.WriteLine(JsonConvert.SerializeObject(tab));

                        procStartInfo = new System.Diagnostics.ProcessStartInfo("taskkill", "/PID "+ tab[tab.Length - 1] + " /F");
                        procStartInfo.RedirectStandardOutput = true;
                        procStartInfo.UseShellExecute = false;
                        procStartInfo.CreateNoWindow = true;
                        proc = new System.Diagnostics.Process();
                        proc.StartInfo = procStartInfo;
                        proc.Start();
                        result = proc.StandardOutput.ReadToEnd();
                        break;
                    }
                }
                newsock = new UdpClient(ipep);
            }
            
            newsock.AllowNatTraversal(true);


            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            bool DoEncoding = false;
            string datas = "";
            while (unload == false)
            {
                DoEncoding = false;
                try {
                    Thread.Sleep(500);
                    if (newsock.Available > 0) // Only read if we have some data 
                    {
                        data = newsock.Receive(ref sender);
                        Console.WriteLine("Message received from {0}:", sender.ToString());
                        string text = Encoding.UTF8.GetString(data, 0, data.Length);
                        //try { Console.WriteLine(text); } catch (Exception error) { Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length)); }
                        try
                        {
                            string ip = sender.Address.ToString();
                            JObject ob = JObject.Parse(text);
                            string type = "";
                            try { type = ob.GetValue("type").Value<string>(); } catch (Exception) { }
                            string function = "";
                            try { function = ob.GetValue("function").Value<string>(); } catch (Exception) { }

                            if (function != "" && function != "GetInfo")
                            {
                                datas = "{\"type\":\"error\", \"cause\":\"no_encoded_function\", \"function\":\"" + function + "\"}";
                                data = Encoding.UTF8.GetBytes(datas);
                                newsock.Send(data, data.Length, sender);
                                continue;
                            }

                            if (type == "encoded")
                            {
                                //Debug.WriteLine("ENCODED FRAME");
                                if (listSocket.ContainsKey(ip) == true)
                                {
                                    try
                                    {
                                        string decoded = Crytography.Decrypt(ob["data"].Value<string>());
                                        Debug.WriteLine("encoded data = " + decoded);
                                        ob = JObject.Parse(decoded);
                                    }
                                    catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); }
                                }
                            }
                            string macro = null;
                            try { macro = ob.GetValue("macro").Value<string>(); } catch (Exception) { }
                            try { function = ob.GetValue("function").Value<string>(); } catch (Exception) { }

                            if (function != "")
                            {
                                if (function != "Login" && function != "GetInfo")
                                    {
                                    Debug.WriteLine(JsonConvert.SerializeObject(listSocket[ip]));
                                    if (listSocket[ip]["loged"].Value<int>() == 0)
                                        {
                                            datas = "{\"type\":\"error\", \"cause\":\"not_loged\"}";
                                            data = Encoding.UTF8.GetBytes(datas);
                                            newsock.Send(data, data.Length, sender);
                                            continue;
                                        }
                                    }
                                if (function == "GetInfo")
                                {
                                    if (listSocket.ContainsKey(ip) == false)
                                    {
                                        JObject sock = new JObject();
                                        sock["addr"] = sender.Address.ToString();
                                        sock["port"] = sender.Port;
                                        sock["keyPU"] = ob.GetValue("keyPU").Value<string>();
                                        sock["last"] = getUnixTimeStamp(false);
                                        sock["loged"] = 0;
                                        listSocket.Add(ip, sock);
                                    }
                                    else
                                    {
                                        listSocket[ip]["keyPU"] = ob.GetValue("keyPU").Value<string>();
                                        listSocket[ip]["last"] = getUnixTimeStamp(false);
                                    }
                                    DoEncoding = true;
                                    datas = GetInfo();
                                    //newsock.Send(data, data.Length, sender);
                                }

                                if (function == "Pong")
                                {
                                    if (listSocket.ContainsKey(sender.Address.ToString()) == true)
                                    {
                                        listSocket[sender.Address.ToString()]["last"] = getUnixTimeStamp(false);
                                    }
                                    continue;
                                }

                                if (function == "Login")
                                {
                                    DoEncoding = true;
                                    string password = null;
                                    try { password = ob.GetValue("password").Value<string>(); } catch (Exception) { }
                                    if (password == GridsList["password"].Value<string>()) {
                                        datas = "{\"function\":\"Login\", \"status\":\"OK\"}";
                                        listSocket[ip]["loged"] = 1;
                                    }
                                    else {
                                        datas = "{\"function\":\"Login\", \"status\":\"error\"}";
                                    }
                                }

                                if (function == "GetGrids" || function == "ForceReload")
                                {
                                    if (function == "ForceReload") { LoadRemoteParams(); }
                                    string gri = JsonConvert.SerializeObject(GridsList.GetValue("grids"));
                                    DoEncoding = true;
                                    datas = "{\"function\":\"SendGrids\", \"grids\":" + gri + "}";
                                    data = Encoding.UTF8.GetBytes(datas);
                                }

                                if (function == "GetImages")
                                {
                                    JArray refs = ob["references"].Value<JArray>();
                                    foreach (string refe in refs)
                                    {
                                        GetImage(refe, ref newsock, ref sender);
                                    }
                                    continue;
                                }
                                if (function == "GetImage")
                                {
                                    GetImage(ob["reference"].Value<string>(), ref newsock, ref sender);
                                    continue;
                                }

                                if (function == "GetSoundInfo" || function == "MuteSound" || function == "VolUp" || function == "VolDown")
                                {
                                    JToken tok = ob.Value<JToken>();
                                    ParseSoundInfo(function, ref newsock, sender, tok);
                                }

                                if (DoEncoding == true)
                                {
                                    try
                                    {
                                        if (listSocket.ContainsKey(ip) == true)
                                        {
                                            string key = listSocket[ip]["keyPU"].Value<string>();
                                            if (function == "GetInfo") { key = ob.GetValue("keyPU").Value<string>();  }
                                            string encoded = Crytography.Encrypt(datas, key);
                                            data = Encoding.UTF8.GetBytes("{\"type\":\"encoded\", \"data\":\"" + encoded + "\"}");
                                            newsock.Send(data, data.Length, sender);
                                        }
                                    }
                                    catch (Exception err) { Debug.WriteLine(JsonConvert.SerializeObject(err)); }
                                    continue;
                                }
                                newsock.Send(data, data.Length, sender);
                            }
                            if (macro != null)
                            {
                                string sound = null;
                                try { sound = ob.GetValue("sound").Value<string>(); } catch (Exception) { }
                                MacrosProcessing.Run(exe, MacroList, macro, sound, newsock, sender);
                            }


                        }
                        catch (Exception error)
                        {
                            Debug.WriteLine(JsonConvert.SerializeObject(error));
                            Debug.WriteLine(Encoding.UTF8.GetString(data, 0, data.Length));
                        }
                    }
                }
                catch (Exception error) {
                    //Debug.WriteLine(error.StackTrace);
                }
            }
        }


        private static void GetImage(string reference, ref UdpClient newsock, ref IPEndPoint sender)
        {
            byte[] data = null;
            string datas = "";
            string filePath = imagesDir + reference;
            bool error = false;
            if (ImagesList.ContainsKey(reference) == true)
            {
                if (File.Exists(filePath) == true)
                {
                    FileInfo fi = new FileInfo(filePath);
                    if (fi.LastWriteTime.ToFileTimeUtc() > (long)ImagesList[reference]["lastTime"]) { ImagesList.Remove(reference); }
                }
                else { error = true; }
            }

            if (error == false)
            {
                if (ImagesList.ContainsKey(reference) == false)
                {
                    if (File.Exists(filePath) == true)
                    {
                        FileInfo fi = new FileInfo(filePath);
                        using (Image image = Image.FromFile(filePath))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();
                                string base64String = Convert.ToBase64String(imageBytes);
                                ImagesList[reference] = new Dictionary<string, object>();
                                ImagesList[reference]["data"] = base64String;
                                ImagesList[reference]["lastTime"] = fi.LastWriteTime.ToFileTimeUtc();
                                ImagesList[reference]["MimeType"] = MimeTypesMap.GetMimeType(filePath);
                                datas = "{\"function\":\"RetGetImage\", \"reference\":\"" + reference + "\", \"result\":\"data:" + ImagesList[reference]["MimeType"] + ";base64," + base64String + "\"}";
                            }
                        }
                    }
                    else { error = true; }
                }
                else
                {
                    datas = "{\"function\":\"RetGetImage\", \"reference\":\"" + reference + "\", \"result\":\"data:" + ImagesList[reference]["MimeType"] + ";base64," + ImagesList[reference]["data"] + "\"}";
                }
            }
            
            if (error == true) { datas = "{\"function\":\"RetGetImage\", \"reference\":\"" + reference + "\", \"result\":\"ERROR\"}"; }
            data = Encoding.UTF8.GetBytes(datas);
            newsock.Send(data, data.Length, sender);
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
                foreach (KeyValuePair<string, JObject> ob in listSocket)
                {
                    newsock.Send(data, data.Length, ob.Value["addr"].Value<string>(), ob.Value["port"].Value<int>());
                }
            }
        }

        private static void OnLoadRemoteParamsChanged(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            LoadRemoteParams(true);
        }

        private static int FreeTcpPort()
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
            try { Program.exe.KillAll(); } catch (Exception) { }
        }

        private static string GetInfo() {
            JObject ob = new JObject();
            ob["function"] = "GetInfo";
            ob["application"] = "LordKBX_Remote_Controller";
            ob["version"] = "1.0";
            ob["hostName"] = System.Environment.MachineName;
            ob["port"] = listhendPort;
            ob["lastUpdate"] = lastUpdate;
            ob["PublicKey"] = Crytography.GetPublicKeyString();
            ob["time"] = getUnixTimeStamp(false);

            return JsonConvert.SerializeObject(ob);
        }

        private static void TimerLoop()
        {
            byte[] data = new byte[1024];
            List<string> addrl = new List<string>();
            List<string> purgeList = new List<string>();
            double currentTime = 0;
            int loopInterval = 5000; //1000

            while (unload == false)
            {
                currentTime = Program.getUnixTimeStamp(false);
                /*
                addrl.Clear();
                purgeList.Clear();
                */
                data = Encoding.UTF8.GetBytes("{\"function\":\"beep\"}");
                //if (newsock == null) { Thread.Sleep(loopInterval);  continue; }
                try { newsock.Send(data, data.Length, "192.168.1.255", 48000); } 
                catch (System.NullReferenceException) { Debug.WriteLine("Beep error"); }
                catch (Exception) { Debug.WriteLine("Beep error"); }
                foreach (KeyValuePair<string, JObject> ob in listSocket)
                {
                    if (ob.Value["last"].Value<int>() + 30.0 < currentTime) { purgeList.Add(ob.Key); }
                }
                for (int i = purgeList.Count-1; i > -1; i--)
                {
                    listSocket.Remove(purgeList[i]);
                }
                data = Encoding.UTF8.GetBytes("{\"function\":\"Ping\"}");
                foreach (KeyValuePair<string, JObject> ob in listSocket) {
                    try
                    {
                        newsock.Send(data, data.Length, ob.Key, ob.Value["port"].Value<int>());

                        IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ob.Value["addr"].Value<string>()), ob.Value["port"].Value<int>());
                        JToken tok = new JObject();
                        ParseSoundInfo("GetSoundInfo", ref newsock, sender, tok);
                    }
                    catch (Exception) { }
                }
                
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(loopInterval);
            }
        }
    }
}
