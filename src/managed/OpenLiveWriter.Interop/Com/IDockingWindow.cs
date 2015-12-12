// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// COM interface to a docking window
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("012dd920-7b26-11d0-8ca9-00a0c92dbfe8")]
    public interface IDockingWindow
    {
        /// <summary>
        /// Return the window handle of the object implementing the DeskBand
        /// </summary>
        /// <param name="phwnd">Out parameter for window handle</param>
        void GetWindow(out IntPtr phwnd);

        /// <summary>
        /// Activate or de-activate context-sensitive help -- this
        /// method is NOT required for DeskBand implementations
        /// </summary>
        /// <param name="fEnterMode">Enter or exit help mode</param>
        void ContextSensitiveHelp([In] bool fEnterMode);

        /// <summary>
        /// Called when the DeskBand is supposed to be shown or hidden
        /// </summary>
        /// <param name="fShow">Deskband shown (true) or hidden (false)</param>
        void ShowDW([In] bool fShow);

        /// <summary>
        /// Called when the DeskBand is about to be closed
        /// </summary>
        /// <param name="dwReserved">Reserved--should always be zero</param>
        void CloseDW([In] UInt32 dwReserved);

        /// <summary>
        /// Notify docking window that the frame's border space has changed -- this
        /// method is NOT called for DeskBands.
        /// </summary>
        /// <param name="prcBorder">Ignored/unused for DeskBands</param>
        /// <param name="punkToolbarSite">Ignored/unused for DeskBands</param>
        /// <param name="fReserved">Ignored/unused for DeskBands</param>
        void ResizeBorderDW(
            IntPtr prcBorder,
            [In, MarshalAs(UnmanagedType.IUnknown)] Object punkToolbarSite,
            bool fReserved);
    }

}
