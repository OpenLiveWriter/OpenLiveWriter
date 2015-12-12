// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanel.
    /// </summary>
    public class WeblogConfigurationWizardPanel : System.Windows.Forms.UserControl, IRtlAware
    {
        // Panel ID's for instrumentation
        public enum ConfigPanelId : uint
        {
            None = 0,
            Welcome = 10,
            BlogType = 20,
            WPCreate = 29,
            SharePointBasicInfo = 32,
            OtherBasicInfo = 33,
            SelectProvider = 40,
            SharePointAuth = 41,
            GoogleBloggerAuth = 42,
            SelectBlog = 50,
            SelectImageEndPoint = 60,
            Confirm = 80,
            Success = 90
        }

        protected System.Windows.Forms.Panel panelMain;
        protected Label labelHeader;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WeblogConfigurationWizardPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelHeader.Font = Res.GetFont(FontSize.Heading, FontStyle.Regular);
            labelHeader.ForeColor = SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(0, 51, 153);
        }

        public string HeaderText
        {
            get { return labelHeader.Text; }
            set { labelHeader.Text = value; }
        }

        public virtual bool ShowProxySettingsLink
        {
            get { return false; }
        }

        /// <summary>
        /// For instrumentation purposes
        /// </summary>
        public virtual ConfigPanelId? PanelId
        {
            get { return null; }
        }

        public void PrepareForAdd()
        {
            MaximizeWidth(labelHeader);
            LayoutHelper.NaturalizeHeight(labelHeader);
            if (Padding.Top - labelHeader.Bottom < 10)
                Padding = new Padding(Padding.Left, labelHeader.Bottom + 10, Padding.Right, Padding.Bottom);
            NaturalizeLayout();
        }

        public virtual void NaturalizeLayout() { }

        public virtual bool ValidatePanel()
        {
            return true;
        }

        protected static void MaximizeWidth(Control c)
        {
            c.Width = c.Parent.ClientSize.Width - c.Left;
        }

        protected void ShowValidationError(Control control, MessageId displayMessageType, params object[] args)
        {
            DisplayMessage.Show(displayMessageType, control.FindForm(), args);
            control.Focus();
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(20, 49);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(392, 195);
            this.panelMain.TabIndex = 0;
            //
            // labelHeader
            //
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.Location = new System.Drawing.Point(16, 15);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(416, 21);
            this.labelHeader.TabIndex = 1;
            this.labelHeader.Text = "Header";
            //
            // WeblogConfigurationWizardPanel
            //
            this.Controls.Add(this.labelHeader);
            this.Controls.Add(this.panelMain);
            this.Name = "WeblogConfigurationWizardPanel";
            this.Padding = new System.Windows.Forms.Padding(0, 49, 20, 0);
            this.Size = new System.Drawing.Size(432, 264);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// Custom RTL layout is necessary to ensure that RTL fixup only happens once.
        /// Specifically, the "Select blog type" panel gets fixed up twice just because
        /// it's part of the wizard form on the form's first layout and it also gets
        /// reversed on the way into the form.
        ///
        /// Really we should not reverse wizard steps on the way in unless the form's
        /// reversal has already occurred, but this way we'll be robust to changes in
        /// our form reversal strategy.
        /// </summary>
        void IRtlAware.Layout()
        {
            if (!rtlApplied)
            {
                rtlApplied = true;
                Control[] childControls = new Control[Controls.Count];
                for (int i = 0; i < childControls.Length; i++)
                    childControls[i] = Controls[i];
                BidiHelper.RtlLayoutFixup(this, true, true, childControls);
            }
        }
        protected bool rtlApplied = false;
    }
}
