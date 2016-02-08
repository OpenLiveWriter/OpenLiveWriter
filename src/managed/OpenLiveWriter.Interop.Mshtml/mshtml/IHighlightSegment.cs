// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F690-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)1)]
    public interface IHighlightSegment : ISegment
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        new void GetPointers([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEnd);
    }
}

