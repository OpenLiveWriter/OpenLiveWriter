// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    using System.Drawing;
    using mshtml;

    /// <summary>
    /// Holds the state of an image that was just inserted into the editor, but hasn't been initialized yet. This
    /// object is passed around and updated as the image is initialized.
    /// </summary>
    internal class NewImageInfo
    {
        /// <summary>
        /// Initializes a new instance of the NewImageInfo class.
        /// </summary>
        /// <param name="imageInfo">Information about the image.</param>
        /// <param name="element">A reference to the image's html element.</param>
        /// <param name="initialSize">The size at which this image should initially appear.</param>
        /// <param name="attachedBehavior">A reference to the behavior that has been manually attached to this image.</param>
        public NewImageInfo(ImagePropertiesInfo imageInfo, IHTMLElement element, Size initialSize, DisabledImageElementBehavior attachedBehavior)
        {
            this.ImageInfo = imageInfo;
            this.Element = element;
            this.InitialSize = initialSize;
            this.DisabledImageBehavior = attachedBehavior;
            this.Remove = false;
        }

        /// <summary>
        /// Gets information about the image.
        /// </summary>
        public ImagePropertiesInfo ImageInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the image's html element.
        /// </summary>
        public IHTMLElement Element
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size at which this image should initially appear.
        /// </summary>
        public Size InitialSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the behavior that has been manually attached to this image.
        /// </summary>
        public DisabledImageElementBehavior DisabledImageBehavior
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the image's file data.
        /// </summary>
        public BlogPostImageData ImageData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this image should be removed from the editor. Set this to true if
        /// there is an error while initializing the image.
        /// </summary>
        public bool Remove
        {
            get;
            set;
        }
    }
}
