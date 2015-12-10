// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandDelete : Command
    {

        public CommandDelete(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandDelete()
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
            // CommandDelete
            //
            this.ContextMenuPath = "-&Delete-@151";
            this.Identifier = "MindShare.ApplicationCore.Commands.Delete";
            this.MainMenuPath = "&Edit@2/-&Delete-@151";
            this.MenuText = "&Delete";
            this.Shortcut = System.Windows.Forms.Shortcut.Del;
            this.Text = "Delete";

        }
        #endregion
    }
}
