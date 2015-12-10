// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandUseTemplateForEditting : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandUseTemplateForEditting(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandUseTemplateForEditting()
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
            // CommandConfigureWeblog
            //
            this.ContextMenuPath = "Author in Weblog &Style@60";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.UseTemplateForEditting";
            this.MainMenuPath = "&Weblog@7/Author in Weblog &Style-@60";
            this.MenuText = "Author in Weblog &Style";

        }
        #endregion
    }
}
