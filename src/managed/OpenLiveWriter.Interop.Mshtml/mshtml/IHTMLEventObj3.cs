// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F680-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLEventObj3
    {
        [DispId(0x40e)]
        bool contentOverflow {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40e)] get; }
        [DispId(0x40f)]
        bool shiftLeft {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40f)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x40f)] get; }
        [DispId(0x410)]
        bool altLeft {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x410)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x410)] get; }
        [DispId(0x411)]
        bool ctrlLeft {[param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x411)] set;[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x411)] get; }
        [ComAliasName("mshtml.LONG_PTR"), DispId(0x412)]
        int imeCompositionChange {[return: ComAliasName("mshtml.LONG_PTR")] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x441), DispId(0x412)] get; }
        [DispId(0x413), ComAliasName("mshtml.LONG_PTR")]
        int imeNotifyCommand {[return: ComAliasName("mshtml.LONG_PTR")] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x441), DispId(0x413)] get; }
        [DispId(0x414), ComAliasName("mshtml.LONG_PTR")]
        int imeNotifyData {[return: ComAliasName("mshtml.LONG_PTR")] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x441), DispId(0x414)] get; }
        [ComAliasName("mshtml.LONG_PTR"), DispId(0x416)]
        int imeRequest {[return: ComAliasName("mshtml.LONG_PTR")] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x416), TypeLibFunc((short)0x441)] get; }
        [ComAliasName("mshtml.LONG_PTR"), DispId(0x417)]
        int imeRequestData {[return: ComAliasName("mshtml.LONG_PTR")] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x417), TypeLibFunc((short)0x441)] get; }
        [DispId(0x415), ComAliasName("mshtml.LONG_PTR")]
        int keyboardLayout {[return: ComAliasName("mshtml.LONG_PTR")] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x415), TypeLibFunc((short)0x441)] get; }
        [DispId(0x418)]
        int behaviorCookie {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x418)] get; }
        [DispId(0x419)]
        int behaviorPart {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x419)] get; }
        [DispId(0x41a)]
        string nextPage {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x41a)] get; }
    }
}

