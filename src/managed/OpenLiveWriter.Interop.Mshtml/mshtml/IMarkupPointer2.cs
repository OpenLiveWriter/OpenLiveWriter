// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F675-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IMarkupPointer2 : IMarkupPointer
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void OwningDoc([MarshalAs(UnmanagedType.Interface)] out IHTMLDocument2 ppDoc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void Gravity(out _POINTER_GRAVITY pGravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void SetGravity([In] _POINTER_GRAVITY Gravity);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void Cling(out int pfCling);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void SetCling([In] int fCLing);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void Unposition();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsPositioned(out int pfPositioned);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void GetContainer([MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppContainer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveAdjacentToElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement, [In] _ELEMENT_ADJACENCY eAdj);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveToPointer([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveToContainer([In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pContainer, [In] int fAtStart);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void left([In] int fMove, out _MARKUP_CONTEXT_TYPE pContext, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement, [In, Out] ref int pcch, out ushort pchText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void right([In] int fMove, out _MARKUP_CONTEXT_TYPE pContext, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement, [In, Out] ref int pcch, out ushort pchText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void CurrentScope([MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElemCurrent);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsLeftOf([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsLeftOfOrEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsRightOf([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsRightOfOrEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfAreEqual);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveUnit([In] _MOVEUNIT_ACTION muAction);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void findText([In] ref ushort pchFindText, [In] uint dwFlags, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndMatch, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndSearch);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsAtWordBreak(out int pfAtBreak);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetMarkupPosition(out int plMP);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveToMarkupPosition([In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pContainer, [In] int lMP);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveUnitBounded([In] _MOVEUNIT_ACTION muAction, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIBoundary);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsInsideURL([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pRight, out int pfResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MoveToContent([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement, [In] int fAtStart);
    }
}

