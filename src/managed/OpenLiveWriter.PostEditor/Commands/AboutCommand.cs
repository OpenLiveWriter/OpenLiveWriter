// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    class AboutCommand : Command
    {
        public AboutCommand() : base(CommandId.About)
        {
        }

        public override string LabelTitle
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, Res.Get(StringId.AboutAbout), ApplicationEnvironment.ProductNameQualified);
            }
        }

        public override string TooltipDescription
        {
            get
            {
                return TextHelper.StripHotkey(LabelTitle);
            }
        }
    }
}
