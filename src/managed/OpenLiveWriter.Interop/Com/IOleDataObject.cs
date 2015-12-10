// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{

    /// <summary>
    /// Version of IOleDataObject that preserves all of the raw signatures so that
    /// implementors can return the appropriate values
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010e-0000-0000-C000-000000000046")]
    public interface IOleDataObject
    {
        /// <summary>
        /// Renders the data described in a FORMATETC structure and transfers it
        /// through the STGMEDIUM structure.
        /// </summary>
        /// <param name="pFormatEtc">Pointer to the FORMATETC structure that defines
        /// the format, medium, and target device to use when passing the data. It is
        /// possible to specify more than one medium by using the Boolean OR operator,
        /// allowing the method to choose the best medium among those specified</param>
        /// <param name="pMedium">Pointer to the STGMEDIUM structure that indicates
        /// the storage medium containing the returned data through its tymed member,
        /// and the responsibility for releasing the medium through the value of its
        /// pUnkForRelease member. If pUnkForRelease is NULL, the receiver of the medium
        /// is responsible for releasing it; otherwise, pUnkForRelease points to the
        /// IUnknown on the appropriate object so its Release method can be called.
        /// The medium must be allocated and filled in by IDataObject::GetData</param>
        [PreserveSig]
        int GetData(ref FORMATETC pFormatEtc, ref STGMEDIUM pMedium);

        /// <summary>
        /// Renders the data described in a FORMATETC structure and transfers it
        /// through the STGMEDIUM structure allocated by the caller.
        /// </summary>
        /// <param name="pFormatEtc">Pointer to the FORMATETC structure that defines
        /// the format, medium, and target device to use when passing the data. It is
        /// possible to specify more than one medium by using the Boolean OR operator,
        /// allowing the method to choose the best medium among those specified</param>
        /// <param name="pMedium">Pointer to the STGMEDIUM structure that defines the
        /// storage medium containing the data being transferred. The medium must be
        /// allocated by the caller and filled in by IDataObject::GetDataHere. The
        /// caller must also free the medium. The implementation of this method must
        /// always supply a value of NULL for the punkForRelease member of the
        /// STGMEDIUM structure to which this parameter points</param>
        [PreserveSig]
        int GetDataHere(ref FORMATETC pFormatEtc, ref STGMEDIUM pMedium);

        /// <summary>
        /// Determines whether the data object is capable of rendering the data
        /// described in the FORMATETC structure.
        /// </summary>
        /// <param name="pFormatEtc">Pointer to the FORMATETC structure defining
        /// the format, medium, and target device to use for the query</param>
        [PreserveSig]
        int QueryGetData(ref FORMATETC pFormatEtc);

        /// <summary>
        /// Provides a standard FORMATETC structure that is logically equivalent
        /// to one that is more complex. You use this method to determine whether
        /// two different FORMATETC structures would return the same data, removing
        /// the need for duplicate rendering
        /// </summary>
        /// <param name="pFormatEtcIn">Pointer to the FORMATETC structure that
        /// defines the format, medium, and target device that the caller would
        /// like to use to retrieve data in a subsequent call such as
        /// IDataObject::GetData. The TYMED member is not significant in this case
        /// and should be ignored</param>
        /// <param name="pFormatEtcOut">Pointer to a FORMATETC structure that contains
        /// the most general information possible for a specific rendering, making it
        /// canonically equivalent to pFormatetcIn. The caller must allocate this
        /// structure and the GetCanonicalFormatEtc method must fill in the data.
        /// To retrieve data in a subsequent call like IDataObject::GetData, the
        /// caller uses the supplied value of pFormatetcOut, unless the value supplied
        /// is NULL. This value is NULL if the method returns DATA_S_SAMEFORMATETC.
        /// The TYMED member is not significant in this case and should be ignored</param>
        /// <returns>S_OK if the logically equivilant structure was provided,
        /// otherwise returns DATA_S_SAMEFORMATETC indicating the structures
        /// are the same (in this case pFormatEtcOut is NULL)</returns>
        [PreserveSig]
        int GetCanonicalFormatEtc(ref FORMATETC pFormatEtcIn, ref FORMATETC pFormatEtcOut);

        /// <summary>
        /// Provides the source data object with data described by a FORMATETC
        /// structure and an STGMEDIUM structure
        /// </summary>
        /// <param name="pFormatEtc">Pointer to the FORMATETC structure defining the
        /// format used by the data object when interpreting the data contained in the
        /// storage medium</param>
        /// <param name="pMedium">Pointer to the STGMEDIUM structure defining the storage
        /// medium in which the data is being passed</param>
        /// <param name="fRelease">If TRUE, the data object called, which implements
        /// IDataObject::SetData, owns the storage medium after the call returns. This
        /// means it must free the medium after it has been used by calling the
        /// ReleaseStgMedium function. If FALSE, the caller retains ownership of the
        /// storage medium and the data object called uses the storage medium for the
        /// duration of the call only</param>
        [PreserveSig]
        int SetData(ref FORMATETC pFormatEtc, ref STGMEDIUM pMedium,
            [MarshalAs(UnmanagedType.Bool)] bool fRelease);

        /// <summary>
        /// Creates and returns a pointer to an object to enumerate the FORMATETC
        /// supported by the data object
        /// </summary>
        /// <param name="dwDirection">Direction of the data through a value from
        /// the enumeration DATADIR</param>
        /// <param name="ppEnumFormatEtc">Address of IEnumFORMATETC* pointer variable
        /// that receives the interface pointer to the new enumerator object</param>
        [PreserveSig]
        int EnumFormatEtc(DATADIR dwDirection, out IEnumFORMATETC ppEnumFormatEtc);

        /// <summary>
        /// Creates a connection between a data object and an advise sink so the
        /// advise sink can receive notifications of changes in the data object
        /// </summary>
        /// <param name="pFormatEtc">Pointer to a FORMATETC structure that defines the
        /// format, target device, aspect, and medium that will be used for future
        /// notifications. For example, one sink may want to know only when the bitmap
        /// representation of the data in the data object changes. Another sink may be
        /// interested in only the metafile format of the same object. Each advise sink
        /// is notified when the data of interest changes. This data is passed back to
        /// the advise sink when notification occurs</param>
        /// <param name="advf">DWORD that specifies a group of flags for controlling
        /// the advisory connection. Valid values are from the enumeration ADVF.
        /// However, only some of the possible ADVF values are relevant for this
        /// method (see MSDN documentation for more details).</param>
        /// <param name="pAdvSink">Pointer to the IAdviseSink interface on the advisory
        /// sink that will receive the change notification</param>
        /// <param name="pdwConnection">Pointer to a DWORD token that identifies this
        /// connection. You can use this token later to delete the advisory connection
        /// (by passing it to IDataObject::DUnadvise). If this value is zero, the
        /// connection was not established</param>
        [PreserveSig]
        int DAdvise(ref FORMATETC pFormatEtc, uint advf, IntPtr pAdvSink, ref uint pdwConnection);

        /// <summary>
        /// Destroys a notification previously set up with the DAdvise method
        /// </summary>
        /// <param name="dwConnection">DWORD token that specifies the connection to remove.
        /// Use the value returned by IDataObject::DAdvise when the connection was originally
        /// established</param>
        [PreserveSig]
        int DUnadvise(uint dwConnection);

        /// <summary>
        /// Creates and returns a pointer to an object to enumerate the current
        /// advisory connections
        /// </summary>
        /// <param name="ppEnumAdvise">Address of IEnumSTATDATA* pointer variable that
        /// receives the interface pointer to the new enumerator object. If the
        /// implementation sets *ppenumAdvise to NULL, there are no connections to
        /// advise sinks at this time</param>
        [PreserveSig]
        int EnumDAdvise(ref IntPtr ppEnumAdvise);
    }

    /// <summary>
    /// Clipboard format strings
    /// </summary>
    public struct CF
    {
        public const string HTML = "HTML Format";
        public const string DRAGCONTEXT = "DragContext";

        // Standard formats defined in WinUser.h
        public const short TEXT = 1;
        public const short UNICODETEXT = 13;
    }

    /// <summary>
    /// EnumFormatEtc data-direction enumeration
    /// </summary>
    public enum DATADIR : uint
    {
        GET = 1,
        SET = 2,
    };

    /// <summary>
    /// Data status values
    /// </summary>
    public struct DATA_S
    {
        /// <summary>
        /// Data has same FORMATETC
        /// </summary>
        public const int SAMEFORMATETC = unchecked((int)0x00040130L);
    }

    /// <summary>
    /// Error codes returned by IDataObject methods
    /// </summary>
    public struct DV_E
    {

        /// <summary>
        /// Invalid value for pFormatetc (this is value returned by GetData,
        /// GetDataHere, or QueryGetData when the requested clipboard format /
        /// storage medium cannot be accomodated
        /// </summary>
        public const int FORMATETC = unchecked((int)0x80040064);

        public const int TARGETDEVICE = unchecked((int)0x80040065);

        public const int TYMED = unchecked((int)0x80040069);
    }

    /// <summary>
    /// The FORMATETC structure is a generalized Clipboard format. It is enhanced to
    /// encompass a target device, the aspect or view of the data, and a storage
    /// medium indicator. Where one might expect to find a Clipboard format, OLE
    /// uses a FORMATETC data structure instead. This structure is used as a parameter
    /// in OLE functions and methods that require data format information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FORMATETC
    {
        /// <summary>
        /// Particular clipboard format of interest. There are three types of formats
        /// recognized by OLE:
        ///	  * Standard interchange formats, such as CF_TEXT.
        ///   * Private application formats understood only by the application offering
        ///     the format, or by other applications offering similar features.
        ///   * OLE formats, which are used to create linked or embedded objects.
        ///  Note that the type of this parameter is CLIPFORMAT. CLIPFORMAT is defined
        ///  as a WORD (or ushort) however the RegisterClipboardFormat function returns
        ///  a uint (which presumably must be down-converted to ushort). Ugliness.
        ///  Anyway, we define this as uint to avoid the downcast -- this gives us
        ///  the correct structure format because packing automatically aligns the
        ///  next field on a 4-byte boundary
        /// </summary>
        public ushort cfFormat;

        /// <summary>
        /// Pointer to a DVTARGETDEVICE structure containing information about the
        /// target device for which the data is being composed. A NULL value is used
        /// whenever the specified data format is independent of the target device
        /// or when the caller doesn't care what device is used
        /// </summary>
        public IntPtr ptd;

        /// <summary>
        /// One of the DVASPECT enumeration constants that indicate how much detail
        /// should be contained in the rendering
        /// </summary>
        public DVASPECT dwAspect;

        /// <summary>
        /// Part of the aspect when the data must be split across page boundaries.
        /// The most common value is -1, which identifies all of the data
        /// </summary>
        public int lindex;

        /// <summary>
        /// One of the TYMED enumeration constants which indicate the type of storage
        /// medium used to transfer the object's data
        /// </summary>
        public TYMED tymed;
    }

    /// <summary>
    /// This structure is a generalized global memory handle used for data transfer
    /// operations by the IAdviseSink, IDataObject, and IOleCache interfaces
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct STGMEDIUM
    {
        /// <summary>
        /// Type of storage medium. The marshaling and unmarshaling routines use this
        /// value to determine which union member was used.
        /// </summary>
        public TYMED tymed;

        /// <summary>
        /// Handle, string, or interface pointer that the receiving process can use to
        /// access the data being transferred. If this is an interface pointer you
        /// will need to call Marshal.GetObjectForIUnknown. If it is a string you will
        /// need to call Marshal.PtrToStringUni.
        /// </summary>
        public IntPtr contents;

        /// <summary>
        /// Pointer to an interface instance that allows the sending process to control
        /// the way the storage is released when the receiving process calls the
        /// ReleaseStgMedium function. If pUnkForRelease is NULL, ReleaseStgMedium uses
        /// default procedures to release the storage; otherwise, ReleaseStgMedium uses
        /// the specified IUnknown interface
        /// </summary>
        public IntPtr pUnkForRelease;
    }

    /// <summary>
    /// The TYMED enumeration values indicate the type of storage medium being used
    /// in a data transfer.
    /// </summary>
    [Flags]
    public enum TYMED : uint
    {
        /// <summary>
        /// No data is being passed
        /// </summary>
        NULL = 0,

        /// <summary>
        /// The storage medium is a global memory handle (HGLOBAL). Allocate the global
        /// handle with the GMEM_SHARE flag. If the STGMEDIUM punkForRelease member is
        /// NULL, the destination process should use GlobalFree to release the memory
        /// </summary>
        HGLOBAL = 1,

        /// <summary>
        /// The storage medium is a disk file identified by a path. If the STGMEDIUM
        /// punkForRelease member is NULL, the destination process should use OpenFile
        /// to delete the file
        /// </summary>
        FILE = 2,

        /// <summary>
        /// The storage medium is a stream object identified by an IStream pointer.
        /// Use ISequentialStream::Read to read the data. If the STGMEDIUM punkForRelease
        /// member is not NULL, the destination process should use IStream::Release to
        /// release the stream component
        /// </summary>
        ISTREAM = 4,

        /// <summary>
        /// The storage medium is a storage component identified by an IStorage pointer.
        /// The data is in the streams and storages contained by this IStorage instance.
        /// If the STGMEDIUM punkForRelease member is not NULL, the destination process
        /// should use IStorage::Release to release the storage component
        /// </summary>
        ISTORAGE = 8,

        /// <summary>
        /// The storage medium is a GDI component (HBITMAP). If the STGMEDIUM
        /// punkForRelease member is NULL, the destination process should use DeleteObject
        /// to delete the bitmap
        /// </summary>
        GDI = 16,

        /// <summary>
        /// The storage medium is a metafile (HMETAFILE). Use the Windows or WIN32 functions
        /// to access the metafile's data. If the STGMEDIUM punkForRelease member is NULL,
        /// the destination process should use DeleteMetaFile to delete the bitmap
        /// </summary>
        MFPICT = 32,

        /// <summary>
        /// The storage medium is an enhanced metafile. If the STGMEDIUM punkForRelease
        /// member is NULL, the destination process should use DeleteEnhMetaFile to delete
        /// the bitmap
        /// </summary>
        ENHMF = 64
    }

    /// <summary>
    /// The DVASPECT enumeration values specify the desired data or view aspect of the
    /// object when drawing or getting data
    /// </summary>
    public enum DVASPECT : uint
    {
        /// <summary>
        /// Provides a representation of an object so it can be displayed as an embedded
        /// object inside of a container. This value is typically specified for compound
        /// document objects. The presentation can be provided for the screen or printer
        /// </summary>
        CONTENT = 1,

        /// <summary>
        /// Provides a thumbnail representation of an object so it can be displayed in a
        /// browsing tool. The thumbnail is approximately a 120 by 120 pixel, 16-color
        /// (recommended) device-independent bitmap potentially wrapped in a metafile
        /// </summary>
        THUMBNAIL = 2,

        /// <summary>
        /// Provides an iconic representation of an object
        /// </summary>
        ICON = 4,

        /// <summary>
        /// Provides a representation of the object on the screen as though it were
        /// printed to a printer using the Print command from the File menu. The
        /// described data may represent a sequence of pages
        /// </summary>
        DOCPRINT = 8
    }
}
