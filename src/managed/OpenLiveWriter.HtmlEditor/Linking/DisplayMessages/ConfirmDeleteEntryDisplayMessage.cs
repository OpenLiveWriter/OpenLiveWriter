// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.HtmlEditor.Linking.DisplayMessages
{
    /// <summary>
    /// Summary description for ConfirmDeleteEntryDisplayMessage.
    /// </summary>
    public class ConfirmDeleteEntryDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ConfirmDeleteEntryDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public ConfirmDeleteEntryDisplayMessage()
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
            // ConfirmDeleteEntryDisplayMessage
            //
            this.Buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
            this.Text = "Are you sure that you want to delete the selected glossary entry?";
            this.Title = "Confirm Delete";
            this.Type = DisplayMessageType.Question;

        }
        #endregion
    }
}
