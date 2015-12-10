// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video.VideoService
{
    abstract class VideoRequestType : IVideoRequestType
    {
        protected VideoRequestType(string typeName, string displayName, Bitmap bitmap)
        {
            _typeName = typeName;
            _displayName = displayName;
            _bitMap = bitmap;
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public Bitmap Icon
        {
            get { return _bitMap; }
        }

        public Image Image
        {
            get { return _bitMap; }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            if (obj is VideoRequestType)
                return (obj as VideoRequestType).TypeName == TypeName;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }

        private readonly string _typeName;
        private readonly string _displayName;
        private readonly Bitmap _bitMap;
    }

    class MyVideosRequestType : VideoRequestType
    {
        public MyVideosRequestType()
            : base("MyVideos", Res.Get(StringId.MyVideos), ResourceHelper.LoadAssemblyResourceBitmap("Video.VideoService.Images.MyVideos.png"))
        {
        }
    }

    class MyFavoritesRequestType : VideoRequestType
    {
        public MyFavoritesRequestType()
            : base("MyFavorites", Res.Get(StringId.MyFavorites), ResourceHelper.LoadAssemblyResourceBitmap("Video.VideoService.Images.MyFavorites.png"))
        {
        }
    }

}
