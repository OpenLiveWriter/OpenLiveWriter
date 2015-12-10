// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Enumeration of values that can be passed as flags to Navigate
    /// </summary>
    [Flags]
    public enum BrowserNavConstants
    {
        navOpenInNewWindow = 0x1,
        navNoHistory = 0x2,
        navNoReadFromCache = 0x4,
        navNoWriteToCache = 0x8,
        navAllowAutosearch = 0x10,
        navBrowserBar = 0x20,
        navHyperlink = 0x40
    }
}
