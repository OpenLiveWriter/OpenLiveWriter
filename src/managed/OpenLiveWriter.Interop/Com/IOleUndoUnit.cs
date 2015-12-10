// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("894AD3B0-EF97-11CE-9BC9-00AA00608E01")]
    public interface IOleUndoUnit
    {
        void Do(IOleUndoManager undoManager);
        void GetDescription(
            [Out, MarshalAs(UnmanagedType.BStr)] out string description);
        void GetUnitType();
        void OnNextAdd();
    }
}
