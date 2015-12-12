// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for Video.
    /// </summary>
    public class Video
    {
        protected Video()
        {

        }

        public Video(string id, string url, string embed, string editorFormat, VideoProvider provider, int width, int height, VideoAspectRatioType aspectRatioType)
        {
            _id = id;
            _url = url;
            _embed = embed;
            _provider = provider;
            _width = width;
            _height = height;
            _editorFormat = editorFormat;
            AspectRatioType = aspectRatioType;

            if (AspectRatioType != VideoAspectRatioType.Unknown)
            {
                _height = VideoAspectRatioHelper.ResizeHeightToAspectRatio(AspectRatioType, new Size(width, height)).Height;
            }
        }

        public string Id { get { return _id; } }
        public string EditorFormat { get { return _editorFormat; } }
        public string Embed { get { return _embed; } }
        public VideoProvider Provider { get { return _provider; } }
        public string Url { get { return _url; } }
        public string ThumbnailUrl { get { return _thumbnailUrl; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public bool IsUploading { get { return _isUploading; } set { _isUploading = value; } }
        internal IStatusWatcher StatusWatcher { get { return _statusWatcher; } set { _statusWatcher = value; } }
        public string Username { get { return _username; } set { _username = value; } }
        public string Permission { get { return _permission; } set { _permission = value; } }
        public Bitmap Snapshot { get { return _snapshot; } set { _snapshot = value; } }
        public VideoAspectRatioType AspectRatioType { get; private set; }

        public override bool Equals(object obj)
        {
            return (obj as Video).Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        private string _id = String.Empty;
        private string _embed = String.Empty;
        private VideoProvider _provider = null;
        private string _url = String.Empty;
        private string _thumbnailUrl = String.Empty;
        private string _permission = String.Empty;
        private int _width = 425;
        private int _height = 350;
        private bool _isUploading = false;
        private IStatusWatcher _statusWatcher = null;
        private string _username = String.Empty;
        private string _editorFormat = String.Empty;
        private Bitmap _snapshot;
    }
}
