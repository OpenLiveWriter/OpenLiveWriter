// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    public class BrowseButton : Button
    {
        private IContainer components = null;

        public BrowseButton()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        public BrowseButton(IContainer container) : base()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // BrowseButton
            //
            this.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Size = new System.Drawing.Size(22, 21);
            this.Text = "...";

        }
        #endregion
    }
}

