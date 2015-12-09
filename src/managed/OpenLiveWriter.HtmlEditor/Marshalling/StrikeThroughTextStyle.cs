// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class StrikeThroughTextStyle : TextStyle, IEquatable<StrikeThroughTextStyle>
    {
        public StrikeThroughTextStyle(bool strikeThrough)
        {
            StrikeThrough = strikeThrough;
        }

        public bool StrikeThrough { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            if (!commands[IDM.STRIKETHROUGH].Enabled)
            {
                return;
            }

            commands[IDM.STRIKETHROUGH].Execute();
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as StrikeThroughTextStyle);
        }

        public bool Equals(StrikeThroughTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return StrikeThrough.Equals(obj.StrikeThrough);
        }

        public override int GetHashCode()
        {
            return StrikeThrough.GetHashCode();
        }
    }

}
