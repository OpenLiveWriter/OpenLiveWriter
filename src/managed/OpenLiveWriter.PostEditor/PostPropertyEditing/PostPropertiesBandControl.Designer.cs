using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    partial class PostPropertiesBandControl
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
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.categoryDropDown = new OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.CategoryDropDownControlM1();
            this.textTags = new OpenLiveWriter.Controls.AutoCompleteTextbox();
            this.datePublishDate = new OpenLiveWriter.PostEditor.PostPropertyEditing.PublishDateTimePicker();
            this.labelPageParent = new System.Windows.Forms.Label();
            this.comboPageParent = new OpenLiveWriter.PostEditor.PostPropertyEditing.PageParentComboBox();
            this.labelPageOrder = new System.Windows.Forms.Label();
            this.textPageOrder = new OpenLiveWriter.Controls.NumericTextBox();
            this.linkViewAll = new System.Windows.Forms.LinkLabel();
            this.panelShadow = new System.Windows.Forms.Panel();
            this.table.SuspendLayout();
            this.SuspendLayout();
            // 
            // table
            // 
            this.table.BackColor = System.Drawing.Color.Transparent;
            this.table.ColumnCount = 9;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.Controls.Add(this.categoryDropDown, 0, 0);
            this.table.Controls.Add(this.textTags, 1, 0);
            this.table.Controls.Add(this.datePublishDate, 2, 0);
            this.table.Controls.Add(this.labelPageParent, 3, 0);
            this.table.Controls.Add(this.comboPageParent, 4, 0);
            this.table.Controls.Add(this.labelPageOrder, 5, 0);
            this.table.Controls.Add(this.textPageOrder, 6, 0);
            this.table.Controls.Add(this.linkViewAll, 8, 0);
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Name = "table";
            this.table.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.table.RowCount = 1;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table.Size = new System.Drawing.Size(1090, 41);
            this.table.TabIndex = 0;
            // 
            // categoryDropDown
            // 
            this.categoryDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryDropDown.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.categoryDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryDropDown.FormattingEnabled = true;
            this.categoryDropDown.IntegralHeight = false;
            this.categoryDropDown.Items.AddRange(new object[] {
            "",
            "",
            "",
            ""});
            this.categoryDropDown.Location = new System.Drawing.Point(8, 10);
            this.categoryDropDown.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.categoryDropDown.Name = "categoryDropDown";
            this.categoryDropDown.Size = new System.Drawing.Size(144, 21);
            this.categoryDropDown.TabIndex = 0;
            // 
            // textTags
            // 
            this.textTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textTags.DefaultText = null;
            this.textTags.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textTags.Location = new System.Drawing.Point(160, 10);
            this.textTags.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.textTags.Name = "textTags";
            this.textTags.ShowButton = true;
            this.textTags.Size = new System.Drawing.Size(198, 20);
            this.textTags.TabIndex = 1;
            // 
            // datePublishDate
            // 
            this.datePublishDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.datePublishDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.datePublishDate.Location = new System.Drawing.Point(366, 10);
            this.datePublishDate.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.datePublishDate.Name = "datePublishDate";
            this.datePublishDate.RightToLeftLayout = true;
            this.datePublishDate.ShowCheckBox = true;
            this.datePublishDate.Size = new System.Drawing.Size(182, 20);
            this.datePublishDate.TabIndex = 2;
            // 
            // labelPageParent
            // 
            this.labelPageParent.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelPageParent.AutoSize = true;
            this.labelPageParent.Location = new System.Drawing.Point(556, 14);
            this.labelPageParent.Margin = new System.Windows.Forms.Padding(0);
            this.labelPageParent.Name = "labelPageParent";
            this.labelPageParent.Size = new System.Drawing.Size(68, 13);
            this.labelPageParent.TabIndex = 3;
            this.labelPageParent.Text = "Page parent:";
            // 
            // comboPageParent
            // 
            this.comboPageParent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboPageParent.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboPageParent.FormattingEnabled = true;
            this.comboPageParent.IntegralHeight = false;
            this.comboPageParent.Location = new System.Drawing.Point(624, 10);
            this.comboPageParent.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.comboPageParent.Name = "comboPageParent";
            this.comboPageParent.Size = new System.Drawing.Size(121, 20);
            this.comboPageParent.TabIndex = 4;
            // 
            // labelPageOrder
            // 
            this.labelPageOrder.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelPageOrder.AutoSize = true;
            this.labelPageOrder.Location = new System.Drawing.Point(753, 14);
            this.labelPageOrder.Margin = new System.Windows.Forms.Padding(0);
            this.labelPageOrder.Name = "labelPageOrder";
            this.labelPageOrder.Size = new System.Drawing.Size(62, 13);
            this.labelPageOrder.TabIndex = 5;
            this.labelPageOrder.Text = "Page order:";
            // 
            // textPageOrder
            // 
            this.textPageOrder.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textPageOrder.Location = new System.Drawing.Point(815, 10);
            this.textPageOrder.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.textPageOrder.Name = "textPageOrder";
            this.textPageOrder.Size = new System.Drawing.Size(63, 20);
            this.textPageOrder.TabIndex = 6;
            // 
            // linkViewAll
            // 
            this.linkViewAll.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(142)))), ((int)(((byte)(213)))));
            this.linkViewAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.linkViewAll.AutoSize = true;
            this.linkViewAll.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkViewAll.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(142)))), ((int)(((byte)(213)))));
            this.linkViewAll.Location = new System.Drawing.Point(1038, 14);
            this.linkViewAll.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.linkViewAll.Name = "linkViewAll";
            this.linkViewAll.Size = new System.Drawing.Size(43, 13);
            this.linkViewAll.TabIndex = 7;
            this.linkViewAll.TabStop = true;
            this.linkViewAll.Text = "View all";
            this.linkViewAll.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(142)))), ((int)(((byte)(213)))));
            this.linkViewAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkViewAll_LinkClicked);
            // 
            // panelShadow
            // 
            this.panelShadow.BackColor = System.Drawing.Color.Transparent;
            this.panelShadow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelShadow.Location = new System.Drawing.Point(0, 41);
            this.panelShadow.Name = "panelShadow";
            this.panelShadow.Size = new System.Drawing.Size(1090, 4);
            this.panelShadow.TabIndex = 1;
            // 
            // PostPropertiesBandControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.table);
            this.Controls.Add(this.panelShadow);
            this.Name = "PostPropertiesBandControl";
            this.Size = new System.Drawing.Size(1090, 45);
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel table;
        private CategoryDropDownControlM1 categoryDropDown;
        private AutoCompleteTextbox textTags;
        private System.Windows.Forms.Label labelPageOrder;
        private PublishDateTimePicker datePublishDate;
        private System.Windows.Forms.Label labelPageParent;
        private PageParentComboBox comboPageParent;
        private NumericTextBox textPageOrder;
        private Panel panelShadow;
        private LinkLabel linkViewAll;
    }
}
