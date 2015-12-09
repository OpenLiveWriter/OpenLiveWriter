// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class BackgroundColorTextStyle : TextStyle, IEquatable<BackgroundColorTextStyle>
    {
        public BackgroundColorTextStyle(Color backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public Color BackgroundColor { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            if (!commands[IDM.BACKCOLOR].Enabled)
            {
                return;
            }

            commands[IDM.BACKCOLOR].Execute(ColorHelper.ColorToString(BackgroundColor));
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as BackgroundColorTextStyle);
        }

        public bool Equals(BackgroundColorTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return BackgroundColor.Equals(obj.BackgroundColor);
        }

        public override int GetHashCode()
        {
            return BackgroundColor.GetHashCode();
        }
    }
}
