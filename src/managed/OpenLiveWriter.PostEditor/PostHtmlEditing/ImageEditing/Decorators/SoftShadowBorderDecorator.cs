// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class SoftShadowBorderDecorator : ImageSliceBorderDecorator
    {
        public readonly static string Id = "SoftShadowBorder";

        protected override Bitmap BorderImage
        {
            get { return ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.ImageEditing.Images.SoftShadowBorder.png"); }
        }

        protected override bool DiscardColors { get { return true; } }

        protected override int[] SliceLines
        {
            get { return new int[] { 45, 57, 45, 53 }; }
        }

        protected override Rectangle AdjustImagePositionAndSize(ImageDecoratorContext context)
        {
            return new Rectangle(22, 14, 102 - 58, 98 - 54);
        }

        public override ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return null;
        }

        public override Bitmap BitmapLarge
        {
            get { return ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.ImageEditing.Images.BorderStyleRoundedCorners.png"); }
        }
    }
}
