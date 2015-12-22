// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class CropDecorator : IImageDecoratorOriginalSizeAdjuster
    {
        public static readonly string Id = "Crop";
        public CropDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            CropDecoratorSettings settings = new CropDecoratorSettings(context.Settings);
            //ImageUtils.AdjustBrightness(bsettings.Brightness, bsettings.Contrast, context.Image);
            Rectangle? rect = settings.CropRectangle;

            // only do the following if polaroid is active
            if (context.EnforcedAspectRatio != null)
            {
                float aspectRatio = context.EnforcedAspectRatio.Value;
                RotateFlipType flip;
                if (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert ||
                        context.InvocationSource == ImageDecoratorInvocationSource.Reset)
                {
                    flip = ImageUtils.GetFixupRotateFlipFromExifOrientation(context.Image);
                }
                else
                {
                    flip = context.ImageRotation;
                }
                if (ImageUtils.IsRotated90(flip))
                    aspectRatio = 1 / aspectRatio;

                rect = RectangleHelper.EnforceAspectRatio(rect ?? new Rectangle(Point.Empty, context.Image.Size), aspectRatio);
            }

            if (rect != null)
            {
                Bitmap cropped = ImageHelper2.CropBitmap(context.Image, (Rectangle)rect);
                try
                {
                    PropertyItem orientation = context.Image.GetPropertyItem(0x112);
                    if (orientation != null)
                        cropped.SetPropertyItem(orientation);
                }
                catch (ArgumentException)
                {
                    // orientation data was not present
                }
                context.Image = cropped;
            }
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new CropEditor();
        }

        public void AdjustOriginalSize(IProperties properties, ref Size size)
        {
            CropDecoratorSettings settings = new CropDecoratorSettings(properties);
            Rectangle? rect = settings.CropRectangle;
            if (rect != null)
                size = rect.Value.Size;
        }
    }

    internal class CropDecoratorSettings
    {
        private const string CROP_RECTANGLE_X = "CropRectangleX";
        private const string CROP_RECTANGLE_Y = "CropRectangleY";
        private const string CROP_RECTANGLE_W = "CropRectangleW";
        private const string CROP_RECTANGLE_H = "CropRectangleH";
        private const string ASPECT_RATIO_ID = "AspectRatioId";
        private const string ASPECT_RATIO = "AspectRatio";
        private readonly IProperties settings;

        public CropDecoratorSettings(IProperties settings)
        {
            this.settings = settings;
        }

        public double? AspectRatio
        {
            get
            {
                string aspectRatioString = settings.GetString(ASPECT_RATIO, null);
                if (aspectRatioString == null)
                    return null;
                else
                    return double.Parse(aspectRatioString, CultureInfo.InvariantCulture);
            }
            set
            {
                if (value == null)
                    settings.Remove(ASPECT_RATIO);
                else
                    settings.SetString(ASPECT_RATIO, value.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public string AspectRatioId
        {
            get { return settings.GetString(ASPECT_RATIO_ID, null); }
            set { settings.SetString(ASPECT_RATIO_ID, value); }
        }

        public Rectangle? CropRectangle
        {
            get
            {
                if (!settings.Contains(CROP_RECTANGLE_X))
                    return null;
                return new Rectangle(
                    settings.GetInt(CROP_RECTANGLE_X, 0),
                    settings.GetInt(CROP_RECTANGLE_Y, 0),
                    settings.GetInt(CROP_RECTANGLE_W, 1),
                    settings.GetInt(CROP_RECTANGLE_H, 1)
                    );
            }
            set
            {
                if (value == null)
                {
                    settings.Remove(CROP_RECTANGLE_X);
                    settings.Remove(CROP_RECTANGLE_Y);
                    settings.Remove(CROP_RECTANGLE_W);
                    settings.Remove(CROP_RECTANGLE_H);
                }
                else
                {
                    settings.SetInt(CROP_RECTANGLE_X, value.Value.X);
                    settings.SetInt(CROP_RECTANGLE_Y, value.Value.Y);
                    settings.SetInt(CROP_RECTANGLE_W, value.Value.Width);
                    settings.SetInt(CROP_RECTANGLE_H, value.Value.Height);
                }
            }
        }

        public StateToken CreateStateToken()
        {
            return new StateToken(this);
        }

        public class StateToken
        {
            private readonly CropDecoratorSettings settings;
            private readonly Rectangle? cropRectangle;
            private readonly double? aspectRatio;
            private readonly string aspectRatioId;

            internal StateToken(CropDecoratorSettings settings)
            {
                this.settings = settings;
                cropRectangle = settings.CropRectangle;
                aspectRatio = settings.AspectRatio;
                aspectRatioId = settings.AspectRatioId;
            }

            public void Restore()
            {
                settings.CropRectangle = cropRectangle;
                settings.AspectRatio = aspectRatio;
                settings.AspectRatioId = aspectRatioId;
            }
        }
    }
}
