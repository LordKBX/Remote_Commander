using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Server
{
    public partial class Program
    {
        private static void SetCleanUpTimer()
        {
            try
            {
                Thread objThread = new Thread(CleanUpLoop);
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start();
            }
            catch (ThreadStartException) { }
            catch (ThreadAbortException) { }
            catch (Exception) { }
        }

        private static void CleanUpLoop()
        {
            int loopInterval = 15000; //15 secondes

            while (unload == false)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(loopInterval);
            }
        }
    }
}
