// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandFocusPreviousPane.
    /// </summary>
    public class CommandFocusPreviousPane : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandFocusPreviousPane(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            //
            //
            //
        }

        public CommandFocusPreviousPane()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
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
            // CommandFocusPreviousPane
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.FocusPreviousPane";
            this.Shortcut = System.Windows.Forms.Shortcut.ShiftF6;
            this.Text = "Previous Pane";
            this.VisibleOnCommandBar = false;
            this.VisibleOnContextMenu = false;
            this.VisibleOnMainMenu = false;

        }
        #endregion
    }
}
