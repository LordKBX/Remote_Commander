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
    [ComVisible(true)]
    public partial class Form1 : Form
    {
        private JObject MacroList;
        private JObject GridsList;
        private bool initialized = false;
        private StyleEditor.GlobalStyleEditor styleGlobal = null;
        private StyleEditor.ButtonStyleEditor styleButton = null;
        private List<string> styleOrder = new List<string>();
        
        public Form1()
        {
            InitializeComponent();
            this.Size = this.MinimumSize;
            loadGrids();
            DeclareEvents();
            this.Resize += Form1_Resize;
            initialized = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (styleGlobal != null) { styleGlobal.WindowState = FormWindowState.Minimized; }
                if (styleButton != null) { styleButton.WindowState = FormWindowState.Minimized; }
            }
            if (WindowState == FormWindowState.Normal)
            {
                if (styleGlobal != null) { styleGlobal.WindowState = FormWindowState.Normal; }
                if (styleButton != null) { styleButton.WindowState = FormWindowState.Normal; }
            }
        }

        public Int32 getUnixTimeStamp() {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void HideInfoButton()
        {
            RG08.Hide(); RG09.Hide(); RG10.Hide(); RG11.Hide();
            RG12.Hide(); RG13.Hide(); RG14.Hide(); RG15.Hide(); RG16.Hide();
        }
        public void ShowInfoButton()
        {
            RG08.Show(); RG09.Show(); RG10.Show(); RG11.Show();
            RG12.Show(); RG13.Show(); RG14.Show(); RG15.Show(); RG16.Show();
        }
        public void SetGridIcon(string imgUrl)
        {
            RG03_box.Text = imgUrl;
            RGboxGrid_TextChanged(RG03_box, null);
        }
        public void SetButtonIcon(string imgUrl)
        {
            RG12_box.Text = imgUrl;
            RGboxBtn_TextChanged(RG12_box, null);
        }
        public string GetGridStyle()
        {
            return RG04_box.Text;
        }
        public string GetGridIcon()
        {
            return RG03_box.Text;
        }
        public string GetGridName()
        {
            return RG02_box.Text;
        }
        public void SetGridStyle(string style)
        {
            RG04_box.Text = style;
            RGboxGrid_TextChanged(RG04_box, null);
        }
        public string GetButtonStyle()
        {
            return RG13_box.Text;
        }
        public void SetButtonStyle(string style)
        {
            RG13_box.Text = style;
            RGboxBtn_TextChanged(RG13_box, null);
        }
        public void SetSound(string style)
        {
            RG14_box.Text = style;
            RGboxBtn_TextChanged(RG14_box, null);
        }
        public string GetMacro()
        {
            return RG14_box.Text;
        }
        public void SetMacro(string style)
        {
            RG15_box.Text = style;
            RGboxBtn_TextChanged(RG15_box, null);
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            Point pos1 = new Point();
            pos1.Y = this.Location.Y;
            if (styleGlobal != null && styleButton != null)
            {
                Point pos2 = new Point();
                pos2.Y = this.Location.Y;

                if (styleOrder.IndexOf("general") < styleOrder.IndexOf("button")) {
                    pos1.X = this.Location.X + this.Width - 10;
                    pos2.X = this.Location.X + this.Width + styleGlobal.Width - 20;
                }
                else {
                    pos2.X = this.Location.X + this.Width - 10;
                    pos1.X = this.Location.X + this.Width + styleButton.Width - 20;
                }
                styleGlobal.Location = pos1;
                styleButton.Location = pos2;
            }
            else if (styleGlobal != null) { pos1.X = this.Location.X + this.Width - 10; styleGlobal.Location = pos1; }
            else if (styleButton != null) { pos1.X = this.Location.X + this.Width - 10; styleButton.Location = pos1; }
        }

        private void modifierMotDePasseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string rez = GridsList["password"].Value<string>();
            DialogResult input = Configurator_Win.MacrosManager.InputBox1.ShowInputDialog("Saisie du mot de passe service", ref rez, false);
            if (input == DialogResult.OK)
            {
                GridsList["password"] = rez;
            }
        }
    }

}
