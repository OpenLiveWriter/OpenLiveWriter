// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    public partial class SidebarHeaderControl : UserControl, IRtlAware
    {
        public SidebarHeaderControl()
        {
            InitializeComponent();
            BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
            labelHeading.Font = Res.GetFont(FontSize.XLarge, FontStyle.Regular);
            labelHeading.ForeColor = ColorizedResources.Instance.SidebarHeaderTextColor;
            linkLabelOptional.Font = linkLabel.Font = Res.GetFont(FontSize.Normal, FontStyle.Regular);
            linkLabelOptional.LinkColor = linkLabel.LinkColor = ColorizedResources.Instance.SidebarLinkColor;
            linkLabelOptional.LinkArea = linkLabel.LinkArea = new LinkArea(0, 0);
            linkLabelOptional.Visible = false;
            linkLabelOptional.FlatStyle = FlatStyle.System;
            linkLabel.FlatStyle = FlatStyle.System;
            linkLabel.AutoEllipsis = true;
            labelHeading.FlatStyle = FlatStyle.Standard;
            labelHeading.AutoEllipsis = true;
            labelHeading.AutoSize = false;
            linkLabelOptional.Visible = false;

            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        public void RefreshLayout()
        {
            ((IRtlAware)this).Layout();
        }

        private bool _doingLayout = false;
        void IRtlAware.Layout()
        {
            _doingLayout = true;
            SuspendLayout();
            separatorControl.Width = labelHeading.Width = linkLabelOptional.Width = linkLabel.Width = Width;
            LayoutHelper.NaturalizeHeightAndDistribute(2, labelHeading, linkLabel, linkLabelOptional);

            //linkLabel.Width = Width;

            Control lastVisibleControl = linkLabel;
            if (linkLabelOptional.Visible)
                lastVisibleControl = linkLabelOptional;

            Height = lastVisibleControl.Bottom + separatorControl.Height + SEP_TOP_PADDING + SEP_BOTTOM_PADDING;
            separatorControl.Top = lastVisibleControl.Bottom + SEP_TOP_PADDING;

            if (BidiHelper.IsRightToLeft)
            {
                labelHeading.Left = Width - labelHeading.Width;
                linkLabel.Left = Width - linkLabel.Width - 1;
                linkLabelOptional.Left = Width - linkLabelOptional.Width - 1;
            }
            else
            {
                labelHeading.Left = -1;
                linkLabel.Left = linkLabelOptional.Left = 0;
            }
            ResumeLayout();
            _doingLayout = false;
        }

        private const int SEP_TOP_PADDING = 15;
        private const int SEP_BOTTOM_PADDING = 12;

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_doingLayout)
                return;

            RefreshLayout();
            base.OnSizeChanged(e);
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string HeaderText
        {
            get
            {
                return labelHeading.Text;
            }
            set
            {
                string accessibleName = ControlHelper.ToAccessibleName(value);
                labelHeading.AccessibleName = accessibleName;
                AccessibleName = accessibleName;
                //TabStop = false;
                labelHeading.Text = StringHelper.Ellipsis(value, 100);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string LinkText
        {
            get
            {
                return linkLabel.Text;
            }
            set
            {
                linkLabel.Text = StringHelper.Ellipsis(value, 100);
                linkLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string SecondLinkText
        {
            get
            {
                return linkLabelOptional.Text;
            }
            set
            {
                linkLabelOptional.Text = StringHelper.Ellipsis(value, 100);
                linkLabelOptional.Visible = !string.IsNullOrEmpty(value);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string LinkUrl
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                linkLabel.LinkArea = new LinkArea(0, new StringInfo(LinkText).LengthInTextElements);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string SecondLinkUrl
        {
            get
            {
                return _secondUrl;
            }
            set
            {
                _secondUrl = value;
                linkLabelOptional.LinkArea = new LinkArea(0, new StringInfo(SecondLinkText).LengthInTextElements);
            }
        }

        private string _url;
        private string _secondUrl;

        private static void LaunchUrl(string url)
        {
            if (String.IsNullOrEmpty(url))
                return;

            ShellHelper.LaunchUrl(url);
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchUrl(_url);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                if (linkLabel.Focused)
                    LaunchUrl(_url);
                else if (linkLabelOptional.Focused)
                    LaunchUrl(_secondUrl);
            }
        }

        private void linkLabelOptional_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchUrl(_secondUrl);
        }

    }
}
