// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.Emoticons;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public static class ImageInsertionManager
    {
        private static Uri LoadingImagePath = new Uri(Path.Combine(ApplicationEnvironment.InstallationDirectory, @"html\loading.png"));
        private static bool selectionChanged;

        /// <summary>
        /// Scans the DOM for images that have not yet been properly initialized by the editor.
        /// As part of the initialization, the default image settings and effects will be applied to the image.
        /// </summary>
        internal static void ScanAndInitializeNewImages(IBlogPostHtmlEditor currentEditor, ISupportingFileService fileService, IEditorAccount editorAccount, ContentEditor editor, Control owner, bool useDefaultTargetSettings, bool selectLastImage)
        {
            if (currentEditor is BlogPostHtmlEditorControl)
            {
                // Scanning the images and doing basic initialization is done on the UI thread.
                List<NewImageInfo> newImages = ScanImages(currentEditor, editorAccount, editor, useDefaultTargetSettings);

                if (newImages.Count > 0)
                {
                    // The time-consuming initialization is done in a background thread.
                    ImageInitializationAsyncOperation imageInitializer = new ImageInitializationAsyncOperation(newImages, fileService, owner);
                    imageInitializer.Completed += new EventHandler((sender, e) =>
                        ProcessInitializedImages(newImages, currentEditor, editor, owner, selectLastImage));

                    editor.DisposeOnEditorChange(imageInitializer);

                    imageInitializer.Start();
                }
            }
        }

        private static List<NewImageInfo> ScanImages(IBlogPostHtmlEditor currentEditor, IEditorAccount editorAccount, ContentEditor editor, bool useDefaultTargetSettings)
        {
            List<NewImageInfo> newImages = new List<NewImageInfo>();

            ApplicationPerformance.ClearEvent("InsertImage");
            ApplicationPerformance.StartEvent("InsertImage");

            using (new WaitCursor())
            {
                IHTMLElement2 postBodyElement = (IHTMLElement2)((BlogPostHtmlEditorControl)currentEditor).PostBodyElement;
                if (postBodyElement != null)
                {
                    foreach (IHTMLElement imgElement in postBodyElement.getElementsByTagName("img"))
                    {
                        string imageSrc = imgElement.getAttribute("srcDelay", 2) as string;

                        if (string.IsNullOrEmpty(imageSrc))
                        {
                            imageSrc = imgElement.getAttribute("src", 2) as string;
                        }

                        // WinLive 96840 - Copying and pasting images within shared canvas should persist source
                        // decorator settings. "wlCopySrcUrl" is inserted while copy/pasting within canvas.
                        bool copyDecoratorSettings = false;
                        string attributeCopySrcUrl = imgElement.getAttribute("wlCopySrcUrl", 2) as string;
                        if (!string.IsNullOrEmpty(attributeCopySrcUrl))
                        {
                            copyDecoratorSettings = true;
                            imgElement.removeAttribute("wlCopySrcUrl", 0);
                        }

                        // Check if we need to apply default values for image decorators
                        bool applyDefaultDecorator = true;
                        string attributeNoDefaultDecorator = imgElement.getAttribute("wlNoDefaultDecorator", 2) as string;
                        if (!string.IsNullOrEmpty(attributeNoDefaultDecorator) && string.Compare(attributeNoDefaultDecorator, "TRUE", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            applyDefaultDecorator = false;
                            imgElement.removeAttribute("wlNoDefaultDecorator", 0);
                        }

                        string applyDefaultMargins = imgElement.getAttribute("wlApplyDefaultMargins", 2) as string;
                        if (!String.IsNullOrEmpty(applyDefaultMargins))
                        {
                            DefaultImageSettings defaultImageSettings = new DefaultImageSettings(editorAccount.Id, editor.DecoratorsManager);
                            MarginStyle defaultMargin = defaultImageSettings.GetDefaultImageMargin();
                            // Now apply it to the image
                            imgElement.style.marginTop = String.Format(CultureInfo.InvariantCulture, "{0} px", defaultMargin.Top);
                            imgElement.style.marginLeft = String.Format(CultureInfo.InvariantCulture, "{0} px", defaultMargin.Left);
                            imgElement.style.marginBottom = String.Format(CultureInfo.InvariantCulture, "{0} px", defaultMargin.Bottom);
                            imgElement.style.marginRight = String.Format(CultureInfo.InvariantCulture, "{0} px", defaultMargin.Right);
                            imgElement.removeAttribute("wlApplyDefaultMargins", 0);
                        }

                        if ((UrlHelper.IsFileUrl(imageSrc) || IsFullPath(imageSrc)) && !ContentSourceManager.IsSmartContent(imgElement))
                        {
                            Uri imageSrcUri = new Uri(imageSrc);
                            try
                            {
                                BlogPostImageData imageData = BlogPostImageDataList.LookupImageDataByInlineUri(editor.ImageList, imageSrcUri);
                                Emoticon emoticon = EmoticonsManager.GetEmoticon(imgElement);
                                if (imageData == null && emoticon != null)
                                {
                                    // This is usually an emoticon copy/paste and needs to be cleaned up.
                                    Uri inlineImageUri = editor.EmoticonsManager.GetInlineImageUri(emoticon);
                                    imgElement.setAttribute("src", UrlHelper.SafeToAbsoluteUri(inlineImageUri), 0);
                                }
                                else if (imageData == null)
                                {
                                    if (!File.Exists(imageSrcUri.LocalPath))
                                        throw new FileNotFoundException(imageSrcUri.LocalPath);

                                    // WinLive 188841: Manually attach the behavior so that the image cannot be selected or resized while its loading.
                                    DisabledImageElementBehavior disabledImageBehavior = new DisabledImageElementBehavior(editor.IHtmlEditorComponentContext);
                                    disabledImageBehavior.AttachToElement(imgElement);

                                    Size sourceImageSize = ImageUtils.GetImageSize(imageSrcUri.LocalPath);
                                    ImagePropertiesInfo imageInfo = new ImagePropertiesInfo(imageSrcUri, sourceImageSize, new ImageDecoratorsList(editor.DecoratorsManager, new BlogPostSettingsBag()));
                                    DefaultImageSettings defaultImageSettings = new DefaultImageSettings(editorAccount.Id, editor.DecoratorsManager);

                                    // Make sure this is set because some imageInfo properties depend on it.
                                    imageInfo.ImgElement = imgElement;

                                    bool isMetafile = ImageHelper2.IsMetafile(imageSrcUri.LocalPath);
                                    ImageClassification imgClass = ImageHelper2.Classify(imageSrcUri.LocalPath);
                                    if (!isMetafile && ((imgClass & ImageClassification.AnimatedGif) != ImageClassification.AnimatedGif))
                                    {
                                        // WinLive 96840 - Copying and pasting images within shared canvas should persist source
                                        // decorator settings.
                                        if (copyDecoratorSettings)
                                        {
                                            // Try to look up the original copied source image.
                                            BlogPostImageData imageDataOriginal = BlogPostImageDataList.LookupImageDataByInlineUri(editor.ImageList, new Uri(attributeCopySrcUrl));
                                            if (imageDataOriginal != null && imageDataOriginal.GetImageSourceFile() != null)
                                            {
                                                // We have the original image reference, so lets make a clone of it.
                                                BlogPostSettingsBag originalBag = (BlogPostSettingsBag)imageDataOriginal.ImageDecoratorSettings.Clone();
                                                ImageDecoratorsList originalDecoratorsList = new ImageDecoratorsList(editor.DecoratorsManager, originalBag);

                                                ImageFileData originalImageFileData = imageDataOriginal.GetImageSourceFile();
                                                Size originalImageSize = new Size(originalImageFileData.Width, originalImageFileData.Height);
                                                imageInfo = new ImagePropertiesInfo(originalImageFileData.Uri, originalImageSize, originalDecoratorsList);
                                            }
                                            else
                                            {
                                                // There are probably decorators applied to the image, but in a different editor so we can't access them.
                                                // We probably don't want to apply any decorators to this image, so apply blank decorators and load the
                                                // image as full size so it looks like it did before.
                                                imageInfo.ImageDecorators = defaultImageSettings.LoadBlankLocalImageDecoratorsList();
                                                imageInfo.InlineImageSizeName = ImageSizeName.Full;
                                            }
                                        }
                                        else if (applyDefaultDecorator)
                                        {
                                            imageInfo.ImageDecorators = defaultImageSettings.LoadDefaultImageDecoratorsList();

                                            if ((imgClass & ImageClassification.TransparentGif) == ImageClassification.TransparentGif)
                                                imageInfo.ImageDecorators.AddDecorator(NoBorderDecorator.Id);
                                        }
                                        else
                                        {
                                            // Don't use default values for decorators
                                            imageInfo.ImageDecorators = defaultImageSettings.LoadBlankLocalImageDecoratorsList();
                                            imageInfo.InlineImageSizeName = ImageSizeName.Full;
                                        }
                                    }
                                    else
                                    {
                                        ImageDecoratorsList decorators = new ImageDecoratorsList(editor.DecoratorsManager, new BlogPostSettingsBag());
                                        decorators.AddDecorator(editor.DecoratorsManager.GetDefaultRemoteImageDecorators());
                                        imageInfo.ImageDecorators = decorators;
                                    }

                                    imageInfo.ImgElement = imgElement;
                                    imageInfo.DhtmlImageViewer = editorAccount.EditorOptions.DhtmlImageViewer;

                                    //discover the "natural" target settings from the DOM
                                    string linkTargetUrl = imageInfo.LinkTargetUrl;
                                    if (linkTargetUrl == imageSrc)
                                    {
                                        imageInfo.LinkTarget = LinkTargetType.IMAGE;
                                    }
                                    else if (!String.IsNullOrEmpty(linkTargetUrl) && !UrlHelper.IsFileUrl(linkTargetUrl))
                                    {
                                        imageInfo.LinkTarget = LinkTargetType.URL;
                                    }
                                    else
                                    {
                                        imageInfo.LinkTarget = LinkTargetType.NONE;
                                    }

                                    if (useDefaultTargetSettings)
                                    {
                                        if (!GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SupportsImageClickThroughs) && imageInfo.DefaultLinkTarget == LinkTargetType.IMAGE)
                                            imageInfo.DefaultLinkTarget = LinkTargetType.NONE;

                                        if (imageInfo.LinkTarget == LinkTargetType.NONE)
                                            imageInfo.LinkTarget = imageInfo.DefaultLinkTarget;
                                        if (imageInfo.DefaultLinkOptions.ShowInNewWindow)
                                            imageInfo.LinkOptions.ShowInNewWindow = true;
                                        imageInfo.LinkOptions.UseImageViewer = imageInfo.DefaultLinkOptions.UseImageViewer;
                                        imageInfo.LinkOptions.ImageViewerGroupName = imageInfo.DefaultLinkOptions.ImageViewerGroupName;
                                    }

                                    Size defaultImageSize = defaultImageSettings.GetDefaultInlineImageSize();
                                    Size initialSize = ImageUtils.GetScaledImageSize(defaultImageSize.Width, defaultImageSize.Height, sourceImageSize);

                                    // add to list of new images
                                    newImages.Add(new NewImageInfo(imageInfo, imgElement, initialSize, disabledImageBehavior));
                                }
                                else
                                {
                                    // When switching blogs, try to adapt image viewer settings according to the blog settings.

                                    ImagePropertiesInfo imageInfo = new ImagePropertiesInfo(imageSrcUri, ImageUtils.GetImageSize(imageSrcUri.LocalPath), new ImageDecoratorsList(editor.DecoratorsManager, imageData.ImageDecoratorSettings));
                                    imageInfo.ImgElement = imgElement;
                                    // Make sure the new crop and tilt decorators get loaded
                                    imageInfo.ImageDecorators.MergeDecorators(DefaultImageSettings.GetImplicitLocalImageDecorators());
                                    string viewer = imageInfo.DhtmlImageViewer;
                                    if (viewer != editorAccount.EditorOptions.DhtmlImageViewer)
                                    {
                                        imageInfo.DhtmlImageViewer = editorAccount.EditorOptions.DhtmlImageViewer;
                                        imageInfo.LinkOptions = imageInfo.DefaultLinkOptions;
                                    }

                                    // If the image is an emoticon, update the EmoticonsManager with the image's uri so that duplicate emoticons can point to the same file.
                                    if (emoticon != null)
                                        editor.EmoticonsManager.SetInlineImageUri(emoticon, imageData.InlineImageFile.Uri);
                                }
                            }
                            catch (ArgumentException e)
                            {
                                Trace.WriteLine("Could not initialize image: " + imageSrc);
                                Trace.WriteLine(e.ToString());
                            }
                            catch (DirectoryNotFoundException)
                            {
                                Debug.WriteLine("Image file does not exist: " + imageSrc);
                            }
                            catch (FileNotFoundException)
                            {
                                Debug.WriteLine("Image file does not exist: " + imageSrc);
                            }
                            catch (IOException e)
                            {
                                Debug.WriteLine("Image file cannot be read: " + imageSrc + " " + e);
                                DisplayMessage.Show(MessageId.FileInUse, imageSrc);
                            }
                        }
                    }
                }
            }

            return newImages;
        }

        private static void ProcessInitializedImages(List<NewImageInfo> newImages, IBlogPostHtmlEditor currentEditor, ContentEditor editor, Control owner, bool selectLastImage)
        {
            // Remove all invalid images first
            List<NewImageInfo> newImagesToUpdate = new List<NewImageInfo>();
            for (int i = 0; i < newImages.Count; i++)
            {
                NewImageInfo info = newImages[i];

                if (info.Remove)
                {
                    using (ContentEditor.EditorUndoUnit undo = new ContentEditor.EditorUndoUnit(currentEditor, true))
                    {
                        info.DisabledImageBehavior.DetachFromElement();
                        HTMLElementHelper.RemoveElement(info.Element);
                        undo.Commit();
                    }
                }
                else
                {
                    newImagesToUpdate.Add(info);
                }
            }

            // Queue up processing for any remaining valid images
            MultiTaskHelper tasks = new MultiTaskHelper(30);
            for (int i = 0; i < newImagesToUpdate.Count; i++)
            {
                bool lastElement = i == (newImagesToUpdate.Count - 1);
                NewImageInfo info = newImagesToUpdate[i];

                tasks.AddWork(delegate
                {
                    // WinLive 214012: If the original insertion operation is undone before we get to this point make
                    // sure we don't attempt to update the HTML. Elements that are removed via an undo, but that we
                    // still hold a reference to, are put into another document whose readyState is "uninitialized".
                    // It's also possible that the picture has already been updated if a user undoes and redoes the
                    // initial insertion multiple times.
                    IHTMLDocument2 doc = (IHTMLDocument2)info.Element.document;
                    if (doc == null || doc.readyState.Equals("uninitialized", StringComparison.OrdinalIgnoreCase) ||
                        BlogPostImageDataList.LookupImageDataByInlineUri(editor.ImageList, info.ImageData.InlineImageFile.Uri) != null)
                    {
                        info.DisabledImageBehavior.DetachFromElement();
                        return;
                    }

                    using (new QuickTimer("ProcessInitializedImage"))
                    using (ContentEditor.EditorUndoUnit undo = new ContentEditor.EditorUndoUnit(currentEditor, true))
                    {
                        editor.ImageList.AddImage(info.ImageData);

                        info.Element.setAttribute("src", UrlHelper.SafeToAbsoluteUri(info.ImageData.InlineImageFile.Uri), 0);
                        info.Element.removeAttribute("srcDelay", 0);
                        // Create the decorators
                        ImageEditingPropertyHandler.UpdateImageSource(info.ImageInfo, info.Element, editor, new ImageInsertHandler(), ImageDecoratorInvocationSource.InitialInsert);

                        // Manually detach the behavior so that the image can be selected and resized.
                        info.DisabledImageBehavior.DetachFromElement();

                        if (lastElement)
                        {
                            ApplicationPerformance.EndEvent("InsertImage");
                            HandleNewImage((BlogPostHtmlEditorControl)currentEditor, owner, info.Element, selectLastImage);
                        }

                        undo.Commit();

                        if (editor.ETWProvider != null)
                            editor.ETWProvider.WriteEvent("InlinePhotoEnd");
                    }
                });
            }

            // If there were no images to update, stop the perf timer
            if (newImagesToUpdate.Count == 0)
            {
                ApplicationPerformance.EndEvent("InsertImage");
            }

            // When the editor is closing, or changing to a new blog post we need to get rid of this object
            // which will then stop all the unfinished images from continuing to load.
            editor.DisposeOnEditorChange(tasks);
            tasks.Start();
        }

        private static bool IsFullPath(string str)
        {
            try
            {
                return File.Exists(str);
            }
            catch (Exception e)
            {
                Trace.Fail("Surprising, File.Exists threw an exception: " + e);
                return false;
            }
        }

        public const int DELAYED_IMAGE_THRESHOLD = 2;

        /// <summary>
        /// Inserts a set of images into the current editor, optionally assigning each image an id.
        /// </summary>
        /// <param name="imagePaths">paths of the images to insert</param>
        internal static void InsertImagesCore(IBlogPostHtmlEditor currentEditor, ISupportingFileService fileService, IEditorAccount editorAccount, OpenLiveWriter.PostEditor.ContentEditor editor, string[] imagePaths)
        {
            using (OpenLiveWriter.PostEditor.ContentEditor.EditorUndoUnit undo = new OpenLiveWriter.PostEditor.ContentEditor.EditorUndoUnit(currentEditor))
            {
                StringBuilder htmlBuilder = new StringBuilder();

                //calculate the size for inserted images (based on the user's saved default for this blog)
                DefaultImageSettings defaultImageSettings = new DefaultImageSettings(editorAccount.Id, editor.DecoratorsManager);
                Size defaultImageSize = defaultImageSettings.GetDefaultInlineImageSize();

                //Generate the default img HTML for the image paths.  Initially, the images are linked to the source image
                //path so that the DOM version of the HTML can be loaded.  Once the images are loaded into the DOM, we can apply
                //the image decorators to generate scaled images, borders, etc.
                ImagePropertiesInfo[] imageInfos = new ImagePropertiesInfo[imagePaths.Length];

                // don't insert into the title
                currentEditor.FocusBody();

                for (int i = 0; i < imageInfos.Length; i++)
                {
                    string imagePath = imagePaths[i];

                    Uri imgUri;
                    if (UrlHelper.IsUrl(imagePath))
                        imgUri = new Uri(imagePath);
                    else
                        imgUri = new Uri(UrlHelper.CreateUrlFromPath(imagePath));

                    Size imageSize = new Size(1, 1);
                    if (imgUri.IsFile && File.Exists(imagePath))
                    {
                        if (!File.Exists(imgUri.LocalPath))
                            Trace.Fail("Error inserting image - the image URL was corrupted: " + imagePath);
                        else
                        {
                            try
                            {
                                //check the validity of the image file
                                imageSize = ImageUtils.GetImageSize(imagePath);
                            }
                            catch (Exception)
                            {
                                Trace.WriteLine("There is a problem with the image file: " + imagePath);

                                // Insert anyway, MSHTML will show a red X in place of the image.
                                htmlBuilder.AppendFormat(CultureInfo.InvariantCulture, "<img src=\"{0}\" /><p>&nbsp;</p>", HtmlUtils.EscapeEntities(UrlHelper.SafeToAbsoluteUri(imgUri)));

                                continue;
                            }
                        }
                    }

                    // If the image has an embedded thumbnail, we'll use it as a place holder for the <img src="...">
                    // until we generate an inline image and apply decorators.
                    Stream embeddedThumbnailStream = ImageHelper2.GetEmbeddedThumbnailStream(imagePath);

                    if (embeddedThumbnailStream != null)
                    {
                        // Save thumbnail to disk.
                        ISupportingFile imageFileEmbeddedThumbnail = fileService.CreateSupportingFile(Path.GetFileName(imagePath), Guid.NewGuid().ToString(), embeddedThumbnailStream);

                        imageSize = ImageUtils.GetScaledImageSize(defaultImageSize.Width, defaultImageSize.Height, imageSize);
                        //insert the default image html
                        String imageElementAttrs = String.Format(CultureInfo.InvariantCulture, " width=\"{0}\" height=\"{1}\"", imageSize.Width, imageSize.Height);

                        htmlBuilder.AppendFormat(CultureInfo.InvariantCulture, "<img src=\"{0}\" srcDelay=\"{1}\" {2} /><p>&nbsp;</p>", HtmlUtils.EscapeEntities(UrlHelper.SafeToAbsoluteUri(imageFileEmbeddedThumbnail.FileUri)), HtmlUtils.EscapeEntities(imgUri.ToString()), imageElementAttrs);
                    }
                    else if (currentEditor is BlogPostHtmlEditorControl && (imagePaths.Length > DELAYED_IMAGE_THRESHOLD || imageSize.Width * imageSize.Height > 16777216/*4096 X 4096*/))
                    {
                        // If we are using a delayed loading tactic then we insert using the srcdelay
                        htmlBuilder.Append(MakeHtmlForImageSourceDelay(imgUri.ToString()));
                    }
                    else
                    {
                        imageSize = ImageUtils.GetScaledImageSize(defaultImageSize.Width, defaultImageSize.Height, imageSize);
                        //insert the default image html
                        String imageElementAttrs = imgUri.IsFile ? String.Format(CultureInfo.InvariantCulture, " width=\"{0}\" height=\"{1}\"", imageSize.Width, imageSize.Height) : String.Empty;

                        htmlBuilder.AppendFormat(CultureInfo.InvariantCulture, "<img src=\"{0}\" {1} /><p>&nbsp;</p>", HtmlUtils.EscapeEntities(UrlHelper.SafeToAbsoluteUri(imgUri)), imageElementAttrs);
                    }
                }

                //insert the HTML into the editor
                currentEditor.InsertHtml(htmlBuilder.ToString(), false);

                selectionChanged = false;
                BlogPostHtmlEditorControl blogPostEditor = currentEditor as BlogPostHtmlEditorControl;
                if (blogPostEditor != null)
                    blogPostEditor.SelectionChanged += blogPostEditor_SelectionChanged;

                //now that the image HTML is inserted into the editor, apply the default settings to the new images.
                undo.Commit();
            }
        }

        private static void HandleNewImage(BlogPostHtmlEditorControl currentEditor, Control owner, IHTMLElement imageElement, bool select)
        {
            // If the selection has changed since we first inserted the img tag, don't force a new selection.
            currentEditor.SelectionChanged -= blogPostEditor_SelectionChanged;
            if (selectionChanged || !select)
                return;

            try
            {
                currentEditor.EmptySelection();
                // programmatically select the image
                currentEditor.SelectImage(imageElement);
                currentEditor.Focus();
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error while attempting to select newly inserted image: " + ex.ToString());
            }
        }

        public static string MakeHtmlForImageSourceDelay(string path)
        {
            return
                String.Format(CultureInfo.InvariantCulture, "<img src=\"{1}\" srcDelay=\"{0}\" /><p>&nbsp;</p>", HtmlUtils.EscapeEntities(path), HtmlUtils.EscapeEntities(UrlHelper.SafeToAbsoluteUri(LoadingImagePath)));
        }

        internal static string ReferenceFixer(BeginTag tag, string reference, ISupportingFileService _fileService, OpenLiveWriter.PostEditor.ContentEditor editor)
        {
            // If it isnt a file url, then it wont be an image from supporting files.
            if (!UrlHelper.IsFile(reference))
                return reference;

            Uri uri = new Uri(reference);
            BlogPostImageData imageData = BlogPostImageDataList.LookupImageDataByInlineUri(editor.ImageList, uri);

            if (imageData == null)
            {
                if (BlogPostImageDataList.LookupImageDataByLinkedUri(editor.ImageList, uri) != null)
                    return null;

                return reference;
            }

            return imageData.ImageSourceFile.Uri.AbsoluteUri;
        }

        private static void blogPostEditor_SelectionChanged(object sender, EventArgs e)
        {
            selectionChanged = true;
        }
    }
}
