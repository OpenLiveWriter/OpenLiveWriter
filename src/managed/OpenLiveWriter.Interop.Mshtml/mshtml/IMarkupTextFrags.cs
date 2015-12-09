// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F5FA-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IMarkupTextFrags
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTextFragCount(out int pcFrags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTextFrag([In] int iFrag, [MarshalAs(UnmanagedType.BStr)] out string pbstrFrag, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFrag);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveTextFrag([In] int iFrag);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InsertTextFrag([In] int iFrag, [In, MarshalAs(UnmanagedType.BStr)] string bstrInsert, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerInsert);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FindTextFragFromMarkupPointer([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFind, out int piFrag, out int pfFragFound);
    }
}

