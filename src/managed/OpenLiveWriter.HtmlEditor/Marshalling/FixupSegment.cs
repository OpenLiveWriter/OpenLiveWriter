// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class FixupSegment
    {
        public FixupSegment(MarkupRange rangeToFixup, TextStyle sourceTextStyle)
        {
            if (rangeToFixup == null)
            {
                throw new ArgumentNullException("rangeToFixup");
            }

            if (!rangeToFixup.Positioned)
            {
                throw new ArgumentException("rangeToFixup must be positioned!");
            }

            if (sourceTextStyle == null)
            {
                throw new ArgumentNullException("sourceTextStyle");
            }

            SourceTextStyle = sourceTextStyle;
            RangeToFixup = rangeToFixup.Clone();

            // We want the RangeToFixup to stick with the text its wrapped around.
            RangeToFixup.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            RangeToFixup.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
        }

        public TextStyle SourceTextStyle { get; private set; }
        public MarkupRange RangeToFixup { get; private set; }

        public void DoFixup(MshtmlMarkupServices markupServices, MshtmlCoreCommandSet commands)
        {
            SourceTextStyle.Apply(markupServices, RangeToFixup, commands);
        }
    }

}
