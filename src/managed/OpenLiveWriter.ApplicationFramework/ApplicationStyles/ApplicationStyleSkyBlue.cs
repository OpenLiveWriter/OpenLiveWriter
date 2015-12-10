// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    public class ApplicationStyleSkyBlue : ApplicationStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ApplicationStyleSkyBlue()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ApplicationStyleSkyBlue
            //
            this.ActiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(107)), ((System.Byte)(140)), ((System.Byte)(210)));
            this.ActiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(183)), ((System.Byte)(203)), ((System.Byte)(245)));
            this.ActiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.ActiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(181)), ((System.Byte)(193)), ((System.Byte)(196)));
            this.ActiveTabTextColor = System.Drawing.Color.Black;
            this.ActiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(207)), ((System.Byte)(227)), ((System.Byte)(253)));
            this.AlertControlColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(255)), ((System.Byte)(194)));
            this.BorderColor = System.Drawing.Color.FromArgb(((System.Byte)(117)), ((System.Byte)(135)), ((System.Byte)(179)));
            this.DisplayName = "Sky Blue";
            this.InactiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(236)), ((System.Byte)(233)), ((System.Byte)(216)));
            this.InactiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(228)), ((System.Byte)(228)), ((System.Byte)(228)));
            this.InactiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.InactiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(189)), ((System.Byte)(189)), ((System.Byte)(189)));
            this.InactiveTabTextColor = System.Drawing.Color.Black;
            this.InactiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(232)), ((System.Byte)(232)), ((System.Byte)(232)));
            this.MenuBitmapAreaColor = System.Drawing.Color.FromArgb(((System.Byte)(195)), ((System.Byte)(215)), ((System.Byte)(249)));
            this.MenuSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(128)), ((System.Byte)(117)), ((System.Byte)(135)), ((System.Byte)(179)));
            this.PrimaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(161)), ((System.Byte)(180)), ((System.Byte)(215)));
            this.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor = System.Drawing.Color.FromArgb(((System.Byte)(117)), ((System.Byte)(135)), ((System.Byte)(179)));
            this.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor = System.Drawing.Color.FromArgb(((System.Byte)(166)), ((System.Byte)(187)), ((System.Byte)(223)));
            this.PrimaryWorkspaceCommandBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(148)), ((System.Byte)(173)), ((System.Byte)(222)));
            this.PrimaryWorkspaceCommandBarBottomLayoutMargin = 3;
            this.PrimaryWorkspaceCommandBarDisabledTextColor = System.Drawing.Color.Gray;
            this.PrimaryWorkspaceCommandBarLeftLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarRightLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarSeparatorLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarTextColor = System.Drawing.Color.Black;
            this.PrimaryWorkspaceCommandBarTopBevelFirstLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopBevelSecondLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(213)), ((System.Byte)(249)));
            this.PrimaryWorkspaceCommandBarTopLayoutMargin = 2;
            this.PrimaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(154)), ((System.Byte)(174)), ((System.Byte)(213)));
            this.SecondaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(183)), ((System.Byte)(203)), ((System.Byte)(245)));
            this.SecondaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(183)), ((System.Byte)(203)), ((System.Byte)(245)));
            this.ToolWindowBackgroundColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowBorderColor = System.Drawing.Color.FromArgb(((System.Byte)(72)), ((System.Byte)(100)), ((System.Byte)(165)));
            this.ToolWindowTitleBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowTitleBarTextColor = System.Drawing.Color.White;
            this.ToolWindowTitleBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(126)), ((System.Byte)(166)), ((System.Byte)(237)));
            this.WindowColor = System.Drawing.Color.White;
            this.WorkspacePaneControlColor = System.Drawing.Color.FromArgb(((System.Byte)(214)), ((System.Byte)(223)), ((System.Byte)(247)));

        }
        #endregion

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public override Image PreviewImage
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.SkyBlue.png");
            }
        }
    }
}
