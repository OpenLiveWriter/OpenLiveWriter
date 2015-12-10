// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public class BlogPostImagePropertiesInfo : ImagePropertiesInfo
    {
        private BlogPostImageData _imageData;
        public BlogPostImagePropertiesInfo(BlogPostImageData imageData, ImageDecoratorsList decorators)
        {
            _imageData = imageData;
            ImageFileData sourceFile = imageData.GetImageSourceFile();
            Init(sourceFile.Uri, new Size(sourceFile.Width, sourceFile.Height), decorators);
        }

        public override Uri ImageSourceUri
        {
            get { return _imageData.GetImageSourceFile().Uri; }
        }

        public override Size ImageSourceSize
        {
            get
            {
                ImageFileData sourceFile = _imageData.GetImageSourceFile();
                return ImageDecorators.GetAdjustedOriginalSize(new Size(sourceFile.Width, sourceFile.Height));
            }
        }

        public override bool IsEmbeddedImage()
        {
            return true;
        }

        public override bool IsEditableEmbeddedImage()
        {
            if (!IsEmbeddedImage())
                return false;

            string path = _imageData.GetImageSourceFile().Uri.LocalPath;
            if (ImageHelper2.IsAnimated(path))
                return false;

            return !ImageHelper2.IsMetafile(path);
        }
    }

    public class ImagePropertiesInfo
    {
        public ImagePropertiesInfo(Uri sourceUri, Size size, ImageDecoratorsList decorators)
        {
            Init(sourceUri, new Size(size.Width, size.Height), decorators);
        }

        protected ImagePropertiesInfo()
        {
        }

        protected void Init(Uri sourceUri, Size size, ImageDecoratorsList decorators)
        {
            _imageSourceUri = sourceUri;
            _size = size;
            ImageDecorators = decorators;
        }

        public virtual bool IsEmbeddedImage()
        {
            return false;
        }

        public virtual bool IsEditableEmbeddedImage()
        {
            return false;
        }

        public virtual Uri ImageSourceUri
        {
            get
            {
                return _imageSourceUri;
            }
        }
        private Uri _imageSourceUri;

        public virtual Size ImageSourceSize
        {
            get
            {
                if (_size == new Size(int.MaxValue, int.MaxValue))
                    return _size;
                else
                {
                    // adjusts for cropping
                    return ImageDecorators.GetAdjustedOriginalSize(_size);
                }
            }
        }
        Size _size = new Size(Int32.MaxValue, Int32.MaxValue);

        /// <summary>
        /// If true, forces any change in the width or height of the inline image to maintain the inline image aspect
        /// ratio.
        /// </summary>
        public bool InlineImageAspectRatioLocked
        {
            get
            {
                return InlineImageSettings.AspectRatioLocked;
            }
            set
            {
                InlineImageSettings.AspectRatioLocked = value;
            }
        }

        /// <summary>
        /// Changing this width while InlineImageAspectRatioLocked is true will also change the height in order to
        /// maintain the inline image aspect ratio.
        /// </summary>
        public int InlineImageWidth
        {
            get
            {
                return InlineImageSettings.ImageSize.Width;
            }
            set
            {
                Size newImageSize = new Size(value, InlineImageSize.Height);
                InlineImageSize = ImageUtils.GetConstrainedImageSize(newImageSize, InlineImageAspectRatioLocked, InlineImageSettings.TargetAspectRatioSize, InlineImageSize);
            }
        }

        /// <summary>
        /// Changing this height while InlineImageAspectRatioLocked is true will also change the width in order to
        /// maintain the inline image aspect ratio.
        /// </summary>
        public int InlineImageHeight
        {
            get
            {
                return InlineImageSettings.ImageSize.Height;
            }
            set
            {
                Size newImageSize = new Size(InlineImageSize.Width, value);
                InlineImageSize = ImageUtils.GetConstrainedImageSize(newImageSize, InlineImageAspectRatioLocked, InlineImageSettings.TargetAspectRatioSize, InlineImageSize);
            }
        }

        /// <summary>
        /// The size of the image when the aspect ratio was locked.
        /// </summary>
        public Size TargetAspectRatioSize
        {
            get
            {
                return InlineImageSettings.TargetAspectRatioSize;
            }
            set
            {
                InlineImageSettings.TargetAspectRatioSize = value;
            }
        }

        /// <summary>
        /// The size of the image that should be inserted directly into the document.
        /// </summary>
        public Size InlineImageSize
        {
            get
            {
                return InlineImageSettings.ImageSize;
            }
            set
            {
                // Keep the ImageSizeName in sync with any changes to the inline image size.
                ImageSizeName newImageSizeName = ImageSizeName.Custom;
                if (value == ImageUtils.ScaleImageSizeName(ImageSizeName.Full, ImageSourceSize, ImageRotation))
                    newImageSizeName = ImageSizeName.Full;
                else if (value == ImageUtils.ScaleImageSizeName(ImageSizeName.Large, ImageSourceSize, ImageRotation))
                    newImageSizeName = ImageSizeName.Large;
                else if (value == ImageUtils.ScaleImageSizeName(ImageSizeName.Medium, ImageSourceSize, ImageRotation))
                    newImageSizeName = ImageSizeName.Medium;
                else if (value == ImageUtils.ScaleImageSizeName(ImageSizeName.Small, ImageSourceSize, ImageRotation))
                    newImageSizeName = ImageSizeName.Small;

                InlineImageSettings.SetImageSize(value, newImageSizeName);
            }
        }

        public ImageSizeName InlineImageSizeName
        {
            get { return InlineImageSettings.ImageSizeName; }
            set
            {
                if (value != ImageSizeName.Custom)
                {
                    // Keep the inline image size in sync with any changes to the inline ImageSizeName.
                    Size newImageSize = ImageUtils.ScaleImageSizeName(value, ImageSourceSize, ImageRotation);
                    InlineImageSettings.SetImageSize(newImageSize, value);
                }
                else
                {
                    InlineImageSettings.ImageSizeName = value;
                }
            }
        }

        /// <summary>
        /// The size of the image excluding margins added by the image border.
        /// </summary>
        public Size InlineImageSizeWithBorder
        {
            get
            {
                return InlineImageSettings.ImageSizeWithBorder;
            }
        }

        /// <summary>
        /// The size of the image that should be inserted directly into the document.
        /// </summary>
        public ImageBorderMargin InlineImageBorderMargin
        {
            get
            {
                return InlineImageSettings.BorderMargin;
            }
            set
            {
                InlineImageSettings.BorderMargin = value;
            }
        }

        /// <summary>
        /// The alignment of the image that should be inserted directly into the document.
        /// </summary>
        public Alignment InlineImageAlignment
        {
            get
            {
                Alignment alignment = Alignment.None;
                switch (InlineAlignmentSettings.Alignment)
                {
                    case ImgAlignment.NONE:
                        alignment = Alignment.None;
                        break;
                    case ImgAlignment.LEFT:
                        alignment = Alignment.Left;
                        break;
                    case ImgAlignment.CENTER:
                        alignment = Alignment.Center;
                        break;
                    case ImgAlignment.RIGHT:
                        alignment = Alignment.Right;
                        break;
                    default:
                        Debug.Fail("Unexpected ImgAlignment");
                        break;
                }

                return alignment;
            }
            set
            {
                ImgAlignment imgAlignment = ImgAlignment.NONE;
                switch (value)
                {
                    case Alignment.None:
                        imgAlignment = ImgAlignment.NONE;
                        break;
                    case Alignment.Left:
                        imgAlignment = ImgAlignment.LEFT;
                        break;
                    case Alignment.Center:
                        imgAlignment = ImgAlignment.CENTER;
                        break;
                    case Alignment.Right:
                        imgAlignment = ImgAlignment.RIGHT;
                        break;
                    default:
                        Debug.Fail("Unexpected Alignment");
                        break;
                }

                InlineAlignmentSettings.Alignment = imgAlignment;
            }
        }

        /// <summary>
        /// The margins of the image that should be inserted directly into the document.
        /// </summary>
        public MarginStyle InlineImageMargin
        {
            get
            {
                return InlineMarginSettings.Margin;
            }
            set
            {
                InlineMarginSettings.Margin = value;
            }
        }

        /// <summary>
        /// The url of the image that should be inserted directly into the document.
        /// </summary>
        public string InlineImageUrl
        {
            get
            {
                return InlineImageSettings.ImageUrl;
            }
            set
            {
                InlineImageSettings.ImageUrl = value;
            }
        }

        /// <summary>
        /// Defines the type of target for the image.
        /// </summary>
        public LinkTargetType LinkTarget
        {
            get
            {
                return ImageTargetSettings.LinkTarget;
            }
            set
            {
                ImageTargetSettings.LinkTarget = value;
            }
        }

        /// <summary>
        /// Defines the default type of target for the image.
        /// </summary>
        public LinkTargetType DefaultLinkTarget
        {
            get
            {
                return ImageTargetSettings.DefaultLinkTarget;
            }
            set
            {
                ImageTargetSettings.DefaultLinkTarget = value;
            }
        }

        /// <summary>
        /// Defines the default link options the image.
        /// </summary>
        public ILinkOptions DefaultLinkOptions
        {
            get
            {
                return ImageTargetSettings.DefaultLinkOptions;
            }
        }

        /// <summary>
        /// Defines the link options for the image.
        /// </summary>
        public ILinkOptions LinkOptions
        {
            get
            {
                return ImageTargetSettings.LinkOptions;
            }
            set
            {
                ImageTargetSettings.LinkOptions = value;
            }
        }

        /// <summary>
        /// Get/Set the url for URL-based link targets.
        /// </summary>
        public string LinkTargetUrl
        {
            get
            {
                //if(LinkTarget != LinkTargetType.URL)
                //	throw new Exception("LinkTargetUrl property not supported for link targets with type: " + LinkTarget.ToString());
                return ImageTargetSettings.LinkTargetUrl;
            }
            set
            {
                ImageTargetSettings.LinkTargetUrl = value;
            }
        }

        /// <summary>
        /// Get/Set the ImageSizeName for the image-based link targets.
        /// </summary>
        public ImageSizeName LinkTargetImageSizeName
        {
            get
            {
                return ImageTargetSettings.ImageSizeName;
            }
            set
            {
                ImageTargetSettings.ImageSizeName = value;
            }
        }

        /// <summary>
        /// Gets the HTML title attribute for the link.
        /// </summary>
        public string LinkTitle
        {
            get
            {
                return ImageTargetSettings.LinkTitle;
            }
        }

        /// <summary>
        /// Gets the HTML rel attribute for the link.
        /// </summary>
        public string LinkRel
        {
            get
            {
                return ImageTargetSettings.LinkRel;
            }
        }

        public void UpdateImageLinkOptions(string title, string rel, bool newWindow)
        {
            ImageTargetSettings.UpdateImageLinkOptions(title, rel, newWindow);
        }

        /// <summary>
        /// Returns the size of the linked image for IMAGE-based link targets
        /// </summary>
        public Size LinkTargetImageSize
        {
            get
            {
                //if(LinkTarget != LinkTargetType.IMAGE)
                //	throw new Exception("LinkTargetUrl property not supported for link targets with type: " + LinkTarget.ToString());
                return ImageTargetSettings.ImageSize;
            }
            set
            {
                ImageTargetSettings.ImageSize = value;
            }
        }

        public string DhtmlImageViewer
        {
            get { return ImageTargetSettings.DhtmlImageViewer; }
            set { ImageTargetSettings.DhtmlImageViewer = value; }
        }

        public string UploadServiceId
        {
            get
            {
                return uploadServiceId;
            }
            set
            {
                uploadServiceId = value;
            }
        }
        string uploadServiceId;

        public BlogPostSettingsBag UploadSettings
        {
            get
            {
                return uploadSettings;
            }
            set
            {
                uploadSettings = value;
            }
        }
        BlogPostSettingsBag uploadSettings;

        public ImageDecoratorsList ImageDecorators
        {
            get { return imageDecorators; }
            set
            {
                imageDecorators = value;
                targetDecoratorSettings = null;
                _inlineImageSettings = null;
            }
        }
        private ImageDecoratorsList imageDecorators;

        private HtmlImageTargetDecoratorSettings ImageTargetSettings
        {
            get
            {
                if (targetDecoratorSettings == null)
                {
                    targetDecoratorSettings = new HtmlImageTargetDecoratorSettings(ImageDecorators.GetImageDecoratorSettings(HtmlImageTargetDecorator.Id), _imgElement);
                }
                return targetDecoratorSettings;
            }
        }
        private HtmlImageTargetDecoratorSettings targetDecoratorSettings;

        private HtmlImageResizeDecoratorSettings InlineImageSettings
        {
            get
            {
                if (_inlineImageSettings == null)
                {
                    _inlineImageSettings = new HtmlImageResizeDecoratorSettings(ImageDecorators.GetImageDecoratorSettings(HtmlImageResizeDecorator.Id), _imgElement);
                }
                return _inlineImageSettings;
            }
        }
        private HtmlImageResizeDecoratorSettings _inlineImageSettings;

        private HtmlAlignDecoratorSettings InlineAlignmentSettings
        {
            get
            {
                if (_inlineAlignmentSettings == null)
                {
                    _inlineAlignmentSettings = new HtmlAlignDecoratorSettings(ImageDecorators.GetImageDecoratorSettings(HtmlAlignDecorator.Id), _imgElement);
                }
                return _inlineAlignmentSettings;
            }
        }
        private HtmlAlignDecoratorSettings _inlineAlignmentSettings;

        private HtmlMarginDecoratorSettings InlineMarginSettings
        {
            get
            {
                if (_inlineMarginSettings == null)
                {
                    _inlineMarginSettings = new HtmlMarginDecoratorSettings(ImageDecorators.GetImageDecoratorSettings(HtmlMarginDecorator.Id), _imgElement);
                }
                return _inlineMarginSettings;
            }
        }
        private HtmlMarginDecoratorSettings _inlineMarginSettings;

        public IHTMLElement ImgElement
        {
            get { return _imgElement; }
            set { _imgElement = value; }
        }
        private IHTMLElement _imgElement;

        public RotateFlipType ImageRotation
        {
            get { return InlineImageSettings.Rotation; }
            set
            {
                bool oldRotated90 = ImageUtils.IsRotated90(InlineImageSettings.Rotation);
                bool newRotated90 = ImageUtils.IsRotated90(value);
                InlineImageSettings.Rotation = value;
                if (oldRotated90 != newRotated90)
                {
                    //then the image axis is turned 90 degrees, so reverse the image width/height settings
                    Size oldInlineSize = InlineImageSize;
                    InlineImageSize = new Size(oldInlineSize.Height, oldInlineSize.Width);
                    Size oldLinkedSize = LinkTargetImageSize;
                    LinkTargetImageSize = new Size(oldLinkedSize.Height, oldLinkedSize.Width);
                }
            }
        }

        public Bitmap Image
        {
            get { return (Bitmap)Bitmap.FromFile(this.ImageSourceUri.LocalPath); }
        }

        public float? EnforcedAspectRatio
        {
            get { return imageDecorators.EnforcedAspectRatio; }
        }

        /// <summary>
        /// Resets the image decorators back to their defaults.
        /// </summary>
        public void ResetImageSettings(ImageDecoratorsList defaultDecorators)
        {
            //preserve the HTML rotation setting
            RotateFlipType oldRotation = ImageRotation;

            //reset the image decorators
            ImageDecorators = defaultDecorators;
            targetDecoratorSettings = null;
            _inlineImageSettings = null;

            //restore the image rotation setting.
            InlineImageSettings.Rotation = oldRotation;
        }

        internal void RemoveLinkTarget()
        {
            ImageTargetSettings.RemoveImageLink();
        }
    }
    public enum LinkTargetType { NONE, IMAGE, URL };
}
