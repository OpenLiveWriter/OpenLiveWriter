// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F6CF-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLWindow4
    {
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x49c)]
        object createPopup([In, Optional, MarshalAs(UnmanagedType.Struct)] ref object varArgIn);
        [DispId(0x49d)]
        IHTMLFrameBase frameElement { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x49d)] get; }
    }
}

