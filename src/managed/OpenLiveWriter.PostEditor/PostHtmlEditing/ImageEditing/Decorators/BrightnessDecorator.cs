// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class BrightnessDecorator : IImageDecorator
    {

        public readonly static string Id = "BrightnessContrast";
        public BrightnessDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            BrightnessDecoratorSettings bsettings = new BrightnessDecoratorSettings(context.Settings);
            context.Image = AdjustBrightness(bsettings.Brightness, bsettings.Contrast, context.Image);
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new BrightnessEditor();
        }

        public static Bitmap AdjustBrightness(float bf, float cf, Bitmap bitmap)
        {
            ColorMatrix bm = new ColorMatrix(new float[][]{
                    new float[]{1f,    0f,    0f,    0f,    0f},
                    new float[]{0f,    1f,    0f,    0f,    0f},
                    new float[]{0f,    0f,    1f,    0f,    0f},
                    new float[]{0f,    0f,    0f,    1f,    0f},
                    new float[]{bf,    bf,    bf,    1f,    1f}});

            //Create the contrast matrix.  Note, the last row offsets the colors by .001 to avoid
            //arithmetic overflows that cause dramatically incorrect colors.
            //Thanks to Bob Powell for that tip: http://www.bobpowell.net/image_contrast.htm
            ColorMatrix cm = new ColorMatrix(new float[][]{
                    new float[]{cf, 0f, 0f, 0f, 0f},
                    new float[]{0f, cf, 0f, 0f, 0f},
                    new float[]{0f, 0f, cf, 0f, 0f},
                    new float[]{0f, 0f, 0f, 1f, 0f},
                    new float[]{0.001f, 0.001f, 0.001f, 0f, 1f}});

            ColorMatrix adjust = null;
            if (bf != 0)
                adjust = bm;
            if (cf != 1)
            {
                if (adjust != null)
                    adjust = ImageUtils.Multiply(adjust, cm);
                else
                    adjust = cm;
            }

            if (adjust != null)
                return ImageHelper.ApplyColorMatrix(bitmap, adjust);
            else
                return bitmap;
        }
    }

    internal class BrightnessDecoratorSettings
    {
        private static readonly string BRIGHTNESS = "Brightness";
        private static readonly string CONTRAST = "Contrast";
        private readonly IProperties Settings;
        public BrightnessDecoratorSettings(IProperties settings)
        {
            Settings = settings;
        }

        public float Brightness
        {
            get { return Settings.GetFloat(BRIGHTNESS, 0f); }
            set { Settings.SetFloat(BRIGHTNESS, value); }
        }

        public float Contrast
        {
            get { return Settings.GetFloat(CONTRAST, 1f); }
            set { Settings.SetFloat(CONTRAST, value); }
        }
    }
}
