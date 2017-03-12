// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    public class MiniTabsControl : LightweightControlContainerControl
    {
        public class SelectedTabChangedEventArgs
        {
            public readonly int SelectedTabIndex;

            public SelectedTabChangedEventArgs(int selectedTabIndex)
            {
                SelectedTabIndex = selectedTabIndex;
            }
        }

        public delegate void SelectedTabChangedEventHandler(object sender, SelectedTabChangedEventArgs args);

        private MiniTab[] tabs = new MiniTab[0];
        private MiniTabContext ctx;
        private int indent = 6;
        private const int PADDING = 5;
        private Color topBorderColor = Color.Transparent;

        public MiniTabsControl()
        {
            TabStop = false;
            ctx = new MiniTabContext(this);

            InitFocusAndAccessibility();
        }

        private void InitFocusAndAccessibility()
        {
            InitFocusManager();
        }

        public event SelectedTabChangedEventHandler SelectedTabChanged;

        public int Indent
        {
            get { return indent; }
            set
            {
                if (indent != value)
                {
                    indent = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        public string[] TabNames
        {
            set
            {
                SuspendLayout();
                try
                {
                    foreach (MiniTab tab in tabs)
                    {
                        tab.SelectedChanged -= MiniTab_SelectedChanged;
                        tab.LightweightControlContainerControl = null;
                        tab.Dispose();
                    }

                    tabs = new MiniTab[value.Length];
                    for (int i = value.Length - 1; i >= 0; i--)
                    {
                        tabs[i] = new MiniTab(ctx);
                        if (i == 0)
                            tabs[i].Select();

                        tabs[i].AccessibleName = value[i];
                        tabs[i].LightweightControlContainerControl = this;
                        tabs[i].Text = value[i];
                        tabs[i].SelectedChanged += MiniTab_SelectedChanged;
                        tabs[i].MouseDown += MiniTabsControl_MouseDown;
                    }

                    ClearFocusableControls();
                    AddFocusableControls(tabs);
                }
                finally
                {
                    ResumeLayout(true);
                }
            }
        }

        public void SetToolTip(int tabNum, string toolTipText)
        {
            tabs[tabNum].ToolTip = toolTipText;
        }

        public Color TopBorderColor
        {
            get { return topBorderColor; }
            set
            {
                topBorderColor = value;
                Invalidate();
            }
        }

        public bool DrawShadow
        {
            get
            {
                return _drawShadow;
            }
            set
            {
                _drawShadow = value;
            }
        }

        private bool _drawShadow;

        public int ShadowWidth
        {
            get
            {
                if (_shadowWidth == -1)
                    _shadowWidth = Width;
                return _shadowWidth;
            }
            set
            {
                _shadowWidth = value;
                Update();
            }
        }

        private int _shadowWidth = -1;

        private void MiniTabsControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ((MiniTab)sender).Select();
        }

        void MiniTab_SelectedChanged(object sender, EventArgs e)
        {
            if (!((MiniTab)sender).Selected)
                return;

            int selectedIndex = -1;
            for (int i = tabs.Length - 1; i >= 0; i--)
            {
                MiniTab tab = tabs[i];
                if (!ReferenceEquals(sender, tab))
                {
                    tab.Unselect();
                    tab.BringToFront();
                }
                else
                {
                    this.AccessibleName = tabs[i].AccessibleName;
                    selectedIndex = i;
                }
            }

            ((MiniTab)sender).BringToFront();

            PerformLayout();
            Invalidate();

            if (selectedIndex >= 0 && SelectedTabChanged != null)
                SelectedTabChanged(this, new SelectedTabChangedEventArgs(selectedIndex));
        }

        protected override void OnLayout(System.Windows.Forms.LayoutEventArgs e)
        {
            base.OnLayout(e);

            int height = 0;

            int x = indent;
            foreach (MiniTab tab in tabs)
            {
                tab.VirtualSize = tab.DefaultVirtualSize;
                tab.VirtualLocation = new Point(x, 0);
                x += tab.VirtualWidth - 1;
                height = Math.Max(height, tab.VirtualBounds.Bottom);
            }

            Height = height + PADDING;

            RtlLayoutFixupLightweightControls(true);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (topBorderColor != Color.Transparent)
            {
                using (Pen p = new Pen(topBorderColor))
                    e.Graphics.DrawLine(p, 0, 0, Width, 0);
            }

            if (DrawShadow)
            {
                BidiGraphics g = new BidiGraphics(e.Graphics, e.ClipRectangle);
                GraphicsHelper.TileFillScaledImageHorizontally(g, ColorizedResources.Instance.DropShadowBitmap, new Rectangle(0, 0, ShadowWidth, ColorizedResources.Instance.DropShadowBitmap.Height));
            }

            base.OnPaint(e);
        }

        public void SelectTab(int i)
        {
            tabs[i].Select();
        }
    }
}
