using System;
using System.Collections;
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

namespace Configurator_Win.MacrosManager
{
    public partial class MacrosManager : Form
    {
        private string FilePath;
        private Form1 parent;
        private JObject MacroList;
        private TableLayoutPanel selectedLayout = null;
        private int PreviousSelectedTab = 0;
        public MacrosManager(Form1 parent = null)
        {
            FilePath = Program.configDirectory + "\\macros.json";
            this.parent = parent;
            InitializeComponent();
            if (parent != null)
            {
                BTN_ADD_SECTION.Hide();
                BTN_ADD_MACRO.Hide();
                BTN_DEL_MACRO.Hide();
            }
            else
            {
                BTN_ADD_MACRO.Hide();
                BTN_DEL_MACRO.Hide();
                BTN_VALID_SELECTION.Text = "Enregistrer Tout";
            }
            BTN_ADD_SECTION.Click += BTN_ADD_SECTION_Click;
            BTN_ADD_MACRO.Click += BTN_ADD_MACRO_Click;
            BTN_DEL_MACRO.Click += BTN_DEL_MACRO_Click;
            BTN_VALID_SELECTION.Click += BTN_VALID_SELECTION_Click;

            string macrofile = System.IO.File.ReadAllText(FilePath);
            MacroList = JObject.Parse(macrofile);

            tabControler.TabPages.Clear();
            tabControler.SelectedIndexChanged += TabControler_SelectedIndexChanged;
            TableLayoutPanel pan = null;

            foreach (JToken section in MacroList["sections"].ToList<JToken>())
            {
                pan = NewTab(section["name"].Value<string>());
                IList<string> keys = section["macros"].Value<JObject>().Properties().Select(p => p.Name).ToList();
                foreach (string key in keys) {
                    CreateLine(pan, section["macros"][key], key);
                }
            }

        }

        public JToken GetMacro(string MacroPath) {
            string[] ta = MacroPath.Split('/');
            if (ta.Length < 2) { return null; }

            foreach (JToken section in MacroList["sections"].ToList<JToken>())
            {
                if (section["name"].Value<string>() == ta[0])
                {
                    IList<string> keys = section["macros"].Value<JObject>().Properties().Select(p => p.Name).ToList();
                    foreach (string key in keys)
                    {
                        if (key == ta[1]) {
                            return section["macros"][key];
                        }
                    }

                }
            }
            return null;
        }

        public void SetMacro(string NameOrigin, string Name, JToken data)
        {
            if (NameOrigin != Name && NameOrigin != null) {
                JObject jo = MacroList["sections"][tabControler.SelectedIndex]["macros"].Value<JObject>();
                if (jo.ContainsKey(NameOrigin) == true) {
                    foreach (Control ct in tabControler.TabPages[tabControler.SelectedIndex].Controls[0].Controls)
                    {
                        string[] ta = ((string)ct.Tag).Split('/');
                        if (ta[1] == NameOrigin)
                        {
                            foreach (Control li in ct.Controls)
                            {
                                if (li.Name.StartsWith("Btn2_")) { Btn2_Click(li, null); }
                            }
                        }
                    }
                }
            }
            MacroList["sections"][tabControler.SelectedIndex]["macros"][Name] = data;
            if (NameOrigin != null)
            {
                foreach (Control ct in tabControler.TabPages[tabControler.SelectedIndex].Controls[0].Controls) {
                    string[] ta = ((string)ct.Tag).Split('/');
                    if (ta[1] == NameOrigin) {
                        TableLayoutPanel pt = (TableLayoutPanel)ct;
                        pt.Tag = ta[0] + "/" + Name;
                        foreach (Control li in pt.Controls) {
                            if (li.Name.StartsWith("Lab_"))  { li.Tag = ta[0] + "/" + Name; ((Label)li).Text = Name; }
                            if (li.Name.StartsWith("Lab2_")) {
                                li.Tag = ta[0] + "/" + Name;
                                string desc = data["description"].Value<string>();
                                if (desc == "") { ((Label)li).Text = "No Description"; ((Label)li).Font = new Font("Arial", 8f, FontStyle.Italic); }
                                else { ((Label)li).Text = desc; ((Label)li).Font = new Font("Arial", 8f, FontStyle.Regular); }
                            }
                            if (li.Name.StartsWith("Lab3_")) { li.Tag = ta[0] + "/" + Name; ((Label)li).Text = MacroEditor.StringifyAllActions(data); }
                        }
                    }
                }
            }
            else { CreateLine((TableLayoutPanel)tabControler.TabPages[tabControler.SelectedIndex].Controls[0], MacroList["sections"][tabControler.SelectedIndex]["macros"][Name], Name); }
        }

        private void BTN_VALID_SELECTION_Click(object sender, EventArgs e)
        {
            if (parent != null)
            {
                Debug.WriteLine("BTN_VALID_SELECTION_Click");
                Debug.WriteLine("tabControler.SelectedIndex = " + tabControler.SelectedIndex);
                foreach (Control ct in tabControler.TabPages[tabControler.SelectedIndex].Controls[0].Controls)
                {
                    if (ct.BackColor == Color.Aqua)
                    {
                        parent.SetMacro((string)ct.Tag);
                        this.Close();
                    }
                }
            }
            else
            {
                try
                {
                    System.IO.File.WriteAllText(Program.configDirectory + "\\macros.json", JsonConvert.SerializeObject(MacroList));
                    MessageBox.Show("Le Fichier des Macros à été enregistré avec succès", "Information");
                    this.Close();
                }
                catch (Exception error) { MessageBox.Show("Le Fichier des Macros na pas été enregistré Correctement ou à complètement échoué", "Erreur"); }
            }
        }

        private void BTN_DEL_MACRO_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("BTN_DEL_MACRO_Click");
            if (tabControler.TabPages[tabControler.SelectedIndex].Text.StartsWith("_") == true)
            {
                Debug.WriteLine("tabControler.SelectedIndex = " + tabControler.SelectedIndex);
                List<CheckBox> boxs = new List<CheckBox>();
                foreach (Control ct in tabControler.TabPages[tabControler.SelectedIndex].Controls[0].Controls) {
                    if (((CheckBox)ct.Controls[0]).Checked == true) { boxs.Add(((CheckBox)ct.Controls[0])); }
                }
                if (boxs.Count > 0) {
                    foreach (CheckBox box in boxs) {
                        TableLayoutPanel pan = (TableLayoutPanel)box.Parent;
                        string[] tma = ((string)pan.Tag).Split('/');
                        try
                        {
                            MacroList["sections"][tabControler.SelectedIndex]["macros"].Children().OfType<JProperty>().Where(attr => attr.Name == tma[1]).ToList().ForEach(attr => attr.Remove());
                            pan.Parent.Controls.Remove(pan);
                        }
                        catch (Exception err) { }
                    }
                }
            }
        }

        private void BTN_ADD_MACRO_Click(object sender, EventArgs e)
        {
            if (tabControler.TabPages[tabControler.SelectedIndex].Text.StartsWith("_") == true)
            {
                MacroEditor mac = new MacroEditor(this, null);
                mac.ShowDialog();
                mac.Dispose();
            }
        }

        private void BTN_ADD_SECTION_Click(object sender, EventArgs e)
        {
            List<JToken> j = MacroList["sections"].ToList<JToken>();
            string rez = null;
            DialogResult input = InputBox1.ShowInputDialog("Nom de la nouvelle section", ref rez);
            if (input == DialogResult.OK) {
                JToken yo = JToken.Parse("{\"name\": \"_"+rez+"\",\"macros\":{ } }");
                MacroList["sections"].Last.AddAfterSelf(yo);
                NewTab("_" + rez);
                tabControler.SelectedIndex = tabControler.TabCount - 1;
            }
        }

        private void UnselectAll(int TabIndex)
        {
            foreach (Control ctr in tabControler.Controls[TabIndex].Controls[0].Controls)
            {
                ctr.BackColor = Color.White;
            }
            selectedLayout = null;
        }
        
        private void TabControler_SelectedIndexChanged(object sender, EventArgs e)
        {
            UnselectAll(PreviousSelectedTab);
            PreviousSelectedTab = tabControler.SelectedIndex;
            if (parent != null) {
                BTN_ADD_SECTION.Hide();
                BTN_ADD_MACRO.Hide();
                BTN_DEL_MACRO.Hide();
            }
            else
            {
                BTN_ADD_SECTION.Show();
                if (tabControler.TabPages[tabControler.SelectedIndex].Text.StartsWith("_") == true) {
                    BTN_ADD_MACRO.Show();
                    BTN_DEL_MACRO.Show();
                }
                else
                {
                    BTN_ADD_MACRO.Hide();
                    BTN_DEL_MACRO.Hide();
                }
            }
        }

        private TableLayoutPanel NewTab(string section)
        {
            Padding p = new Padding();
            p.All = 0;
            Padding p2 = new Padding();
            p2.All = 4;

            TabPage tab = new TabPage();
            tab.Text = section;
            tab.Padding = p;
            tab.Margin = p;
            TableLayoutPanel pan = new TableLayoutPanel();
            pan.Padding = p;
            pan.Margin = p;
            pan.Dock = DockStyle.Fill;
            pan.AutoScroll = true;
            //pan.Resize += Pan_Resize;
            pan.ColumnCount = 1;
            pan.RowCount = 0;
            pan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tab.Controls.Add(pan);
            tabControler.TabPages.Add(tab);
            return pan;
        }

        private void Pan_Resize(object sender, EventArgs e)
        {
            FlowLayoutPanel pan = (FlowLayoutPanel)sender;
            for (int i=0; i < pan.Controls.Count; i++)
            {
                pan.Controls[i].Size = new Size(pan.Size.Width - 18, 30);
            }
        }

        public void CreateLine(TableLayoutPanel pan, JToken data, string Name)
        {
            string section = pan.Parent.Text;
            Padding p = new Padding();
            p.All = 0;
            Padding p2 = new Padding();
            p2.All = 4;
            TableLayoutPanel tab1 = new TableLayoutPanel();
            tab1.SuspendLayout();
            tab1.Tag = section + "/" + Name;
            tab1.Name = Name;
            tab1.BackColor = Color.White;
            tab1.ForeColor = Color.Black;
            tab1.Size = new Size(pan.Size.Width - 18, 30);
            tab1.ColumnCount = 3;
            tab1.RowCount = 1;
            tab1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tab1.ForeColor = Color.Black;
            tab1.Padding = p;
            tab1.Margin = p;
            tab1.Dock = DockStyle.Top;
            tab1.CausesValidation = true;
            tab1.ColumnStyles.Clear();
            tab1.Click += Tab1_Click;
            tab1.DoubleClick += Tab1_DoubleClick;

            if (parent == null && section.StartsWith("_") == true)
            {
                tab1.ColumnCount = 6;
                CheckBox box = new CheckBox();
                box.Name = "Check_"+Name;
                box.Dock = DockStyle.Fill;
                box.Margin = p;
                box.Padding = p2;
                box.TextAlign = ContentAlignment.MiddleCenter;
                box.Click += Box_Click;
                tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                tab1.Controls.Add(box, 0, 0);
            }

            Label lab = new Label();
            lab.Text = Name;
            lab.Name = "Lab_" + Name;
            lab.Dock = DockStyle.Fill;
            lab.Margin = p;
            lab.Padding = p;
            lab.TextAlign = ContentAlignment.MiddleCenter;
            lab.Click += Lab_Click;
            lab.DoubleClick += Lab_DoubleClick;
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tab1.Controls.Add(lab, (parent == null && section.StartsWith("_") == true)?1:0, 0);
            
            Label lab2 = new Label();
            lab2.Name = "Lab2_" + Name;
            string desc = "";
            try { desc = data["description"].Value<string>(); } catch (Exception error) { }
            if (desc != "") { lab2.Text = desc; }
            else
            {
                lab2.Text = "No Description";
                lab2.Font = new Font("Arial", 8f, FontStyle.Italic);
            }

            lab2.Dock = DockStyle.Fill;
            lab2.Margin = p;
            lab2.Padding = p;
            lab2.TextAlign = ContentAlignment.MiddleCenter;
            lab2.Click += Lab_Click;
            lab2.DoubleClick += Lab_DoubleClick;
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            tab1.Controls.Add(lab2, (parent == null && section.StartsWith("_") == true)?2:1, 0);
            
            Label lab3 = new Label();
            lab3.Name = "Lab3_" + Name;
            lab3.Text = MacroEditor.StringifyAllActions(data);
            lab3.Dock = DockStyle.Fill;
            lab3.Margin = p;
            lab3.Padding = p;
            lab3.TextAlign = ContentAlignment.MiddleCenter;
            lab3.Click += Lab_Click;
            lab3.DoubleClick += Lab_DoubleClick;
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tab1.Controls.Add(lab3, (parent == null && section.StartsWith("_") == true)?3:2, 0);

            if (parent == null && section.StartsWith("_") == true)
            {
                Button btn = new Button();
                btn.Text = "";
                btn.Font = new Font("Segoe MDL2 Assets", 9f);
                btn.Margin = p;
                btn.Padding = p;
                btn.BackColor = Color.Silver;
                btn.Dock = DockStyle.Fill;
                btn.Click += Btn_Click;
                tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
                tab1.Controls.Add(btn, 4, 0);

                Button btn2 = new Button();
                btn2.Name = "Btn2_" + Name;
                btn2.Text = "X";
                btn2.Margin = p;
                btn2.Padding = p;
                btn2.BackColor = Color.Silver;
                btn2.Dock = DockStyle.Fill;
                btn2.Click += Btn2_Click;
                tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
                tab1.Controls.Add(btn2, 5, 0);
            }
            pan.RowCount += 1;
            pan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            pan.Controls.Add(tab1);
            tab1.ResumeLayout(true);
            pan.ResumeLayout(true);
        }

        private void Tab1_Click(object sender, EventArgs e)
        {
            TableLayoutPanel table = (TableLayoutPanel)sender;
            UnselectAll(tabControler.SelectedIndex);
            table.BackColor = Color.Aqua;
            selectedLayout = table;
            Debug.WriteLine((string)table.Tag);
            table.Focus();
        }

        private void Tab1_DoubleClick(object sender, EventArgs e)
        {
            if (parent == null) { return; }
        }

        private void Box_Click(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            Tab1_Click(box.Parent, null);
        }

        private void Lab_Click(object sender, EventArgs e)
        {
            Label lab = (Label)sender;

            Tab1_Click(lab.Parent, null);
        }

        private void Lab_DoubleClick(object sender, EventArgs e)
        {
            Tab1_DoubleClick( ((Label)sender).Parent, null);
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Tab1_Click(btn.Parent, null);
            MacroEditor mac = new MacroEditor(this, (TableLayoutPanel)btn.Parent);
            mac.ShowDialog();
            mac.Dispose();
        }

        private void Btn2_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Btn2_Click");
            Button btn = (Button)sender;
            if (tabControler.TabPages[tabControler.SelectedIndex].Text.StartsWith("_") == true)
            {
                string[] tma = ((string)btn.Parent.Tag).Split('/');
                Debug.WriteLine("tma[1] = " + tma[1]);
                try
                {
                    MacroList["sections"][tabControler.SelectedIndex]["macros"].Children().OfType<JProperty>()
                        .Where(attr => attr.Name == tma[1]).ToList().ForEach(attr => attr.Remove());
                    btn.Parent.Parent.Controls.Remove(btn.Parent);
                    Debug.WriteLine(JsonConvert.SerializeObject(MacroList["sections"][tabControler.SelectedIndex]));
                }
                catch (Exception err)
                {
                    Debug.WriteLine("Btn2_Click error");
                }
            }
        }
    }
}
