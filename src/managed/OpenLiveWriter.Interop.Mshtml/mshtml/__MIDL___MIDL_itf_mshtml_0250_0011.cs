// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct __MIDL___MIDL_itf_mshtml_0250_0011
    {
        public uint cbSize;
        public uint fType;
        public uint fState;
        public uint wID;
        [ComConversionLoss, ComAliasName("mshtml.wireHBITMAP")]
        public IntPtr hbmpChecked;
        [ComConversionLoss, ComAliasName("mshtml.wireHBITMAP")]
        public IntPtr hbmpUnchecked;
        public uint dwItemData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=80)]
        public ushort[] szString;
        [ComConversionLoss, ComAliasName("mshtml.wireHBITMAP")]
        public IntPtr hbmpItem;
    }
}

