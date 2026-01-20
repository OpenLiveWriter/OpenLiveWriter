// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Interop.Windows.TaskDialog;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.PostEditor.JumpList;
using OpenLiveWriter.PostEditor.OpenPost;
using OpenLiveWriter.PostEditor.Configuration.Wizard;
using OpenLiveWriter.PostEditor.Updates;

namespace OpenLiveWriter
{
    public class ApplicationLauncher
    {
        public static void LaunchBloggingForm(string[] args, IDisposable splashScreen, bool isFirstInstance)
        {
            try
            {
                using (ProcessKeepalive.Open())
                {
                    System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: CheckforUpdates");
                    UpdateManager.CheckforUpdates();

                    // If the COM registration is not set up correctly, we won't be able to launch.
                    System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: EnsureComRegistration");
                    RunningObjectTable.EnsureComRegistration();

                    // make sure blogging is configured before we proceed
                    System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: EnsureBloggingConfigured");
                    if (EnsureBloggingConfigured(splashScreen))
                    {
                        System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: EnsureBloggingConfigured returned true");
                        WriterCommandLineOptions options = WriterCommandLineOptions.Create(args);

                        // check for a prefs request
                        if (options.IsShowPreferences)
                        {
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: ShowPreferences branch");
                            if (splashScreen != null)
                                splashScreen.Dispose();

                            ExecuteShowPreferences(options.PreferencesPage);
                        }

                        // check for an open-post request
                        else if (options.IsOpenPost)
                        {
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: OpenPost branch");
                            if (splashScreen != null)
                                splashScreen.Dispose();

                            ExecuteOpenPost();
                        }

                        // check for opening an existing post via the shell file association
                        else if (options.IsPostEditorFile)
                        {
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: PostEditorFile branch");
                            ExecutePostEditorFile(options.PostEditorFileName, splashScreen);
                        }

                        // check for recovered posts
                        else if (isFirstInstance && RecoverPosts(splashScreen))
                        {
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: RecoverPosts branch");
                            return;
                        }

                        // launch with an new empty post
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: NewPost branch - calling ExecuteNewPost");
                            ExecuteNewPost(splashScreen, null);
                            System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: ExecuteNewPost returned");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[OLW-DEBUG] LaunchBloggingForm: EnsureBloggingConfigured returned false");
                    }
                }
            }
            catch
            {
                if (splashScreen != null)
                    splashScreen.Dispose();
                throw;
            }
        }

        private static bool RecoverPosts(IDisposable splashScreen)
        {
            if (!PostEditorSettings.AutoSaveDrafts)
                return false;

            string autoSaveDir = PostEditorSettings.AutoSaveDirectory;
            string[] autoSavedPostFiles = Directory.GetFiles(autoSaveDir, "*.wpost");
            if (autoSavedPostFiles.Length > 0)
            {
                if (splashScreen != null)
                    splashScreen.Dispose();

                AutoRecoverPromptResult result = AutoRecoverPrompt(null, autoSavedPostFiles.Length);

                switch (result)
                {
                    case AutoRecoverPromptResult.Recover:
                        foreach (string autoSavedPost in autoSavedPostFiles)
                        {
                            ExecutePostEditorFile(autoSavedPost, splashScreen);
                        }
                        return true;
                    case AutoRecoverPromptResult.Discard:
                        foreach (string autoSavedPost in autoSavedPostFiles)
                            File.Delete(autoSavedPost);
                        return false;
                    case AutoRecoverPromptResult.AskLater:
                        return false;
                }
            }
            return false;
        }

        private enum AutoRecoverPromptResult
        {
            Recover,
            Discard,
            AskLater
        }

        private static AutoRecoverPromptResult AutoRecoverPrompt(IWin32Window window, int count)
        {
            // .NET 10: TaskDialog has struct marshaling issues, use MessageBox instead
            while (true)
            {
                var mbResult = System.Windows.Forms.MessageBox.Show(
                    string.Format(CultureInfo.CurrentCulture,
                        "{0} found {1} unsaved post(s). Would you like to recover them?\n\nYes = Recover\nNo = Discard\nCancel = Ask Later",
                        ApplicationEnvironment.ProductNameQualified, count),
                    ApplicationEnvironment.ProductNameQualified,
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.Question);

                switch (mbResult)
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        return AutoRecoverPromptResult.Recover;
                    case System.Windows.Forms.DialogResult.No:
                        // Confirm discard
                        if (DialogResult.Yes == DisplayMessage.Show(MessageId.AutoRecoverPromptDiscardConfirm, Win32WindowImpl.ActiveWin32Window))
                            return AutoRecoverPromptResult.Discard;
                        continue; // Ask again
                    default:
                        return AutoRecoverPromptResult.AskLater;
                }
            }
        }

        private static bool NeedsExpirationWarning()
        {
            if (DateTime.Now > ExpirationSettings.Expires)
                return true;

            int[] days = new int[] { 1, 2, 3, 4, 5, 15, 30 };
            int index = Array.BinarySearch(days, ExpirationSettings.DaysRemaining);

            if (index < 0)
                index = ~index;

            if (index >= days.Length)
                return false;

            int bucket = days[index];
            if (ExpirationSettings.LastWarnDays != bucket)
            {
                ExpirationSettings.LastWarnDays = bucket;
                return true;
            }
            return false;
        }

        private static void ExecuteShowPreferences(string panelName)
        {
            PreferencesHandler.Instance.ShowPreferences(Win32WindowImpl.DesktopWin32Window, panelName);
        }

        private static void ExecuteOpenPost()
        {
            using (OpenPostForm openPostForm = new OpenPostForm())
            {
                if (openPostForm.ShowDialog(Win32WindowImpl.DesktopWin32Window) == DialogResult.OK)
                {
                    IBlogPostEditingContext editingContext = openPostForm.BlogPostEditingContext;
                    PostEditorForm.Launch(editingContext, true);
                }
            }
        }

        private static void ExecutePostEditorFile(string filename, IDisposable splashScreen)
        {
            if (VerifyPostEditorFileIsEditable(filename))
            {
                // load the contents of the file
                PostEditorFile postEditorFile = PostEditorFile.GetExisting(new FileInfo(filename));
                IBlogPostEditingContext editingContext = postEditorFile.Load();

                // launch the editing form (request post synchronization)
                PostEditorForm.Launch(editingContext, true, splashScreen);
            }
            else
            {
                if (splashScreen != null)
                    splashScreen.Dispose();
            }
        }

        private static bool VerifyPostEditorFileIsEditable(string fileName)
        {
            // determine if the file is read-only (we don't support read-only b/c
            // we need to save the file before publishing it)
            if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) > 0)
            {
                DisplayMessage.Show(MessageId.ReadOnlyPostFile, Path.GetFileNameWithoutExtension(fileName));
                return false;
            }
            else
            {
                return true;
            }

        }

        private static void ExecuteNewPost(IDisposable splashScreen, string switchToBlog)
        {
            PostEditorForm.Launch(switchToBlog, splashScreen);
        }

        private static bool EnsureBloggingConfigured(IDisposable splashScreen)
        {
            // see if the user needs to configure their blog first
            if (!BloggingConfigured || ApplicationDiagnostics.SimulateFirstRun)
            {
                // create a new profile
                if (CreateInitialProfile(splashScreen))
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }
        }

        private static bool CreateInitialProfile(IDisposable splashScreen)
        {
            using (new WaitCursor())
            {
                if (splashScreen != null)
                    splashScreen.Dispose();

                if (WeblogConfigurationWizardController.Welcome(null) != null)
                {
                    // ensure we show the list of recent posts
                    PostListCache.Update();
                    WriterJumpList.Invalidate(IntPtr.Zero);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool BloggingConfigured
        {
            get
            {
                return BlogSettings.DefaultBlogId != String.Empty;
            }
        }
    }
}
