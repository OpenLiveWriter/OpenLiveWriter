// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    ///
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b722bcc5-4e68-101b-a2bc-00aa00404770")]
    public interface IOleDocument
    {
        void CreateView(
            [In] IOleInPlaceSite pIPSite,
            [In] IStream pstm,
            [In] uint dwReserved,
            [Out] out IOleDocumentView ppView);

        void GetDocMiscStatus(
            [Out] out DOCMISC pdwStatus);

        void EnumViews(
            [Out] out IEnumOleDocumentViews ppEnum,
            [Out] out IOleDocumentView ppView);
    }

    [Flags]
    public enum DOCMISC : uint
    {
        CANCREATEMULTIPLEVIEWS = 1,
        SUPPORTCOMPLEXRECTANGLES = 2,
        CANTOPENEDIT = 4,
        NOFILESUPPORT = 8
    };

}
