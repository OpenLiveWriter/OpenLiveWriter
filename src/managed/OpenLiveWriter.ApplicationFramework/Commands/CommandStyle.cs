// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandStyle : Command
    {

        public CommandStyle(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandStyle()
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
            // CommandStyle
            //
            this.ContextMenuPath = "";
            this.Identifier = "MindShare.ApplicationCore.Commands.Style";
            this.MainMenuPath = "";
            this.MenuText = "";
            this.Text = "Style";
            this.VisibleOnContextMenu = false;
            this.VisibleOnMainMenu = false;

        }
        #endregion
    }
}
