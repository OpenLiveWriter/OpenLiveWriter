// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short)1), Guid("3050F604-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IHTMLCaret
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveCaretToPointer([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer, [In] int fScrollIntoView, [In] _CARET_DIRECTION eDir);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveCaretToPointerEx([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer, [In] int fVisible, [In] int fScrollIntoView, [In] _CARET_DIRECTION eDir);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveMarkupPointerToCaret([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIMarkupPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveDisplayPointerToCaret([In, MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsVisible(out int pIsVisible);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Show([In] int fScrollIntoView);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Hide();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void InsertText([In] ref ushort pText, [In] int lLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void scrollIntoView();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetLocation(out tagPOINT pPoint, [In] int fTranslate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCaretDirection(out _CARET_DIRECTION peDir);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCaretDirection([In] _CARET_DIRECTION eDir);
    }
}

