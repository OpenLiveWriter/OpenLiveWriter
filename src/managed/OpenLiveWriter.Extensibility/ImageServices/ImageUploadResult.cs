// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Extensibility.ImageServices;

namespace OpenLiveWriter.Extensibility.ImageServices
{
    public class ImageUploadResult : IImageUploadResult
    {
        public ImageUploadResult(string imageUrl)
        {
            _imageUrl = imageUrl;
        }

        public string ImageUrl
        {
            get
            {
                return _imageUrl;
            }
            set
            {
                _imageUrl = value;
            }
        }
        private string _imageUrl;
    }
}
