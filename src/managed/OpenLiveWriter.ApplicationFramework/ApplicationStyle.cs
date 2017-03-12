// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// The ApplicationStyle class.  Defines common application style elements.
    /// </summary>
    public class ApplicationStyle : Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The display name of the ApplicationStyle.
        /// </summary>
        private string displayName;

        /// <summary>
        /// Gets or sets the display name of the ApplicationStyle.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return displayName;
            }
            set
            {
                if (displayName != value)
                {
                    displayName = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public virtual Image PreviewImage
        {
            get
            {
                return null;
            }
        }

        public virtual Font NormalApplicationFont
        {
            get
            {
                return Res.DefaultFont;
            }
        }

        /// <summary>
        /// The active selection color.
        /// </summary>
        private Color activeSelectionColor;

        /// <summary>
        /// Gets or sets the active selection color.
        /// </summary>
        [
            Category("Appearance.Selection"),
                Localizable(false),
                Description("Specifies the active selection color.")
        ]
        public virtual Color ActiveSelectionColor
        {
            get
            {
                return activeSelectionColor;
            }
            set
            {
                if (activeSelectionColor != value)
                {
                    activeSelectionColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive selection color.
        /// </summary>
        private Color inactiveSelectionColor;

        /// <summary>
        /// Gets or sets the inactive selection color.
        /// </summary>
        [
            Category("Appearance.Selection"),
                Localizable(false),
                Description("Specifies the inactive selection color.")
        ]
        public virtual Color InactiveSelectionColor
        {
            get
            {
                return inactiveSelectionColor;
            }
            set
            {
                if (inactiveSelectionColor != value)
                {
                    inactiveSelectionColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The menu bitmap area color.
        /// </summary>
        private Color menuBitmapAreaColor;

        /// <summary>
        /// Gets or sets the menu bitmap area color.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                Description("Specifies the menu bitmap area color.")
        ]
        public virtual Color MenuBitmapAreaColor
        {
            get
            {
                return menuBitmapAreaColor;
            }
            set
            {
                if (menuBitmapAreaColor != value)
                {
                    menuBitmapAreaColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The menu selection color.
        /// </summary>
        private Color menuSelectionColor;

        /// <summary>
        /// Gets or sets the menu selection color.
        /// </summary>
        [
            Category("Appearance.Selection"),
                Localizable(false),
                Description("Specifies the menu selection color.")
        ]
        public virtual Color MenuSelectionColor
        {
            get
            {
                return menuSelectionColor;
            }
            set
            {
                if (menuSelectionColor != value)
                {
                    menuSelectionColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace top color.
        /// </summary>
        private Color primaryWorkspaceTopColor;

        /// <summary>
        /// Gets or sets primary workspace color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace top color.")
        ]
        public virtual Color PrimaryWorkspaceTopColor
        {
            get
            {
                return primaryWorkspaceTopColor;
            }
            set
            {
                if (primaryWorkspaceTopColor != value)
                {
                    primaryWorkspaceTopColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace bottom color.
        /// </summary>
        private Color primaryWorkspaceBottomColor;

        /// <summary>
        /// Gets or sets primary workspace bottom color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace bottom color.")
        ]
        public virtual Color PrimaryWorkspaceBottomColor
        {
            get
            {
                return primaryWorkspaceBottomColor;
            }
            set
            {
                if (primaryWorkspaceBottomColor != value)
                {
                    primaryWorkspaceBottomColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The secondary workspace top color.
        /// </summary>
        private Color secondaryWorkspaceTopColor;

        /// <summary>
        /// Gets or sets secondary workspace top color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the secondary workspace top color.")
        ]
        public virtual Color SecondaryWorkspaceTopColor
        {
            get
            {
                return secondaryWorkspaceTopColor;
            }
            set
            {
                if (secondaryWorkspaceTopColor != value)
                {
                    secondaryWorkspaceTopColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The secondary workspace bottom color.
        /// </summary>
        private Color secondaryWorkspaceBottomColor;

        /// <summary>
        /// Gets or sets secondary workspace color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the secondary workspace bottom color.")
        ]
        public virtual Color SecondaryWorkspaceBottomColor
        {
            get
            {
                return secondaryWorkspaceBottomColor;
            }
            set
            {
                if (secondaryWorkspaceBottomColor != value)
                {
                    secondaryWorkspaceBottomColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        #region Primary Workspace Command Bar

        /// <summary>
        /// The primary workspace command bar top color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTopColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar top color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top color.")
        ]
        public virtual Color PrimaryWorkspaceCommandBarTopColor
        {
            get
            {
                return primaryWorkspaceCommandBarTopColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarTopColor != value)
                {
                    primaryWorkspaceCommandBarTopColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar bottom color.
        /// </summary>
        private Color primaryWorkspaceCommandBarBottomColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom color.")
        ]
        public virtual Color PrimaryWorkspaceCommandBarBottomColor
        {
            get
            {
                return primaryWorkspaceCommandBarBottomColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarBottomColor != value)
                {
                    primaryWorkspaceCommandBarBottomColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar top bevel first line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTopBevelFirstLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar top bevel first line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top bevel first line color.")
        ]
        public Color PrimaryWorkspaceCommandBarTopBevelFirstLineColor
        {
            get
            {
                return primaryWorkspaceCommandBarTopBevelFirstLineColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarTopBevelFirstLineColor != value)
                {
                    primaryWorkspaceCommandBarTopBevelFirstLineColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar top bevel second line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTopBevelSecondLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar top bevel second line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top bevel second line color.")
        ]
        public Color PrimaryWorkspaceCommandBarTopBevelSecondLineColor
        {
            get
            {
                return primaryWorkspaceCommandBarTopBevelSecondLineColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarTopBevelSecondLineColor != value)
                {
                    primaryWorkspaceCommandBarTopBevelSecondLineColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar bottom bevel first line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarBottomBevelFirstLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom bevel first line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom bevel first line color.")
        ]
        public Color PrimaryWorkspaceCommandBarBottomBevelFirstLineColor
        {
            get
            {
                return primaryWorkspaceCommandBarBottomBevelFirstLineColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarBottomBevelFirstLineColor != value)
                {
                    primaryWorkspaceCommandBarBottomBevelFirstLineColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar bottom bevel second line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarBottomBevelSecondLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom bevel second line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom bevel second line color.")
        ]
        public Color PrimaryWorkspaceCommandBarBottomBevelSecondLineColor
        {
            get
            {
                return primaryWorkspaceCommandBarBottomBevelSecondLineColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarBottomBevelSecondLineColor != value)
                {
                    primaryWorkspaceCommandBarBottomBevelSecondLineColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar text color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTextColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar text color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar text color.")
        ]
        public Color PrimaryWorkspaceCommandBarTextColor
        {
            get
            {
                return primaryWorkspaceCommandBarTextColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarTextColor != value)
                {
                    primaryWorkspaceCommandBarTextColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar disabled text color.
        /// </summary>
        private Color primaryWorkspaceCommandBarDisabledTextColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar disabled text color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar disabled text color.")
        ]
        public Color PrimaryWorkspaceCommandBarDisabledTextColor
        {
            get
            {
                return primaryWorkspaceCommandBarDisabledTextColor;
            }
            set
            {
                if (primaryWorkspaceCommandBarDisabledTextColor != value)
                {
                    primaryWorkspaceCommandBarDisabledTextColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar top layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarTopLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar top layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarTopLayoutMargin
        {
            get
            {
                return primaryWorkspaceCommandBarTopLayoutMargin;
            }
            set
            {
                if (primaryWorkspaceCommandBarTopLayoutMargin != value)
                {
                    primaryWorkspaceCommandBarTopLayoutMargin = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar left layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarLeftLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar left layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar left layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarLeftLayoutMargin
        {
            get
            {
                return primaryWorkspaceCommandBarLeftLayoutMargin;
            }
            set
            {
                if (primaryWorkspaceCommandBarLeftLayoutMargin != value)
                {
                    primaryWorkspaceCommandBarLeftLayoutMargin = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar bottom layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarBottomLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarBottomLayoutMargin
        {
            get
            {
                return primaryWorkspaceCommandBarBottomLayoutMargin;
            }
            set
            {
                if (primaryWorkspaceCommandBarBottomLayoutMargin != value)
                {
                    primaryWorkspaceCommandBarBottomLayoutMargin = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar right layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarRightLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar right layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar right layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarRightLayoutMargin
        {
            get
            {
                return primaryWorkspaceCommandBarRightLayoutMargin;
            }
            set
            {
                if (primaryWorkspaceCommandBarRightLayoutMargin != value)
                {
                    primaryWorkspaceCommandBarRightLayoutMargin = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar separator layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarSeparatorLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar separator layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary primary workspace command bar separator layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarSeparatorLayoutMargin
        {
            get
            {
                return primaryWorkspaceCommandBarSeparatorLayoutMargin;
            }
            set
            {
                if (primaryWorkspaceCommandBarSeparatorLayoutMargin != value)
                {
                    primaryWorkspaceCommandBarSeparatorLayoutMargin = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        #endregion Primary Workspace Command Bar

        /// <summary>
        /// The border color.
        /// </summary>
        private Color borderColor;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        [
            Category("Appearance.Border"),
                Localizable(false),
                Description("Specifies the border color.")
        ]
        public Color BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The window color.
        /// </summary>
        private Color windowColor;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        [
            Category("Appearance.Window"),
                Localizable(false),
                Description("Specifies the window color.")
        ]
        public Color WindowColor
        {
            get
            {
                return windowColor;
            }
            set
            {
                if (windowColor != value)
                {
                    windowColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The workspace pane control color.
        /// </summary>
        private Color workspacePaneControlColor;

        /// <summary>
        /// Gets or sets the workspace pane control color.
        /// </summary>
        [
            Category("Appearance.Workspace"),
                Localizable(false),
                Description("Specifies the workspace pane control color.")
        ]
        public Color WorkspacePaneControlColor
        {
            get
            {
                return workspacePaneControlColor;
            }
            set
            {
                if (workspacePaneControlColor != value)
                {
                    workspacePaneControlColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The alert control color.
        /// </summary>
        private Color alertControlColor;

        /// <summary>
        /// Gets or sets the alert control color.
        /// </summary>
        [
            Category("Appearance.Alert"),
                Localizable(false),
                Description("Specifies the alert control color.")
        ]
        public Color AlertControlColor
        {
            get
            {
                return alertControlColor;
            }
            set
            {
                if (alertControlColor != value)
                {
                    alertControlColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab top color.
        /// </summary>
        private Color activeTabTopColor;

        /// <summary>
        /// Gets or sets the active tab color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab top color.")
        ]
        public Color ActiveTabTopColor
        {
            get
            {
                return activeTabTopColor;
            }
            set
            {
                if (activeTabTopColor != value)
                {
                    activeTabTopColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab bottom color.
        /// </summary>
        private Color activeTabBottomColor;

        /// <summary>
        /// Gets or sets the active tab color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab bottom color.")
        ]
        public Color ActiveTabBottomColor
        {
            get
            {
                return activeTabBottomColor;
            }
            set
            {
                if (activeTabBottomColor != value)
                {
                    activeTabBottomColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab highlight color.
        /// </summary>
        private Color activeTabHighlightColor;

        /// <summary>
        /// Gets or sets the active tab highlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab highlight color.")
        ]
        public Color ActiveTabHighlightColor
        {
            get
            {
                return activeTabHighlightColor;
            }
            set
            {
                if (activeTabHighlightColor != value)
                {
                    activeTabHighlightColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab lowlight color.
        /// </summary>
        private Color activeTabLowlightColor;

        /// <summary>
        /// Gets or sets the active tab lowlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab lowlight color.")
        ]
        public Color ActiveTabLowlightColor
        {
            get
            {
                return activeTabLowlightColor;
            }
            set
            {
                if (activeTabLowlightColor != value)
                {
                    activeTabLowlightColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab text color.
        /// </summary>
        private Color activeTabTextColor;

        /// <summary>
        /// Gets or sets the active tab text color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab text color.")
        ]
        public Color ActiveTabTextColor
        {
            get
            {
                return activeTabTextColor;
            }
            set
            {
                if (activeTabTextColor != value)
                {
                    activeTabTextColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab top color.
        /// </summary>
        private Color inactiveTabTopColor;

        /// <summary>
        /// Gets or sets the inactive tab color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab top color.")
        ]
        public Color InactiveTabTopColor
        {
            get
            {
                return inactiveTabTopColor;
            }
            set
            {
                if (inactiveTabTopColor != value)
                {
                    inactiveTabTopColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab bottom color.
        /// </summary>
        private Color inactiveTabBottomColor;

        /// <summary>
        /// Gets or sets the inactive tab bottom color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab bottom color.")
        ]
        public Color InactiveTabBottomColor
        {
            get
            {
                return inactiveTabBottomColor;
            }
            set
            {
                if (inactiveTabBottomColor != value)
                {
                    inactiveTabBottomColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab highlight color.
        /// </summary>
        private Color inactiveTabHighlightColor;

        /// <summary>
        /// Gets or sets the inactive tab highlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab highlight color.")
        ]
        public Color InactiveTabHighlightColor
        {
            get
            {
                return inactiveTabHighlightColor;
            }
            set
            {
                if (inactiveTabHighlightColor != value)
                {
                    inactiveTabHighlightColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab lowlight color.
        /// </summary>
        private Color inactiveTabLowlightColor;

        /// <summary>
        /// Gets or sets the inactive tab lowlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab lowlight color.")
        ]
        public Color InactiveTabLowlightColor
        {
            get
            {
                return inactiveTabLowlightColor;
            }
            set
            {
                if (inactiveTabLowlightColor != value)
                {
                    inactiveTabLowlightColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab text color.
        /// </summary>
        private Color inactiveTabTextColor;

        /// <summary>
        /// Gets or sets the inactive tab text color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab text color.")
        ]
        public Color InactiveTabTextColor
        {
            get
            {
                return inactiveTabTextColor;
            }
            set
            {
                if (inactiveTabTextColor != value)
                {
                    inactiveTabTextColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window border color.
        /// </summary>
        private Color toolWindowBorderColor;

        /// <summary>
        /// Gets or sets the tool window border color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window border color.")
        ]
        public Color ToolWindowBorderColor
        {
            get
            {
                return toolWindowBorderColor;
            }
            set
            {
                if (toolWindowBorderColor != value)
                {
                    toolWindowBorderColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window title bar top color.
        /// </summary>
        private Color toolWindowTitleBarTopColor;

        /// <summary>
        /// Gets or sets the tool window title bar top color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window title bar top color.")
        ]
        public Color ToolWindowTitleBarTopColor
        {
            get
            {
                return toolWindowTitleBarTopColor;
            }
            set
            {
                if (toolWindowTitleBarTopColor != value)
                {
                    toolWindowTitleBarTopColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window title bar bottom color.
        /// </summary>
        private Color toolWindowTitleBarBottomColor;

        /// <summary>
        /// Gets or sets the tool window title bar bottom color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window title bar bottom color.")
        ]
        public Color ToolWindowTitleBarBottomColor
        {
            get
            {
                return toolWindowTitleBarBottomColor;
            }
            set
            {
                if (toolWindowTitleBarBottomColor != value)
                {
                    toolWindowTitleBarBottomColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window title bar text color.
        /// </summary>
        private Color toolWindowTitleBarTextColor;

        /// <summary>
        /// Gets or sets the tool window title bar text color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window title bar text color.")
        ]
        public Color ToolWindowTitleBarTextColor
        {
            get
            {
                return toolWindowTitleBarTextColor;
            }
            set
            {
                if (toolWindowTitleBarTextColor != value)
                {
                    toolWindowTitleBarTextColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window background color.
        /// </summary>
        private Color toolWindowBackgroundColor;

        /// <summary>
        /// Gets or sets the tool window background color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window background color.")
        ]
        public Color ToolWindowBackgroundColor
        {
            get
            {
                return toolWindowBackgroundColor;
            }
            set
            {
                if (toolWindowBackgroundColor != value)
                {
                    toolWindowBackgroundColor = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when a setting in ApplicationStyle changes.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Initializes a new instance of the ApplicationStyle class.
        /// </summary>
        public ApplicationStyle()
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
            components = new System.ComponentModel.Container();
        }

        #endregion

        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }
    }
}
