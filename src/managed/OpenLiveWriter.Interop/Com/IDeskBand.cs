// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Primary interface to a band object
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EB0FE172-1A3A-11D0-89B3-00A0C90A90AC")]
    public interface IDeskBand
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

        /// <summary>
        /// Get information about the DeskBand
        /// </summary>
        /// <param name="dwBandID">Identifier of the band. This value is assigned by the shell</param>
        /// <param name="dwViewMode">The view mode of the band object</param>
        /// <param name="pdbi">Reference to DESKBANDINFO structure to fill in</param>
        void GetBandInfo(
            UInt32 dwBandID,
            UInt32 dwViewMode,
            ref DESKBANDINFO pdbi);
    }

    /// <summary>
    /// Structure containing Desk Band information
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DESKBANDINFO
    {
        /// <summary>
        /// Flags that determine which members of the structure are being requested
        /// </summary>
        public DBIM dwMask;

        /// <summary>
        /// Minimum size of the band object
        /// </summary>
        public Point ptMinSize;

        /// <summary>
        /// Maximum size of the band object
        /// </summary>
        public Point ptMaxSize;

        /// <summary>
        /// Sizing step of the band object
        /// </summary>
        public Point ptIntegral;

        /// <summary>
        /// Ideal size of the band object (this size is not guaranteed)
        /// </summary>
        public Point ptActual;

        /// <summary>
        /// Title of the band object
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public String wszTitle;

        /// <summary>
        /// Mode of operations for the band object (can contain multiple values)
        /// </summary>
        public DBIMF dwModeFlags;

        /// <summary>
        /// Background color of the band object -- only valid if DBIMF.BKCOLOR is
        /// specified in dwModeFlags
        /// </summary>
        public Int32 crBkgnd;
    };

    /// <summary>
    /// Mask which determines which DESKBANDINFO fields are being requested
    /// </summary>
    [Flags]
    public enum DBIM : uint
    {
        /// <summary>
        /// ptMinSize is being requested
        /// </summary>
        MINSIZE = 0x0001,

        /// <summary>
        /// ptMaxSize is being requested
        /// </summary>
        MAXSIZE = 0x0002,

        /// <summary>
        /// ptIntegral is being requested
        /// </summary>
        INTEGRAL = 0x0004,

        /// <summary>
        /// ptActual is being requested
        /// </summary>
        ACTUAL = 0x0008,

        /// <summary>
        /// wszTitle is being requested
        /// </summary>
        TITLE = 0x0010,

        /// <summary>
        /// dwModeFlags is being requested
        /// </summary>
        MODEFLAGS = 0x0020,

        /// <summary>
        /// crBkgnd is being requested
        /// </summary>
        BKCOLOR = 0x0040
    }

    /// <summary>
    /// View Mode for DeskBands
    /// </summary>
    [Flags]
    public enum DBIF_VIEWMODE : uint
    {
        /// <summary>
        /// The band is being displayed horizontally
        /// </summary>
        NORMAL = 0x0000,

        /// <summary>
        /// The band is being displayed vertically
        /// </summary>
        VERTICAL = 0x0001,

        // <summary>
        /// The band is floating
        /// </summary>
        FLOATING = 0x0002,

        /// <summary>
        /// The band is being displayed tranparently
        /// </summary>
        TRANSPARENT = 0x0004
    }

    /// <summary>
    /// Mode of operation for a band object
    /// </summary>
    [Flags]
    public enum DBIMF : uint
    {
        /// <summary>
        /// Default normal behavior
        /// </summary>
        NORMAL = 0x0000,

        /// <summary>
        /// Undocumented
        /// </summary>
        FIXED = 0x0001,

        /// <summary>
        /// Undocumented
        /// </summary>
        FIXEDBMP = 0x0004,

        /// <summary>
        /// The height of the band object can be modified and the ptIntegral
        /// member denotes the increment by which the band object can be resized
        /// </summary>
        VARIABLEHEIGHT = 0x0008,

        /// <summary>
        /// Undocumented
        /// </summary>
        UNDELETEABLE = 0x0010,

        /// <summary>
        /// The band object will be displayed with a sunken look
        /// </summary>
        DEBOSSED = 0x0020,

        /// <summary>
        /// The band will be displayed with the background color crBkgnd
        /// </summary>
        BKCOLOR = 0x0040,

        /// <summary>
        /// Undocumented
        /// </summary>
        USECHEVRON = 0x0080,

        /// <summary>
        /// Undocumented
        /// </summary>
        BREAK = 0x0100,

        /// <summary>
        /// Undocumented
        /// </summary>
        DDTOFRONT = 0x0200,

        /// <summary>
        /// Undocumented
        /// </summary>
        TOPALIGN = 0x0400
    }

}
