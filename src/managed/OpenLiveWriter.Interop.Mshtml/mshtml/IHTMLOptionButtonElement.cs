// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F2BC-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLOptionButtonElement
    {
        [DispId(-2147413011)]
        string value { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147413011)] set; [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147413011), TypeLibFunc((short) 20)] get; }
        [DispId(0x7d0)]
        string type { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d0)] get; }
        [DispId(-2147418112)]
        string name { [param: In, MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147418112), TypeLibFunc((short) 20)] set; [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418112)] get; }
        [DispId(0x7d9)]
        bool @checked { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d9), TypeLibFunc((short) 4)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(0x7d9)] get; }
        [DispId(0x7d8)]
        bool defaultChecked { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d8), TypeLibFunc((short) 4)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(0x7d8)] get; }
        [DispId(-2147412082)]
        object onchange { [param: In, MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147412082)] set; [return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147412082), TypeLibFunc((short) 20)] get; }
        [DispId(-2147418036)]
        bool disabled { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418036)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 20), DispId(-2147418036)] get; }
        [DispId(0x7d1)]
        bool status { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d1)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d1)] get; }
        [DispId(0x7d7)]
        bool indeterminate { [param: In] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x7d7), TypeLibFunc((short) 4)] set; [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 4), DispId(0x7d7)] get; }
        [DispId(-2147416108)]
        IHTMLFormElement form { [return: MarshalAs(UnmanagedType.Interface)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147416108)] get; }
    }
}

