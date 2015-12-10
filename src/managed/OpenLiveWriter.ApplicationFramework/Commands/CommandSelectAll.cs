// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandClear.
    /// </summary>
    public class CommandSelectAll : Command
    {

        public CommandSelectAll(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandSelectAll()
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
            // CommandSelectAll
            //
            this.ContextMenuPath = "-Select &All@501";
            this.Identifier = "MindShare.ApplicationCore.Commands.SelectAll";
            this.MainMenuPath = "&Edit@2/-Select &All@501";
            this.MenuText = "Select &All";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
            this.Text = "Select All";

        }
        #endregion
    }
}
