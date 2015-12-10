// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandAlignCenter : Command
    {

        public CommandAlignCenter(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandAlignCenter()
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
            // CommandAlignCenter
            //
            this.ContextMenuPath = "";
            this.Identifier = "MindShare.ApplicationCore.Commands.AlignCenter";
            this.MainMenuPath = "F&ormat@5/-&Align@300/&Center@101";
            this.Text = "Center Align";

        }
        #endregion
    }
}
