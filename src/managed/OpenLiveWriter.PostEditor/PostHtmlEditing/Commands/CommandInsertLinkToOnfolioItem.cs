// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Commands
{
    /// <summary>
    /// Summary description for CommandInsertLinkToOnfolioItem.
    /// </summary>
    public class CommandInsertLinkToOnfolioItem : Command
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CommandInsertLinkToOnfolioItem(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public CommandInsertLinkToOnfolioItem()
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
            // CommandInsertLinkToOnfolioItem
            //
            this.ContextMenuPath = "";
            this.Identifier = "OpenLiveWriter.PostEditor.PostHtmlEditing.Commands.InsertLinkToOnfolioItem";
            this.MainMenuPath = "&Insert@4/Link to &Onfolio Item...@200";
            this.Shortcut = System.Windows.Forms.Shortcut.CtrlL;
            this.Text = "Insert Link to Onfolio Item";

        }
        #endregion
    }
}
