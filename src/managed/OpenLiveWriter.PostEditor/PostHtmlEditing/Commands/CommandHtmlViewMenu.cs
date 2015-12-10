// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandHtmlViewMenu : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandHtmlViewMenu(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandHtmlViewMenu()
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
            // CommandHtmlViewMenu
            //
            this.CommandBarButtonText = "View";
            this.Identifier = "OpenLiveWriter.PostEditor.Commands.PostHtmlEditing.HtmlViewMenu";
            this.MenuText = "View";
            this.Text = "Change View Mode";

        }
        #endregion
    }
}
