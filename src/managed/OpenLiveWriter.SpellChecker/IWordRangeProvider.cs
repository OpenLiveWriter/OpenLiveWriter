// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.SpellChecker
{
    [Guid("F4F06001-99F6-448F-9199-E863D771066B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IWordRangeProvider
    {
        IWordRange GetSubjectSpellcheckWordRange();

        void CloseSubjectSpellcheckWordRange();
    }
}
