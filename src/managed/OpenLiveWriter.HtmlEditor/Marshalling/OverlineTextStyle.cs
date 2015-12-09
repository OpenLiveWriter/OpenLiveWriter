// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class OverlineTextStyle : TextStyle, IEquatable<OverlineTextStyle>
    {
        public OverlineTextStyle(bool overline)
        {
            Overline = overline;
        }

        public bool Overline { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            const string overlineAttribute = "style=\"text-decoration: overline\"";
            ElementFactory factory = () => markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_FONT, overlineAttribute);
            WrapInElement(factory, markupServices, markupRange);
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as OverlineTextStyle);
        }

        public bool Equals(OverlineTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return Overline.Equals(obj.Overline);
        }

        public override int GetHashCode()
        {
            return Overline.GetHashCode();
        }
    }

}
