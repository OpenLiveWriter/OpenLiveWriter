// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using mshtml;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal interface IImagePropertyEditingContext
    {
        IHTMLImgElement SelectedImage { get; }
        event ImagePropertyEventHandler ImagePropertyChanged;
        ImagePropertiesInfo ImagePropertiesInfo { get; set; }
    }

    /// <summary>
    /// Summary description for ImagePropertiesHandler.
    /// </summary>
    internal class ImageEditingPropertyHandler
    {
        ImageInsertHandler _imageInsertHandler;
        IImagePropertyEditingContext _propertyEditingContext;
        IBlogPostImageEditingContext _editorContext;

        internal ImageEditingPropertyHandler(IImagePropertyEditingContext propertyEditingContext, CreateFileCallback createFileCallback, IBlogPostImageEditingContext imageEditingContext)
        {
            _propertyEditingContext = propertyEditingContext;
            _imageInsertHandler = new ImageInsertHandler();
            _editorContext = imageEditingContext;
        }

        public void RefreshView()
        {
            _propertyEditingContext.ImagePropertyChanged -= new ImagePropertyEventHandler(imageProperties_ImagePropertyChanged);

            IHTMLImgElement imgElement = ImgElement as IHTMLImgElement;
            if (imgElement != null)
                _propertyEditingContext.ImagePropertiesInfo = GetImagePropertiesInfo(imgElement, _editorContext);
            else
                _propertyEditingContext.ImagePropertiesInfo = null;

            _propertyEditingContext.ImagePropertyChanged += new ImagePropertyEventHandler(imageProperties_ImagePropertyChanged);
        }

        public static ImagePropertiesInfo GetImagePropertiesInfo(IHTMLImgElement imgElement, IBlogPostImageEditingContext editorContext)
        {
            IHTMLElement imgHtmlElement = (IHTMLElement)imgElement;
            string imgSrc = imgHtmlElement.getAttribute("src", 2) as string;
            BlogPostImageData imageData = null;
            try
            {
                imageData = BlogPostImageDataList.LookupImageDataByInlineUri(editorContext.ImageList, new Uri(imgSrc));
            }
            catch (UriFormatException)
            {
                //this URI is probably relative web URL, so extract the image src letting the
                //DOM fill in the full URL for us based on the base URL.
                imgSrc = imgHtmlElement.getAttribute("src", 0) as string;
            }

            ImagePropertiesInfo info;
            if (imageData != null && imageData.GetImageSourceFile() != null)
            {
                //clone the image data to the sidebar doesn't change it (required for preserving image undo/redo state)
                imageData = (BlogPostImageData)imageData.Clone();
                //this is an attached local image
                info = new BlogPostImagePropertiesInfo(imageData, new ImageDecoratorsList(editorContext.DecoratorsManager, imageData.ImageDecoratorSettings));
                info.ImgElement = imgHtmlElement;
            }
            else
            {
                //this is not an attached local image, so treat as a web image
                ImageDecoratorsList remoteImageDecoratorsList = new ImageDecoratorsList(editorContext.DecoratorsManager, new BlogPostSettingsBag());
                remoteImageDecoratorsList.AddDecorator(editorContext.DecoratorsManager.GetDefaultRemoteImageDecorators());

                //The source image size is unknown, so calculate the actual image size by removing
                //the size attributes, checking the size, and then placing the size attributes back
                string oldHeight = imgHtmlElement.getAttribute("height", 2) as string;
                string oldWidth = imgHtmlElement.getAttribute("width", 2) as string;
                imgHtmlElement.removeAttribute("width", 0);
                imgHtmlElement.removeAttribute("height", 0);
                int width = imgElement.width;
                int height = imgElement.height;

                if (!String.IsNullOrEmpty(oldHeight))
                    imgHtmlElement.setAttribute("height", oldHeight, 0);
                if (!String.IsNullOrEmpty(oldWidth))
                    imgHtmlElement.setAttribute("width", oldWidth, 0);
                Uri infoUri;
                if (Uri.TryCreate(imgSrc, UriKind.Absolute, out infoUri))
                {
                    info = new ImagePropertiesInfo(infoUri, new Size(width, height), remoteImageDecoratorsList);
                }
                else
                {
                    info = new ImagePropertiesInfo(new Uri("http://www.example.com"), new Size(width, height), remoteImageDecoratorsList);
                }
                info.ImgElement = imgHtmlElement;

                // Sets the correct inline image size and image size name for the remote image.
                if (!String.IsNullOrEmpty(oldWidth) && !String.IsNullOrEmpty(oldHeight))
                {
                    int inlineWidth, inlineHeight;
                    if (Int32.TryParse(oldWidth, NumberStyles.Integer, CultureInfo.InvariantCulture, out inlineWidth) &&
                        Int32.TryParse(oldHeight, NumberStyles.Integer, CultureInfo.InvariantCulture, out inlineHeight))
                    {
                        info.InlineImageSize = new Size(inlineWidth, inlineHeight);
                    }
                }

                // Sets the correct border style for the remote image.
                if (new HtmlBorderDecoratorSettings(imgHtmlElement).InheritBorder)
                {
                    if (!info.ImageDecorators.ContainsDecorator(HtmlBorderDecorator.Id))
                        info.ImageDecorators.AddDecorator(HtmlBorderDecorator.Id);
                }
                else if (new NoBorderDecoratorSettings(imgHtmlElement).NoBorder)
                {
                    if (!info.ImageDecorators.ContainsDecorator(NoBorderDecorator.Id))
                        info.ImageDecorators.AddDecorator(NoBorderDecorator.Id);
                }
            }

            //transfer image data properties
            if (imageData != null)
            {
                info.UploadSettings = imageData.UploadInfo.Settings;
                info.UploadServiceId = imageData.UploadInfo.ImageServiceId;
                if (info.UploadServiceId == null)
                {
                    info.UploadServiceId = editorContext.ImageServiceId;
                }
            }

            return info;
        }

        private IHTMLElement ImgElement
        {
            get
            {
                return _propertyEditingContext.SelectedImage as IHTMLElement;
            }
        }

        private void imageProperties_ImagePropertyChanged(object source, ImagePropertyEvent evt)
        {
            if (ImgElement != null)
            {
                switch (evt.PropertyType)
                {
                    case ImagePropertyType.Source:
                    case ImagePropertyType.InlineSize:
                    case ImagePropertyType.Decorators:
                        UpdateImageSource(evt.ImageProperties, evt.InvocationSource);
                        break;
                    default:
                        Debug.Fail("Unsupported image property type update: " + evt.PropertyType);
                        break;
                }
            }
        }

        private void UpdateImageSource(ImagePropertiesInfo imgProperties, ImageDecoratorInvocationSource invocationSource)
        {
            UpdateImageSource(imgProperties, ImgElement, _editorContext, _imageInsertHandler, invocationSource);
        }

        internal static void UpdateImageSource(ImagePropertiesInfo imgProperties, IHTMLElement imgElement, IBlogPostImageEditingContext editorContext, ImageInsertHandler imageInsertHandler, ImageDecoratorInvocationSource invocationSource)
        {
            ISupportingFile oldImageFile = null;
            try
            {
                oldImageFile = editorContext.SupportingFileService.GetFileByUri(new Uri((string)imgElement.getAttribute("src", 2)));
            }
            catch (UriFormatException) { }
            if (oldImageFile != null) //then this is a known supporting image file
            {
                using (new WaitCursor())
                {
                    BlogPostImageData imageData = BlogPostImageDataList.LookupImageDataByInlineUri(editorContext.ImageList, oldImageFile.FileUri);
                    if (imageData != null)
                    {
                        //Create a new ImageData object based on the image data attached to the current image src file.
                        BlogPostImageData newImageData = (BlogPostImageData)imageData.Clone();

                        //initialize some handlers for creating files based on the image's existing ISupportingFile objects
                        //This is necessary so that the new image files are recognized as being updates to an existing image
                        //which allows the updates to be re-uploaded back to the same location.
                        CreateImageFileHandler inlineFileCreator = new CreateImageFileHandler(editorContext.SupportingFileService,
                                                                                              newImageData.InlineImageFile != null ? newImageData.InlineImageFile.SupportingFile : null);
                        CreateImageFileHandler linkedFileCreator = new CreateImageFileHandler(editorContext.SupportingFileService,
                                                                                              newImageData.LinkedImageFile != null ? newImageData.LinkedImageFile.SupportingFile : null);

                        //re-write the image files on disk using the latest settings
                        imageInsertHandler.WriteImages(imgProperties, true, invocationSource, new CreateFileCallback(inlineFileCreator.CreateFileCallback), new CreateFileCallback(linkedFileCreator.CreateFileCallback), editorContext.EditorOptions);

                        //update the ImageData file references
                        Size imageSizeWithBorder = imgProperties.InlineImageSizeWithBorder;

                        //force a refresh of the image size values in the DOM by setting the new size attributes
                        imgElement.setAttribute("width", imageSizeWithBorder.Width, 0);
                        imgElement.setAttribute("height", imageSizeWithBorder.Height, 0);

                        newImageData.InlineImageFile.SupportingFile = inlineFileCreator.ImageSupportingFile;
                        newImageData.InlineImageFile.Height = imageSizeWithBorder.Height;
                        newImageData.InlineImageFile.Width = imageSizeWithBorder.Width;
                        if (imgProperties.LinkTarget == LinkTargetType.IMAGE)
                        {
                            newImageData.LinkedImageFile = new ImageFileData(linkedFileCreator.ImageSupportingFile, imgProperties.LinkTargetImageSize.Width, imgProperties.LinkTargetImageSize.Height, ImageFileRelationship.Linked);
                        }
                        else
                            newImageData.LinkedImageFile = null;

                        //assign the image decorators applied during WriteImages
                        //Note: this is a clone so the sidebar doesn't affect the decorator values for the newImageData image src file
                        newImageData.ImageDecoratorSettings = (BlogPostSettingsBag)imgProperties.ImageDecorators.SettingsBag.Clone();

                        //update the upload settings
                        newImageData.UploadInfo.ImageServiceId = imgProperties.UploadServiceId;

                        //save the new image data in the image list
                        editorContext.ImageList.AddImage(newImageData);
                    }
                    else
                        Debug.Fail("imageData could not be located");
                }
            }

            if (imgProperties.LinkTarget == LinkTargetType.NONE)
            {
                imgProperties.RemoveLinkTarget();
            }
        }

        //Utility for an updating image file based on a particular ISupportingFile.
        private class CreateImageFileHandler
        {
            public ISupportingFile ImageSupportingFile;
            ISupportingFileService _fileService;
            public CreateImageFileHandler(ISupportingFileService fileService, ISupportingFile supportingFile)
            {
                _fileService = fileService;
                ImageSupportingFile = supportingFile;
            }

            public string CreateFileCallback(string requestedFileName)
            {
                if (ImageSupportingFile == null)
                    ImageSupportingFile = _fileService.CreateSupportingFile(requestedFileName, new MemoryStream(new byte[0]));
                else
                    ImageSupportingFile = ImageSupportingFile.UpdateFile(new MemoryStream(new byte[0]), requestedFileName);
                return ImageSupportingFile.FileUri.LocalPath;
            }
        }
    }

    public delegate void ImagePropertyEventHandler(object source, ImagePropertyEvent evt);
    public enum ImagePropertyType { Source, InlineSize, Decorators };
    public class ImagePropertyEvent : EventArgs
    {

        public ImagePropertiesInfo ImageProperties
        {
            get
            {
                return _imageProperties;
            }
        }
        private readonly ImagePropertiesInfo _imageProperties;

        public readonly ImagePropertyType PropertyType;
        public readonly ImageDecoratorInvocationSource InvocationSource;
        public ImagePropertyEvent(ImagePropertyType propertyType, ImagePropertiesInfo imgProperties, ImageDecoratorInvocationSource invocationSource)
        {
            PropertyType = propertyType;
            _imageProperties = imgProperties;
            InvocationSource = invocationSource;
        }
    }
}
