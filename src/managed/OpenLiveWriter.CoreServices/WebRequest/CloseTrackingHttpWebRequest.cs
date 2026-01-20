// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Net;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// If you use this to wrap an HttpWebRequest, you'll get asserts
    /// if you get the response but forget to close it (or dispose the
    /// response stream--you just need to do one or the other).
    /// 
    /// Note: In .NET 10, the RealProxy-based implementation is not available.
    /// This is now a no-op stub that passes through the original request.
    /// For debugging unclosed responses, use HttpClient with IHttpClientFactory
    /// and proper logging instead.
    /// </summary>
    internal static class CloseTrackingHttpWebRequest
    {
        [Conditional("DEBUG")]
        public static void Wrap(ref HttpWebRequest request)
        {
            // No-op in .NET 10 - RealProxy is not available
            // The original request is used as-is
        }
    }
}
