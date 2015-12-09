// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("E4E23071-4D07-11D2-AE76-0080C73BC199"), InterfaceType((short) 1)]
    public interface IXMLGenericParse
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetGenericParse([In] bool fDoGeneric);
    }
}

