using Microsoft.Extensions.Logging;
using SharpDX.DirectSound;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using System.Drawing.Imaging;

namespace Server
{
    public partial class Program
    {
        private static ILogger _logger;
        private static ILoggerFactory _loggerFactory;
        private static WebSocketServer socket;
        private static Dictionary<string, Dictionary<string, string>> sessionsData;

        private static void InitWebsocket() { 
            sessionsData = new Dictionary<string, Dictionary<string, string>>();
            socket = new WebSocketServer(IPAddress.Any, Program.listhendPort, true);
            socket.SslConfiguration.ServerCertificate = new X509Certificate2(baseDir + Path.DirectorySeparatorChar + "cert.pfx", "lordkbx");
            socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 & System.Security.Authentication.SslProtocols.Tls13;
            socket.AddWebSocketService<WebSocketService>("/Service");
            socket.Start();
            Console.WriteLine("Waiting for a client...");
        }

        private static void StopWebSocket() { socket.Stop(); }

        public static Dictionary<string, string> GetSessionData(string sessionID) {
            if (sessionsData.ContainsKey(sessionID)) {
                return sessionsData[sessionID];
            }
            return null;
        }

        public static bool HasSession(string sessionID) {
            if (sessionsData.ContainsKey(sessionID)) { return true; }
            return false;
        }

        public static void DelSession(string sessionID) {
            if (sessionsData.ContainsKey(sessionID)) { sessionsData.Remove(sessionID); }
        }

        public static string GetSessionData(string sessionID, string variable) {
            if (sessionsData.ContainsKey(sessionID)) {
                if(sessionsData[sessionID].ContainsKey(variable)) { return sessionsData[sessionID][variable]; }
            }
            return null;
        }

        public static void SetSessionData(string sessionID, string variable, string value) {
            if (!sessionsData.ContainsKey(sessionID)) { sessionsData.Add(sessionID, new Dictionary<string, string>()); }
            if (!sessionsData[sessionID].ContainsKey(variable)) { sessionsData[sessionID].Add(variable, value); }
            else { sessionsData[sessionID][variable] = value; }
        }

    }

    public class CustomSocket : WebSocketServer { 
    
    }

    public class WebSocketService : WebSocketBehavior
    {
        private string _suffix;

        public WebSocketService() : this(null)
        {
            this.EmitOnPing = true;
        }

        public WebSocketService(string suffix)
        {
            _suffix = suffix ?? String.Empty;
        }

        public void SendMessage(string message) { this.Send(message); }

        protected override void OnClose(CloseEventArgs e)
        {
            Program.DelSession(this.ID);
            base.OnClose(e);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Message received {0}", e.Data);
            JObject ob = JObject.Parse(e.Data);
            string type = "";
            string function = "";
            string datas = "";
            byte[] data = null;
            bool DoEncoding = false;
            try { type = ob.GetValue("type").Value<string>(); } catch (Exception) { }
            try { function = ob.GetValue("function").Value<string>(); } catch (Exception) { }
            string ip = this.Context.Host;
            IWebSocketSession session = this.Sessions[this.ID];

            if (function != "" && function != "GetInfo")
            {
                datas = "{\"type\":\"error\", \"cause\":\"no_encoded_function\", \"function\":\"" + function + "\"}";
                this.Send(datas);
                return;
            }

            if (type == "encoded")
            {
                //Console.WriteLine("ENCODED FRAME");
                try
                {
                    string decoded = Crytography.Decrypt(ob["data"].Value<string>());
                    Console.WriteLine("encoded data = " + decoded);
                    ob = JObject.Parse(decoded);
                }
                catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); }
            }
            string macro = null;
            string sound = null;
            try { macro = ob.GetValue("macro").Value<string>(); } catch (Exception er) { Console.WriteLine(er.Message); }
            try { sound = ob.GetValue("sound").Value<string>(); } catch (Exception er) { Console.WriteLine(er.Message); }
            try { function = ob.GetValue("function").Value<string>(); } catch (Exception) { }

            if (function != "")
            {
                if (function != "Login" && function != "GetInfo")
                {
                    string logued = Program.GetSessionData(this.ID, "loged");
                    if (logued == null || logued == "0")
                    {
                        datas = "{\"type\":\"error\", \"cause\":\"not_loged\"}";
                        this.Send(datas);
                        return;
                    }
                }
                if (function == "GetInfo")
                {
                    if (!Program.HasSession(this.ID))
                    {
                        Program.SetSessionData(this.ID, "addr", this.Context.UserEndPoint.Address.ToString());
                        Program.SetSessionData(this.ID, "port", ""+this.Context.UserEndPoint.Port);
                        Program.SetSessionData(this.ID, "keyPU", ob.GetValue("keyPU").Value<string>());
                        Program.SetSessionData(this.ID, "last", ""+Program.getUnixTimeStamp(false));
                        Program.SetSessionData(this.ID, "loged", "0");
                    }
                    else
                    {
                        Program.SetSessionData(this.ID, "keyPU", ob.GetValue("keyPU").Value<string>());
                        Program.SetSessionData(this.ID, "last", "" + Program.getUnixTimeStamp(false));
                    }
                    DoEncoding = true;
                    datas = Program.GetInfo();
                }

                if (function == "Ping")
                {
                    if (Program.HasSession(this.ID))
                    {
                        Program.SetSessionData(this.ID, "last", "" + Program.getUnixTimeStamp(false));
                        datas = "{\"function\":\"Pong\", \"status\":\"OK\"}";
                        this.Send(datas);
                    }
                    return;
                }

                if (function == "Login")
                {
                    DoEncoding = true;
                    string password = null;
                    try { password = ob.GetValue("password").Value<string>(); } catch (Exception) { }
                    if (password == Program.GridsList["password"].Value<string>())
                    {
                        datas = "{\"function\":\"Login\", \"status\":\"OK\"}";
                        Program.SetSessionData(this.ID, "loged", "1");
                    }
                    else
                    {
                        datas = "{\"function\":\"Login\", \"status\":\"error\"}";
                    }
                }

                if (function == "GetGrids" || function == "ForceReload")
                {
                    if (function == "ForceReload") { Program.LoadRemoteParams(); }
                    string gri = JsonConvert.SerializeObject(Program.GridsList.GetValue("grids"));
                    DoEncoding = true;
                    datas = "{\"function\":\"SendGrids\", \"grids\":" + gri + "}";
                }

                if (function == "GetImages")
                {
                    JArray refs = ob["references"].Value<JArray>();
                    foreach (string refe in refs)
                    {
                        Program.GetImage(refe, this);
                    }
                    return;
                }
                if (function == "GetImage")
                {
                    Program.GetImage(ob["reference"].Value<string>(), this);
                    return;
                }

                if (function == "GetSoundInfo" || function == "MuteSound" || function == "VolUp" || function == "VolDown")
                {
                    JToken tok = ob.Value<JToken>();
                    Program.ParseSoundInfo(function, this, tok);
                }

                if (DoEncoding == true)
                {
                    try
                    {
                        if (Program.HasSession(this.ID))
                        {
                            string key = Program.GetSessionData(this.ID, "keyPU");
                            if (function == "GetInfo") { key = ob.GetValue("keyPU").Value<string>(); }
                            string encoded = "{\"type\":\"encoded\", \"data\":\"" + Crytography.Encrypt(datas, key) + "\"}";
                            this.Send(encoded);
                            Console.WriteLine(encoded);
                        }
                    }
                    catch (Exception err) { Console.WriteLine(JsonConvert.SerializeObject(err)); }
                    return;
                }
                this.Send(datas);
            }
            if (macro != null || sound != null)
            {
                Console.WriteLine(macro);
                Console.WriteLine(sound);
                MacrosProcessing.Run(Program.MacroList, macro, sound.Replace("/", Program.osSep), this);
            }

        }
    }
}
