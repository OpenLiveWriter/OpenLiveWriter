// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandClear.
    /// </summary>
    public class CommandClear : Command
    {

        public CommandClear(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandClear()
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
            // CommandClear
            //
            this.ContextMenuPath = "-Clea&r@151";
            this.Identifier = "MindShare.ApplicationCore.Commands.Clear";
            this.MainMenuPath = "&Edit@2/-Clea&r@151";
            this.MenuText = "-Clea&r";
            this.Shortcut = System.Windows.Forms.Shortcut.Del;
            this.Text = "Clear";

        }
        #endregion
    }
}
