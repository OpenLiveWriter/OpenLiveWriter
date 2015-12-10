// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    class TiltDecorator : IImageDecorator
    {
        public static string Id = "TiltDecorator";
        public void Decorate(ImageDecoratorContext context)
        {
            if (context.ImageEmbedType == ImageEmbedType.Embedded && context.InvocationSource != ImageDecoratorInvocationSource.TiltPreview)
            {
                TiltDecoratorSettings settings = new TiltDecoratorSettings(context.Settings);
                if (settings.TiltDegrees != 0)
                {
                    DropShadowBorderDecoratorSettings borderSettings =
                        new DropShadowBorderDecoratorSettings(context);

                    Size oldSize = context.Image.Size;
                    Debug.WriteLine(
                        string.Format(CultureInfo.InvariantCulture, "Tilting {0}x{1} image by {2} degrees", context.Image.Width, context.Image.Height,
                                      settings.TiltDegrees));

                    context.Image =
                        ImageHelper.RotateBitmap(context.Image, settings.TiltDegrees, borderSettings.BackgroundColor);
                    Size newSize = context.Image.Size;

                    context.BorderMargin = new ImageBorderMargin(
                        context.BorderMargin,
                        newSize.Width - oldSize.Width,
                        newSize.Height - oldSize.Height,
                        new BorderCalculation(newSize.Width / (float)oldSize.Width, newSize.Height / (float)oldSize.Height));
                }
            }
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new TiltDecoratorEditor();
        }
    }
}
