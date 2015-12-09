// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class BoldTextStyle : TextStyle, IEquatable<BoldTextStyle>
    {
        public BoldTextStyle(bool bold)
        {
            Bold = bold;
        }

        public bool Bold { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            if (!commands[IDM.BOLD].Enabled)
            {
                return;
            }

            BoldApplier boldApplier = new BoldApplier(markupServices, markupRange, commands[IDM.BOLD]);
            boldApplier.Execute();
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as BoldTextStyle);
        }

        public bool Equals(BoldTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return Bold.Equals(obj.Bold);
        }

        public override int GetHashCode()
        {
            return Bold.GetHashCode();
        }
    }

}
