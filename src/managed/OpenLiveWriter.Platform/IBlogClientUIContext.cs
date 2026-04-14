// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-agnostic UI context for blog client operations.
    /// Provides the native window handle needed for dialog ownership
    /// on platforms that support it.
    /// </summary>
    public interface IBlogClientUIContext
    {
        /// <summary>
        /// The native window handle for dialog ownership.
        /// Returns IntPtr.Zero on platforms that don't support native window handles.
        /// </summary>
        IntPtr NativeWindowHandle { get; }
    }
}
