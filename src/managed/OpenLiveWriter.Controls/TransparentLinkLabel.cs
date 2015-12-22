// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    public class TransparentLinkLabel : LinkLabel
    {
        public TransparentLinkLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent ;
            FlatStyle = FlatStyle.System ;
        }
    }
}
