// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandIndent : Command
    {

        public CommandIndent(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandIndent()
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
            // CommandIndent
            //
            this.ContextMenuPath = "-&Increase Indent@110";
            this.Identifier = "MindShare.ApplicationCore.Commands.Indent";
            this.MainMenuPath = "F&ormat@5/&Indent@500/&Increase@100";
            this.MenuText = "&Increase Indent";
            this.Text = "Increase Indent";

        }
        #endregion
    }
}
