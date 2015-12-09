// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class SubScriptTextStyle : TextStyle, IEquatable<SubScriptTextStyle>
    {
        public SubScriptTextStyle(bool subScript)
        {
            SubScript = subScript;
        }

        public bool SubScript { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            ElementFactory factory = () => markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_SUB, String.Empty);
            WrapInElement(factory, markupServices, markupRange);
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as SubScriptTextStyle);
        }

        public bool Equals(SubScriptTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return SubScript.Equals(obj.SubScript);
        }

        public override int GetHashCode()
        {
            return SubScript.GetHashCode();
        }
    }

}
