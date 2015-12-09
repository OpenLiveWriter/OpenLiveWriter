// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, ComConversionLoss, Guid("3050F6A7-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IHTMLPaintSite
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InvalidatePainterInfo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InvalidateRect([In] ref tagRECT prcInvalid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InvalidateRegion([In] IntPtr rgnInvalid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDrawInfo([In] int lFlags, out _HTML_PAINT_DRAW_INFO pDrawInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TransformGlobalToLocal([In] tagPOINT ptGlobal, out tagPOINT pptLocal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TransformLocalToGlobal([In] tagPOINT ptLocal, out tagPOINT pptGlobal);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetHitTestCookie(out int plCookie);
    }
}

