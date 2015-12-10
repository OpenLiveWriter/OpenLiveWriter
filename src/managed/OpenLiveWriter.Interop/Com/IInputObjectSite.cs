// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Interface used to communicate focus changes to the DeskBand's site
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("f1db8392-7331-11d0-8c99-00a0c92dbfe8")]
    public interface IInputObjectSite
    {
        /// <summary>
        /// Informs the browser that the focus has changed. PreserveSig attribute is
        /// used to suppress spurious COM errors from throwing exceptions (this was
        /// used in the C# DeskBand example -- not sure if it is a necessary precaution)
        /// TODO: test whether PreserveSig is necessary
        /// </summary>
        /// <param name="punkObj">Address of object gaining or losing the focus</param>
        /// <param name="fSetFocus">Non-zero to indicate the object has gained focus,
        /// zero to indicate it has lost focus</param>
        /// <returns>HRESULT indicating error status</returns>
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        Int32 OnFocusChangeIS(
            [MarshalAs(UnmanagedType.IUnknown)] Object punkObj, Int32 fSetFocus);
    }
}
