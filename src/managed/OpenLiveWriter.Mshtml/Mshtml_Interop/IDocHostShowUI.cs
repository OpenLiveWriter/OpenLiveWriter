// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing the UI of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("c4d244b0-d43e-11cf-893b-00aa00bdce1a")]
    public interface IDocHostShowUI
    {
        [PreserveSig]
        int ShowMessage(
            [In] IntPtr hwnd,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrText,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrCaption,
            [In] uint dwType,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrHelpFile,
            [In] uint dwHelpContext,
            [Out] out int plResult);

        [PreserveSig]
        int ShowHelp(
            [In] IntPtr hwnd,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrHelpFile,
            [In] uint uCommand,
            [In] uint dwData,
            [In] POINT ptMouse,
            IntPtr pDispatchObjectHit);
    }
}

