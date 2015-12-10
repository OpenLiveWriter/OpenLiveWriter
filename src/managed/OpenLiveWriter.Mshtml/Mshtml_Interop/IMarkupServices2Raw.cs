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
    [Guid("3050f682-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IMarkupServices2Raw
    {
        void CreateMarkupPointer(
            [Out] out IMarkupPointerRaw ppPointer);

        void CreateMarkupContainer(
            [Out] out IMarkupContainer ppMarkupContainer);

        void CreateElement(
            [In] _ELEMENT_TAG_ID tagID,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pchAttributes,
            [Out] out IHTMLElement ppElement);

        void CloneElement(
            [In] IHTMLElement pElemCloneThis,
            [Out] out IHTMLElement ppElementTheClone);

        void InsertElement(
            [In] IHTMLElement pElementInsert,
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish);

        void RemoveElement(
            [In] IHTMLElement pElementRemove);

        void Remove(
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish);

        void Copy(
            [In] IMarkupPointerRaw pPointerSourceStart,
            [In] IMarkupPointerRaw pPointerSourceFinish,
            [In] IMarkupPointerRaw pPointerTarget);

        void Move(
            [In] IMarkupPointerRaw pPointerSourceStart,
            [In] IMarkupPointerRaw pPointerSourceFinish,
            [In] IMarkupPointerRaw pPointerTarget);

        void InsertText(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pchText,
            [In] int cch,
            [In] IMarkupPointerRaw pPointerTarget);

        void ParseString(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pchHTML,
            [In] uint dwFlags,
            [Out] out IMarkupContainer ppContainerResult,
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish);

        void ParseGlobal(
            [In] IntPtr hglobalHTML,
            [In] uint dwFlags,
            [Out] out IMarkupContainer ppContainerResult,
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish);

        void IsScopedElement(
            [In] IHTMLElement pElement,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfScoped);

        void GetElementTagId(
            [In] IHTMLElement pElement,
            [Out] out _ELEMENT_TAG_ID ptagId);

        void GetTagIDForName(
            [In, MarshalAs(UnmanagedType.BStr)] string bstrName,
            [Out] out _ELEMENT_TAG_ID ptagId);

        /// <summary>
        /// Note: use Marshal.PtrToStringBSTR to convert the returned IntPtr into
        /// a .NET string variable.
        /// </summary>
        void GetNameForTagID(
            [In] _ELEMENT_TAG_ID tagId,
            [Out] out IntPtr pbstrName);

        void MovePointersToRange(
            [In] IHTMLTxtRange pIRange,
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish);

        void MoveRangeToPointers(
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish,
            [In] IHTMLTxtRange pIRange);

        void BeginUndoUnit(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pchTitle);

        void EndUndoUnit();

        void ParseGlobalEx(
            [In] IntPtr hglobalHTML,
            [In] uint dwFlags,
            [In] IMarkupContainerRaw pContext,
            [Out] out IMarkupContainerRaw ppContainerResult,
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish);

        void ValidateElements(
            [In] IMarkupPointerRaw pPointerStart,
            [In] IMarkupPointerRaw pPointerFinish,
            [In] IMarkupPointerRaw pPointerTarget,
            [In, Out] ref IMarkupPointerRaw pPointerStatus,
            [Out] out IHTMLElement ppElemFailBottom,
            [Out] out IHTMLElement ppElemFailTop);

        void SaveSegmentsToClipboard(
            [In] ISegmentList pSegmentList,
            [In] uint dwFlags);
    }
}

