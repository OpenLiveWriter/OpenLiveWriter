// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandCheckSpelling : Command
    {

        public CommandCheckSpelling(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandCheckSpelling()
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
            // CommandCheckSpelling
            //
            this.CommandBarButtonText = "";
            this.ContextMenuPath = "";
            this.MenuText = "Check &Spelling";
            this.Identifier = "MindShare.ApplicationCore.Commands.CheckSpelling";
            this.MainMenuPath = "&Tools@7/Check &Spelling...@100";
            this.Shortcut = System.Windows.Forms.Shortcut.F7;
            this.Text = "Check Spelling";

        }
        #endregion
    }
}
