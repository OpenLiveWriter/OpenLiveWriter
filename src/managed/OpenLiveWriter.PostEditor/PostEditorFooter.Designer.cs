using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor
{
    partial class PostEditorFooter
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelSeparator = new System.Windows.Forms.Label();
            this.labelWordCount = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelStatus, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelSeparator, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelWordCount, 4, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new Padding(0, 0, 5, 0);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(34, 19);
            this.tableLayoutPanel1.TabIndex = 1;
            //
            // flowLayoutPanel
            //
            this.flowLayoutPanel.AccessibleRole = System.Windows.Forms.AccessibleRole.PageTabList;
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel.Location = new System.Drawing.Point(8, 0);
            this.flowLayoutPanel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 6);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel.TabIndex = 2;
            this.flowLayoutPanel.WrapContents = false;
            //
            // labelStatus
            //
            this.labelStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(11, 3);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.UseMnemonic = false;
            //
            // labelSeparator
            //
            this.labelSeparator.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelSeparator.AutoSize = true;
            this.labelSeparator.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelSeparator.Location = new System.Drawing.Point(14, 3);
            this.labelSeparator.Margin = new System.Windows.Forms.Padding(0);
            this.labelSeparator.Name = "labelSeparator";
            this.labelSeparator.Size = new System.Drawing.Size(9, 13);
            this.labelSeparator.TabIndex = 4;
            this.labelSeparator.Text = "|";
            //
            // labelWordCount
            //
            this.labelWordCount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelWordCount.AutoSize = true;
            this.labelWordCount.Location = new System.Drawing.Point(26, 3);
            this.labelWordCount.Name = "labelWordCount";
            this.labelWordCount.Size = new System.Drawing.Size(0, 13);
            this.labelWordCount.TabIndex = 5;
            //
            // PostEditorFooter
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.ForeColor = Color.FromArgb(0x3E, 0x5C, 0x7C);
            this.Name = "PostEditorFooter";
            this.Size = new System.Drawing.Size(34, 19);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelSeparator;
        private System.Windows.Forms.Label labelWordCount;
    }
}
