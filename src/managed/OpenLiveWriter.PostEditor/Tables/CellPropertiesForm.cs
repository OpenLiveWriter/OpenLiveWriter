// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public class CellPropertiesForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private OpenLiveWriter.PostEditor.Tables.CellPropertiesControl cellPropertiesControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public CellPropertiesForm(CellProperties cellProperties)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.cellPropertiesControl.Text = Res.Get(StringId.Cells);
            this.Text = Res.Get(StringId.CellPropertiesTitle);

            cellPropertiesControl.CellProperties = cellProperties;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (AutoGrow autoGrow = new AutoGrow(this, AnchorStyles.Right, false))
            {
                autoGrow.AllowAnchoring = true;

                cellPropertiesControl.AdjustSize();
                LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
            }
        }

        public CellProperties CellProperties
        {
            get
            {
                return cellPropertiesControl.CellProperties;
            }

        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (ValidateInput())
                DialogResult = DialogResult.OK;
        }

        private bool ValidateInput()
        {
            return true;

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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.cellPropertiesControl = new OpenLiveWriter.PostEditor.Tables.CellPropertiesControl();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(85, 112);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(166, 112);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            //
            // cellPropertiesControl
            //
            this.cellPropertiesControl.Location = new System.Drawing.Point(9, 8);
            this.cellPropertiesControl.Name = "cellPropertiesControl";
            this.cellPropertiesControl.Size = new System.Drawing.Size(231, 97);
            this.cellPropertiesControl.TabIndex = 4;
            //
            // CellPropertiesForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(250, 143);
            this.Controls.Add(this.cellPropertiesControl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CellPropertiesForm";
            this.Text = "Cell Properties";
            this.ResumeLayout(false);

        }
        #endregion

    }
}
