// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    using System.ComponentModel;
    using System.Drawing;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// The application style sky blue.
    /// </summary>
    public class ApplicationStyleSkyBlue : ApplicationStyle
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStyleSkyBlue"/> class.
        /// </summary>
        public ApplicationStyleSkyBlue()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public override Image PreviewImage
            => ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.SkyBlue.png"); // Not L10N

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // ApplicationStyleSkyBlue
            this.ActiveSelectionColor = Color.FromArgb(107, 140, 210);
            this.ActiveTabBottomColor = Color.FromArgb(183, 203, 245);
            this.ActiveTabHighlightColor = Color.FromArgb(243, 243, 243);
            this.ActiveTabLowlightColor = Color.FromArgb(181, 193, 196);
            this.ActiveTabTextColor = Color.Black;
            this.ActiveTabTopColor = Color.FromArgb(207, 227, 253);
            this.AlertControlColor = Color.FromArgb(255, 255, 194);
            this.BorderColor = Color.FromArgb(117, 135, 179);
            this.DisplayName = "Sky Blue"; // Not L10N
            this.InactiveSelectionColor = Color.FromArgb(236, 233, 216);
            this.InactiveTabBottomColor = Color.FromArgb(228, 228, 228);
            this.InactiveTabHighlightColor = Color.FromArgb(243, 243, 243);
            this.InactiveTabLowlightColor = Color.FromArgb(189, 189, 189);
            this.InactiveTabTextColor = Color.Black;
            this.InactiveTabTopColor = Color.FromArgb(232, 232, 232);
            this.MenuBitmapAreaColor = Color.FromArgb(195, 215, 249);
            this.MenuSelectionColor = Color.FromArgb(128, 117, 135, 179);
            this.PrimaryWorkspaceBottomColor = Color.FromArgb(161, 180, 215);
            this.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor = Color.FromArgb(117, 135, 179);
            this.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor = Color.FromArgb(166, 187, 223);
            this.PrimaryWorkspaceCommandBarBottomColor = Color.FromArgb(148, 173, 222);
            this.PrimaryWorkspaceCommandBarBottomLayoutMargin = 3;
            this.PrimaryWorkspaceCommandBarDisabledTextColor = Color.Gray;
            this.PrimaryWorkspaceCommandBarLeftLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarRightLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarSeparatorLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarTextColor = Color.Black;
            this.PrimaryWorkspaceCommandBarTopBevelFirstLineColor = Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopBevelSecondLineColor = Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopColor = Color.FromArgb(193, 213, 249);
            this.PrimaryWorkspaceCommandBarTopLayoutMargin = 2;
            this.PrimaryWorkspaceTopColor = Color.FromArgb(154, 174, 213);
            this.SecondaryWorkspaceBottomColor = Color.FromArgb(183, 203, 245);
            this.SecondaryWorkspaceTopColor = Color.FromArgb(183, 203, 245);
            this.ToolWindowBackgroundColor = Color.FromArgb(94, 131, 200);
            this.ToolWindowBorderColor = Color.FromArgb(72, 100, 165);
            this.ToolWindowTitleBarBottomColor = Color.FromArgb(94, 131, 200);
            this.ToolWindowTitleBarTextColor = Color.White;
            this.ToolWindowTitleBarTopColor = Color.FromArgb(126, 166, 237);
            this.WindowColor = Color.White;
            this.WorkspacePaneControlColor = Color.FromArgb(214, 223, 247);
        }
    }
}
