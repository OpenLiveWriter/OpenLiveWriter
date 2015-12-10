// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandRename.
    /// </summary>
    public class CommandUndo : Command
    {

        public CommandUndo(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandUndo()
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
            // CommandUndo
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.Undo";
            this.MainMenuPath = "&Edit@2/&Undo@99";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
            this.Text = "Undo";

        }
        #endregion
    }
}
