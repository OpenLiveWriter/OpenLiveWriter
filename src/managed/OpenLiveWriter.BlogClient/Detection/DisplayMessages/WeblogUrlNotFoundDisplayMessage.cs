// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.BlogClient.Detection.DisplayMessages
{
    /// <summary>
    /// Summary description for DeleteSingleItemDisplayMessage.
    /// </summary>
    public class WeblogUrlNotFoundDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogUrlNotFoundDisplayMessage(IContainer container)
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

        public WeblogUrlNotFoundDisplayMessage()
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
            // WeblogPostUrlNotFoundDisplayMessage
            //
            this.Text = "The server has reported that the following URL could not be found:{0}{1}{0}Pleas" +
                "e ensure that you have specified a valid and reachable URL.";
            this.Title = "Weblog URL Not Found";
            this.Type = DisplayMessageType.Warning;

        }
        #endregion
    }
}
