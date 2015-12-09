// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1010), InterfaceType((short) 2), Guid("3050F6BD-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface HTMLNamespaceEvents
    {
        [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-609)]
        void onreadystatechange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
    }
}

