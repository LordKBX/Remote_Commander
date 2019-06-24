using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Timers;

namespace Configurator_Win
{
    static class Program
    {
        public static string configDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static System.Timers.Timer aTimer;
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            UpdateBaseDir();
            aTimer = new System.Timers.Timer(5000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static Double getUnixTimeStamp(bool InMilliseconds = false)
        {
            if (InMilliseconds == false) { return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; }
            else { return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds; }
        }

        public static void CleanMemery()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e) { CleanMemery(); }

        private static void UpdateBaseDir()
        {
            if (!System.IO.File.Exists(configDirectory + "\\grids.json"))
            {
                DirectoryInfo dir1 = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                dir1 = dir1.Parent.Parent.Parent;
                configDirectory = dir1.FullName + "\\Server";
                if (!System.IO.File.Exists(configDirectory + "\\grids.json"))
                {
                    configDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Remote Commander";
                    if (!System.IO.File.Exists(configDirectory + "\\grids.json"))
                    {
                        MessageBox.Show("Impossible de trouver les fichiers de configuration", "Error");
                    }
                }
            }
            if (!System.IO.File.Exists(baseDirectory + "\\html\\jquery-1.12.4.min.js"))
            {
                DirectoryInfo dir2 = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                dir2 = dir2.Parent.Parent;
                baseDirectory = dir2.FullName;
                if (!System.IO.File.Exists(baseDirectory + "\\html\\jquery-1.12.4.min.js"))
                {
                    configDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Remote Commander";
                    if (!System.IO.File.Exists(configDirectory + "\\html\\jquery-1.12.4.min.js"))
                    {
                        MessageBox.Show("Impossible de trouver les fichiers html", "Error");
                    }
                }
            }
        }
    }
}
