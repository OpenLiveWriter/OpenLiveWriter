// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.HtmlEditor.DisplayMessages
{
    /// <summary>
    /// Summary description for PasteSpecialInvalidDataDisplayMessage.
    /// </summary>
    public class PasteSpecialInvalidDataDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public PasteSpecialInvalidDataDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

            //
            //
            //
        }

        public PasteSpecialInvalidDataDisplayMessage()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            //
            //
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
            // PasteSpecialInvalidDataDisplayMessage
            //
            this.Buttons = System.Windows.Forms.MessageBoxButtons.OK;
            this.Text = "Paste Special can only be used with HTML and text data.";
            this.Title = "Invalid Data Selection";
            this.Type = DisplayMessageType.Information;

        }
        #endregion
    }
}
