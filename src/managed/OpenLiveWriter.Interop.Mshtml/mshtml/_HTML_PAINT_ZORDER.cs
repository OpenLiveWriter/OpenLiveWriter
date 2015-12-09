// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;

    public enum _HTML_PAINT_ZORDER
    {
        HTML_PAINT_ZORDER_Max = 0x7fffffff,
        HTMLPAINT_ZORDER_ABOVE_CONTENT = 7,
        HTMLPAINT_ZORDER_ABOVE_FLOW = 6,
        HTMLPAINT_ZORDER_BELOW_CONTENT = 4,
        HTMLPAINT_ZORDER_BELOW_FLOW = 5,
        HTMLPAINT_ZORDER_NONE = 0,
        HTMLPAINT_ZORDER_REPLACE_ALL = 1,
        HTMLPAINT_ZORDER_REPLACE_BACKGROUND = 3,
        HTMLPAINT_ZORDER_REPLACE_CONTENT = 2,
        HTMLPAINT_ZORDER_WINDOW_TOP = 8
    }
}

