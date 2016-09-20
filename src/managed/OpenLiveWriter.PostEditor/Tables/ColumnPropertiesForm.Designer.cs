// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class ColumnPropertiesForm
    {
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxSize;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button buttonOK;
        private OpenLiveWriter.PostEditor.Tables.ColumnWidthControl columnWidthControl;
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxSize = new System.Windows.Forms.GroupBox();
            this.columnWidthControl = new OpenLiveWriter.PostEditor.Tables.ColumnWidthControl();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.cellPropertiesControl = new OpenLiveWriter.PostEditor.Tables.CellPropertiesControl();
            this.groupBoxSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(379, 201);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(105, 33);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            // 
            // groupBoxSize
            // 
            this.groupBoxSize.Controls.Add(this.columnWidthControl);
            this.groupBoxSize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSize.Location = new System.Drawing.Point(13, 11);
            this.groupBoxSize.Name = "groupBoxSize";
            this.groupBoxSize.Size = new System.Drawing.Size(341, 79);
            this.groupBoxSize.TabIndex = 0;
            this.groupBoxSize.TabStop = false;
            this.groupBoxSize.Text = "Size";
            // 
            // columnWidthControl
            // 
            this.columnWidthControl.Location = new System.Drawing.Point(21, 25);
            this.columnWidthControl.Name = "columnWidthControl";
            this.columnWidthControl.Size = new System.Drawing.Size(308, 36);
            this.columnWidthControl.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(1, 1);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 23);
            this.label11.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(262, 201);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(105, 33);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // cellPropertiesControl
            // 
            cellProperties1.BackgroundColor = null;
            cellProperties1.HorizontalAlignment = OpenLiveWriter.PostEditor.Tables.HorizontalAlignment.Left;
            cellProperties1.VerticalAlignment = OpenLiveWriter.PostEditor.Tables.VerticalAlignment.Middle;
            this.cellPropertiesControl.CellProperties = cellProperties1;
            this.cellPropertiesControl.Location = new System.Drawing.Point(13, 113);
            this.cellPropertiesControl.Name = "cellPropertiesControl";
            this.cellPropertiesControl.Size = new System.Drawing.Size(243, 120);
            this.cellPropertiesControl.TabIndex = 1;
            // 
            // ColumnPropertiesForm
            // 
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(496, 245);
            this.Controls.Add(this.cellPropertiesControl);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxSize);
            this.Controls.Add(this.buttonCancel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColumnPropertiesForm";
            this.Text = "Column Properties";
            this.groupBoxSize.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
