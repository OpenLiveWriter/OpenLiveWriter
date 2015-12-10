// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Summary description for CommandOpenRecentPosts.
    /// </summary>
    public class CommandOpenRecentPosts : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public CommandOpenRecentPosts(System.ComponentModel.IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public CommandOpenRecentPosts()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Identifier = "OpenLiveWriter.PostEditor.OpenRecentPosts";
        }
        #endregion
    }
}
