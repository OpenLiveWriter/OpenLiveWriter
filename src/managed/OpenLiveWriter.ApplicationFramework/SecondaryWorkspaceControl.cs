// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Secondary version of the WorkspaceControl.
    /// </summary>
    public class SecondaryWorkspaceControl : WorkspaceControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        ///	Gets the top color.
        /// </summary>
        public override Color TopColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.SecondaryWorkspaceTopColor;
            }
        }

        /// <summary>
        ///	Gets the bottom color.
        /// </summary>
        public override Color BottomColor
        {
            get
            {
                return ApplicationManager.ApplicationStyle.SecondaryWorkspaceBottomColor;
            }
        }

        /// <summary>
        ///	Initializes a new instance of the SecondaryWorkspaceControl class.
        /// </summary>
        public SecondaryWorkspaceControl() : this(ApplicationManager.CommandManager)
        {
        }

        /// <summary>
        ///	Initializes a new instance of the SecondaryWorkspaceControl class.
        /// </summary>
        public SecondaryWorkspaceControl(CommandManager commandManager) : base(commandManager)
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
            components = new System.ComponentModel.Container();
        }
        #endregion
    }
}
