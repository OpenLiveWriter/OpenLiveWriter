// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace mshtml
{
    /// <summary>
    /// Interface used tell IE that we don't want VML support.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("667115AC-DC02-11D1-BA57-00C04FC2040E")]
    public interface IVersionHost
    {
        void QueryUseLocalVersionVector(
            [Out] out bool pfUseLocal);

        void QueryVersionVector(
            [In] IVersionVector pVersion);
    }
}
