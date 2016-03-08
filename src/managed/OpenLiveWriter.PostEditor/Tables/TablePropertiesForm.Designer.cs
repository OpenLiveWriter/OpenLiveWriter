// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.


namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class TablePropertiesForm
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxSize;
        private System.Windows.Forms.Label labelRows;
        private System.Windows.Forms.Label labelColumns;
        private System.Windows.Forms.Panel panelRowsAndColumns;
        private System.Windows.Forms.Label label1;
        private OpenLiveWriter.PostEditor.Tables.ColumnWidthControl columnWidthControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.GroupBox groupBoxAppearance;
        private OpenLiveWriter.Controls.NumericTextBox numericTextBoxBorder;
        private OpenLiveWriter.Controls.NumericTextBox numericTextBoxColumns;
        private OpenLiveWriter.Controls.NumericTextBox numericTextBoxRows;
        private OpenLiveWriter.Controls.NumericTextBox numericTextBoxCellPadding;
        private OpenLiveWriter.Controls.NumericTextBox numericTextBoxCellSpacing;
        private System.Windows.Forms.CheckBox checkBoxShowBorder;
        private System.Windows.Forms.Label labelSpacingPixels;
        private System.Windows.Forms.Label labelPaddingPixels;
        private System.Windows.Forms.Label labelBorderPixels;
        private System.Windows.Forms.Label label3;

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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxSize = new System.Windows.Forms.GroupBox();
            this.columnWidthControl = new OpenLiveWriter.PostEditor.Tables.ColumnWidthControl();
            this.panelRowsAndColumns = new System.Windows.Forms.Panel();
            this.numericTextBoxColumns = new OpenLiveWriter.Controls.NumericTextBox();
            this.numericTextBoxRows = new OpenLiveWriter.Controls.NumericTextBox();
            this.labelRows = new System.Windows.Forms.Label();
            this.labelColumns = new System.Windows.Forms.Label();
            this.groupBoxAppearance = new System.Windows.Forms.GroupBox();
            this.labelSpacingPixels = new System.Windows.Forms.Label();
            this.labelPaddingPixels = new System.Windows.Forms.Label();
            this.labelBorderPixels = new System.Windows.Forms.Label();
            this.checkBoxShowBorder = new System.Windows.Forms.CheckBox();
            this.numericTextBoxCellSpacing = new OpenLiveWriter.Controls.NumericTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericTextBoxBorder = new OpenLiveWriter.Controls.NumericTextBox();
            this.numericTextBoxCellPadding = new OpenLiveWriter.Controls.NumericTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxSize.SuspendLayout();
            this.panelRowsAndColumns.SuspendLayout();
            this.groupBoxAppearance.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(139, 247);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(221, 247);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            //
            // groupBoxSize
            //
            this.groupBoxSize.Controls.Add(this.columnWidthControl);
            this.groupBoxSize.Controls.Add(this.panelRowsAndColumns);
            this.groupBoxSize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSize.Location = new System.Drawing.Point(9, 8);
            this.groupBoxSize.Name = "groupBoxSize";
            this.groupBoxSize.Size = new System.Drawing.Size(287, 97);
            this.groupBoxSize.TabIndex = 0;
            this.groupBoxSize.TabStop = false;
            this.groupBoxSize.Text = "Size";
            //
            // columnWidthControl
            //
            this.columnWidthControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.columnWidthControl.ColumnWidth = new PixelPercent();
            this.columnWidthControl.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.columnWidthControl.Location = new System.Drawing.Point(16, 55);
            this.columnWidthControl.Name = "columnWidthControl";
            this.columnWidthControl.Size = new System.Drawing.Size(254, 23);
            this.columnWidthControl.TabIndex = 1;
            //
            // panelRowsAndColumns
            //
            this.panelRowsAndColumns.Controls.Add(this.numericTextBoxColumns);
            this.panelRowsAndColumns.Controls.Add(this.numericTextBoxRows);
            this.panelRowsAndColumns.Controls.Add(this.labelRows);
            this.panelRowsAndColumns.Controls.Add(this.labelColumns);
            this.panelRowsAndColumns.Location = new System.Drawing.Point(16, 24);
            this.panelRowsAndColumns.Name = "panelRowsAndColumns";
            this.panelRowsAndColumns.Size = new System.Drawing.Size(227, 23);
            this.panelRowsAndColumns.TabIndex = 0;
            //
            // numericTextBoxColumns
            //
            this.numericTextBoxColumns.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numericTextBoxColumns.Location = new System.Drawing.Point(155, 0);
            this.numericTextBoxColumns.MaxLength = 9;
            this.numericTextBoxColumns.Name = "numericTextBoxColumns";
            this.numericTextBoxColumns.Size = new System.Drawing.Size(46, 21);
            this.numericTextBoxColumns.TabIndex = 3;
            //
            // numericTextBoxRows
            //
            this.numericTextBoxRows.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numericTextBoxRows.Location = new System.Drawing.Point(43, 0);
            this.numericTextBoxRows.MaxLength = 9;
            this.numericTextBoxRows.Name = "numericTextBoxRows";
            this.numericTextBoxRows.Size = new System.Drawing.Size(46, 21);
            this.numericTextBoxRows.TabIndex = 1;
            //
            // labelRows
            //
            this.labelRows.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelRows.Location = new System.Drawing.Point(0, 3);
            this.labelRows.Name = "labelRows";
            this.labelRows.Size = new System.Drawing.Size(37, 15);
            this.labelRows.TabIndex = 0;
            this.labelRows.Text = "&Rows:";
            //
            // labelColumns
            //
            this.labelColumns.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelColumns.Location = new System.Drawing.Point(104, 3);
            this.labelColumns.Name = "labelColumns";
            this.labelColumns.Size = new System.Drawing.Size(49, 15);
            this.labelColumns.TabIndex = 2;
            this.labelColumns.Text = "&Columns:";
            //
            // groupBoxAppearance
            //
            this.groupBoxAppearance.Controls.Add(this.labelSpacingPixels);
            this.groupBoxAppearance.Controls.Add(this.labelPaddingPixels);
            this.groupBoxAppearance.Controls.Add(this.labelBorderPixels);
            this.groupBoxAppearance.Controls.Add(this.checkBoxShowBorder);
            this.groupBoxAppearance.Controls.Add(this.numericTextBoxCellSpacing);
            this.groupBoxAppearance.Controls.Add(this.label3);
            this.groupBoxAppearance.Controls.Add(this.numericTextBoxBorder);
            this.groupBoxAppearance.Controls.Add(this.numericTextBoxCellPadding);
            this.groupBoxAppearance.Controls.Add(this.label1);
            this.groupBoxAppearance.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxAppearance.Location = new System.Drawing.Point(9, 108);
            this.groupBoxAppearance.Name = "groupBoxAppearance";
            this.groupBoxAppearance.Size = new System.Drawing.Size(287, 132);
            this.groupBoxAppearance.TabIndex = 1;
            this.groupBoxAppearance.TabStop = false;
            this.groupBoxAppearance.Text = "Appearance";
            //
            // labelSpacingPixels
            //
            this.labelSpacingPixels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSpacingPixels.Location = new System.Drawing.Point(206, 94);
            this.labelSpacingPixels.Name = "labelSpacingPixels";
            this.labelSpacingPixels.Size = new System.Drawing.Size(64, 15);
            this.labelSpacingPixels.TabIndex = 9;
            this.labelSpacingPixels.Text = "pixels";
            //
            // labelPaddingPixels
            //
            this.labelPaddingPixels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPaddingPixels.Location = new System.Drawing.Point(206, 65);
            this.labelPaddingPixels.Name = "labelPaddingPixels";
            this.labelPaddingPixels.Size = new System.Drawing.Size(64, 15);
            this.labelPaddingPixels.TabIndex = 8;
            this.labelPaddingPixels.Text = "pixels";
            //
            // labelBorderPixels
            //
            this.labelBorderPixels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelBorderPixels.Location = new System.Drawing.Point(206, 29);
            this.labelBorderPixels.Name = "labelBorderPixels";
            this.labelBorderPixels.Size = new System.Drawing.Size(64, 16);
            this.labelBorderPixels.TabIndex = 7;
            this.labelBorderPixels.Text = "pixels";
            //
            // checkBoxShowBorder
            //
            this.checkBoxShowBorder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxShowBorder.Location = new System.Drawing.Point(16, 24);
            this.checkBoxShowBorder.Name = "checkBoxShowBorder";
            this.checkBoxShowBorder.Size = new System.Drawing.Size(141, 24);
            this.checkBoxShowBorder.TabIndex = 0;
            this.checkBoxShowBorder.Text = "Show table &border:";
            this.checkBoxShowBorder.CheckedChanged += new System.EventHandler(this.checkBoxShowBorder_CheckedChanged);
            //
            // numericTextBoxCellSpacing
            //
            this.numericTextBoxCellSpacing.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numericTextBoxCellSpacing.Location = new System.Drawing.Point(166, 91);
            this.numericTextBoxCellSpacing.MaxLength = 9;
            this.numericTextBoxCellSpacing.Name = "numericTextBoxCellSpacing";
            this.numericTextBoxCellSpacing.Size = new System.Drawing.Size(35, 21);
            this.numericTextBoxCellSpacing.TabIndex = 6;
            //
            // label3
            //
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(16, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "&Space between cells:";
            //
            // numericTextBoxBorder
            //
            this.numericTextBoxBorder.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numericTextBoxBorder.Location = new System.Drawing.Point(166, 26);
            this.numericTextBoxBorder.MaxLength = 9;
            this.numericTextBoxBorder.Name = "numericTextBoxBorder";
            this.numericTextBoxBorder.Size = new System.Drawing.Size(35, 21);
            this.numericTextBoxBorder.TabIndex = 2;
            //
            // numericTextBoxCellPadding
            //
            this.numericTextBoxCellPadding.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.numericTextBoxCellPadding.Location = new System.Drawing.Point(166, 61);
            this.numericTextBoxCellPadding.MaxLength = 9;
            this.numericTextBoxCellPadding.Name = "numericTextBoxCellPadding";
            this.numericTextBoxCellPadding.Size = new System.Drawing.Size(35, 21);
            this.numericTextBoxCellPadding.TabIndex = 4;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(16, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "&Pad cell contents:";
            //
            // TablePropertiesForm
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(305, 277);
            this.Controls.Add(this.groupBoxAppearance);
            this.Controls.Add(this.groupBoxSize);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TablePropertiesForm";
            this.Text = "Insert Table";
            this.groupBoxSize.ResumeLayout(false);
            this.panelRowsAndColumns.ResumeLayout(false);
            this.panelRowsAndColumns.PerformLayout();
            this.groupBoxAppearance.ResumeLayout(false);
            this.groupBoxAppearance.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion


    }
}
