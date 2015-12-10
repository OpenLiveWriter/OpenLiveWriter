// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f6a0-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLEditHostRaw
    {
        [PreserveSig]
        int SnapRect(IHTMLElement pIElement, ref RECT prcNEW, _ELEMENT_CORNER elementCorner);
    }
}
