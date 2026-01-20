// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    public class ApplicationStyleGreen : ApplicationStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ApplicationStyleGreen()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ApplicationStyleGreen
            //
            this.ActiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(127)), ((System.Byte)(168)), ((System.Byte)(163)));
            this.ActiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(234)), ((System.Byte)(228)));
            this.ActiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.ActiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(181)), ((System.Byte)(193)), ((System.Byte)(196)));
            this.ActiveTabTextColor = System.Drawing.Color.Black;
            this.ActiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(201)), ((System.Byte)(238)), ((System.Byte)(231)));
            this.AlertControlColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(255)), ((System.Byte)(194)));
            this.BoldApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.BorderColor = System.Drawing.Color.FromArgb(((System.Byte)(127)), ((System.Byte)(168)), ((System.Byte)(163)));
            this.InactiveSelectionColor = System.Drawing.SystemColors.Control;
            this.InactiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(228)), ((System.Byte)(228)), ((System.Byte)(228)));
            this.InactiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.InactiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(189)), ((System.Byte)(189)), ((System.Byte)(189)));
            this.InactiveTabTextColor = System.Drawing.Color.Black;
            this.InactiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(232)), ((System.Byte)(232)), ((System.Byte)(232)));
            this.ItalicApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Italic);
            this.LinkApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
            this.MenuBitmapAreaColor = System.Drawing.Color.FromArgb(((System.Byte)(197)), ((System.Byte)(236)), ((System.Byte)(230)));
            this.MenuSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(128)), ((System.Byte)(127)), ((System.Byte)(168)), ((System.Byte)(163)));
            this.NormalApplicationFont = new System.Drawing.Font("Tahoma", 8.25F);
            this.PrimaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(164)), ((System.Byte)(203)), ((System.Byte)(197)));
            this.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor = System.Drawing.Color.FromArgb(((System.Byte)(127)), ((System.Byte)(168)), ((System.Byte)(163)));
            this.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor = System.Drawing.Color.FromArgb(((System.Byte)(175)), ((System.Byte)(213)), ((System.Byte)(205)));
            this.PrimaryWorkspaceCommandBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(164)), ((System.Byte)(212)), ((System.Byte)(204)));
            this.PrimaryWorkspaceCommandBarBottomLayoutMargin = 3;
            this.PrimaryWorkspaceCommandBarDisabledTextColor = System.Drawing.Color.Gray;
            this.PrimaryWorkspaceCommandBarLeftLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarRightLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarSeparatorLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarTextColor = System.Drawing.Color.Black;
            this.PrimaryWorkspaceCommandBarTopBevelFirstLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopBevelSecondLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(201)), ((System.Byte)(238)), ((System.Byte)(231)));
            this.PrimaryWorkspaceCommandBarTopLayoutMargin = 2;
            this.PrimaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(164)), ((System.Byte)(203)), ((System.Byte)(197)));
            this.SecondaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(234)), ((System.Byte)(228)));
            this.SecondaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(234)), ((System.Byte)(228)));
            this.SmallApplicationFont = new System.Drawing.Font("Tahoma", 6.75F);
            this.WorkspacePaneControlColor = System.Drawing.Color.FromArgb(((System.Byte)(204)), ((System.Byte)(224)), ((System.Byte)(221)));

        }
        #endregion
    }
}
