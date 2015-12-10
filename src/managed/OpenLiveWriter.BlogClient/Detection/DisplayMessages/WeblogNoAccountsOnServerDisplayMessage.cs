// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.BlogClient.Detection.DisplayMessages
{
    /// <summary>
    /// Summary description for DeleteSingleItemDisplayMessage.
    /// </summary>
    public class WeblogNoAccountsOnServerDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogNoAccountsOnServerDisplayMessage(IContainer container)
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

        public WeblogNoAccountsOnServerDisplayMessage()
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
            // WeblogNoAccountsOnServerDisplayMessage
            //
            this.Text = "A successful connection was made to your account however the server{0}reported th" +
                "at you do not currently have an active weblog. Please ensure{0}that your account" +
                " with this provider is current before proceeding.";
            this.Title = "No Weblogs Found on Server";
            this.Type = DisplayMessageType.Warning;

        }
        #endregion
    }
}
