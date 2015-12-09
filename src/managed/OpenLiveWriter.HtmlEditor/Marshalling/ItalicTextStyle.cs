// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class ItalicTextStyle : TextStyle, IEquatable<ItalicTextStyle>
    {
        public ItalicTextStyle(bool italic)
        {
            Italic = italic;
        }

        public bool Italic { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            if (!commands[IDM.ITALIC].Enabled)
            {
                return;
            }

            commands[IDM.ITALIC].Execute();
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as ItalicTextStyle);
        }

        public bool Equals(ItalicTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return Italic.Equals(obj.Italic);
        }

        public override int GetHashCode()
        {
            return Italic.GetHashCode();
        }
    }

}
