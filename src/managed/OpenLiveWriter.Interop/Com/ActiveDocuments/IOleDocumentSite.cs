// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    /// <summary>
    /// The IOleDocumentSite interface enables a document that has been
    /// implemented as a document object to bypass the normal activation
    /// sequence for in-place-active objects and to directly instruct its
    /// client site to activate it as a document object. A client site with
    /// this ability is called a document site.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b722bcc7-4e68-101b-a2bc-00aa00404770")]
    public interface IOleDocumentSite
    {
        void ActivateMe(
            [In] IOleDocumentView pViewToActivate);
    }

}
