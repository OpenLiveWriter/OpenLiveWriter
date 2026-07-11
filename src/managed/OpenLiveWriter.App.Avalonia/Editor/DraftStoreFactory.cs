// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using OpenLiveWriter.Platform;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Resolves the default local draft store location via the platform services
    /// (never hardcoding <c>~/Library/...</c>): a "Drafts" folder under the
    /// app-data directory returned by <see cref="IPlatformServices.GetApplicationDataDirectory"/>.
    /// </summary>
    public static class DraftStoreFactory
    {
        public const string DraftsFolderName = "Drafts";

        /// <summary>The platform-resolved directory drafts are stored in.</summary>
        public static string GetDraftsDirectory()
        {
            PlatformContext.EnsureInitialized();
            return Path.Combine(PlatformContext.Services.GetApplicationDataDirectory(), DraftsFolderName);
        }

        /// <summary>Creates the default file-backed draft store for this platform.</summary>
        public static IDraftStore CreateDefault() => new FileDraftStore(GetDraftsDirectory());
    }
}
