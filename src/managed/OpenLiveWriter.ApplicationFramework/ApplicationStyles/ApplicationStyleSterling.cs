// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    public class ApplicationStyleSterling : ApplicationStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ApplicationStyleSterling()
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
            // ApplicationStyleSterling
            //
            this.ActiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(146)), ((System.Byte)(155)), ((System.Byte)(174)));
            this.ActiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(219)), ((System.Byte)(224)), ((System.Byte)(229)));
            this.ActiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.ActiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(181)), ((System.Byte)(193)), ((System.Byte)(196)));
            this.ActiveTabTextColor = System.Drawing.Color.Black;
            this.ActiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(230)), ((System.Byte)(234)), ((System.Byte)(238)));
            this.AlertControlColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(255)), ((System.Byte)(194)));
            this.BoldApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.BorderColor = System.Drawing.Color.FromArgb(((System.Byte)(157)), ((System.Byte)(161)), ((System.Byte)(167)));
            this.DisplayName = "Sterling";
            this.InactiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(208)), ((System.Byte)(212)), ((System.Byte)(220)));
            this.InactiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(194)), ((System.Byte)(202)));
            this.InactiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.InactiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(189)), ((System.Byte)(189)), ((System.Byte)(189)));
            this.InactiveTabTextColor = System.Drawing.Color.Black;
            this.InactiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(205)), ((System.Byte)(206)), ((System.Byte)(215)));
            this.ItalicApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Italic);
            this.LinkApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
            this.MenuBitmapAreaColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(198)), ((System.Byte)(203)));
            this.MenuSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(128)), ((System.Byte)(157)), ((System.Byte)(161)), ((System.Byte)(167)));
            this.NormalApplicationFont = new System.Drawing.Font("Tahoma", 8.25F);
            this.PrimaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(198)), ((System.Byte)(203)));
            this.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor = System.Drawing.Color.FromArgb(((System.Byte)(157)), ((System.Byte)(161)), ((System.Byte)(167)));
            this.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(215)), ((System.Byte)(219)));
            this.PrimaryWorkspaceCommandBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(191)), ((System.Byte)(199)), ((System.Byte)(206)));
            this.PrimaryWorkspaceCommandBarBottomLayoutMargin = 3;
            this.PrimaryWorkspaceCommandBarDisabledTextColor = System.Drawing.Color.Gray;
            this.PrimaryWorkspaceCommandBarLeftLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarRightLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarSeparatorLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarTextColor = System.Drawing.Color.Black;
            this.PrimaryWorkspaceCommandBarTopBevelFirstLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopBevelSecondLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(223)), ((System.Byte)(228)), ((System.Byte)(234)));
            this.PrimaryWorkspaceCommandBarTopLayoutMargin = 2;
            this.PrimaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(193)), ((System.Byte)(198)), ((System.Byte)(203)));
            this.SecondaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(217)), ((System.Byte)(222)), ((System.Byte)(227)));
            this.SecondaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(217)), ((System.Byte)(222)), ((System.Byte)(227)));
            this.SmallApplicationFont = new System.Drawing.Font("Tahoma", 6.75F);
            this.ToolWindowBackgroundColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowBorderColor = System.Drawing.Color.FromArgb(((System.Byte)(72)), ((System.Byte)(100)), ((System.Byte)(165)));
            this.ToolWindowTitleBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowTitleBarFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.ToolWindowTitleBarTextColor = System.Drawing.Color.White;
            this.ToolWindowTitleBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(126)), ((System.Byte)(166)), ((System.Byte)(237)));
            this.WindowColor = System.Drawing.Color.White;
            this.WorkspacePaneControlColor = System.Drawing.Color.FromArgb(((System.Byte)(237)), ((System.Byte)(240)), ((System.Byte)(243)));

        }
        #endregion

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public override Image PreviewImage
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.Sterling.png");
            }
        }
    }
}
