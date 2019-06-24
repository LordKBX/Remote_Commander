using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Configurator_Win
{
    public partial class Form1 : Form
    {

        public void notify(string data)
        {
            Debug.WriteLine("JSNotify >> " + data);
            try
            {
                JObject obj = JObject.Parse(data);
            }
            catch (Exception error) { }
        }

        public void UpdateBouttonInfo(string id)
        {
            JToken tabsT = GridsList.GetValue("grids");
            List<JToken> tabs = tabsT.ToList<JToken>();
            List<JToken> btns = tabs[tabControler.SelectedIndex]["buttons"].ToList<JToken>();
            foreach (JToken btn in btns)
            {
                if (btn["id"].Value<string>() == id)
                {
                    ShowInfoButton();
                    RG09_box.Text = id;
                    RG10_box.Text = btn["name"].Value<string>();
                    RG11_Num1.Value = btn["width"].Value<int>();
                    RG11_Num2.Value = btn["height"].Value<int>();
                    RG12_box.Text = btn["icon"].Value<string>();
                    RG13_box.Text = btn["style"].Value<string>();
                    RG14_box.Text = btn["sound"].Value<string>();
                    RG15_box.Text = btn["macro"].Value<string>();
                }
            }
        }

        public void AddBouttonInfo(string data)
        {
            Debug.WriteLine("AddBouttonInfo >> " + data);
            JObject obj = JObject.Parse(data);
            JToken tabsT = GridsList.GetValue("grids");
            ((JToken)(GridsList["grids"][tabControler.SelectedIndex]["buttons"])).Children().Last<JToken>().AddAfterSelf(obj);
        }

        public void UpdateButtonsOrder(string data)
        {
            Debug.WriteLine("NewOrder >> " + data);
            string[] list = data.Split(';');
            JToken[] tokl = new JToken[list.Length];
            

            for (int i = 0; i < list.Length; i++) {
                JToken rez = getObjectButton(list[i]);
                if (rez != null) { tokl[i] = rez; }
            }
            if (tokl.Length > 0) {
                string re = JsonConvert.SerializeObject(tokl);
                Debug.WriteLine("NewOrder >> " + re);
                GridsList["grids"][tabControler.SelectedIndex]["buttons"] = JToken.FromObject(tokl);
            }
            /*
            Debug.WriteLine("AddBouttonInfo >> " + data);
            JObject obj = JObject.Parse(data);
            JToken tabsT = GridsList.GetValue("grids");
            ((JToken)(GridsList["grids"][tabControler.SelectedIndex]["buttons"])).Children().Last<JToken>().AddAfterSelf(obj);
            */
        }
        private JToken getObjectButton(string id) {
            foreach (JToken tok in GridsList["grids"][tabControler.SelectedIndex]["buttons"].ToList<JToken>())
            {
                if (tok["id"].Value<string>() == id)
                {
                    return tok;
                }
            }
            return null;
        }
    }
}
