// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short)0x1040), Guid("3050F658-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLCurrentStyle2
    {
        [DispId(-2147412957)]
        string layoutFlow {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412957)] get; }
        [DispId(-2147412954)]
        string wordWrap {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412954)] get; }
        [DispId(-2147412953)]
        string textUnderlinePosition {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412953)] get; }
        [DispId(-2147412952)]
        bool hasLayout {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412952)] get; }
        [DispId(-2147412932)]
        object scrollbarBaseColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412932)] get; }
        [DispId(-2147412931)]
        object scrollbarFaceColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412931)] get; }
        [DispId(-2147412930)]
        object scrollbar3dLightColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412930), TypeLibFunc((short)20)] get; }
        [DispId(-2147412929)]
        object scrollbarShadowColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412929)] get; }
        [DispId(-2147412928)]
        object scrollbarHighlightColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412928)] get; }
        [DispId(-2147412927)]
        object scrollbarDarkShadowColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412927)] get; }
        [DispId(-2147412926)]
        object scrollbarArrowColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412926), TypeLibFunc((short)20)] get; }
        [DispId(-2147412916)]
        object scrollbarTrackColor {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412916), TypeLibFunc((short)20)] get; }
        [DispId(-2147412920)]
        string writingMode {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412920), TypeLibFunc((short)20)] get; }
        [DispId(-2147412959)]
        object zoom {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147412959)] get; }
        [DispId(-2147413030)]
        string filter {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)20), DispId(-2147413030)] get; }
        [DispId(-2147412909)]
        string textAlignLast {[return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412909), TypeLibFunc((short)20)] get; }
        [DispId(-2147412908)]
        object textKashidaSpace {[return: MarshalAs(UnmanagedType.Struct)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-2147412908), TypeLibFunc((short)20)] get; }
        [DispId(-2147412904)]
        bool isBlock {[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x455), DispId(-2147412904)] get; }
    }
}

