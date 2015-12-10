// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for Decorator.
    /// </summary>
    public class HtmlImageTargetDecorator : IImageDecorator, IImageDecoratorDefaultSettingsCustomizer
    {
        public const string Id = "ImageTarget";
        public HtmlImageTargetDecorator()
        {
        }

        public void Decorate(ImageDecoratorContext context)
        {
            HtmlImageTargetDecoratorSettings settings = new HtmlImageTargetDecoratorSettings(context.Settings, context.ImgElement);
            if (context.InvocationSource == ImageDecoratorInvocationSource.InitialInsert ||
               context.InvocationSource == ImageDecoratorInvocationSource.Reset)
            {
                //set the default link target type.
                //settings.LinkTarget = settings.DefaultLinkTarget;

                //the default size is a scaled version of the image based on the default inline size constraints.
                Size defaultSizeBounds = settings.DefaultTargetBoundsSize;

                settings.BaseSize = context.Image.Size;

                //calculate the base image size to scale from.  If the image is rotated 90 degrees, then switch the height/width
                Size baseImageSize = context.Image.Size;
                if (ImageUtils.IsRotated90(context.ImageRotation))
                    baseImageSize = new Size(baseImageSize.Height, baseImageSize.Width);

                //calculate and set the scaled default size using the defaultSizeBounds
                //Note: if the image dimensions are smaller than the default, don't scale that dimension (bug 419446)
                Size defaultSize = ImageUtils.GetScaledImageSize(Math.Min(baseImageSize.Width, defaultSizeBounds.Width), Math.Min(baseImageSize.Height, defaultSizeBounds.Height), baseImageSize);
                settings.ImageSize = defaultSize;
                settings.ImageSizeName = settings.DefaultTargetBoundsSizeName;
            }
            else if (settings.BaseSizeChanged(context.Image) && context.ImageEmbedType == ImageEmbedType.Linked)
            {
                Size newBaseSize = context.Image.Size;
                settings.ImageSize = HtmlImageResizeDecorator.AdjustImageSizeForNewBaseSize(false, settings, newBaseSize, context.ImageRotation, context);
                settings.BaseSize = newBaseSize;
            }

            if (context.InvocationSource == ImageDecoratorInvocationSource.Reset)
            {
                //set the initial link options
                settings.LinkOptions = settings.DefaultLinkOptions;
            }

            //this decorator only applies to linked images.
            if (context.ImageEmbedType == ImageEmbedType.Linked)
            {
                Size imageSize = settings.ImageSize;

                //resize the image and update the image used by the context.
                Bitmap bitmap = HtmlImageResizeDecorator.ResizeImage(context.Image, imageSize, context.ImageRotation);

                context.Image = bitmap;
                if (settings.ImageSize != bitmap.Size)
                    settings.ImageSize = bitmap.Size;
            }
        }

        public ImageDecoratorEditor CreateEditor(CommandManager commandManager)
        {
            return new HtmlImageTargetEditor();
        }

        #region IImageDecoratorDefaultSettingsCustomizer Members

        void IImageDecoratorDefaultSettingsCustomizer.CustomizeDefaultSettingsBeforeSave(ImageDecoratorEditorContext context, IProperties defaultSettings)
        {
            HtmlImageTargetDecoratorSettings defaultTargetSettings = new HtmlImageTargetDecoratorSettings(defaultSettings, context.ImgElement);
            HtmlImageTargetDecoratorSettings targetSettings = new HtmlImageTargetDecoratorSettings(context.Settings, context.ImgElement);

            //save a reasonable value for the default link target.
            //If the link target is a URL, default to NONE since the user clearly doesn't want to preserve
            //the URL currently associated with the image for all future images
            if (defaultTargetSettings.LinkTarget != LinkTargetType.URL)
                defaultTargetSettings.DefaultLinkTarget = defaultTargetSettings.LinkTarget;
            else
                defaultTargetSettings.DefaultLinkTarget = LinkTargetType.NONE;

            defaultTargetSettings.DefaultLinkOptions = targetSettings.LinkOptions;
            defaultTargetSettings.DefaultTargetBoundsSizeName = targetSettings.ImageSizeName;
            defaultTargetSettings.DefaultTargetBoundsSize = targetSettings.ImageSize;
        }

        #endregion
    }

    internal class HtmlImageTargetDecoratorSettings : IResizeDecoratorSettings
    {
        private const string WIDTH = "ImageWidth";
        private const string HEIGHT = "ImageHeight";
        private const string BOUNDS = "ImageBoundsSize";
        private const string BASE_WIDTH = "BaseWidth";
        private const string BASE_HEIGHT = "BaseHeight";
        private const string TARGET_TYPE = "TargetType";
        private const string DEFAULT_TARGET_TYPE = "DefaultTargetType";
        private const string DEFAULT_OPEN_NEW_WINDOW = "DefaultOpenNewWindow";
        private const string DEFAULT_TARGET_WIDTH = "DefaultTargetWidth";
        private const string DEFAULT_TARGET_HEIGHT = "DefaultTargetHeight";
        private const string DEFAULT_TARGET_SIZE_NAME = "DefaultTargetSizeName";
        private const string DHTML_IMAGE_VIEWER = "DhtmlImageViewer";
        private const string DEFAULT_USE_IMAGE_VIEWER = "DefaultUseImageViewer";
        private const string DEFAULT_IMAGE_VIEWER_GROUP = "DefaultImageViewerGroup";

        //  Anything in this list will be removed from the properties when the image reference is fixed
        //  because ti is being synced with a posted edited outside of writer.
        public static readonly string[] ImageReferenceFixedStaleProperties = new string[1] { TARGET_TYPE };

        private readonly IProperties Settings;
        private readonly IHTMLElement ImgElement;
        public HtmlImageTargetDecoratorSettings(IProperties settings, IHTMLElement imgElement)
        {
            Settings = settings;
            ImgElement = imgElement;
        }

        public Size ImageSize
        {
            get
            {
                IHTMLImgElement imgElement = (IHTMLImgElement)ImgElement;
                int width = Settings.GetInt(WIDTH, imgElement.width);
                int height = Settings.GetInt(HEIGHT, imgElement.height);
                return new Size(width, height);
            }
            set
            {
                Settings.SetInt(WIDTH, value.Width);
                Settings.SetInt(HEIGHT, value.Height);
            }
        }

        /// <summary>
        /// The maximum bounds that were allowed when determining the current image size.
        /// This value is used to decide what the best initial size should be for the image if the current
        /// size is saved as the default size.  Rather than forcing all future images to be exactly the current
        /// size, the named size associated with the bounds can be used for more flexibility.
        /// </summary>
        public ImageSizeName ImageSizeName
        {
            get
            {
                ImageSizeName bounds =
                    (ImageSizeName)ImageSizeName.Parse(
                    typeof(ImageSizeName),
                    Settings.GetString(BOUNDS, ImageSizeName.Full.ToString()));

                return bounds;
            }
            set
            {
                Settings.SetString(BOUNDS, value.ToString());
            }
        }

        /// <summary>
        /// Defines the type of target for the image.
        /// </summary>
        public LinkTargetType LinkTarget
        {
            get
            {
                LinkTargetType linkTargetType = uninitializedDefaultLinkTarget;

                string linkTarget = Settings.GetString(TARGET_TYPE, null);
                if (linkTarget != null)
                {
                    linkTargetType = (LinkTargetType)LinkTargetType.Parse(typeof(LinkTargetType), linkTarget);
                }
                else
                {
                    //The link target type is completely uninitialized (no one has set it, and no image decorator defaults have been saved).
                    //Examine the link values from the DOM to figure out the correct value for this property.
                    //If the image is surrounded by an anchor to a remote image, then set the target type to link.
                    //If the image is not surrounded by an anchor then set the target type to None.
                    //Else leave as the default (which is a local image).
                    string linkTargetUrl = LinkTargetUrl;
                    if (linkTargetUrl == null)
                        linkTargetType = LinkTargetType.NONE;
                    else if (!UrlHelper.IsFileUrl(linkTargetUrl))
                    {
                        linkTargetType = LinkTargetType.URL;
                    }
                }

                return linkTargetType;
            }
            set
            {
                Settings.SetString(TARGET_TYPE, value.ToString());
            }
        }

        public LinkTargetType DefaultLinkTarget
        {
            get
            {
                string linkTarget = Settings.GetString(DEFAULT_TARGET_TYPE, uninitializedDefaultLinkTarget.ToString());
                LinkTargetType linkTargetType = (LinkTargetType)LinkTargetType.Parse(typeof(LinkTargetType), linkTarget);
                return linkTargetType;
            }
            set
            {
                Settings.SetString(DEFAULT_TARGET_TYPE, value.ToString());
            }
        }

        private readonly LinkTargetType uninitializedDefaultLinkTarget = LinkTargetType.IMAGE;

        internal Size DefaultTargetBoundsSize
        {
            get
            {
                ImageSizeName boundsSize = DefaultTargetBoundsSizeName;
                Size defaultBoundsSize;
                if (boundsSize != ImageSizeName.Custom)
                    defaultBoundsSize = ImageSizeHelper.GetSizeConstraints(boundsSize);
                else
                {
                    int defaultWidth = Settings.GetInt(DEFAULT_TARGET_WIDTH, 300);
                    int defaultHeight = Settings.GetInt(DEFAULT_TARGET_HEIGHT, 300);
                    defaultBoundsSize = new Size(defaultWidth, defaultHeight);
                }
                return defaultBoundsSize;
            }
            set
            {
                Settings.SetInt(DEFAULT_TARGET_WIDTH, value.Width);
                Settings.SetInt(DEFAULT_TARGET_HEIGHT, value.Height);
            }
        }

        internal ImageSizeName DefaultTargetBoundsSizeName
        {
            get
            {
                ImageSizeName bounds =
                    (ImageSizeName)ImageSizeName.Parse(
                    typeof(ImageSizeName),
                    Settings.GetString(DEFAULT_TARGET_SIZE_NAME, ImageSizeName.Large.ToString()));

                return bounds;
            }
            set
            {
                Settings.SetString(DEFAULT_TARGET_SIZE_NAME, value.ToString());
            }
        }

        Size IResizeDecoratorSettings.DefaultBoundsSize
        {
            get { return DefaultTargetBoundsSize; }
        }

        ImageSizeName IResizeDecoratorSettings.DefaultBoundsSizeName
        {
            get { return DefaultTargetBoundsSizeName; }
        }

        /// <summary>
        /// Get/Set the url for URL-based link targets.
        /// </summary>
        public string LinkTargetUrl
        {
            get
            {
                IHTMLElement anchorElement = GetAnchorElement();
                if (anchorElement != null)
                    return (string)anchorElement.getAttribute("href", 2);
                return null;
            }
            set
            {
                UpdateImageLink(value, ImgElement, DefaultLinkOptions);
            }
        }

        public void UpdateImageLinkOptions(string title, string rel, bool newWindow)
        {
            IHTMLElement anchorElement = GetAnchorElement();

            IHTMLAnchorElement htmlAnchorElement = (anchorElement as IHTMLAnchorElement);
            if (htmlAnchorElement != null)
            {
                //set the default target attribute for the new element
                string target = newWindow ? "_blank" : null;
                if (htmlAnchorElement.target != target) //don't set the target to null if its already null (avoids adding empty target attr)
                    htmlAnchorElement.target = target;
                if (title != String.Empty)
                {
                    anchorElement.setAttribute("title", title, 0);
                }
                else
                {
                    anchorElement.removeAttribute("title", 0);
                }
                if (rel != String.Empty)
                {
                    anchorElement.setAttribute("rel", rel, 0);
                }
                else
                {
                    anchorElement.removeAttribute("rel", 0);
                }
            }
        }

        public string LinkTitle
        {
            get
            {
                IHTMLElement anchorElement = GetAnchorElement();
                if (anchorElement != null)
                {
                    if (anchorElement.title != null)
                        return anchorElement.title;
                    else
                        return String.Empty;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public string LinkRel
        {
            get
            {
                IHTMLElement anchorElement = GetAnchorElement();

                IHTMLAnchorElement htmlAnchorElement = (anchorElement as IHTMLAnchorElement);
                if (htmlAnchorElement != null)
                {
                    if (htmlAnchorElement.rel != null)
                        return htmlAnchorElement.rel;
                    else
                        return String.Empty;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public ILinkOptions LinkOptions
        {
            get
            {
                bool openInNewWindow = false;
                bool useImageViewer = false;
                string imageViewerGroupName = null;
                IHTMLElement anchorElement = GetAnchorElement();
                if (anchorElement != null)
                {
                    IHTMLAnchorElement htmlAnchorElement = (IHTMLAnchorElement)anchorElement;
                    openInNewWindow = htmlAnchorElement.target != null;
                    ImageViewer viewer = DhtmlImageViewers.GetImageViewer(DhtmlImageViewer);
                    if (viewer != null)
                        viewer.Detect(htmlAnchorElement, ref useImageViewer, ref imageViewerGroupName);
                }
                return new LinkOptions(openInNewWindow, useImageViewer, imageViewerGroupName);
            }
            set
            {
                IHTMLElement anchorElement = GetAnchorElement();
                if (anchorElement != null)
                {
                    string target = value.ShowInNewWindow ? "_blank" : null;
                    IHTMLAnchorElement htmlAnchorElement = (anchorElement as IHTMLAnchorElement);
                    if (htmlAnchorElement != null)
                    {
                        if (target == null)
                            ((IHTMLElement)htmlAnchorElement).removeAttribute("target", 0);
                        else
                            htmlAnchorElement.target = target;
                    }

                    ImageViewer viewer = DhtmlImageViewers.GetImageViewer(DhtmlImageViewer);
                    if (viewer != null)
                    {
                        if (value.UseImageViewer)
                            viewer.Apply(htmlAnchorElement, value.ImageViewerGroupName);
                        else
                            viewer.Remove(htmlAnchorElement);
                    }

                    //save the value as the default for this image so that we can properly restore the options
                    //if the user toggles the target and causes the anchor to be removed-then-added
                    DefaultLinkOptions = value;
                }
            }
        }

        public ILinkOptions DefaultLinkOptions
        {
            get
            {
                bool openInNewWindow = Settings.GetBoolean(DEFAULT_OPEN_NEW_WINDOW, false);
                bool useImageViewer = Settings.GetBoolean(DEFAULT_USE_IMAGE_VIEWER, true);
                string groupName = Settings.GetString(DEFAULT_IMAGE_VIEWER_GROUP, null);
                return new LinkOptions(openInNewWindow, useImageViewer, groupName);
            }
            set
            {
                Settings.SetBoolean(DEFAULT_OPEN_NEW_WINDOW, value.ShowInNewWindow);
                Settings.SetBoolean(DEFAULT_USE_IMAGE_VIEWER, value.UseImageViewer);
                Settings.SetString(DEFAULT_IMAGE_VIEWER_GROUP, value.ImageViewerGroupName);
            }
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

        public string DhtmlImageViewer
        {
            get { return Settings.GetString(DHTML_IMAGE_VIEWER, null); }
            set
            {
                if (value != DhtmlImageViewer)
                {
                    // Unload the existing viewer, if any exists
                    ImageViewer viewer = DhtmlImageViewers.GetImageViewer(DhtmlImageViewer);
                    if (viewer != null)
                    {
                        IHTMLAnchorElement anchor = GetAnchorElement() as IHTMLAnchorElement;
                        if (anchor != null)
                            viewer.Remove(anchor);
                    }

                    Settings.SetString(DHTML_IMAGE_VIEWER, value);
                }
            }
        }

        public bool BaseSizeChanged(Bitmap image)
        {
            Size? baseSize = BaseSize;
            if (baseSize == null)
                return true;
            return !baseSize.Equals(image.Size);
        }

        internal IHTMLElement GetAnchorElement()
        {
            IHTMLElement parentElement = ImgElement.parentElement;
            while (parentElement != null)
            {
                if (parentElement is IHTMLAnchorElement)
                    return parentElement;
                parentElement = parentElement.parentElement;
            }
            return null;
        }

        private void UpdateImageLink(string href, IHTMLElement ImgElement, ILinkOptions defaultOptions)
        {
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)ImgElement.document);
            IHTMLElement parentElement = ImgElement.parentElement;
            if (!(parentElement is IHTMLAnchorElement))
            {
                parentElement = markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_A, null);
                MarkupRange range = markupServices.CreateMarkupRange();
                range.MoveToElement(ImgElement, true);
                markupServices.InsertElement(parentElement, range.Start, range.End);

                //set the default target attribute for the new element
                string target = defaultOptions.ShowInNewWindow ? "_blank" : null;
                IHTMLAnchorElement htmlAnchorElement = (parentElement as IHTMLAnchorElement);
                if (htmlAnchorElement.target != target) //don't set the target to null if its already null (avoids adding empty target attr)
                    htmlAnchorElement.target = target;

                ImageViewer viewer = DhtmlImageViewers.GetImageViewer(DhtmlImageViewer);
                if (viewer != null)
                {
                    if (defaultOptions.UseImageViewer)
                        viewer.Apply(htmlAnchorElement, defaultOptions.ImageViewerGroupName);
                }
            }
            parentElement.setAttribute("href", href, 0);
        }

        public void RemoveImageLink()
        {
            IHTMLElement parentElement = ImgElement.parentElement;
            if (parentElement is IHTMLAnchorElement)
            {
                ((IHTMLDOMNode)parentElement).removeNode(false);
            }
        }
    }
}
