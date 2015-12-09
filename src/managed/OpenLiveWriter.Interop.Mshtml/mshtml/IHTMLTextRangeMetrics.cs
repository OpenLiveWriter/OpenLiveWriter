// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F40B-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLTextRangeMetrics
    {
        [DispId(0x40b)]
        int offsetTop { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40b)] get; }
        [DispId(0x40c)]
        int offsetLeft { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40c)] get; }
        [DispId(0x40d)]
        int boundingTop { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40d)] get; }
        [DispId(0x40e)]
        int boundingLeft { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40e)] get; }
        [DispId(0x40f)]
        int boundingWidth { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x40f)] get; }
        [DispId(0x410)]
        int boundingHeight { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x410)] get; }
    }
}

