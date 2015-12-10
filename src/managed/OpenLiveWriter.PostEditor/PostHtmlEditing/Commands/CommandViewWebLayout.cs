// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandViewWebLayout : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandViewWebLayout(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandViewWebLayout()
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
            // CommandViewWebpage
            //
            this.ContextMenuPath = "";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewWebLayout";
            this.MainMenuPath = "&View@2/Web &Layout@101";
            this.MenuText = "Web &Layout";
            this.Shortcut = System.Windows.Forms.Shortcut.F11 ;
            this.Text = "Web Layout";

        }
        #endregion
    }
}
