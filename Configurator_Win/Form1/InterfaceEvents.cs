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
        private void DeclareEvents()
        {
            tabControler.Selected += TabControler_Selected;
            RG02_box.TextChanged += RGboxGrid_TextChanged;
            RG03_box.TextChanged += RGboxGrid_TextChanged;
            RG04_box.TextChanged += RGboxGrid_TextChanged;
            RG05_num.ValueChanged += RG05_num_ValueChanged;

            RG10_box.TextChanged += RGboxBtn_TextChanged;
            RG12_box.TextChanged += RGboxBtn_TextChanged;
            RG13_box.TextChanged += RGboxBtn_TextChanged;
            RG14_box.TextChanged += RGboxBtn_TextChanged;
            RG15_box.TextChanged += RGboxBtn_TextChanged;

            RG11_Num1.ValueChanged += RGNum_ValueChanged;
            RG11_Num2.ValueChanged += RGNum_ValueChanged;
        }

        private void TabControler_Selected(object sender, TabControlEventArgs e)
        {
            if (initialized == true) { UpdateTabInfo(tabControler.SelectedIndex); }
        }

        private void RGboxGrid_TextChanged(object sender, EventArgs e)
        {
            if (tabControler.TabCount > 0)
            {
                string param = "";
                if (((System.Windows.Forms.TextBox)sender).Name == "RG02_box") { param = "name"; }
                if (((System.Windows.Forms.TextBox)sender).Name == "RG03_box") { param = "icon"; }
                if (((System.Windows.Forms.TextBox)sender).Name == "RG04_box") { param = "style"; }
                try
                {
                    if (param == "name")
                    {
                        Debug.WriteLine(RG02_box.Text);
                        if (RG02_box.Text == "") { tabControler.TabPages[tabControler.SelectedIndex].Text = "#"+GridsList["grids"][tabControler.SelectedIndex]["id"].Value<string>(); }
                        else { tabControler.TabPages[tabControler.SelectedIndex].Text = RG02_box.Text; }
                    }
                    ((WebBrowser)tabControler.GetControl(tabControler.SelectedIndex).Controls[0]).Document
                        .InvokeScript("updateParam", new[] { param, ((System.Windows.Forms.TextBox)sender).Text });
                    GridsList["grids"][tabControler.SelectedIndex][param] = ((System.Windows.Forms.TextBox)sender).Text;
                } catch (Exception error) { }
                if (param != "style")
                {
                    if (styleGlobal != null) { styleGlobal.RefreshFrame(); }
                }
                else
                {
                    if (styleBlock != null) { styleBlock.UpdatePane(); }
                }
            }
        }

        private void RG05_num_ValueChanged(object sender, EventArgs e)
        {
            if (tabControler.TabCount > 0)
            {
                try
                {
                    ((WebBrowser)tabControler.GetControl(tabControler.SelectedIndex).Controls[0]).Document
                        .InvokeScript("updateParam", new[] { "width", ((System.Windows.Forms.NumericUpDown)sender).Value.ToString() });

                    GridsList["grids"][tabControler.SelectedIndex]["width"] = ((System.Windows.Forms.TextBox)sender).Text;
                    RG11_Num1.Maximum = ((System.Windows.Forms.NumericUpDown)sender).Value;
                    RG11_Num2.Maximum = ((System.Windows.Forms.NumericUpDown)sender).Value;
                } catch (Exception error) { }
            }
        }

        private void RGboxBtn_TextChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("RGboxBtn_TextChanged");
            if (tabControler.TabCount > 0)
            {
                string id = RG09_box.Text;
                string param = "";
                if (((System.Windows.Forms.TextBox)sender).Name == "RG10_box") { param = "name"; }
                if (((System.Windows.Forms.TextBox)sender).Name == "RG12_box") { param = "icon"; }
                if (((System.Windows.Forms.TextBox)sender).Name == "RG13_box") { param = "style"; }
                if (((System.Windows.Forms.TextBox)sender).Name == "RG14_box") { param = "sound"; }
                if (((System.Windows.Forms.TextBox)sender).Name == "RG15_box") { param = "macro"; }
                int index = 0;
                foreach (JToken tok in GridsList["grids"][tabControler.SelectedIndex]["blocks"].ToList<JToken>())
                {
                    if (tok["id"].Value<string>() == id)
                    {
                        GridsList["grids"][tabControler.SelectedIndex]["blocks"][index][param] = ((System.Windows.Forms.TextBox)sender).Text;
                        break;
                    }
                    index += 1;
                }
                try
                {
                    ((WebBrowser)tabControler.GetControl(tabControler.SelectedIndex).Controls[0]).Document
                        .InvokeScript("updateBlock", new[] { RG09_box.Text, param, ((System.Windows.Forms.TextBox)sender).Text });
                } catch (Exception error) { }

                if (styleBlock != null) { styleBlock.UpdatePane(); }
            }
        }

        private void RGNum_ValueChanged(object sender, EventArgs e)
        {
            if (tabControler.TabCount > 0)
            {
                string id = RG09_box.Text;
                string param = "";
                if (((System.Windows.Forms.NumericUpDown)sender).Name == "RG11_Num1") { param = "width"; }
                if (((System.Windows.Forms.NumericUpDown)sender).Name == "RG11_Num2") { param = "height"; }
                int index = 0;
                foreach (JToken tok in GridsList["grids"][tabControler.SelectedIndex]["blocks"].ToList<JToken>())
                {
                    if (tok["id"].Value<string>() == id)
                    {
                        GridsList["grids"][tabControler.SelectedIndex]["blocks"][index][param] = ((System.Windows.Forms.NumericUpDown)sender).Value;
                        break;
                    }
                    index += 1;
                }
                try {
                    ((WebBrowser)tabControler.GetControl(tabControler.SelectedIndex).Controls[0]).Document
                  .InvokeScript("updateBlock", new[] { id, param, ((System.Windows.Forms.NumericUpDown)sender).Value.ToString() });
                } catch (Exception error) { }
            }
        }



        private void RG01_btnDel_Click(object sender, EventArgs e)
        {
            if (tabControler.Controls.Count <= 1) {
                MessageBox.Show("Impossible d'avoir moins d'une Grille !!!",
                "Attention !", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            DialogResult rez = MessageBox.Show("Souhaitez vous vraiment suppprimer la grille \""+GridsList["grids"][tabControler.SelectedIndex]["name"].Value<string>()+"\" ?", 
                "Attention !", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (rez == System.Windows.Forms.DialogResult.Yes)
            {
                int index = tabControler.SelectedIndex;
                GridsList["grids"][index].Remove();
                tabControler.Controls.RemoveAt(index);
                tabControler.SelectedIndex = index - 1;
                Program.CleanMemery();
            }
        }

        private void RG01_btnAdd_Click(object sender, EventArgs e)
        {
            //JObject tab = new JObject();
            //JObject tab2 = new JObject();
            //tab["id"] = "" + Program.getUnixTimeStamp(true);
            //tab["name"] = "NEW TAB";
            //tab["width"] = 4;
            //tab["icon"] = "";
            //tab["blocks"] = "";
            //tab2["id"] = Guid.NewGuid().ToString();
            //tab2["type"] = "button";
            //tab2["name"] = "New Block";
            //tab2["width"] = 1;
            //tab2["height"] = 1;
            //tab2["icon"] = "";
            //tab2["style"] = "";
            //tab2["sound"] = "";
            //tab2["macro"] = "";

            JObject tab = JObject.Parse("{\"id\":\""+Program.getUnixTimeStamp(true)+"\",\"name\":\"c\"," +
                "\"width\":4," +
                "\"icon\":\"\"," +
                "\"style\":\"\"," +
                "\"blocks\":[{\"id\":\""+ Guid.NewGuid().ToString() + "\",\"type\":\"button\",\"name\":\"New Block\",\"width\":1,\"height\":1,\"icon\":\"\",\"style\":\"\",\"sound\":\"\",\"macro\":\"PlaySword\"}]" +
                "}");
            GridsList["grids"].Last.AddAfterSelf(tab);
            setTabContent(tab);
            tabControler.SelectedIndex = tabControler.Controls.Count - 1;
        }

        private void RG03_btn_Click(object sender, EventArgs e)
        {
            ImagesManager.ImagesManager images = new ImagesManager.ImagesManager(this, "grid");
            images.ShowDialog();
        }

        private void RG04_btn_Click(object sender, EventArgs e)
        {
            if (styleGlobal != null) { return; }
            styleGlobal = new StyleEditor.GlobalStyleEditor(this, "file:///" + (Program.baseDirectory.Replace("\\", "/") + "/html/index.html").Replace("//", "/"));
            styleGlobal.FormClosed += StyleGlobal_FormClosed;
            Point pos = new Point();
            pos.Y = this.Location.Y;
            if (styleBlock != null) { pos.X = styleBlock.Location.X + styleBlock.Width - 10; }
            else { pos.X = this.Location.X + this.Width - 10; }
            styleGlobal.StartPosition = FormStartPosition.Manual;
            styleGlobal.Location = pos;
            styleGlobal.Owner = this;
            styleGlobal.Show();
            styleOrder.Add("general");
        }

        private void StyleGlobal_FormClosed(object sender, FormClosedEventArgs e) {
            styleGlobal = null; styleOrder.Remove("general");
            this.Show();
            if (styleBlock != null)
            {
                Point pos = new Point();
                pos.Y = this.Location.Y;
                pos.X = this.Location.X + this.Width - 10;
                styleBlock.Location = pos;
                styleBlock.Show();
            }
        }

        private void RG6_Click(object sender, EventArgs e)
        {
            try { ((WebBrowser)tabControler.GetControl(tabControler.SelectedIndex).Controls[0]).Document.InvokeScript("newBlock"); } catch (Exception error) { }
        }

        private void RG06_2_Click(object sender, EventArgs e)
        {

        }

        private void RG12_btn_Click(object sender, EventArgs e)
        {
            ImagesManager.ImagesManager images = new ImagesManager.ImagesManager(this, "button");
            images.StartPosition = FormStartPosition.CenterScreen;
            images.ShowDialog();
        }

        private void RG13_btn_Click(object sender, EventArgs e)
        {
            if (styleBlock != null) { return; }
            styleBlock = new StyleEditor.BlockStyleEditor(this, "file:///" + (Program.baseDirectory.Replace("\\", "/") + "/html/index.html").Replace("//", "/"));
            styleBlock.FormClosed += StyleBlock_FormClosed;
            Point pos = new Point();
            pos.Y = this.Location.Y;
            if (styleGlobal != null) { pos.X = styleGlobal.Location.X + styleGlobal.Width - 10; }
            else { pos.X = this.Location.X + this.Width - 10; }
            styleBlock.StartPosition = FormStartPosition.Manual;
            styleBlock.Location = pos;
            styleBlock.Owner = this;
            styleBlock.Show();
            styleOrder.Add("block");
        }

        private void StyleBlock_FormClosed(object sender, FormClosedEventArgs e) {
            styleBlock = null; styleOrder.Remove("block");
            this.Show();
            if (styleGlobal != null) {
                Point pos = new Point();
                pos.Y = this.Location.Y;
                pos.X = this.Location.X + this.Width - 10;
                styleGlobal.Location = pos;
                styleGlobal.Show();
            }
        }

        private void RG14_btn_Click(object sender, EventArgs e)
        {
            SoundsManager.SoundsManager sndm = new SoundsManager.SoundsManager(this);
            sndm.StartPosition = FormStartPosition.CenterScreen;
            sndm.ShowDialog();
        }

        private void RG15_btn_Click(object sender, EventArgs e)
        {
            MacrosManager.MacrosManager mac = new MacrosManager.MacrosManager(this);
            mac.ShowDialog();
            mac.Dispose();
        }

        private void RG16_Click(object sender, EventArgs e)
        {
            string id = RG09_box.Text;
            try { ((WebBrowser)tabControler.GetControl(tabControler.SelectedIndex).Controls[0]).Document.InvokeScript("delBlock", new[] { id }); } catch (Exception error) { }
            int index = 0;
            foreach (JToken tok in GridsList["grids"][tabControler.SelectedIndex]["blocks"].ToList<JToken>()) {
                if (tok["id"].Value<string>() == id) {
                    GridsList["grids"][tabControler.SelectedIndex]["blocks"][index].Remove();
                    break;
                }
                index += 1;
            }
            HideInfoBlock();
            Program.CleanMemery();
        }

        private void RG03_clear_Click(object sender, EventArgs e) { RG03_box.Text = ""; }
        private void RG04_clear_Click(object sender, EventArgs e) { RG04_box.Text = ""; if (styleGlobal != null) { styleGlobal.UpdatePane(); } }
        private void RG12_clear_Click(object sender, EventArgs e) { RG12_box.Text = ""; }
        private void RG13_clear_Click(object sender, EventArgs e) { RG13_box.Text = ""; }
        private void RG14_clear_Click(object sender, EventArgs e) { RG14_box.Text = ""; }
        private void RG15_clear_Click(object sender, EventArgs e) { RG15_box.Text = ""; }

    }
}
