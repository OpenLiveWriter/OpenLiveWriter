// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.HtmlEditor.Linking.Commands;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    public interface IHyperlinkForm : IDisposable
    {
        string LinkText { get; set; }
        string Hyperlink { get; set; }
        string LinkTitle { get; set; }
        string Rel { get; set; }
        bool NewWindow { get; set; }
        bool EditStyle { set; }
        DialogResult ShowDialog(IWin32Window owner);
    }

    /// <summary>
    /// Summary description for HyperlinkForm.
    /// </summary>
    public class HyperlinkForm : ApplicationDialog, IHyperlinkForm
    {
        private Button buttonInsert;
        private Button buttonCancel;
        private Label label1;
        private TextBox textBoxAddress;
        private Label labelLinkText;
        private TextBox textBoxLinkText;
        private Label labelTitle;
        private TextBox textBoxTitle;
        private Label labelRel;
        private ComboBoxRel comboBoxRel;
        private CheckBox ckboxNewWindow;
        private GroupLabelControl bevel;

        private LinkOptionsButton btnOptions;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.Button btnRemove;
        private System.ComponentModel.IContainer components;
        private ToolTip2 btnOptionsToolTip;
        private System.Windows.Forms.CheckBox ckBoxGlossary;

        private bool _isNotTextLink = false;

        private bool _isMaxed;

        public readonly CommandManager CommandManager;

        private readonly bool _slimOptions;

        public HyperlinkForm(CommandManager commandManager, bool showAllOptions)
        {
            _slimOptions = !showAllOptions;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            CommandManager = commandManager;
            buttonInsert.Text = Res.Get(StringId.InsertButtonText);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            label1.Text = Res.Get(StringId.UrlLabel);
            labelLinkText.Text = Res.Get(StringId.LinkTextLabel);
            labelTitle.Text = Res.Get(StringId.LinkTitleLabel);
            labelRel.Text = Res.Get(StringId.LinkRelLabel);
            ckboxNewWindow.Text = Res.Get(StringId.LinkNewWindowLabel);
            btnOptions.Text = " " + Res.Get(StringId.LinkLinkTo);
            btnOptions.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.LinkLinkTo));
            btnOptionsToolTip.SetToolTip(this.btnOptions, Res.Get(StringId.LinkOptionsTooltip));
            btnRemove.Text = Res.Get(StringId.LinkRemoveLink);
            ckBoxGlossary.Text = Res.Get(StringId.LinkAddToGlossary);
            this.Text = Res.Get(StringId.LinkFormTitle);

            CommandManager.BeginUpdate();

            CommandManager.Add(new CommandGlossary());

            CommandManager.EndUpdate();

            textBoxAddress.GotFocus += new EventHandler(textBoxAddress_GotFocus);
            textBoxAddress.RightToLeft = System.Windows.Forms.RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                textBoxAddress.TextAlign = HorizontalAlignment.Right;
        }

        /// <summary>
        /// Override OnLoad
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnLoad(EventArgs e)
        {
            // call base
            base.OnLoad(e);

            // scale size of btnOptions based on text
            if (!_slimOptions)
            {
                int existingWidth = btnOptions.Width;
                int preferredWidth = btnOptions.GetPreferredWidth();
                btnOptions.Left = btnOptions.Right - preferredWidth;
                btnOptions.Width = preferredWidth;
                textBoxAddress.Width = btnOptions.Left - 5 - textBoxAddress.Left;
            }
            else
            {
                textBoxAddress.Width = btnOptions.Right - textBoxAddress.Left;
                btnOptions.Visible = false;

                labelRel.Visible = false;
                comboBoxRel.Visible = false;
                labelTitle.Visible = false;
                textBoxTitle.Visible = false;
                ckboxNewWindow.Visible = false;
                ckBoxGlossary.Visible = false;
                btnAdvanced.Visible = false;
                ClientSize = new Size(ClientSize.Width, textBoxLinkText.Bottom + textBoxLinkText.Left);
            }

            if (btnRemove != null)
            {
                DisplayHelper.AutoFitSystemButton(btnRemove, buttonInsert.Width, int.MaxValue);
                if (btnAdvanced != null)
                {
                    btnRemove.Left = btnAdvanced.Left - btnRemove.Width - (int)Math.Round(DisplayHelper.ScaleX(8));
                }
            }

            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Left, buttonInsert.Width, int.MaxValue, buttonInsert, buttonCancel);
            }

            //now, need to move the advanced button over
            if (btnAdvanced.Visible)
            {
                SetAdvancedText();
                DisplayHelper.AutoFitSystemButton(btnAdvanced, buttonInsert.Width, int.MaxValue);
                btnAdvanced.Left = buttonInsert.Right - btnAdvanced.Width;
            }

            // install auto-complete on the address text box
            int result = Shlwapi.SHAutoComplete(textBoxAddress.Handle,
                SHACF.URLALL | SHACF.AUTOSUGGEST_FORCE_ON);

            // ensure we installed it successfully (if we didn't, no biggie -- the user will
            // just not get autocomplete support)
            Debug.Assert(result == HRESULT.S_OK, "Unexpected failure to install AutoComplete");

            // prepopulate the text box w/ http prefix and move the cursor to the end
            if (textBoxAddress.Text == String.Empty)
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        string clipboardText = Clipboard.GetText();
                        if (Regex.IsMatch(clipboardText ?? "", "^https?://", RegexOptions.IgnoreCase)
                            && UrlHelper.IsUrl(clipboardText))
                        {
                            textBoxAddress.Text = clipboardText;
                            textBoxAddress.Select(0, textBoxAddress.TextLength);
                        }
                    }
                }
                catch (ExternalException)
                {
                }
                catch (ThreadStateException)
                {
                }
            }

            if (textBoxAddress.Text == String.Empty)
            {
                textBoxAddress.Text = HTTP_PREFIX;
                textBoxAddress.Select(HTTP_PREFIX.Length, 0);
            }

            //decide whether it should be maximized
            ShowAdvancedOptions = (LinkSettings.ShowAdvancedOptions || Rel != String.Empty || LinkTitle != String.Empty)
                && comboBoxRel.Visible;

            //use new window sticky setting if this isn't an edit
            if (!_editStyle)
                NewWindow = LinkSettings.OpenInNewWindow;

        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                if (textBoxLinkText.Text.Trim() != "")
                {
                    ActiveControl = textBoxAddress;
                    textBoxAddress.Focus();
                }
            }
        }

        public string LinkText
        {
            get
            {
                string linkText = textBoxLinkText.Text.Trim();
                if (linkText == string.Empty)
                    linkText = Hyperlink;
                return linkText;
            }
            set
            {
                if (!_isNotTextLink)
                    textBoxLinkText.Text = value.Trim();
            }
        }

        public bool ShowAdvancedOptions
        {
            get
            {
                return _isMaxed;
            }
            set
            {
                _isMaxed = value && !_slimOptions;
                handleState();
            }
        }

        /// <summary>
        /// Get the hyperlink specified by the user
        /// </summary>
        public string Hyperlink
        {
            get
            {
                return textBoxAddress.Text.Trim();
            }
            set
            {
                textBoxAddress.Text = value.Trim();
            }
        }

        /// <summary>
        /// Get the title specified by the user
        /// </summary>
        public string LinkTitle
        {
            get
            {
                return textBoxTitle.Text.Trim();
            }
            set
            {
                textBoxTitle.Text = value.Trim();
            }
        }

        /// <summary>
        /// Get the rel specified by the user
        /// </summary>
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

        public bool NewWindow
        {
            get
            {
                return ckboxNewWindow.Checked;
            }
            set
            {
                ckboxNewWindow.Checked = value;
            }
        }

        public bool IsInGlossary
        {
            set
            {
                ckBoxGlossary.Visible = !value && !_slimOptions;
            }
        }

        public bool EditStyle
        {
            set
            {
                _editStyle = value;
                btnRemove.Visible = value && !_isNotTextLink;
                if (value)
                {
                    Text = Res.Get(StringId.LinkEditHyperlink);
                    buttonInsert.Text = Res.Get(StringId.OKButtonText);
                }
            }
        }
        private bool _editStyle = false;

        public bool ContainsImage
        {
            set
            {
                if (value)
                {
                    _isNotTextLink = true;
                    textBoxLinkText.Enabled = false;
                    textBoxLinkText.Text = Res.Get(StringId.LinkSelectionContainsImage);
                    ckBoxGlossary.Visible = false;
                    ckBoxGlossary.Enabled = false;
                    btnRemove.Visible = false;
                    btnRemove.Enabled = false;
                }
            }
        }

        private void SetAdvancedText()
        {
            if (_isMaxed)
                btnAdvanced.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.LinkAdvanced), (char)0x00AB);
            else
                btnAdvanced.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.LinkAdvanced), (char)0x00BB);
        }

        private void handleState()
        {

            SetAdvancedText();
            if (!_slimOptions)
                ClientSize = new Size(ClientSize.Width, Convert.ToInt32(DisplayHelper.ScaleY(_isMaxed ? 300 : 196)));

            bevel.Visible = _isMaxed;
            labelTitle.Visible = _isMaxed;
            textBoxTitle.Visible = _isMaxed;
            labelRel.Visible = _isMaxed;
            comboBoxRel.Visible = _isMaxed;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (LinkText == String.Empty)

            {
                DisplayMessage.Show(MessageId.NoLinkTextSpecified, this);
                textBoxLinkText.Focus();
            }
            else if (Hyperlink.Trim() == String.Empty || Hyperlink.Trim().ToUpper(CultureInfo.InvariantCulture) == HTTP_PREFIX.ToUpper(CultureInfo.InvariantCulture))
            {
                DisplayMessage.Show(MessageId.NoValidHyperlinkSpecified, this);
                textBoxAddress.Focus();
            }
            else
            {
                DialogResult = DialogResult.OK;
            }

            //need to check whether this needs to be added/removed from the Glossary
            if (DialogResult == DialogResult.OK)
            {
                if (ckBoxGlossary.Checked)
                {
                    if (GlossaryManager.Instance.ContainsEntry(LinkText))
                    {
                        if (DisplayMessage.Show(MessageId.ConfirmReplaceEntry) == DialogResult.Yes)
                            GlossaryManager.Instance.AddEntry(LinkText, Hyperlink, textBoxTitle.Text, comboBoxRel.Text, ckboxNewWindow.Checked);
                    }
                    else
                        GlossaryManager.Instance.AddEntry(LinkText, Hyperlink, textBoxTitle.Text, comboBoxRel.Text, ckboxNewWindow.Checked);

                }
                LinkSettings.OpenInNewWindow = NewWindow;
                LinkSettings.ShowAdvancedOptions = ShowAdvancedOptions;

            }
        }

        private void textBoxAddress_GotFocus(object sender, EventArgs e)
        {
            if (Hyperlink == HTTP_PREFIX)
                textBoxAddress.Select(HTTP_PREFIX.Length, 0);
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HyperlinkForm));
            this.buttonInsert = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.bevel = new OpenLiveWriter.Controls.GroupLabelControl();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.labelLinkText = new System.Windows.Forms.Label();
            this.textBoxLinkText = new System.Windows.Forms.TextBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.labelRel = new System.Windows.Forms.Label();
            this.comboBoxRel = new OpenLiveWriter.Controls.ComboBoxRel();
            this.ckboxNewWindow = new System.Windows.Forms.CheckBox();
            this.btnOptions = new OpenLiveWriter.HtmlEditor.Linking.LinkOptionsButton();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnOptionsToolTip = new ToolTip2(components);
            this.ckBoxGlossary = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            //
            // buttonInsert
            //
            this.buttonInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInsert.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonInsert.Location = new System.Drawing.Point(370, 10);
            this.buttonInsert.Name = "buttonInsert";
            this.buttonInsert.Size = new System.Drawing.Size(75, 23);
            this.buttonInsert.TabIndex = 12;
            this.buttonInsert.Text = "OK";
            this.buttonInsert.Click += new System.EventHandler(this.buttonOk_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(370, 39);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            //
            // bevel
            //
            this.bevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.bevel.Location = new System.Drawing.Point(6, 191);
            this.bevel.MultiLine = false;
            this.bevel.Name = "bevel";
            this.bevel.Size = new System.Drawing.Size(435, 4);
            this.bevel.TabIndex = 0;
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(10, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(312, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "&URL:";
            //
            // textBoxAddress
            //
            this.textBoxAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAddress.Location = new System.Drawing.Point(10, 28);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(258, 21);
            this.textBoxAddress.TabIndex = 2;
            this.textBoxAddress.TextChanged += new System.EventHandler(this.textBoxAddress_TextChanged);
            //
            // labelLinkText
            //
            this.labelLinkText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLinkText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelLinkText.Location = new System.Drawing.Point(10, 57);
            this.labelLinkText.Name = "labelLinkText";
            this.labelLinkText.Size = new System.Drawing.Size(312, 15);
            this.labelLinkText.TabIndex = 4;
            this.labelLinkText.Text = "Te&xt:";
            //
            // textBoxLinkText
            //
            this.textBoxLinkText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLinkText.Location = new System.Drawing.Point(10, 73);
            this.textBoxLinkText.Name = "textBoxLinkText";
            this.textBoxLinkText.Size = new System.Drawing.Size(351, 21);
            this.textBoxLinkText.TabIndex = 5;
            this.textBoxLinkText.TextChanged += new System.EventHandler(this.textBoxLinkText_TextChanged);
            //
            // labelTitle
            //
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTitle.Location = new System.Drawing.Point(10, 207);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(341, 15);
            this.labelTitle.TabIndex = 8;
            this.labelTitle.Text = "&Title:";
            //
            // textBoxTitle
            //
            this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTitle.Location = new System.Drawing.Point(10, 223);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(351, 21);
            this.textBoxTitle.TabIndex = 9;
            this.textBoxTitle.TextChanged += new System.EventHandler(this.textBoxTitle_TextChanged);
            //
            // labelRel
            //
            this.labelRel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelRel.Location = new System.Drawing.Point(10, 252);
            this.labelRel.Name = "labelRel";
            this.labelRel.Size = new System.Drawing.Size(341, 15);
            this.labelRel.TabIndex = 10;
            this.labelRel.Text = "R&el:";
            //
            // comboBoxRel
            //
            this.comboBoxRel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRel.Location = new System.Drawing.Point(10, 268);
            this.comboBoxRel.Name = "comboBoxRel";
            this.comboBoxRel.Rel = "";
            this.comboBoxRel.Size = new System.Drawing.Size(351, 21);
            this.comboBoxRel.TabIndex = 11;
            //
            // ckboxNewWindow
            //
            this.ckboxNewWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ckboxNewWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ckboxNewWindow.Location = new System.Drawing.Point(10, 105);
            this.ckboxNewWindow.Name = "ckboxNewWindow";
            this.ckboxNewWindow.Size = new System.Drawing.Size(430, 22);
            this.ckboxNewWindow.TabIndex = 6;
            this.ckboxNewWindow.Text = "&Open link in new window";
            //
            // btnOptions
            //
            this.btnOptions.AccessibleName = "&Link to";
            this.btnOptions.AccessibleRole = System.Windows.Forms.AccessibleRole.ButtonMenu;
            this.btnOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnOptions.Image")));
            this.btnOptions.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOptions.Location = new System.Drawing.Point(273, 27);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(87, 23);
            this.btnOptions.TabIndex = 3;
            this.btnOptions.Text = "&Link to";
            this.btnOptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOptionsToolTip.SetToolTip(this.btnOptions, "Browse for link");
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            //
            // btnAdvanced
            //
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAdvanced.Location = new System.Drawing.Point(331, 159);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(114, 23);
            this.btnAdvanced.TabIndex = 15;
            this.btnAdvanced.Text = "&Advanced »";
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            //
            // btnRemove
            //
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnRemove.Location = new System.Drawing.Point(210, 159);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(114, 23);
            this.btnRemove.TabIndex = 14;
            this.btnRemove.Text = "&Remove Link";
            //
            // ckBoxGlossary
            //
            this.ckBoxGlossary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ckBoxGlossary.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ckBoxGlossary.Location = new System.Drawing.Point(10, 129);
            this.ckBoxGlossary.Name = "ckBoxGlossary";
            this.ckBoxGlossary.Size = new System.Drawing.Size(430, 22);
            this.ckBoxGlossary.TabIndex = 7;
            this.ckBoxGlossary.Text = " ";
            //
            // HyperlinkForm
            //
            this.AcceptButton = this.buttonInsert;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(452, 300);
            this.Controls.Add(this.ckBoxGlossary);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.bevel);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.ckboxNewWindow);
            this.Controls.Add(this.labelLinkText);
            this.Controls.Add(this.textBoxLinkText);
            this.Controls.Add(this.textBoxAddress);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelRel);
            this.Controls.Add(this.comboBoxRel);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonInsert);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HyperlinkForm";
            this.Text = "Insert Hyperlink";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void btnOptions_Click(object sender, System.EventArgs e)
        {
            CommandContextMenuDefinition menu = new CommandContextMenuDefinition();
            menu.Entries.Add(CommandId.RecentPost, false, false);
            menu.Entries.Add(CommandId.Glossary, false, false);

            Point screenPoint = PointToScreen(new Point(btnOptions.Left, btnOptions.Bottom));
            Point altScreenPoint = PointToScreen(new Point(btnOptions.Right, btnOptions.Bottom));
            LinkingCommand command = (LinkingCommand)CommandContextMenu.ShowModal(CommandManager, this, screenPoint, altScreenPoint.X, menu);

            if (command != null)
            {
                if (command.FindLink(textBoxLinkText.Text, this))
                {
                    textBoxAddress.Focus();
                }
            }
        }
        private void btnAdvanced_Click(object sender, System.EventArgs e)
        {
            _isMaxed = !_isMaxed;
            handleState();
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {
            ModifyCkBoxGlossary();
        }

        private void textBoxLinkText_TextChanged(object sender, EventArgs e)
        {
            ModifyCkBoxGlossary();
        }

        private void textBoxTitle_TextChanged(object sender, EventArgs e)
        {
            ModifyCkBoxGlossary();
        }

        private void ModifyCkBoxGlossary()
        {
            if (!_isNotTextLink && !_slimOptions)
            {
                ckBoxGlossary.Visible = true;
            }
        }
    }
}
