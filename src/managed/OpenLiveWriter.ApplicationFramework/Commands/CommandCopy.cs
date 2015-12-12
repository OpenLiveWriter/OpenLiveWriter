// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandCopy : Command
    {

        public CommandCopy(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandCopy()
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
            // CommandCopy
            //
            this.ContextMenuPath = "&Copy@102";
            this.Identifier = "MindShare.ApplicationCore.Commands.Copy";
            this.MainMenuPath = "&Edit@2/&Copy@102";
            this.MenuText = "&Copy";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.Text = "Copy";

        }
        #endregion
    }
}
