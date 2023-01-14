using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Server
{
    public static partial class Program
    {
        private static void updateBaseDir()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true) { Program.osSep = "\\"; }
            DirectoryInfo[] ldir = dir.GetDirectories();
            foreach (DirectoryInfo di in ldir)
            {
                Console.WriteLine(di.Name);
            }
            if (Directory.Exists(dir.FullName + osSep + "Extentions") == true)
            {
                baseDir = dir.FullName;
                extentionsDir = baseDir + osSep + "Extentions" + osSep;
                imagesDir = baseDir + osSep + "Images" + osSep;
                soundDir = baseDir + osSep + "Sounds" + osSep;
                modsDir = baseDir + osSep + "Mods" + osSep;
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
                        if (di.Name == "Extentions")
                        {
                            baseDir = dir.FullName;
                            extentionsDir = baseDir + osSep + "Extentions" + osSep;
                            imagesDir = baseDir + osSep + "Images" + osSep;
                            soundDir = baseDir + osSep + "Sounds" + osSep;
                            modsDir = baseDir + osSep + "Mods" + osSep;
                            return;
                        }
                        Console.WriteLine(di.Name);
                    }
                }
            }
            //Environment.Exit(1);
        }

        public static void LoadRemoteParams(bool update = false)
        {
            string macrofile = System.IO.File.ReadAllText(baseDir + osSep + "macros.json").Replace("\\n", " ");
            string gridsfile = System.IO.File.ReadAllText(baseDir + osSep + "grids.json").Replace("\\n", " ");
            MacroList = JObject.Parse(macrofile);
            GridsList = JObject.Parse(gridsfile);

            ModsList.Clear();
            if(!Directory.Exists(modsDir)) { Directory.CreateDirectory(modsDir); }

            string[] fileEntries = Directory.GetFiles(modsDir);
            foreach (string fileName in fileEntries) {
                if (fileName.ToLower().EndsWith(".xml"))
                {
                    string name = fileName.Substring(fileName.LastIndexOf(osSep)+1);
                    name = name.Substring(0, name.LastIndexOf('.'));

                    var messages = new StringBuilder();
                    XmlReaderSettings settings = new XmlReaderSettings { ValidationType = ValidationType.DTD, DtdProcessing = DtdProcessing.Parse };
                    settings.ValidationEventHandler += (sender, args) => messages.AppendLine(args.Message);
                    var reader = XmlReader.Create(fileName, settings);

                    if (messages.Length > 0)
                    {
                        Console.WriteLine(fileName+"; Document is not valid!");
                    }
                    else
                    {
                        ModsList.Add(name, System.IO.File.ReadAllText(fileName)/*.Replace("\r", "").Replace("\n", "").Replace("\t", "")*/);
                    }
                }
            }

            lastUpdate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            SocketMaxTick = 1;
            if (Program.GridsList.ContainsKey("sources"))
            {
                List<JToken> sources = Program.GridsList["sources"].ToList<JToken>();
                Console.WriteLine("sources.Count = " + sources.Count);
                for (int mod = 0; mod < sources.Count; mod++)
                {
                    string moduleName = sources[mod]["name"].Value<string>();
                    List<JToken> interfaces = sources[mod]["interfaces"].ToList<JToken>();

                    for (int itfs = 0; itfs < interfaces.Count; itfs++)
                    {
                        int interval = interfaces[itfs]["interval"].Value<int>();
                        string minterfaceName = interfaces[itfs]["name"].Value<string>();
                        if (interval > SocketMaxTick) { SocketMaxTick = interval; }
                    }
                }
            }
            //SocketMaxTick += 1;

            if (update == true)
            {
                string gri = JsonConvert.SerializeObject(GridsList.GetValue("grids"));
                string mods = JsonConvert.SerializeObject(ModsList);
                string orientation = Program.GridsList["orientation"].Value<string>();
                string data = "{\"function\":\"SendGrids\", \"grids\":" + gri + ", \"mods\":" + mods + ", \"orientation\":\"" + orientation + "\"}";
                socket.WebSocketServices["/Service"].Sessions.Broadcast(data);
            }
        }

        private static void OnLoadRemoteParamsChanged(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            LoadRemoteParams(true);
        }

        private static void SetParamWatcher()
        {
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

            watcherMods = new FileSystemWatcher(modsDir);
            watcherMods.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
            watcherMods.Filter = "*.xml";
            watcherMods.Changed += OnLoadRemoteParamsChanged;
            watcherMods.Created += OnLoadRemoteParamsChanged;
            //watcherMods.Deleted += OnLoadRemoteParamsChanged;
            watcherMods.Renamed += OnLoadRemoteParamsChanged;
            watcherMods.EnableRaisingEvents = true;
        }
    }
}