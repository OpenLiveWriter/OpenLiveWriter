// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.DisplayMessages
{
    /// <summary>
    /// Summary description for DeleteSingleItemDisplayMessage.
    /// </summary>
    public class QueryForUnpostedEntryDisplayMessage : DisplayMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public QueryForUnpostedEntryDisplayMessage(IContainer container)
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

        public QueryForUnpostedEntryDisplayMessage()
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
            // QueryForUnpostedEntryDisplayMessage
            //
            this.Buttons = System.Windows.Forms.MessageBoxButtons.YesNoCancel;
            this.Text = "The weblog entry you are editing has not been posted.{0}{0}Do you want to post a " +
                "draft of the entry now?";
            this.Title = "Post Entry as Draft";
            this.Type = DisplayMessageType.Question;

        }
        #endregion
    }
}
