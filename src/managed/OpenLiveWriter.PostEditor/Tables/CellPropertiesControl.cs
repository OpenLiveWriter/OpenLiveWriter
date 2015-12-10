// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{
    /// <summary>
    /// Summary description for TableAppearanceControl.
    /// </summary>
    public class CellPropertiesControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.GroupBox groupBoxCells;
        private OpenLiveWriter.PostEditor.Tables.VerticalAlignmentControl verticalAlignmentControl;
        private OpenLiveWriter.PostEditor.Tables.HorizontalAlignmentControl horizontalAlignmentControl;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public CellPropertiesControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.groupBoxCells.Text = Res.Get(StringId.Cells);
        }

        public void AdjustSize()
        {
            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                using (new AutoGrow(groupBoxCells, AnchorStyles.Right, false))
                {
                    TableAlignmentControl.AdjustSizes(verticalAlignmentControl, horizontalAlignmentControl);
                    verticalAlignmentControl.NaturalizeHeight();
                    horizontalAlignmentControl.NaturalizeHeight();
                }
            }

        }

        public override string Text
        {
            get
            {
                return groupBoxCells.Text;
            }
            set
            {
                groupBoxCells.Text = value;
            }
        }

        public CellProperties CellProperties
        {
            get
            {
                CellProperties cellProperties = new CellProperties();
                cellProperties.BackgroundColor = _cellColor;
                cellProperties.HorizontalAlignment = horizontalAlignmentControl.HorizontalAlignment;
                cellProperties.VerticalAlignment = verticalAlignmentControl.VerticalAlignment;
                return cellProperties;
            }
            set
            {
                // null == default
                CellProperties cellProperties = value;
                if (cellProperties == null)
                    cellProperties = new CellProperties();

                // set values
                _cellColor = cellProperties.BackgroundColor;
                horizontalAlignmentControl.HorizontalAlignment = cellProperties.HorizontalAlignment;
                verticalAlignmentControl.VerticalAlignment = cellProperties.VerticalAlignment;
            }
        }
        // NOTE: right now we don't support editing of cell color so we just have a member
        // variable that tracks the value which was set -- we simply return this same
        // value when the caller does a get
        private CellColor _cellColor;

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
            this.groupBoxCells.Controls.Add(this.verticalAlignmentControl);
            this.groupBoxCells.Controls.Add(this.horizontalAlignmentControl);
            this.groupBoxCells.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxCells.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCells.Name = "groupBoxCells";
            this.groupBoxCells.Size = new System.Drawing.Size(304, 136);
            this.groupBoxCells.TabIndex = 2;
            this.groupBoxCells.TabStop = false;
            this.groupBoxCells.Text = "Cells";
            //
            // verticalAlignmentControl
            //
            this.verticalAlignmentControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.verticalAlignmentControl.Location = new System.Drawing.Point(15, 56);
            this.verticalAlignmentControl.Name = "verticalAlignmentControl";
            this.verticalAlignmentControl.Size = new System.Drawing.Size(283, 21);
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
            this.horizontalAlignmentControl.Size = new System.Drawing.Size(283, 21);
            this.horizontalAlignmentControl.TabIndex = 0;
            //
            // CellPropertiesControl
            //
            this.Controls.Add(this.groupBoxCells);
            this.Name = "CellPropertiesControl";
            this.Size = new System.Drawing.Size(304, 136);
            this.groupBoxCells.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
