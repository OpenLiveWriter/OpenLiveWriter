// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandMinimize.
    /// </summary>
    public class CommandClose : Command
    {

        public CommandClose(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public CommandClose()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
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
            // CommandClose
            //
            this.AccessibleDescription = "";
            this.ContextMenuPath = "-Close@20";
            this.Identifier = "OpenLiveWriter.ApplicationFramework.CommandClose";
            this.MainMenuPath = "&File@0/-&Close@500";
            this.Shortcut = System.Windows.Forms.Shortcut.AltF4;
            this.Text = "Close";
            this.VisibleOnMainMenu = true;

        }
        #endregion
    }
}
