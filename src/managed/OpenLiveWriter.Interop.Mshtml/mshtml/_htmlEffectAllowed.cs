// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;

    public enum _htmlEffectAllowed
    {
        htmlEffectAllowed_Max = 0x7fffffff,
        htmlEffectAllowedAll = 6,
        htmlEffectAllowedCopy = 0,
        htmlEffectAllowedCopyLink = 3,
        htmlEffectAllowedCopyMove = 4,
        htmlEffectAllowedLink = 1,
        htmlEffectAllowedLinkMove = 5,
        htmlEffectAllowedMove = 2,
        htmlEffectAllowedNone = 7,
        htmlEffectAllowedUninitialized = 8
    }
}

