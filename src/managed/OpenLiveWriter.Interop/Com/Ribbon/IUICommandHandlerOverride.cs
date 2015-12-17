// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    /// <summary>
    /// Allows Mail to override shared canvas ribbon properties
    /// </summary>
    [Guid("6CB2B077-C85A-46dc-AF61-D5842F799F0E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IUICommandHandlerOverride
    {
        // We'll add support as necessary for shared canvas ribbon properties.
        // Currently supported properties:
        // + ContextAvailability

        // Overrides shared canvas logic with respect to the given property key.
        // IUICommandHandler::UpdateProperty will reflect any active overrides
        [PreserveSig]
        Int32 OverrideProperty(UInt32 commandId,
                               [In] ref PropertyKey key,
                               [In, Optional] PropVariantRef overrideValue);

        // Cancels override for given property key.
        // Returns S_OK if override was active.
        // Returns S_FALSE if override was not active.
        // Returns E_INVALIDARG for an unsupported commandId and/or property key.
        [PreserveSig]
        Int32 CancelOverride(UInt32 commandId,
                             [In] ref PropertyKey key);
    }

}
