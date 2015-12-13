// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandPasteSpecial : Command
    {

        public CommandPasteSpecial(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandPasteSpecial()
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
            this.ContextMenuPath = "";
            this.Identifier = "MindShare.ApplicationCore.Commands.PasteSpecial";
            this.MainMenuPath = "&Edit@2/&Paste Special...@108";
            this.MenuText = "Paste Spe&cial...";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftV;
            this.Text = "Paste Special...";

        }
        #endregion
    }
}
