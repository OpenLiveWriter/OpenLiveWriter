// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandClear.
    /// </summary>
    public class CommandFind : Command
    {

        public CommandFind(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandFind()
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
            // CommandFind
            //
            this.ContextMenuPath = "Fi&nd Text...@601";
            this.Identifier = "MindShare.ApplicationCore.Commands.Find";
            this.MainMenuPath = "&Edit@2/Fi&nd Text...@601";
            this.MenuText = "Fi&nd Text...";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
            this.Text = "Find";

        }
        #endregion
    }
}
