// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices.Diagnostics
{

    public class UnhandledExceptionErrorMessage : UnexpectedErrorMessage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public UnhandledExceptionErrorMessage()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // UnhandledExceptionErrorMessage
            //
            this.Text = Res.Get(StringId.UnhandledExceptionErrorMessage);
            this.Title = Res.Get(StringId.UnhandledExceptionErrorTitle);

        }
        #endregion
    }
}
