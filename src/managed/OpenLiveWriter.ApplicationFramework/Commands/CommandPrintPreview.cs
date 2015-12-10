// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for PrintCommand.
    /// </summary>
    public class CommandPrintPreview : Command
    {

        public CommandPrintPreview(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        public CommandPrintPreview()
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
            // CommandPrintPreview
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.PrintPreview";
            this.MainMenuPath = "&File@0/Print Pre&view...-@122";
            this.Text = "Print Preview";

        }
        #endregion
    }
}
