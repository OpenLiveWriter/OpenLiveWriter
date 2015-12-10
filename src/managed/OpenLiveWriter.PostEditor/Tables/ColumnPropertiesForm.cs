// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public class ColumnPropertiesForm : ApplicationDialog
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

        public ColumnPropertiesForm(ColumnProperties columnProperties)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.groupBoxSize.Text = Res.Get(StringId.Size);
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.Text = Res.Get(StringId.ColumnProperties);

            columnWidthControl.ColumnWidth = columnProperties.Width;
            cellPropertiesControl.CellProperties = columnProperties.CellProperties;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (AutoGrow autoGrow = new AutoGrow(this, AnchorStyles.Right, false))
            {
                autoGrow.AllowAnchoring = true;
                cellPropertiesControl.AdjustSize();
                groupBoxSize.Width = cellPropertiesControl.Width;

                LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
            }
        }


        public ColumnProperties ColumnProperties
        {
            get
            {
                ColumnProperties columnProperties = new ColumnProperties();
                columnProperties.Width = columnWidthControl.ColumnWidth;
                columnProperties.CellProperties = cellPropertiesControl.CellProperties;
                return columnProperties;
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (columnWidthControl.ValidateInput(1000))
            {
                DialogResult = DialogResult.OK;
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
            this.buttonCancel.Location = new System.Drawing.Point(169, 184);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            // 
            // groupBoxSize
            // 
            this.groupBoxSize.Controls.Add(this.columnWidthControl);
            this.groupBoxSize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSize.Location = new System.Drawing.Point(9, 8);
            this.groupBoxSize.Name = "groupBoxSize";
            this.groupBoxSize.Size = new System.Drawing.Size(234, 66);
            this.groupBoxSize.TabIndex = 0;
            this.groupBoxSize.TabStop = false;
            this.groupBoxSize.Text = "Size";
            // 
            // columnWidthControl
            // 
            this.columnWidthControl.ColumnWidth = 0;
            this.columnWidthControl.Location = new System.Drawing.Point(15, 24);
            this.columnWidthControl.Name = "columnWidthControl";
            this.columnWidthControl.Size = new System.Drawing.Size(209, 23);
            this.columnWidthControl.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(1, 1);
            this.label11.Name = "label11";
            this.label11.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(86, 184);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // cellPropertiesControl
            // 
            this.cellPropertiesControl.Location = new System.Drawing.Point(9, 79);
            this.cellPropertiesControl.Name = "cellPropertiesControl";
            this.cellPropertiesControl.Size = new System.Drawing.Size(234, 98);
            this.cellPropertiesControl.TabIndex = 1;
            // 
            // ColumnPropertiesForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(253, 215);
            this.Controls.Add(this.cellPropertiesControl);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxSize);
            this.Controls.Add(this.buttonCancel);
            this.Location = new System.Drawing.Point(0, 0);
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
