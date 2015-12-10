// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandSavePost : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandSavePost(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandSavePost()
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
            // CommandSavePost
            //
            this.CommandBarButtonText = "Save Draft";
            this.Identifier = "OpenLiveWriter.PostEditor.SavePost";
            this.MainMenuPath = "&File@0/-&Save Local Draft@30";
            this.MenuText = "&Save Local Draft";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.Text = "Save Local Draft";

        }
        #endregion
    }
}
