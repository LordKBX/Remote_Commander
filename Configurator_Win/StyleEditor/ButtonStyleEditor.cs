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
    public partial class BlockStyleEditor : Form
    {
        private Form1 parent;
        private string pageRef;
        private string[] borderStyleMatch = new string[] { "groove", "ridge", "dotted", "dashed", "solid", "double", "inset", "outset"};
        private bool InUpdate = false;
        public BlockStyleEditor(Form1 parent, string pageRef)
        {
            this.parent = parent;
            this.pageRef = pageRef;
            InitializeComponent();
            UpdatePane();
            GR02_box.BackColorChanged += GR02_03_box_BackColorChanged;
            GR03_box.BackColorChanged += GR02_03_box_BackColorChanged;
            GR05_comboBox.SelectedValueChanged += GR05_comboBox_SelectedValueChanged;
            GR04_numbox.ValueChanged += GR04_numbox_ValueChanged;
        }

        private void GR02_03_box_BackColorChanged(object sender, EventArgs e) { if(InUpdate == false) UpdateStyle(); }
        private void GR04_numbox_ValueChanged(object sender, EventArgs e) { if (InUpdate == false) UpdateStyle(); }
        private void GR05_comboBox_SelectedValueChanged(object sender, EventArgs e) { if (InUpdate == false) UpdateStyle(); }

        public void UpdatePane()
        {
            InUpdate = true;
            string source = parent.GetGridStyle();
            string source2 = parent.GetBlockStyle();
            Debug.WriteLine(source);
            GR05_comboBox.Items.Clear();
            foreach (string st in borderStyleMatch) { GR05_comboBox.Items.Add(st); }

            GR02_box.Text = "D";
            GR03_box.Text = "D";
            GR02_box.BackColor = Color.White;
            GR03_box.BackColor = Color.Black;
            GR05_comboBox.Text = "";
            GR04_numbox.Value = 0;

            CssStyleSheet css = new CssLoader().ParseSheet(source, new Uri(pageRef), new Uri(pageRef));
            foreach (CssStyleRule rule in css.StyleRules)
            {
                if (rule.SelectorGroups.First().Selectors.Single().Classes.Last() == "control-grid-block")
                {
                    foreach (CssPropertyValue val in rule.Declaration.AllData) { if (val.Property == CssProperty.BorderTopLeftRadius) { GR04_numbox.Value = Convert.ToDecimal(val.Value.Float); break; } }
                    try
                    {
                        Color colo = Color.FromArgb(rule.Declaration.BackgroundColor.Color.R, rule.Declaration.BackgroundColor.Color.G, rule.Declaration.BackgroundColor.Color.B);
                        GR02_box.BackColor = colo; GR02_box.Text = "";
                    }
                    catch (Exception Error) { }
                    try
                    {
                        Color colo = Color.FromArgb(rule.Declaration.Color.Color.R, rule.Declaration.Color.Color.G, rule.Declaration.Color.Color.B);
                        GR03_box.BackColor = colo; GR03_box.Text = "";
                    }
                    catch (Exception Error) { }
                    try
                    {
                        if (rule.Declaration.BorderBottomStyle.Int == 0 || rule.Declaration.BorderBottomStyle.Int == 9) { GR05_comboBox.Text = borderStyleMatch[4]; }
                        else { GR05_comboBox.Text = borderStyleMatch[rule.Declaration.BorderBottomStyle.Int - 1]; }
                    }
                    catch (Exception Error) { }
                }
            }

            string[] tline = source2.Replace(" ", "").Replace(".control-tab{", "").Replace("}", "").Split(';');
            foreach (string attr in tline)
            {
                string[] tattr = attr.Split(':');
                if (tattr[0] == "background-color") { GR02_box.BackColor = ColorTranslator.FromHtml(tattr[1]); GR02_box.Text = ""; }
                if (tattr[0] == "color") { GR03_box.BackColor = ColorTranslator.FromHtml(tattr[1]); GR03_box.Text = ""; }
                if (tattr[0] == "border-style") { if (borderStyleMatch.Contains<string>(tattr[1]) == true) { GR05_comboBox.Text = tattr[1]; } }
                if (tattr[0] == "border-radius") { GR04_numbox.Value = Convert.ToDecimal(tattr[1].Replace("px","")); }
            }
            InUpdate = false;
        }

        
        private void UpdateStyle()
        {
            string endstyle = "";
            if (GR02_box.Text != "D") { endstyle += "background-color:" + ColorTranslator.ToHtml(GR02_box.BackColor) + ";"; }
            if (GR03_box.Text != "D") { endstyle += "color:" + ColorTranslator.ToHtml(GR03_box.BackColor) + ";"; }
            endstyle += "border-radius:" + GR04_numbox.Value + "px;";
            if (GR05_comboBox.Text == "") { endstyle += "border-style:" + borderStyleMatch[4] + ";"; }
            else { endstyle += "border-style:" + GR05_comboBox.Text + ";"; }
            Debug.WriteLine(endstyle);
            parent.SetBlockStyle(endstyle);
        }

        private void GR_btnColor_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            TextBox box = null;
            if (btn.Name == "GR02_btn") { box = GR02_box; }
            if (btn.Name == "GR03_btn") { box = GR03_box; }
            ColorDialog MyDialog = new ColorDialog(); MyDialog.AllowFullOpen = true; MyDialog.ShowHelp = true;
            MyDialog.Color = box.BackColor;
            if (MyDialog.ShowDialog() == DialogResult.OK) { box.Text = ""; box.BackColor = MyDialog.Color; }
        }
    }
}
