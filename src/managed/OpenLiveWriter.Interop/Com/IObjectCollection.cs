// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport()]
    [GuidAttribute("5632B1A4-E38A-400A-928A-D4CD63230295")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectCollection
    {
        // IObjectArray
        [PreserveSig]
        void GetCount(out uint cObjects);
        [PreserveSig]
        void GetAt(
            uint iIndex,
            ref Guid riid,
            [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject);

        // IObjectCollection
        void AddObject(
            [MarshalAs(UnmanagedType.Interface)] object pvObject);
        void AddFromArray(
            [MarshalAs(UnmanagedType.Interface)] IObjectArray poaSource);
        void RemoveObject(uint uiIndex);
        void Clear();
    }

    [GuidAttribute("2D3468C1-36A7-43B6-AC24-D3F02FD9607A")]
    [ClassInterfaceAttribute(ClassInterfaceType.None)]
    [ComImportAttribute()]
    public class CEnumerableObjectCollection { }
}
