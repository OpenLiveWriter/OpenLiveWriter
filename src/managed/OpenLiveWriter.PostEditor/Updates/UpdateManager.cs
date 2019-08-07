// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using Squirrel;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace OpenLiveWriter.PostEditor.Updates
{
    public class UpdateManager
    {
        public static DateTime Expires = DateTime.MaxValue;
        
        public static void CheckforUpdates(bool forceCheck = false)
        {
#if !DesktopUWP
            // Update using Squirrel if not a Desktop UWP package
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
                        using (var manager = new Squirrel.UpdateManager(downloadUrl))
                        {
                            var update = await manager.CheckForUpdate();

                            if(update != null && 
                               update.ReleasesToApply.Count > 0 && 
                               update.FutureReleaseEntry.Version < update.CurrentlyInstalledVersion.Version)
                            {
                                Trace.WriteLine("Update is older than currently running version, not installing.");
                                Trace.WriteLine($"Current: {update.CurrentlyInstalledVersion.Version} Update: {update.FutureReleaseEntry.Version}");
                                return;
                            }

                            await manager.UpdateApp();
                        }
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
