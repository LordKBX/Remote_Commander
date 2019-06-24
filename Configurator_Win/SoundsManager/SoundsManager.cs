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
using NAudio.Wave;

namespace Configurator_Win.SoundsManager
{
    public partial class SoundsManager : Form
    {
        private Form1 parent = null;
        private int currentTab = 0;
        private TableLayoutPanel SelectedSound = null;
        private Double LastClickTime = 0;
        private Player player = null;
        public SoundsManager(Form1 parent = null)
        {
            this.parent = parent;
            player = new Player(this);
            InitializeComponent();
            if (parent == null) { main_label.Text = "Gestion des Sonds"; SelectBtn.Hide(); }
            else
            {
                add_btn.Hide();
                main_label.Text = "Selection sond";
            }
            del_btn.Hide();

            info_listBox.Items.Clear();
            loadSounds();
            tabControler.SelectedIndexChanged += TabControler_SelectedIndexChanged;
            this.FormClosed += SoundsManager_FormClosed;
        }

        public void SoundsManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            player.StopAll();
        }

        private void TabControler_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentTab != tabControler.SelectedIndex) { UnselectSound(currentTab); SelectedSound = null; info_listBox.Items.Clear(); del_btn.Hide(); }
            currentTab = tabControler.SelectedIndex;
        }

        private void loadSounds()
        {
            string path = Program.configDirectory;
            //tabControler.Controls.RemoveAt(tabControler.SelectedIndex);
            DirectoryInfo d = new DirectoryInfo(Program.configDirectory + "\\Sounds");
            Debug.WriteLine(d.FullName);
            IEnumerable<FileInfo> Files = null;
            Files = d.GetFiles("*", SearchOption.AllDirectories).Where(f =>
                f.FullName.EndsWith(".mp3") ||
                f.FullName.EndsWith(".MP3") ||
                f.FullName.EndsWith(".wav") ||
                f.FullName.EndsWith(".WAV")
                );
            Debug.WriteLine(JsonConvert.SerializeObject(Files));
            string prevFolder = "";
            Size si = new Size(64, 64);
            TabPage tabObj;
            FlowLayoutPanel flow = null;
            foreach (FileInfo file in Files.ToList<FileInfo>())
            {
                if (prevFolder != file.Directory.FullName)
                {
                    if (file.Directory.Name == "_Custom")
                    {
                        tabObj = tabControler.TabPages[0];
                        flow = (FlowLayoutPanel)tabObj.Controls[0];
                    }
                    else
                    {
                        tabObj = new TabPage();
                        tabObj.Text = file.Directory.Name;
                        tabObj.BackColor = Color.Beige;
                        flow = new FlowLayoutPanel();
                        flow.Dock = DockStyle.Fill;
                        flow.AutoScroll = true;
                        tabObj.Controls.Add(flow);
                        tabControler.TabPages.Add(tabObj);
                    }
                    prevFolder = file.Directory.FullName;
                }
                InjectTabSound(flow, file);
            }
        }

        private void InjectTabSound(FlowLayoutPanel flow, FileInfo file)
        {
            Padding p = new Padding();
            p.All = 0;

            Label lab = new Label();
            lab.Text = file.Name;
            lab.Dock = DockStyle.Fill;
            lab.Margin = p;
            lab.Padding = p;
            lab.Click += Lab_Click;
            lab.DoubleClick += Lab_DoubleClick;

            AudioFileReader fir = new AudioFileReader(file.FullName);
            Label lab2 = new Label();
            //lab2.Text = "gg";
            lab2.Text = "" + fir.TotalTime.Minutes.ToString("D2") + ":" + ((fir.TotalTime.Seconds < 1)?1:fir.TotalTime.Seconds).ToString("D2");
            lab2.Tag = "lab2";
            lab2.Click += Lab_Click;
            lab2.DoubleClick += Lab_DoubleClick;
            lab2.Margin = p;
            lab2.Padding = p;
            lab2.TextAlign = ContentAlignment.MiddleCenter;

            Button btn = new Button();
            btn.Text = "Play";
            btn.Click += Btn_Click;
            btn.DoubleClick += Btn_DoubleClick;
            btn.Margin = p;
            btn.Padding = p;
            btn.BackColor = Color.Silver;

            TableLayoutPanel tab1 = new TableLayoutPanel();
            tab1.SuspendLayout();
            tab1.BackColor = Color.White;
            tab1.ForeColor = Color.Black;
            tab1.Click += Tab1_Click;
            tab1.DoubleClick += Tab1_DoubleClick;
            tab1.Tag = file.FullName;
            tab1.ColumnCount = 1;
            tab1.RowCount = 2;
            tab1.Size = new Size(150, 50);
            tab1.MaximumSize = new Size(150, 50);
            tab1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tab1.ForeColor = Color.Black;
            tab1.RowStyles.Clear();
            tab1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tab1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tab1.Padding = p;

            TableLayoutPanel tab2 = new TableLayoutPanel();
            tab2.Click += Tab1_Click;
            tab2.DoubleClick += Tab1_DoubleClick;
            tab2.Name = "tab2";
            tab2.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tab2.Size = new Size(140, 40);
            tab2.MaximumSize = new Size(140, 40);
            tab2.Dock = DockStyle.Fill;
            tab2.Padding = p;
            tab2.Margin = p;
            
            tab2.ColumnStyles.Clear();
            tab2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            tab2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tab2.Controls.Add(lab2, 0, 0);
            tab2.Controls.Add(btn, 1, 0);
            tab2.CausesValidation = true;
            tab2.ResumeLayout();


            tab1.Controls.Add(lab, 0, 0);
            tab1.Controls.Add(tab2, 0, 1);
            tab1.CausesValidation = true;
            tab1.ResumeLayout();
            flow.Controls.Add(tab1);
        }

        private void UpdateInfoPannel(string filePath)
        {
            string name = new FileInfo(filePath).Name;
            string type = "";
            string width = "";
            string height = "";

            if (filePath.EndsWith(".mp3") == true) { type = "audio/mpeg3"; }
            if (filePath.EndsWith(".wav") == true) { type = "audio/wav"; }

            AudioFileReader fir = new AudioFileReader(filePath);
            info_listBox.Items.Clear();
            info_listBox.Items.Add("Name: " + name);
            info_listBox.Items.Add("Type: " + type);
            info_listBox.Items.Add("Duration: " + fir.TotalTime.Minutes.ToString("D2") + ":" + ((fir.TotalTime.Seconds < 1) ? 1 : fir.TotalTime.Seconds).ToString("D2"));
            if (parent == null && tabControler.SelectedIndex == 0) { del_btn.Show(); }
            else { del_btn.Hide(); }
        }
    }
}
