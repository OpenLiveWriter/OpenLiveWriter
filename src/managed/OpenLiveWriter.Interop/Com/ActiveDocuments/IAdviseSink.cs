// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    /// The IAdviseSink interface enables containers and other objects to receive
    /// notifications of data changes, view changes, and compound-document changes
    /// occurring in objects of interest. Container applications, for example,
    /// require such notifications to keep cached presentations of their linked
    /// and embedded objects up-to-date. Calls to IAdviseSink methods are
    /// asynchronous, so the call is sent and then the next instruction is
    /// executed without waiting for the call's return.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010f-0000-0000-C000-000000000046")]
    public interface IAdviseSink
    {
        [PreserveSig]
        void OnDataChange(
            [In] ref FORMATETC pFormatetc,
            [In] ref STGMEDIUM pStgmed);

        [PreserveSig]
        void OnViewChange(
            [In] DVASPECT dwAspect,
            [In] int lindex);

        [PreserveSig]
        void OnRename(
            [In] IMoniker pmk);

        [PreserveSig]
        void OnSave();

        [PreserveSig]
        void OnClose();
    }
}
