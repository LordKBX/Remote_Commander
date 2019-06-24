using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Configurator_Win.SoundsManager
{
    public partial class SoundsManager : Form
    {
        private void add_btn_Click(object sender, EventArgs e)
        {
            tabControler.SelectTab(0);
            string DeestDir = Program.configDirectory + "\\Sounds\\_Custom";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Program.configDirectory + "\\Sounds";
            openFileDialog.Filter = "All Sounds files (*.mp3;*.wav)|*.mp3;*.wav|MP3 files (*.mp3)|*.mp3|WAV files (*.wav)|*.wav";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TabPage tabObj = tabControler.TabPages[0];
                FlowLayoutPanel flow = (FlowLayoutPanel)tabObj.Controls[0];
                Size si = new Size(64, 64);
                //Get the path of specified file
                string[] files = openFileDialog.FileNames;
                foreach (string fil in files)
                {
                    string end = DeestDir + "\\" + new FileInfo(fil).Name;
                    File.Copy(fil, end);
                    FileInfo file = new FileInfo(end);
                    InjectTabSound(flow, file);
                }
            }
        }

        private void del_btn_Click(object sender, EventArgs e)
        {
            if (currentTab != 0) { return; }
            if (SelectedSound == null) { return; }
            DialogResult rez = MessageBox.Show("Souhaitez vous vraiment suppprimer l'image \"" + new FileInfo((string)SelectedSound.Tag).Name + "\" ?",
                "Attention !", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (rez == System.Windows.Forms.DialogResult.Yes)
            {
                info_listBox.Items.Clear();
                File.Delete((string)SelectedSound.Tag);
                SelectedSound.Parent.Controls.Remove(SelectedSound);
            }
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("SelectBtn_Click");
            if (SelectedSound == null || parent == null) { return; }
            string tt = (string)SelectedSound.Tag;
            string loc = (tt.Replace(Program.configDirectory + "\\Sounds\\", "").Replace("\\", "/")).Replace("//", "/");
            if (parent != null)
            {
                parent.SetSound(loc);
            }
            this.Close();
        }

        private void Lab_DoubleClick(object sender, EventArgs e)
        {
            //Debug.WriteLine("Lab_DoubleClick");
            SelectBtn_Click(null, null);
        }

        private void Tab1_DoubleClick(object sender, EventArgs e)
        {
            //Debug.WriteLine("Tab1_DoubleClick");
            SelectBtn_Click(null, null);
        }

        private void Btn_DoubleClick(object sender, EventArgs e)
        {
            //Debug.WriteLine("Btn_DoubleClick");
            SelectBtn_Click(null, null);
        }



        private void Tab1_Click(object sender, EventArgs e)
        {
            TableLayoutPanel tab = (TableLayoutPanel)sender;
            if (tab.Name == "tab2") { tab = (TableLayoutPanel)tab.Parent; }
            Double tim = Program.getUnixTimeStamp(true);
            SelectedSound = tab;
            LastClickTime = tim;
            UnselectSound(currentTab);
            tab.BackColor = Color.Red;
            UpdateInfoPannel((string)tab.Tag);
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("Btn_Click");
            Button btn = (Button)sender;
            TableLayoutPanel tab = (TableLayoutPanel)btn.Parent.Parent;
            SelectedSound = tab;
            UnselectSound(tabControler.SelectedIndex);
            player.PlaySound((string)tab.Tag);
            Tab1_Click(tab, EventArgs.Empty);
        }

        private void Lab_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("Lab_Click");
            Label lab = (Label)sender;
            TableLayoutPanel tab = (TableLayoutPanel)lab.Parent;
            try { if ((string)lab.Tag == "lab2") { tab = (TableLayoutPanel)tab.Parent; } } catch (Exception error) { }
            SelectedSound = tab;
            UnselectSound(tabControler.SelectedIndex);
            Tab1_Click(tab, EventArgs.Empty);
        }

        private void UnselectSound(int TabIndex)
        {
            if (TabIndex < 0 || tabControler.TabPages.Count <= TabIndex) { return; }
            TabPage tabObj = tabControler.TabPages[TabIndex];
            foreach (Control item in tabObj.Controls)
            {
                foreach (Control item2 in item.Controls)
                {
                    ((TableLayoutPanel)item2).BackColor = Color.White;
                }
            }
        }

    }
}
