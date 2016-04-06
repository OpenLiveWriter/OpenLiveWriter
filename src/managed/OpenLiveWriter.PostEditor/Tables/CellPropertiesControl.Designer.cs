// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Tables
{
    /// <summary>
    /// Summary description for TableAppearanceControl.
    /// </summary>
    public partial class CellPropertiesControl
    {
        private System.Windows.Forms.GroupBox groupBoxCells;
        private OpenLiveWriter.PostEditor.Tables.VerticalAlignmentControl verticalAlignmentControl;
        private OpenLiveWriter.PostEditor.Tables.HorizontalAlignmentControl horizontalAlignmentControl;
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxCells = new System.Windows.Forms.GroupBox();
            this.verticalAlignmentControl = new OpenLiveWriter.PostEditor.Tables.VerticalAlignmentControl();
            this.horizontalAlignmentControl = new OpenLiveWriter.PostEditor.Tables.HorizontalAlignmentControl();
            this.groupBoxCells.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCells
            // 
            this.groupBoxCells.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCells.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxCells.Controls.Add(this.verticalAlignmentControl);
            this.groupBoxCells.Controls.Add(this.horizontalAlignmentControl);
            this.groupBoxCells.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxCells.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCells.Name = "groupBoxCells";
            this.groupBoxCells.Size = new System.Drawing.Size(233, 108);
            this.groupBoxCells.TabIndex = 2;
            this.groupBoxCells.TabStop = false;
            this.groupBoxCells.Text = "Cells";
            // 
            // verticalAlignmentControl
            // 
            this.verticalAlignmentControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.verticalAlignmentControl.Location = new System.Drawing.Point(15, 60);
            this.verticalAlignmentControl.Name = "verticalAlignmentControl";
            this.verticalAlignmentControl.Size = new System.Drawing.Size(212, 28);
            this.verticalAlignmentControl.TabIndex = 1;
            this.verticalAlignmentControl.VerticalAlignment = OpenLiveWriter.PostEditor.Tables.VerticalAlignment.Middle;
            // 
            // horizontalAlignmentControl
            // 
            this.horizontalAlignmentControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.horizontalAlignmentControl.HorizontalAlignment = OpenLiveWriter.PostEditor.Tables.HorizontalAlignment.Left;
            this.horizontalAlignmentControl.Location = new System.Drawing.Point(15, 25);
            this.horizontalAlignmentControl.Name = "horizontalAlignmentControl";
            this.horizontalAlignmentControl.Size = new System.Drawing.Size(212, 31);
            this.horizontalAlignmentControl.TabIndex = 0;
            // 
            // CellPropertiesControl
            // 
            this.Controls.Add(this.groupBoxCells);
            this.Name = "CellPropertiesControl";
            this.Size = new System.Drawing.Size(239, 108);
            this.groupBoxCells.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
