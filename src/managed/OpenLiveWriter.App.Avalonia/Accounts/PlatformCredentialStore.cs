// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Platform;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia.Accounts
{
    /// <summary>
    /// Adapts the cross-platform <see cref="ICredentialStore"/> seam (used by the account
    /// layer in <c>OpenLiveWriter.Publishing</c>) onto the platform's secure credential
    /// storage (<see cref="ICredentialStorage"/> — the macOS Keychain via
    /// <c>MacCredentialStorage</c>). This keeps the publishing assembly free of any
    /// Platform dependency while the real password lands in the Keychain.
    ///
    /// Tests never use this adapter; they inject <see cref="InMemoryCredentialStore"/>, so
    /// the real <c>security</c> CLI is never invoked under <c>dotnet test</c>.
    /// </summary>
    public sealed class PlatformCredentialStore : ICredentialStore
    {
        private readonly ICredentialStorage _storage;

        public PlatformCredentialStore(ICredentialStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>Creates an adapter over the initialized platform credential storage.</summary>
        public static PlatformCredentialStore CreateDefault()
        {
            PlatformContext.EnsureInitialized();
            return new PlatformCredentialStore(PlatformContext.Credentials);
        }

        public void Store(string key, string username, string password) =>
            _storage.StoreCredential(key, username, password);

        public (string Username, string Password)? Retrieve(string key)
        {
            var cred = _storage.RetrieveCredential(key);
            if (cred == null) return null;
            return (cred.Value.username, cred.Value.password);
        }

        public void Delete(string key) => _storage.DeleteCredential(key);

        public bool Exists(string key) => _storage.CredentialExists(key);
    }
}
