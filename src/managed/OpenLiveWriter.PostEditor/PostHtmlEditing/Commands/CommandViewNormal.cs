// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandViewNormal : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandViewNormal(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandViewNormal()
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
            // CommandViewNormal
            //
            this.ContextMenuPath = "&Normal@100";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewNormal";
            this.MainMenuPath = "&View@2/&Normal@100";
            this.MenuText = "&Normal";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlF11 ;
            this.Text = "Normal View";
            this.SuppressMenuBitmap = true;
        }
        #endregion
    }
}
