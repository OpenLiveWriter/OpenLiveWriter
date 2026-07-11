// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.Publishing.Accounts
{
    /// <summary>
    /// Persistence seam for <see cref="BlogAccount"/> metadata. Hides the on-disk
    /// format (see <see cref="FileAccountStore"/>) so callers and tests can swap in an
    /// in-memory implementation. Passwords are NOT handled here — they live in the
    /// separate <see cref="ICredentialStore"/> (Keychain on macOS).
    /// </summary>
    public interface IAccountStore
    {
        /// <summary>Creates or overwrites an account. Assigns <see cref="BlogAccount.Id"/> if new.</summary>
        BlogAccount Save(BlogAccount account);

        /// <summary>Loads an account by id; returns null if missing, throws <see cref="AccountStoreException"/> if corrupt.</summary>
        BlogAccount Load(string id);

        /// <summary>Lists all valid accounts (corrupt entries are skipped).</summary>
        IReadOnlyList<BlogAccount> List();

        /// <summary>Deletes an account by id (no-op if absent).</summary>
        void Delete(string id);

        /// <summary>Whether an account with the given id exists.</summary>
        bool Exists(string id);

        /// <summary>
        /// Id of the last-selected ("current") account, or null/empty if none. Persisted
        /// so the selection survives restarts. Setting an id that does not exist is allowed
        /// but resolves to no current account on read via <see cref="IAccountStore"/> callers.
        /// </summary>
        string CurrentAccountId { get; set; }
    }

    /// <summary>Thrown when an account file cannot be read (corrupt/unreadable).</summary>
    public class AccountStoreException : Exception
    {
        public AccountStoreException(string message, Exception inner) : base(message, inner) { }
    }
}
