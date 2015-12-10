// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// General purpose implementation of an Ole DataObject that accepts arbirary
    /// binary data (the .NET IDataObject implementation does not allow you to pass
    /// arbitrary binary data so if you want to do this you need a class like this).
    /// This implementation does not support advise sinks.
    /// </summary>
    public class OleDataObjectImpl : IOleDataObject, IDisposable
    {
        /// <summary>
        /// Create an OleDataObjectImpl
        /// </summary>
        public OleDataObjectImpl()
        {
        }

        /// <summary>
        /// Dispose the data object
        /// </summary>
        public void Dispose()
        {
            if (oleDataEntries != null)
            {
                foreach (OleDataEntry dataEntry in oleDataEntries)
                    Ole32.ReleaseStgMedium(ref dataEntry.stgm);
                oleDataEntries = null;
            }
        }

        /// <summary>
        /// Verify the user called Dispose at garbage collection time
        /// </summary>
        ~OleDataObjectImpl()
        {
            Debug.Assert(oleDataEntries == null, "You must call Dispose on OleDataObjectImpl when finished using it!");
        }

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
        public int GetData(ref FORMATETC pFormatEtc, ref STGMEDIUM pMedium)
        {
            // check to see if we have data of the requested type
            int dataFormatIndex;
            int result = FindDataFormat(ref pFormatEtc, out dataFormatIndex);

            // if we do then return a clone of it (returns error code if an
            // error occurs during the clone)
            if (result == HRESULT.S_OK)
            {
                // lookup the entry
                OleDataEntry dataEntry = (OleDataEntry)oleDataEntries[dataFormatIndex];

                // clone the storage and return
                return CloneStgMedium(dataEntry.stgm, ref pMedium);
            }
            // don't have the data, return the error code passed back to us
            // from FindDataFormat
            else
            {
                return result;
            }
        }

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
        public int GetDataHere(ref FORMATETC pFormatEtc, ref STGMEDIUM pMedium)
        {
            // For now we don't support this method. MFC uses the internal method
            // AfxCopyStgMedium to implement this -- if we absolutely positively
            // need to suppport this then we should base are implementation on
            // that code (source is the file atlmfc\src\mfc\olemisc.cpp)
            return HRESULT.E_NOTIMPL;
        }

        /// <summary>
        /// Determines whether the data object is capable of rendering the data
        /// described in the FORMATETC structure.
        /// </summary>
        /// <param name="pFormatEtc">Pointer to the FORMATETC structure defining
        /// the format, medium, and target device to use for the query</param>
        public int QueryGetData(ref FORMATETC pFormatEtc)
        {
            int dataFormatIndex;
            return FindDataFormat(ref pFormatEtc, out dataFormatIndex);
        }

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
        public int GetCanonicalFormatEtc(ref FORMATETC pFormatEtcIn, ref FORMATETC pFormatEtcOut)
        {
            return DATA_S.SAMEFORMATETC;
        }

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
        public int SetData(ref FORMATETC pFormatEtc, ref STGMEDIUM pMedium, bool fRelease)
        {
            // check and see if we have an existing format of this type
            int dataFormatIndex;
            int result = FindDataFormat(ref pFormatEtc, out dataFormatIndex);

            // if we have an existing format of this type then free it and
            // remove it from the list
            if (result == HRESULT.S_OK)
            {
                OleDataEntry oleDataEntry = (OleDataEntry)oleDataEntries[dataFormatIndex];
                Ole32.ReleaseStgMedium(ref oleDataEntry.stgm);
                oleDataEntries.RemoveAt(dataFormatIndex);
            }

            // create an entry to add to our internal list
            OleDataEntry dataEntry;

            // if the caller is releasing the data that is being set then just
            // copy bit for bit (we are now responsible for freeing the storage)
            if (fRelease)
            {
                dataEntry = new OleDataEntry(pFormatEtc, pMedium);
            }

            // if the caller is not releasing the data object to us then
            // we only get to use it for the duration of the call -- we need
            // to therefore clone the storage so that we have our own
            // copy/reference
            else
            {
                // attempt to clone the storage medium
                STGMEDIUM mediumClone = new STGMEDIUM();
                result = CloneStgMedium(pMedium, ref mediumClone);
                if (result != HRESULT.S_OK)
                    return result;

                // cloned it, initialize the data entry using the cloned storage
                dataEntry = new OleDataEntry(pFormatEtc, mediumClone);
            }

            // add the entry to our internal list
            oleDataEntries.Add(dataEntry);

            // return OK
            return HRESULT.S_OK;
        }

        /// <summary>
        /// Creates and returns a pointer to an object to enumerate the FORMATETC
        /// supported by the data object
        /// </summary>
        /// <param name="dwDirection">Direction of the data through a value from
        /// the enumeration DATADIR</param>
        /// <param name="ppEnumFormatEtc">Address of IEnumFORMATETC* pointer variable
        /// that receives the interface pointer to the new enumerator object</param>
        public int EnumFormatEtc(DATADIR dwDirection, out IEnumFORMATETC ppEnumFormatEtc)
        {
            // don't support enumeration of set formats
            if (dwDirection == DATADIR.SET)
            {
                ppEnumFormatEtc = null;
                return HRESULT.E_NOTIMPL;
            }

            // return a new enumerator for our data entries
            IEnumFORMATETC enumerator = new EnumFORMATETC(oleDataEntries);
            enumerators.Add(enumerator);
            ppEnumFormatEtc = enumerator;
            return HRESULT.S_OK;
        }

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
        public int DAdvise(ref FORMATETC pFormatEtc, uint advf, IntPtr pAdvSink, ref uint pdwConnection)
        {
            return OLE_E.ADVISENOTSUPPORTED;
        }

        /// <summary>
        /// Destroys a notification previously set up with the DAdvise method
        /// </summary>
        /// <param name="dwConnection">DWORD token that specifies the connection to remove.
        /// Use the value returned by IDataObject::DAdvise when the connection was originally
        /// established</param>
        public int DUnadvise(uint dwConnection)
        {
            return OLE_E.ADVISENOTSUPPORTED;
        }

        /// <summary>
        /// Creates and returns a pointer to an object to enumerate the current
        /// advisory connections
        /// </summary>
        /// <param name="ppEnumAdvise">Address of IEnumSTATDATA* pointer variable that
        /// receives the interface pointer to the new enumerator object. If the
        /// implementation sets *ppenumAdvise to NULL, there are no connections to
        /// advise sinks at this time</param>
        public int EnumDAdvise(ref IntPtr ppEnumAdvise)
        {
            return OLE_E.ADVISENOTSUPPORTED;
        }

        /// <summary>
        /// Private helper method to find an existing data format
        /// </summary>
        /// <param name="pFormatEtc">format spec</param>
        /// <param name="dataIndex">returned index of data format if we've got it,
        /// -1 if we don't have it</param>
        /// <returns>S_OK if the data format was found, otherwise the appropriate
        /// OLE error code (see QueryGetData for documentation on error codes)</returns>
        private int FindDataFormat(ref FORMATETC pFormatEtc, out int dataIndex)
        {
            // default to data not found
            dataIndex = -1;

            // no support for comparing target devices
            if (pFormatEtc.ptd != IntPtr.Zero)
                return DV_E.TARGETDEVICE;

            // iterate through our FORMATETC structures to see if one matches
            // this format spec
            for (int i = 0; i < oleDataEntries.Count; i++)
            {
                // get the data entry
                OleDataEntry dataEntry = (OleDataEntry)oleDataEntries[i];

                // check for matching format spec
                if ((dataEntry.format.cfFormat == pFormatEtc.cfFormat) &&
                    (dataEntry.format.dwAspect == pFormatEtc.dwAspect) &&
                    (dataEntry.format.lindex == pFormatEtc.lindex))
                {
                    // check for matching data type
                    if ((dataEntry.format.tymed & pFormatEtc.tymed) > 0)
                    {
                        dataIndex = i;
                        return HRESULT.S_OK;
                    }
                    else
                        return DV_E.TYMED;
                }
            }

            // no matching format found
            return DV_E.FORMATETC;
        }

        /// <summary>
        /// Create a cloned copy of the the passed storage medium. This method works via
        /// a combination of actually copying underling data and incrementing reference
        /// counts on embedded objects.
        /// </summary>
        /// <param name="stgmIn">storage medium in</param>
        /// <param name="stgmOut">storage medium out</param>
        /// <returns>HRESULT.S_OK if the medium was successfully cloned, various
        /// OLE error codes if an error occurs during the clone </returns>
        private int CloneStgMedium(STGMEDIUM stgmIn, ref STGMEDIUM stgmOut)
        {
            // copy storage type
            stgmOut.tymed = stgmIn.tymed;

            // copy or add ref count to the actual data
            switch (stgmIn.tymed)
            {
                // global memory blocks get copied
                case TYMED.HGLOBAL:
                    using (HGlobalLock input = new HGlobalLock(stgmIn.contents))
                        stgmOut.contents = input.Clone();
                    break;

                // COM interfaces get copied w/ their ref-count incremented
                case TYMED.ISTREAM:
                case TYMED.ISTORAGE:
                    stgmOut.contents = stgmIn.contents;
                    Marshal.AddRef(stgmOut.contents);
                    break;

                // don't know how to clone other storage medium types (return error)
                case TYMED.ENHMF:
                case TYMED.FILE:
                case TYMED.GDI:
                case TYMED.MFPICT:
                default:
                    return DV_E.TYMED;
            }

            // copy pUnkForRelease and add a reference count on it if there is one
            stgmOut.pUnkForRelease = stgmIn.pUnkForRelease;
            if (stgmOut.pUnkForRelease != IntPtr.Zero)
                Marshal.AddRef(stgmOut.pUnkForRelease);

            // return success
            return HRESULT.S_OK;
        }

        /// <summary>
        /// Data entries contained within the data object
        /// </summary>
        private ArrayList oleDataEntries = new ArrayList();

        /// <summary>
        /// Track the enumerators that we have returned so that a .NET reference
        /// is maintained to them
        /// </summary>
        private ArrayList enumerators = new ArrayList();

    }

    /// <summary>
    /// Class which represents a data entry being managed by this class
    /// </summary>
    internal class OleDataEntry
    {
        public OleDataEntry(FORMATETC fmt, STGMEDIUM stg)
        {
            format = fmt;
            stgm = stg;
        }
        public FORMATETC format = new FORMATETC();
        public STGMEDIUM stgm = new STGMEDIUM();
    }

    /// <summary>
    /// Implementation of IEnumFORMATETC for OleDataEntry list
    /// </summary>
    internal class EnumFORMATETC : IEnumFORMATETC
    {
        /// <summary>
        /// Initialize with an array list of OleDataEntry objects
        /// </summary>
        /// <param name="entries"></param>
        public EnumFORMATETC(ArrayList entries)
        {
            oleDataEntries = entries;
        }

        /// <summary>
        /// Get the next celt entries from the enumeration
        /// </summary>
        /// <param name="celt">entries to fetch</param>
        /// <param name="rgelt">array to fetch into (allocated by caller)</param>
        /// <param name="pceltFetched">number of entries fetched</param>
        /// <returns>S_OK if celt entries are supplied, otherwise S_FALSE</returns>
        public int Next(uint celt, FORMATETC[] rgelt, IntPtr pceltFetched)
        {
            // see how many of the requested items we can serve
            int itemsRequested = Convert.ToInt32(celt);
            int itemsToReturn = Math.Min(itemsRequested, oleDataEntries.Count - (currentItem + 1));

            // copy the format structures into the caller's array of structures
            for (int i = 0; i < itemsToReturn; i++)
            {
                OleDataEntry dataEntry = (OleDataEntry)oleDataEntries[++currentItem];
                rgelt[i] = dataEntry.format;
            }

            // update the fetch parameter if requested
            if (pceltFetched != IntPtr.Zero)
                Marshal.WriteInt32(pceltFetched, itemsToReturn);

            // return the correct status code depending upon whether we
            // returned all of the items requested
            if (itemsToReturn == itemsRequested)
                return HRESULT.S_OK;
            else
                return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Skip the next celt entries
        /// </summary>
        /// <param name="celt"></param>
        [PreserveSig]
        public int Skip(uint celt)
        {
            currentItem += Convert.ToInt32(celt);
            if (currentItem < oleDataEntries.Count)
                return HRESULT.S_OK;
            else
                return HRESULT.S_FALSE;
        }

        /// <summary>
        /// Reset to the beginning of the enumeration
        /// </summary>
        public void Reset()
        {
            currentItem = -1;
        }

        /// <summary>
        /// Clone the current enumerator by returning another enumerator with its
        /// position the same as the current one
        /// </summary>
        /// <returns></returns>
        public void Clone(out IEnumFORMATETC ppenum)
        {
            EnumFORMATETC clone = new EnumFORMATETC(oleDataEntries);
            clone.currentItem = currentItem;
            ppenum = clone;
        }

        /// <summary>
        /// List of data entries we are enumerating
        /// </summary>
        private ArrayList oleDataEntries;

        /// <summary>
        /// Item are enumerator is currently positioned over
        /// </summary>
        private int currentItem = -1;

    }

}

