// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    using JetBrains.Annotations;

    /// <summary>
    /// Exception thrown by the CreateContent methods of the ContentSource and SmartContentSource classes
    /// when they are unable to create content due to an error. Exceptions of this type are caught
    /// and displayed using a standard content-creation error dialog.
    /// </summary>
    [Serializable]
    public class ContentCreationException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCreationException"/> class.
        /// </summary>
        /// <param name="title">
        /// Title of exception (used as the caption of the error dialog).
        /// </param>
        /// <param name="description">
        /// Description of exception (used to provide additional details within the error dialog).
        /// </param>
        public ContentCreationException([CanBeNull] string title, [CanBeNull] string description)
            : base($"{title}: {description}")
        {
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCreationException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ContentCreationException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Title = info.GetString(nameof(this.Title));
            this.Description = info.GetString(nameof(this.Description));
        }

        /// <summary>
        /// Gets the description of exception (used to provide additional details within the error dialog).
        /// </summary>
        [CanBeNull]
        public string Description { get; }

        /// <summary>
        /// Gets the title of exception (used as the caption of the error dialog)
        /// </summary>
        [CanBeNull]
        public string Title { get; }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Title), this.Title, typeof(string));
            info.AddValue(nameof(this.Description), this.Description, typeof(string));
        }
    }
}
