// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// The canonical ISettingsPersister interface has moved to OpenLiveWriter.Platform.
// This file provides a backward-compatible derived interface so existing code
// that references OpenLiveWriter.CoreServices.Settings.ISettingsPersister continues
// to compile without changes. New code should use OpenLiveWriter.Platform.ISettingsPersister.

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// Backward-compatible alias for OpenLiveWriter.Platform.ISettingsPersister.
    /// New code should use OpenLiveWriter.Platform.ISettingsPersister directly.
    /// </summary>
    public interface ISettingsPersister : OpenLiveWriter.Platform.ISettingsPersister
    {
    }
}
