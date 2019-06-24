using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Configurator_Win.MacrosManager
{
    public partial class ActionEditor : Form
    {
        private MacroEditor macroEditor;
        private TableLayoutPanel panel;
        public ActionEditor(MacroEditor macroEditor, TableLayoutPanel panel = null)
        {
            this.macroEditor = macroEditor;
            this.panel = panel;
            InitializeComponent();
            if (panel != null) {
                //tab1.Tag
                JToken action = (JToken)panel.Tag;
                string type = action["type"].Value<string>();
                if (type == "KeyBoardInput")
                {
                    tabControler.SelectedIndex = 0;
                    try { TAB1_KEY_BOX.Text = action["key"].Value<string>(); } catch (Exception error) { }
                    try { TAB1_CTRL_CHBOX.Checked = action["ctrl"].Value<bool>(); } catch (Exception error) { }
                    try { TAB1_ALT_CHBOX.Checked = action["alt"].Value<bool>(); } catch (Exception error) { }
                    try { TAB1_MAJ_CHBOX.Checked = action["maj"].Value<bool>(); } catch (Exception error) { }
                    try { TAB1_WIN_CHBOX.Checked = action["win"].Value<bool>(); } catch (Exception error) { }
                }
                if (type == "MuteSound" || type == "VolUp" || type == "VolDown")
                {
                    tabControler.SelectedIndex = 1;
                    if (type == "MuteSound") { TAB2_MUTE_RBOX.Checked = true; }
                    if (type == "VolUp") {
                        TAB2_VOLUP_RBOX.Checked = true;
                        try { TAB2_VOLUP_NBOX.Value = action["step"].Value<decimal>(); } catch (Exception error) { }
                    }
                    if (type == "VolDown") {
                        TAB2_VOLDOWN_RBOX.Checked = true;
                        try { TAB2_VOLDOWN_NBOX.Value = action["step"].Value<decimal>(); } catch (Exception error) { }
                    }
                }
                if (type == "Execute")
                {
                    tabControler.SelectedIndex = 2;
                    try { TAB3_APP_BOX.Text = action["executable"].Value<string>(); } catch (Exception error) { }
                    try { TAB3_OPT_BOX.Text = action["params"].Value<string>(); } catch (Exception error) { }
                }
                if (type == "Sleep")
                {
                    tabControler.SelectedIndex = 3;
                    try { TAB4_NBOX.Value = action["delay"].Value<decimal>(); } catch (Exception error) { }
                }
            }
        }

        private void TAB1_KEY_BTN_Click(object sender, EventArgs e)
        {
            TAB1_CTRL_CHBOX.Checked = false;
            TAB1_ALT_CHBOX.Checked = false;
            TAB1_MAJ_CHBOX.Checked = false;
            TAB1_WIN_CHBOX.Checked = false;
            KeyboardScan k = new KeyboardScan(this);
            k.ShowDialog();
            k.Dispose();
        }
        public void SetKeyboardActionKey(string key) {
            TAB1_KEY_BOX.Text = key;
        }
        public void SetKeyboardActionCtrl(bool opt) { TAB1_CTRL_CHBOX.Checked = opt; }
        public void SetKeyboardActionAlt(bool opt) { TAB1_ALT_CHBOX.Checked = opt; }
        public void SetKeyboardActionMaj(bool opt) { TAB1_MAJ_CHBOX.Checked = opt; }
        public void SetKeyboardActionWin(bool opt) { TAB1_WIN_CHBOX.Checked = opt; }

        private void BTN_CLOSE_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BTN_OK_Click(object sender, EventArgs e)
        {
            JToken action = new JObject();
            if (tabControler.SelectedIndex == 0)
            {
                action["type"] = "KeyBoardInput";
                action["key"] = TAB1_KEY_BOX.Text;
                action["ctrl"] = TAB1_CTRL_CHBOX.Checked;
                action["alt"] = TAB1_ALT_CHBOX.Checked;
                action["maj"] = TAB1_MAJ_CHBOX.Checked;
                action["win"] = TAB1_WIN_CHBOX.Checked;
            }
            else if (tabControler.SelectedIndex == 1)
            {
                if (TAB2_MUTE_RBOX.Checked == true) { action["type"] = "MuteSound"; }
                else if (TAB2_VOLUP_RBOX.Checked == true)
                {
                    action["type"] = "VolUp";
                    action["step"] = TAB2_VOLUP_NBOX.Value;
                }
                else if (TAB2_VOLDOWN_RBOX.Checked == true)
                {
                    action["type"] = "VolDown";
                    action["step"] = TAB2_VOLDOWN_NBOX.Value;
                }
            }
            else if (tabControler.SelectedIndex == 2)
            {
                action["type"] = "Execute";
                action["executable"] = TAB3_APP_BOX.Text;
                action["params"] = TAB3_OPT_BOX.Text;
            }
            else if (tabControler.SelectedIndex == 3)
            {
                action["type"] = "Sleep";
                action["delay"] = TAB4_NBOX.Value;
            }
            else { this.Close(); return; }

            if (panel == null) {
                this.macroEditor.NewAction(action);
            }
            else { this.macroEditor.UpdateAction(action, panel); }
            this.Close();
        }

        private void TAB3_APP_BTN_Click(object sender, EventArgs e)
        {
            string DeestDir = Program.configDirectory;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Program.configDirectory;
            openFileDialog.Filter = "All executable files (*.bat;*.exe)|*.bat;*.exe|BAT files (*.bat)|*.bat|EXE files (*.exe)|*.exe";
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = openFileDialog.FileNames;
                foreach (string fil in files)
                {
                    TAB3_APP_BOX.Text = new FileInfo(fil).FullName;
                }
            }
        }

        private void TAB3_TEST_BTN_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(
                    TAB3_APP_BOX.Text,
                    TAB3_OPT_BOX.Text
                    );
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                //string result = proc.StandardOutput.ReadToEnd();
            }
            catch (Exception objException)
            {
                MessageBox.Show(objException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
