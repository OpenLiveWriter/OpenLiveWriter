// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using OpenLiveWriter.Platform;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.App.Avalonia.Accounts
{
    /// <summary>
    /// Builds the default <see cref="BlogAccountService"/> for the running app: a
    /// file-backed account store under the platform-resolved app-data <c>Accounts</c>
    /// folder, with passwords in the platform credential store (Keychain on macOS).
    /// The folder is resolved through <c>OpenLiveWriter.Platform</c>, never hardcoded —
    /// same pattern as <see cref="Editor.DraftStoreFactory"/>.
    /// </summary>
    public static class AccountServiceFactory
    {
        public const string AccountsFolderName = "Accounts";

        /// <summary>Resolves the app-data <c>Accounts</c> directory (created lazily on save).</summary>
        public static string GetAccountsDirectory()
        {
            PlatformContext.EnsureInitialized();
            return Path.Combine(PlatformContext.Services.GetApplicationDataDirectory(), AccountsFolderName);
        }

        /// <summary>Creates the account service backed by files + the platform Keychain.</summary>
        public static BlogAccountService CreateDefault()
        {
            var store = new FileAccountStore(GetAccountsDirectory());
            var credentials = PlatformCredentialStore.CreateDefault();
            return new BlogAccountService(store, credentials);
        }
    }
}
