// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.InteropServices;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Interface used for customizing the UI of MSHTML
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3050f3f0-98b5-11cf-bb82-00aa00bdce0b")]
    public interface ICustomDoc
    {
        void SetUIHandler(
            [In] IDocHostUIHandler pUIHandler);
    }
}

