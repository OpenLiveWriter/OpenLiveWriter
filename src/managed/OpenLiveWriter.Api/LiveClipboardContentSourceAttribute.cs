// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromLiveClipboard method to enable creation of new content from Live
    /// Clipboard data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class LiveClipboardContentSourceAttribute : Attribute
    {
        /// <summary>
        /// The content type
        /// </summary>
        [NotNull]
        private string contentType = string.Empty;

        /// <summary>
        /// The description
        /// </summary>
        [NotNull]
        private string description = string.Empty;

        /// <summary>
        /// The image path
        /// </summary>
        [NotNull]
        private string imagePath = string.Empty;

        /// <summary>
        /// The name
        /// </summary>
        [NotNull]
        private string name = string.Empty;

        /// <summary>
        /// The type
        /// </summary>
        [NotNull]
        private string type = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveClipboardContentSourceAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// End-user presentable name of data format handled by this ContentSource.
        /// </param>
        /// <param name="contentType">
        /// MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the <code>&lt;lc:format&gt;</code> tag)
        /// </param>
        public LiveClipboardContentSourceAttribute([NotNull] string name, [NotNull] string contentType)
        {
            this.Name = name;
            this.ContentType = contentType;
        }

        /// <summary>
        /// Gets or sets the MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the <code>&lt;lc:format&gt;</code> tag)
        /// </summary>
        [NotNull]
        public string ContentType
        {
            get
            {
                return this.contentType;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.ContentType));
                }

                this.contentType = value;
            }
        }

        /// <summary>
        /// Gets or sets the end-user presentable description of the data format handled by this ContentSource.
        /// (used within the Live Clipboard Preferences panel (Optional).
        /// </summary>
        [NotNull]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.Description));
                }

                this.description = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to embedded image resource used to represent this format within the Live Clipboard
        /// Preferences panel. The embedded image should be 20x18. If this attribute is not specified
        /// then the image specified in the WriterPlugin attribute is used.
        /// </summary>
        [NotNull]
        public string ImagePath
        {
            get
            {
                return this.imagePath;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.ImagePath));
                }

                this.imagePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the end-user presentable name of data format handled by this ContentSource.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.Name));
                }

                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the content sub-type handled by this content source. (corresponds to the
        /// type attribute of the <code>&lt;lc:format&gt;</code> tag). Optional (required only
        /// for formats which require additional disambiguration of the contentType
        /// attribute).
        /// </summary>
        [NotNull]
        public string Type
        {
            get
            {
                return this.type;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(this.Type));
                }

                this.type = value;
            }
        }
    }
}
