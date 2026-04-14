// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// OpenForm for showing the detail of the selected DiagnosticsConsole entries.
    /// </summary>
    public class OpenForm : BaseForm
    {
        #region Designer Generated Code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private Button buttonClose;
        private RichTextBox richTextBox;

        #endregion Designer Generated Code

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the OpenForm class.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public OpenForm()
        {
            RightToLeft = RightToLeft.No;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Icon = ApplicationEnvironment.ProductIcon;
        }

        /// <summary>
        /// Initializes a new instance of the OpenForm class.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public OpenForm(string text)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Icon = ApplicationEnvironment.ProductIcon;

            //	Set the text.
            richTextBox.Text = text;
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

        #endregion Class Initialization & Termination

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(OpenForm));
            this.buttonClose = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            //
            // buttonClose
            //
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(708, 544);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            //
            // richTextBox
            //
            this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox.Location = new System.Drawing.Point(10, 10);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.RightToLeft = RightToLeft.No;
            this.richTextBox.Size = new System.Drawing.Size(772, 524);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            this.richTextBox.WordWrap = false;
            this.richTextBox.TextChanged += new System.EventHandler(this.richTextBox_TextChanged);
            //
            // OpenForm
            //
            this.AcceptButton = this.buttonClose;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(792, 574);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.buttonClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OpenForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Diagnostics";
            this.ResumeLayout(false);

        }
        #endregion

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
