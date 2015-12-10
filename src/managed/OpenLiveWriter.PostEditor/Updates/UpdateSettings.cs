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
            // Force these settings temporarily in case other devs already got defaults set.
            AutoUpdate = true;
            CheckForBetaUpdates = false;
            UpdateDownloadUrl = UPDATEDOWNLOADURL;
            BetaUpdateDownloadUrl = BETAUPDATEDOWNLOADURL;
        }

        public static bool AutoUpdate
        {
            get { return settings.GetBoolean(AUTOUPDATE, true); }
            set { settings.SetBoolean(AUTOUPDATE, value); }
        }

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

        private const string AUTOUPDATE = "AutoUpdate";
        private const string CHECKFORBETAUPDATES = "CheckForBetaUpdates";

        private const string CHECKUPDATESURL = "CheckUpdatesUrl";
        private const string UPDATEDOWNLOADURL = "https://openlivewriter.azureedge.net/stable/Releases";
        private const string CHECKBETAUPDATESURL = "CheckBetaUpdatesUrl";
        private const string BETAUPDATEDOWNLOADURL = "https://openlivewriter.azureedge.net/nightly/Releases";

        private static readonly SettingsPersisterHelper settings = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Updates");
    }
}
