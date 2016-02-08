using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing
{
    partial class PostPropertiesForm
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
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelPageOrder = new System.Windows.Forms.Label();
            this.textPageOrder = new OpenLiveWriter.Controls.NumericTextBox();
            this.labelPageParent = new System.Windows.Forms.Label();
            this.comboPageParent = new OpenLiveWriter.PostEditor.PostPropertyEditing.PageParentComboBox();
            this.labelCategories = new System.Windows.Forms.Label();
            this.categoryDropDown = new OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl.CategoryDropDownControlM1();
            this.labelTags = new System.Windows.Forms.Label();
            this.textTags = new OpenLiveWriter.Controls.AutoCompleteTextbox();
            this.labelPublishDate = new System.Windows.Forms.Label();
            this.datePublishDate = new OpenLiveWriter.PostEditor.PostPropertyEditing.PublishDateTimePicker();
            this.labelComments = new System.Windows.Forms.Label();
            this.comboComments = new System.Windows.Forms.ComboBox();
            this.labelPings = new System.Windows.Forms.Label();
            this.comboPings = new System.Windows.Forms.ComboBox();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.labelSlug = new System.Windows.Forms.Label();
            this.textSlug = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textPassword = new System.Windows.Forms.TextBox();
            this.labelExcerpt = new System.Windows.Forms.Label();
            this.textExcerpt = new System.Windows.Forms.TextBox();
            this.labelTrackbacks = new System.Windows.Forms.Label();
            this.textTrackbacks = new System.Windows.Forms.TextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.panelComboAuthorIsolate = new System.Windows.Forms.Panel();
            this.comboAuthor = new OpenLiveWriter.Controls.DelayedFetchComboBox(this.components);
            this.flowLayoutPanel.SuspendLayout();
            this.panelComboAuthorIsolate.SuspendLayout();
            this.SuspendLayout();
            //
            // flowLayoutPanel
            //
            this.flowLayoutPanel.AutoScroll = true;
            this.flowLayoutPanel.Controls.Add(this.panel1);
            this.flowLayoutPanel.Controls.Add(this.labelPageOrder);
            this.flowLayoutPanel.Controls.Add(this.textPageOrder);
            this.flowLayoutPanel.Controls.Add(this.labelPageParent);
            this.flowLayoutPanel.Controls.Add(this.comboPageParent);
            this.flowLayoutPanel.Controls.Add(this.labelCategories);
            this.flowLayoutPanel.Controls.Add(this.categoryDropDown);
            this.flowLayoutPanel.Controls.Add(this.labelTags);
            this.flowLayoutPanel.Controls.Add(this.textTags);
            this.flowLayoutPanel.Controls.Add(this.labelPublishDate);
            this.flowLayoutPanel.Controls.Add(this.datePublishDate);
            this.flowLayoutPanel.Controls.Add(this.labelComments);
            this.flowLayoutPanel.Controls.Add(this.comboComments);
            this.flowLayoutPanel.Controls.Add(this.labelPings);
            this.flowLayoutPanel.Controls.Add(this.comboPings);
            this.flowLayoutPanel.Controls.Add(this.labelAuthor);
            this.flowLayoutPanel.Controls.Add(this.panelComboAuthorIsolate);
            this.flowLayoutPanel.Controls.Add(this.labelSlug);
            this.flowLayoutPanel.Controls.Add(this.textSlug);
            this.flowLayoutPanel.Controls.Add(this.labelPassword);
            this.flowLayoutPanel.Controls.Add(this.textPassword);
            this.flowLayoutPanel.Controls.Add(this.labelExcerpt);
            this.flowLayoutPanel.Controls.Add(this.textExcerpt);
            this.flowLayoutPanel.Controls.Add(this.labelTrackbacks);
            this.flowLayoutPanel.Controls.Add(this.textTrackbacks);
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(228, 360);
            this.flowLayoutPanel.TabIndex = 0;
            this.flowLayoutPanel.WrapContents = false;
            this.flowLayoutPanel.ClientSizeChanged += new System.EventHandler(this.flowLayoutPanel_ClientSizeChanged);
            //
            // panel1
            //
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 1);
            this.panel1.TabIndex = 10;
            //
            // labelPageOrder
            //
            this.labelPageOrder.AutoSize = true;
            this.labelPageOrder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPageOrder.Location = new System.Drawing.Point(3, 1);
            this.labelPageOrder.Name = "labelPageOrder";
            this.labelPageOrder.Size = new System.Drawing.Size(64, 15);
            this.labelPageOrder.TabIndex = 2;
            this.labelPageOrder.Text = "Page order";
            //
            // textPageOrder
            //
            this.textPageOrder.Location = new System.Drawing.Point(3, 19);
            this.textPageOrder.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.textPageOrder.Name = "textPageOrder";
            this.textPageOrder.Size = new System.Drawing.Size(72, 23);
            this.textPageOrder.TabIndex = 3;
            //
            // labelPageParent
            //
            this.labelPageParent.AutoSize = true;
            this.labelPageParent.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPageParent.Location = new System.Drawing.Point(3, 54);
            this.labelPageParent.Name = "labelPageParent";
            this.labelPageParent.Size = new System.Drawing.Size(70, 15);
            this.labelPageParent.TabIndex = 4;
            this.labelPageParent.Text = "Page parent";
            //
            // comboPageParent
            //
            this.comboPageParent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboPageParent.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboPageParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPageParent.FormattingEnabled = true;
            this.comboPageParent.IntegralHeight = false;
            this.comboPageParent.Location = new System.Drawing.Point(3, 72);
            this.comboPageParent.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.comboPageParent.Name = "comboPageParent";
            this.comboPageParent.Size = new System.Drawing.Size(200, 24);
            this.comboPageParent.TabIndex = 5;
            //
            // labelCategories
            //
            this.labelCategories.AutoSize = true;
            this.labelCategories.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCategories.Location = new System.Drawing.Point(3, 108);
            this.labelCategories.Name = "labelCategories";
            this.labelCategories.Size = new System.Drawing.Size(63, 15);
            this.labelCategories.TabIndex = 6;
            this.labelCategories.Text = "Categories:";
            //
            // categoryDropDown
            //
            this.categoryDropDown.AccessibleName = "Categories";
            this.categoryDropDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categoryDropDown.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.categoryDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryDropDown.IntegralHeight = false;
            this.categoryDropDown.Items.AddRange(new object[] {
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ""});
            this.categoryDropDown.Location = new System.Drawing.Point(3, 126);
            this.categoryDropDown.Margin = new Padding(3, 3, 3, 12);
            this.categoryDropDown.Name = "categoryDropDown";
            this.categoryDropDown.Size = new System.Drawing.Size(200, 24);
            this.categoryDropDown.TabIndex = 7;
            //
            // labelTags
            //
            this.labelTags.AutoSize = true;
            this.labelTags.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTags.Location = new System.Drawing.Point(3, 153);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(32, 15);
            this.labelTags.TabIndex = 8;
            this.labelTags.Text = "Tags:";
            //
            // textTags
            //
            this.textTags.DefaultText = null;
            this.textTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTags.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textTags.Location = new System.Drawing.Point(3, 171);
            this.textTags.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.textTags.Name = "textTags";
            this.textTags.ShowButton = true;
            this.textTags.Size = new System.Drawing.Size(200, 23);
            this.textTags.TabIndex = 9;
            //
            // labelPublishDate
            //
            this.labelPublishDate.AutoSize = true;
            this.labelPublishDate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPublishDate.Location = new System.Drawing.Point(3, 206);
            this.labelPublishDate.Name = "labelPublishDate";
            this.labelPublishDate.Size = new System.Drawing.Size(72, 15);
            this.labelPublishDate.TabIndex = 11;
            this.labelPublishDate.Text = "Publish date:";
            //
            // datePublishDate
            //
            this.datePublishDate.CustomFormat = "M/d/yyyy, h:mm tt";
            this.datePublishDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datePublishDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.datePublishDate.Location = new System.Drawing.Point(3, 224);
            this.datePublishDate.RightToLeftLayout = true;
            this.datePublishDate.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.datePublishDate.Name = "datePublishDate";
            this.datePublishDate.ShowCheckBox = true;
            this.datePublishDate.Size = new System.Drawing.Size(200, 23);
            this.datePublishDate.TabIndex = 12;
            //
            // labelComments
            //
            this.labelComments.AutoSize = true;
            this.labelComments.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelComments.Location = new System.Drawing.Point(3, 259);
            this.labelComments.Name = "labelComments";
            this.labelComments.Size = new System.Drawing.Size(66, 15);
            this.labelComments.TabIndex = 13;
            this.labelComments.Text = "Comments";
            //
            // comboComments
            //
            this.comboComments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboComments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboComments.FormattingEnabled = true;
            this.comboComments.Location = new System.Drawing.Point(3, 277);
            this.comboComments.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.comboComments.Name = "comboComments";
            this.comboComments.Size = new System.Drawing.Size(200, 23);
            this.comboComments.TabIndex = 14;
            //
            // labelPings
            //
            this.labelPings.AutoSize = true;
            this.labelPings.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPings.Location = new System.Drawing.Point(3, 312);
            this.labelPings.Name = "labelPings";
            this.labelPings.Size = new System.Drawing.Size(36, 15);
            this.labelPings.TabIndex = 15;
            this.labelPings.Text = "Pings";
            //
            // comboPings
            //
            this.comboPings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboPings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPings.FormattingEnabled = true;
            this.comboPings.Location = new System.Drawing.Point(3, 330);
            this.comboPings.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.comboPings.Name = "comboPings";
            this.comboPings.Size = new System.Drawing.Size(200, 23);
            this.comboPings.TabIndex = 16;
            //
            // labelAuthor
            //
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelAuthor.Location = new System.Drawing.Point(3, 365);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(44, 15);
            this.labelAuthor.TabIndex = 17;
            this.labelAuthor.Text = "Author";
            //
            // labelSlug
            //
            this.labelSlug.AutoSize = true;
            this.labelSlug.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSlug.Location = new System.Drawing.Point(3, 418);
            this.labelSlug.Name = "labelSlug";
            this.labelSlug.Size = new System.Drawing.Size(30, 15);
            this.labelSlug.TabIndex = 19;
            this.labelSlug.Text = "Slug";
            //
            // textSlug
            //
            this.textSlug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSlug.Location = new System.Drawing.Point(3, 436);
            this.textSlug.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.textSlug.Name = "textSlug";
            this.textSlug.Size = new System.Drawing.Size(200, 23);
            this.textSlug.TabIndex = 20;
            //
            // labelPassword
            //
            this.labelPassword.AutoSize = true;
            this.labelPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPassword.Location = new System.Drawing.Point(3, 471);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(57, 15);
            this.labelPassword.TabIndex = 21;
            this.labelPassword.Text = "Password";
            //
            // textPassword
            //
            this.textPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textPassword.Location = new System.Drawing.Point(3, 489);
            this.textPassword.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.textPassword.Name = "textPassword";
            this.textPassword.Size = new System.Drawing.Size(200, 23);
            this.textPassword.TabIndex = 22;
            //
            // labelExcerpt
            //
            this.labelExcerpt.AutoSize = true;
            this.labelExcerpt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelExcerpt.Location = new System.Drawing.Point(3, 524);
            this.labelExcerpt.Name = "labelExcerpt";
            this.labelExcerpt.Size = new System.Drawing.Size(45, 15);
            this.labelExcerpt.TabIndex = 23;
            this.labelExcerpt.Text = "Excerpt";
            //
            // textExcerpt
            //
            this.textExcerpt.AcceptsReturn = true;
            this.textExcerpt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textExcerpt.Location = new System.Drawing.Point(3, 542);
            this.textExcerpt.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.textExcerpt.Multiline = true;
            this.textExcerpt.Name = "textExcerpt";
            this.textExcerpt.Size = new System.Drawing.Size(200, 81);
            this.textExcerpt.TabIndex = 24;
            //
            // labelTrackbacks
            //
            this.labelTrackbacks.AutoSize = true;
            this.labelTrackbacks.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTrackbacks.Location = new System.Drawing.Point(3, 635);
            this.labelTrackbacks.Name = "labelTrackbacks";
            this.labelTrackbacks.Size = new System.Drawing.Size(66, 15);
            this.labelTrackbacks.TabIndex = 25;
            this.labelTrackbacks.Text = "Trackbacks:";
            //
            // textTrackbacks
            //
            this.textTrackbacks.AcceptsReturn = true;
            this.textTrackbacks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTrackbacks.Location = new System.Drawing.Point(3, 653);
            this.textTrackbacks.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.textTrackbacks.Name = "textTrackbacks";
            this.textTrackbacks.Size = new System.Drawing.Size(200, 23);
            this.textTrackbacks.TabIndex = 26;
            //
            // buttonClose
            //
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.FlatStyle = FlatStyle.System;
            this.buttonClose.Location = new System.Drawing.Point(166, 384);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            //
            // panelComboAuthorIsolate
            //
            this.panelComboAuthorIsolate.Controls.Add(this.comboAuthor);
            this.panelComboAuthorIsolate.Location = new System.Drawing.Point(3, 383);
            this.panelComboAuthorIsolate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.panelComboAuthorIsolate.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.panelComboAuthorIsolate.Name = "panelComboAuthorIsolate";
            this.panelComboAuthorIsolate.Size = new System.Drawing.Size(200, 23);
            this.panelComboAuthorIsolate.TabIndex = 18;
            //
            // comboAuthor
            //
            this.comboAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboAuthor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAuthor.FormattingEnabled = true;
            this.comboAuthor.Location = new System.Drawing.Point(0, 0);
            this.comboAuthor.Margin = new System.Windows.Forms.Padding(0);
            this.comboAuthor.Name = "comboAuthor";
            this.comboAuthor.Size = new System.Drawing.Size(200, 23);
            this.comboAuthor.TabIndex = 19;
            //
            // PostPropertiesForm
            //
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(252, 417);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.flowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "PostPropertiesForm";
            this.Padding = new System.Windows.Forms.Padding(12, 12, 12, 45);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Post Properties";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PostPropertiesForm_FormClosing);
            this.flowLayoutPanel.ResumeLayout(false);
            this.flowLayoutPanel.PerformLayout();
            this.panelComboAuthorIsolate.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelPageOrder;
        private NumericTextBox textPageOrder;
        private System.Windows.Forms.Label labelPageParent;
        private PageParentComboBox comboPageParent;
        private System.Windows.Forms.Label labelCategories;
        private CategoryDropDownControlM1 categoryDropDown;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelTags;
        private AutoCompleteTextbox textTags;
        private System.Windows.Forms.Label labelPublishDate;
        private PublishDateTimePicker datePublishDate;
        private System.Windows.Forms.Label labelComments;
        private System.Windows.Forms.ComboBox comboComments;
        private System.Windows.Forms.Label labelPings;
        private System.Windows.Forms.ComboBox comboPings;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.Label labelSlug;
        private System.Windows.Forms.TextBox textSlug;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textPassword;
        private System.Windows.Forms.Label labelExcerpt;
        private System.Windows.Forms.TextBox textExcerpt;
        private System.Windows.Forms.Label labelTrackbacks;
        private System.Windows.Forms.TextBox textTrackbacks;
        private System.Windows.Forms.Panel panelComboAuthorIsolate;
        private DelayedFetchComboBox comboAuthor;
    }
}