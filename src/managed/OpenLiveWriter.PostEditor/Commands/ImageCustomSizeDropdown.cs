// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class ImageCustomSizeDropdown : MutuallyExclusiveDropdown<ImageSizeName>
    {
        public ImageCustomSizeDropdown(Command dropDownCommand, Command[] commands, EventHandler executeHandler)
            : base(dropDownCommand, commands, executeHandler)
        {
        }

        public override void SelectTag(ImageSizeName imageSizeName)
        {
            if (imageSizeName == ImageSizeName.Custom)
            {
                DropDownCommand.LabelTitle = Res.Get(StringId.ImgSBSizerCustom);
                foreach (Command c in Commands)
                {
                    c.Latched = false;
                    c.Invalidate(Keys);
                }
            }
            else
            {
                switch (imageSizeName)
                {
                    case ImageSizeName.Small:
                        SelectCommand(CommandId.CustomSizeSmall);
                        break;
                    case ImageSizeName.Medium:
                        SelectCommand(CommandId.CustomSizeMedium);
                        break;
                    case ImageSizeName.Large:
                        SelectCommand(CommandId.CustomSizeLarge);
                        break;
                    case ImageSizeName.Full:
                        SelectCommand(CommandId.CustomSizeOriginal);
                        break;
                    default:
                        Debug.Fail("Unexpected ImageSizeName");
                        break;
                }
            }
        }

        public override void UpdateDropdown(Command selectedCommand)
        {
            Debug.Assert(selectedCommand.Tag is ImageSizeName);
            ImageSizeName imageSizeName = (ImageSizeName)selectedCommand.Tag;

            switch (imageSizeName)
            {
                case ImageSizeName.Small:
                    DropDownCommand.LabelTitle = Res.Get(StringId.ImgSBSizerSmall);
                    break;
                case ImageSizeName.Medium:
                    DropDownCommand.LabelTitle = Res.Get(StringId.ImgSBSizerMedium);
                    break;
                case ImageSizeName.Large:
                    DropDownCommand.LabelTitle = Res.Get(StringId.ImgSBSizerLarge);
                    break;
                case ImageSizeName.Full:
                    DropDownCommand.LabelTitle = Res.Get(StringId.ImgSBSizerOriginal);
                    break;
                //case ImageSizeName.Custom:
                //    DropDownCommand.LabelTitle = Res.Get(StringId.ImgSBSizerCustom);
                //    break;
                default:
                    Debug.Fail("Unexpected ImageSizeName");
                    break;
            }
        }
    }
}
