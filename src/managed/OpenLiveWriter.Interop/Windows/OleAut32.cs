// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    internal static class OleAut32
    {
        [DllImport("OleAut32.dll", PreserveSig = true)] // psa is actually returned, not hresult
        internal extern static IntPtr SafeArrayCreateVector(ushort vt, int lowerBound, uint cElems);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        internal extern static IntPtr SafeArrayAccessData(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        internal extern static void SafeArrayUnaccessData(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = true)] // retuns uint32
        internal extern static uint SafeArrayGetDim(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        internal extern static int SafeArrayGetLBound(IntPtr psa, uint nDim);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        internal extern static int SafeArrayGetUBound(IntPtr psa, uint nDim);

        // This decl for SafeArrayGetElement is only valid for cDims==1!
        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        [return: MarshalAs(UnmanagedType.IUnknown)]
        internal extern static object SafeArrayGetElement(IntPtr psa, ref int rgIndices);
    }
}
