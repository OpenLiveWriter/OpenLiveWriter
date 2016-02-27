// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class CellPropertiesForm : ApplicationDialog
    {
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
    }
}
