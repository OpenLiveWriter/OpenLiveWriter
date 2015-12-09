// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F645-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLSubmitData
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f4)]
        void appendNameValuePair([In, Optional, MarshalAs(UnmanagedType.BStr)] string name /* = "" */, [In, Optional, MarshalAs(UnmanagedType.BStr)] string value /* = "" */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f5)]
        void appendNameFilePair([In, Optional, MarshalAs(UnmanagedType.BStr)] string name /* = "" */, [In, Optional, MarshalAs(UnmanagedType.BStr)] string filename /* = "" */);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(0x3f6)]
        void appendItemSeparator();
    }
}

