// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandCopy.
    /// </summary>
    public class CommandInsertExtendedEntry : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandInsertExtendedEntry(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();
        }

        public CommandInsertExtendedEntry()
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
            // CommandInsertExtendedEntry
            //
            this.ContextMenuPath = "";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.InsertExtendedEntry";
            this.MainMenuPath = "F&ormat@5/-&Split Post@600";
            this.MenuText = "Split Post";
            this.Text = "Split Post";

        }
        #endregion

    }
}
