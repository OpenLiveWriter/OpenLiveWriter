namespace LocEdit
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonLoad = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.buttonFind = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridColKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridColValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridColComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonLoad,
            this.toolStripButtonSave});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip.Size = new System.Drawing.Size(800, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            // 
            // toolStripButtonLoad
            // 
            this.toolStripButtonLoad.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLoad.Image")));
            this.toolStripButtonLoad.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLoad.Name = "toolStripButtonLoad";
            this.toolStripButtonLoad.Size = new System.Drawing.Size(53, 22);
            this.toolStripButtonLoad.Text = "Load";
            this.toolStripButtonLoad.Click += new System.EventHandler(this.ToolStripButtonLoad_Click);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSave.Image")));
            this.toolStripButtonSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(51, 22);
            this.toolStripButtonSave.Text = "Save";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 425);
            this.tableLayoutPanel1.TabIndex = 1;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.TableLayoutPanel1_Paint);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.textBoxFind, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonFind, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 392);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(794, 30);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // textBoxFind
            // 
            this.textBoxFind.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxFind.Location = new System.Drawing.Point(60, 5);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(650, 20);
            this.textBoxFind.TabIndex = 0;
            this.textBoxFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxFind_KeyUp);
            // 
            // buttonFind
            // 
            this.buttonFind.Location = new System.Drawing.Point(716, 3);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(75, 23);
            this.buttonFind.TabIndex = 1;
            this.buttonFind.Text = "Find Next";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.ButtonFind_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "Find Key:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dataGridView
            // 
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridColKey,
            this.dataGridColValue,
            this.dataGridColComment});
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(3, 3);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 72;
            this.dataGridView.Size = new System.Drawing.Size(794, 383);
            this.dataGridView.TabIndex = 1;
            // 
            // dataGridColKey
            // 
            this.dataGridColKey.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridColKey.HeaderText = "Key";
            this.dataGridColKey.MinimumWidth = 9;
            this.dataGridColKey.Name = "dataGridColKey";
            // 
            // dataGridColValue
            // 
            this.dataGridColValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridColValue.HeaderText = "Value";
            this.dataGridColValue.MinimumWidth = 9;
            this.dataGridColValue.Name = "dataGridColValue";
            // 
            // dataGridColComment
            // 
            this.dataGridColComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridColComment.HeaderText = "Comment";
            this.dataGridColComment.MinimumWidth = 9;
            this.dataGridColComment.Name = "dataGridColComment";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "LocEdit";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonLoad;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridColKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridColValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridColComment;
        private System.Windows.Forms.Label label1;
    }
}

