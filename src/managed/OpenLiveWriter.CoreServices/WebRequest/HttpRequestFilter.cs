// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Net;

    /// <summary>
    /// Delegate for augmenting and HTTP request.
    /// </summary>
    /// <param name="request">The request.</param>
    public delegate void HttpRequestFilter(HttpWebRequest request);
}