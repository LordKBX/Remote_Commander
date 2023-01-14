using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Xml.Serialization;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Configurator_Win.ModsManager
{
    [ComVisible(true)]
    public partial class ModsManager : Form
    {
        private readonly Color ModuleIdleBackColor = Color.White;
        private readonly Color ModuleIdleForeColor = Color.Black;
        private readonly Color ModuleActivBackColor = Color.FromArgb(255, 50, 50);
        private readonly Color ModuleActivForeColor = Color.White;

        private Form parent;
        private Dictionary<string, Module> ModsList = new Dictionary<string, Module>();
        private string lastSelected = "";
        private bool IsSelection = false;

        public ModsManager(Form parent = null, bool selection = false)
        {
            IsSelection = selection;
            this.parent = parent;
            InitializeComponent();
            updateSelectedModInfo();
            if (!IsSelection) { 
                SelectBtn.Enabled = false; 
                SelectBtn.Visible = false; 
            }

            DirectoryInfo d = new DirectoryInfo(Program.configDirectory + Program.osSep + "Mods");
            IEnumerable<FileInfo> Files = null;
            Files = d.GetFiles("*", SearchOption.TopDirectoryOnly).Where(f =>
                f.FullName.EndsWith(".xml") ||
                f.FullName.EndsWith(".XML")
                );

            foreach (FileInfo file in Files)
            {
                string name = file.Name.Substring(0, file.Name.LastIndexOf('.'));
                Module module = Module.Parse(file.FullName);

                if (module != null) {
                    ModsList.Add(name, module);
                    InjectTabSound(modsLayoutPanel, file);
                } 
            }

        }

        public void Console(string input) { Debug.WriteLine("JS >> " + input); }

        private void InjectTabSound(FlowLayoutPanel flow, FileInfo file)
        {
            Module module = Module.Parse(file.FullName);
            if (module == null) { return; }
            Padding p = new Padding(0);
            int baseCaseSize = 50;
            string name = file.Name.Substring(0, file.Name.LastIndexOf('.'));

            TableLayoutPanel tab1 = new TableLayoutPanel();
            tab1.SuspendLayout();
            tab1.BackColor = ModuleIdleBackColor;
            tab1.ForeColor = ModuleIdleForeColor;
            tab1.Click += Tab1_Click;
            tab1.DoubleClick += Tab1_Click;
            tab1.Tag = name;
            tab1.ColumnCount = 1;
            tab1.RowCount = 2;
            tab1.Size = new Size(module.minWidth * baseCaseSize + 10, module.minHeight * baseCaseSize + 23);
            tab1.ColumnStyles.Add(new ColumnStyle());
            tab1.ColumnStyles[0].Width = 100;
            tab1.ColumnStyles[0].SizeType = SizeType.Percent;
            tab1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            tab1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab1.BorderStyle = BorderStyle.FixedSingle;

            Label lab = new Label();
            tab1.Controls.Add(lab, 0, 0);
            tab1.SetRowSpan(lab, 1);
            tab1.SetColumnSpan(lab, 1);
            lab.Text = name;
            lab.Tag = name;
            lab.BackColor = ModuleIdleBackColor;
            lab.ForeColor = ModuleIdleForeColor;
            lab.Size = new Size(module.minWidth * baseCaseSize, 20);
            lab.Anchor = AnchorStyles.Left;
            lab.Margin = p;
            lab.Padding = new Padding(1);
            lab.Click += Lab_Click;
            lab.DoubleClick += Lab_Click;

            WebBrowser web = new WebBrowser();
            web.Size = new Size(tab1.Size.Width - 4, module.minHeight * baseCaseSize);
            web.Top = 20;
            web.Left = 1;
            web.Tag = name;
            web.Url = new Uri("file:///" + (Program.baseDirectory.Replace("\\", "/") + "/html/mod.html").Replace("//", "/"));
            web.ObjectForScripting = this;
            web.DocumentCompleted += WebObj_DocumentCompleted;
            //web.Navigated += WebObj_Navigated;
            tab1.Controls.Add(web, 0, 1);
            tab1.SetRowSpan(web, 1);
            tab1.SetColumnSpan(web, 1);

            flow.Controls.Add(tab1);
        }

        private void uncheckAll() {
            foreach (TableLayoutPanel panel in modsLayoutPanel.Controls) {
                panel.BackColor = ModuleIdleBackColor;
                panel.ForeColor = ModuleIdleForeColor;
                panel.Controls[0].BackColor = ModuleIdleBackColor;
                panel.Controls[0].ForeColor = ModuleIdleForeColor;
            }
            lastSelected = "";
        }

        public void moduleClicked(string name) {
            Debug.WriteLine("JS >>" + name);
            if (lastSelected == name) { return; }
            uncheckAll();
            lastSelected = name;
            foreach (TableLayoutPanel panel in modsLayoutPanel.Controls)
            {
                if ((string)panel.Tag == name)
                {
                    panel.BackColor = ModuleActivBackColor;
                    panel.ForeColor = ModuleActivForeColor;
                    panel.Controls[0].BackColor = ModuleActivBackColor;
                    panel.Controls[0].ForeColor = ModuleActivForeColor;
                }
            }
            updateSelectedModInfo();
        }

        private void Lab_Click(object sender, EventArgs e)
        {
            Label lab = (Label)sender;
            if (lastSelected == (string)lab.Tag) { return; }
            uncheckAll();
            lastSelected = (string)lab.Tag;
            lab.BackColor = ModuleActivBackColor;
            lab.ForeColor = ModuleActivForeColor;
            lab.Parent.BackColor = ModuleActivBackColor;
            lab.Parent.ForeColor = ModuleActivForeColor;
            updateSelectedModInfo();
        }

        private void Tab1_Click(object sender, EventArgs e)
        {
            TableLayoutPanel panel = (TableLayoutPanel)sender;
            if (lastSelected == (string)panel.Tag) { return; }
            uncheckAll();
            lastSelected = (string)panel.Tag;
            panel.BackColor = ModuleIdleBackColor;
            panel.ForeColor = ModuleIdleForeColor;
            panel.Controls[0].BackColor = ModuleIdleBackColor;
            panel.Controls[0].ForeColor = ModuleIdleForeColor;
            updateSelectedModInfo();
        }

        private async void WebObj_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) 
        {
            WebBrowser web = ((WebBrowser)sender);
            string name = (string)(web.Tag);

            if (!ModsList.ContainsKey(name)) { return; }
            Module module = ModsList[name];
            if (module == null) { return; }
            int ModWidth = web.Size.Width - 8;
            int ModHeight = web.Size.Height - 8;

            string template = module.template;
            string style = module.style;
            template = template.Replace("%mid%", name);
            style = style.Replace("%mid%", name);
            template = template.Replace("%bmid%", name);
            style = style.Replace("%bmid%", name);
            foreach (Var vari in module.vars.var) {
                //Debug.WriteLine("vari.name = " + vari.name);
                //Debug.WriteLine("vari.eval = " + vari.eval);
                if (vari.eval.ToLower() != "true")
                {
                    template = template.Replace("%" + vari.name + "%", vari.value);
                    style = style.Replace("%" + vari.name + "%", vari.value);
                }
                else
                {
                    var rez = await CSharpScript.EvaluateAsync("int ModWidth = " + ModWidth + "; int ModHeight = " + ModHeight + "; "+ vari.value);
                    template = template.Replace("%" + vari.name + "%", rez.ToString());
                    style = style.Replace("%" + vari.name + "%", rez.ToString());
                }
            }

            try
            {
                web.Document.InvokeScript("injectData", new[] { template, style, name });
            }
            catch (Exception error) { }
        }

        private void modsLayoutPanel_Click(object sender, EventArgs e)
        {
            uncheckAll();
            updateSelectedModInfo();
        }

        private void updateSelectedModInfo() {
            if (lastSelected == "")
            {
                main_label.Text = "N/A";
                info_listBox.Items.Clear();
            }
            else {
                Module module = ModsList[lastSelected];
                main_label.Text = module.id;
                FileInfo fi = new FileInfo(Program.configDirectory + Program.osSep + "Mods" + Program.osSep + lastSelected + ".xml");
                info_listBox.Items.Clear();
                info_listBox.Items.Add("FileSize: " + fi.Length + " Octets");
                info_listBox.Items.Add("Import: " + fi.CreationTime.ToShortDateString());
                info_listBox.Items.Add("MinWidth: " + module.minWidth);
                info_listBox.Items.Add("MinHeight: " + module.minHeight);
                info_listBox.Items.Add("Vars Count: " + module.vars.var.Count);
                info_listBox.Items.Add("Scripts Count: " + module.scriptxs.scriptx.Count);
            }
        }

        private void add_btn_Click(object sender, EventArgs e)
        {

        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {

        }
    }
}
