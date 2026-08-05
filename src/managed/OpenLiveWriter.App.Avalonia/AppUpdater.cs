// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Self-update for the macOS app, against the same GitHub Releases feed the
    /// Windows build uses.
    ///
    /// This deliberately mirrors OpenLiveWriter.PostEditor.Updates.UpdateManager
    /// rather than sharing it: that type lives in a net10.0-windows project the
    /// Avalonia app cannot reference.
    ///
    /// Prereleases are included. Velopack requires a 3-part SemVer2 package
    /// version, so alpha builds are published as MAJOR.MINOR.PATCH-alpha.BUILD;
    /// with prereleases excluded an installed alpha would never see a newer one.
    /// </summary>
    internal static class AppUpdater
    {
        /// <summary>The repository whose Releases carry the update feed.</summary>
        public const string RepositoryUrl = "https://github.com/OpenLiveWriter/OpenLiveWriter";

        /// <summary>
        /// Runs Velopack's startup hooks. Must be the first thing Main does,
        /// before any UI: on an update Velopack relaunches the app with hook
        /// arguments and expects to handle them and exit.
        /// </summary>
        public static void RunStartupHooks()
        {
            VelopackApp.Build().Run();
        }

        /// <summary>
        /// Checks for a newer release and stages it. Fire and forget: the
        /// download applies on next launch, and any failure is logged rather
        /// than surfaced, because a failed update check must never block
        /// starting the editor.
        /// </summary>
        public static void CheckInBackground()
        {
            _ = CheckAsync();
        }

        /// <summary>
        /// Returns the version that was staged, or null when the app is not a
        /// Velopack install, is already current, or the check failed.
        /// </summary>
        public static async Task<string> CheckAsync()
        {
            try
            {
                var manager = new UpdateManager(
                    new GithubSource(RepositoryUrl, accessToken: null, prerelease: true));

                // False for a plain `dotnet run`, or a bundle someone copied out
                // of the DMG by hand rather than installing. Nothing to update.
                if (!manager.IsInstalled)
                {
                    Console.WriteLine("[OLW-Update] not a Velopack install; skipping check");
                    return null;
                }

                var update = await manager.CheckForUpdatesAsync().ConfigureAwait(false);
                if (update == null)
                {
                    Console.WriteLine("[OLW-Update] no update available");
                    return null;
                }

                var version = update.TargetFullRelease.Version.ToString();
                Console.WriteLine("[OLW-Update] downloading " + version);
                await manager.DownloadUpdatesAsync(update).ConfigureAwait(false);
                Console.WriteLine("[OLW-Update] staged " + version + "; applies on next launch");
                return version;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[OLW-Update] check failed: " + ex.Message);
                return null;
            }
        }
    }
}
