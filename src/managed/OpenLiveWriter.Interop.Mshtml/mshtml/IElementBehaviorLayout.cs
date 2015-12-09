// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F6BA-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IElementBehaviorLayout
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSize([In] int dwFlags, [In] tagSIZE sizeContent, [In, Out] ref tagPOINT pptTranslateBy, [In, Out] ref tagPOINT pptTopLeft, [In, Out] ref tagSIZE psizeProposed);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetLayoutInfo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPosition([In] int lFlags, [In, Out] ref tagPOINT pptTopLeft);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MapSize([In] ref tagSIZE psizeIn, out tagRECT prcOut);
    }
}

