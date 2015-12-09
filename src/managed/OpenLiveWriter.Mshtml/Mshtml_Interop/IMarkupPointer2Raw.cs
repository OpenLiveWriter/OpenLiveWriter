// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f675-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IMarkupPointer2Raw
    {
        void OwningDoc(
            [Out] out IHTMLDocument2 ppDoc);

        void Gravity(
            [Out] out _POINTER_GRAVITY pGravity);

        void SetGravity(
            [In] _POINTER_GRAVITY Gravity);

        void Cling(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfCling);

        void SetCling(
            [In, MarshalAs(UnmanagedType.Bool)] bool fCling);

        void Unposition();

        void IsPositioned(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfPositioned);

        void GetContainer(
            [Out] out IMarkupContainerRaw ppContainer);

        void MoveAdjacentToElement(
            [In] IHTMLElement pElement,
            [In] _ELEMENT_ADJACENCY eAdj);

        void MoveToPointer(
            [In] IMarkupPointerRaw pPointer);

        void MoveToContainer(
            [In] IMarkupContainerRaw pContainer,
            [In, MarshalAs(UnmanagedType.Bool)] bool fAtStart);

        void Left(
            [In, MarshalAs(UnmanagedType.Bool)] bool fMove,
            [Out] out _MARKUP_CONTEXT_TYPE pContext,
            [Out] out IHTMLElement ppElement,
            [In, Out] IntPtr pcch,
            [Out] IntPtr pchText);

        void Right(
            [In, MarshalAs(UnmanagedType.Bool)] bool fMove,
            [Out] out _MARKUP_CONTEXT_TYPE pContext,
            [Out] out IHTMLElement ppElement,
            [In, Out] IntPtr pcch,
            [Out] IntPtr pchText);

        void CurrentScope(
            [Out] out IHTMLElement ppElemCurrent);

        void IsLeftOf(
            [In] IMarkupPointerRaw pPointerThat,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfResult);

        void IsLeftOfOrEqualTo(
            [In] IMarkupPointerRaw pPointerThat,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfResult);

        void IsRightOf(
            [In] IMarkupPointerRaw pPointerThat,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfResult);

        void IsRightOfOrEqualTo(
            [In] IMarkupPointerRaw pPointerThat,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfResult);

        void IsEqualTo(
            [In] IMarkupPointerRaw pPointerThat,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAreEqual);

        void MoveUnit(
            [In] _MOVEUNIT_ACTION muAction);

        void FindText(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pchFindText,
            [In] uint dwFlags,
            [In] IMarkupPointerRaw pIEndMatch,
            [In] IMarkupPointerRaw pIEndSearch);

        void IsAtWordBreak(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAtBreak);

        void GetMarkupPosition(
            [Out] out int plMP);

        void MoveToMarkupPosition(
            [In] IMarkupContainerRaw pContainer,
            [In] int lMP);

        void MoveUnitBounded(
            [In] _MOVEUNIT_ACTION muAction,
            [In] IMarkupPointerRaw pIBoundary);

        void IsInsideURL(
            [In] IMarkupPointerRaw pRight,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfResult);

        void MoveToContent(
            [In] IHTMLElement pIElement,
            [In, MarshalAs(UnmanagedType.Bool)] bool fAtStart);
    }
}

