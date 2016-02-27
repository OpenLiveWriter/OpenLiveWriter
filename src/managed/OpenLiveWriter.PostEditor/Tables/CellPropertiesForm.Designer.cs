// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class CellPropertiesForm
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private OpenLiveWriter.PostEditor.Tables.CellPropertiesControl cellPropertiesControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

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
            OpenLiveWriter.PostEditor.Tables.CellProperties cellProperties1 = new OpenLiveWriter.PostEditor.Tables.CellProperties();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.cellPropertiesControl = new OpenLiveWriter.PostEditor.Tables.CellPropertiesControl();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(114, 158);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(105, 33);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(227, 158);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(105, 33);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            // 
            // cellPropertiesControl
            // 
            cellProperties1.BackgroundColor = null;
            cellProperties1.HorizontalAlignment = OpenLiveWriter.PostEditor.Tables.HorizontalAlignment.Left;
            cellProperties1.VerticalAlignment = OpenLiveWriter.PostEditor.Tables.VerticalAlignment.Middle;
            this.cellPropertiesControl.CellProperties = cellProperties1;
            this.cellPropertiesControl.Location = new System.Drawing.Point(13, 11);
            this.cellPropertiesControl.Name = "cellPropertiesControl";
            this.cellPropertiesControl.Size = new System.Drawing.Size(323, 139);
            this.cellPropertiesControl.TabIndex = 4;
            // 
            // CellPropertiesForm
            // 
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(345, 202);
            this.Controls.Add(this.cellPropertiesControl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CellPropertiesForm";
            this.Text = "Cell Properties";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
