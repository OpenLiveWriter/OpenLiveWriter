// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard.DisplayMessages
{

    public class ErrorConnectingPromptContinueDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ErrorConnectingPromptContinueDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public ErrorConnectingPromptContinueDisplayMessage()
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
            // MappingVerificationFailedDisplayMessage
            //
            this.Buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
            this.DefaultButton = System.Windows.Forms.MessageBoxDefaultButton.Button2;
            this.Text = "The remote server could not be contacted to verify the settings. Do you want to continue anyway?";
            this.Title = "Error Connecting";
            this.Type = OpenLiveWriter.Controls.DisplayMessageType.Question;

        }
        #endregion
    }
}
