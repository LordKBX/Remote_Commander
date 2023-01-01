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

        public bool IsLegacyGrid(JToken tabsG) {
            var dic = tabsG.ToObject<Dictionary<string, object>>();
            object jj = null;
            dic.TryGetValue("buttons", out jj);
            if (dic.ContainsKey("buttons") && jj != null) { return true; }
            return false;
        }

        public void UpdateBlockInfo(string id)
        {
            JToken tabsT = GridsList.GetValue("grids");
            List<JToken> tabs = tabsT.ToList<JToken>();
            var dic = tabs[tabControler.SelectedIndex].ToObject<Dictionary<string, object>>();
            List<JToken> btns = null;
            
            btns = tabs[tabControler.SelectedIndex]["blocks"].ToList<JToken>();
            foreach (JToken btn in btns)
            {
                if (btn["id"].Value<string>() == id)
                {
                    ShowInfoBlock();
                    RG09_box.Text = id;
                    RG09_2_comboBox.SelectedIndex = ((btn["type"].Value<string>() == "module")?1:0);
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

        public void AddBlockInfo(string data)
        {
            Debug.WriteLine("AddBlockInfo >> " + data);
            JObject obj = JObject.Parse(data);
            JToken tabsT = GridsList.GetValue("grids");
            ((JToken)(GridsList["grids"][tabControler.SelectedIndex]["blocks"])).Children().Last<JToken>().AddAfterSelf(obj);
        }

        public void WriteLine(string data) { Console.WriteLine(data); }

        public void UpdateBlocksOrder(string data)
        {
            Debug.WriteLine("NewOrder >> " + data);
            string[] list = data.Split(';');
            JToken[] tokl = new JToken[list.Length];


            for (int i = 0; i < list.Length; i++) {
                JToken rez = getObjectBlock(list[i]);
                if (rez != null) { tokl[i] = rez; }
            }
            if (tokl.Length > 0) {
                string re = JsonConvert.SerializeObject(tokl);
                Debug.WriteLine("NewOrder >> " + re);
                GridsList["grids"][tabControler.SelectedIndex]["blocks"] = JToken.FromObject(tokl);
            }
            /*
            Debug.WriteLine("AddBlockInfo >> " + data);
            JObject obj = JObject.Parse(data);
            JToken tabsT = GridsList.GetValue("grids");
            ((JToken)(GridsList["grids"][tabControler.SelectedIndex]["buttons"])).Children().Last<JToken>().AddAfterSelf(obj);
            */
        }
        private JToken getObjectBlock(string id)
        {
            foreach (JToken tok in GridsList["grids"][tabControler.SelectedIndex]["blocks"].ToList<JToken>())
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
