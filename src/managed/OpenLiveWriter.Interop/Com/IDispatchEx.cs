// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("A6EF9860-C720-11D0-9337-00A0C90DCAA9")]
    public interface IDispatchEx
    {
        void GetTypeInfoCount();
        void GetTypeInfo();
        void GetIDsOfNames();
        void Invoke();

        void GetDispID([MarshalAs(UnmanagedType.BStr)] string bstrName, uint grfdex, out IntPtr dispId);

        void InvokeEx(
            IntPtr id,
            uint lcid,
            Int16 wFlags,
            ref System.Runtime.InteropServices.ComTypes.DISPPARAMS pdp,
            IntPtr pVarRes,
            ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pei,
            IntPtr pspCaller
            );

        void DeleteMemberByName();

        void DeleteMemberByDispID();

        void GetMemberProperties();

        void GetMemberName();

        void GetNextDispID();

        void GetNameSpaceParent();
    }
}
