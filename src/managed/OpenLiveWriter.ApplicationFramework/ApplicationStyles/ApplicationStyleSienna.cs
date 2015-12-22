// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    public class ApplicationStyleSienna : ApplicationStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new insance of the ApplicationStyleSienna class.
        /// </summary>
        public ApplicationStyleSienna()
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
            // ApplicationStyleSienna
            //
            this.ActiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(161)), ((System.Byte)(156)), ((System.Byte)(112)));
            this.ActiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(226)), ((System.Byte)(221)), ((System.Byte)(176)));
            this.ActiveTabHighlightColor = System.Drawing.Color.FromArgb(((System.Byte)(243)), ((System.Byte)(243)), ((System.Byte)(243)));
            this.ActiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(214)), ((System.Byte)(208)), ((System.Byte)(181)));
            this.ActiveTabTextColor = System.Drawing.Color.Black;
            this.ActiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(250)), ((System.Byte)(245)), ((System.Byte)(202)));
            this.AlertControlColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(255)), ((System.Byte)(194)));
            this.BoldApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.BorderColor = System.Drawing.Color.FromArgb(((System.Byte)(161)), ((System.Byte)(156)), ((System.Byte)(112)));
            this.DisplayName = "Sienna";
            this.InactiveSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(229)), ((System.Byte)(225)), ((System.Byte)(184)));
            this.InactiveTabBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(227)), ((System.Byte)(224)), ((System.Byte)(202)));
            this.InactiveTabHighlightColor = System.Drawing.Color.White;
            this.InactiveTabLowlightColor = System.Drawing.Color.FromArgb(((System.Byte)(214)), ((System.Byte)(208)), ((System.Byte)(181)));
            this.InactiveTabTextColor = System.Drawing.Color.Black;
            this.InactiveTabTopColor = System.Drawing.Color.FromArgb(((System.Byte)(227)), ((System.Byte)(224)), ((System.Byte)(202)));
            this.ItalicApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Italic);
            this.LinkApplicationFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Underline);
            this.MenuBitmapAreaColor = System.Drawing.Color.FromArgb(((System.Byte)(227)), ((System.Byte)(222)), ((System.Byte)(178)));
            this.MenuSelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(128)), ((System.Byte)(161)), ((System.Byte)(156)), ((System.Byte)(112)));
            this.NormalApplicationFont = new System.Drawing.Font("Tahoma", 8.25F);
            this.PrimaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(199)), ((System.Byte)(194)), ((System.Byte)(149)));
            this.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor = System.Drawing.Color.FromArgb(((System.Byte)(171)), ((System.Byte)(166)), ((System.Byte)(120)));
            this.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor = System.Drawing.Color.FromArgb(((System.Byte)(214)), ((System.Byte)(208)), ((System.Byte)(181)));
            this.PrimaryWorkspaceCommandBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(203)), ((System.Byte)(198)), ((System.Byte)(152)));
            this.PrimaryWorkspaceCommandBarBottomLayoutMargin = 3;
            this.PrimaryWorkspaceCommandBarDisabledTextColor = System.Drawing.Color.Gray;
            this.PrimaryWorkspaceCommandBarLeftLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarRightLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarSeparatorLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarTextColor = System.Drawing.Color.Black;
            this.PrimaryWorkspaceCommandBarTopBevelFirstLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopBevelSecondLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(250)), ((System.Byte)(245)), ((System.Byte)(202)));
            this.PrimaryWorkspaceCommandBarTopLayoutMargin = 2;
            this.PrimaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(199)), ((System.Byte)(194)), ((System.Byte)(149)));
            this.SecondaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(226)), ((System.Byte)(221)), ((System.Byte)(176)));
            this.SecondaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((System.Byte)(226)), ((System.Byte)(221)), ((System.Byte)(176)));
            this.SmallApplicationFont = new System.Drawing.Font("Tahoma", 6.75F);
            this.ToolWindowBackgroundColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowBorderColor = System.Drawing.Color.FromArgb(((System.Byte)(72)), ((System.Byte)(100)), ((System.Byte)(165)));
            this.ToolWindowTitleBarBottomColor = System.Drawing.Color.FromArgb(((System.Byte)(94)), ((System.Byte)(131)), ((System.Byte)(200)));
            this.ToolWindowTitleBarFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.ToolWindowTitleBarTextColor = System.Drawing.Color.White;
            this.ToolWindowTitleBarTopColor = System.Drawing.Color.FromArgb(((System.Byte)(126)), ((System.Byte)(166)), ((System.Byte)(237)));
            this.WindowColor = System.Drawing.Color.White;
            this.WorkspacePaneControlColor = System.Drawing.Color.FromArgb(((System.Byte)(227)), ((System.Byte)(224)), ((System.Byte)(202)));

        }
        #endregion

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public override Image PreviewImage
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.Sienna.png");
            }
        }
    }
}
