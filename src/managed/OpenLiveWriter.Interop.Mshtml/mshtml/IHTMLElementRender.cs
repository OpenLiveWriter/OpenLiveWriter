// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F669-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLElementRender
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DrawToDC([In, ComAliasName("mshtml.wireHDC")] ref _RemotableHandle hdc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetDocumentPrinter([In, MarshalAs(UnmanagedType.BStr)] string bstrPrinterName, [In, ComAliasName("mshtml.wireHDC")] ref _RemotableHandle hdc);
    }
}

