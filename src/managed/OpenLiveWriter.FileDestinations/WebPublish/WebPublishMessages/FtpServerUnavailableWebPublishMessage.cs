// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.FileDestinations
{
    /// <summary>
    /// Summary description for TargetPathNotFound.
    /// </summary>
    public class FtpServerUnavailableWebPublishMessage : WebPublishMessage
    {


        public FtpServerUnavailableWebPublishMessage(params object[] textFormatArgs)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            // set text format args if they were specified
            TextFormatArgs = textFormatArgs ;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // FtpServerUnavailableWebPublishMessage
            //
            this.Text = "The specified hostname does not appear to be running an FTP server";
            this.Title = "FTP Server Unavailable";

        }
        #endregion
    }
}
