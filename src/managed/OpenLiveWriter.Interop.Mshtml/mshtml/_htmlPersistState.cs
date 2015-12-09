// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;

    public enum _htmlPersistState
    {
        htmlPersistState_Max = 0x7fffffff,
        htmlPersistStateFavorite = 1,
        htmlPersistStateHistory = 2,
        htmlPersistStateNormal = 0,
        htmlPersistStateSnapshot = 3,
        htmlPersistStateUserData = 4
    }
}

