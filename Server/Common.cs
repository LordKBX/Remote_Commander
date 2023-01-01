using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Reflection;

namespace Server
{
    public partial class Program
    {
        public static Double getUnixTimeStamp(bool InMilliseconds = false)
        {
            if (InMilliseconds == false) { return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; }
            else { return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds; }
        }

        [Conditional("DEBUG")]
        private static void isDebugF() { 
            IsDebug = true; 
        }

        private static void IsAdmin() {
            return;
            if (IsDebug == false) // si non mode debug, non admin fermée
            {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    if (pricipal.IsInRole(WindowsBuiltInRole.Administrator) == false)
                    {
                        // Environment.Exit(0); 
                    }
                    else {
                        Dictionary<string, object> plug = PluginGet("TrayIcon");
                        if (plug == null) { return; }
                        Assembly asem = (Assembly)plug["assembly"];
                        Type t = (Type)plug["type"];
                        object instance = (object)plug["instance"];

                        MethodInfo m = t.GetMethod("HideConsole");
                        m.Invoke(instance, new object[] { });
                    }
                }
            }
        }

        private static void HideWindow() {
            Dictionary<string, object> plug = Server.Program.PluginGet("TrayIcon");
            if (plug == null) { return; }
            try
            {
                Assembly asem = (Assembly)plug["assembly"];
                Type t = (Type)plug["type"];
                object instance = (object)plug["instance"];

                MethodInfo m = t.GetMethod("HideConsole");
                Console.WriteLine("m.Invoke(instance, new object[] {});");
                m.Invoke(instance, new object[] { });
            }
            catch (Exception) { }
        }

        private static void ShowWindow() {
            Dictionary<string, object> plug = Server.Program.PluginGet("TrayIcon");
            if (plug == null) { return; }
            try
            {
                Assembly asem = (Assembly)plug["assembly"];
                Type t = (Type)plug["type"];
                object instance = (object)plug["instance"];

                MethodInfo m = t.GetMethod("ShowConsole");
                Console.WriteLine("m.Invoke(instance, new object[] {});");
                m.Invoke(instance, new object[] { });
            }
            catch (Exception) { }
        }
    }
}