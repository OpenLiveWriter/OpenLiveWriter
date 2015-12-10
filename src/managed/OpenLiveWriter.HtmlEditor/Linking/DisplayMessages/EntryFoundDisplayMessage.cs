// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.HtmlEditor.Linking.DisplayMessages
{
    /// <summary>
    /// Summary description for EntryFoundDisplayMessage.
    /// </summary>
    public class EntryFoundDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public EntryFoundDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public EntryFoundDisplayMessage()
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
            // EntryFoundDisplayMessage
            //
            this.Buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
            this.Text = "Found a matching entry with link text {0} and URL {1}. \r\n\r\nDo you want to use this link information?";
            this.Title = "Entry Found";
            this.Type = DisplayMessageType.Question;

        }
        #endregion
    }
}
