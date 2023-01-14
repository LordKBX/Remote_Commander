using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Configurator_Win
{
    public partial class Form1 : Form
    {
        private void loadGrids()
        {
            string macrofile = System.IO.File.ReadAllText(Program.configDirectory + "\\macros.json");
            string gridsfile = System.IO.File.ReadAllText(Program.configDirectory + "\\grids.json");
            MacroList = JObject.Parse(macrofile);
            GridsList = JObject.Parse(gridsfile);
            tabControler.Controls.RemoveAt(tabControler.SelectedIndex);
            if (!GridsList.ContainsKey("orientation")) { GridsList["orientation"] = "vertical"; IsVertical = true; OrientationComboBox.SelectedIndex = 0; }
            if (GridsList["orientation"].Value<string>() != "vertical" && GridsList["orientation"].Value<string>() != "horizontal") { GridsList["orientation"] = "vertical"; IsVertical = true; OrientationComboBox.SelectedIndex = 0; }
            if (GridsList["orientation"].Value<string>() == "horizontal") { IsVertical = false; OrientationComboBox.SelectedIndex = 1; }
            OrientationComboBox_SelectedIndexChanged(null, null);
            JToken tabsT = GridsList.GetValue("grids");
            List<JToken> tabs = tabsT.ToList<JToken>();
            int index = 0;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (IsLegacyGrid(tabs[i]))// Conversion old file format to new format
                {
                    GridsList["grids"][i]["blocks"] = GridsList["grids"][i]["buttons"].DeepClone();
                    GridsList["grids"][i]["buttons"] = null;
                    List<JToken> tabs2 = GridsList["grids"][i]["blocks"].ToList<JToken>();
                    for (int j = 0; j < tabs2.Count; j++)
                    {
                        GridsList["grids"][i]["blocks"][j]["type"] = "button";
                    }
                }
                setTabContent(GridsList["grids"][i]);
            }
            UpdateTabInfo(0);
            HideInfoBlock();
        }

        private void setTabContent(JToken tab) {
            Debug.WriteLine(JsonConvert.SerializeObject(tab));
            TabPage tabObj = new TabPage();
            if (tab["name"].Value<string>() == "") { tabObj.Text = "#" + tab["id"].Value<string>(); }
            else { tabObj.Text = tab["name"].Value<string>(); }
            tabObj.Tag = tab["id"].Value<string>();
            WebBrowser webObj = new WebBrowser();
            webObj.Tag = tab["id"].Value<string>();
            Uri muri = new Uri("file:///" + (Program.baseDirectory.Replace("\\", "/") + "/html/index.html").Replace("//", "/"));
            webObj.Url = muri;
            webObj.ObjectForScripting = this;
            webObj.DocumentCompleted += WebObj_DocumentCompleted1;
            //webObj.Navigated += WebObj_Navigated;
            //webObj.ProgressChanged += WebObj_ProgressChanged;
            webObj.Dock = DockStyle.Fill;
            Control ctrl = ((Control)(webObj));
            Padding marg1 = ((Padding)(ctrl.Margin));
            Padding marg12 = ((Padding)(ctrl.Padding));
            marg1.All = 0;
            marg12.All = 0;
            tabObj.Controls.Add(webObj);
            Control ctrl2 = ((Control)(tabObj));
            Padding marg2 = ((Padding)(ctrl.Margin));
            Padding marg22 = ((Padding)(ctrl.Padding));
            marg2.All = 0;
            marg22.All = 0;
            tabControler.TabPages.Add(tabObj);
        }
        
        private void WebObj_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e) { WebObj_DocumentCompleted1(sender, null); }

        private void WebObj_Navigated(object sender, WebBrowserNavigatedEventArgs e) { WebObj_DocumentCompleted1(sender, null); }

        private void WebObj_DocumentCompleted1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser web = ((WebBrowser)sender);
            string tag = (string)(web.Tag);
            JToken tabsT = GridsList.GetValue("grids");
            List<JToken> tabs = tabsT.ToList<JToken>();
            string end = "";
            int index = 0;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i]["id"].Value<string>() == tag)
                {
                    end = JsonConvert.SerializeObject(tabs[i]);
                }
            }
            try { 
                web.Document.InvokeScript("injectData", new[] { end, (Program.configDirectory + "\\Images\\").Replace("\\", "/") });
                web.Document.InvokeScript("rotateUpdate", new[] { "" });
            } 
            catch (Exception error) { }
        }

        private void UpdateTabInfo(int index)
        {
            Debug.WriteLine("Index Page >> " + index);
            JToken tabsT = GridsList.GetValue("grids");
            List<JToken> tabs = tabsT.ToList<JToken>();
            RG02_box.Text = tabs[index]["name"].Value<string>();
            RG03_box.Text = tabs[index]["icon"].Value<string>();
            RG04_box.Text = tabs[index]["style"].Value<string>();
            RG05_num.Value = tabs[index]["width"].Value<int>();
            HideInfoBlock();
            for (int i = 0; i < tabControler.Controls.Count; i++)
            {
                try { ((WebBrowser)tabControler.Controls[i].Controls[0]).Document.InvokeScript("UnselectAll"); } catch (Exception error) { }
            }
        }
    }
}
