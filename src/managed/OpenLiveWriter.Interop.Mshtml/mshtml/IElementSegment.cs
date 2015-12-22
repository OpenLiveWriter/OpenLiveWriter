// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short)1), Guid("3050F68F-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IElementSegment : ISegment
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        new void GetPointers([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEnd);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetElement([MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppIElement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetPrimary([In] int fPrimary);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsPrimary(out int pfPrimary);
    }
}

