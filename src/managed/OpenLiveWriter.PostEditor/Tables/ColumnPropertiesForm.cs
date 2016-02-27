// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class ColumnPropertiesForm : ApplicationDialog
    {
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
    }
}
