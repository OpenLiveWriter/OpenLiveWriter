// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard.DisplayMessages
{

    public class SpacesHomepageUrlInvalidDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public SpacesHomepageUrlInvalidDisplayMessage(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();

        }

        public SpacesHomepageUrlInvalidDisplayMessage()
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
            // SpacesHomepageUrlInvalidDisplayMessage
            //
            this.Text = "The specified URL does not appear to be valid Spaces homepage.\r\nPlease enter a va" +
                "lid Spaces URL or set up a Windows Live Space before continuing.";
            this.Title = "Invalid Spaces URL Found";
            this.Type = OpenLiveWriter.Controls.DisplayMessageType.Warning;

        }
        #endregion
    }
}
