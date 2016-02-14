// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Interface used to process accelerators and change UI activation for a band object
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("68284faa-6a48-11d0-8c78-00c04fd918b4")]
    public interface IInputObject
    {
        /// <summary>
        /// Activates or de-activates the object
        /// </summary>
        /// <param name="fActivate">The value is non-zero if the band is being activated;
        /// otherwise, it is zero</param>
        /// <param name="msg">This is a pointer to a MSG structure that contains the message
        /// that caused the change in activation states</param>
        void UIActivateIO(Int32 fActivate, ref MSG msg);

        /// <summary>
        /// Determines if one of the object's windows has the keyboard focus. Note: PreserveSig
        /// attribute is used to prevent .NET COM Interop from throwing an exception when
        /// we return S_FALSE.
        /// </summary>
        /// <returns>S_OK if one of the object's windows has keyboard focus,
        /// otherwise S_FALSE</returns>
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        Int32 HasFocusIO();

        /// <summary>
        /// Passes keyboard accelerators to the object. Note: PreserveSig attribute is used to
        /// prevent .NET COM Interop from throwing an exception when we return S_FALSE.
        /// </summary>
        /// <param name="msg">Structure containing the keyboard message to be translated</param>
        /// <returns>S_OK if the accelerator was translated; otherwise S_FALSE</returns>
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        Int32 TranslateAcceleratorIO(ref MSG msg);
    }
}
