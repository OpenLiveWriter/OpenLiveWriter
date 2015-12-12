// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandInsertOnfolioItem.
    /// </summary>
    public class CommandInsertOnfolioItem : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandInsertOnfolioItem(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public CommandInsertOnfolioItem()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // CommandInsertOnfolioItem
            //
            this.ContextMenuPath = "";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.InsertOnfolioItem";
            this.MainMenuPath = "&Insert@4/Onfolio &Item...@150";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlJ;
            this.Text = "Insert Onfolio Item";

        }
        #endregion
    }
}

