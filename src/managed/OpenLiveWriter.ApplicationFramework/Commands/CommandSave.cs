// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandSave.
    /// </summary>
    public class CommandSave : Command
    {

        public CommandSave(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandSave()
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
            // CommandSave
            //
            this.Identifier = "MindShare.ApplicationCore.Commands.Save";
            this.MainMenuPath = "&File@0/&Save@10";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.Text = "Save";

        }
        #endregion
    }
}
