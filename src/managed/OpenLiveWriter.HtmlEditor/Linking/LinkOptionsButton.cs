// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.HtmlEditor.Linking
{
    internal class LinkOptionsButton : XPBitmapButton
    {
        public LinkOptionsButton()
        {
            Bitmap buttonFace = ResourceHelper.LoadAssemblyResourceBitmap("Linking.Images.OptionsArrow.png");
            Initialize(buttonFace, buttonFace, ContentAlignment.MiddleRight);
            Text = "  " + Res.Get(StringId.LinkTo);
            AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.LinkTo));
            TextAlign = ContentAlignment.MiddleLeft;
            AccessibleRole = System.Windows.Forms.AccessibleRole.ButtonMenu;
        }
    }

}
