// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// Secret-storage seam for account passwords, kept out of the account JSON. The
    /// macOS app wires this to the Keychain (via <c>MacCredentialStorage</c> behind a
    /// thin adapter in the app layer); tests use <see cref="InMemoryCredentialStore"/>
    /// so no real Keychain / <c>security</c> CLI is ever touched.
    ///
    /// This deliberately mirrors <c>OpenLiveWriter.Platform.ICredentialStorage</c>, but
    /// lives in the cross-platform publishing assembly so account logic has no Platform
    /// dependency.
    /// </summary>
    public interface ICredentialStore
    {
        /// <summary>Stores (or replaces) the username + password for <paramref name="key"/>.</summary>
        void Store(string key, string username, string password);

        /// <summary>Retrieves the username + password for <paramref name="key"/>, or null if absent.</summary>
        (string Username, string Password)? Retrieve(string key);

        /// <summary>Removes any stored credential for <paramref name="key"/> (no-op if absent).</summary>
        void Delete(string key);

        /// <summary>Whether a credential is stored for <paramref name="key"/>.</summary>
        bool Exists(string key);
    }

    /// <summary>
    /// In-memory <see cref="ICredentialStore"/> for tests and as a safe fallback when no
    /// platform credential storage is available. Never persists to disk or Keychain.
    /// </summary>
    public sealed class InMemoryCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, (string Username, string Password)> _store =
            new Dictionary<string, (string, string)>(StringComparer.Ordinal);

        public void Store(string key, string username, string password)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key required.", nameof(key));
            _store[key] = (username ?? string.Empty, password ?? string.Empty);
        }

        public (string Username, string Password)? Retrieve(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return _store.TryGetValue(key, out var value) ? value : ((string, string)?)null;
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            _store.Remove(key);
        }

        public bool Exists(string key) => !string.IsNullOrEmpty(key) && _store.ContainsKey(key);
    }
}
