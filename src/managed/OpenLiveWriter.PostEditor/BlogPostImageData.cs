// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor
{
    public class BlogPostImageData : ICloneable
    {
        public BlogPostImageData() : this(null, null, null, null, new BlogPostImageServiceUploadInfo(), new BlogPostSettingsBag())
        {
        }
        public BlogPostImageData(ImageFileData imageSource) : this(imageSource, null, null, null, new BlogPostImageServiceUploadInfo(), new BlogPostSettingsBag())
        {
        }

        private BlogPostImageData(ImageFileData imageSource, ImageFileData imageShadowSource, ImageFileData inlineImageFile, ImageFileData linkedImageFile, BlogPostImageServiceUploadInfo uploadInfo, BlogPostSettingsBag decoratorSettings)
        {
            ImageSourceFile = imageSource;
            ImageSourceShadowFile = imageShadowSource;
            InlineImageFile = inlineImageFile;
            LinkedImageFile = linkedImageFile;
            UploadInfo = uploadInfo;
            ImageDecoratorSettings = decoratorSettings;
        }

        /// <summary>
        /// The source image file.
        /// </summary>
        public ImageFileData GetImageSourceFile()
        {
            if (File.Exists(ImageSourceFile.Uri.LocalPath))
                return ImageSourceFile;
            else
                return ImageSourceShadowFile;
        }

        /// <summary>
        /// The source image file.
        /// </summary>
        public ImageFileData ImageSourceFile;

        /// <summary>
        /// The shadow copy of the source image file.
        /// </summary>
        public ImageFileData ImageSourceShadowFile
        {
            get
            {
                if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShadowImageForDrafts))
                    Debug.Assert(_imageSourceShadowFile != null, "shadow file is not available");
                return _imageSourceShadowFile;
            }
            set
            {
                _imageSourceShadowFile = value;
            }
        }
        private ImageFileData _imageSourceShadowFile;

        internal void InitShadowFile(ISupportingFileService fileService)
        {
            if (_imageSourceShadowFile == null)
            {
                if (ImageSourceFile != null && File.Exists(ImageSourceFile.Uri.LocalPath))
                {
                    CreateShadowFile(ImageSourceFile, fileService);
                }
                else if (LinkedImageFile != null && File.Exists(LinkedImageFile.Uri.LocalPath))
                {
                    CreateShadowFile(LinkedImageFile, fileService);
                }
                else if (InlineImageFile != null && File.Exists(InlineImageFile.Uri.LocalPath))
                {
                    CreateShadowFile(InlineImageFile, fileService);
                }
            }
        }

        /// <summary>
        /// Creates an embedded shadow copy of a source image file.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="fileService"></param>
        private void CreateShadowFile(ImageFileData sourceFile, ISupportingFileService fileService)
        {
            Size shadowSize = new Size(1280, 960);
            using (MemoryStream shadowStream = new MemoryStream())
            {
                ImageFormat format;
                string fileExt;
                ImageHelper2.GetImageFormat(sourceFile.SupportingFile.FileName, out fileExt, out format);
                using (Bitmap sourceImage = new Bitmap(sourceFile.Uri.LocalPath))
                {
                    if (sourceImage.Width > shadowSize.Width || sourceImage.Height > shadowSize.Height)
                    {
                        shadowSize = ImageHelper2.SaveScaledThumbnailImage(Math.Min(shadowSize.Width, sourceImage.Width),
                                                                           Math.Min(shadowSize.Height, sourceImage.Height),
                                                                           sourceImage, format, shadowStream);
                    }
                    else
                    {
                        shadowSize = sourceImage.Size;
                        using (FileStream fs = File.OpenRead(sourceFile.Uri.LocalPath))
                        {
                            StreamHelper.Transfer(fs, shadowStream);
                        }
                    }
                }
                shadowStream.Seek(0, SeekOrigin.Begin);

                ISupportingFile supportingFile = fileService.CreateSupportingFile(sourceFile.SupportingFile.FileName, shadowStream);
                _imageSourceShadowFile = new ImageFileData(supportingFile, shadowSize.Width, shadowSize.Height, ImageFileRelationship.SourceShadow);
            }
        }

        /// <summary>
        /// The inline version of the image.
        /// </summary>
        public ImageFileData InlineImageFile;

        /// <summary>
        /// The linked version of the image.
        /// </summary>
        public ImageFileData LinkedImageFile;

        /// <summary>
        /// Returns the settings assigned to this image and its derivatives.
        /// </summary>
        public BlogPostSettingsBag ImageDecoratorSettings;

        public BlogPostImageServiceUploadInfo UploadInfo;
        #region ICloneable Members

        public object Clone()
        {
            return new BlogPostImageData(
                (ImageFileData)ImageSourceFile.Clone(),
                ImageSourceShadowFile != null ? (ImageFileData)ImageSourceShadowFile.Clone() : null,
                (ImageFileData)InlineImageFile.Clone(),
                LinkedImageFile != null ? (ImageFileData)LinkedImageFile.Clone() : null,
                (BlogPostImageServiceUploadInfo)UploadInfo.Clone(),
                (BlogPostSettingsBag)ImageDecoratorSettings.Clone());
        }

        #endregion
    }

    public enum ImageFileRelationship { Undefined, Source, Inline, Linked, SourceShadow };
    public class ImageFileData : ICloneable
    {
        private ISupportingFile _supportingFile;
        public ImageFileData(ISupportingFile supportingFile)
        {
            _supportingFile = supportingFile;
        }
        public ImageFileData(ISupportingFile supportingFile, int width, int height, ImageFileRelationship relationship)
        {
            _supportingFile = supportingFile;
            Width = width;
            Height = height;
            Relationship = relationship;
        }

        public ISupportingFile SupportingFile
        {
            get { return _supportingFile; }
            set { _supportingFile = value; }
        }

        public Uri Uri
        {
            get { return _supportingFile.FileUri; }
        }
        public Uri GetPublishedUri(string publishContext)
        {
            ISupportingFileUploadInfo uploadInfo = _supportingFile.GetUploadInfo(publishContext);
            if (uploadInfo != null)
                return uploadInfo.UploadUri;
            else
                return null;
        }
        public int Width
        {
            get { return _supportingFile.Settings.GetInt(WIDTH, 0); }
            set { _supportingFile.Settings.SetInt(WIDTH, value); }
        }
        private string WIDTH = "image.width";

        public int Height
        {
            get { return _supportingFile.Settings.GetInt(HEIGHT, 0); }
            set { _supportingFile.Settings.SetInt(HEIGHT, value); }
        }
        private string HEIGHT = "image.height";

        public ImageFileRelationship Relationship
        {
            get
            {
                string relationship = _supportingFile.Settings.GetString(RELATIONSHIP, ImageFileRelationship.Undefined.ToString());
                try
                {
                    return (ImageFileRelationship)ImageFileRelationship.Parse(typeof(ImageFileRelationship), relationship);
                }
                catch (Exception)
                {
                    return ImageFileRelationship.Undefined;
                }
            }
            set { _supportingFile.Settings.SetString(RELATIONSHIP, value.ToString()); }
        }
        private string RELATIONSHIP = "image.relationship";

        public object Clone()
        {
            return new ImageFileData(_supportingFile, Width, Height, Relationship);
        }
    }

    public class BlogPostImageServiceUploadInfo : ICloneable
    {
        internal BlogPostImageServiceUploadInfo()
        {
            Settings = new BlogPostSettingsBag();
        }
        internal BlogPostImageServiceUploadInfo(string imageServiceId, BlogPostSettingsBag settings)
        {
            ImageServiceId = imageServiceId;
            Settings = settings;
        }

        public string ImageServiceId;
        public BlogPostSettingsBag Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }
        private BlogPostSettingsBag _settings;

        public object Clone()
        {
            BlogPostImageServiceUploadInfo upload = new BlogPostImageServiceUploadInfo(ImageServiceId, (BlogPostSettingsBag)Settings.Clone());
            return upload;
        }
    }

    public class BlogPostImageDataList : IEnumerable
    {
        ArrayList _list = new ArrayList();

        public BlogPostImageDataList()
        {

        }
        public BlogPostImageDataList(BlogPostImageData[] images)
        {
            _list.AddRange(images);
        }
        public void AddImage(BlogPostImageData imageData)
        {
            _list.Add(imageData);
        }

        public void RemoveImage(BlogPostImageData imageData)
        {
            _list.Add(imageData);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public BlogPostImageData this[int i]
        {
            get
            {
                return (BlogPostImageData)_list[i];
            }
            set
            {
                _list[i] = value;
            }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        public static BlogPostImageData LookupImageDataByInlineUri(BlogPostImageDataList imageDataList, Uri inlineUri)
        {
            foreach (BlogPostImageData imageData in imageDataList)
            {
                ImageFileData fileData = imageData.InlineImageFile;

                //Check for condition that caused bug 483278, but we couldn't repro, investigate how we get into this state.
                Debug.Assert(fileData != null && fileData.Uri != null, "Illegal state for filedata detected!");

                if (fileData != null && fileData.Uri != null && fileData.Uri.Equals(inlineUri))
                    return imageData;
            }
            return null;
        }

        public static BlogPostImageData LookupImageDataByLinkedUri(BlogPostImageDataList imageDataList, Uri inlineUri)
        {
            foreach (BlogPostImageData imageData in imageDataList)
            {
                if (imageData.LinkedImageFile != null && imageData.LinkedImageFile.Uri.Equals(inlineUri))
                    return imageData;
            }
            return null;
        }
    }
}
