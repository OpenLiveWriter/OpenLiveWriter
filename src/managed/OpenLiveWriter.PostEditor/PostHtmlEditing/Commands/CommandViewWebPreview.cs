// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandViewWebPreview : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandViewWebPreview(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandViewWebPreview()
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
            // CommandViewPreview
            //
            this.ContextMenuPath = "&Web Preview@100";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewPreview";
            this.MainMenuPath = "&View@2/&Web Preview@103";
            this.MenuText = "&Web Preview";
            this.Shortcut = System.Windows.Forms.Shortcut.F12 ;
            this.Text = "Web Preview";

        }
        #endregion
    }
}
