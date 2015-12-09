// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace OpenLiveWriter.Controls
{
    public class NewFolderButton : BitmapButton
    {
        private IContainer components = null;

        public NewFolderButton()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        public NewFolderButton(IContainer container) : base(container)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewFolderButton));
            //
            // NewFolderButton
            //
            // this.AutoSizeHeight = true;
            // this.AutoSizeWidth = true;
            this.BitmapDisabled = ((System.Drawing.Bitmap)(resources.GetObject("$this.BitmapDisabled")));
            this.BitmapEnabled = ((System.Drawing.Bitmap)(resources.GetObject("$this.BitmapEnabled")));
            this.BitmapSelected = ((System.Drawing.Bitmap)(resources.GetObject("$this.BitmapSelected")));
            this.Name = "NewFolderButton";
            this.Size = new System.Drawing.Size(22, 17);
            this.ToolTip = "Create a new folder";

        }
        #endregion
    }
}

