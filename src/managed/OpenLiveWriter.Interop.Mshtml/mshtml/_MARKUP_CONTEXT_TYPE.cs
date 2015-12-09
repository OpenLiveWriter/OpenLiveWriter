// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;

    public enum _MARKUP_CONTEXT_TYPE
    {
        CONTEXT_TYPE_EnterScope = 2,
        CONTEXT_TYPE_ExitScope = 3,
        CONTEXT_TYPE_None = 0,
        CONTEXT_TYPE_NoScope = 4,
        CONTEXT_TYPE_Text = 1,
        MARKUP_CONTEXT_TYPE_Max = 0x7fffffff
    }
}

