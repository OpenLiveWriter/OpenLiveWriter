// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.HtmlEditor.DisplayMessages
{

    public class NotLinkableDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public NotLinkableDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public NotLinkableDisplayMessage()
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
            // NotLinkableDisplayMessage
            //
            this.Type = DisplayMessageType.Information;
            this.Buttons = MessageBoxButtons.OK;
            this.Text = "Your selection cannot be hyperlinked. Please select text, or click on an image.";
            this.Title = "Cannot Add Link to Selection";

        }
        #endregion
    }
}
