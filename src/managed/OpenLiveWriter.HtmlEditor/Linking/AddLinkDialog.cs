// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    public class AddLinkDialog : OpenLiveWriter.Controls.ApplicationDialog
    {
        private readonly ICollection currentEntryKeys;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.TextBox txtTitle;
        private CheckBox checkBoxNewWindow;
        private Button buttonAdvanced;
        private GroupLabelControl groupBoxSeparator;
        private ComboBoxRel comboBoxRel;
        private Label labelRel;
        private Panel panelAdvanced;
        private System.ComponentModel.IContainer components = null;

        public AddLinkDialog(ICollection currentEntryKeys)
        {
            this.currentEntryKeys = currentEntryKeys;
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.btnOK.Text = Res.Get(StringId.OKButtonText);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.label1.Text = Res.Get(StringId.LinkGlossaryText);
            this.label2.Text = Res.Get(StringId.LinkGlossaryUrl);
            this.label3.Text = Res.Get(StringId.LinkGlossaryTitleOptional);
            this.Text = Res.Get(StringId.LinkGlossaryTitle);
            this.labelRel.Text = Res.Get(StringId.LinkGlossaryRelOptional);
            checkBoxNewWindow.Text = Res.Get(StringId.LinkNewWindowLabel);
            RefreshAdvancedState(false);

            txtURL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                txtURL.TextAlign = HorizontalAlignment.Right;
        }

        private void RefreshAdvancedState(bool showAdvanced)
        {
            if (!showAdvanced)
            {
                buttonAdvanced.Text =
                    String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.LinkAdvanced), (char)0x00BB);
                panelAdvanced.Visible = _advancedPanelIsShowing = false;
                Height -= panelAdvanced.Height;
            }
            else
            {
                buttonAdvanced.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.LinkAdvanced), (char)0x00AB);
                panelAdvanced.Visible = _advancedPanelIsShowing = true;
                Height += panelAdvanced.Height;
            }
        }
        private bool _advancedPanelIsShowing = false;

        protected override void OnLoad(EventArgs e)
        {
            // call base
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Left, btnOK.Width, int.MaxValue, btnOK, btnCancel, buttonAdvanced);
            }

            // install auto-complete on the address text box
            int result = Shlwapi.SHAutoComplete(txtURL.Handle,
                SHACF.URLALL | SHACF.AUTOSUGGEST_FORCE_ON);

            // ensure we installed it successfully (if we didn't, no biggie -- the user will
            // just not get autocomplete support)
            Debug.Assert(result == HRESULT.S_OK, "Unexpected failure to install AutoComplete");

            // prepopulate the text box w/ http prefix and move the cursor to the end
            if (txtURL.Text == String.Empty)
            {
                txtURL.Text = HTTP_PREFIX;
            }
            txtText.Focus();

            if (!string.IsNullOrEmpty(comboBoxRel.Rel) || !string.IsNullOrEmpty(txtTitle.Text))
                RefreshAdvancedState(true);
        }

        private const string HTTP_PREFIX = "http://";

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

        public bool Edit
        {
            set
            {
                _edit = value;
                if (value)
                {
                    Text = Res.Get(StringId.LinkGlossaryEdit);
                }
            }
        }

        private bool _edit;

        public string LinkText
        {
            get
            {
                return txtText.Text.Trim();
            }
            set
            {
                txtText.Text = value.Trim();
            }
        }

        public string Url
        {
            get
            {
                return txtURL.Text.Trim();
            }
            set
            {
                txtURL.Text = value.Trim();
            }
        }

        public string Title
        {
            get
            {
                return txtTitle.Text.Trim();
            }
            set
            {
                txtTitle.Text = value.Trim();
            }
        }

        public string Rel
        {
            get
            {
                return comboBoxRel.Rel;
            }
            set
            {
                comboBoxRel.Rel = value;
            }
        }

        public bool OpenInNewWindow
        {
            get
            {
                return checkBoxNewWindow.Checked;
            }
            set
            {
                checkBoxNewWindow.Checked = value;
            }
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.checkBoxNewWindow = new System.Windows.Forms.CheckBox();
            this.buttonAdvanced = new System.Windows.Forms.Button();
            this.groupBoxSeparator = new OpenLiveWriter.Controls.GroupLabelControl();
            this.comboBoxRel = new OpenLiveWriter.Controls.ComboBoxRel();
            this.labelRel = new System.Windows.Forms.Label();
            this.panelAdvanced = new System.Windows.Forms.Panel();
            this.panelAdvanced.SuspendLayout();
            this.SuspendLayout();
            //
            // btnOK
            //
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(306, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(73, 23);
            this.btnOK.TabIndex = 18;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(306, 37);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(73, 23);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "Cancel";
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Te&xt:";
            //
            // txtText
            //
            this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtText.Location = new System.Drawing.Point(10, 28);
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(286, 23);
            this.txtText.TabIndex = 2;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "&URL:";
            //
            // txtURL
            //
            this.txtURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtURL.Location = new System.Drawing.Point(10, 73);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(286, 23);
            this.txtURL.TabIndex = 4;
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 15);
            this.label3.TabIndex = 25;
            this.label3.Text = "&Title (optional):";
            //
            // txtTitle
            //
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.Location = new System.Drawing.Point(0, 33);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(286, 23);
            this.txtTitle.TabIndex = 30;
            //
            // checkBoxNewWindow
            //
            this.checkBoxNewWindow.AutoSize = true;
            this.checkBoxNewWindow.Location = new System.Drawing.Point(10, 105);
            this.checkBoxNewWindow.Name = "checkBoxNewWindow";
            this.checkBoxNewWindow.Size = new System.Drawing.Size(160, 19);
            this.checkBoxNewWindow.TabIndex = 10;
            this.checkBoxNewWindow.Text = "Open link in new window";
            this.checkBoxNewWindow.UseVisualStyleBackColor = true;
            //
            // buttonAdvanced
            //
            this.buttonAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdvanced.Location = new System.Drawing.Point(306, 129);
            this.buttonAdvanced.Name = "buttonAdvanced";
            this.buttonAdvanced.Size = new System.Drawing.Size(73, 23);
            this.buttonAdvanced.TabIndex = 20;
            this.buttonAdvanced.Text = "Advanced";
            this.buttonAdvanced.UseVisualStyleBackColor = true;
            this.buttonAdvanced.Click += new System.EventHandler(this.buttonAdvanced_Click);
            //
            // groupBoxSeparator
            //
            this.groupBoxSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSeparator.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSeparator.MultiLine = false;
            this.groupBoxSeparator.Name = "groupBoxSeparator";
            this.groupBoxSeparator.Size = new System.Drawing.Size(371, 10);
            this.groupBoxSeparator.TabIndex = 11;
            //
            // comboBoxRel
            //
            this.comboBoxRel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRel.Location = new System.Drawing.Point(0, 84);
            this.comboBoxRel.Name = "comboBoxRel";
            this.comboBoxRel.Rel = "";
            this.comboBoxRel.Size = new System.Drawing.Size(286, 23);
            this.comboBoxRel.TabIndex = 40;
            //
            // labelRel
            //
            this.labelRel.AutoSize = true;
            this.labelRel.Location = new System.Drawing.Point(0, 67);
            this.labelRel.Name = "labelRel";
            this.labelRel.Size = new System.Drawing.Size(81, 15);
            this.labelRel.TabIndex = 35;
            this.labelRel.Text = "&Rel (optional):";
            //
            // panelAdvanced
            //
            this.panelAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAdvanced.Controls.Add(this.comboBoxRel);
            this.panelAdvanced.Controls.Add(this.txtTitle);
            this.panelAdvanced.Controls.Add(this.labelRel);
            this.panelAdvanced.Controls.Add(this.label3);
            this.panelAdvanced.Controls.Add(this.groupBoxSeparator);
            this.panelAdvanced.Location = new System.Drawing.Point(10, 154);
            this.panelAdvanced.Name = "panelAdvanced";
            this.panelAdvanced.Size = new System.Drawing.Size(371, 108);
            this.panelAdvanced.TabIndex = 15;
            //
            // AddLinkDialog
            //
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(388, 272);
            this.Controls.Add(this.buttonAdvanced);
            this.Controls.Add(this.checkBoxNewWindow);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.txtText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.panelAdvanced);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddLinkDialog";
            this.Text = "Add Link to Glossary";
            this.panelAdvanced.ResumeLayout(false);
            this.panelAdvanced.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            if (LinkText == String.Empty)
            {
                DisplayMessage.Show(MessageId.NoLinkTextSpecified, this);
                txtText.Focus();
            }
            else if (Url.Trim() == String.Empty || Url.Trim() == HTTP_PREFIX)
            {
                DisplayMessage.Show(MessageId.NoValidHyperlinkSpecified, this);
            }
            else if (!_edit && IsAlreadyInGlossary(LinkText.Trim()))
            {
                if (DisplayMessage.Show(MessageId.ConfirmReplaceEntry) == DialogResult.Yes)
                    DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private bool IsAlreadyInGlossary(string entry)
        {
            foreach (string str in currentEntryKeys)
                if (str == entry)
                    return true;
            return false;
        }

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            RefreshAdvancedState(!_advancedPanelIsShowing);
        }
    }
}

