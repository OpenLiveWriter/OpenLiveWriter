// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandViewImageProperties : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandViewImageProperties(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandViewImageProperties()
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
            // CommandViewImageProperties
            //
            this.ContextMenuPath = "P&roperties@310";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ImageProperties";
            this.MainMenuPath = "&View@2/P&roperties@310";
            this.MenuText = "Show P&roperties";
            this.Text = "Properties";

        }
        #endregion
    }
}
