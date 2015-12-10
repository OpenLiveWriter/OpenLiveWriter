// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework ;

namespace OpenLiveWriter.PostEditor.Tables.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandMoveRowUp : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandMoveRowUp(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandMoveRowUp()
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
            // CommandFont
            //
            this.Identifier = "OpenLiveWriter.PostEditor.Tables.Commands.MoveRowUp";
            this.MainMenuPath = "T&able@6/Move Row &Up@210";
            this.MenuText = "Move Row &Up" ;
            this.Text = "Move Row Up";
        }
        #endregion
    }
}
