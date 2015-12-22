// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f429-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IElementBehaviorFactoryRaw
    {
        // Fix bug 519990: Setting ppBehavior to null, even in the failure case,
        // causes Writer to crash when an embedded Google Map is pasted into the
        // editor. If there is no behavior, DON'T TOUCH ppBehavior!
        void FindBehavior(
            [In, MarshalAs(UnmanagedType.BStr)] string bstrBehavior,
            [In, MarshalAs(UnmanagedType.BStr)] string bstrBehaviorUrl,
            [In] IElementBehaviorSite pSite,
            [In, Out] ref IElementBehaviorRaw ppBehavior);
    }
}

