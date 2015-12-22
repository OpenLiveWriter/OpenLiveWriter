// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.ApplicationFramework.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandAlignLeft : Command
    {

        public CommandAlignLeft(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandAlignLeft()
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
            // CommandAlignLeft
            //
            this.ContextMenuPath = "";
            this.Identifier = "MindShare.ApplicationCore.Commands.AlignLeft";
            this.MainMenuPath = "F&ormat@5/-&Align@300/&Left@100";
            this.Text = "Left Align";

        }
        #endregion
    }
}
