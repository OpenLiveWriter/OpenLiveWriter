// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandPasteWithoutFormatting : Command
    {

        public CommandPasteWithoutFormatting(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandPasteWithoutFormatting()
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
            // CommandPasteWithoutFormatting
            //
            this.ContextMenuPath = "Paste &Without Formatting@105";
            this.Identifier = "MindShare.ApplicationCore.Commands.PasteWithoutFormatting";
            this.MainMenuPath = "&Edit@2/Paste &Without Formatting@105";
            this.MenuText = "Paste &Without Formatting";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftV;
            this.Text = "Paste";

        }
        #endregion
    }
}
