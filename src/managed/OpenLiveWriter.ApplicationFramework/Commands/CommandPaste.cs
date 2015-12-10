// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandPaste : Command
    {

        public CommandPaste(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandPaste()
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
            // CommandPaste
            //
            this.ContextMenuPath = "&Paste@104";
            this.Identifier = "MindShare.ApplicationCore.Commands.Paste";
            this.MainMenuPath = "&Edit@2/&Paste@106";
            this.MenuText = "&Paste";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
            this.Text = "Paste";

        }
        #endregion
    }
}
