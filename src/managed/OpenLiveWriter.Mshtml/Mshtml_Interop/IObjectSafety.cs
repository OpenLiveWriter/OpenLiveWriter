// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CB5BDC81-93C1-11cf-8F20-00805F2CD064")]
    public interface IObjectSafety
    {
        void GetInterfaceSafetyOptions(
            [In] ref Guid riid,
            [Out] out uint pdwSupportedOptions,
            [Out] out uint pdwEnabledOptions);

        void SetInterfaceSafetyOptions(
            [In] ref Guid riid,
            [In] uint dwOptionSetMask,
            [In] uint dwEnabledOptions);
    }

}

