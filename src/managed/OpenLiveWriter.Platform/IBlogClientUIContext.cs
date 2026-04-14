// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-agnostic UI context for blog client operations.
    /// </summary>
    public interface IBlogClientUIContext
    {
        object Invoke(Delegate method, object[] args);
        IAsyncResult BeginInvoke(Delegate method, object[] args);
        object EndInvoke(IAsyncResult result);
        bool InvokeRequired { get; }
        IntPtr NativeWindowHandle { get; }
    }
}
