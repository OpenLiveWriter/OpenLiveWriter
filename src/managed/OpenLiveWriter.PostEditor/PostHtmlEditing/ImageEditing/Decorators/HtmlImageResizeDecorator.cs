// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class HtmlImageResizeDecorator : IImageDecorator, IImageDecoratorDefaultSettingsCustomizer
    {
        public const string Id = "ImageResizeRotate";
        public HtmlImageResizeDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            bool useOriginalImage = true;
            HtmlImageResizeDecoratorSettings settings = new HtmlImageResizeDecoratorSettings(context.Settings, context.ImgElement);
            if (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert ||
                context.InvocationSource == ImageDecoratorInvocationSource.Reset)
            {
                // WinLive 96840 - Copying and pasting images within shared canvas should persist source
                // decorator settings.
                // If ImageSizeName is set, then use that instead of default values
                if (settings.IsImageSizeNameSet && context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert)
                {
                    // We must be copying settings from another instance of the same image
                    settings.SetImageSize(settings.ImageSize, settings.ImageSizeName);

                    // WinLive 96840 - Copying and pasting images within shared canvas should persist source
                    // decorator settings.
                    // Also if we are copying settings, then use the image instance from context instead of the
                    // original image. This ensures we use the cropped image (if any) to resize.
                    useOriginalImage = false;
                }
                else
                {
                    //calculate the default image size and rotation.  If the camera has indicated that the
                    //orientation of the photo is rotated (in the EXIF data), shift the rotation appropriately
                    //to insert the image correctly.

                    //Fix the image orientation based on the Exif data (added by digital cameras).
                    RotateFlipType fixedRotation = ImageUtils.GetFixupRotateFlipFromExifOrientation(context.Image);
                    settings.Rotation = fixedRotation;

                    settings.BaseSize = context.Image.Size;

                    //the default size is a scaled version of the image based on the default inline size constraints.
                    Size defaultBoundsSize;
                    if (settings.DefaultBoundsSizeName != ImageSizeName.Full)
                    {
                        defaultBoundsSize = settings.DefaultBoundsSize;
                    }
                    else //original size is default, so we aren't going to scale
                    {
                        defaultBoundsSize = context.Image.Size;
                    }
                    //calculate the base image size to scale from.  If the image is rotated 90 degrees, then switch the height/width
                    Size baseImageSize = context.Image.Size;
                    if (ImageUtils.IsRotated90(settings.Rotation))
                        baseImageSize = new Size(baseImageSize.Height, baseImageSize.Width);

                    //calculate and set the scaled default size using the defaultSizeBounds
                    //Note: if the image dimensions are smaller than the default, don't scale that dimension (bug 419446)
                    Size defaultSize =
                        ImageUtils.GetScaledImageSize(Math.Min(baseImageSize.Width, defaultBoundsSize.Width),
                                                      Math.Min(baseImageSize.Height, defaultBoundsSize.Height),
                                                      baseImageSize);
                    if (defaultSize.Width < defaultBoundsSize.Width && defaultSize.Height < defaultBoundsSize.Height)
                        settings.SetImageSize(defaultSize, ImageSizeName.Full);
                    else
                        settings.SetImageSize(defaultSize, settings.DefaultBoundsSizeName);
                }
            }
            else if (settings.BaseSizeChanged(context.Image))
            {
                Size newBaseSize = context.Image.Size;
                settings.SetImageSize(AdjustImageSizeForNewBaseSize(true, settings, newBaseSize, settings.Rotation, context), null);
                settings.BaseSize = newBaseSize;
            }

            //this decorator only applies to embedded images.
            if (context.ImageEmbedType == ImageEmbedType.Embedded && !ImageHelper2.IsAnimated(context.Image))
            {
                Bitmap imageToResize = null;

                // To make image insertion faster, we've already created an initial resized image on a background thread.
                if (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert && useOriginalImage)
                {
                    try
                    {
                        string imageSrc = context.ImgElement.getAttribute("src", 2) as string;
                        if (!string.IsNullOrEmpty(imageSrc) && (UrlHelper.IsFileUrl(imageSrc) || File.Exists(new Uri(imageSrc).ToString())))
                        {
                            Uri imageSrcUri = new Uri(imageSrc);
                            imageToResize = (Bitmap)Image.FromFile(imageSrcUri.LocalPath);
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.Write("Failed to load pre-created initial image: " + e);
                    }
                }

                if (imageToResize == null)
                    imageToResize = context.Image;

                // Figure the desired image size by taking the size of the img element
                // and calculate what borderless image size we'd need to start with
                // to end up at that size. This is different than simply subtracting
                // the existing border size, since borders can be relative to the
                // size of the base image.
                Size desiredImageSize = settings.BorderMargin.ReverseCalculateImageSize(settings.ImageSizeWithBorder);

                //resize the image and update the image used by the context.
                if (desiredImageSize != imageToResize.Size || settings.Rotation != RotateFlipType.RotateNoneFlipNone)
                {
                    context.Image = ResizeImage(imageToResize, desiredImageSize, settings.Rotation);
                }
                else
                {
                    context.Image = imageToResize;
                }

                if (settings.ImageSize != context.Image.Size)
                    settings.SetImageSize(context.Image.Size, settings.ImageSizeName);
            }
        }

        internal static Size AdjustImageSizeForNewBaseSize(bool allowEnlargement, IResizeDecoratorSettings s, Size newBaseSize, RotateFlipType rotation, ImageDecoratorContext context)
        {
            Size rotatedBaseSize = ImageUtils.IsRotated90(rotation)
                ? new Size(newBaseSize.Height, newBaseSize.Width)
                : newBaseSize;

            if (s.ImageSizeName != ImageSizeName.Custom)
            {
                // If a standard image size is being used, fit to that

                Size sizeBounds = ImageSizeHelper.GetSizeConstraints(s.ImageSizeName);
                if (!allowEnlargement)
                {
                    sizeBounds.Width = Math.Min(rotatedBaseSize.Width, sizeBounds.Width);
                    sizeBounds.Height = Math.Min(rotatedBaseSize.Height, sizeBounds.Height);
                }
                return ImageUtils.GetScaledImageSize(sizeBounds.Width, sizeBounds.Height, rotatedBaseSize);
            }
            else
            {
                // If custom size, but we know the base size, preserve
                // the aspect ratio "skew" (difference in x and y DPI)
                // and pixel area

                Size imageSize = s.ImageSize;
                // Need to get the image size to the non-rotated angle,
                // because s.BaseSize dimensions are always pre-rotation.
                // Although ImageSize has not been fully updated for this
                // decorator yet (that's what we're trying to do here),
                // the width/height gets flipped immediately when a
                // rotation is applied, so rotation is already taken
                // into account.
                if (ImageUtils.IsRotated90(rotation))
                    imageSize = new Size(imageSize.Height, imageSize.Width);

                // If the base size has not been set yet, we have to guess.
                // This basically means the image was inserted using an older
                // build of Writer that did not have the crop feature. Ideally
                // we would use the original image size, but this is expensive
                // to get from here. It just so happens that newBaseSize works
                // for now because the crop dialog defaults to the same aspect
                // ratio as the original image, but if that ever changes this
                // will break.
#if DEBUG
                if (s.BaseSize == null)
                {
                    using (Bitmap bitmap = (Bitmap)Bitmap.FromFile(context.SourceImageUri.LocalPath))
                    {
                        Size size = new Size(Math.Max(1, bitmap.Width / 2),
                                             Math.Max(1, bitmap.Height / 2));
                        Debug.Assert(size.Equals(newBaseSize) || bitmap.Size.Equals(newBaseSize), "Check base size assumptions. Can't use 's.BaseSize ?? newBaseSize', instead must calculate original image size (context.SourceImageUri.LocalPath).");
                    }
                }
#endif
                Size baseSize = s.BaseSize ?? newBaseSize;

                double xFactor = imageSize.Width / (double)baseSize.Width;
                double yFactor = imageSize.Height / (double)baseSize.Height;
                newBaseSize = new Size(
                    (int)Math.Round(xFactor * newBaseSize.Width),
                    (int)Math.Round(yFactor * newBaseSize.Height)
                    );

                // Need to re-apply the rotation if necessary.
                if (ImageUtils.IsRotated90(rotation))
                    newBaseSize = new Size(newBaseSize.Height, newBaseSize.Width);

                // At this point, newBaseSize has the right aspect ratio; we now
                // need to scale it so it uses about the same number of pixels
                // as it did before.

                double factor = (imageSize.Width * imageSize.Height) / (double)(newBaseSize.Width * newBaseSize.Height);
                factor = Math.Sqrt(factor);
                newBaseSize.Width = (int)Math.Round(newBaseSize.Width * factor);
                newBaseSize.Height = (int)Math.Round(newBaseSize.Height * factor);

                if (!allowEnlargement)
                {
                    if (newBaseSize.Width > rotatedBaseSize.Width || newBaseSize.Height > rotatedBaseSize.Height)
                        newBaseSize = ImageUtils.GetScaledImageSize(rotatedBaseSize.Width, rotatedBaseSize.Height, newBaseSize);
                }

                return newBaseSize;
            }
        }

        internal static Bitmap ResizeImage(Bitmap image, Size newSize, RotateFlipType rotation)
        {
            if (rotation != RotateFlipType.RotateNoneFlipNone)
            {
                image.RotateFlip(rotation);
                image = new Bitmap(image);
            }

            int newWidth = Math.Max(newSize.Width, 2);
            int newHeight = Math.Max(newSize.Height, 2);

            //resize the image (if its not already the correct size!)
            Bitmap bitmap;
            if (image.Width != newWidth || image.Height != newHeight)
                bitmap = ImageHelper2.CreateResizedBitmap(image, newWidth, newHeight, image.RawFormat);
            else
                bitmap = image;
            return bitmap;
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new HtmlImageResizeEditor(commandManager);
        }

        void IImageDecoratorDefaultSettingsCustomizer.CustomizeDefaultSettingsBeforeSave(ImageDecoratorEditorContext context, IProperties defaultSettings)
        {
            //clear all defaulted settings for this decorator
            foreach (string key in defaultSettings.Names)
                defaultSettings.Remove(key);

            HtmlImageResizeDecoratorSettings defaultResizeSettings = new HtmlImageResizeDecoratorSettings(defaultSettings, context.ImgElement);
            HtmlImageResizeDecoratorSettings resizeSettings = new HtmlImageResizeDecoratorSettings(context.Settings, context.ImgElement);

            //explicitly save the settings we want to support defaulting for.
            defaultResizeSettings.DefaultBoundsSizeName = resizeSettings.ImageSizeName;
            if (resizeSettings.ImageSizeName == ImageSizeName.Custom)
            {
                defaultResizeSettings.DefaultBoundsSize = resizeSettings.ImageSize;
            }
        }
    }

    internal interface IResizeDecoratorSettings
    {
        Size ImageSize { get; }

        /// <summary>
        /// The image size name used when determining the current image size.
        /// This value is used to decide what the best initial size should be for the image if the current
        /// size is saved as the default size.  Rather than forcing all future images to be exactly the current
        /// size, the named size can be used for more flexibility.
        /// </summary>
        ImageSizeName ImageSizeName { get; }

        Size DefaultBoundsSize { get; }

        ImageSizeName DefaultBoundsSizeName { get; }

        Size? BaseSize { get; }
    }

    internal class HtmlImageResizeDecoratorSettings : IResizeDecoratorSettings
    {
        private readonly IProperties Settings;
        private readonly IHTMLElement ImgElement;
        private const string ROTATION = "Rotation";
        private const string SIZE_NAME = "ImageSizeName";
        private const string ASPECT_RATIO_LOCKED = "AspectRatioLocked";
        private const string DEFAULT_WIDTH = "DefaultImageWidth";
        private const string DEFAULT_HEIGHT = "DefaultImageHeight";
        private const string DEFAULT_SIZE_NAME = "DefaultImageSizeName";
        private const string IMAGE_WIDTH_OFFSET = "ImageWidthOffset";
        private const string IMAGE_HEIGHT_OFFSET = "ImageHeightOffset";

        private const string TARGET_ASPECT_RATIO_WIDTH = "TargetAspectRatioWidth";
        private const string TARGET_ASPECT_RATIO_HEIGHT = "TargetAspectRatioHeight";

        private const string BASE_WIDTH = "BaseWidth";
        private const string BASE_HEIGHT = "BaseHeight";
        private const string PREV_ROTATION = "PrevRotation";

        private const string BORDER_INFO = "BorderInfo";
        private const string BORDER_TOP = "BorderTop";
        private const string BORDER_RIGHT = "BorderRight";
        private const string BORDER_BOTTOM = "BorderBottom";
        private const string BORDER_LEFT = "BorderLeft";
        public HtmlImageResizeDecoratorSettings(IProperties settings, IHTMLElement imgElement)
        {
            Settings = settings;
            ImgElement = imgElement;
        }

        public Size ImageSize
        {
            get
            {
                ImageBorderMargin borderMargin = BorderMargin;
                Size imageSizeWithBorder = ImageSizeWithBorder;
                int width = imageSizeWithBorder.Width - borderMargin.Width;
                int height = imageSizeWithBorder.Height - borderMargin.Height;
                Size size = new Size(width, height);
                //Initialize the saved aspect ratio if it has no value
                if (TargetAspectRatioSize.Width == -1)
                    TargetAspectRatioSize = size;
                return size;
                //return borderMargin.ReverseCalculateImageSize(imageSizeWithBorder);
            }
        }

        public Size ImageSizeWithBorder
        {
            get
            {
                IHTMLImgElement imgElement = (IHTMLImgElement)ImgElement;

                //get the true size of the image by removing size offsets that may be applied to the image by the CSS margin/padding
                int width = imgElement.width - Settings.GetInt(IMAGE_WIDTH_OFFSET, 0);
                int height = imgElement.height - Settings.GetInt(IMAGE_HEIGHT_OFFSET, 0);
                return new Size(width, height);
            }
        }

        /// <summary>
        /// Sets the new size of the image (not including the border)
        /// </summary>
        /// <param name="size"></param>
        /// <param name="sizeName"></param>
        public void SetImageSize(Size size, ImageSizeName? sizeName)
        {
            IHTMLImgElement imgElement = (IHTMLImgElement)ImgElement;

            ImageBorderMargin borderMargin = BorderMargin;
            // The next line is a little bit tortured, but
            // I'm trying to introduce the concept of "calculated image size"
            // for more complex border calculations without breaking any
            // existing logic.
            Size sizeWithBorder = ImageSize.Equals(size)
                ? ImageSizeWithBorder : borderMargin.CalculateImageSize(size);

            if (imgElement.width != sizeWithBorder.Width || imgElement.height != sizeWithBorder.Height)
            {
                imgElement.width = sizeWithBorder.Width;
                imgElement.height = sizeWithBorder.Height;
            }

            //remember the size offsets which are added by CSS margins/padding
            Settings.SetInt(IMAGE_WIDTH_OFFSET, imgElement.width - sizeWithBorder.Width);
            Settings.SetInt(IMAGE_HEIGHT_OFFSET, imgElement.height - sizeWithBorder.Height);

            if (sizeName != null)
                ImageSizeName = sizeName.Value;

            //Initialize the saved aspect ratio if it has no value
            //OR update it if the ratio has been changed
            Size targetSize = TargetAspectRatioSize;
            if (targetSize.Width == -1 ||
               (size.Width != Math.Round((targetSize.Width * (float)size.Height) / targetSize.Height) &&
                size.Height != Math.Round((targetSize.Height * (float)size.Width) / targetSize.Width)))
            {
                TargetAspectRatioSize = size;
            }
        }

        public ImageBorderMargin BorderMargin
        {
            get
            {

                if (Settings.ContainsSubProperties(BORDER_INFO))
                {
                    return new ImageBorderMargin(Settings.GetSubProperties(BORDER_INFO));
                }

                // backwards compatibility with previous releases
                int top = Settings.GetInt(BORDER_TOP, 0);
                int right = Settings.GetInt(BORDER_RIGHT, 0);
                int bottom = Settings.GetInt(BORDER_BOTTOM, 0);
                int left = Settings.GetInt(BORDER_LEFT, 0);
                return new ImageBorderMargin(right + left, bottom + top, new BorderCalculation(right + left, bottom + top));
            }
            set
            {
                value.Save(Settings.GetSubProperties(BORDER_INFO));

                Settings.SetInt(BORDER_TOP, value.Height);
                Settings.SetInt(BORDER_RIGHT, value.Width);
                Settings.SetInt(BORDER_BOTTOM, 0);
                Settings.SetInt(BORDER_LEFT, 0);
            }
        }

        /// <summary>
        /// The image size name used when determining the current image size.
        /// This value is used to decide what the best initial size should be for the image if the current
        /// size is saved as the default size.  Rather than forcing all future images to be exactly the current
        /// size, the named size can be used for more flexibility.
        /// </summary>
        public ImageSizeName ImageSizeName
        {
            get
            {
                ImageSizeName bounds =
                    (ImageSizeName)ImageSizeName.Parse(
                    typeof(ImageSizeName),
                    Settings.GetString(SIZE_NAME, ImageSizeName.Full.ToString()));

                return bounds;
            }
            set
            {
                Settings.SetString(SIZE_NAME, value.ToString());
            }
        }

        public bool IsImageSizeNameSet
        {
            get
            {
                return Settings.GetString(SIZE_NAME, null) != null;
            }
        }

        public bool AspectRatioLocked
        {
            get
            {
                return Settings.GetBoolean(ASPECT_RATIO_LOCKED, true);
            }
            set
            {
                Settings.SetBoolean(ASPECT_RATIO_LOCKED, value);
            }
        }

        public Size TargetAspectRatioSize
        {
            get
            {
                return new Size(Settings.GetInt(TARGET_ASPECT_RATIO_WIDTH, -1), Settings.GetInt(TARGET_ASPECT_RATIO_HEIGHT, -1));
            }
            set
            {
                Settings.SetInt(TARGET_ASPECT_RATIO_WIDTH, value.Width);
                Settings.SetInt(TARGET_ASPECT_RATIO_HEIGHT, value.Height);
            }
        }

        public Size DefaultBoundsSize
        {
            get
            {
                return GetDefaultImageSize(Settings);
            }
            set
            {
                Settings.SetInt(DEFAULT_WIDTH, value.Width);
                Settings.SetInt(DEFAULT_HEIGHT, value.Height);
            }
        }

        public static Size GetDefaultImageSize(IProperties settings)
        {
            ImageSizeName boundsSize = GetDefaultBoundsSizeName(settings);
            Size defaultBoundsSize;
            if (boundsSize != ImageSizeName.Custom)
                defaultBoundsSize = ImageSizeHelper.GetSizeConstraints(boundsSize);
            else
            {
                int defaultWidth = settings.GetInt(DEFAULT_WIDTH, 240);
                int defaultHeight = settings.GetInt(DEFAULT_HEIGHT, 240);
                defaultBoundsSize = new Size(defaultWidth, defaultHeight);
            }
            return defaultBoundsSize;
        }

        // This will callback to the hosting application, so we lazy load it.
        private static readonly LazyLoader<string> DefaultSizeName = new LazyLoader<string>(() => GlobalEditorOptions.GetSetting<string>(ContentEditorSetting.ImageDefaultSize));

        public static ImageSizeName GetDefaultBoundsSizeName(IProperties settings)
        {
            string defaultSizeName = DefaultSizeName;
            if (!Enum.IsDefined(typeof(ImageSizeName), defaultSizeName))
            {
                // This is our fallback if the hosting application returns a bad value.
                defaultSizeName = ImageSizeName.Small.ToString();
            }

            return (ImageSizeName)Enum.Parse(typeof(ImageSizeName), settings.GetString(DEFAULT_SIZE_NAME, defaultSizeName));
        }

        public ImageSizeName DefaultBoundsSizeName
        {
            get
            {
                return GetDefaultBoundsSizeName(Settings);
            }
            set
            {
                Settings.SetString(DEFAULT_SIZE_NAME, value.ToString());
            }
        }

        public string ImageUrl
        {
            get
            {
                return (string)ImgElement.getAttribute("src", 2);
            }
            set
            {
                ImgElement.setAttribute("src", value, 0);
            }
        }

        public RotateFlipType Rotation
        {
            get
            {
                string rotation = Settings.GetString(ROTATION, RotateFlipType.RotateNoneFlipNone.ToString());
                try
                {
                    return (RotateFlipType)RotateFlipType.Parse(typeof(RotateFlipType), rotation);
                }
                catch (Exception)
                {
                    return RotateFlipType.RotateNoneFlipNone;
                }
            }
            set { Settings.SetString(ROTATION, value.ToString()); }
        }

        // The base size is used to quickly determine whether the image has
        // been cropped since the last time the default bounds were calculated.
        public Size? BaseSize
        {
            get
            {
                if (!Settings.Contains(BASE_WIDTH) || !Settings.Contains(BASE_HEIGHT))
                    return null;

                try
                {
                    return new Size(Settings.GetInt(BASE_WIDTH, -1), Settings.GetInt(BASE_HEIGHT, -1));
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    Settings.Remove(BASE_WIDTH);
                    Settings.Remove(BASE_HEIGHT);
                }
                else
                {
                    Settings.SetInt(BASE_WIDTH, value.Value.Width);
                    Settings.SetInt(BASE_HEIGHT, value.Value.Height);
                }
            }
        }

        public RotateFlipType? PrevRotation
        {
            get { return (RotateFlipType)Enum.Parse(typeof(RotateFlipType), Settings.GetString(PREV_ROTATION, RotateFlipType.RotateNoneFlipNone.ToString())); }
            set { Settings.SetString(PREV_ROTATION, value.ToString()); }
        }

        public bool BaseSizeChanged(Bitmap image)
        {
            Size? baseSize = BaseSize;
            if (baseSize == null)
                return true;
            return !baseSize.Equals(image.Size);
        }
    }
}
