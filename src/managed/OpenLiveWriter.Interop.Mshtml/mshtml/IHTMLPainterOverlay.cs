// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F7E3-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IHTMLPainterOverlay
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void onmove([In] tagRECT rcDevice);
    }
}

