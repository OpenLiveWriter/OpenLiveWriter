// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a primary WorkspaceCommandBarLightweightControl.
    /// </summary>
    public class PrimaryWorkspaceWorkspaceCommandBarLightweightControl : CommandBarLightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Gets the top layout margin.
        /// </summary>
        public override int TopLayoutMargin
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopLayoutMargin;
            }
        }

        /// <summary>
        /// Gets the left layout margin.
        /// </summary>
        public override int LeftLayoutMargin
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarLeftLayoutMargin;
            }
        }

        /// <summary>
        /// Gets the bottom layout margin.
        /// </summary>
        public override int BottomLayoutMargin
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomLayoutMargin;
            }
        }

        /// <summary>
        /// Gets the right layout margin.
        /// </summary>
        public override int RightLayoutMargin
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarRightLayoutMargin;
            }
        }

        /// <summary>
        /// Gets the separator layout margin.
        /// </summary>
        public override int SeparatorLayoutMargin
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarSeparatorLayoutMargin;
            }
        }

        /// <summary>
        ///	Gets the top command bar color.
        /// </summary>
        public override Color TopColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopColor;
            }
        }

        /// <summary>
        ///	Gets the bottom command bar color.
        /// </summary>
        public override Color BottomColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor;
            }
        }

        /// <summary>
        ///	Gets the top bevel first line color.
        /// </summary>
        public override Color TopBevelFirstLineColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopBevelFirstLineColor;
            }
        }

        /// <summary>
        ///	Gets the top bevel second line color.
        /// </summary>
        public override Color TopBevelSecondLineColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopBevelSecondLineColor;
            }
        }

        /// <summary>
        ///	Gets the bottom bevel first line color.
        /// </summary>
        public override Color BottomBevelFirstLineColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor;
            }
        }

        /// <summary>
        ///	Gets the bottom bevel second line color.
        /// </summary>
        public override Color BottomBevelSecondLineColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor;
            }
        }

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public override Color TextColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor;
            }
        }

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public override Color DisabledTextColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarDisabledTextColor;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationWorkspaceCommandBarLightweightControl class.
        /// </summary>
        public PrimaryWorkspaceWorkspaceCommandBarLightweightControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationWorkspaceCommandBarLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public PrimaryWorkspaceWorkspaceCommandBarLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
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
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion
    }
}
