// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("3050F682-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IMarkupServices2 : IMarkupServices
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void CreateMarkupPointer([MarshalAs(UnmanagedType.Interface)] out IMarkupPointer ppPointer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void CreateMarkupContainer([MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppMarkupContainer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void createElement([In] _ELEMENT_TAG_ID tagID, [In] ref ushort pchAttributes, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void CloneElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElemCloneThis, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElementTheClone);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void InsertElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElementInsert, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void RemoveElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElementRemove);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void remove([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void Copy([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceFinish, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void move([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceFinish, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void InsertText([In] ref ushort pchText, [In] int cch, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void ParseString([In] ref ushort pchHTML, [In] uint dwFlags, [MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppContainerResult, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer ppPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer ppPointerFinish);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void ParseGlobal([In, ComAliasName("mshtml.wireHGLOBAL")] ref _userHGLOBAL hglobalHTML, [In] uint dwFlags, [MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppContainerResult, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void IsScopedElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement, out int pfScoped);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void GetElementTagId([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement, out _ELEMENT_TAG_ID ptagId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void GetTagIDForName([In, MarshalAs(UnmanagedType.BStr)] string bstrName, out _ELEMENT_TAG_ID ptagId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void GetNameForTagID([In] _ELEMENT_TAG_ID tagID, [MarshalAs(UnmanagedType.BStr)] out string pbstrName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MovePointersToRange([In, MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange pIRange, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void MoveRangeToPointers([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish, [In, MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange pIRange);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void BeginUndoUnit([In] ref ushort pchTitle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        new void EndUndoUnit();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ParseGlobalEx([In, ComAliasName("mshtml.wireHGLOBAL")] ref _userHGLOBAL hglobalHTML, [In] uint dwFlags, [In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pContext, [MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppContainerResult, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ValidateElements([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget, [In, Out, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStatus, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElemFailBottom, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElemFailTop);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SaveSegmentsToClipboard([In, MarshalAs(UnmanagedType.Interface)] ISegmentList pSegmentList, [In] uint dwFlags);
    }
}

