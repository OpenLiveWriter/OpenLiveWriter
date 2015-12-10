// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Represents a gallery command whose properties are updated based on the selected gallery item.
    /// </summary>
    public class SelectGalleryCommand<T> : GalleryCommand<T> where T : IComparable
    {
        protected SelectGalleryCommand(CommandId commandId)
            : base(commandId, false)
        {
        }

        public override string LabelTitle
        {
            get
            {
                if (selectedIndex != INVALID_INDEX)
                    return TextHelper.GetTitleFromText(items[selectedIndex].Label, RibbonHelper.GalleryItemTextMaxChars, TextHelper.Units.Characters);

                return String.Empty;
            }
        }

        public override Bitmap LargeImage
        {
            get
            {
                if (selectedIndex != INVALID_INDEX)
                    return items[selectedIndex].Image;

                return null;
            }
        }

        public override Bitmap LargeHighContrastImage
        {
            get
            {
                if (selectedIndex != INVALID_INDEX)
                    return items[selectedIndex].Image;

                return null;
            }
        }

        public override Bitmap SmallImage
        {
            get
            {
                if (selectedIndex != INVALID_INDEX)
                    return items[selectedIndex].Image;

                return null;
            }
        }

        public override Bitmap SmallHighContrastImage
        {
            get
            {
                if (selectedIndex != INVALID_INDEX)
                    return items[selectedIndex].Image;

                return null;
            }
        }
    }
}
