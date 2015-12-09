// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class FontFamilyTextStyle : TextStyle, IEquatable<FontFamilyTextStyle>
    {
        public FontFamilyTextStyle(string fontFamily)
        {
            if (fontFamily == null)
            {
                throw new ArgumentNullException("fontFamily");
            }

            FontFamily = fontFamily;
        }

        public string FontFamily { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            if (!commands[IDM.FONTNAME].Enabled)
            {
                return;
            }

            commands[IDM.FONTNAME].Execute(FontFamily);
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as FontFamilyTextStyle);
        }

        public bool Equals(FontFamilyTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return FontFamily.Equals(obj.FontFamily, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return FontFamily.GetHashCode();
        }
    }
}
