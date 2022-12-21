﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Server
{
    class Execute
    {
        public List<int> procs = new List<int>();
        private bool unload = false;
        public void ExecuteCommandSync(object command)
        {
            try
            {
                string[] tab = ((string)(command)).Split("<<>>");
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(tab[0], tab[1]);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                if (unload == false) { procs.Add(proc.Id); }
                string result = proc.StandardOutput.ReadToEnd();
                Console.WriteLine(result);
                if (unload == false)
                {
                    proc.Close();
                    proc.Dispose();
                }
            }
            catch (Exception objException)
            {
                Debug.WriteLine(objException.Message);
                Debug.WriteLine(objException.Source);
                Debug.WriteLine(objException.Data);
            }
        }

        public void ExecuteCommandAsync(string command, string options)
        {
            try
            {
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start(command + "<<>>" + options);
            }
            catch (ThreadStartException) { }
            catch (ThreadAbortException) { }
            catch (Exception) { }
        }

        public void KillAll() {
            unload = true;
            foreach (int id in procs) { ExecuteCommandSync("taskkill<<>>/PID " + id + " /F"); }
        }
    }
}
