// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Platform-specific secure credential storage.
    /// Windows: DPAPI. macOS: Keychain. Linux: libsecret.
    /// </summary>
    public interface ICredentialStorage
    {
        void StoreCredential(string key, string username, string password);
        (string username, string password)? RetrieveCredential(string key);
        void DeleteCredential(string key);
        bool CredentialExists(string key);
    }
}
