// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class FontColorTextStyle : TextStyle, IEquatable<FontColorTextStyle>
    {
        public FontColorTextStyle(Color fontColor)
        {
            FontColor = fontColor;
        }

        public Color FontColor { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            if (!commands[IDM.FORECOLOR].Enabled)
            {
                return;
            }

            commands[IDM.FORECOLOR].Execute(ColorHelper.ColorToString(FontColor));
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as FontColorTextStyle);
        }

        public bool Equals(FontColorTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return FontColor.Equals(obj.FontColor);
        }

        public override int GetHashCode()
        {
            return FontColor.GetHashCode();
        }
    }

}
