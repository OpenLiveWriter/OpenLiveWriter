// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F32D-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLEventObj
    {
        [DispId(0x3e9)]
        IHTMLElement srcElement {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3e9)] get; }
        [DispId(0x3ea)]
        bool altKey {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ea)] get; }
        [DispId(0x3eb)]
        bool ctrlKey {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3eb)] get; }
        [DispId(0x3ec)]
        bool shiftKey {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ec)] get; }
        [DispId(0x3ef)]
        object returnValue {[param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ef)] set;[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ef)] get; }
        [DispId(0x3f0)]
        bool cancelBubble {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f0)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f0)] get; }
        [DispId(0x3f1)]
        IHTMLElement fromElement {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f1)] get; }
        [DispId(0x3f2)]
        IHTMLElement toElement {[return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f2)] get; }
        [DispId(0x3f3)]
        int keyCode {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f3)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f3)] get; }
        [DispId(0x3f4)]
        int button {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f4)] get; }
        [DispId(0x3f5)]
        string type {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f5)] get; }
        [DispId(0x3f6)]
        string qualifier {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f6)] get; }
        [DispId(0x3f7)]
        int reason {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f7)] get; }
        [DispId(0x3ed)]
        int x {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ed)] get; }
        [DispId(0x3ee)]
        int y {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ee)] get; }
        [DispId(0x3fc)]
        int clientX {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fc)] get; }
        [DispId(0x3fd)]
        int clientY {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fd)] get; }
        [DispId(0x3fe)]
        int offsetX {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3fe)] get; }
        [DispId(0x3ff)]
        int offsetY {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ff)] get; }
        [DispId(0x400)]
        int screenX {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x400)] get; }
        [DispId(0x401)]
        int screenY {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x401)] get; }
        [DispId(0x402)]
        object srcFilter {[return: MarshalAs(UnmanagedType.IDispatch)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x402)] get; }
    }
}

