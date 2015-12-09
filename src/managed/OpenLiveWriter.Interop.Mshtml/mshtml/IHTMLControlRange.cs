// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, DefaultMember("item"), Guid("3050F29C-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLControlRange
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ea)]
        void select();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3eb)]
        void add([In, MarshalAs(UnmanagedType.Interface)] IHTMLControlElement item);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ec)]
        void remove([In] int index);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
        IHTMLElement item([In] int index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ee)]
        void scrollIntoView([In, Optional, MarshalAs(UnmanagedType.Struct)] object varargStart);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ef)]
        bool queryCommandSupported([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f0)]
        bool queryCommandEnabled([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f1)]
        bool queryCommandState([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f2)]
        bool queryCommandIndeterm([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f3)]
        string queryCommandText([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f4)]
        object queryCommandValue([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f5)]
        bool execCommand([In, MarshalAs(UnmanagedType.BStr)] string cmdID, [In, Optional] bool showUI /* = false */, [In, Optional, MarshalAs(UnmanagedType.Struct)] object value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f6)]
        bool execCommandShowHelp([In, MarshalAs(UnmanagedType.BStr)] string cmdID);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3f7)]
        IHTMLElement commonParentElement();
        [DispId(0x3ed)]
        int length {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x3ed)] get; }
    }
}

