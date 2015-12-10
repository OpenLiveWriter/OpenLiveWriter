// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{

    /// <summary>
    /// Interface used for customizing editing behavior of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f663-98b5-11cf-bb82-00aa00bdce0b")]
    public interface IHTMLEditServicesRaw
    {
        void AddDesigner(
            [In] IHTMLEditDesignerRaw pIDesigner);

        void RemoveDesigner(
            [In] IHTMLEditDesignerRaw pIDesigner);

        void GetSelectionServices(
            [In] IMarkupContainerRaw pIContainer,
            [Out] out ISelectionServicesRaw ppSelSvc);

        void MoveToSelectionAnchor(
            [In] IMarkupPointerRaw pIStartAnchor);

        void MoveToSelectionEnd(
            [In] IMarkupPointerRaw pIEndAnchor);

        void SelectRange(
            [In] IMarkupPointerRaw pStart,
            [In] IMarkupPointerRaw pEnd,
            [In] _SELECTION_TYPE eType);
    }
}

