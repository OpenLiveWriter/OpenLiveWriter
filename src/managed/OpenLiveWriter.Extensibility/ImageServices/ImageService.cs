// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Extensibility.ImageServices;

namespace OpenLiveWriter.Extensibility.ImageServices
{
    public interface IImageFile
    {
        int Width {get; }
        int Height {get; }
        string FilePath { get; }
        ImageEmbedType ImageEmbedType { get; }
    }

    public interface IImageUploadResult
    {
        string ImageUrl { get; }
    }

    public interface IUploadImageContext
    {
        IImageFile[] UploadImageFiles {get; }
        IProperties ImageFileUploadSettings { get; }
        //IImageFile GenerateImageFile(int width, int height, ImageGenerationFlags flags);
    }

    public interface IImageServiceUploader
    {
        void Connect();
        void Disconnect();
        IImageUploadResult[] UploadImages(IUploadImageContext uploadImageContext);
    }

    public interface IImageService
    {
        /// <summary>
        /// Create an image service uploader instance.
        /// </summary>
        /// <param name="imageServiceSettings"></param>
        /// <returns></returns>
        IImageServiceUploader CreateImageServiceUploader(IProperties imageServiceSettings);

        /// <summary>
        /// Create an editor for customizing the image service settings.
        /// </summary>
        /// <returns></returns>
        ImageServiceSettingsEditor CreateImageServiceSettingsEditor();

        /// <summary>
        /// Create an editor for customizing the upload settings for an individual image file.
        /// </summary>
        /// <returns></returns>
        ImageFileUploadSettingsEditor CreateImageFileUploadSettingsEditor();
    }
}

