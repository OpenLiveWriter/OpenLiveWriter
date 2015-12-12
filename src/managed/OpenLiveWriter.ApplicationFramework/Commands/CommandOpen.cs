// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandOpen : Command
    {

        public CommandOpen(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandOpen()
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
            // CommandOpen
            //
            this.ContextMenuPath = "&Open...@201";
            this.Identifier = "MindShare.ApplicationCore.Commands.Open";
            this.MainMenuPath = "&Edit@2/&Open...@201";
            this.MenuText = "&Open...";
            this.Text = "Open";

        }
        #endregion
    }
}
