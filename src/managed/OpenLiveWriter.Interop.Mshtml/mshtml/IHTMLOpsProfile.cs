// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F401-98B5-11CF-BB82-00AA00BDCE0B"), TypeLibType((short)0x1040)]
    public interface IHTMLOpsProfile
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
        bool addRequest([In, MarshalAs(UnmanagedType.BStr)] string name, [In, Optional, MarshalAs(UnmanagedType.Struct)] object reserved);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
        void clearRequest();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
        void doRequest([In, MarshalAs(UnmanagedType.Struct)] object usage, [In, Optional, MarshalAs(UnmanagedType.Struct)] object fname, [In, Optional, MarshalAs(UnmanagedType.Struct)] object domain, [In, Optional, MarshalAs(UnmanagedType.Struct)] object path, [In, Optional, MarshalAs(UnmanagedType.Struct)] object expire, [In, Optional, MarshalAs(UnmanagedType.Struct)] object reserved);
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)]
        string getAttribute([In, MarshalAs(UnmanagedType.BStr)] string name);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)]
        bool setAttribute([In, MarshalAs(UnmanagedType.BStr)] string name, [In, MarshalAs(UnmanagedType.BStr)] string value, [In, Optional, MarshalAs(UnmanagedType.Struct)] object prefs);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)]
        bool commitChanges();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)]
        bool addReadRequest([In, MarshalAs(UnmanagedType.BStr)] string name, [In, Optional, MarshalAs(UnmanagedType.Struct)] object reserved);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)]
        void doReadRequest([In, MarshalAs(UnmanagedType.Struct)] object usage, [In, Optional, MarshalAs(UnmanagedType.Struct)] object fname, [In, Optional, MarshalAs(UnmanagedType.Struct)] object domain, [In, Optional, MarshalAs(UnmanagedType.Struct)] object path, [In, Optional, MarshalAs(UnmanagedType.Struct)] object expire, [In, Optional, MarshalAs(UnmanagedType.Struct)] object reserved);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)]
        bool doWriteRequest();
    }
}

