using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Loader;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    public static partial class Program
    {
        public static void PluginLoadAll()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(extentionsDir);//Assuming Test is your Folder
                FileInfo[] Files = null;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    Files = d.GetFiles("*_*.dll"); //Getting dll files
                }

                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {

                }

                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    Files = d.GetFiles("*.so"); //Getting so files
                }

                foreach (FileInfo file in Files) { PluginLoad(file); }

            }
            catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); }
        }

        public static bool PluginLoadFromFile(string folder, string stringfile)
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(folder);//Assuming Test is your Folder
                FileInfo[] Files = null;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    if (stringfile.EndsWith(".dll") == false && stringfile.EndsWith(".DLL") == false) { return false; }
                    Files = d.GetFiles(stringfile); //Getting dll files
                }

                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {

                }

                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    if (stringfile.EndsWith(".so") == false && stringfile.EndsWith(".SO") == false) { return false; }
                    Files = d.GetFiles(stringfile); //Getting so files
                }

                PluginLoad(Files[0]);
                return true; ;
            }
            catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); return false; }
        }

        public static Dictionary<string, object> PluginGet(string AssemblyName)
        {
            Dictionary<string, object> plug = null;
            Server.Program.extentionsInfosList.TryGetValue(AssemblyName, out plug);
            return plug;
        }

        public static Dictionary<string, object> SourceGet(string AssemblyName)
        {
            Dictionary<string, object> plug = null;
            Server.Program.sourcesInfosList.TryGetValue(AssemblyName, out plug);
            return plug;
        }

        public static object PluginGetValue(this MemberInfo memberInfo)
        {
            object obj = null;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(obj);
                default:
                    return obj;
            }
        }

        private static void PluginLoad(FileInfo file)
        {
            try
            {
                bool IsSource = false;
                string name = null;
                Dictionary<string, string> interfaces = null;
                Console.WriteLine("load plugin : "+ extentionsDir + file.Name);
                Assembly asem = AssemblyLoadContext.Default.LoadFromAssemblyPath(extentionsDir + file.Name);
                IEnumerator<Type> bb = asem.ExportedTypes.GetEnumerator();
                bb.MoveNext();
                if (bb.Current.Name == "ExtType")
                {
                    var instance = asem.CreateInstance(bb.Current.Namespace + "." + bb.Current.Name);
                    Type mType = asem.GetType(bb.Current.Namespace + "." + bb.Current.Name);
                    string stype = (string)PluginGetValue(mType.GetMember("Type")[0]);
                    if (stype == "Source")
                    {
                        IsSource = true;
                        name = (string)PluginGetValue(mType.GetMember("Name")[0]);
                        interfaces = (Dictionary<string, string>)PluginGetValue(mType.GetMember("Interfaces")[0]);
                    }
                    bb.MoveNext();
                }
                Type myType = asem.GetType(bb.Current.Namespace + "." + bb.Current.Name);
                Dictionary<string, object> obj = new Dictionary<string, object>();
                obj["assembly"] = asem;
                obj["assemblyName"] = bb.Current.Namespace + "." + bb.Current.Name;
                obj["path"] = extentionsDir + file.Name;
                obj["type"] = myType;
                obj["instance"] = asem.CreateInstance(bb.Current.Namespace + "." + bb.Current.Name);
                obj["interfaces"] = interfaces;
                if (IsSource == false) { 
                    extentionsInfosList.Add(bb.Current.Name, obj);
                    Console.WriteLine("Extention " + bb.Current.Name + " is OK");
                }
                else { 
                    if(interfaces.ContainsKey("Init"))
                    {
                        Type t = (Type)obj["type"];
                        MethodInfo m = t.GetMethod("Init");
                        m.Invoke(obj["instance"], new object[] { });
                    }
                    sourcesInfosList.Add((IsSource) ? name : bb.Current.Name, obj);
                    Console.WriteLine("Source " + bb.Current.Name + " is OK");
                }
            }
            catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); }
        }

        private static void PluginDisposeAll()
        {
            try
            {
                foreach (Dictionary<string, object> plug in extentionsInfosList.Values) {
                    plug["type"] = null;
                    plug["instance"] = null;
                }
                extentionsInfosList.Clear();
            }
            catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); }
        }

        public static void PluginDispose(string AssemblyName)
        {
            try
            {
                Dictionary<string, object> plug = PluginGet(AssemblyName);
                plug["type"] = null;
                plug["instance"] = null;
                extentionsInfosList.Remove(AssemblyName);
            }
            catch (Exception error) { Console.WriteLine(JsonConvert.SerializeObject(error)); }
        }

    }
}
