// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandImageSaveDefaults : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandImageSaveDefaults(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        public CommandImageSaveDefaults()
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
            // CommandImageSaveDefaults
            //
            this.ContextMenuPath = "";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.ImageSaveDefaults";
            this.MainMenuPath = "";
            this.Text = "Save image settings as defaults";
            this.VisibleOnMainMenu = false;

        }
        #endregion
    }
}
