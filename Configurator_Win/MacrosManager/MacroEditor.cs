using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MacroEditor : Form
    {
        public static string StringifyAction(JToken action)
        {
            string strAction = "";
            try
            {
                if (action["type"].Value<string>() == "Sleep") { strAction += "{ Sleep, " + action["delay"].Value<int>() + "ms }"; }
                if (action["type"].Value<string>() == "MuteSound") { strAction += "{ MuteSound }"; }
                if (action["type"].Value<string>() == "VolUp") { strAction += "{ VolUp, Step=" + action["step"].Value<int>() + " }"; }
                if (action["type"].Value<string>() == "VolDown") { strAction += "{ VolDown, Step=" + action["step"].Value<int>() + " }"; }
                if (action["type"].Value<string>() == "KeyBoardInput")
                {
                    strAction += "{ KeyBoardInput, ";
                    bool prev = false;
                    try
                    {
                        string key = action["key"].Value<string>().ToUpper();
                        if (key != "" && key != null) { strAction += key; prev = true; }
                    }
                    catch (Exception error) { }
                    if (prev == true) { strAction += "+"; }
                    //CTRL
                    prev = false;
                    try { if (action["ctrl"].Value<bool>() == true) { strAction += "[CTRL]"; prev = true; } }
                    catch (Exception error) { }
                    if (prev == true) { strAction += "+"; }
                    //ALT
                    prev = false;
                    try { if (action["alt"].Value<bool>() == true) { strAction += "[ALT]"; prev = true; } }
                    catch (Exception error) { }
                    if (prev == true) { strAction += "+"; }
                    //MAJ
                    prev = false;
                    try { if (action["maj"].Value<bool>() == true) { strAction += "[MAJ]"; prev = true; } }
                    catch (Exception error) { }
                    if (prev == true) { strAction += "+"; }
                    //WIN
                    try { if (action["win"].Value<bool>() == true) { strAction += "[WIN]"; } }
                    catch (Exception error) { }
                    if (strAction[strAction.Length - 1] == '+') { strAction = strAction.Remove(strAction.Length - 1, 1); }
                    strAction += " }";
                }
                if (action["type"].Value<string>() == "Execute")
                {
                    strAction += "{ Execute, ";
                    try { strAction += "executable: " + action["executable"].Value<string>(); } catch (Exception error) { }
                    try { strAction += ", params: \"" + action["params"].Value<string>() + "\""; } catch (Exception error) { }
                    strAction += " }";
                }
            }
            catch (Exception error) { }
            return strAction;
        }
        public static string StringifyAllActions(JToken macro) {
            string strActions = "";
            int y = 0;
            List<JToken> actions = macro["actions"].ToList<JToken>();
            if (actions.Count > 2) { strActions = "Macro composée de " + actions.Count + " actions"; }
            else
            {
                foreach (JToken action in actions)
                {
                    if (y > 0) { strActions += ", "; }
                    strActions += StringifyAction(action);
                    y++;
                }
            }

            return strActions;
        }


        private MacrosManager parent;
        private TableLayoutPanel pan;
        private JToken Data;
        public MacroEditor(MacrosManager parent, TableLayoutPanel pan = null)
        {
            this.parent = parent;
            this.pan = pan;

            InitializeComponent();
            if (pan != null)
            {
                Data = parent.GetMacro((string)pan.Tag);
                if (Data == null) { throw new InvalidOperationException(); }
                //Debug.WriteLine(JsonConvert.SerializeObject(this.Data));
                string[] ta = ((string)pan.Tag).Split('/');
                STB1_box.Text = ta[1];
                try { STB2_box.Text = Data["description"].Value<string>(); } catch (Exception err) { }
                try
                {
                    STB4.RowCount = Data["actions"].ToList<JToken>().Count;
                    STB4.RowStyles.Clear();
                    int noline = 0;
                    foreach (JToken action in Data["actions"].ToList<JToken>())
                    {
                        AddUiActionLine(action, noline);
                        noline++;
                    }
                }
                catch (Exception err) { }
            }
            else {
                Data = new JObject();
                Data["description"] = null;
                Data["actions"] = new JArray();
                STB4.RowCount = 0;
                STB4.RowStyles.Clear();
            }
        }

        public void NewAction(JToken action) {
            int cp = STB4.Controls.Count - 1;
            //if (STB4.Controls[cp].Name != "LINE") { }
            //else {
                AddUiActionLine(action, STB4.Controls.Count);
            //}
            UpdateActionsListByTabCells();
            Debug.WriteLine(JsonConvert.SerializeObject(Data["actions"]));
        }

        public void UpdateAction(JToken action, TableLayoutPanel pan) {
            pan.Tag = action;
            ((Label)pan.Controls[0]).Text = MacroEditor.StringifyAction(action);
            UpdateActionsListByTabCells();
            Debug.WriteLine(JsonConvert.SerializeObject(Data["actions"]));
        }

        private void AddUiActionLine(JToken action, int noline) {
            Padding p = new Padding();
            p.All = 0;
            Padding p2 = new Padding();
            p2.All = 4;

            //Debug.WriteLine(JsonConvert.SerializeObject(action));
            string strAction = MacroEditor.StringifyAction(action);

            TableLayoutPanel tab1 = new TableLayoutPanel();
            tab1.SuspendLayout();
            tab1.Name = "LINE";
            tab1.Tag = action;
            tab1.BackColor = Color.White;
            tab1.ForeColor = Color.Black;
            tab1.Size = new Size(STB4.Size.Width - 18, 30);
            tab1.ColumnCount = 4;
            tab1.RowCount = 1;
            tab1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tab1.ForeColor = Color.Black;
            tab1.Padding = p;
            tab1.Margin = p;
            tab1.Dock = DockStyle.Top;
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            tab1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));

            Label lab = new Label();
            lab.Text = strAction;
            lab.Dock = DockStyle.Fill;
            lab.Margin = p;
            lab.Padding = p;
            lab.TextAlign = ContentAlignment.MiddleCenter;
            tab1.Controls.Add(lab, 0, 0);

            TableLayoutPanel tab2 = new TableLayoutPanel();
            tab2.SuspendLayout();
            tab2.BackColor = Color.White;
            tab2.ForeColor = Color.Black;
            tab2.Size = new Size(30, 30);
            tab2.ColumnCount = 1;
            tab2.RowCount = 2;
            tab2.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tab2.ForeColor = Color.Black;
            tab2.Padding = p;
            tab2.Margin = p;
            tab2.Dock = DockStyle.Fill;
            tab2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            tab2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));

            Button up = new Button();
            up.Text = "";
            up.Font = new Font("Segoe MDL2 Assets", 6f);
            up.Margin = p;
            up.Padding = p;
            up.Size = new Size(30, 15);
            up.BackColor = Color.Silver;
            up.Dock = DockStyle.Fill;
            up.Click += Up_Click;
            tab2.Controls.Add(up, 0, 0);

            Button down = new Button();
            down.Text = "";
            down.Font = new Font("Segoe MDL2 Assets", 6f);
            down.Margin = p;
            down.Padding = p;
            down.Size = new Size(30, 15);
            down.BackColor = Color.Silver;
            down.Dock = DockStyle.Fill;
            down.Click += Down_Click;
            tab2.Controls.Add(down, 0, 1);

            tab2.CausesValidation = true;
            tab2.ResumeLayout(true);
            tab1.Controls.Add(tab2, 1, 0);

            Button btn = new Button();
            btn.Text = "";
            btn.Font = new Font("Segoe MDL2 Assets", 9f);
            btn.Margin = p;
            btn.Padding = p;
            btn.Size = new Size(50, 30);
            btn.BackColor = Color.Silver;
            btn.Dock = DockStyle.Fill;
            btn.Click += Btn_Click;
            tab1.Controls.Add(btn, 2, 0);

            Button btn2 = new Button();
            btn2.Text = "X";
            btn2.Margin = p;
            btn2.Padding = p;
            btn2.Size = new Size(50, 30);
            btn2.BackColor = Color.Silver;
            btn2.Dock = DockStyle.Fill;
            btn2.Click += Btn2_Click;
            tab1.Controls.Add(btn2, 3, 0);

            tab1.CausesValidation = true;
            tab1.ResumeLayout(true);

            STB4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            STB4.Controls.Add(tab1, 0, noline);
            STB4.ResumeLayout(true);
        }

        private void Up_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            TableLayoutPanel pani = (TableLayoutPanel)btn.Parent.Parent;
            TableLayoutPanelCellPosition a = STB4.GetCellPosition(pani);
            if (a.Row == 0) { return; }
            TableLayoutPanel pani2 = (TableLayoutPanel)STB4.GetControlFromPosition(a.Column, a.Row - 1);
            STB4.SetCellPosition(pani, new TableLayoutPanelCellPosition(a.Column, a.Row -1));
            STB4.SetCellPosition(pani2, a);
            UpdateActionsListByTabCells();
        }

        private void Down_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            TableLayoutPanel pani = (TableLayoutPanel)btn.Parent.Parent;
            TableLayoutPanelCellPosition a = STB4.GetCellPosition(pani);
            if (a.Row + 1 >= STB4.RowCount) { return; }
            TableLayoutPanel pani2 = (TableLayoutPanel)STB4.GetControlFromPosition(a.Column, a.Row + 1);
            STB4.SetCellPosition(pani, new TableLayoutPanelCellPosition(a.Column, a.Row + 1));
            STB4.SetCellPosition(pani2, a);
            UpdateActionsListByTabCells();
        }

        private void UpdateActionsListByTabCells() {
            JArray ar = null;
            try { ar = (JArray)Data["actions"]; } catch (Exception error) { ar = new JArray(); }
            ar.Clear();
            foreach (Control line in STB4.Controls) {
                TableLayoutPanelCellPosition pos = STB4.GetCellPosition(line);
                while(pos.Row >= ar.Count) { ar.Add(null); }
                ar[pos.Row] = (JToken)line.Tag;
            }
            Data["actions"] = ar;
            //Debug.WriteLine(JsonConvert.SerializeObject(Data));
        }

        private void Btn2_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            TableLayoutPanel pani = (TableLayoutPanel)btn.Parent;
            int max = STB4.Controls.Count;
            TableLayoutPanelCellPosition pos = STB4.GetCellPosition(pani);
            TableLayoutPanelCellPosition pos2;
            TableLayoutPanel[] poss = new TableLayoutPanel[STB4.Controls.Count];
            foreach(Control line in STB4.Controls)
            {
                pos2 = STB4.GetCellPosition(line);
                poss[pos2.Row] = (TableLayoutPanel)line;
            }
            STB4.Controls.Remove(pani);
            for (int i = pos.Row; i < STB4.Controls.Count; i++) {
                STB4.SetCellPosition(poss[i + 1], new TableLayoutPanelCellPosition(0, i));
            }
            UpdateActionsListByTabCells();
            Debug.WriteLine(JsonConvert.SerializeObject(Data["actions"]));
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ActionEditor ae = new ActionEditor(this, (TableLayoutPanel)btn.Parent);
            ae.ShowDialog();
            ae.Dispose();
        }

        private void STB1_box_TextChanged(object sender, EventArgs e)
        {
            string pattern = "[^a-zA-Z0-9]+"; string pattern2 = "^[0-9]";
            Regex reg_exp = new Regex(pattern); Regex reg_exp2 = new Regex(pattern2);
            TextBox box = (TextBox)sender; int pos = box.SelectionStart;
            string tx = reg_exp.Replace(box.Text, ""); string tx2 = reg_exp2.Replace(tx, "");
            if (tx != tx2) { box.Text = tx2; box.SelectionStart = pos - 1; }
            else { box.Text = tx; box.SelectionStart = pos; }
            box.SelectionLength = 0;
        }

        private void STB5_BTN_CANCEL_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void STB5_BTN_ADD_Click(object sender, EventArgs e)
        {
            ActionEditor ae = new ActionEditor(this, null);
            ae.ShowDialog();
            ae.Dispose();
        }

        private void STB5_BTN_OK_Click(object sender, EventArgs e)
        {
            UpdateActionsListByTabCells();
            string macroName = STB1_box.Text;
            Data["description"] = STB2_box.Text;
            if (this.pan == null) {
                parent.SetMacro(null, macroName, Data);
            }
            else
            {
                string[] ta = ((string)pan.Tag).Split('/');
                parent.SetMacro(ta[1], macroName, Data);
            }
            this.Close();
        }
    }
}
