// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    [ComImport, Guid("3050F606-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)1)]
    public interface IHighlightRenderingServicesRaw
    {
        void AddSegment(
            [In] IDisplayPointerRaw pDispPointerStart,
            [In] IDisplayPointerRaw pDispPointerEnd,
            [In] IHTMLRenderStyle pIRenderStyle,
            out IHighlightSegmentRaw ppISegment);

        void MoveSegmentToPointers(
            [In] IHighlightSegmentRaw pISegment,
            [In] IDisplayPointerRaw pDispPointerStart,
            [In] IDisplayPointerRaw pDispPointerEnd);

        void RemoveSegment([In] IHighlightSegmentRaw pISegment);
    }
}
