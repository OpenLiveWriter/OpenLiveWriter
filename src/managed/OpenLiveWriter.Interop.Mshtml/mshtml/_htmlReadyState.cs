// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;

    public enum _htmlReadyState
    {
        htmlReadyState_Max = 0x7fffffff,
        htmlReadyStatecomplete = 4,
        htmlReadyStateinteractive = 3,
        htmlReadyStateloaded = 2,
        htmlReadyStateloading = 1,
        htmlReadyStateuninitialized = 0
    }
}

