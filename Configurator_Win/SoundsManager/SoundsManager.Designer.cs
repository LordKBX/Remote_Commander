namespace Configurator_Win.SoundsManager
{
    partial class SoundsManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControler = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.GR_MAIN = new System.Windows.Forms.TableLayoutPanel();
            this.GR_RIGHT = new System.Windows.Forms.TableLayoutPanel();
            this.main_label = new System.Windows.Forms.Label();
            this.GRR2 = new System.Windows.Forms.TableLayoutPanel();
            this.info_label = new System.Windows.Forms.Label();
            this.add_btn = new System.Windows.Forms.Button();
            this.info_listBox = new System.Windows.Forms.ListBox();
            this.del_btn = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SelectBtn = new System.Windows.Forms.Button();
            this.tabControler.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.GR_MAIN.SuspendLayout();
            this.GR_RIGHT.SuspendLayout();
            this.GRR2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControler
            // 
            this.tabControler.Controls.Add(this.tabPage1);
            this.tabControler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControler.Location = new System.Drawing.Point(3, 3);
            this.tabControler.Name = "tabControler";
            this.tabControler.SelectedIndex = 0;
            this.tabControler.Size = new System.Drawing.Size(594, 444);
            this.tabControler.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.flowLayoutPanel2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(586, 418);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "_Custom";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoScroll = true;
            this.flowLayoutPanel2.BackColor = System.Drawing.Color.Beige;
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(580, 412);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // GR_MAIN
            // 
            this.GR_MAIN.ColumnCount = 2;
            this.GR_MAIN.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.GR_MAIN.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.GR_MAIN.Controls.Add(this.GR_RIGHT, 1, 0);
            this.GR_MAIN.Controls.Add(this.tabControler, 0, 0);
            this.GR_MAIN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GR_MAIN.Location = new System.Drawing.Point(0, 0);
            this.GR_MAIN.Name = "GR_MAIN";
            this.GR_MAIN.RowCount = 1;
            this.GR_MAIN.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.GR_MAIN.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.GR_MAIN.Size = new System.Drawing.Size(800, 450);
            this.GR_MAIN.TabIndex = 2;
            // 
            // GR_RIGHT
            // 
            this.GR_RIGHT.BackColor = System.Drawing.Color.Transparent;
            this.GR_RIGHT.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.GR_RIGHT.ColumnCount = 1;
            this.GR_RIGHT.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.GR_RIGHT.Controls.Add(this.main_label, 0, 0);
            this.GR_RIGHT.Controls.Add(this.GRR2, 0, 1);
            this.GR_RIGHT.Controls.Add(this.info_listBox, 0, 2);
            this.GR_RIGHT.Controls.Add(this.del_btn, 0, 3);
            this.GR_RIGHT.Controls.Add(this.flowLayoutPanel1, 0, 4);
            this.GR_RIGHT.Controls.Add(this.SelectBtn, 0, 5);
            this.GR_RIGHT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GR_RIGHT.Location = new System.Drawing.Point(600, 0);
            this.GR_RIGHT.Margin = new System.Windows.Forms.Padding(0);
            this.GR_RIGHT.Name = "GR_RIGHT";
            this.GR_RIGHT.RowCount = 6;
            this.GR_RIGHT.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.GR_RIGHT.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.GR_RIGHT.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.GR_RIGHT.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.GR_RIGHT.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.GR_RIGHT.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.GR_RIGHT.Size = new System.Drawing.Size(200, 450);
            this.GR_RIGHT.TabIndex = 1;
            // 
            // main_label
            // 
            this.main_label.AutoSize = true;
            this.main_label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.main_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.main_label.Location = new System.Drawing.Point(4, 1);
            this.main_label.Name = "main_label";
            this.main_label.Size = new System.Drawing.Size(192, 30);
            this.main_label.TabIndex = 0;
            this.main_label.Text = "label1";
            this.main_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GRR2
            // 
            this.GRR2.ColumnCount = 2;
            this.GRR2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.GRR2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.GRR2.Controls.Add(this.info_label, 0, 0);
            this.GRR2.Controls.Add(this.add_btn, 1, 0);
            this.GRR2.Location = new System.Drawing.Point(1, 32);
            this.GRR2.Margin = new System.Windows.Forms.Padding(0);
            this.GRR2.Name = "GRR2";
            this.GRR2.RowCount = 1;
            this.GRR2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.GRR2.Size = new System.Drawing.Size(194, 30);
            this.GRR2.TabIndex = 1;
            // 
            // info_label
            // 
            this.info_label.AutoSize = true;
            this.info_label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.info_label.Location = new System.Drawing.Point(3, 3);
            this.info_label.Margin = new System.Windows.Forms.Padding(3);
            this.info_label.Name = "info_label";
            this.info_label.Size = new System.Drawing.Size(148, 24);
            this.info_label.TabIndex = 0;
            this.info_label.Text = "Informations Fichier";
            this.info_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // add_btn
            // 
            this.add_btn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.add_btn.Location = new System.Drawing.Point(157, 3);
            this.add_btn.Name = "add_btn";
            this.add_btn.Size = new System.Drawing.Size(34, 24);
            this.add_btn.TabIndex = 1;
            this.add_btn.Text = "+";
            this.add_btn.UseVisualStyleBackColor = true;
            this.add_btn.Click += new System.EventHandler(this.add_btn_Click);
            // 
            // info_listBox
            // 
            this.info_listBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.info_listBox.FormattingEnabled = true;
            this.info_listBox.Items.AddRange(new object[] {
            "info",
            "info2"});
            this.info_listBox.Location = new System.Drawing.Point(1, 63);
            this.info_listBox.Margin = new System.Windows.Forms.Padding(0);
            this.info_listBox.Name = "info_listBox";
            this.info_listBox.Size = new System.Drawing.Size(198, 60);
            this.info_listBox.TabIndex = 2;
            // 
            // del_btn
            // 
            this.del_btn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.del_btn.Location = new System.Drawing.Point(4, 127);
            this.del_btn.Name = "del_btn";
            this.del_btn.Size = new System.Drawing.Size(192, 24);
            this.del_btn.TabIndex = 3;
            this.del_btn.Text = "Supprimer Image";
            this.del_btn.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 158);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(192, 257);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // SelectBtn
            // 
            this.SelectBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectBtn.Location = new System.Drawing.Point(4, 422);
            this.SelectBtn.Name = "SelectBtn";
            this.SelectBtn.Size = new System.Drawing.Size(192, 24);
            this.SelectBtn.TabIndex = 5;
            this.SelectBtn.Text = "Selectionner";
            this.SelectBtn.UseVisualStyleBackColor = true;
            this.SelectBtn.Click += new System.EventHandler(this.SelectBtn_Click);
            // 
            // SoundsManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.GR_MAIN);
            this.Name = "SoundsManager";
            this.Text = "SoundsManager";
            this.tabControler.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.GR_MAIN.ResumeLayout(false);
            this.GR_RIGHT.ResumeLayout(false);
            this.GR_RIGHT.PerformLayout();
            this.GRR2.ResumeLayout(false);
            this.GRR2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControler;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel GR_MAIN;
        private System.Windows.Forms.TableLayoutPanel GR_RIGHT;
        private System.Windows.Forms.Label main_label;
        private System.Windows.Forms.TableLayoutPanel GRR2;
        private System.Windows.Forms.Label info_label;
        private System.Windows.Forms.Button add_btn;
        private System.Windows.Forms.ListBox info_listBox;
        private System.Windows.Forms.Button del_btn;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button SelectBtn;
    }
}