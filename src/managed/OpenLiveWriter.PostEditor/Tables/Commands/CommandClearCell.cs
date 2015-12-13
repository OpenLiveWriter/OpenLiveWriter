// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework ;

namespace OpenLiveWriter.PostEditor.Tables.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandClearCell : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandClearCell(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandClearCell()
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
            // CommandClearCell
            //
            this.Identifier = "OpenLiveWriter.PostEditor.Tables.Commands.ClearCell";
            this.MainMenuPath = "T&able@6/-Cl&ear Cell{0}@320";
            this.MenuText = "Cl&ear Cell{0}";
            this.Text = "Clear Cell";

        }
        #endregion
    }
}
