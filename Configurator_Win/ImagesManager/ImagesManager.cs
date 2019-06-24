using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MetadataExtractor;

namespace Configurator_Win.ImagesManager
{
    public partial class ImagesManager : Form
    {
        private Form1 parent = null;
        private string focus = null;
        private int currentTab = 0;
        private PictureBox SelectedImage = null;
        private Double LastClickTime = 0;
        public ImagesManager(Form1 parent = null, string focus=null)
        {
            this.parent = parent;
            this.focus = focus;
            InitializeComponent();
            if (parent == null) { main_label.Text = "Gestion des Images"; SelectBtn.Hide(); }
            else {
                add_btn.Hide();
                main_label.Text = "Selection icone";
            }
            del_btn.Hide();

            info_listBox.Items.Clear();
            loadImages();
            tabControler.SelectedIndexChanged += TabControler_SelectedIndexChanged;
        }

        private void TabControler_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentTab != tabControler.SelectedIndex) { UnselectImages(currentTab); SelectedImage = null; info_listBox.Items.Clear(); del_btn.Hide(); }
            currentTab = tabControler.SelectedIndex;
        }

        private void loadImages()
        {
            string path = Program.configDirectory;
            //tabControler.Controls.RemoveAt(tabControler.SelectedIndex);
            DirectoryInfo d = new DirectoryInfo(Program.configDirectory+ "\\Images");
            Debug.WriteLine(d.FullName);
            IEnumerable<FileInfo> Files = null;
            Files = d.GetFiles("*", SearchOption.AllDirectories).Where(f => 
                f.FullName.EndsWith(".jpg") || 
                f.FullName.EndsWith(".JPG") || 
                f.FullName.EndsWith(".jpeg") ||
                f.FullName.EndsWith(".JPEG") ||
                f.FullName.EndsWith(".gif") ||
                f.FullName.EndsWith(".GIF") ||
                f.FullName.EndsWith(".png") ||
                f.FullName.EndsWith(".PNG")
                );
            Debug.WriteLine(JsonConvert.SerializeObject(Files));
            string prevFolder = "";
            Size si = new Size(64, 64);
            TabPage tabObj;
            FlowLayoutPanel flow = null;
            foreach (FileInfo file in Files.ToList<FileInfo>()) {
                if (prevFolder != file.Directory.FullName) {
                    if (file.Directory.Name == "_Custom") {
                        tabObj = tabControler.TabPages[0];
                        flow = (FlowLayoutPanel)tabObj.Controls[0];
                    }
                    else {
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
                PictureBox pix = new PictureBox();
                pix.ImageLocation = file.FullName;
                pix.Size = si;
                pix.SizeMode = PictureBoxSizeMode.StretchImage;
                pix.Click += Pix_Click;
                flow.Controls.Add(pix);
            }
        }

        private void UnselectImages(int TabIndex) {
            if (TabIndex < 0 || tabControler.TabPages.Count <= TabIndex) { return; }
            TabPage tabObj = tabControler.TabPages[TabIndex];
            foreach (Control item in tabObj.Controls) {
                foreach (Control item2 in item.Controls)
                {
                    ((PictureBox)item2).BorderStyle = BorderStyle.None;
                }
            }
        }

        private void Pix_Click(object sender, EventArgs e)
        {
            PictureBox img = (PictureBox)sender;
            if (SelectedImage != null)
            {
                if (SelectedImage.ImageLocation == img.ImageLocation)
                {
                    if (LastClickTime + 1001 >= Program.getUnixTimeStamp(true))
                    {
                        Pix_DoubleClick(img, null);
                        return;
                    }
                }
            }
            SelectedImage = img;
            LastClickTime = Program.getUnixTimeStamp(true);
            UnselectImages(currentTab);
            img.BorderStyle = BorderStyle.Fixed3D;

            string name = new FileInfo(img.ImageLocation).Name;
            string type = "";
            string width = "";
            string height = "";

            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(img.ImageLocation);
            foreach (var directory in directories)
                foreach (var tag in directory.Tags) {
                    if (tag.Name == "Image Width") { width = tag.Description; }
                    if (tag.Name == "Image Height") { height = tag.Description; }
                    if (tag.Name == "Detected MIME Type") { type = tag.Description; }
                }

            info_listBox.Items.Clear();
            info_listBox.Items.Add("Name: "+ name);
            info_listBox.Items.Add("Type: " + type);
            info_listBox.Items.Add("Size: " + width + " x " + height + " (px)");
            if (parent == null && tabControler.SelectedIndex == 0) { del_btn.Show(); }
            else { del_btn.Hide(); }
        }

        private void Pix_DoubleClick(object sender, EventArgs e)
        {
            //Debug.WriteLine("Pix_DoubleClick");
            if (parent == null) { return; }
            PictureBox img = (PictureBox)sender;
            string loc = (img.ImageLocation.Replace(Program.configDirectory + "\\Images\\", "").Replace("\\", "/")).Replace("//", "/");
            if (parent != null) {
                if (focus == "grid") { parent.SetGridIcon(loc); }
                if (focus == "button") { parent.SetButtonIcon(loc); }
            }
            this.Close();
        }

        private void del_btn_Click(object sender, EventArgs e)
        {
            if (currentTab != 0) { return; }
            if (SelectedImage == null) { return; }
            DialogResult rez = MessageBox.Show("Souhaitez vous vraiment suppprimer l'image \"" + new FileInfo(SelectedImage.ImageLocation).Name + "\" ?",
                "Attention !", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (rez == System.Windows.Forms.DialogResult.Yes) {
                info_listBox.Items.Clear();
                File.Delete(SelectedImage.ImageLocation);
                SelectedImage.Parent.Controls.Remove(SelectedImage);
            }
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            if (SelectedImage == null) { return; }
            Pix_DoubleClick(SelectedImage, null);
        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            tabControler.SelectTab(0);
            string DeestDir = Program.configDirectory + "\\Images\\_Custom";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Program.configDirectory + "\\Images";
            openFileDialog.Filter = "All images files (*.gif;*.jpg;*.jpeg;*.png)|*.gif;*.jpg;*.jpeg;*.png|GIF files (*.gif)|*.gif|JPEG files (*.jpg;*.jpeg)|*.jpg;*.jpeg|PNG files (*.png)|*.png";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TabPage tabObj = tabControler.TabPages[0];
                FlowLayoutPanel flow = (FlowLayoutPanel)tabObj.Controls[0];
                Size si = new Size(64, 64);
                //Get the path of specified file
                string[] files = openFileDialog.FileNames;
                foreach (string fil in files) {

                    File.Copy(fil, DeestDir+"\\"+ new FileInfo(fil).Name);
                    PictureBox pix = new PictureBox();
                    pix.ImageLocation = DeestDir + "\\" + new FileInfo(fil).Name;
                    pix.Size = si;
                    pix.SizeMode = PictureBoxSizeMode.StretchImage;
                    pix.Click += Pix_Click;
                    flow.Controls.Add(pix);
                }

            }
        }
    }
}
