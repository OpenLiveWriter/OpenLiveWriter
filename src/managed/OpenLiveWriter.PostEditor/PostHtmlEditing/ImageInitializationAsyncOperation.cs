// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using CoreServices;
    using AsyncOperation = CoreServices.AsyncOperation;

    /// <summary>
    /// Given a list of images, initializes them and registers them with the editor on a background thread.
    /// </summary>
    internal class ImageInitializationAsyncOperation : AsyncOperation, IDisposable
    {
        /// <summary>
        /// A list of images to initialize.
        /// </summary>
        private List<NewImageInfo> newImages;

        /// <summary>
        /// Allows us to register the images with the editor.
        /// </summary>
        private ISupportingFileService fileService;

        /// <summary>
        /// Initializes a new instance of the ImageInitializationAsyncOperation class.
        /// </summary>
        /// <param name="newImages">A list of images to initialize.</param>
        /// <param name="fileService">Allows us to register the images with the editor.</param>
        /// <param name="owner">All events raised from this object will be delivered via this target.</param>
        public ImageInitializationAsyncOperation(List<NewImageInfo> newImages, ISupportingFileService fileService, ISynchronizeInvoke owner)
            : base(owner)
        {
            this.newImages = newImages;
            this.fileService = fileService;
        }

        /// <summary>
        /// Cancels this async operation.
        /// </summary>
        public void Dispose()
        {
            // By waiting, we make sure the fileService doesn't get reset between the time we cancel and the time the
            // background thread realizes its been cancelled.
            CancelAndWait();
        }

        /// <summary>
        /// Initializes and registers images with the editor.
        /// </summary>
        protected override void DoWork()
        {
            foreach (NewImageInfo newImage in this.newImages)
            {
                // Before starting on the next image, make sure we weren't cancelled.
                if (CancelRequested)
                {
                    AcknowledgeCancel();
                    return;
                }

                try
                {
                    // Register the image file as a BlogPostImageData so that the editor can manage the supporting files
                    ISupportingFile sourceFile = this.fileService.AddLinkedSupportingFileReference(newImage.ImageInfo.ImageSourceUri);
                    newImage.ImageData = new BlogPostImageData(new ImageFileData(sourceFile, newImage.ImageInfo.ImageSourceSize.Width, newImage.ImageInfo.ImageSourceSize.Height, ImageFileRelationship.Source));

                    // Create the shadow file if necessary.
                    if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShadowImageForDrafts))
                    {
                        newImage.ImageData.InitShadowFile(this.fileService);
                    }

                    // Create the initial inline image.
                    Stream inlineImageStream = new MemoryStream();
                    using (Bitmap sourceBitmap = new Bitmap(newImage.ImageInfo.ImageSourceUri.LocalPath))
                    {
                        string extension;
                        ImageFormat imageFormat;
                        ImageHelper2.GetImageFormat(newImage.ImageInfo.ImageSourceUri.LocalPath, out extension, out imageFormat);

                        using (Bitmap resizedBitmap = ImageHelper2.CreateResizedBitmap(sourceBitmap, newImage.InitialSize.Width, newImage.InitialSize.Height, imageFormat))
                        {
                            // Resizing the bitmap is a time-consuming operation, so its possible we were cancelled during it.
                            if (CancelRequested)
                            {
                                AcknowledgeCancel();
                                return;
                            }

                            ImageHelper2.SaveImage(resizedBitmap, imageFormat, inlineImageStream);
                        }

                        inlineImageStream.Seek(0, SeekOrigin.Begin);
                    }

                    // Saving the bitmap is a time-consuming operation, so its possible we were cancelled during it.
                    if (CancelRequested)
                    {
                        AcknowledgeCancel();
                        return;
                    }

                    // Link up the initial inline image.
                    ISupportingFile imageFilePlaceholderHolder = this.fileService.CreateSupportingFile(Path.GetFileName(newImage.ImageInfo.ImageSourceUri.LocalPath), Guid.NewGuid().ToString(), inlineImageStream);
                    newImage.ImageData.InlineImageFile = new ImageFileData(imageFilePlaceholderHolder, newImage.InitialSize.Width, newImage.InitialSize.Height, ImageFileRelationship.Inline);
                }
                catch (Exception e)
                {
                    // Something failed for this image, flag this for removal
                    Debug.WriteLine("Image file could not be initialized: " + newImage.ImageInfo.ImageSourceUri.LocalPath + " " + e);
                    Trace.WriteLine("Could not initialize image: " + newImage.ImageInfo.ImageSourceUri.LocalPath);
                    Trace.WriteLine(e.ToString());
                    newImage.Remove = true;
                }
            }

            // We've successfully initialized all the images, but make sure we weren't cancelled at the last second.
            if (CancelRequested)
            {
                AcknowledgeCancel();
                return;
            }
        }
    }
}
