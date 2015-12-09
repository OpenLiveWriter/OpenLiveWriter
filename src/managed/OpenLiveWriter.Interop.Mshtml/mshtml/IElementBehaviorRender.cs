// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F4AA-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IElementBehaviorRender
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Draw([In, ComAliasName("mshtml.wireHDC")] ref _RemotableHandle hdc, [In] int lLayer, [In] ref tagRECT pRect, [In, MarshalAs(UnmanagedType.IUnknown)] object pReserved);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetRenderInfo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int HitTestPoint([In] ref tagPOINT pPoint, [In, MarshalAs(UnmanagedType.IUnknown)] object pReserved);
    }
}

