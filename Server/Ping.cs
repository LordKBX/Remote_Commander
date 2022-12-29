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
        private static void SetPingTimer()
        {
            try
            {
                Thread objThread = new Thread(PingLoop);
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start();
            }
            catch (ThreadStartException) { }
            catch (ThreadAbortException) { }
            catch (Exception) { }
        }

        private static void PingLoop()
        {
            byte[] data = new byte[1024];
            List<string> addrl = new List<string>();
            List<string> purgeList = new List<string>();
            double currentTime = 0;
            int loopInterval = 5000; //5 secondes

            while (unload == false)
            {
                currentTime = Program.getUnixTimeStamp(false);
                try
                {
                    data = Encoding.UTF8.GetBytes("{\"function\":\"beep\"}");
                    newsock.Send(data, data.Length, "192.168.1.255", 48000); 
                }
                catch (System.NullReferenceException) { Console.Beep(); Console.WriteLine("Beep error"); }
                catch (Exception) { Console.Beep(); Console.WriteLine("Beep error"); }

                purgeList.Clear();
                foreach (KeyValuePair<string, JObject> ob in listSocket) { if (ob.Value["last"].Value<int>() + 30.0 < currentTime) { purgeList.Add(ob.Key); } }
                for (int i = purgeList.Count - 1; i > -1; i--) { listSocket.Remove(purgeList[i]); }

                data = Encoding.UTF8.GetBytes("{\"function\":\"Ping\"}");

                foreach (KeyValuePair<string, JObject> ob in listSocket)
                {
                    try
                    {
                        newsock.Send(data, data.Length, ob.Key, ob.Value["port"].Value<int>());

                        IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ob.Value["addr"].Value<string>()), ob.Value["port"].Value<int>());
                        JToken tok = new JObject();
                        //ParseSoundInfo("GetSoundInfo", ref newsock, sender, tok);
                    }
                    catch (Exception) { }
                }
                Thread.Sleep(loopInterval);
            }
        }
    }
}
