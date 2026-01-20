// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for WorkspaceColumnPaneEmptyControl.
    /// </summary>
    public class WorkspaceColumnPaneEmptyControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WorkspaceColumnPaneEmptyControl()
        {
            // This call is required by the Windows.Forms Form Designer.
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
            // WorkspaceColumnPaneEmptyControl
            //
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Name = "WorkspaceColumnPaneEmptyControl";

        }
        #endregion
    }
}
