// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f425-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IElementBehaviorRaw
    {
        void Init(IElementBehaviorSite pBehaviorSite);

        void Notify(
            [In] int lEvent,
            [Out][In] IntPtr pVar);

        void Detach();
    }
}

