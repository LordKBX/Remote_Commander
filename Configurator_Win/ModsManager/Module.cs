using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Configurator_Win.ModsManager
{
    [XmlRoot("Module")]
    public class Module
    {
        [XmlElement("Id")]
        public string id { get; set; }
        public string bid { get; set; }
        [XmlElement("MinWidth")]
        public int minWidth { get; set; }
        public int width { get; set; }
        [XmlElement("MinHeight")]
        public int minHeight { get; set; }
        public int height { get; set; }
        [XmlElement("Template")]
        public string template { get; set; }
        public string processedTemplate { get; set; }
        [XmlElement("Style")]
        public string style { get; set; }
        public string processedStyle { get; set; }
        [XmlElement("Vars")]
        public Vars vars { get; set; }
        [XmlElement("Declares")]
        public Declares declares { get; set; }
        [XmlElement("Scriptxs")]
        public Scriptxs scriptxs { get; set; }

        public FileInfo fileInfo = null;

        public static Module Parse(string fileName)
        {
            if (File.Exists(fileName))
            {
                Module returnObject = null;
                if (string.IsNullOrEmpty(fileName)) return null;

                try
                {
                    StreamReader xmlStream = new StreamReader(fileName);
                    XmlSerializer serializer = new XmlSerializer(typeof(Module));
                    returnObject = (Module)serializer.Deserialize(xmlStream);
                    returnObject.fileInfo = new FileInfo(fileName);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    Debug.Write(ex.StackTrace);
                }
                return returnObject;
            }

            return null;
        }

        public bool processSync() { var task = process(); task.Wait(); return task.Result; }

        public bool processSync(int width, int height) { this.width = width; this.height = height; return processSync(); }

        public async Task<bool> process() {
            processedTemplate = template;
            processedStyle = style;
            try
            {
                processedTemplate = processedTemplate.Replace("%mid%", id);
                processedTemplate = processedTemplate.Replace("%bmid%", bid);
                processedStyle = processedStyle.Replace("%mid%", id);
                processedStyle = processedStyle.Replace("%bmid%", bid);

                foreach (Var vari in this.vars.var)
                {
                    if (vari.eval.ToLower() != "true")
                    {
                        processedTemplate = processedTemplate.Replace("%" + vari.name + "%", vari.value);
                        processedStyle = processedStyle.Replace("%" + vari.name + "%", vari.value);
                    }
                    else
                    {
                        var rez = await CSharpScript.EvaluateAsync("int ModWidth = " + width + "; int ModHeight = " + height + "; " + vari.value);
                        processedTemplate = processedTemplate.Replace("%" + vari.name + "%", rez.ToString());
                        processedStyle = processedStyle.Replace("%" + vari.name + "%", rez.ToString());
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string toString() { return "Module{\n\t\"Id\":\"" + id + "\",\n\t\"MinWidth\":" + minWidth + ",\n\t\"MinHeight\":" + minHeight + ",\n\t\"Template\":\"" + template.Replace("\r", "").Replace("\n", "").Replace("\t", "") + "\",\n\t\"Style\":\"" + style.Replace("\r", "").Replace("\n", "").Replace("\t", "") + "\",\n\t\"Vars\": " + vars.ToString() + ",\n\t\"Scriptxs\": " + scriptxs.ToString() + "\n}"; }
        public override string ToString() { return toString(); }
    }

    public class VList<T> : List<T>
    {
        public string toString()
        {
            string st = "";
            for (int i = 0; i < Count; i++)
            {
                if (i > 0) st += ", ";
                st += this[i].ToString();
            }
            st += "";
            return st;
        }
        public override string ToString() { return toString(); }
    }

    public class Vars
    {
        [XmlElement("Var")]
        public VList<Var> var { get; set; }

        public string toString() { return "Vars{\"Var\":" + var.ToString() + "}"; }
        public override string ToString() { return toString(); }
    }

    public class Var
    {
        [XmlAttribute("name")]
        public string name = "";
        [XmlAttribute("eval")]
        public string eval = "";
        [XmlText()]
        public string value = "";

        public string toString() { return "Var{\"name\":\"" + name + "\", \"eval\":\"" + eval + "\", \"value\":\"" + value.Replace("\r", "").Replace("\n", "").Replace("\t", "") + "\"}"; }
        public override string ToString() { return toString(); }
    }

    public class Declares
    {
        [XmlElement("Declare")]
        public VList<Declare> declare { get; set; }

        public string toString() { return "Declares{\"Var\":" + declare.ToString() + "}"; }
        public override string ToString() { return toString(); }
    }

    public class Declare
    {
        [XmlText()]
        public string value = "";

        public string toString() { return "Declare{\"value\":\"" + value.Replace("\r", "").Replace("\n", "").Replace("\t", "") + "\"}"; }
        public override string ToString() { return toString(); }
    }

    public class Scriptxs
    {
        [XmlElement("Scriptx")]
        public VList<Scriptx> scriptx { get; set; }

        public string toString() { return "Scriptxs{\"Scriptx\":" + scriptx.ToString() + "}"; }
        public override string ToString() { return toString(); }
    }

    public class Scriptx
    {
        [XmlAttribute("sources")]
        public string sources = "";
        [XmlText()]
        public string value = "";

        public string toString() { return "Scriptx{\"sources\":\"" + sources + "\", \"value\":\"" + value.Replace("\r", "").Replace("\n", "").Replace("\t", "") + "\"}"; }
        public override string ToString() { return toString(); }
    }
}
