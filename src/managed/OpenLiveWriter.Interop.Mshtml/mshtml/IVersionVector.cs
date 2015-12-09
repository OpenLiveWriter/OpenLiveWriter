// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;

namespace mshtml
{
    /// <summary>
    /// Interface used tell IE that we don't want VML support.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4EB01410-DB1A-11D1-BA53-00C04FC2040E")]
    public interface IVersionVector
    {
        void SetVersion(
            [In] string pchComponent,
            [In] string pchVersion);

        void GetVersion(
            [In] string pchComponent,
            [Out] out string pchVersion,
            [Out][In] ref uint pcchVersion);
    }
}
