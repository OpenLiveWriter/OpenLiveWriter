// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandUpdateWeblogStyle : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandUpdateWeblogStyle(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandUpdateWeblogStyle()
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
            this.ContextMenuPath = "&Update Weblog Style@57";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.UpdateWeblogStyle";
            this.MainMenuPath = "&View@2/-&Update Weblog Style-@120";
            this.MenuText = "&Update Weblog Style";

        }
        #endregion
    }
}
