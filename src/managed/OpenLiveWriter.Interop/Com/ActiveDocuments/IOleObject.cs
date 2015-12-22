// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000112-0000-0000-C000-000000000046")]
    public interface IOleObject
    {
        void SetClientSite(
            [In] IOleClientSite pClientSite);

        void GetClientSite(
            [Out] out IOleClientSite site);

        void SetHostNames(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerApp,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerObj);

        [PreserveSig]
        int Close(
            [In] OLECLOSE dwSaveOption);

        void SetMoniker(
            [In] OLEWHICHMK dwWhichMoniker,
            [In] IMoniker pmk);

        void GetMoniker(
            [In] OLEGETMONIKER dwAssign,
            [In] OLEWHICHMK dwWhichMoniker,
            [Out] out IMoniker moniker);

        [PreserveSig]
        int InitFromData(
            [In] IOleDataObject pDataObject,
            [In, MarshalAs(UnmanagedType.Bool)] bool fCreation,
            [In] uint dwReserved);

        [PreserveSig]
        int GetClipboardData(
            [In] uint dwReserved,
            [Out] out IOleDataObject data);

        [PreserveSig]
        int DoVerb(
            [In] int iVerb,
            [In] IntPtr lpmsg,
            [In] IOleClientSite pActiveSite,
            [In] int lindex,
            [In] IntPtr hwndParent,
            [In] ref RECT lprcPosRect);

        [PreserveSig]
        int EnumVerbs(
            [Out] out IEnumOLEVERB ppEnumOleVerb);

        [PreserveSig]
        int Update();

        [PreserveSig]
        int IsUpToDate();

        void GetUserClassID(
            [In, Out] ref Guid pClsid);

        [PreserveSig]
        void GetUserType(
            [In, MarshalAs(UnmanagedType.U4)] USERCLASSTYPE dwFormOfType,
            [Out] out IntPtr userType  /* (LPOLESTR*) */ );

        void SetExtent(
            [In] DVASPECT dwDrawAspect,
            [In] ref SIZEL pSizel);

        void GetExtent(
            [In] DVASPECT dwDrawAspect,
            [Out] out SIZEL pSizel);

        void Advise(
            [In] IAdviseSink pAdvSink,
            [Out] out uint pdwConnection);

        void Unadvise(
            [In] uint dwConnection);

        [PreserveSig]
        int EnumAdvise(
            [Out] out IEnumSTATDATA ppenumAdvise);

        [PreserveSig]
        int GetMiscStatus(
            [In] DVASPECT dwAspect,
            [Out] out uint pdwStatus);

        [PreserveSig]
        int SetColorScheme(
            [In] ref LOGPALETTE pLogpal);
    }

    /// <summary>
    /// The OLEVERB structure defines a verb that an object supports.
    /// The IOleObject::EnumVerbs method creates an enumerator that can enumerate
    /// these structures for an object, and supplies a pointer to the enumerator's
    /// IEnumOLEVERB.
    /// </summary>
    public struct OLEVERB
    {
        public int lVerb;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszVerbName;
        public uint fuFlags;
        public OLEVERBATTRIB grfAttribs;
    };

    /// <summary>
    /// Identifiers for DoVerb OLEVERBS
    /// </summary>
    public struct OLEIVERB
    {
        public const int PRIMARY = 0;
        public const int SHOW = -1;
        public const int OPEN = -2;
        public const int HIDE = -3;
        public const int UIACTIVATE = -4;
        public const int INPLACEACTIVATE = -5;
        public const int DISCARDUNDOSTATE = -6;
    }

    /// <summary>
    /// The OLEVERBATTRIB enumeration constants are used in the OLEVERB structure
    /// to describe the attributes of a specified verb for an object. Values are
    /// used in the enumerator (which supports the IEnumOLEVERB interface) that is
    /// created by a call to IOleObject::EnumVerbs.
    /// </summary>
    [Flags]
    public enum OLEVERBATTRIB : uint
    {
        NEVERDIRTIES = 1,
        ONCONTAINERMENU = 2
    };

    /// <summary>
    /// The STATDATA structure is the data structure used to specify each advisory
    /// connection. It is used for enumerating current advisory connections. It holds
    /// data returned by the IEnumSTATDATA enumerator. This enumerator interface is
    /// returned by IDataObject:DAdvise. Each advisory connection is specified by a
    /// unique STATDATA structure.
    /// </summary>
    public struct STATDATA
    {
        public FORMATETC formatetc;
        public ADVF grfAdvf;
        public IAdviseSink pAdvSink;
        public uint dwConnection;
    };

    /// <summary>
    /// The ADVF enumeration values are flags used by a container object to specify
    /// the requested behavior when setting up an advise sink or a caching connection
    /// with an object. These values have different meanings, depending on the type
    /// of connection in which they are used, and each interface uses its own subset
    /// of the flags.
    /// </summary>
    [Flags]
    public enum ADVF : uint
    {
        NODATA = 1,
        ONLYONCE = 2,
        PRIMEFIRST = 4,
        CACHE_NOHANDLER = 8,
        CACHE_FORCEBUILTIN = 16,
        CACHE_ONSAVE = 32,
        DATAONSTOP = 64
    };

    /// <summary>
    /// The OLECLOSE enumeration constants are used in the IOleObject::Close method to
    /// determine whether the object should be saved before closing
    /// </summary>
    public enum OLECLOSE : uint
    {
        SAVEIFDIRTY = 0,
        NOSAVE = 1,
        PROMPTSAVE = 2
    };

    /// <summary>
    /// The OLEWHICHMK enumeration constants indicate which part of an object's moniker
    /// is being set or retrieved. These constants are used in the IOleObject and
    /// IOleClientSite interfaces.
    /// </summary>
    public enum OLEWHICHMK : uint
    {
        CONTAINER = 1,
        OBJREL = 2,
        OBJFULL = 3
    };

    /// <summary>
    /// The OLEWHICHMK enumeration constants indicate which part of an object's moniker
    /// is being set or retrieved. These constants are used in the IOleObject and
    /// IOleClientSite interfaces
    /// </summary>
    public enum OLEGETMONIKER : uint
    {
        ONLYIFTHERE = 1,
        FORCEASSIGN = 2,
        UNASSIGN = 3,
        TEMPFORUSER = 4
    };

    /// <summary>
    /// The USERCLASSTYPE enumeration constants indicate the different variants of
    /// the display name associated with a class of objects. They are used in the
    /// IOleObject::GetUserType method and the OleRegGetUserType function.
    /// </summary>
    public enum USERCLASSTYPE : uint
    {
        FULL = 1,
        SHORT = 2,
        APPNAME = 3,
    };

    /// <summary>
    /// The LOGPALETTE structure defines a logical palette.
    /// </summary>
    public struct LOGPALETTE
    {
        public UInt16 palVersion;
        public UInt16 palNumEntries;
        public IntPtr palPalEntry;
    };

}
