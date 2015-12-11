// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Reflection;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromLiveClipboard method to enable creation of new content from Live
    /// Clipboard data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LiveClipboardContentSourceAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of a LiveClipboardContentSourceAttribute
        /// </summary>
        /// <param name="name">End-user presentable name of data format handled by this ContentSource.</param>
        /// <param name="contentType">MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the &lt;lc:format&gt; tag)</param>
        public LiveClipboardContentSourceAttribute(string name, string contentType)
        {
            Name = name;
            ContentType = contentType;
        }

        /// <summary>
        /// End-user presentable name of data format handled by this ContentSource.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("LiveClipboardContentSource.Name");

                _name = value;
            }
        }
        private string _name = String.Empty;

        /// <summary>
        /// MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the &lt;lc:format&gt; tag)
        /// </summary>
        public string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("LiveClipboardContentSource.ContentType");

                _contentType = value;
            }
        }
        private string _contentType = String.Empty;

        /// <summary>
        /// Path to embedded image resource used to represent this format within the Live Clipboard
        /// Preferences panel. The embedded image should be 20x18. If this attribute is not specified
        /// then the image specified in the WriterPlugin attribute is used.
        /// </summary>
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("LiveClipboardContentSource.ImagePath");

                _imagePath = value;
            }
        }
        private string _imagePath = String.Empty;

        /// <summary>
        /// End-user presentable description of the data format handled by this ContentSource.
        /// (used within the Live Clipboard Preferences panel (Optional).
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("LiveClipboardContentSource.Description");

                _description = value;
            }
        }
        private string _description = String.Empty;

        /// <summary>
        /// Content sub-type handled by this content source. (corresponds to the
        /// type attribute of the &lt;lc:format&gt; tag). Optional (required only
        /// for formats which require additional disambiguation of the contentType
        /// attribute).
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("LiveClipboardContentSource.Type");

                _type = value;
            }
        }
        private string _type = String.Empty;
    }
}
