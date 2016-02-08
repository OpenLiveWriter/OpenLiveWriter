// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Mshtml
{
    [ComImport, InterfaceType((short)1), Guid("3050F683-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface ISegmentRaw
    {
        void GetPointers(
            [In] IMarkupPointerRaw pIStart,
            [In] IMarkupPointerRaw pIEnd);
    }
}
