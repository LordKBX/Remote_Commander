using Microsoft.Extensions.Logging;
using SharpDX.DirectSound;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
using System.Timers;
using System.Linq;
using System.Reflection;
using static System.Net.WebRequestMethods;
using Pluralsight.Crypto;

namespace Server
{
    public static partial class Program
    {
        private static ILogger _logger;
        private static ILoggerFactory _loggerFactory;
        private static WebSocketServer socket;
        private static Dictionary<string, Dictionary<string, string>> sessionsData;
        public static string lastSoundInfo = "";
        private static System.Timers.Timer aTimer;
        public static int SocketMaxTick = 0;

        private static void InitWebsocket() {
            CryptContext ctx = new CryptContext();
            ctx.Open();
            X509Certificate2 cert = ctx.CreateSelfSignedCertificate(
            new SelfSignedCertProperties
            {
                IsPrivateKeyExportable = true,
                KeyBitLength = 4096,
                Name = new X500DistinguishedName("cn=*"),
                ValidFrom = DateTime.Today.AddDays(-1),
                ValidTo = DateTime.Today.AddYears(1),
            });
            ctx.Dispose();

            sessionsData = new Dictionary<string, Dictionary<string, string>>();
            socket = new WebSocketServer(IPAddress.Any, Program.listhendPort, true);
            socket.SslConfiguration.ServerCertificate = cert;
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

    public class WebSocketService : WebSocketBehavior
    {
        private string _suffix;
        private System.Timers.Timer aTimer;
        private double lastTime = 0;
        private int tick = 0;

        public WebSocketService() : this(null)
        {
            this.EmitOnPing = true;
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += this.OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public WebSocketService(string suffix)
        {
            _suffix = suffix ?? String.Empty;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            double rez = Program.getUnixTimeStamp() - lastTime;
            //Console.WriteLine("rez = " + rez);
            if (rez >= 15 && lastTime != 0) {//if last ping >= 15 secondes then kill socket
                Console.WriteLine("Session Expired");
                aTimer.Close();
                this.Context.WebSocket.Close();
                try { Sessions.CloseSession(ID); } catch (Exception) { }
                return;
            }

            if (Program.GridsList.ContainsKey("sources"))
            {
                List<JToken> sources = Program.GridsList["sources"].ToList<JToken>();
                //Console.WriteLine("sources.Count = " + sources.Count);
                for (int mod = 0; mod < sources.Count; mod++) {
                    string moduleName = sources[mod]["name"].Value<string>();
                    List<JToken> interfaces = sources[mod]["interfaces"].ToList<JToken>();

                    for (int itfs = 0; itfs < interfaces.Count; itfs++) {
                        int interval = interfaces[itfs]["interval"].Value<int>();
                        string minterfaceName = interfaces[itfs]["name"].Value<string>();
                        if (tick % interval == 0)
                        {
                            Dictionary<string, object> plug = Program.SourceGet(moduleName);
                            if (plug != null)
                            {
                                Dictionary<string, string> pinterfaces = (Dictionary<string, string>)plug["interfaces"];
                                JObject obf = new JObject();
                                obf["function"] = moduleName + "." + minterfaceName;
                                string rtype = null;
                                object ret = null;
                                if (pinterfaces.ContainsKey(minterfaceName))
                                {
                                    try
                                    {
                                        rtype = pinterfaces[minterfaceName];
                                        Assembly asem = (Assembly)plug["assembly"];
                                        Type t = (Type)plug["type"];
                                        object instance = (object)plug["instance"];
                                        MethodInfo m = t.GetMethod(minterfaceName);
                                        ret = m.Invoke(instance, new object[] { });

                                        if (ret.GetType().Name == "String") {
                                            if (rtype == "json") {
                                                try { obf["data"] = JObject.Parse((string)ret); }
                                                catch (Exception) { obf["data"] = (string)ret; }
                                            }
                                            else { obf["data"] = (string)ret; }
                                        }
                                        else if (ret.GetType().Name == "Boolean") { obf["data"] = (bool)ret; }
                                        else if (ret.GetType().Name == "Double") { obf["data"] = (double)ret; }
                                        else if (ret.GetType().Name == "Float") { obf["data"] = (float)ret; }
                                        else if (ret.GetType().Name == "Task`1")
                                        {
                                            ((Task)ret).Wait();
                                            if (rtype == "string") { obf["data"] = ((Task<string>)ret).Result; }
                                            else if (rtype == "json") { obf["data"] = JObject.Parse(((Task<string>)ret).Result); }
                                            else
                                            {
                                                obf["data"] = "" + ((Task<object>)ret).Result.ToString();
                                            }
                                        }
                                        else
                                        {
                                            obf["data"] = "" + ret.ToString();
                                        }
                                        this.Send(JsonConvert.SerializeObject(obf));
                                    }
                                    catch (Exception er)
                                    {
                                        //Console.WriteLine("---------------------");
                                        //Console.WriteLine(er.Message); 
                                        //Console.WriteLine(er.StackTrace);
                                        //Console.WriteLine(moduleName);
                                        //Console.WriteLine(minterfaceName);
                                        //Console.WriteLine("rtype = " + rtype);
                                        //Console.WriteLine("ret.GetType().Name = " + ((ret == null)?"null":ret.GetType().Name));
                                        obf["data"] = "Error";
                                        try { this.Send(JsonConvert.SerializeObject(obf)); } catch(Exception) { 
                                            this.aTimer.Stop(); 
                                            this.Context.WebSocket.Close();
                                            try { Sessions.CloseSession(ID); } catch (Exception erx) { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //this.Send(JsonConvert.SerializeObject(Program.GetSoundInfo((tick == 5)?true:false)));
            tick = (tick >= Program.SocketMaxTick) ? 0 : tick + 1;
        }

        public void SendMessage(string message) { this.Send(message); }

        protected override void OnClose(CloseEventArgs e)
        {
            aTimer.Close();
            Program.DelSession(this.ID);
            base.OnClose(e);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Exception.Message);
            Console.WriteLine(e.Exception.StackTrace);
            base.OnError(e);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                if (e.Data == null) { return; }
                if (e.Data.Trim() == "") { return; }
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

                //if (function != "" && function != "GetInfo")
                //{
                //    datas = "{\"type\":\"error\", \"cause\":\"no_encoded_function\", \"function\":\"" + function + "\"}";
                //    this.Send(datas);
                //    return;
                //}

                if (type == "encoded")
                {
                    //Console.WriteLine("ENCODED FRAME");
                    try
                    {
                        string decoded = Crytography.Decrypt(ob["data"].Value<string>());
                        //Console.WriteLine("encoded data = " + decoded);
                        ob = JObject.Parse(decoded);
                    }
                    catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); }
                }
                string macro = null;
                string sound = null;
                try { macro = ob.GetValue("macro").Value<string>(); } catch (Exception) {  }
                try { sound = ob.GetValue("sound").Value<string>(); } catch (Exception) {  }
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
                        lastTime = Program.getUnixTimeStamp();
                    }

                    if (function == "Ping" || function == "Pong")
                    {
                        //Console.WriteLine("Ping");
                        lastTime = Program.getUnixTimeStamp();
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
                        string orientation = Program.GridsList["orientation"].Value<string>();
                        string mods = JsonConvert.SerializeObject(Program.ModsList);
                        //DoEncoding = true;
                        datas = "{\"function\":\"SendGrids\", \"grids\":" + gri + ", \"mods\":" + mods + ", \"orientation\":\"" + orientation + "\"}";
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

                    if (function == "Source")
                    {
                        string module = null;
                        string method = null;
                        Console.WriteLine(ob.ToString());
                        try { module = ob.GetValue("module").Value<string>(); } catch (Exception) { }
                        try { method = ob.GetValue("method").Value<string>(); } catch (Exception) { }
                        if (module == null || method == null) { Console.WriteLine("Call Source: incomplete"); }
                        Dictionary<string, object> plug = Program.SourceGet(module);
                        if (plug == null) { Console.WriteLine("Call Source: the module do not exit"); }
                        else {
                            Dictionary<string, string> pinterfaces = (Dictionary<string, string>)plug["interfaces"];
                            string rtype = "";
                            object ret = null;
                            JObject obf = new JObject();
                            obf["function"] = module + "." + method;

                            if (!pinterfaces.ContainsKey(method)) { Console.WriteLine("Call Source: the method do not exit"); }
                            else {
                                try
                                {
                                    //Console.WriteLine("---------------------");
                                    //Console.WriteLine(method);
                                    rtype = pinterfaces[method];
                                    //Console.WriteLine("rtype = " + rtype);
                                    Assembly asem = (Assembly)plug["assembly"];
                                    Type t = (Type)plug["type"];
                                    object instance = (object)plug["instance"];
                                    MethodInfo m = t.GetMethod(method);
                                    ParameterInfo[] parameters = m.GetParameters();
                                    if (parameters.Length == 0) { ret = m.Invoke(instance, new object[] { }); }
                                    else {
                                        List<object> lip = new List<object>();
                                        int linep = 0;
                                        foreach (ParameterInfo p in parameters)
                                        {
                                            Console.WriteLine(p.ParameterType.Namespace);
                                            if(p.ParameterType.Name == "String")
                                            try { lip.Add(ob.GetValue("data"+((linep==0)?"":""+linep)).Value<string>()); } catch (Exception) { }
                                            if(p.ParameterType.Name == "Float")
                                            try { lip.Add(ob.GetValue("data"+((linep==0)?"":""+linep)).Value<float>()); } catch (Exception) { }
                                            if(p.ParameterType.Name == "Single")
                                            try { lip.Add(ob.GetValue("data"+((linep==0)?"":""+linep)).Value<float>()); } catch (Exception) { }
                                            if(p.ParameterType.Name == "Boolean")
                                            try { lip.Add(ob.GetValue("data"+((linep==0)?"":""+linep)).Value<bool>()); } catch (Exception) { }
                                        }
                                        ret = m.Invoke(instance, lip.ToArray());
                                    }

                                    if (ret == null) { }
                                    else
                                    {
                                        //Console.WriteLine("ret.GetType().Name = " + ret.GetType().Name);
                                        if (ret.GetType().Name == "String")
                                        {
                                            if (rtype == "json") { obf["data"] = JObject.Parse((string)ret); }
                                            else { obf["data"] = (string)ret; }
                                        }
                                        else if (ret.GetType().Name == "Boolean") { obf["data"] = (bool)ret; }
                                        else if (ret.GetType().Name == "Double") { obf["data"] = (double)ret; }
                                        else if (ret.GetType().Name == "Float") { obf["data"] = (float)ret; }
                                        else if (ret.GetType().Name == "Task`1")
                                        {
                                            ((Task)ret).Wait();
                                            if (rtype == "string") { obf["data"] = ((Task<string>)ret).Result; }
                                            else if (rtype == "json") { obf["data"] = JObject.Parse(((Task<string>)ret).Result); }
                                            else
                                            {
                                                obf["data"] = "" + ((Task<object>)ret).Result.ToString();
                                            }
                                        }
                                        else
                                        {
                                            obf["data"] = "" + ret.ToString();
                                        }
                                        this.Send(JsonConvert.SerializeObject(obf));
                                    }
                                }
                                catch (Exception er)
                                {
                                    Console.WriteLine("---------------------");
                                    Console.WriteLine(er.Message);
                                    Console.WriteLine(er.StackTrace);
                                    Console.WriteLine(module);
                                    Console.WriteLine(method);
                                    Console.WriteLine("rtype = " + rtype);
                                    Console.WriteLine("ret.GetType().Name = " + ret.GetType().Name);
                                    obf["data"] = "Error";
                                    try { this.Send(JsonConvert.SerializeObject(obf)); }
                                    catch (Exception)
                                    {
                                        this.aTimer.Stop();
                                        this.Context.WebSocket.Close();
                                        try { Sessions.CloseSession(ID); } catch (Exception erx) { }
                                    }
                                }
                            }
                        }
                    }

                    if (DoEncoding == true)
                    {
                        try
                        {
                            if (Program.HasSession(this.ID))
                            {
                                string key = Program.GetSessionData(this.ID, "keyPU");
                                if (function == "GetInfo") { key = ob.GetValue("keyPU").Value<string>(); }
                                string coded = Crytography.Encrypt(datas, key);
                                string encoded = "{\"type\":\"encoded\", \"data\":\"" + coded + "\"}";
                                this.Send(encoded);
                                //Console.WriteLine("key = " + key);
                                //Console.WriteLine("decoded = " + Crytography.Decrypt(coded, key));
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
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.WriteLine(ex.StackTrace); }
        }
    }
}
