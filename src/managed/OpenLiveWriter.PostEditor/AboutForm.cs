// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using OpenLiveWriter.Localization;
using System.Text;

namespace OpenLiveWriter.PostEditor
{

    public class AboutForm : ApplicationDialog
    {
        /// <summary>
        /// Form fields.
        /// </summary>
        private Label labelCopyright;
        private Button buttonOK;
        private PictureBox pictureBoxLogo;
        private Label labelProduct;
        private Label labelVersion;
        private System.Windows.Forms.LinkLabel lnkShowLogFile;
        private Label labelConfigVersion;
        private TextBox copyrightTextbox;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        // Copyright notices are not to be localized.
        string[] credits = {
            "DeltaCompressionDotNet (MS-PL) Copyright © Todd Aspeotis 2013 \nhttps://github.com/taspeotis/DeltaCompressionDotNet",
            "Mono.Cecil (MIT) Copyright © 2008 - 2015 Jb Evain Copyright © 2008 - 2011 Novell, Inc \nhttps://github.com/jbevain/cecil",
            "Splat (MIT) Copyright © 2013 Paul Betts \nhttps://github.com/paulcbetts/splat/",
            "Squirrel.Windows (MIT) Copyright © 2012 GitHub, Inc. \n https://github.com/squirrel/squirrel.windows",
            /* XmpMetadata.cs */ "Portions Copyright © 2011 Omar Shahine, licensed under Creative Commons Attribution 3.0 Unported License.",
            /* Brian Lambert */ "Portions Copyright © 2003 Brian Lambert, used with permission of the author under the MIT License.",
        };

        public AboutForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            labelProduct.Font = Res.GetFont(FontSize.XXLarge, FontStyle.Regular);
            buttonOK.Text = Res.Get(StringId.OKButtonText);
            labelVersion.Text = Res.Get(StringId.AboutVersion);

            labelCopyright.Text = Res.Get(StringId.AboutCopyright);

            lnkShowLogFile.Text = Res.Get(StringId.ShowLogFile);
            lnkShowLogFile.Refresh();

            //	Piece of crap designer.
            Bitmap aboutBoxImage = ResourceHelper.LoadAssemblyResourceBitmap("Images.AboutBoxImageSmall.png");
            pictureBoxLogo = new PictureBox();
            pictureBoxLogo.Bounds = new Rectangle(7, 16, aboutBoxImage.Width, aboutBoxImage.Height);
            pictureBoxLogo.Image = aboutBoxImage;
            pictureBoxLogo.RightToLeft = RightToLeft.No;
            Controls.Add(pictureBoxLogo);

            //	Set the dialog text.
            Text = TextHelper.StripHotkey(String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.AboutAbout), ApplicationEnvironment.ProductNameQualified));
#if DEBUG
            string displayVersion = ApplicationEnvironment.ProductDisplayVersion + " " + CultureInfo.CurrentUICulture;
#else
            string displayVersion = ApplicationEnvironment.ProductDisplayVersion;
#endif

            labelProduct.Text = ApplicationEnvironment.ProductNameVersioned;
            labelVersion.Text = String.Format(CultureInfo.CurrentCulture, labelVersion.Text, displayVersion);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labelConfigVersion.Text = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.AboutConfigurationVersion), version);
            labelConfigVersion.Visible = false;

            copyrightTextbox.Font = Res.GetFont(FontSize.Small, FontStyle.Regular);
            StringBuilder strCredits = new StringBuilder();

            foreach (string str in credits)
            {
                strCredits.AppendFormat("{0}\r\n\r\n", str);
            }
            copyrightTextbox.Text = strCredits.ToString().TrimEnd();
            copyrightTextbox.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.CopyrightInformation));

            //	Drive focus down to the OK button.
            buttonOK.Focus();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom | AnchorStyles.Right, true))
            {
                pictureBoxLogo.Left = labelCopyright.Left;
                labelProduct.Left = pictureBoxLogo.Right + 10;
                labelVersion.Left = pictureBoxLogo.Right + 10;
                LayoutHelper.NaturalizeHeight(labelProduct);
                labelProduct.Top = pictureBoxLogo.Top + (pictureBoxLogo.Height / 2) - labelProduct.Height;
                LayoutHelper.DistributeVertically(15, false, labelProduct, labelVersion);
                LayoutHelper.NaturalizeHeightAndDistribute(8, labelVersion, labelCopyright);

                LayoutHelper.FitControlsBelow(10, labelCopyright);

                DisplayHelper.AutoFitSystemButton(buttonOK, buttonOK.Width, int.MaxValue);
            }
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelProduct = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.lnkShowLogFile = new System.Windows.Forms.LinkLabel();
            this.labelConfigVersion = new System.Windows.Forms.Label();
            this.copyrightTextbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // labelProduct
            //
            this.labelProduct.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelProduct.Font = new System.Drawing.Font("Tahoma", 12F);
            this.labelProduct.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelProduct.Location = new System.Drawing.Point(22, 16);
            this.labelProduct.Name = "labelProduct";
            this.labelProduct.Size = new System.Drawing.Size(287, 24);
            this.labelProduct.TabIndex = 0;
            this.labelProduct.Text = "Open Live Writer";
            //
            // labelCopyright
            //
            this.labelCopyright.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCopyright.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCopyright.Location = new System.Drawing.Point(22, 100);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(284, 30);
            this.labelCopyright.TabIndex = 1;
            this.labelCopyright.Text = "© 2015 .NET Foundation. All rights reserved.";
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(344, 280);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // labelVersion
            //
            this.labelVersion.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelVersion.Location = new System.Drawing.Point(22, 55);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(287, 20);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Build xxx.xxxx.xxxx";
            //
            // lnkShowLogFile
            //
            this.lnkShowLogFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkShowLogFile.AutoSize = true;
            this.lnkShowLogFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lnkShowLogFile.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkShowLogFile.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.lnkShowLogFile.Location = new System.Drawing.Point(20, 282);
            this.lnkShowLogFile.Name = "lnkShowLogFile";
            this.lnkShowLogFile.Size = new System.Drawing.Size(75, 15);
            this.lnkShowLogFile.TabIndex = 8;
            this.lnkShowLogFile.TabStop = true;
            this.lnkShowLogFile.Text = "Show log file";
            this.lnkShowLogFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkShowLogFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkShowLogFile_LinkClicked);
            //
            // labelConfigVersion
            //
            this.labelConfigVersion.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelConfigVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelConfigVersion.Location = new System.Drawing.Point(22, 75);
            this.labelConfigVersion.Name = "labelConfigVersion";
            this.labelConfigVersion.Size = new System.Drawing.Size(284, 18);
            this.labelConfigVersion.TabIndex = 9;
            this.labelConfigVersion.Text = "Configuration Version 12.0.2342.0324";
            //
            // copyrightTextbox
            //
            this.copyrightTextbox.AcceptsReturn = true;
            this.copyrightTextbox.Location = new System.Drawing.Point(12, 123);
            this.copyrightTextbox.Multiline = true;
            this.copyrightTextbox.Name = "copyrightTextbox";
            this.copyrightTextbox.ReadOnly = true;
            this.copyrightTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.copyrightTextbox.Size = new System.Drawing.Size(410, 151);
            this.copyrightTextbox.TabIndex = 12;
            //
            // AboutForm
            //
            this.AcceptButton = this.buttonOK;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.buttonOK;
            this.ClientSize = new System.Drawing.Size(434, 312);
            this.Controls.Add(this.copyrightTextbox);
            this.Controls.Add(this.labelConfigVersion);
            this.Controls.Add(this.lnkShowLogFile);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.labelProduct);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.Text = "About {0}";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// Shows the AboutForm.
        /// </summary>
        /// <returns>The DialogResult.</returns>
        public static DialogResult DisplayDialog()
        {
            using (new WaitCursor())
            {
                using (AboutForm aboutForm = new AboutForm())
                    return aboutForm.ShowDialog(Win32WindowImpl.ForegroundWin32Window);
            }
        }

        /// <summary>
        /// buttonOK_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void lnkShowLogFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", string.Format(CultureInfo.InvariantCulture, "/select,\"{0}\"", ApplicationEnvironment.LogFilePath));
        }
    }
}
