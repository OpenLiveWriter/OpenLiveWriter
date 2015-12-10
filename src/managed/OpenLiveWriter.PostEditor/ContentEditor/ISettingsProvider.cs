// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Interop.Com;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.PostEditor
{
    [Guid("A2AA01B9-C568-49d9-AFC3-B3F4C4B6544F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface ISettingsProvider
    {
        /// <summary>
        /// Returns the value for the given setting key.
        /// </summary>
        ManagedPropVariant GetSetting(ContentEditorSetting setting);
    }

    [Guid("6E153F62-F711-4109-B9BB-5403284CD70C")]
    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential)]
    public struct ManagedPropVariant
    {
        // This struct is just a barebones PropVariant so that .NET can marshal it correctly. We use this instead of a
        // OpenLiveWriter.Interop.Com.Ribbon.PropVariant so that when we export to TLB, we don't have to add an
        // additional reference to OpenLiveWriter.Interop.tlb.
        private ushort vt;
        private ushort wReserved1;
        private ushort wReserved2;
        private ushort wReserved3;
        private IntPtr p;
        private int p2;

        public static ManagedPropVariant FromPropVariant(PropVariant propVariant)
        {
            // Do a bitwise copy from the PropVariant that was passed in.
            ManagedPropVariant managedPropVariant = new ManagedPropVariant();
            UnsafeNativeMethods.CopyFromPropVariant(out managedPropVariant, ref propVariant);
            return managedPropVariant;
        }

        public static PropVariant ToPropVariant(ManagedPropVariant managedPropVariant)
        {
            // Do a bitwise copy from the ManagedPropVariant that was passed in.
            PropVariant propVariant = new PropVariant();
            UnsafeNativeMethods.CopyToPropVariant(out propVariant, ref managedPropVariant);
            return propVariant;
        }

        public void Clear()
        {
            // Can't pass "this" by ref, so make a copy to call PropVariantClear with
            ManagedPropVariant var = this;
            UnsafeNativeMethods.PropVariantClear(ref var);

            // Since we couldn't pass "this" by ref, we need to clear the member fields manually
            // NOTE: PropVariantClear already freed heap data for us, so we are just setting
            //       our references to null.
            vt = (ushort)VarEnum.VT_EMPTY;
            wReserved1 = wReserved2 = wReserved3 = 0;
            p = IntPtr.Zero;
            p2 = 0;
        }

        private static class UnsafeNativeMethods
        {
            [DllImport("Ole32.dll", PreserveSig = false, EntryPoint = "PropVariantCopy")]
            internal extern static void CopyToPropVariant([Out] out PropVariant pDst, [In] ref ManagedPropVariant pSrc);

            [DllImport("Ole32.dll", PreserveSig = false, EntryPoint = "PropVariantCopy")]
            internal extern static void CopyFromPropVariant([Out] out ManagedPropVariant pDst, [In] ref PropVariant pSrc);

            [DllImport("Ole32.dll", PreserveSig = false)]
            internal extern static void PropVariantClear([In, Out] ref ManagedPropVariant pvar);
        }
    }

    [Guid("48A7B6C4-1C33-4d08-AED6-AFE368DEE20E")]
    [ComVisible(true)]
    public enum ContentEditorSetting
    {
        /// <summary>
        /// A registry path for MSHTML to look for default options. Should return a string (VT_BSTR, VT_LPWSTR or
        /// VT_LPSTR).
        /// </summary>
        MshtmlOptionKeyPath = 4,

        /// <summary>
        /// The default size of an inline image. Should return a string (VT_BSTR, VT_LPWSTR or VT_LPSTR). Return one of
        /// the following: "Small", "Medium", "Large" or "Full".
        /// </summary>
        ImageDefaultSize = 6,

        /// <summary>
        /// The current UI language. Should return a string (VT_BSTR, VT_LPWSTR or VT_LPSTR).
        /// </summary>
        Language = 7,
    }
}
