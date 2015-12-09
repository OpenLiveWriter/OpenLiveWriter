// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Mshtml.Mshtml_Interop
{
    /// <summary>
    /// Interface used for protecting focus change
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("d81f90a3-8156-44f7-ad28-5abb87003274")]
    public interface IProtectFocus
    {
        bool AllowFocusChange();
    }
}
