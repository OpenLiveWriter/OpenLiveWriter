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
    [Guid("3050f684-98b5-11cf-bb82-00aa00bdce0b")]
    public interface ISelectionServicesRaw
    {
        void SetSelectionType(
            [In] SELECTION_TYPE eType,
            [In] IntPtr pIListener);

        void GetMarkupContainer(
            [Out] out IMarkupContainerRaw ppIContainer);

        void AddSegment(
            [In] IMarkupPointerRaw pIStart,
            [In] IMarkupPointerRaw pIEnd,
            [Out] out ISegment ppISegmentAdded);

        void AddElementSegment(
            [In] IHTMLElement pIElement,
            [Out] out IElementSegment ppISegmentAdded);

        void RemoveSegment(
            [In] ISegment pISegment);

        void GetSelectionServicesListener(
            [Out] out ISelectionServicesListener ppISelectionServicesListener);
    }

    /// <summary>
    /// Enumeration that describes the selection type
    /// </summary>
    public enum SELECTION_TYPE
    {
        None = 0,
        Caret = 1,
        Text = 2,
        Control = 3
    };

}

