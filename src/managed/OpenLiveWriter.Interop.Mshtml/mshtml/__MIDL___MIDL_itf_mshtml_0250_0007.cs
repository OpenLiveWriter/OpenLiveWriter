// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct __MIDL___MIDL_itf_mshtml_0250_0007
    {
        public uint dwSize;
        public uint dwStyle;
        public uint dwCount;
        public uint dwSelection;
        public uint dwPageStart;
        public uint dwPageSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=1)]
        public uint[] dwOffset;
    }
}

