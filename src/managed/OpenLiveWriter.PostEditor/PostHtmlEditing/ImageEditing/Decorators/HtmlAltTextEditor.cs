// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class HtmlAltTextEditor : ImageDecoratorEditor
    {
        private IContainer components = null;

        public HtmlAltTextEditor()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            labelPicture.Text = Res.Get(StringId.ImgAltTextPicture);
            labelAltText.Text = Res.Get(StringId.ImgAltTextAlternateText);
            labelTitle.Text = Res.Get(StringId.ImgAltTextTitle);
            buttonOK.Text = Res.Get(StringId.OKButtonText);
            buttonCancel.Text = Res.Get(StringId.CancelButton);

            Text = Res.Get(StringId.AltTextEditorTitle);

            buttonOK.Click += new EventHandler(buttonOK_Click);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelPicture = new System.Windows.Forms.Label();
            this.labelFilename = new System.Windows.Forms.Label();
            this.labelSize = new System.Windows.Forms.Label();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.labelAltText = new System.Windows.Forms.Label();
            this.textBoxAltText = new System.Windows.Forms.TextBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // labelPicture
            //
            this.labelPicture.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPicture.Location = new System.Drawing.Point(8, 0);
            this.labelPicture.Name = "labelPicture";
            this.labelPicture.Size = new System.Drawing.Size(193, 13);
            this.labelPicture.TabIndex = 0;
            this.labelPicture.Text = "Picture:";
            //
            // labelFilename
            //
            this.labelFilename.AutoSize = true;
            this.labelFilename.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFilename.Location = new System.Drawing.Point(0, 0);
            this.labelFilename.Margin = new Padding(0, 0, 0, 0);
            this.labelFilename.Name = "labelFilename";
            this.labelFilename.Padding = new Padding(0, 0, 0, 0);
            this.labelFilename.Size = new System.Drawing.Size(193, 13);
            this.labelFilename.TabIndex = 1;
            //
            // labelSize
            //
            this.labelSize.AutoSize = true;
            this.labelSize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSize.Location = new System.Drawing.Point(0, 0);
            this.labelSize.Margin = new Padding(0, 0, 0, 0);
            this.labelSize.Name = "labelSize";
            this.labelSize.Padding = new Padding(0, 0, 0, 0);
            this.labelSize.Size = new System.Drawing.Size(193, 13);
            this.labelSize.TabIndex = 2;
            //
            // tableLayoutPanel
            //
            this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.ColumnCount = 3;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.labelFilename, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.labelSize, 2, 0);
            this.tableLayoutPanel.Margin = new Padding(0, 0, 0, 0);
            this.tableLayoutPanel.Padding = new Padding(0, 0, 0, 0);
            this.tableLayoutPanel.Location = new System.Drawing.Point(8, 16);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(193, 20);
            this.tableLayoutPanel.TabIndex = 3;
            //
            // labelAltText
            //
            this.labelAltText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelAltText.Location = new System.Drawing.Point(8, 46);
            this.labelAltText.Name = "labelAltText";
            this.labelAltText.Size = new System.Drawing.Size(193, 13);
            this.labelAltText.TabIndex = 4;
            this.labelAltText.Text = "Alternate &text:";
            //
            // textBoxAltText
            //
            this.textBoxAltText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAltText.Location = new System.Drawing.Point(8, 62);
            this.textBoxAltText.Name = "textBoxAltText";
            this.textBoxAltText.Size = new System.Drawing.Size(193, 20);
            this.textBoxAltText.TabIndex = 5;
            //
            // labelTitle
            //
            this.labelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTitle.Location = new System.Drawing.Point(8, 92);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(193, 13);
            this.labelTitle.TabIndex = 6;
            this.labelTitle.Text = "T&itle:";
            //
            // textBoxTitle
            //
            this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTitle.Location = new System.Drawing.Point(8, 108);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(193, 20);
            this.textBoxTitle.TabIndex = 7;
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(47, 148);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "button1";
            this.buttonOK.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(126, 148);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "button2";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // HtmlAltTextEditor
            //
            this.Controls.Add(this.labelPicture);
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.labelAltText);
            this.Controls.Add(this.textBoxAltText);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "HtmlAltTextEditor";
            this.Size = new System.Drawing.Size(208, 180);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();
            HtmlAltTextSettings = new HtmlAltTextDecoratorSettings(EditorContext.ImgElement);
            labelFilename.Text = HttpUtility.UrlDecode(UrlHelper.GetFileNameForUrl(UrlHelper.SafeToAbsoluteUri(EditorContext.SourceImageUri)));
            labelSize.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.DimensionsFormat), EditorContext.SourceImageSize.Width, EditorContext.SourceImageSize.Height);
            textBoxAltText.Text = HtmlAltTextSettings.AltText;
            textBoxTitle.Text = HtmlAltTextSettings.Title;
        }
        private Label labelPicture;
        private Label labelFilename;
        private Label labelSize;
        private TableLayoutPanel tableLayoutPanel;
        private Label labelAltText;
        private TextBox textBoxAltText;
        private Label labelTitle;
        private TextBox textBoxTitle;
        private Button buttonOK;
        private Button buttonCancel;
        private HtmlAltTextDecoratorSettings HtmlAltTextSettings;

        public override Size GetPreferredSize()
        {
            return new Size(300, 200);
        }

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();
            HtmlAltTextSettings.AltText = textBoxAltText.Text;
            HtmlAltTextSettings.Title = textBoxTitle.Text;
        }

        void buttonOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Form form = FindForm();
            form.AcceptButton = buttonOK;
            form.CancelButton = buttonCancel;

            //Get chrome size BEFORE changing position of controls
            int chromeSize = form.Height - this.Height;

            LayoutHelper.NaturalizeHeightAndDistribute(3, labelPicture, tableLayoutPanel);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelAltText, textBoxAltText);
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelTitle, textBoxTitle);
            LayoutHelper.NaturalizeHeightAndDistribute(10, new ControlGroup(labelPicture, tableLayoutPanel), new ControlGroup(labelAltText, textBoxAltText), new ControlGroup(labelTitle, textBoxTitle));
            LayoutHelper.NaturalizeHeightAndDistribute(30, new ControlGroup(labelPicture, tableLayoutPanel, labelAltText, textBoxAltText, labelTitle, textBoxTitle), new ControlGroup(buttonOK, buttonCancel));
            form.Height = buttonOK.Bottom + chromeSize + (int)Math.Ceiling(DisplayHelper.ScaleY(5));

            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
        }
    }
}

