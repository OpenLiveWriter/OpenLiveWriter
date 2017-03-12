// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices.UI;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    /// <summary>
    /// Summary description for SidebarControl.
    /// </summary>
    public class SidebarControl : System.Windows.Forms.UserControl, IVirtualTransparencyHost
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SidebarControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            ColorizedResources.Instance.ColorizationChanged += new EventHandler(RefreshColors);
            RefreshColors(this, EventArgs.Empty);
        }

        public event EventHandler MinimumSizeChanged;

        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                if (value != MinimumSize)
                {
                    base.MinimumSize = value;
                    if (MinimumSizeChanged != null)
                        MinimumSizeChanged(this, EventArgs.Empty);
                }
            }
        }

        private void RefreshColors(object sender, EventArgs e)
        {
            BackColor = ColorizedResources.Instance.SidebarGradientBottomColor;
        }

        public virtual bool HasStatusBar
        {
            get { return true; }
        }

        public virtual void UpdateView(object htmlSelection, bool force)
        {

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ColorizedResources.Instance.ColorizationChanged -= new EventHandler(RefreshColors);
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            ColorizedResources colRes = ColorizedResources.Instance;

            using (Brush b = new SolidBrush(colRes.SidebarGradientBottomColor))
                pevent.Graphics.FillRectangle(b, ClientRectangle);
            Rectangle gradientRect = new Rectangle(0, 0, ClientSize.Width, 65);
            using (Brush b = new LinearGradientBrush(gradientRect, colRes.SidebarGradientTopColor, colRes.SidebarGradientBottomColor, LinearGradientMode.Vertical))
                pevent.Graphics.FillRectangle(b, gradientRect);
        }

        void IVirtualTransparencyHost.Paint(PaintEventArgs args)
        {
            OnPaintBackground(args);
        }
    }
}
