// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F699-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short)1)]
    public interface ISelectionServicesListener
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void BeginSelectionUndo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void EndSelectionUndo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnSelectedElementExit([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIElementStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIElementEnd, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIElementContentStart, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIElementContentEnd);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnChangeType([In] _SELECTION_TYPE eType, [In, MarshalAs(UnmanagedType.Interface)] ISelectionServicesListener pIListener);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetTypeDetail([MarshalAs(UnmanagedType.BStr)] out string pTypeDetail);
    }
}

