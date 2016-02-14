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
                    UpdateManager.CheckforUpdates();

                    // If the COM registration is not set up correctly, we won't be able to launch.
                    RunningObjectTable.EnsureComRegistration();

                    // make sure blogging is configured before we proceed
                    if (EnsureBloggingConfigured(splashScreen))
                    {
                        WriterCommandLineOptions options = WriterCommandLineOptions.Create(args);

                        // check for a prefs request
                        if (options.IsShowPreferences)
                        {
                            if (splashScreen != null)
                                splashScreen.Dispose();

                            ExecuteShowPreferences(options.PreferencesPage);
                        }

                        // check for an open-post request
                        else if (options.IsOpenPost)
                        {
                            if (splashScreen != null)
                                splashScreen.Dispose();

                            ExecuteOpenPost();
                        }

                        // check for opening an existing post via the shell file association
                        else if (options.IsPostEditorFile)
                        {
                            ExecutePostEditorFile(options.PostEditorFileName, splashScreen);
                        }

                        // check for recovered posts
                        else if (isFirstInstance && RecoverPosts(splashScreen))
                        {
                            return;
                        }

                        // launch with an new empty post
                        else
                        {
                            ExecuteNewPost(splashScreen, null);
                        }
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
            const int ID_RECOVER = 100,
                ID_DISCARD = 101,
                ID_ASKLATER = 2; // same as ID_CANCEL

            while (true)
            {
                TaskDialog td = new TaskDialog();

                td.WindowTitle = ApplicationEnvironment.ProductNameQualified;
                td.MainInstruction = string.Format(CultureInfo.CurrentCulture,
                                                   Res.Get(StringId.AutoRecoverDialogInstruction),
                                                   ApplicationEnvironment.ProductNameQualified);
                td.Content = string.Format(CultureInfo.CurrentCulture,
                                           Res.Get(StringId.AutoRecoverDialogContent),
                                           ApplicationEnvironment.ProductNameQualified);
                //            td.MainIcon = TaskDialogIcon.Warning;

                td.AllowDialogCancellation = true;

                td.UseCommandLinks = true;
                td.Buttons.Add(new TaskDialogButton(ID_RECOVER, Res.Get(StringId.AutoRecoverDialogButtonRecover)));
                td.Buttons.Add(new TaskDialogButton(ID_DISCARD, Res.Get(StringId.AutoRecoverDialogButtonDiscard)));
                td.Buttons.Add(new TaskDialogButton(ID_ASKLATER, Res.Get(StringId.AutoRecoverDialogButtonAskLater)));

                int result, radioResult;
                bool flag;
                td.Show(window, out result, out radioResult, out flag);
                switch (result)
                {
                    case ID_RECOVER:
                        return AutoRecoverPromptResult.Recover;
                    case ID_DISCARD:
                        // WinLive 225110 - Pass ActiveWin32Window here as owner instead of default ForegroundWin32Window
                        // ForegroundWin32Window could return a window of an app that is in admin mode and if we are running
                        // non-admin then trying to use that as parent for MessageBox would cause it to return 'No' without showing
                        // the dialog.
                        if (DialogResult.Yes == DisplayMessage.Show(MessageId.AutoRecoverPromptDiscardConfirm, Win32WindowImpl.ActiveWin32Window))
                            return AutoRecoverPromptResult.Discard;
                        continue;
                    case ID_ASKLATER:
                        return AutoRecoverPromptResult.AskLater;
                    default:
                        Debug.Fail("Unknown ID " + result);
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
