// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.SpellChecker
{
    public class SpellingTimer : System.Windows.Forms.Timer
    {
            public SpellingTimer(int interval)
            {
                Interval = interval;
            }
    }
}
