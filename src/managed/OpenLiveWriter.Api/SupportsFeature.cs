// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Specifies whether a feature is supported by a publishing context.
    /// </summary>
    public enum SupportsFeature
    {
        /// <summary>
        /// Support for the feature is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The feature is supported.
        /// </summary>
        Yes,

        /// <summary>
        /// The feature is not supported.
        /// </summary>
        No
    }
}