// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandAddWeblog : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandAddWeblog(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandAddWeblog()
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
            // CommandViewWeblog
            //
            this.ContextMenuPath = "&Add Weblog Account...@2000";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.AddWeblog";
            this.MainMenuPath = "&Weblog@7/-&Add Weblog Account...@2000";
            this.MenuText = "&Add Weblog Account...";
            this.Text = "Add Weblog Account...";

        }
        #endregion
    }
}
