// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x1040), Guid("3050F4FF-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTCEventBehavior
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), DispId(-2147417612)]
        void fire([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pVar);
    }
}

