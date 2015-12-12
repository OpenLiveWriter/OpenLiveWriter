// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing the UI of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("bd3f23c0-d43e-11cf-893b-00aa00bdce1a")]
    public interface IDocHostUIHandler
    {
        [PreserveSig]
        int ShowContextMenu(
            [In] int dwID,
            [In] ref POINT ppt,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pcmdtReserved,
            [In, MarshalAs(UnmanagedType.IDispatch)] object pdispReserved);

        void GetHostInfo(
            [Out][In] ref DOCHOSTUIINFO pInfo);

        [PreserveSig]
        int ShowUI(
            [In] DOCHOSTUITYPE dwID,
            [In] IOleInPlaceActiveObject pActiveObject,
            [In] IOleCommandTarget pCommandTarget,
            [In] IOleInPlaceFrame pFrame,
            [In] IOleInPlaceUIWindow pDoc);

        void HideUI();

        void UpdateUI();

        void EnableModeless(
            [In, MarshalAs(UnmanagedType.Bool)] bool fEnable);

        void OnDocWindowActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

        void OnFrameWindowActivate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

        void ResizeBorder(
            [In] ref RECT prcBorder,
            [In] IOleInPlaceUIWindow pUIWindow,
            [In, MarshalAs(UnmanagedType.Bool)] bool frameWindow);

        [PreserveSig]
        int TranslateAccelerator(
            [In] ref MSG lpMsg,
            [In] ref Guid pguidCmdGroup,
            [In] uint nCmdID);

        void GetOptionKeyPath(
            [Out] out IntPtr pchKey,
            [In] uint dwReserved);

        [PreserveSig]
        int GetDropTarget(
            [In] IDropTarget pDropTarget,
            [Out] out IDropTarget ppDropTarget);

        void GetExternal(
            [Out] out IntPtr ppDispatch);

        [PreserveSig]
        int TranslateUrl(
            [In] uint dwReserved,
            [In] IntPtr pchURLIn,
            [Out] out IntPtr ppchURLOut);

        [PreserveSig]
        int FilterDataObject(
            [In] IOleDataObject pDO,
            [Out] out IOleDataObject ppDORet);
    }

    /// <summary>
    /// Context menu constants
    /// </summary>
    public struct CONTEXT_MENU
    {
        public static int DEFAULT = 0;
        public static int IMAGE = 1;
        public static int CONTROL = 2;
        public static int TABLE = 3;
        public static int TEXTSELECT = 4;
        public static int ANCHOR = 5;
        public static int UNKNOWN = 6;
    }

    /// <summary>
    /// ShowContextMenu constants (used in call to IDocHostUIHandler.ShowContextMenu, derived
    /// directly from CONTEXT_MENU constants)
    /// </summary>
    public struct SHOW_CONTEXT_MENU
    {
        public static int DEFAULT = 0x01 << CONTEXT_MENU.DEFAULT;
        public static int IMAGE = 0x01 << CONTEXT_MENU.IMAGE;
        public static int CONTROL = 0x01 << CONTEXT_MENU.CONTROL;
        public static int TABLE = 0x01 << CONTEXT_MENU.TABLE;
        public static int TEXTSELECT = 0x01 << CONTEXT_MENU.TEXTSELECT;
        public static int ANCHOR = 0x01 << CONTEXT_MENU.ANCHOR;
        public static int UNKNOWN = 0x01 << CONTEXT_MENU.UNKNOWN;
    }

    /// <summary>
    /// Used by the IDocHostUIHandler::GetHostInfo method to allow MSHTML to retrieve information about
    /// the host's UI requirements.
    /// </summary>
    public struct DOCHOSTUIINFO
    {
        public uint cbSize;
        public DOCHOSTUIFLAG dwFlags;
        public DOCHOSTUIDBLCLK dwDoubleClick;
        public IntPtr pchHostCss;
        public IntPtr pchHostNS;
    };

    /// <summary>
    /// Type of UI being displayed by MSHTML
    /// </summary>
    public enum DOCHOSTUITYPE : uint
    {
        BROWSE = 0,
        AUTHOR = 1
    }

    /// <summary>
    /// Defines values used to indicate the proper action on a double-click event.
    /// </summary>
    public enum DOCHOSTUIDBLCLK : uint
    {
        DEFAULT = 0,
        SHOWPROPERTIES = 1,
        SHOWCODE = 2,
    }

    /// <summary>
    /// Defines a set of flags that indicate the capabilities of an IDocHostUIHandler implementation.
    /// </summary>
    [Flags]
    public enum DOCHOSTUIFLAG : uint
    {
        NONE = 0x00000000,
        DIALOG = 0x00000001,
        DISABLE_HELP_MENU = 0x00000002,
        NO3DBORDER = 0x00000004,
        SCROLL_NO = 0x00000008,
        DISABLE_SCRIPT_INACTIVE = 0x00000010,
        OPENNEWWIN = 0x00000020,
        DISABLE_OFFSCREEN = 0x00000040,
        FLAT_SCROLLBAR = 0x00000080,
        DIV_BLOCKDEFAULT = 0x00000100,
        ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
        OVERRIDEBEHAVIORFACTORY = 0x00000400,
        CODEPAGELINKEDFONTS = 0x00000800,
        URL_ENCODING_DISABLE_UTF8 = 0x00001000,
        URL_ENCODING_ENABLE_UTF8 = 0x00002000,
        ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
        ENABLE_INPLACE_NAVIGATION = 0x00010000,
        IME_ENABLE_RECONVERSION = 0x00020000,
        THEME = 0x00040000,
        NOTHEME = 0x00080000,
        NOPICS = 0x00100000,
        NO3DOUTERBORDER = 0x00200000,
        DISABLE_EDIT_NS_FIXUP = 0x00400000,
        LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
        DISABLE_UNTRUSTEDPROTOCOL = 0x01000000,
        DPI_AWARE = 0x04000000,
    }

}
