// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Primary version of the WorkspaceControl.
    /// </summary>
    public class PrimaryWorkspaceControl : WorkspaceControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The first CommandBarLightweightControl.
        /// </summary>
        private PrimaryWorkspaceWorkspaceCommandBarLightweightControl primaryWorkspaceFirstCommandBarLightweightControl;

        /// <summary>
        /// The second CommandBarLightweightControl.
        /// </summary>
        private PrimaryWorkspaceWorkspaceCommandBarLightweightControl primaryWorkspaceSecondCommandBarLightweightControl;

        /// <summary>
        /// Gets the first command bar lightweight control.
        /// </summary>
        public override CommandBarLightweightControl FirstCommandBarLightweightControl
        {
            get
            {
                return primaryWorkspaceFirstCommandBarLightweightControl;
            }
        }

        /// <summary>
        /// Gets the second command bar lightweight control.
        /// </summary>
        public override CommandBarLightweightControl SecondCommandBarLightweightControl
        {
            get
            {
                return primaryWorkspaceSecondCommandBarLightweightControl;
            }
        }

        /// <summary>
        ///	Gets the top color.
        /// </summary>
        public override Color TopColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceTopColor;
            }
        }

        /// <summary>
        ///	Gets the bottom color.
        /// </summary>
        public override Color BottomColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.PrimaryWorkspaceBottomColor;
            }
        }

        /// <summary>
        ///	Initializes a new instance of the PrimaryWorkspaceControl class.
        /// </summary>
        public PrimaryWorkspaceControl(CommandManager commandManager) : base(commandManager)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Set the CommandManager of the CommandBarLightweightControls.
            primaryWorkspaceFirstCommandBarLightweightControl.CommandManager = commandManager;
            primaryWorkspaceSecondCommandBarLightweightControl.CommandManager = commandManager;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                primaryWorkspaceFirstCommandBarLightweightControl.LightweightControlContainerControl = null;
                primaryWorkspaceSecondCommandBarLightweightControl.LightweightControlContainerControl = null;
                primaryWorkspaceFirstCommandBarLightweightControl.CommandBarDefinition = null;
                primaryWorkspaceSecondCommandBarLightweightControl.CommandBarDefinition = null;
                primaryWorkspaceFirstCommandBarLightweightControl.CommandManager = null;
                primaryWorkspaceSecondCommandBarLightweightControl.CommandManager = null;
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
            this.components = new System.ComponentModel.Container();
            this.primaryWorkspaceFirstCommandBarLightweightControl = new OpenLiveWriter.ApplicationFramework.PrimaryWorkspaceWorkspaceCommandBarLightweightControl(this.components);
            this.primaryWorkspaceSecondCommandBarLightweightControl = new OpenLiveWriter.ApplicationFramework.PrimaryWorkspaceWorkspaceCommandBarLightweightControl(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.primaryWorkspaceFirstCommandBarLightweightControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.primaryWorkspaceSecondCommandBarLightweightControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // primaryWorkspaceFirstCommandBarLightweightControl
            //
            this.primaryWorkspaceFirstCommandBarLightweightControl.LightweightControlContainerControl = this;
            //
            // primaryWorkspaceSecondCommandBarLightweightControl
            //
            this.primaryWorkspaceSecondCommandBarLightweightControl.LightweightControlContainerControl = this;
            //
            // PrimaryWorkspaceControl
            //
            this.AllowDrop = false;
            this.Name = "PrimaryWorkspaceControl";
            ((System.ComponentModel.ISupportInitialize)(this.primaryWorkspaceFirstCommandBarLightweightControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.primaryWorkspaceSecondCommandBarLightweightControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion
    }
}
