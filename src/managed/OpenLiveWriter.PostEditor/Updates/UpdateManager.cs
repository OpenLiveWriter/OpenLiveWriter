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
                string url = UpdateSettings.CheckForBetaUpdates
                    ? UpdateSettings.BetaUpdateDownloadUrl
                    : UpdateSettings.UpdateDownloadUrl;
                var mgr = new Velopack.UpdateManager(new SimpleWebSource(url));
                if (!mgr.IsInstalled) return;
                var updateInfo = await mgr.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    await mgr.DownloadUpdatesAsync(updateInfo);
                    // Will apply on next restart
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Velopack update check failed: " + ex.Message);
            }
        }

        private const int UPDATELAUNCHDELAY = 10000;
    }
}
