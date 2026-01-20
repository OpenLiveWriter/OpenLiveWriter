// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using Velopack;
using Velopack.Sources;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLiveWriter.PostEditor.Updates
{
    public class UpdateManager
    {
        public static DateTime Expires = DateTime.MaxValue;
        
        public static void CheckforUpdates(bool forceCheck = false)
        {
#if !DesktopUWP
            // Update using Velopack if not a Desktop UWP package
            var checkNow = forceCheck || UpdateSettings.AutoUpdate;
            var downloadUrl = UpdateSettings.CheckForBetaUpdates ?
                UpdateSettings.BetaUpdateDownloadUrl : UpdateSettings.UpdateDownloadUrl;

            // Schedule Open Live Writer 10 seconds after the launch
            var delayUpdate = new DelayUpdateHelper(UpdateOpenLiveWriter(downloadUrl, checkNow), UPDATELAUNCHDELAY);
            delayUpdate.StartBackgroundUpdate("Background OpenLiveWriter application update");
#endif
        }

        private static ThreadStart UpdateOpenLiveWriter(string downloadUrl, bool checkNow)
        {
            return async () =>
            {
                if (checkNow)
                {
                    try
                    {
                        var source = new SimpleWebSource(downloadUrl);
                        var manager = new Velopack.UpdateManager(source);
                        
                        if (!manager.IsInstalled)
                        {
                            Trace.WriteLine("Application is not installed via Velopack, skipping update check.");
                            return;
                        }

                        var updateInfo = await manager.CheckForUpdatesAsync();
                        
                        if (updateInfo == null)
                        {
                            Trace.WriteLine("No updates available.");
                            return;
                        }

                        Trace.WriteLine($"Update available: {updateInfo.TargetFullRelease.Version}");
                        
                        // Download and apply the update
                        await manager.DownloadUpdatesAsync(updateInfo);
                        
                        // The update will be applied on next restart
                        Trace.WriteLine("Update downloaded and will be applied on next restart.");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Unexpected error while updating Open Live Writer. " + ex);
                    }
                }
            };
        }

        private const int UPDATELAUNCHDELAY = 10000;
    }
}
