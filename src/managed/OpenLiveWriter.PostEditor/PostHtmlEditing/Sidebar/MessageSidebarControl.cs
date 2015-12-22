// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    public class MessageSidebarControl : SidebarControl
    {
        LabelControl labelControl;
        public MessageSidebarControl(string messageText)
        {
            labelControl = new LabelControl();
            labelControl.BackColor = Color.Transparent;
            labelControl.Text = messageText;
            Controls.Add(labelControl);
        }

        private const int WIDTH_PADDING = 8;
        protected override void OnLayout(LayoutEventArgs levent)
        {
            labelControl.Location = new Point(WIDTH_PADDING, 8);
            labelControl.Width = Width - WIDTH_PADDING * 2;
            labelControl.Height = Height;
            base.OnLayout(levent);
        }
    }
}
