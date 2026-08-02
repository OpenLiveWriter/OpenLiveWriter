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
            try
            {
                var source = CreateUpdateSource();
                if (source == null) return;
                var mgr = new Velopack.UpdateManager(source);
                if (!mgr.IsInstalled) return;
                var updateInfo = await mgr.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    Trace.WriteLine("Velopack update available: " + updateInfo.TargetFullRelease.Version);
                    await mgr.DownloadUpdatesAsync(updateInfo);
                    // Will apply on next restart
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Velopack update check failed: " + ex.Message);
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
