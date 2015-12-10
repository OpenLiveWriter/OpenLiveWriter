// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardPanelAutoDetection.
    /// </summary>
    public class WeblogConfigurationWizardPanelAutoDetection : WeblogConfigurationWizardPanelProgress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WeblogConfigurationWizardPanelAutoDetection()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            Init(2000, AnimationBitmaps);

            // force control creation so that we are guaranteed to be
            // able to provide a synchronization context
            CreateControl();
        }

        public Control BrowserParentControl
        {
            get
            {
                return panelBrowserParent;
            }
        }

        private System.Windows.Forms.Panel panelBrowserParent;

        // TODO: replace with auto-detection animation
        private Bitmap[] AnimationBitmaps
        {
            get
            {
                if (_animationBitmaps == null)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 1; i <= 26; i++)
                    {
                        string resourceName = String.Format(CultureInfo.InvariantCulture, "Images.PublishAnimation.post{0:00}.png", i);
                        list.Add(ResourceHelper.LoadAssemblyResourceBitmap(resourceName));
                    }
                    _animationBitmaps = (Bitmap[])list.ToArray(typeof(Bitmap));
                }
                return _animationBitmaps;
            }
        }
        private Bitmap[] _animationBitmaps;

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
            this.panelBrowserParent = new System.Windows.Forms.Panel();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.panelBrowserParent);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(368, 232);
            this.panelMain.Controls.SetChildIndex(this.panelBrowserParent, 0);
            //
            // panelBrowserParent
            //
            this.panelBrowserParent.Location = new System.Drawing.Point(72, 200);
            this.panelBrowserParent.Name = "panelBrowserParent";
            this.panelBrowserParent.Size = new System.Drawing.Size(200, 24);
            this.panelBrowserParent.TabIndex = 6;
            this.panelBrowserParent.Visible = false;
            //
            // WeblogConfigurationWizardPanelAutoDetection
            //
            this.Name = "WeblogConfigurationWizardPanelAutoDetection";
            this.Size = new System.Drawing.Size(432, 264);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
