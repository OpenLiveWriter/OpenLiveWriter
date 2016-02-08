// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    public class ApplicationStyleWintergreen : ApplicationStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ApplicationStyleWintergreen()
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
            // ApplicationStyleWintergreen
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
            this.DisplayName = "Wintergreen";
            this.InactiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(204)), ((System.Byte)(224)), ((System.Byte)(221)));
            this.InactiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(198)), ((System.Byte)(215)), ((System.Byte)(212)));
            this.InactiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.InactiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(189)), ((System.Byte)(189)), ((System.Byte)(189)));
            this.InactiveTabTextColor = System.Drawing.Color.Black;
            this.InactiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(202)), ((System.Byte)(217)), ((System.Byte)(215)));
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
            this.ToolWindowBackgroundColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowBorderColor = System.Drawing.Color.FromArgb(((System.Byte)(72)), ((System.Byte)(100)), ((System.Byte)(165)));
            this.ToolWindowTitleBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowTitleBarFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.ToolWindowTitleBarTextColor = System.Drawing.Color.White;
            this.ToolWindowTitleBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(126)), ((System.Byte)(166)), ((System.Byte)(237)));
            this.WindowColor = System.Drawing.Color.White;
            this.WorkspacePaneControlColor = System.Drawing.Color.FromArgb(((System.Byte)(204)), ((System.Byte)(224)), ((System.Byte)(221)));

        }
        #endregion

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public override Image PreviewImage
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.Wintergreen.png");
            }
        }
    }
}
