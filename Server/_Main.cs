﻿using System;
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
    public static partial class Program
    {
        public static string osSep = "/";
        private static bool unload = false;
        private static bool IsDebug = false;
        private static readonly int listhendPort = 25000;//valeur par defaut 25000
        private static string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static string extentionsDir = AppDomain.CurrentDomain.BaseDirectory+"\\Extentions\\";
        private static string imagesDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Images\\";
        public static string soundDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Sounds\\";
        public static string modsDir = AppDomain.CurrentDomain.BaseDirectory+ "\\Mods\\";
        private static Dictionary<string, Dictionary<string, object>> extentionsInfosList = new Dictionary<string, Dictionary<string, object>>();
        private static Dictionary<string, Dictionary<string, object>> sourcesInfosList = new Dictionary<string, Dictionary<string, object>>();
        private static Dictionary<string, Dictionary<string, object>> ImagesList = new Dictionary<string, Dictionary<string, object>>();
        public static Dictionary<string, string> ModsList = new Dictionary<string, string>();

        private static Dictionary<string, JObject> listSocket;
        private static RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider(1024);

        private static FileSystemWatcher watcher;
        private static FileSystemWatcher watcherMods;
        public static JObject MacroList;
        public static JObject GridsList;
        private static int lastUpdate;
        //public static UdpClient newsock = null;
        public static UdpClient newsock;

        public static void Main(string[] args)
        {
            Console.WriteLine("Start");
            isDebugF();
            Console.WriteLine("isDebugF");
            updateBaseDir();

            Console.WriteLine("baseDir = " + baseDir);
            Console.WriteLine("extentionsDir = " + extentionsDir);
            Console.WriteLine("imagesDir = " + imagesDir);
            Console.WriteLine("soundDir = " + soundDir);

            Crytography.Init();

            PluginLoadAll();

            IsAdmin();
            Console.WriteLine("IsAdmin");
            if (IsDebug == false) { HideWindow(); }
            ShowWindow();


            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            listSocket = new Dictionary<string, JObject>();
            LoadRemoteParams();
            SetParamWatcher();
            SetCleanUpTimer();

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, listhendPort);
            try { newsock = new UdpClient(ipep); newsock.Close(); }
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

                //Console.WriteLine(result);
                string[] rezs = result.Split("\r");
                foreach (string line in rezs)
                {
                    if(line.Contains("0.0.0.0:" + listhendPort) == true) {
                        string ml = line;
                        Console.WriteLine(line);
                        while (ml.Contains("  ")) {
                            ml = ml.Replace("  ", " ");
                        }
                        Console.WriteLine(ml);
                        string[] tab = ml.Split(" ");
                        Console.WriteLine(JsonConvert.SerializeObject(tab));

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
            }

            InitWebsocket();

            while (unload == false)
            {
                Thread.Sleep(500);
            }
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
            try { Execute.KillAll(); } catch (Exception) { }
        }

        public static string GetInfo() {
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
    }
}
