// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework ;

namespace OpenLiveWriter.PostEditor.Tables.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandInsertColumnRight : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandInsertColumnRight(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandInsertColumnRight()
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
            this.Identifier = "OpenLiveWriter.PostEditor.Tables.Commands.InsertColumnRight";
            this.MainMenuPath = "T&able@6/Insert Column Ri&ght@255";
            this.MenuText = "Insert Column Ri&ght" ;
            this.Text = "Insert Column Right";

        }
        #endregion
    }
}
