// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, ComConversionLoss, Guid("3050F6A6-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IHTMLPainter
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Draw([In] tagRECT rcBounds, [In] tagRECT rcUpdate, [In] int lDrawFlags, [In, ComAliasName("mshtml.wireHDC")] ref _RemotableHandle hdc, [In] IntPtr pvDrawObject);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void onresize([In] tagSIZE size);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPainterInfo(out _HTML_PAINTER_INFO pInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void HitTestPoint([In] tagPOINT pt, out int pbHit, out int plPartID);
    }
}

