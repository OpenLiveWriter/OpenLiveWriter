// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices
{
    public sealed class PlatformHelper
    {
        /// <summary>
        /// Determines if the application is running on Windows 7
        /// </summary>
        public static bool RunningOnWin7OrHigher()
        {
            Version version = Environment.OSVersion.Version;
            return (version.Major >= 7) || (version.Major == 6 && version.Minor >= 1);
        }

        /// <summary>
        /// Throws PlatformNotSupportedException if the application is not running on Windows 7
        /// </summary>
        public static void ThrowIfNotWin7OrHigher()
        {
            if (!RunningOnWin7OrHigher())
            {
                throw new PlatformNotSupportedException("Only supported on Windows 7 or newer.");
            }
        }
    }

}
