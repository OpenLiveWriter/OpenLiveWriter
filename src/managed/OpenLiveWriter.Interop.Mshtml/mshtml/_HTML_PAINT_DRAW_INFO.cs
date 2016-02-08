// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4), ComConversionLoss]
    public struct _HTML_PAINT_DRAW_INFO
    {
        public tagRECT rcViewport;
        [ComConversionLoss]
        public IntPtr hrgnUpdate;
        public _HTML_PAINT_XFORM xform;
    }
}

