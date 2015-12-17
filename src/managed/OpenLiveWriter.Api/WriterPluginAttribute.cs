// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Attribute which should be applied to all classes derived from WriterPlugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WriterPluginAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of WriterPluginAttribute.
        /// </summary>
        /// <param name="id">Unique ID for the plugin (must be a GUID without leading and trailing braces)</param>
        /// <param name="name">Plugin name (this will appear in the Plugins preferences panel)</param>
        public WriterPluginAttribute(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Initialize a new instance of WriterPluginAttribute.
        /// </summary>
        /// <param name="id">Unique ID for the plugin (must be a GUID without leading and trailing braces)</param>
        /// <param name="name">Plugin name (this will appear in the Plugins preferences panel)</param>
        /// <param name="imagePath">Path to embedded image resource used to represent this plugin within the
        /// Open Live Writer UI (menu bitmap, sidebar bitmap, etc.). The size of the embedded image must be 20x18.</param>
        [Obsolete("This method is for compatibility with pre-beta plugins, and will be removed in the future.")]
        public WriterPluginAttribute(string id, string name, string imagePath)
        {
            Id = id;
            Name = name;
            ImagePath = imagePath;
        }

        /// <summary>
        /// Unique ID for the plugin (should be a GUID without leading and trailing braces)
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("WriterPlugin.Id");

                if (!ValidateGuid(value))
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The value specifed ({0}) was not a GUID", value), "WriterPlugin.Id");

                _id = value;
            }
        }
        private string _id;

        /// <summary>
        /// Plugin name (this will appear in the Plugins preferences panel)
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
                    throw new ArgumentNullException("WriterPlugin.Name");

                _name = value;
            }
        }
        private string _name;

        /// <summary>
        /// Path to embedded image resource used to represent this plugin within the
        /// Open Live Writer UI (menu bitmap, sidebar bitmap, etc.). The size of
        /// the embedded image must be 20x18 or 16x16 pixels.
        /// </summary>
        /// <remarks>
        /// Early Beta versions of Open Live Writer required icons to be 20x18, but
        /// later versions prefer 16x16. Later versions of Writer will scale 20x18 images
        /// to 16x16, or, if only the center 16x16 pixels of the 20x18 are non-transparent,
        /// the image will simply be cropped to 16x16.
        /// </remarks>
        public string ImagePath
        {
            get
            {

                return _imagePath;
            }
            set
            {
                _imagePath = value;
            }
        }
        private string _imagePath;

        /// <summary>
        /// Short description (1-2 sentences) of the plugin (displayed in the Plugins Preferences panel). Optional.
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
                    throw new ArgumentNullException("WriterPlugin.Description");

                _description = value;
            }
        }
        private string _description = String.Empty;

        /// <summary>
        /// URL of the publisher for the Plugin (linked to from Plugins Preferences panel). Optional.
        /// </summary>
        public string PublisherUrl
        {
            get
            {
                return _publisherUrl;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("WriterPlugin.PublisherUrl");

                _publisherUrl = value;
            }
        }
        private string _publisherUrl = String.Empty;

        /// <summary>
        /// Indicates whether the Plugin has editable options. That is, whether it overrides the EditOptions
        /// method of the WriterPlugin base class. Default is false.
        /// </summary>
        public bool HasEditableOptions
        {
            get
            {
                return _hasEditableOptions;
            }
            set
            {
                _hasEditableOptions = value;
            }
        }
        private bool _hasEditableOptions = false;

        private bool ValidateGuid(string id)
        {
            try
            {
                Guid guidId = new Guid(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
