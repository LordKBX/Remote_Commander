using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Alba.CsCss.Style;

namespace Configurator_Win.StyleEditor
{
    public partial class GlobalStyleEditor : Form
    {
        private Form1 parent = null;
        private string pageRef = null;
        private bool InLoad = false;
        private string[] borderStyleMatch = new string[] { "groove", "ridge", "dotted", "dashed", "solid", "double", "inset", "outset"};
        public GlobalStyleEditor(Form1 parent, string pageRef)
        {
            this.parent = parent;
            this.pageRef = pageRef;
            InitializeComponent();
            this.Size = this.MinimumSize;
            UpdatePane();
            Uri muri = new Uri("file:///" + (Program.baseDirectory.Replace("\\", "/") + "/html/tab.html").Replace("//", "/"));
            GR03.Navigate(muri);
            GR03.DocumentCompleted += GR03_DocumentCompleted1;
            GR03.Navigated += GR03_Navigated;
            GR03.ProgressChanged += GR03_ProgressChanged;

            GR04_box.BackColorChanged += GRtab_box_BackColorChanged;
            GR05_box.BackColorChanged += GRtab_box_BackColorChanged;
            GR06_combobox.TextChanged += GR06_combobox_TextChanged;

            GR08_box.BackColorChanged += GRgrid_box_BackColorChanged;
            GR11_box.BackColorChanged += GRgrid_box_BackColorChanged;
            GR12_box.BackColorChanged += GRgrid_box_BackColorChanged;
            GR14_comboBox.TextChanged += GR14_comboBox_TextChanged;
            GR13_numbox.ValueChanged += GR13_numbox_ValueChanged1;
        }


        private void GR13_numbox_ValueChanged1(object sender, EventArgs e) { UpdateStyle(); }
        private void GR14_comboBox_TextChanged(object sender, EventArgs e) { UpdateStyle(); }
        private void GRgrid_box_BackColorChanged(object sender, EventArgs e) { UpdateStyle(); }
        private void GRtab_box_BackColorChanged(object sender, EventArgs e) { RefreshFrame(); UpdateStyle(); }
        private void GR06_combobox_TextChanged(object sender, EventArgs e) { RefreshFrame(); UpdateStyle(); }

        private void GR03_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e) { GR03_DocumentCompleted1(sender, null); }
        private void GR03_Navigated(object sender, WebBrowserNavigatedEventArgs e) { GR03_DocumentCompleted1(sender, null); }
        private void GR03_DocumentCompleted1(object sender, WebBrowserDocumentCompletedEventArgs e) { RefreshFrame(); }

        public void RefreshFrame() {
            try
            {
                GR03.Document.InvokeScript("loadData", new[] {
                (GR04_box.Text=="D")?"inerit":ColorTranslator.ToHtml(GR04_box.BackColor),
                (GR04_box.Text=="D")?"inerit":ColorTranslator.ToHtml(GR05_box.BackColor),
                parent.GetGridIcon(),
                parent.GetGridName(),
                GR06_combobox.Text
            });
            }
            catch (Exception error) { }
        }

        public void UpdatePane()
        {
            if (InLoad == true) { return; }
            InLoad = true;
            string source = parent.GetGridStyle();
            Debug.WriteLine(source);
            string[] tabsStyles = new string[] { "square", "tabulate", "rounded" };
            GR06_combobox.Items.Clear();
            foreach (string st in tabsStyles) { GR06_combobox.Items.Add(st); }
            GR14_comboBox.Items.Clear();
            foreach (string st in borderStyleMatch) { GR14_comboBox.Items.Add(st); }

            GR04_box.Text = "D";
            GR05_box.Text = "D";
            GR08_box.Text = "D";
            GR11_box.Text = "D";
            GR12_box.Text = "D";
            GR04_box.BackColor = Color.White;
            GR05_box.BackColor = Color.Black;
            GR08_box.BackColor = Color.White;
            GR11_box.BackColor = Color.White;
            GR12_box.BackColor = Color.Black;
            GR14_comboBox.Text = "";
            GR13_numbox.Value = 0;

            CssStyleSheet css = new CssLoader().ParseSheet(source, new Uri(pageRef), new Uri(pageRef));
            foreach (CssStyleRule rule in css.StyleRules)
            {
                if (rule.SelectorGroups.First().Selectors.Single().Classes.Last() == "control-tab")
                {
                    string[] lines = source.Split("\n".ToArray<char>());
                    foreach (string line in lines) {
                        if (line.Contains(".control-tab") == true)
                        {
                            string[] tline = line.Replace(" ", "").Replace(".control-tab{", "").Replace("}", "").Split(';');
                            foreach (string attr in tline) {
                                if (attr.StartsWith("style:") == true) { GR06_combobox.Text = attr.Replace("style:", ""); }
                                Debug.WriteLine(attr);
                            }
                            break;
                        }
                    }
                    try
                    {
                        Color colo = Color.FromArgb(rule.Declaration.BackgroundColor.Color.R, rule.Declaration.BackgroundColor.Color.G, rule.Declaration.BackgroundColor.Color.B);
                        GR04_box.BackColor = colo; GR04_box.Text = "";
                    }
                    catch (Exception Error) { }
                    try
                    {
                        Color colo = Color.FromArgb(rule.Declaration.Color.Color.R, rule.Declaration.Color.Color.G, rule.Declaration.Color.Color.B);
                        GR05_box.BackColor = colo; GR05_box.Text = "";
                    }
                    catch (Exception Error) { }
                }

                if (rule.SelectorGroups.First().Selectors.Single().Classes.Last() == "control-grid")
                {
                    try
                    {
                        Color colo = Color.FromArgb(rule.Declaration.BackgroundColor.Color.R, rule.Declaration.BackgroundColor.Color.G, rule.Declaration.BackgroundColor.Color.B);
                        GR08_box.BackColor = colo; GR08_box.Text = "";
                    }
                    catch (Exception Error) { }
                }

                if (rule.SelectorGroups.First().Selectors.Single().Classes.Last() == "control-grid-button")
                {
                    foreach (CssPropertyValue val in rule.Declaration.AllData) { if (val.Property == CssProperty.BorderTopLeftRadius) { GR13_numbox.Value = Convert.ToDecimal(val.Value.Float); break; } }
                    try
                    {
                        Color colo = Color.FromArgb( rule.Declaration.BackgroundColor.Color.R, rule.Declaration.BackgroundColor.Color.G, rule.Declaration.BackgroundColor.Color.B );
                        GR11_box.BackColor = colo; GR11_box.Text = "";
                    }
                    catch (Exception Error) { }
                    try
                    {
                        Color colo = Color.FromArgb(rule.Declaration.Color.Color.R, rule.Declaration.Color.Color.G, rule.Declaration.Color.Color.B);
                        GR12_box.BackColor = colo; GR12_box.Text = "";
                    }
                    catch (Exception Error) { }
                    try {
                        if (rule.Declaration.BorderBottomStyle.Int == 0 || rule.Declaration.BorderBottomStyle.Int == 9) { GR14_comboBox.Text = borderStyleMatch[4]; }
                        else { GR14_comboBox.Text = borderStyleMatch[rule.Declaration.BorderBottomStyle.Int-1]; }
                    } catch (Exception Error) { }
                }
            }
            InLoad = false;
        }

        private void UpdateStyle() {
            string endstyle = "";
            endstyle += ".control-tab{";
            if (GR04_box.Text != "D") { endstyle += "background-color:" + ColorTranslator.ToHtml(GR04_box.BackColor) + ";"; }
            if (GR05_box.Text != "D") { endstyle += "color:" + ColorTranslator.ToHtml(GR05_box.BackColor) + ";"; }
            endstyle += "}\n";
            endstyle += ".control-tab-style{";
            endstyle += GR06_combobox.Text;
            endstyle += "}\n";
            endstyle += ".control-grid{";
            if (GR08_box.Text != "D") { endstyle += "background-color:" + ColorTranslator.ToHtml(GR08_box.BackColor) + ";"; }
            endstyle += "}\n";
            endstyle += ".control-grid-button{";
            if (GR11_box.Text != "D") { endstyle += "background-color:" + ColorTranslator.ToHtml(GR11_box.BackColor) + ";"; }
            if (GR12_box.Text != "D") { endstyle += "color:" + ColorTranslator.ToHtml(GR12_box.BackColor) + ";"; }
            endstyle += "border-radius:" + GR13_numbox.Value + "px;";
            if (GR14_comboBox.Text == "") { endstyle += "border-style:" + borderStyleMatch[4] + ";"; }
            else { endstyle += "border-style:" + GR14_comboBox.Text + ";"; }
            endstyle += "}";
            Debug.WriteLine(endstyle);
            parent.SetGridStyle(endstyle);
        }

        private void GRbox_BackColorChanged(object sender, EventArgs e)
        {
            UpdateStyle();
        }

        private void GR13_numbox_ValueChanged(object sender, EventArgs e)
        {
            UpdateStyle();
        }

        private void GR06_combobox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateStyle();
        }

        private void GR14_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateStyle();
        }

        private void GR_btnColor_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            TextBox box = null;
            if (btn.Name == "GR04_btn") { box = GR04_box; }
            if (btn.Name == "GR05_btn") { box = GR05_box; }
            if (btn.Name == "GR08_btn") { box = GR08_box; }
            if (btn.Name == "GR11_btn") { box = GR11_box; }
            if (btn.Name == "GR12_btn") { box = GR12_box; }
            ColorDialog MyDialog = new ColorDialog(); MyDialog.AllowFullOpen = true; MyDialog.ShowHelp = true;
            MyDialog.Color = box.BackColor;
            if (MyDialog.ShowDialog() == DialogResult.OK) { box.Text = ""; box.BackColor = MyDialog.Color; }
        }
    }
}
