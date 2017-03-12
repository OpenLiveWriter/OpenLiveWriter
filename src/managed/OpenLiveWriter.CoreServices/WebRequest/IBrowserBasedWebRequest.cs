// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Common interface for performing HTTP GET and POST using
    /// the available browser context, if any.
    /// </summary>
    public interface IBrowserBasedWebRequest
    {
        Stream HttpGet(string url);
        Stream HttpPost(string url, Stream postData);
    }
}
