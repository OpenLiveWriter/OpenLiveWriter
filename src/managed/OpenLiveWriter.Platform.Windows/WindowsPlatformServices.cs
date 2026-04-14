// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Win32;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsPlatformServices : IPlatformServices
    {
        private const string APP_NAME = "OpenLiveWriter";

        public string GetApplicationDataDirectory()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                APP_NAME);
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetLocalApplicationDataDirectory()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                APP_NAME);
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetShortPathName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            StringBuilder shortPath = new StringBuilder(260);
            uint result = GetShortPathNameNative(path, shortPath, (uint)shortPath.Capacity);
            if (result == 0 || result > shortPath.Capacity)
                return path;

            return shortPath.ToString();
        }

        [DllImport("kernel32.dll", EntryPoint = "GetShortPathNameW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetShortPathNameNative(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        public void ExtractCabinet(string cabinetPath, string targetDirectory)
        {
            throw new NotImplementedException("Cabinet extraction will be implemented when CabinetFileExtractor is moved to this project.");
        }

        public bool IsApplicationInstalled()
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("OPEN_LIVE_WRITER"))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public ISettingsPersister CreateUserSettingsPersister(string subKey)
        {
            string fullKey = $@"SOFTWARE\{APP_NAME}\{subKey}";
            return new RegistrySettingsPersister(Registry.CurrentUser, fullKey);
        }
    }
}
