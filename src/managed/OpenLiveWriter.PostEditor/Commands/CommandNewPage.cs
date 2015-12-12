// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{

    public class CommandNewPage : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandNewPage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandNewPage()
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
            // CommandNewPost
            //
            this.CommandBarButtonText = "New";
            this.Identifier = "OpenLiveWriter.PostEditor.NewPage";
            this.MainMenuPath = "&File@0/New &Page@12";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlG;
            this.MenuText = "New &Page";
            this.Text = "New Page";

        }
        #endregion
    }
}
