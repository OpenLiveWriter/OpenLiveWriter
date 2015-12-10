// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for PrintCommand.
    /// </summary>
    public class CommandPageSetup : Command
    {

        public CommandPageSetup(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        public CommandPageSetup()
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
            // CommandPageSetup
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.PageSetup";
            this.MainMenuPath = "&File@0/-Page Set&up...@120";
            this.Text = "Page Setup";

        }
        #endregion
    }
}
