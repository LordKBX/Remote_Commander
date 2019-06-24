namespace Configurator_Win.MacrosManager
{
    partial class MacroEditor
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
            this.TB1 = new System.Windows.Forms.TableLayoutPanel();
            this.STB1 = new System.Windows.Forms.TableLayoutPanel();
            this.STB1_label = new System.Windows.Forms.Label();
            this.STB1_box = new System.Windows.Forms.TextBox();
            this.STB2 = new System.Windows.Forms.TableLayoutPanel();
            this.STB2_label = new System.Windows.Forms.Label();
            this.STB2_box = new System.Windows.Forms.TextBox();
            this.STB3 = new System.Windows.Forms.Label();
            this.STB4 = new System.Windows.Forms.TableLayoutPanel();
            this.STB5 = new System.Windows.Forms.TableLayoutPanel();
            this.STB5_BTN_ADD = new System.Windows.Forms.Button();
            this.STB5_BTN_CANCEL = new System.Windows.Forms.Button();
            this.STB5_BTN_OK = new System.Windows.Forms.Button();
            this.STB5_SEP_LABEL = new System.Windows.Forms.Label();
            this.TB1.SuspendLayout();
            this.STB1.SuspendLayout();
            this.STB2.SuspendLayout();
            this.STB5.SuspendLayout();
            this.SuspendLayout();
            // 
            // TB1
            // 
            this.TB1.ColumnCount = 1;
            this.TB1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TB1.Controls.Add(this.STB1, 0, 0);
            this.TB1.Controls.Add(this.STB2, 0, 1);
            this.TB1.Controls.Add(this.STB3, 0, 2);
            this.TB1.Controls.Add(this.STB4, 0, 3);
            this.TB1.Controls.Add(this.STB5, 0, 4);
            this.TB1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TB1.Location = new System.Drawing.Point(0, 0);
            this.TB1.Name = "TB1";
            this.TB1.RowCount = 5;
            this.TB1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.TB1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.TB1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.TB1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TB1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.TB1.Size = new System.Drawing.Size(384, 451);
            this.TB1.TabIndex = 0;
            // 
            // STB1
            // 
            this.STB1.ColumnCount = 2;
            this.STB1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.STB1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB1.Controls.Add(this.STB1_label, 0, 0);
            this.STB1.Controls.Add(this.STB1_box, 1, 0);
            this.STB1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB1.Location = new System.Drawing.Point(3, 3);
            this.STB1.Name = "STB1";
            this.STB1.RowCount = 1;
            this.STB1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB1.Size = new System.Drawing.Size(378, 24);
            this.STB1.TabIndex = 0;
            // 
            // STB1_label
            // 
            this.STB1_label.AutoSize = true;
            this.STB1_label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB1_label.Location = new System.Drawing.Point(3, 0);
            this.STB1_label.Name = "STB1_label";
            this.STB1_label.Size = new System.Drawing.Size(94, 24);
            this.STB1_label.TabIndex = 0;
            this.STB1_label.Text = "Nom";
            this.STB1_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // STB1_box
            // 
            this.STB1_box.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB1_box.Location = new System.Drawing.Point(100, 0);
            this.STB1_box.Margin = new System.Windows.Forms.Padding(0);
            this.STB1_box.Name = "STB1_box";
            this.STB1_box.Size = new System.Drawing.Size(278, 20);
            this.STB1_box.TabIndex = 1;
            this.STB1_box.TextChanged += new System.EventHandler(this.STB1_box_TextChanged);
            // 
            // STB2
            // 
            this.STB2.ColumnCount = 2;
            this.STB2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.STB2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB2.Controls.Add(this.STB2_label, 0, 0);
            this.STB2.Controls.Add(this.STB2_box, 1, 0);
            this.STB2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB2.Location = new System.Drawing.Point(3, 33);
            this.STB2.Name = "STB2";
            this.STB2.RowCount = 1;
            this.STB2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB2.Size = new System.Drawing.Size(378, 24);
            this.STB2.TabIndex = 5;
            // 
            // STB2_label
            // 
            this.STB2_label.AutoSize = true;
            this.STB2_label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB2_label.Location = new System.Drawing.Point(3, 0);
            this.STB2_label.Name = "STB2_label";
            this.STB2_label.Size = new System.Drawing.Size(94, 24);
            this.STB2_label.TabIndex = 0;
            this.STB2_label.Text = "Description";
            this.STB2_label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // STB2_box
            // 
            this.STB2_box.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB2_box.Location = new System.Drawing.Point(100, 0);
            this.STB2_box.Margin = new System.Windows.Forms.Padding(0);
            this.STB2_box.Name = "STB2_box";
            this.STB2_box.Size = new System.Drawing.Size(278, 20);
            this.STB2_box.TabIndex = 1;
            // 
            // STB3
            // 
            this.STB3.AutoSize = true;
            this.STB3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.STB3.Location = new System.Drawing.Point(3, 60);
            this.STB3.Name = "STB3";
            this.STB3.Size = new System.Drawing.Size(378, 30);
            this.STB3.TabIndex = 2;
            this.STB3.Text = "Actions";
            this.STB3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // STB4
            // 
            this.STB4.AutoScroll = true;
            this.STB4.BackColor = System.Drawing.Color.White;
            this.STB4.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.STB4.ColumnCount = 1;
            this.STB4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB4.Location = new System.Drawing.Point(0, 90);
            this.STB4.Margin = new System.Windows.Forms.Padding(0);
            this.STB4.Name = "STB4";
            this.STB4.RowCount = 1;
            this.STB4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB4.Size = new System.Drawing.Size(384, 331);
            this.STB4.TabIndex = 3;
            // 
            // STB5
            // 
            this.STB5.ColumnCount = 5;
            this.STB5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.STB5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.STB5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.STB5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.STB5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.STB5.Controls.Add(this.STB5_BTN_ADD, 2, 0);
            this.STB5.Controls.Add(this.STB5_BTN_CANCEL, 0, 0);
            this.STB5.Controls.Add(this.STB5_BTN_OK, 4, 0);
            this.STB5.Controls.Add(this.STB5_SEP_LABEL, 1, 0);
            this.STB5.Dock = System.Windows.Forms.DockStyle.Top;
            this.STB5.Location = new System.Drawing.Point(0, 421);
            this.STB5.Margin = new System.Windows.Forms.Padding(0);
            this.STB5.Name = "STB5";
            this.STB5.RowCount = 1;
            this.STB5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.STB5.Size = new System.Drawing.Size(384, 30);
            this.STB5.TabIndex = 4;
            // 
            // STB5_BTN_ADD
            // 
            this.STB5_BTN_ADD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB5_BTN_ADD.Location = new System.Drawing.Point(142, 0);
            this.STB5_BTN_ADD.Margin = new System.Windows.Forms.Padding(0);
            this.STB5_BTN_ADD.Name = "STB5_BTN_ADD";
            this.STB5_BTN_ADD.Size = new System.Drawing.Size(100, 30);
            this.STB5_BTN_ADD.TabIndex = 3;
            this.STB5_BTN_ADD.Text = "Ajouter Action";
            this.STB5_BTN_ADD.UseVisualStyleBackColor = true;
            this.STB5_BTN_ADD.Click += new System.EventHandler(this.STB5_BTN_ADD_Click);
            // 
            // STB5_BTN_CANCEL
            // 
            this.STB5_BTN_CANCEL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB5_BTN_CANCEL.Location = new System.Drawing.Point(0, 0);
            this.STB5_BTN_CANCEL.Margin = new System.Windows.Forms.Padding(0);
            this.STB5_BTN_CANCEL.Name = "STB5_BTN_CANCEL";
            this.STB5_BTN_CANCEL.Size = new System.Drawing.Size(100, 30);
            this.STB5_BTN_CANCEL.TabIndex = 0;
            this.STB5_BTN_CANCEL.Text = "Cancel";
            this.STB5_BTN_CANCEL.UseVisualStyleBackColor = true;
            this.STB5_BTN_CANCEL.Click += new System.EventHandler(this.STB5_BTN_CANCEL_Click);
            // 
            // STB5_BTN_OK
            // 
            this.STB5_BTN_OK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.STB5_BTN_OK.Location = new System.Drawing.Point(284, 0);
            this.STB5_BTN_OK.Margin = new System.Windows.Forms.Padding(0);
            this.STB5_BTN_OK.Name = "STB5_BTN_OK";
            this.STB5_BTN_OK.Size = new System.Drawing.Size(100, 30);
            this.STB5_BTN_OK.TabIndex = 1;
            this.STB5_BTN_OK.Text = "OK";
            this.STB5_BTN_OK.UseVisualStyleBackColor = true;
            this.STB5_BTN_OK.Click += new System.EventHandler(this.STB5_BTN_OK_Click);
            // 
            // STB5_SEP_LABEL
            // 
            this.STB5_SEP_LABEL.AutoSize = true;
            this.STB5_SEP_LABEL.Location = new System.Drawing.Point(103, 0);
            this.STB5_SEP_LABEL.Name = "STB5_SEP_LABEL";
            this.STB5_SEP_LABEL.Size = new System.Drawing.Size(0, 13);
            this.STB5_SEP_LABEL.TabIndex = 2;
            // 
            // MacroEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 451);
            this.Controls.Add(this.TB1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 490);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 490);
            this.Name = "MacroEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MacroEditor";
            this.TB1.ResumeLayout(false);
            this.TB1.PerformLayout();
            this.STB1.ResumeLayout(false);
            this.STB1.PerformLayout();
            this.STB2.ResumeLayout(false);
            this.STB2.PerformLayout();
            this.STB5.ResumeLayout(false);
            this.STB5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TB1;
        private System.Windows.Forms.TableLayoutPanel STB2;
        private System.Windows.Forms.Label STB2_label;
        private System.Windows.Forms.TextBox STB2_box;
        private System.Windows.Forms.TableLayoutPanel STB1;
        private System.Windows.Forms.Label STB1_label;
        private System.Windows.Forms.TextBox STB1_box;
        private System.Windows.Forms.Label STB3;
        private System.Windows.Forms.TableLayoutPanel STB5;
        private System.Windows.Forms.Button STB5_BTN_ADD;
        private System.Windows.Forms.Button STB5_BTN_CANCEL;
        private System.Windows.Forms.Button STB5_BTN_OK;
        private System.Windows.Forms.Label STB5_SEP_LABEL;
        private System.Windows.Forms.TableLayoutPanel STB4;
    }
}