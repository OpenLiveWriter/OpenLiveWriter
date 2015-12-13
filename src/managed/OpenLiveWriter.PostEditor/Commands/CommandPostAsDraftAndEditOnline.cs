// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandPostAsDraftAndEditOnline : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandPostAsDraftAndEditOnline(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandPostAsDraftAndEditOnline()
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
            // CommandPostAsDraft
            //
            this.ContextMenuPath = "Post Draft and &Edit Online@102";
            this.Identifier = "OpenLiveWriter.PostAsDraftAndEditOnline";
            this.MainMenuPath = "&File@0/Post &Draft and &Edit Online@36";
            this.MenuText = "Post Draft and &Edit Online";
            this.Text = "Post Draft and Edit Online";

        }
        #endregion
    }
}
