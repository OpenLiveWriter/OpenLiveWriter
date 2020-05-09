// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.Win32;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.ResourceDownloading;
using Squirrel;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace OpenLiveWriter.PostEditor.Updates
{
    public class UpdateManager
    {
        public static DateTime Expires = DateTime.MaxValue;
        
        public static void CheckforUpdates(bool forceCheck = false)
        {
<<<<<<< HEAD
#if !DesktopUWP
            // Update using Squirrel if not a Desktop UWP package
            var checkNow = forceCheck || UpdateSettings.AutoUpdate;
            var downloadUrl = UpdateSettings.CheckForBetaUpdates ?
                UpdateSettings.BetaUpdateDownloadUrl : UpdateSettings.UpdateDownloadUrl;

            // Schedule Open Live Writer 10 seconds after the launch
            var delayUpdate = new DelayUpdateHelper(UpdateOpenLiveWriter(downloadUrl, checkNow), UPDATELAUNCHDELAY);
            delayUpdate.StartBackgroundUpdate("Background OpenLiveWriter application update");
#endif
=======
            var result = System.Windows.Forms.DialogResult.No;

            if (UpdateSettings.AutoUpdate && HasNet462OrLater())
            {
                result = MessageBox.Show("An update for OpenLiveWriter may be available. Would you like to check for it?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Select Cancel to permanently cancel update checks."
                , "Update", MessageBoxButtons.YesNoCancel);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    var checkNow = forceCheck || UpdateSettings.AutoUpdate;
                    var downloadUrl = UpdateSettings.CheckForBetaUpdates ?
                        UpdateSettings.BetaUpdateDownloadUrl : UpdateSettings.UpdateDownloadUrl;

                    // Schedule Open Live Writer 10 seconds after the launch
                    var delayUpdate = new DelayUpdateHelper(UpdateOpenLiveWriter(downloadUrl, checkNow), UPDATELAUNCHDELAY);
                    delayUpdate.StartBackgroundUpdate("Background OpenLiveWriter application update");
                } else if (result == System.Windows.Forms.DialogResult.Cancel) {
                    MessageBox.Show(@"Attempting to disable AutoUpdate."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "AutoUpdate can be turned on and off by setting this registry key: " +
                        @"HKEY_CURRENT_USER\Software\OpenLiveWriter\Updates\AutoUpdate to 0", "Disabling AutoUpdate", MessageBoxButtons.OK);
                    UpdateSettings.AutoUpdate = false;

                    //Manually writing out values since they're not always persisted
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\OpenLiveWriter\Updates", "AutoUpdate", 0);
                }
                return;
            }
>>>>>>> origin/win7-sunset
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

        private static bool HasNet462OrLater()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null && (int)ndpKey.GetValue("Release") >= 394254) //.NET 4.6.1 or later is installed
                {
                    return true;
                }
                else
                {
                    Trace.WriteLine("Old operating system or .NET version, skipping update.");
                    Trace.WriteLine("Windows Version: " + Environment.OSVersion.VersionString);
                    Trace.WriteLine(".NET NPD version number: " + ndpKey);
                }
            }
            return false;
        }

        private const int UPDATELAUNCHDELAY = 10000;
    }
}
