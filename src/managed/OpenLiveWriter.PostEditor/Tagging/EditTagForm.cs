// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Tagging
{
    /// <summary>
    /// Summary description for CreateTagForm.
    /// </summary>
    public class EditTagForm : BaseForm
    {
        public TagProvider Provider
        {
            get
            {
                TagProvider provider;
                if (_id != null)
                    provider = new TagProvider(_id);
                else
                    provider = new TagProvider();

                provider.Name = textBoxName.Text;
                provider.HtmlFormat = textBoxHtmlFormat.Text;
                provider.Caption = textBoxCaption.Text;
                provider.Separator = textBoxSeparator.Text;
                return provider;
            }
            set
            {
                _id = value.Id;
                textBoxName.Text = value.Name;
                if (value.Name != null && value.Name != string.Empty)
                    Text = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.TagsEditPattern), value.Name);
                textBoxCaption.Text = value.Caption;
                textBoxHtmlFormat.Text = value.HtmlFormat;
                textBoxSeparator.Text = value.Separator;
                if (!value.New)
                    _captionDirty = true;
            }
        }

        private Button buttonOk;
        private Button buttonCancel;
        private Label label1;
        private Label label5;
        private TextBox textBoxSeparator;
        private Label labelName;
        private TextBox textBoxName;
        private TextBox textBoxHtmlFormat;
        private Label label4;
        private TextBox textBoxCaption;
        private TextBox textBoxPreview;
        private Label label2;
        private PictureBox pictureBox1;
        private HelpProvider helpProviderCreateTag;
        private string _id;
        private Panel panelDetails;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public EditTagForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            textBoxHtmlFormat.RightToLeft = RightToLeft.No;
            textBoxSeparator.RightToLeft = RightToLeft.No;
            textBoxCaption.RightToLeft = RightToLeft.No;
            textBoxPreview.RightToLeft = RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
            {
                textBoxHtmlFormat.TextAlign =
                    textBoxSeparator.TextAlign =
                    textBoxCaption.TextAlign = textBoxPreview.TextAlign = HorizontalAlignment.Right;
            }

            buttonOk.Text = Res.Get(StringId.OKButtonText);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            label1.Text = Res.Get(StringId.TagsHtmlTemplateLabel);
            helpProviderCreateTag.SetHelpString(textBoxHtmlFormat,
                                                string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.TagsHtmlTemplateHelpString), "{tag}",
                                                              "{tag-encoded}"));
            label5.Text = Res.Get(StringId.TagsHtmlDelimiterLabel);
            helpProviderCreateTag.SetHelpString(textBoxSeparator, Res.Get(StringId.TagsHtmlDelimiterHelpString));
            labelName.Text = Res.Get(StringId.TagsProviderNameLabel);
            helpProviderCreateTag.SetHelpString(textBoxName, Res.Get(StringId.TagsProviderNameHelpString));
            label4.Text = Res.Get(StringId.TagsHtmlCaptionLabel);
            helpProviderCreateTag.SetHelpString(textBoxCaption,
                                                string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.TagsHtmlCaptionHelpString), "{tag-group}"));
            label2.Text = Res.Get(StringId.TagsHtmlPreviewLabel);
            helpProviderCreateTag.SetHelpString(textBoxPreview, Res.Get(StringId.TagsHtmlPreviewHelpString));
            Text = Res.Get(StringId.TagsCreateNew);

            LayoutHelper.FixupOKCancel(buttonOk, buttonCancel);
            SinkEvents();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int origHeight = panelDetails.Height;
            using (new AutoGrow(panelDetails, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, panelDetails.Controls);

                LayoutHelper.NaturalizeHeightAndDistribute(2, labelName, textBoxName);
                LayoutHelper.NaturalizeHeightAndDistribute(8, textBoxName, label1);

                LayoutHelper.NaturalizeHeightAndDistribute(2, label1, textBoxHtmlFormat);
                LayoutHelper.NaturalizeHeightAndDistribute(8, textBoxHtmlFormat, label5);

                LayoutHelper.NaturalizeHeightAndDistribute(2, label5, textBoxSeparator);
                LayoutHelper.NaturalizeHeightAndDistribute(8, textBoxSeparator, label4);

                LayoutHelper.NaturalizeHeightAndDistribute(2, label4, textBoxCaption);
                LayoutHelper.NaturalizeHeightAndDistribute(8, textBoxCaption, label2);

                LayoutHelper.NaturalizeHeightAndDistribute(2, label2, textBoxPreview);
            }
            Height += panelDetails.Height - origHeight;

            LayoutHelper.FixupOKCancel(buttonOk, buttonCancel);

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            UnSinkEvents();
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditTagForm));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxHtmlFormat = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSeparator = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxCaption = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.helpProviderCreateTag = new System.Windows.Forms.HelpProvider();
            this.panelDetails = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelDetails.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOk
            //
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOk.Location = new System.Drawing.Point(232, 376);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 50;
            this.buttonOk.Text = "OK";
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(312, 376);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 55;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // textBoxHtmlFormat
            //
            this.textBoxHtmlFormat.AcceptsReturn = true;
            this.textBoxHtmlFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProviderCreateTag.SetHelpString(this.textBoxHtmlFormat, resources.GetString("textBoxHtmlFormat.HelpString"));
            this.textBoxHtmlFormat.Location = new System.Drawing.Point(0, 64);
            this.textBoxHtmlFormat.Multiline = true;
            this.textBoxHtmlFormat.Name = "textBoxHtmlFormat";
            this.helpProviderCreateTag.SetShowHelp(this.textBoxHtmlFormat, true);
            this.textBoxHtmlFormat.Size = new System.Drawing.Size(296, 64);
            this.textBoxHtmlFormat.TabIndex = 15;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(0, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(288, 16);
            this.label1.TabIndex = 10;
            this.label1.Text = "&HTML template for each tag:";
            //
            // label5
            //
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label5.Location = new System.Drawing.Point(0, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(288, 16);
            this.label5.TabIndex = 20;
            this.label5.Text = "&Separate the HTML for each tag using:";
            //
            // textBoxSeparator
            //
            this.textBoxSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProviderCreateTag.SetHelpString(this.textBoxSeparator, "The separator that should appear between tag HTML. The separator will appear betw" +
                    "een the HTML that is generated for each tag.");
            this.textBoxSeparator.Location = new System.Drawing.Point(0, 152);
            this.textBoxSeparator.Name = "textBoxSeparator";
            this.helpProviderCreateTag.SetShowHelp(this.textBoxSeparator, true);
            this.textBoxSeparator.Size = new System.Drawing.Size(296, 23);
            this.textBoxSeparator.TabIndex = 25;
            //
            // labelName
            //
            this.labelName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelName.Location = new System.Drawing.Point(0, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(288, 16);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "&Provider name:";
            //
            // textBoxName
            //
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProviderCreateTag.SetHelpString(this.textBoxName, "The name of the tag provider. This name will be displayed when inserting tags.");
            this.textBoxName.Location = new System.Drawing.Point(0, 16);
            this.textBoxName.Name = "textBoxName";
            this.helpProviderCreateTag.SetShowHelp(this.textBoxName, true);
            this.textBoxName.Size = new System.Drawing.Size(296, 23);
            this.textBoxName.TabIndex = 5;
            //
            // textBoxCaption
            //
            this.textBoxCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProviderCreateTag.SetHelpString(this.textBoxCaption, "The HTML caption that will appear with the HTML generated for the tags. Use {tag-" +
                    "group} where you\'d like the HTML generated from the HTML tag template to be plac" +
                    "ed.");
            this.textBoxCaption.Location = new System.Drawing.Point(0, 200);
            this.textBoxCaption.Multiline = true;
            this.textBoxCaption.Name = "textBoxCaption";
            this.helpProviderCreateTag.SetShowHelp(this.textBoxCaption, true);
            this.textBoxCaption.Size = new System.Drawing.Size(296, 40);
            this.textBoxCaption.TabIndex = 35;
            //
            // label4
            //
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label4.Location = new System.Drawing.Point(0, 184);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(288, 16);
            this.label4.TabIndex = 30;
            this.label4.Text = "HTML &caption for tag list:";
            //
            // textBoxPreview
            //
            this.textBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProviderCreateTag.SetHelpString(this.textBoxPreview, "A preview of the literal HTML that will be generated for a set of tags.");
            this.textBoxPreview.Location = new System.Drawing.Point(0, 264);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.helpProviderCreateTag.SetShowHelp(this.textBoxPreview, true);
            this.textBoxPreview.Size = new System.Drawing.Size(296, 88);
            this.textBoxPreview.TabIndex = 45;
            this.textBoxPreview.TabStop = false;
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(0, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(288, 16);
            this.label2.TabIndex = 40;
            this.label2.Text = "HTML preview:";
            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 24);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(72, 64);
            this.pictureBox1.TabIndex = 46;
            this.pictureBox1.TabStop = false;
            //
            // panelDetails
            //
            this.panelDetails.Controls.Add(this.label5);
            this.panelDetails.Controls.Add(this.label2);
            this.panelDetails.Controls.Add(this.label4);
            this.panelDetails.Controls.Add(this.textBoxHtmlFormat);
            this.panelDetails.Controls.Add(this.textBoxSeparator);
            this.panelDetails.Controls.Add(this.label1);
            this.panelDetails.Controls.Add(this.textBoxPreview);
            this.panelDetails.Controls.Add(this.textBoxName);
            this.panelDetails.Controls.Add(this.textBoxCaption);
            this.panelDetails.Controls.Add(this.labelName);
            this.panelDetails.Location = new System.Drawing.Point(88, 8);
            this.panelDetails.Name = "panelDetails";
            this.panelDetails.Size = new System.Drawing.Size(296, 352);
            this.panelDetails.TabIndex = 0;
            //
            // EditTagForm
            //
            this.AcceptButton = this.buttonOk;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(402, 416);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.panelDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditTagForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create New Tag Provider";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private bool Valid()
        {
            if (textBoxName.Text.Trim() == string.Empty)
            {
                DisplayMessage.Show(MessageId.TagProviderNameRequired);
                textBoxName.Focus();
                return false;
            }

            if (textBoxHtmlFormat.Text == string.Empty)
            {
                DisplayMessage.Show(MessageId.TagFormatRequired);
                textBoxHtmlFormat.Focus();
                return false;
            }

            if (textBoxCaption.Text.IndexOf(TagProvider.TAG_GROUP_TOKEN) < 0)
            {
                DisplayMessage.Show(MessageId.TagMissingToken, TagProvider.TAG_GROUP_TOKEN);
                textBoxCaption.Focus();
                return false;
            }

            return true;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (Valid())
                DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SinkEvents()
        {
            textBoxCaption.TextChanged += new EventHandler(This_TextChanged);
            textBoxHtmlFormat.TextChanged += new EventHandler(This_TextChanged);
            textBoxName.TextChanged += new EventHandler(TextBoxName_TextChanged);
            textBoxSeparator.TextChanged += new EventHandler(This_TextChanged);
            textBoxCaption.KeyDown += new KeyEventHandler(textBoxCaption_KeyDown);
        }

        private void UnSinkEvents()
        {
            textBoxCaption.TextChanged -= new EventHandler(This_TextChanged);
            textBoxHtmlFormat.TextChanged -= new EventHandler(This_TextChanged);
            textBoxName.TextChanged -= new EventHandler(TextBoxName_TextChanged);
            textBoxSeparator.TextChanged -= new EventHandler(This_TextChanged);
            textBoxCaption.KeyDown -= new KeyEventHandler(textBoxCaption_KeyDown);
        }

        private void GeneratePreview()
        {
            textBoxPreview.Text =
                Provider.GenerateHtmlForTags(new string[] { Res.Get(StringId.TagsTagExample1), Res.Get(StringId.TagsTagExample2) });
        }

        private void TextBoxName_TextChanged(object sender, EventArgs e)
        {
            if (!_captionDirty)
            {
                if (_captionText == null)
                    _captionText = textBoxCaption.Text;
                textBoxCaption.Text = textBoxName.Text + " " + _captionText;
            }
            GeneratePreview();
        }

        private void This_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private bool _captionDirty = false;
        private string _captionText = null;

        private void textBoxCaption_KeyDown(object sender, KeyEventArgs e)
        {
            _captionDirty = true;
        }
    }
}
