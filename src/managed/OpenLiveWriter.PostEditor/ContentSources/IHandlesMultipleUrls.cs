// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    /// <summary>
    /// Summary description for IHandlesMultipleUrls.
    ///
    /// unpublished interface for internal plugins that can have multiple changing URL Regex's that they match
    ///
    /// object must be quick to instantiate and test
    /// </summary>
    public interface IHandlesMultipleUrls
    {
        bool HasUrlMatch(string url);
    }
}
