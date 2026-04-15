// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;

namespace OpenLiveWriter.Platform.Mac
{
    public class MacPlatformServices : IPlatformServices
    {
        private const string APP_NAME = "OpenLiveWriter";
        private string _appDataDir;
        private string _localAppDataDir;

        public string GetApplicationDataDirectory()
        {
            if (_appDataDir == null)
            {
                _appDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    APP_NAME);
                Directory.CreateDirectory(_appDataDir);
            }
            return _appDataDir;
        }

        public string GetLocalApplicationDataDirectory()
        {
            if (_localAppDataDir == null)
            {
                _localAppDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    APP_NAME);
                Directory.CreateDirectory(_localAppDataDir);
            }
            return _localAppDataDir;
        }

        public string GetShortPathName(string path) => path; // No 8.3 paths on macOS

        public void ExtractCabinet(string cabinetPath, string targetDirectory)
            => throw new PlatformNotSupportedException("Cabinet extraction is not supported on macOS.");

        public bool IsApplicationInstalled() => true; // If we're running, we're installed

        public ISettingsPersister CreateUserSettingsPersister(string subKey)
        {
            // Use XML-based settings on macOS
            string settingsDir = Path.Combine(GetApplicationDataDirectory(), "Settings");
            Directory.CreateDirectory(settingsDir);
            // For now, return a simple memory persister. XmlSettingsPersister will be wired up
            // when CoreServices is merged into the cross-platform build.
            throw new NotImplementedException("XML settings persister will be wired up when CoreServices merge is complete.");
        }
    }
}
