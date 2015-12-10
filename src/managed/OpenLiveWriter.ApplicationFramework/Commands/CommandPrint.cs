// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for PrintCommand.
    /// </summary>
    public class CommandPrint : Command
    {

        public CommandPrint(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        public CommandPrint()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // CommandPrint
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.Print";
            this.MainMenuPath = "&File@0/&Print...@121";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.Text = "Print";

        }
        #endregion
    }
}
