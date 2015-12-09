// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.Localization;
using System.Diagnostics;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing
{
    public class ImageTargetDropDownCommand : Command
    {
        private LinkTargetType _linkTargetType;

        public ImageTargetDropDownCommand(CommandId commandId)
            : base(commandId)
        {
        }

        public LinkTargetType LinkTargetType
        {
            get { return _linkTargetType; }
            set
            {
                _linkTargetType = value;

                switch (_linkTargetType)
                {
                    case LinkTargetType.IMAGE:
                        LabelTitle = Res.Get(StringId.LinkToSourceLabelTitle);
                        break;
                    case LinkTargetType.URL:
                        LabelTitle = Res.Get(StringId.LinkToURLLabelTitle);
                        break;
                    case LinkTargetType.NONE:
                        LabelTitle = Res.Get(StringId.LinkToNoneLabelTitle);
                        break;
                    default:
                        Debug.Fail("Unexpected LinkTargetType.");
                        break;
                }
            }
        }
    }
}
