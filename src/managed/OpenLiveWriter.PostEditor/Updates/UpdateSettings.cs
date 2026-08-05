// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.PostEditor.Updates
{
    public class UpdateSettings
    {
        static UpdateSettings()
        {
            // Force these settings temporarily in case people already got defaults set.
            BetaUpdateDownloadUrl = BETAUPDATEDOWNLOADURL;
        }

        public static bool AutoUpdate
        {
            get { return settings.GetBoolean(AUTOUPDATE, true); }
            set { settings.SetBoolean(AUTOUPDATE, value); }
        }

        /// <summary>
        /// Whether the update check considers prereleases. Defaults to true
        /// while the project ships alphas: Velopack requires a 3-part SemVer2
        /// package version, so per-build alphas are versioned
        /// MAJOR.MINOR.PATCH-alpha.BUILD. GithubSource only returns prerelease
        /// releases when this is set, so with it off an installed alpha would
        /// never see a newer alpha. Revisit once a stable channel exists.
        /// </summary>
        public static bool CheckForBetaUpdates
        {
            get { return settings.GetBoolean(CHECKFORBETAUPDATES, true); }
            set { settings.SetBoolean(CHECKFORBETAUPDATES, value); }
        }

        public static string UpdateDownloadUrl
        {
            get { return settings.GetString(CHECKUPDATESURL, UPDATEDOWNLOADURL); }
            set { settings.SetString(CHECKUPDATESURL, value); }
        }

        public static string BetaUpdateDownloadUrl
        {
            get { return settings.GetString(CHECKBETAUPDATESURL, BETAUPDATEDOWNLOADURL); }
            set { settings.SetString(CHECKBETAUPDATESURL, value); }
        }

        /// <summary>
        /// Which feed auto-update checks: "github" (GitHub Releases via
        /// Velopack's GithubSource) or "website" (static Velopack feed on the
        /// Open Live Writer website via SimpleWebSource).
        /// </summary>
        public static string UpdateFeedType
        {
            get { return settings.GetString(UPDATEFEEDTYPE, "github"); }
            set { settings.SetString(UPDATEFEEDTYPE, value); }
        }

        /// <summary>
        /// GitHub repository (owner/name or full URL) whose Releases feed the
        /// auto-update check when UpdateFeedType is "github".
        /// </summary>
        public static string GitHubRepoUrl
        {
            get { return settings.GetString(GITHUBREPOURL, DEFAULTGITHUBREPOURL); }
            set { settings.SetString(GITHUBREPOURL, value); }
        }

        private const string AUTOUPDATE = "AutoUpdate";
        private const string CHECKFORBETAUPDATES = "CheckForBetaUpdates";

        private const string CHECKUPDATESURL = "CheckUpdatesUrl";
        private const string UPDATEDOWNLOADURL = "https://openlivewriter.com/releases/stable"; // Website feed for stable builds
        private const string CHECKBETAUPDATESURL = "CheckBetaUpdatesUrl";
        private const string BETAUPDATEDOWNLOADURL = "https://openlivewriter.com/releases/nightly"; // Website feed for CI builds

        private const string UPDATEFEEDTYPE = "UpdateFeedType";
        private const string GITHUBREPOURL = "GitHubRepoUrl";
        private const string DEFAULTGITHUBREPOURL = "https://github.com/OpenLiveWriter/OpenLiveWriter";

        private static readonly SettingsPersisterHelper settings = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Updates");
    }
}
