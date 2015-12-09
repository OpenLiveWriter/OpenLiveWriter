// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F3D1-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLStyleSheet2
    {
        [DispId(0x3f8)]
        HTMLStyleSheetPagesCollection pages { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f8)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f9)]
        int addPageRule([In, MarshalAs(UnmanagedType.BStr)] string bstrSelector, [In, MarshalAs(UnmanagedType.BStr)] string bstrStyle, [In, Optional] int lIndex /* = -1 */);
    }
}

