// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Specifies the desired interaction with the Internet cache during
    /// HTTP requests.
    /// </summary>
    public enum HttpRequestCacheLevel
    {
        /// <summary>
        /// Do not read from or write to the Internet cache.
        /// </summary>
        BypassCache,

        /// <summary>
        /// Read from the Internet cache if possible, otherwise retrieve
        /// from the network. If the request is successful then write the
        /// response to the Internet cache.
        /// </summary>
        CacheIfAvailable,

        /// <summary>
        /// Attempt to read the requested resource from the Internet cache
        /// (in no case attempt to retrieve the resource from the network).
        /// </summary>
        CacheOnly,

        /// <summary>
        /// Attempt to retrieve the requested resource from the network. If
        /// the request is successful then write the response to the Internet cache.
        /// </summary>
        Reload
    }
}