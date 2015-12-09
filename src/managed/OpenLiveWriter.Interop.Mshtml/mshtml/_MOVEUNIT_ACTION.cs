// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;

    public enum _MOVEUNIT_ACTION
    {
        MOVEUNIT_ACTION_Max = 0x7fffffff,
        MOVEUNIT_NEXTBLOCK = 0x13,
        MOVEUNIT_NEXTCHAR = 1,
        MOVEUNIT_NEXTCLUSTERBEGIN = 3,
        MOVEUNIT_NEXTCLUSTEREND = 5,
        MOVEUNIT_NEXTPROOFWORD = 11,
        MOVEUNIT_NEXTSENTENCE = 0x11,
        MOVEUNIT_NEXTURLBEGIN = 12,
        MOVEUNIT_NEXTURLEND = 14,
        MOVEUNIT_NEXTWORDBEGIN = 7,
        MOVEUNIT_NEXTWORDEND = 9,
        MOVEUNIT_PREVBLOCK = 0x12,
        MOVEUNIT_PREVCHAR = 0,
        MOVEUNIT_PREVCLUSTERBEGIN = 2,
        MOVEUNIT_PREVCLUSTEREND = 4,
        MOVEUNIT_PREVPROOFWORD = 10,
        MOVEUNIT_PREVSENTENCE = 0x10,
        MOVEUNIT_PREVURLBEGIN = 13,
        MOVEUNIT_PREVURLEND = 15,
        MOVEUNIT_PREVWORDBEGIN = 6,
        MOVEUNIT_PREVWORDEND = 8
    }
}

