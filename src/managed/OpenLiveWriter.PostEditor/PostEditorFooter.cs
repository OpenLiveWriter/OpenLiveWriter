// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor
{
    public partial class PostEditorFooter : UserControl, IStatusBar
    {
        private const int MIN_STATUS_MESSAGE_WIDTH = 50;
        private readonly List<ViewSwitchTabControl> tabs = new List<ViewSwitchTabControl>();
        private readonly Stack<string> statusStack = new Stack<string>();
        private string defaultStatus;
        private readonly Image imgBg;

        public PostEditorFooter()
        {
            imgBg = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusBackground.png");

            SetStyle(
                ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
                true);

            SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();
            tableLayoutPanel1.BackColor = Color.Transparent;
            ManageVisibility();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!SystemInformation.HighContrast)
            {
                Rectangle fadeRect = new Rectangle(0, tableLayoutPanel1.Top + 1, ClientSize.Width, imgBg.Height);
                GraphicsHelper.TileFillUnscaledImageHorizontally(e.Graphics, imgBg, fadeRect);
                using (Brush b = new SolidBrush(new HCColor(229, 238, 248, SystemColors.Window)))
                    e.Graphics.FillRectangle(b, 0, fadeRect.Bottom, ClientSize.Width, ClientSize.Height - fadeRect.Bottom);
            }
            else
            {
                e.Graphics.Clear(SystemColors.Window);
            }

            HCColor startColor = new HCColor(0xA5A5A5, SystemColors.WindowFrame);
            HCColor endColor = new HCColor(0xC1CEDE, SystemColors.WindowFrame);
            if (BidiHelper.IsRightToLeft)
                Swap(ref startColor, ref endColor);

            const int GRADIENT_WIDTH = 350;
            int gradientStart = !BidiHelper.IsRightToLeft ? flowLayoutPanel.Right : flowLayoutPanel.Left - GRADIENT_WIDTH;
            int gradientEnd = !BidiHelper.IsRightToLeft ? flowLayoutPanel.Right + GRADIENT_WIDTH : flowLayoutPanel.Left;
            DrawGradientLine(e.Graphics, startColor, endColor, 0, tableLayoutPanel1.Top, ClientSize.Width, gradientStart, gradientEnd);
        }

        private static void DrawGradientLine(Graphics g, Color leftColor, Color rightColor, int left, int top, int right, int startGradient, int endGradient)
        {
            if (startGradient > left)
                using (Pen p = new Pen(leftColor))
                    g.DrawLine(p, left, top, startGradient, top);

            if (endGradient < right)
                using (Pen p = new Pen(rightColor))
                    g.DrawLine(p, endGradient, top, right, top);

            if (startGradient < right && endGradient > left)
            {
                using (Brush b = new LinearGradientBrush(
                    new Rectangle(startGradient, top, endGradient - startGradient, 1),
                    leftColor, rightColor, LinearGradientMode.Horizontal))
                {
                    using (Pen p = new Pen(b))
                    {
                        g.DrawLine(p, startGradient, top, endGradient, top);
                    }
                }
            }
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public string[] TabNames
        {
            set
            {
                flowLayoutPanel.Controls.Clear();
                tabs.Clear();
                int i = 0;
                foreach (string name in value)
                {
                    i++;
                    var tab = new ViewSwitchTabControl();
                    tab.AccessibleName = name;
                    tab.AccessibleRole = AccessibleRole.PageTab;
                    tab.Text = name;
                    tab.TabStop = true;
                    tab.TabIndex = i;
                    tab.Margin = new Padding(0);
                    tab.Click += tab_Click;
                    tab.SelectedChanged += tab_SelectedChanged;
                    flowLayoutPanel.Controls.Add(tab);
                    tabs.Add(tab);
                }
            }
        }

        public string[] Shortcuts
        {
            set
            {
                for (int i = 0; i < value.Length; i++)
                    tabs[i].Shortcut = value[i];
            }
        }

        void tab_SelectedChanged(object sender, EventArgs e)
        {
            if (!((ViewSwitchTabControl)sender).Selected)
                return;

            int selectedIndex = tabs.IndexOf((ViewSwitchTabControl)sender);

            for (int i = 0; i < tabs.Count; i++)
            {
                ViewSwitchTabControl tab = tabs[i];
                if (i != selectedIndex)
                    tab.Selected = false;
            }

            foreach (ViewSwitchTabControl tab in tabs)
                tab.Refresh();
        }

        void tab_Click(object sender, EventArgs e)
        {
            int index = tabs.IndexOf((ViewSwitchTabControl)sender);
            SelectTab(index);
        }

        public int SelectedTabIndex
        {
            get
            {
                for (int i = 0; i < tabs.Count; i++)
                    if (tabs[i].Selected)
                        return i;
                return -1;
            }
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= tabs.Count)
                throw new IndexOutOfRangeException();

            for (int i = 0; i < tabs.Count; i++)
                tabs[i].Selected = (index == i);

            if (SelectedTabChanged != null)
                SelectedTabChanged(this, EventArgs.Empty);
        }

        public event EventHandler SelectedTabChanged;

        public void SetWordCountMessage(string msg)
        {
            labelWordCount.Text = msg ?? "";
            ManageVisibility();
        }

        public void PushStatusMessage(string msg)
        {
            statusStack.Push(msg);
            UpdateStatusMessage();
        }

        public void PopStatusMessage()
        {
            Debug.Assert(statusStack.Count > 0);
            if (statusStack.Count > 0)
                statusStack.Pop();
            UpdateStatusMessage();
        }

        public void SetStatusMessage(string msg)
        {
            defaultStatus = msg;
            UpdateStatusMessage();
        }

        private void UpdateStatusMessage()
        {
            string msg = null;
            foreach (string str in statusStack)
            {
                if (str != null)
                {
                    msg = str;
                    break;
                }
            }

            if (msg == null)
                msg = defaultStatus;

            labelStatus.Text = msg ?? "";
            ManageVisibility();
        }

        private void ManageVisibility()
        {
            tableLayoutPanel1.SuspendLayout();

            labelWordCount.Visible = labelWordCount.Text.Length > 0;
            labelStatus.Visible = labelStatus.Text.Length > 0;
            labelSeparator.Visible = labelStatus.Visible && labelWordCount.Visible;

            // Set a minimum width so the columns in the TableLayoutPanel don't overlap.
            int tableMinimumWidth = tableLayoutPanel1.Padding.Horizontal + tableLayoutPanel1.Margin.Horizontal;
            // FIXME: Kludge to fix height of footer on Hi-DPI, for some reason the height becomes double what it should be.
            // This will always set to the height of the flow layout panel, which works assuming the status text is taller than the tabs.
            // A proper fix would involve finding where the mysterious extra height/padding comes from on hi-dpi.
            int tableMinimumHeight = flowLayoutPanel.Height;

            if (flowLayoutPanel.Visible)
                tableMinimumWidth += flowLayoutPanel.Padding.Horizontal + flowLayoutPanel.Margin.Horizontal + flowLayoutPanel.Width;

            if (labelWordCount.Visible)
                tableMinimumWidth += labelWordCount.Padding.Horizontal + labelWordCount.Margin.Horizontal + labelWordCount.Width;

            if (labelSeparator.Visible)
                tableMinimumWidth += labelSeparator.Padding.Horizontal + labelSeparator.Margin.Horizontal + labelSeparator.Width;

            if (labelStatus.Visible)
            {
                tableMinimumWidth += labelStatus.Padding.Horizontal + labelStatus.Margin.Horizontal;

                int maximumWidth = Math.Max(Width - tableMinimumWidth, MIN_STATUS_MESSAGE_WIDTH);
                tableMinimumWidth += FitOrAutoEllipsisLabel(labelStatus, maximumWidth);
            }

            tableLayoutPanel1.MinimumSize = new Size(tableMinimumWidth, tableMinimumHeight);

            tableLayoutPanel1.ResumeLayout(true);
        }

        /// <param name="label">The label to try and fit in the remaining width.</param>
        /// <param name="maximumWidth">The maximum width the label can be.</param>
        /// <returns>The width of the label after fitting or auto-ellipsising it.</returns>
        private int FitOrAutoEllipsisLabel(Label label, int maximumWidth)
        {
            if (label.PreferredWidth > maximumWidth)
            {
                label.AutoEllipsis = true;
                label.AutoSize = false;
                label.Width = maximumWidth;
                label.Height = label.PreferredHeight;
                return maximumWidth;
            }
            else
            {
                label.AutoEllipsis = false;
                label.AutoSize = true;
                return label.PreferredWidth;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            ManageVisibility();
        }
    }
}
