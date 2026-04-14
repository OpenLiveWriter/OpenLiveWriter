// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsCredentialStorage : ICredentialStorage
    {
        private const string CREDENTIAL_REGISTRY_PATH = @"SOFTWARE\OpenLiveWriter\Credentials";

        public void StoreCredential(string key, string username, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] encryptedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);
            string encryptedBase64 = Convert.ToBase64String(encryptedBytes);

            using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey($@"{CREDENTIAL_REGISTRY_PATH}\{key}"))
            {
                regKey.SetValue("Username", username);
                regKey.SetValue("Password", encryptedBase64);
            }
        }

        public (string username, string password)? RetrieveCredential(string key)
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey($@"{CREDENTIAL_REGISTRY_PATH}\{key}"))
            {
                if (regKey == null)
                    return null;

                string username = regKey.GetValue("Username") as string;
                string encryptedBase64 = regKey.GetValue("Password") as string;

                if (username == null || encryptedBase64 == null)
                    return null;

                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
                    byte[] passwordBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                    string password = Encoding.UTF8.GetString(passwordBytes);
                    return (username, password);
                }
                catch (CryptographicException)
                {
                    return null;
                }
            }
        }

        public void DeleteCredential(string key)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"{CREDENTIAL_REGISTRY_PATH}\{key}", false);
            }
            catch
            {
                // Key doesn't exist
            }
        }

        public bool CredentialExists(string key)
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey($@"{CREDENTIAL_REGISTRY_PATH}\{key}"))
            {
                return regKey != null;
            }
        }
    }
}
