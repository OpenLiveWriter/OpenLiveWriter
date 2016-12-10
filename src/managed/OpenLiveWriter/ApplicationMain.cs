// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Principal;
    using System.Windows.Forms;

    using JetBrains.Annotations;

    using Microsoft.Win32;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.Settings;
    using OpenLiveWriter.Interop.Windows;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.PostEditor;
    using OpenLiveWriter.PostEditor.JumpList;

    /// <summary>
    /// Class ApplicationMain.
    /// </summary>
    public class ApplicationMain
    {
        private static CultureInfo currentUICulture;

        /// <summary>
        /// Sample args:
        ///	    /culture:fofo
        /// </summary>
        /// <param name="args">The arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            // WinLive 281407: Remove the current working directory from the dll search path
            // This prevents a rogue dll (wlidcli.dll) from being loaded while doing
            // something like opening a .wpost from a network location.
            Kernel32.SetDllDirectory(string.Empty);

            // Enable custom visual styles for Writer.
            Application.EnableVisualStyles();

            // OLW doesn't work well at all from the guest account, so just block that
            // There is no protection from asserts here, so DON'T ASSERT
            var currentUser = WindowsIdentity.GetCurrent();
            if (currentUser.User?.IsWellKnown(WellKnownSidType.AccountGuestSid) ?? true)
            {
                DisplayMessage.Show(MessageId.GuestNotSupported);
                return;
            }

            // add Plugins directory to probing path
            var currentDomain = AppDomain.CurrentDomain;

            ProcessKeepalive.SetFactory(ManualKeepalive.Factory);

            var opts = WriterCommandLineOptions.Create(args);
            if (opts == null)
            {
                return;
            }

            opts.ApplyOptions();

            // Make the appId unique by base directory and locale.
            // This might allow for easier testing of English vs. loc builds.
            var appId = "OpenLiveWriterApplication";
            try
            {
                appId = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1}.{2}{3}",
                    "OpenLiveWriterApplication",
                    currentDomain.BaseDirectory.GetHashCode(),
                    CultureInfo.CurrentUICulture,
                    Res.DebugMode ? ".locspy" : string.Empty);
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
            }

            // initialize the default internet features for browser controls
            InitInternetFeatures();

            SingleInstanceApplicationManager.Run(appId, LaunchAction, args);
        }

        /// <summary>
        /// Initializes the Internet features that should be applied to the embedded browsers launched by this process.
        /// </summary>
        public static void InitInternetFeatures()
        {
            // Turn on Local Machine Lockdown mode.  This ensures HTML accidentally executed in the
            // Local Machine security zone does not get privileges elevated
            UrlMon.CoInternetSetFeatureEnabled(FEATURE.LOCALMACHINE_LOCKDOWN, INTERNETSETFEATURE.ON_PROCESS, true);

            // enable the security band so it will be displayed if any embedded browser control wants to prompt for permission
            // Note: the security band does not work for the MSHTML editor.
            UrlMon.CoInternetSetFeatureEnabled(FEATURE.SECURITYBAND, INTERNETSETFEATURE.ON_PROCESS, true);

            // Unset locking down the Behaviors feature so that the editor's custom behaviors will load.
            // Local Machine lockdown mode will lockdown the behaviors feature completely for HTML executed
            // in the Local Machine security zone.  This guarantees that editor behaviors no matter how tightly
            // the system has browsers locked down.
            // Note: this probably isn't necessary now that the editor always loads HTML in the Internet Zone.
            UrlMon.CoInternetSetFeatureEnabled(FEATURE.BEHAVIORS, INTERNETSETFEATURE.ON_PROCESS, false);

            // Turn off the annoying click sound that occurs if a browser navigation occurs in an embedded control
            UrlMon.CoInternetSetFeatureEnabled(FEATURE.DISABLE_NAVIGATION_SOUNDS, INTERNETSETFEATURE.ON_PROCESS, true);
        }

        /// <summary>
        /// Loads the culture.
        /// </summary>
        /// <param name="cultureName">Name of the culture.</param>
        private static void LoadCulture([NotNull] string cultureName)
        {
            try
            {
                CultureHelper.ApplyUICulture(cultureName);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// This is false until this process is ready to launch windows.
        /// Protects against secondary windows starting up before initialization
        /// has finished.
        /// </summary>
        private static volatile bool initComplete = false;

        /// <summary>
        /// Launches the action.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="isFirstInstance">if set to <c>true</c> [is first instance].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool LaunchAction(string[] args, bool isFirstInstance)
        {
            if (isFirstInstance)
            {
                // Use GDI strings everywhere
                Application.SetCompatibleTextRenderingDefault(false);

                InitializeApplicationEnvironment();

                try
                {
                    // TODO:OLW
                    // using (WindowsLiveSetup windowsLiveSetup = new WindowsLiveSetup())
                    // {
                    try
                    {
                        // TODO:OLW
                        // Load the culture.
                        LoadCulture("en");

                        // Apply any culture overrides.
                        var opts = WriterCommandLineOptions.Create(args);
                        if (!string.IsNullOrEmpty(opts.CultureOverride))
                        {
                            LoadCulture(opts.CultureOverride);
                        }

                        // Save the current culture for other instances.
                        currentUICulture = CultureInfo.CurrentUICulture;

                        // show splash screen
                        IDisposable splashScreen = null;

                        // Show the splash screen.
                        var splashScreenForm = new SplashScreen();
                        splashScreenForm.Show();
                        splashScreenForm.Update();
                        splashScreen = new FormSplashScreen(splashScreenForm);

                        LaunchFirstInstance(splashScreen, args);
                    }
                    finally
                    {
                        // TODO:OLW
                        // windowsLiveSetup.ClearThreadUILanguages();
                    }

                    // }
                }
                catch (Exception)
                {
                    // Most likely we failed to create WLSetupClass, installation corrupt or incomplete
                    // Redirect user to repair/reinstall.
                    DisplayMessage.Show(MessageId.WriterCannotStart);
                }

                return true;
            }

            return LaunchAdditionalInstance(args);
        }

        /// <summary>
        /// Initializes the application environment.
        /// </summary>
        private static void InitializeApplicationEnvironment()
        {
            // initialize application environment
            ApplicationEnvironment.Initialize();
            ApplicationEnvironment.ProductName_Short = "Writer";
            ApplicationEnvironment.ProductDisplayVersion = string.Format(
                CultureInfo.InvariantCulture,
                Res.Get(StringId.ProductDisplayVersion),
                ApplicationEnvironment.ProductVersion);

            if (PlatformHelper.RunningOnWin7OrHigher())
            {
                TaskbarManager.Instance.ApplicationId = ApplicationEnvironment.TaskbarApplicationId;
            }
        }

        /// <summary>
        /// Launches a window as part of the starting of a process.
        /// </summary>
        /// <param name="splashScreen">The splash screen.</param>
        /// <param name="args">The arguments.</param>
        private static void LaunchFirstInstance([CanBeNull]IDisposable splashScreen, [CanBeNull] string[] args)
        {
            try
            {
                PostEditorFile.Initialize();

                MaybeMigrateSettings();

                // Make sure editor options are available before we launch the first instance.
                GlobalEditorOptions.Init(new OpenLiveWriterContentTarget(), new OpenLiveWriterSettingsProvider());

                // register file associations
                // Removing this call, as it causes exceptions in Vista
                // RegisterFileAssociations() ;
                Trace.WriteLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Starting {0} {1}",
                        ApplicationEnvironment.ProductNameQualified,
                        ApplicationEnvironment.ProductVersion));
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, ".NET version: {0}", Environment.Version));

                // force initialization which may fail with error dialogs
                // and/or cause the whole application to not load
                if (PostEditorLifetimeManager.Initialize())
                {
                    initComplete = true;

                    // launch blogging form
                    ApplicationLauncher.LaunchBloggingForm(args, splashScreen, true);
                }

                if (splashScreen != null)
                {
                    try
                    {
                        using (var splashScreenForm = ((FormSplashScreen)splashScreen).Form)
                        {
                            if (splashScreenForm != null && !splashScreenForm.IsDisposed)
                            {
                                Application.Run(splashScreenForm);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Fail("Failed to show splash screen: " + e);
                    }
                }

                ManualKeepalive.Wait(true);
            }
            catch (DirectoryException ex)
            {
                if (ex.MessageId != null)
                {
                    DisplayMessage.Show(ex.MessageId.Value);
                }
                else if (ex.Path != null)
                {
                    DisplayMessage.Show(MessageId.DirectoryFail, ex.Path);
                }
                else
                {
                    UnexpectedErrorMessage.Show(ex);
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
            finally
            {
                try
                {
                    // shut down
                    PostEditorLifetimeManager.Uninitialize();
                    TempFileManager.Instance.Dispose();

                    // Delete legacy post supporting files that might have
                    // been orphaned by a previous version of Writer. We
                    // now keep these in the temp directory like everything
                    // else
                    var legacyPostSupportingFiles = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "PostSupportingFiles");
                    if (Directory.Exists(legacyPostSupportingFiles))
                    {
                        Directory.Delete(legacyPostSupportingFiles, true);
                    }

                }
                catch (Exception ex2)
                {
                    Trace.Fail(ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Launches a window requested by another instance of OpenLiveWriter.exe.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool LaunchAdditionalInstance(string[] args)
        {
            // This process received a request from another process before
            // initialization was complete; this request should be considered
            // a failure, and the other process should try again
            if (!initComplete)
            {
                return false;
            }

            // Do this only after initComplete is true, otherwise currentUICulture could be null.
            CultureHelper.ApplyUICulture(currentUICulture.Name);

            try
            {
                // TODO:OLW
                {
                    // using (Setup Setup = new Setup())
                    try
                    {
                        // TODO:OLW
                        // Set native code's preferred language list
                        // Setup.SetThreadUILanguages();
                        ApplicationLauncher.LaunchBloggingForm(args, null, false);
                        return true;
                    }
                    catch (ManualKeepaliveOperationException)
                    {
                        return false;
                    }
                    catch (Exception e)
                    {
                        Trace.Fail(e.ToString());
                        return true;
                    }
                    finally
                    {
                        // TODO:OLW
                        // Setup.ClearThreadUILanguages();
                    }
                }
            }
            catch (Exception)
            {
                // Most likely we failed to create WLSetupClass, installation corrupt or incomplete
                // Redirect user to repair/reinstall.
                DisplayMessage.Show(MessageId.WriterCannotStart);
            }

            return true;
        }

        /// <summary>
        /// Maybes the migrate settings.
        /// </summary>
        private static void MaybeMigrateSettings()
        {
            try
            {
                var applicationSettingsVer = 1;
                var userSettings = ApplicationEnvironment.UserSettingsRoot;
                var currentSettingsVer = userSettings.GetInt32(SETTINGS_VERSION, 0);
                if (currentSettingsVer == 0)
                {
                    userSettings.SetInt32(SETTINGS_VERSION, applicationSettingsVer);
                    Trace.WriteLine("Migrating user settings...");

                    var legacyKeyName = @"Software\Open Live Writer";
                    var legacySettings = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.CurrentUser, legacyKeyName));
                    ApplicationEnvironment.UserSettingsRoot.CopyFrom(legacySettings, true, false); // Don't overwrite existing settings

                    Registry.CurrentUser.DeleteSubKeyTree(legacyKeyName);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Error while attempting to migrate settings.", ex.ToString());
            }
        }

        /// <summary>
        /// The settings version
        /// </summary>
        private const string SETTINGS_VERSION = "SettingsVer";
    }
}
