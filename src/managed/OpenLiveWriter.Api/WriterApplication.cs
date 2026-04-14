// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Provides the ability to query whether the Writer application is installed.
    /// Platform-specific launch operations (NewPost, OpenPost, ShowOptions) are
    /// available through IPlatformServices on Windows where COM automation is supported.
    /// </summary>
    public sealed class WriterApplication
    {
        /// <summary>
        /// Is Open Live Writer currently installed.
        /// </summary>
        public static bool IsInstalled
        {
            get
            {
                if (!PlatformContext.IsInitialized)
                    return false;

                return PlatformContext.Services.IsApplicationInstalled();
            }
        }
    }
}
