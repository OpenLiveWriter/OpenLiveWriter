// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class UnderlineTextStyle : TextStyle, IEquatable<UnderlineTextStyle>
    {
        public UnderlineTextStyle(bool underline)
        {
            Underline = underline;
        }

        public bool Underline { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            // Special case because MSHTML is not able remove underlines from <a> elements.
            if (!Underline)
            {
                // Find the parent <a> element (if one exists).
                IHTMLElement currentElement = markupRange.Start.CurrentScope;
                while (currentElement != null && !(currentElement is IHTMLAnchorElement))
                {
                    currentElement = currentElement.parentElement;
                }

                if (currentElement != null)
                {
                    // Explicitly remove the underline.
                    const string noTextDecorationAttribute = "style=\"text-decoration: none\"";
                    ElementFactory factory = () => markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_FONT, noTextDecorationAttribute);
                    WrapInElement(factory, markupServices, markupRange);

                    return;
                }
            }

            if (!commands[IDM.UNDERLINE].Enabled)
            {
                return;
            }

            commands[IDM.UNDERLINE].Execute();

            Debug.Assert(commands[IDM.UNDERLINE].Latched == Underline, "UnderlineTextStyle did not yield the correct results.");
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as UnderlineTextStyle);
        }

        public bool Equals(UnderlineTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return Underline.Equals(obj.Underline);
        }

        public override int GetHashCode()
        {
            return Underline.GetHashCode();
        }
    }

}
