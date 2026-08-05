// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace OpenLiveWriter.PostEditor.Updates
{
    /// <summary>
    /// Application update manager using Velopack.
    /// </summary>
    public class UpdateManager
    {
        public static DateTime Expires = DateTime.MaxValue;

        public static void CheckforUpdates(bool forceCheck = false)
        {
            if (!UpdateSettings.AutoUpdate && !forceCheck)
                return;
            _ = CheckForUpdatesAsync();
        }

        private static async Task CheckForUpdatesAsync()
        {
            await CheckAsync();
        }

        /// <summary>The outcome of a check, so a caller can tell the user.</summary>
        public enum UpdateCheckResult
        {
            /// <summary>Not a Velopack install (a dev run, or an unpacked copy).</summary>
            NotInstalled,
            /// <summary>Already on the newest release.</summary>
            UpToDate,
            /// <summary>A newer release was downloaded; it applies on next launch.</summary>
            Staged,
            /// <summary>The check could not complete.</summary>
            Failed,
        }

        /// <summary>
        /// Checks for a newer release and stages it, reporting what happened so
        /// a menu command can say so. The fire-and-forget startup check ignores
        /// the result; "Check for Updates" shows it.
        /// </summary>
        public static async Task<(UpdateCheckResult Result, string Version, string Error)> CheckAsync()
        {
            try
            {
                var source = CreateUpdateSource();
                if (source == null)
                    return (UpdateCheckResult.Failed, null, "No update source is configured.");

                var mgr = new Velopack.UpdateManager(source);
                if (!mgr.IsInstalled)
                    return (UpdateCheckResult.NotInstalled, null, null);

                var updateInfo = await mgr.CheckForUpdatesAsync();
                if (updateInfo == null)
                    return (UpdateCheckResult.UpToDate, null, null);

                string version = updateInfo.TargetFullRelease.Version.ToString();
                Trace.WriteLine("Velopack update available: " + version);
                await mgr.DownloadUpdatesAsync(updateInfo);
                return (UpdateCheckResult.Staged, version, null);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Velopack update check failed: " + ex.Message);
                return (UpdateCheckResult.Failed, null, ex.Message);
            }
        }

        /// <summary>
        /// Builds the update feed source: GitHub Releases (default) or the
        /// Open Live Writer website, per UpdateSettings.UpdateFeedType. The beta
        /// channel maps to GitHub prereleases / the nightly website feed.
        /// </summary>
        private static IUpdateSource CreateUpdateSource()
        {
            bool website = string.Equals(UpdateSettings.UpdateFeedType, "website",
                StringComparison.OrdinalIgnoreCase);
            if (website)
            {
                string url = UpdateSettings.CheckForBetaUpdates
                    ? UpdateSettings.BetaUpdateDownloadUrl
                    : UpdateSettings.UpdateDownloadUrl;
                if (string.IsNullOrEmpty(url)) return null;
                return new SimpleWebSource(url);
            }

            string repo = UpdateSettings.GitHubRepoUrl;
            if (string.IsNullOrEmpty(repo)) return null;
            return new GithubSource(repo, null, UpdateSettings.CheckForBetaUpdates);
        }

        private const int UPDATELAUNCHDELAY = 10000;
    }
}
