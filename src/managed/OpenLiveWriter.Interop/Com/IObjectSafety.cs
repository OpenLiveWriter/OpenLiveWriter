// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// The IObjectSafety interface provides methods to retrieve and set safety options.
    ///
    /// For details, see: http://msdn.microsoft.com/workshop/components/com/reference/ifaces/iobjectsafety/iobjectsafety.asp
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CB5BDC81-93C1-11cf-8F20-00805F2CD064")]
    public interface IObjectSafety
    {
        /// <summary>
        /// Retrieves the safety options supported by an object as well as the safety options that are currently set for that object.
        /// </summary>
        [PreserveSig]
        int GetInterfaceSafetyOptions(
            [In] ref Guid riid,
            [In, Out] ref uint pdwSupportedOptions,
            [In, Out] ref uint pdwEnabledOptions
            );

        /// <summary>
        /// Makes an object safe for initialization or scripting
        /// </summary>
        [PreserveSig]
        int SetInterfaceSafetyOptions(
            [In] ref Guid riid,
            uint dwOptionSetMask,
            uint dwEnabledOptions
            );
    };

    /// <summary>
    /// Flags that indicate what type of safe access the object supports or is enabled for.
    /// </summary>
    public struct INTERFACESAFE
    {
        public const int FOR_UNTRUSTED_CALLER = 0x00000001;
        public const int FOR_UNTRUSTED_DATA = 0x00000002;
    }

}
