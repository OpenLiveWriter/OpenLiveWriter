// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f692-98b5-11cf-bb82-00aa00bdce0b")]
    public interface ISegmentListIteratorRaw
    {
        void Current(
            [Out] out ISegment ppISegment);

        void First();

        [PreserveSig]
        int IsDone();

        [PreserveSig]
        int Advance();
    }
}

