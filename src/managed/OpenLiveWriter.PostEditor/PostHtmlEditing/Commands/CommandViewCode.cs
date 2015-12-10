// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandClose.
    /// </summary>
    public class CommandViewCode : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandViewCode(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
        }

        public CommandViewCode()
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
            // CommandViewCode
            //
            this.ContextMenuPath = "&HTML Code@105";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.ViewCode";
            this.MainMenuPath = "&View@2/&HTML Code@105";
            this.MenuText = "&HTML Code";
            this.Shortcut = System.Windows.Forms.Shortcut.ShiftF11 ;
            this.Text = "HTML Code View";
            this.SuppressMenuBitmap = true;

        }
        #endregion
    }
}
