// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using JetBrains.Annotations;

    /// <summary>
    /// Provides read-only information about a category.
    /// </summary>
    public interface ICategoryInfo
    {
        /// <summary>
        /// Gets the ID of the category.
        /// </summary>
        [NotNull]
        string Id { get; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the category is newly created and does not exist on the server yet.
        /// </summary>
        bool IsNew { get; }
    }
}