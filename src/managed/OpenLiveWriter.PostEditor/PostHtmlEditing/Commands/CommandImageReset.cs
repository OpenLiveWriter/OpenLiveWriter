// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandImageReset : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandImageReset(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        public CommandImageReset()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // CommandImageReset
            //
            this.ContextMenuPath = "";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.ImageReset";
            this.MainMenuPath = "";
            this.Text = "Reset image";
            this.VisibleOnMainMenu = false;

        }
        #endregion
    }
}
