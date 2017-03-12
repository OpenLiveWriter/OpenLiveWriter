// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    /// <summary>
    /// Summary description for SidebarGutter.
    /// </summary>
    public class SidebarGutter : System.Windows.Forms.UserControl
    {
        const int BLOCK_HEIGHT = 30;

        private Bitmap bitmapChevronLeft = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Sidebar.Images.ChevronLeft.png");
        private Bitmap bitmapChevronLeftHover = ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.Sidebar.Images.ChevronLeftHover.png");

        private BitmapButton buttonChevron;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SidebarGutter()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            buttonChevron = new BitmapButton(this.components);
            ColorizedResources.Instance.ColorizationChanged += new EventHandler(ColorizationChanged);
            buttonChevron.BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
            buttonChevron.BitmapEnabled = bitmapChevronLeft;
            buttonChevron.BitmapSelected = bitmapChevronLeftHover;
            buttonChevron.BitmapPushed = bitmapChevronLeftHover;
            buttonChevron.Bounds =
                RectangleHelper.Center(new Size(10, 9), new Rectangle(0, 0, ClientSize.Width, BLOCK_HEIGHT), false);
            buttonChevron.ButtonStyle = ButtonStyle.Bitmap;
            buttonChevron.ButtonText = Res.Get(StringId.ShowSidebar);
            buttonChevron.ToolTip = Res.Get(StringId.ShowSidebar);
            buttonChevron.Click += new EventHandler(buttonChevron_Click);
            buttonChevron.AccessibleName = Res.Get(StringId.ShowSidebar);
            buttonChevron.TabStop = true;
            buttonChevron.TabIndex = 0;
            buttonChevron.RightToLeft = (BidiHelper.IsRightToLeft ? RightToLeft.No : RightToLeft.Yes);
            buttonChevron.AllowMirroring = true;
            Controls.Add(buttonChevron);
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);
            Invalidate();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            ColorizedResources.Instance.ColorizationChanged -= new EventHandler(ColorizationChanged);
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
            this.components = new Container();
            //
            // SidebarGutter
            //
            this.Name = "SidebarGutter";
            this.Size = new System.Drawing.Size(12, 150);

        }
        #endregion

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            ColorizedResources colRes = ColorizedResources.Instance;
            Color blockColor = colRes.SidebarHeaderBackgroundColor;

            using (Brush b = new SolidBrush(colRes.SidebarGradientBottomColor))
                e.Graphics.FillRectangle(b, ClientRectangle);
            using (Brush b = new SolidBrush(blockColor))
                e.Graphics.FillRectangle(b, 0, 0, ClientSize.Width, BLOCK_HEIGHT);
        }

        private void buttonChevron_Click(object sender, EventArgs e)
        {
            OnClick(EventArgs.Empty);
        }

        private void ColorizationChanged(object sender, EventArgs e)
        {
            buttonChevron.BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
        }
    }
}
