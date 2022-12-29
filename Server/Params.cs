using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.InteropServices;

namespace Server
{
    public partial class Program
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
            string macrofile = System.IO.File.ReadAllText(baseDir + "\\macros.json").Replace("\\n", " ");
            string gridsfile = System.IO.File.ReadAllText(baseDir + "\\grids.json").Replace("\\n", " ");
            MacroList = JObject.Parse(macrofile);
            GridsList = JObject.Parse(gridsfile);
            lastUpdate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            if (update == true)
            {
                string gri = JsonConvert.SerializeObject(GridsList.GetValue("grids"));
                string data = "{\"function\":\"SendGrids\", \"grids\":" + gri + "}";
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
        }
    }
}