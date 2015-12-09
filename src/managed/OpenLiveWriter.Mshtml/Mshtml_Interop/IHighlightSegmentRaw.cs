// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Mshtml
{
    [ComImport, Guid("3050F690-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)1)]
    public interface IHighlightSegmentRaw : ISegmentRaw
    {
        new void GetPointers(
            [In] IMarkupPointerRaw pIStart,
            [In] IMarkupPointerRaw pIEnd);
    }
}
