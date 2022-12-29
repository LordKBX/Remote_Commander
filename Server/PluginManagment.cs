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
    public partial class Program
    {
        public static void PluginLoadAll()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(extentionsDir);//Assuming Test is your Folder
                FileInfo[] Files = null;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    Files = d.GetFiles("*.dll"); //Getting dll files
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
            catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); }
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
            catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); return false; }
        }

        public static Dictionary<string, object> PluginGet(string AssemblyName)
        {
            Dictionary<string, object> plug = null;
            Server.Program.extentionsInfosList.TryGetValue(AssemblyName, out plug);
            return plug;
        }



        private static void PluginLoad(FileInfo file)
        {
            try
            {
                Assembly asem = AssemblyLoadContext.Default.LoadFromAssemblyPath(extentionsDir + file.Name);
                IEnumerator<Type> bb = asem.ExportedTypes.GetEnumerator();
                bb.MoveNext();
                Type myType = asem.GetType(bb.Current.Namespace + "." + bb.Current.Name);
                Dictionary<string, object> obj = new Dictionary<string, object>();
                obj["assembly"] = asem;
                obj["assemblyName"] = bb.Current.Namespace + "." + bb.Current.Name;
                obj["path"] = extentionsDir + file.Name;
                obj["type"] = asem.GetType(bb.Current.Namespace + "." + bb.Current.Name);
                obj["instance"] = asem.CreateInstance(bb.Current.Namespace + "." + bb.Current.Name);
                extentionsInfosList.Add(bb.Current.Name, obj);
                Console.WriteLine(bb.Current.Name);
            }
            catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); }
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
            catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); }
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
            catch (Exception error) { Debug.WriteLine(JsonConvert.SerializeObject(error)); }
        }

    }
}
