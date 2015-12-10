// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandInsertLink : Command
    {

        public CommandInsertLink(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();
        }

        public CommandInsertLink()
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
            // CommandInsertLink
            //
            this.ContextMenuPath = "-&Hyperlink...@130";
            this.Identifier = "OpenLiveWriter.ApplicationFramework.Commands.InsertLink";
            this.MainMenuPath = "&Insert@4/&Hyperlink...@100";
            this.MenuText = "&Hyperlink...";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlK;
            this.Text = "Insert Hyperlink";

        }
        #endregion
    }
}
