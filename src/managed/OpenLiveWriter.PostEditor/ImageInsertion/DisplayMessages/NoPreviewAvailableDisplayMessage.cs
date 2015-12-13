// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.ImageInsertion.DisplayMessages
{
    /// <summary>
    /// Summary description for NoPreviewAvailableDisplayMessage.
    /// </summary>
    public class NoPreviewAvailableDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public NoPreviewAvailableDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public NoPreviewAvailableDisplayMessage()
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
            // NoPreviewAvailableDisplayMessage
            //
            this.Buttons = System.Windows.Forms.MessageBoxButtons.OK;
            this.Text = "Preview is currently unavailable. Please check the entered URL and your network connection.";
            this.Title = "Preview Unavailable";
            this.Type = DisplayMessageType.Information;

        }
        #endregion
    }
}
