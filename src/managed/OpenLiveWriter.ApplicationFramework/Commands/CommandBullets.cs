// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandBullets : Command
    {

        public CommandBullets(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandBullets()
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
            // CommandBullets
            //
            this.ContextMenuPath = "";
            this.Identifier = "MindShare.ApplicationCore.Commands.Bullets";
            this.MainMenuPath = "F&ormat@5/&Bullets@402";
            this.Text = "Bullets";
            this.SuppressMenuBitmap = true;
        }
        #endregion
    }
}
