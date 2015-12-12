// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandRename.
    /// </summary>
    public class CommandRedo : Command
    {

        public CommandRedo(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandRedo()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // CommandRedo
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.Redo";
            this.MainMenuPath = "&Edit@2/&Redo-@100";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlY;
            this.Text = "Redo";

        }
        #endregion
    }
}
