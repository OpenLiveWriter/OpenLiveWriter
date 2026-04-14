// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-specific services that vary by operating system.
    /// </summary>
    public interface IPlatformServices
    {
        string GetApplicationDataDirectory();
        string GetLocalApplicationDataDirectory();
        string GetShortPathName(string path);
        void ExtractCabinet(string cabinetPath, string targetDirectory);
        bool IsApplicationInstalled();
        ISettingsPersister CreateUserSettingsPersister(string subKey);
    }
}
