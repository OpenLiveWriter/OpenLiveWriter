// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Attribute which should be applied to all classes derived from WriterPlugin.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WriterPluginAttribute : Attribute
    {
        /// <summary>
        /// The description
        /// </summary>
        [NotNull]
        private string description = string.Empty;

        /// <summary>
        /// The identifier
        /// </summary>
        [NotNull]
        private string id = string.Empty;

        /// <summary>
        /// The name
        /// </summary>
        [NotNull]
        private string name = string.Empty;

        /// <summary>
        /// The publisher URL
        /// </summary>
        [NotNull]
        private string publisherUrl = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterPluginAttribute" /> class.
        /// </summary>
        /// <param name="id">Unique ID for the plugin (must be a GUID without leading and trailing braces)</param>
        /// <param name="name">Plugin name (this will appear in the Plugins preferences panel)</param>
        public WriterPluginAttribute([NotNull] string id, [NotNull] string name)
        {
            this.Id = id;
            this.Name = name;
            this.ImagePath = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterPluginAttribute"/> class.
        /// </summary>
        /// <param name="id">
        /// Unique ID for the plugin (must be a GUID without leading and trailing braces)
        /// </param>
        /// <param name="name">
        /// Plugin name (this will appear in the Plugins preferences panel)
        /// </param>
        /// <param name="imagePath">
        /// Path to embedded image resource used to represent this plugin within the
        /// Open Live Writer UI (menu bitmap, sidebar bitmap, etc.). The size of the embedded image must be 20x18.
        /// </param>
        [Obsolete("This method is for compatibility with pre-beta plugins, and will be removed in the future.")]
        public WriterPluginAttribute([NotNull] string id, [NotNull] string name, [NotNull] string imagePath)
        {
            this.Id = id;
            this.Name = name;
            this.ImagePath = imagePath;
        }

        /// <summary>
        /// Gets or sets the short description (1-2 sentences) of the plugin (displayed in the Plugins Preferences panel). Optional.
        /// </summary>
        /// <value>The description.</value>
        /// <exception cref="System.ArgumentNullException">Description cannot be null</exception>
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
                    throw new ArgumentNullException(nameof(WriterPluginAttribute.Description));
                }

                this.description = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Plugin has editable options. That is, whether it overrides the EditOptions
        /// method of the WriterPlugin base class. Default is false.
        /// </summary>
        /// <value><c>true</c> if this instance has editable options; otherwise, <c>false</c>.</value>
        public bool HasEditableOptions { get; set; } = false;

        /// <summary>
        /// Gets or sets the unique ID for the plugin (should be a GUID without leading and trailing braces)
        /// </summary>
        /// <value>The identifier.</value>
        /// <exception cref="System.ArgumentNullException">Id cannot be null</exception>
        /// <exception cref="System.ArgumentException">Id must be a valid GUID</exception>
        [NotNull]
        public string Id
        {
            get
            {
                return this.id;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(WriterPluginAttribute.Id));
                }

                if (!WriterPluginAttribute.ValidateGuid(value))
                {
                    throw new ArgumentException($"The value specifed ({value}) was not a GUID", nameof(WriterPluginAttribute.Id));
                }

                this.id = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to embedded image resource used to represent this plugin within the
        /// Open Live Writer UI (menu bitmap, sidebar bitmap, etc.). The size of
        /// the embedded image must be 20x18 or 16x16 pixels.
        /// </summary>
        /// <value>The image path.</value>
        /// <remarks>Early Beta versions of Open Live Writer required icons to be 20x18, but
        /// later versions prefer 16x16. Later versions of Writer will scale 20x18 images
        /// to 16x16, or, if only the center 16x16 pixels of the 20x18 are non-transparent,
        /// the image will simply be cropped to 16x16.</remarks>
        [NotNull]
        public string ImagePath { get; set; }

        /// <summary>
        /// Gets or sets the plugin name (this will appear in the Plugins preferences panel)
        /// </summary>
        /// <value>The name.</value>
        /// <exception cref="System.ArgumentNullException">Name cannot be null</exception>
        [NotNull]
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
                    throw new ArgumentNullException(nameof(WriterPluginAttribute.Name));
                }

                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL of the publisher for the Plugin (linked to from Plugins Preferences panel). Optional.
        /// </summary>
        /// <value>The publisher URL.</value>
        /// <exception cref="System.ArgumentNullException">PublisherUrl cannot be null</exception>
        [NotNull]
        public string PublisherUrl
        {
            get
            {
                return this.publisherUrl;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(WriterPluginAttribute.PublisherUrl));
                }

                this.publisherUrl = value;
            }
        }

        /// <summary>
        /// Validates the unique identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool ValidateGuid([CanBeNull] string id)
        {
            Guid guid;
            return Guid.TryParse(id, out guid);
        }
    }
}
