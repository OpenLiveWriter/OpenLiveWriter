// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandPostProperties : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandPostProperties(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandPostProperties()
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
            // CommandPostProperties
            //
            this.ContextMenuPath = "-&Properties@300";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.PostProperties";
            this.MainMenuPath = "&View@2/&Properties@150";
            this.MenuText = "&Properties";
            this.Shortcut = System.Windows.Forms.Shortcut.F2;
            this.Text = "Properties";
            this.SuppressMenuBitmap = true;
        }
        #endregion
    }
}
