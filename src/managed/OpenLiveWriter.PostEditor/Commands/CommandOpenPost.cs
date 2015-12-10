// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandOpenPost : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandOpenPost(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandOpenPost()
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
            // CommandOpenPost
            //
            this.CommandBarButtonText = "Open";
            this.Identifier = "OpenLiveWriter.PostEditor.OpenPost";
            this.MainMenuPath = "&File@0/&Open...@15";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.Text = "Open";

        }
        #endregion
    }
}
