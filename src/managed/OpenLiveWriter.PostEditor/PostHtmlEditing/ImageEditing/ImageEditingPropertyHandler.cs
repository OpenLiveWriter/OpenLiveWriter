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
using OpenLiveWriter.HtmlEditor;
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

        /// <summary>
        /// Ensure subscription to ImagePropertyChanged for WebView2 mode.
        /// Unlike RefreshView(), this doesn't try to get ImagePropertiesInfo from MSHTML element.
        /// </summary>
        public void EnsureWebView2Subscription()
        {
            // Unsubscribe first to avoid duplicate subscriptions
            _propertyEditingContext.ImagePropertyChanged -= new ImagePropertyEventHandler(imageProperties_ImagePropertyChanged);
            _propertyEditingContext.ImagePropertyChanged += new ImagePropertyEventHandler(imageProperties_ImagePropertyChanged);
            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] ImageEditingPropertyHandler: WebView2 subscription ensured");
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

        /// <summary>
        /// Create ImagePropertiesInfo from a WebView2 selected image.
        /// </summary>
        public static ImagePropertiesInfo GetImagePropertiesInfoFromWebView2(
            ISelectedImage selectedImage, 
            IHtmlImageElement htmlImageElement,
            IBlogPostImageEditingContext editorContext)
        {
            if (selectedImage == null || htmlImageElement == null)
                return null;

            string imgSrc = selectedImage.Src;
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] GetImagePropertiesInfoFromWebView2: Original src={imgSrc}");
            
            // Normalize file:// URLs to proper format
            if (imgSrc != null && imgSrc.StartsWith("https://olw-local-"))
            {
                // Convert back from our custom scheme to file://
                // https://olw-local-c/path -> file:///C:/path
                string driveLetter = imgSrc.Substring("https://olw-local-".Length, 1);
                string path = imgSrc.Substring("https://olw-local-".Length + 1);
                imgSrc = $"file:///{driveLetter}:{path}";
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] GetImagePropertiesInfoFromWebView2: Converted to={imgSrc}");
            }
            
            BlogPostImageData imageData = null;
            try
            {
                if (!string.IsNullOrEmpty(imgSrc))
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] GetImagePropertiesInfoFromWebView2: Looking up URI={new Uri(imgSrc)}");
                    imageData = BlogPostImageDataList.LookupImageDataByInlineUri(editorContext.ImageList, new Uri(imgSrc));
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] GetImagePropertiesInfoFromWebView2: Lookup result={(imageData != null ? "FOUND" : "NOT FOUND")}");
                }
            }
            catch (UriFormatException ex)
            {
                // URI format error - treat as remote image
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] GetImagePropertiesInfoFromWebView2: URI format error: {ex.Message}");
            }

            ImagePropertiesInfo info;
            if (imageData != null && imageData.GetImageSourceFile() != null)
            {
                // Clone the image data so sidebar doesn't change it (preserves undo/redo state)
                imageData = (BlogPostImageData)imageData.Clone();
                // This is an attached local image
                info = new BlogPostImagePropertiesInfo(imageData, new ImageDecoratorsList(editorContext.DecoratorsManager, imageData.ImageDecoratorSettings));
                info.HtmlImageElement = htmlImageElement;
            }
            else
            {
                // This is not an attached local image, so treat as a web image
                ImageDecoratorsList remoteImageDecoratorsList = new ImageDecoratorsList(editorContext.DecoratorsManager, new BlogPostSettingsBag());
                remoteImageDecoratorsList.AddDecorator(editorContext.DecoratorsManager.GetDefaultRemoteImageDecorators());

                // For WebView2, we can get natural dimensions directly
                int width = selectedImage.NaturalWidth > 0 ? selectedImage.NaturalWidth : selectedImage.Width;
                int height = selectedImage.NaturalHeight > 0 ? selectedImage.NaturalHeight : selectedImage.Height;

                Uri infoUri;
                if (Uri.TryCreate(imgSrc, UriKind.Absolute, out infoUri))
                {
                    info = new ImagePropertiesInfo(infoUri, new Size(width, height), remoteImageDecoratorsList);
                }
                else
                {
                    info = new ImagePropertiesInfo(new Uri("http://www.example.com"), new Size(width, height), remoteImageDecoratorsList);
                }
                info.HtmlImageElement = htmlImageElement;

                // Set the correct inline image size
                if (selectedImage.Width > 0 && selectedImage.Height > 0)
                {
                    info.InlineImageSize = new Size(selectedImage.Width, selectedImage.Height);
                }
            }

            // Transfer image data properties
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
            // MSHTML path
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
            // WebView2 path - use HtmlImageElement abstraction
            else if (evt.ImageProperties?.HtmlImageElement != null)
            {
                Debug.WriteLine($"[OLW-DEBUG] imageProperties_ImagePropertyChanged: WebView2 path, propertyType={evt.PropertyType}");
                switch (evt.PropertyType)
                {
                    case ImagePropertyType.Source:
                    case ImagePropertyType.InlineSize:
                    case ImagePropertyType.Decorators:
                        UpdateImageSourceWebView2(evt.ImageProperties, _editorContext, _imageInsertHandler, evt.InvocationSource);
                        break;
                    default:
                        Debug.WriteLine($"[OLW-DEBUG] Unsupported image property type for WebView2: {evt.PropertyType}");
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

        /// <summary>
        /// WebView2 version of UpdateImageSource that uses IHtmlImageElement abstraction.
        /// </summary>
        internal static void UpdateImageSourceWebView2(ImagePropertiesInfo imgProperties, IBlogPostImageEditingContext editorContext, ImageInsertHandler imageInsertHandler, ImageDecoratorInvocationSource invocationSource)
        {
            var htmlImageElement = imgProperties.HtmlImageElement;
            if (htmlImageElement == null)
            {
                Debug.WriteLine("[OLW-DEBUG] UpdateImageSourceWebView2: No HtmlImageElement");
                return;
            }

            // Get current src and normalize from virtual host URL to file:// URL
            string imgSrc = htmlImageElement.Src;
            Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Original src={imgSrc}");
            
            if (imgSrc != null && imgSrc.StartsWith("https://olw-local-"))
            {
                string driveLetter = imgSrc.Substring("https://olw-local-".Length, 1);
                string path = imgSrc.Substring("https://olw-local-".Length + 1);
                imgSrc = $"file:///{driveLetter}:{path}";
                Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Normalized src={imgSrc}");
            }

            ISupportingFile oldImageFile = null;
            try
            {
                oldImageFile = editorContext.SupportingFileService.GetFileByUri(new Uri(imgSrc));
            }
            catch (UriFormatException) { }

            if (oldImageFile != null)
            {
                using (new WaitCursor())
                {
                    BlogPostImageData imageData = BlogPostImageDataList.LookupImageDataByInlineUri(editorContext.ImageList, oldImageFile.FileUri);
                    if (imageData != null)
                    {
                        Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Found imageData, processing resize");
                        
                        // Ensure the inline size is properly set in decorator settings before WriteImages
                        // This ensures filter mode uses the correct target size (from DOM) not the source size
                        Size currentInlineSize = imgProperties.InlineImageSize;
                        Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Setting target size to current inline={currentInlineSize}");
                        imgProperties.InlineImageSize = currentInlineSize; // This calls SetImageSize which stores TARGET_WIDTH/HEIGHT
                        
                        BlogPostImageData newImageData = (BlogPostImageData)imageData.Clone();

                        CreateImageFileHandler inlineFileCreator = new CreateImageFileHandler(editorContext.SupportingFileService,
                            newImageData.InlineImageFile != null ? newImageData.InlineImageFile.SupportingFile : null);
                        CreateImageFileHandler linkedFileCreator = new CreateImageFileHandler(editorContext.SupportingFileService,
                            newImageData.LinkedImageFile != null ? newImageData.LinkedImageFile.SupportingFile : null);

                        try
                        {
                            // Re-write the image files on disk using the latest settings (runs decorators)
                            imageInsertHandler.WriteImages(imgProperties, true, invocationSource, 
                                new CreateFileCallback(inlineFileCreator.CreateFileCallback), 
                                new CreateFileCallback(linkedFileCreator.CreateFileCallback), 
                                editorContext.EditorOptions);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: WriteImages EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                            Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Stack: {ex.StackTrace}");
                            throw;
                        }

                        Size imageSizeWithBorder = imgProperties.InlineImageSizeWithBorder;
                        Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: New size={imageSizeWithBorder}");

                        // Update DOM via IHtmlImageElement abstraction
                        string newSrcUri = UrlHelper.SafeToAbsoluteUri(inlineFileCreator.ImageSupportingFile.FileUri);
                        Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: New src URI (before conversion)={newSrcUri}");
                        
                        // Convert file:// URL to virtual host URL for WebView2
                        // file:///C:/path -> https://olw-local-c/path
                        if (newSrcUri != null && newSrcUri.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(newSrcUri, @"file:///([A-Za-z]):/(.*)");
                            if (match.Success)
                            {
                                var driveLetter = match.Groups[1].Value.ToLowerInvariant();
                                var path = match.Groups[2].Value;
                                newSrcUri = $"https://olw-local-{driveLetter}/{path}";
                                Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Converted to={newSrcUri}");
                            }
                        }
                        
                        // Update src to new resized image file
                        // NOTE: We do NOT update width/height here - the DOM should keep the user's 
                        // requested size, even if the actual file is larger (e.g., due to drop shadow).
                        // This prevents feedback loops where reading the new DOM size triggers another resize.
                        htmlImageElement.Src = newSrcUri;
                        Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: DOM src updated, NOT updating dimensions");

                        // Update ImageData file references
                        newImageData.InlineImageFile.SupportingFile = inlineFileCreator.ImageSupportingFile;
                        newImageData.InlineImageFile.Height = imageSizeWithBorder.Height;
                        newImageData.InlineImageFile.Width = imageSizeWithBorder.Width;
                        
                        if (imgProperties.LinkTarget == LinkTargetType.IMAGE)
                        {
                            newImageData.LinkedImageFile = new ImageFileData(linkedFileCreator.ImageSupportingFile, 
                                imgProperties.LinkTargetImageSize.Width, imgProperties.LinkTargetImageSize.Height, ImageFileRelationship.Linked);
                        }
                        else
                        {
                            newImageData.LinkedImageFile = null;
                        }

                        newImageData.ImageDecoratorSettings = (BlogPostSettingsBag)imgProperties.ImageDecorators.SettingsBag.Clone();
                        newImageData.UploadInfo.ImageServiceId = imgProperties.UploadServiceId;
                        editorContext.ImageList.AddImage(newImageData);
                        
                        Debug.WriteLine($"[OLW-DEBUG] UpdateImageSourceWebView2: Complete, updated ImageList");
                    }
                    else
                    {
                        Debug.WriteLine("[OLW-DEBUG] UpdateImageSourceWebView2: imageData could not be located");
                    }
                }
            }
            else
            {
                Debug.WriteLine("[OLW-DEBUG] UpdateImageSourceWebView2: oldImageFile is null (not a supporting file)");
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
