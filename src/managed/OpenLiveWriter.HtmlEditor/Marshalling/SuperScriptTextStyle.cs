// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class SuperScriptTextStyle : TextStyle, IEquatable<SuperScriptTextStyle>
    {
        public SuperScriptTextStyle(bool superScript)
        {
            SuperScript = superScript;
        }

        public bool SuperScript { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            ElementFactory factory = () => markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_SUP, String.Empty);
            WrapInElement(factory, markupServices, markupRange);
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as SuperScriptTextStyle);
        }

        public bool Equals(SuperScriptTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return SuperScript.Equals(obj.SuperScript);
        }

        public override int GetHashCode()
        {
            return SuperScript.GetHashCode();
        }
    }

}
