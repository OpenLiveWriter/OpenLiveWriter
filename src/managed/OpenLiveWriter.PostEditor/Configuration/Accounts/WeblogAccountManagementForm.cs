// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.PostEditor.Configuration.Accounts
{
    /// <summary>
    /// Summary description for WeblogAccountManagementForm.
    /// </summary>
    public class WeblogAccountManagementForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonClose;
        private OpenLiveWriter.PostEditor.Configuration.Accounts.WeblogAccountManagementControl weblogAccountManagementControl1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WeblogAccountManagementForm(IBlogPostEditingSite editingSite)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            weblogAccountManagementControl1.EditingSite = editingSite ;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonClose = new System.Windows.Forms.Button();
            this.weblogAccountManagementControl1 = new OpenLiveWriter.PostEditor.Configuration.Accounts.WeblogAccountManagementControl();
            this.SuspendLayout();
            //
            // buttonClose
            //
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClose.Location = new System.Drawing.Point(279, 264);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            //
            // weblogAccountManagementControl1
            //
            this.weblogAccountManagementControl1.BlogSettingsEditors = null;
            this.weblogAccountManagementControl1.EditingSite = null;
            this.weblogAccountManagementControl1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.weblogAccountManagementControl1.Location = new System.Drawing.Point(9, 10);
            this.weblogAccountManagementControl1.Name = "weblogAccountManagementControl1";
            this.weblogAccountManagementControl1.Size = new System.Drawing.Size(345, 245);
            this.weblogAccountManagementControl1.TabIndex = 0;
            //
            // WeblogAccountManagementForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(362, 294);
            this.ControlBox = false;
            this.Controls.Add(this.weblogAccountManagementControl1);
            this.Controls.Add(this.buttonClose);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WeblogAccountManagementForm";
            this.Text = "Accounts";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
