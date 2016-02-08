// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text;

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short)1), Guid("3050F6C3-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLComputedStyle
    {
        [DispId(0x3e9)]
        bool bold {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3ea)]
        bool italic {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3eb)]
        bool underline {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3ec)]
        bool overline {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3ed)]
        bool strikeOut {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3ee)]
        bool subScript {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3ef)]
        bool superScript {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f0)]
        bool explicitFace {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f1)]
        int fontWeight {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f2)]
        int fontSize {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void fontName(StringBuilder fontName);
        [DispId(0x3f4)]
        bool hasBgColor {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f5)]
        int textColor {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f6)]
        int backgroundColor {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f7)]
        bool preFormatted {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f8)]
        bool direction {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3f9)]
        bool blockDirection {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [DispId(0x3fa)]
        bool OL {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] get; }
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void isEqual([In, MarshalAs(UnmanagedType.Interface)] IHTMLComputedStyle pComputedStyle, out bool pfEqual);
    }
}

