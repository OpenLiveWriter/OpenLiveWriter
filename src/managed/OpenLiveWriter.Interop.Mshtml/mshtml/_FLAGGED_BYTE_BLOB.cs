// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _FLAGGED_BYTE_BLOB
    {
        public uint fFlags;
        public uint clSize;
        [ComConversionLoss]
        public IntPtr abData;
    }
}

