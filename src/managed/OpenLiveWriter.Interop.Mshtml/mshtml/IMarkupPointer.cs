// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short)1), Guid("3050F49F-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IMarkupPointer
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OwningDoc([MarshalAs(UnmanagedType.Interface)] out IHTMLDocument2 ppDoc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Gravity(out _POINTER_GRAVITY pGravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetGravity([In] _POINTER_GRAVITY Gravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Cling(out int pfCling);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCling([In] int fCLing);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Unposition();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsPositioned(out int pfPositioned);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetContainer([MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppContainer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveAdjacentToElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement, [In] _ELEMENT_ADJACENCY eAdj);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveToPointer([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveToContainer([In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pContainer, [In] int fAtStart);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void left([In] int fMove, out _MARKUP_CONTEXT_TYPE pContext, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement, [In, Out] ref int pcch, out ushort pchText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void right([In] int fMove, out _MARKUP_CONTEXT_TYPE pContext, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement, [In, Out] ref int pcch, out ushort pchText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CurrentScope([MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElemCurrent);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsLeftOf([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsLeftOfOrEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsRightOf([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsRightOfOrEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfAreEqual);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveUnit([In] _MOVEUNIT_ACTION muAction);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void findText([In] ref ushort pchFindText, [In] uint dwFlags, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndMatch, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndSearch);
    }
}

