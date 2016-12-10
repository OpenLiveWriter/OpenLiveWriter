// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;

    using JetBrains.Annotations;

    using OpenLiveWriter.BlogClient;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.Interop.Windows.TaskDialog;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.PostEditor;
    using OpenLiveWriter.PostEditor.Configuration.Wizard;
    using OpenLiveWriter.PostEditor.JumpList;
    using OpenLiveWriter.PostEditor.OpenPost;
    using OpenLiveWriter.PostEditor.Updates;

    /// <summary>
    /// Class ApplicationLauncher.
    /// </summary>
    public class ApplicationLauncher
    {
        /// <summary>
        /// Launches the blogging form.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="splashScreen">The splash screen.</param>
        /// <param name="isFirstInstance">if set to <c>true</c> [is first instance].</param>
        public static void LaunchBloggingForm(string[] args, [CanBeNull] IDisposable splashScreen, bool isFirstInstance)
        {
            try
            {
                using (ProcessKeepalive.Open())
                {
                    UpdateManager.CheckforUpdates();

                    // If the COM registration is not set up correctly, we won't be able to launch.
                    RunningObjectTable.EnsureComRegistration();

                    // make sure bloggging is configured before we proceed
                    if (EnsureBloggingConfigured(splashScreen))
                    {
                        var options = WriterCommandLineOptions.Create(args);

                        // check for a prefs request
                        if (options.IsShowPreferences)
                        {
                            splashScreen?.Dispose();

                            ExecuteShowPreferences(options.PreferencesPage);
                        }

                        // check for an open-post request
                        else if (options.IsOpenPost)
                        {
                            splashScreen?.Dispose();

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
                splashScreen?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Recovers the posts.
        /// </summary>
        /// <param name="splashScreen">The splash screen.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool RecoverPosts([CanBeNull] IDisposable splashScreen)
        {
            if (!PostEditorSettings.AutoSaveDrafts)
            {
                return false;
            }

            var autoSaveDir = PostEditorSettings.AutoSaveDirectory;
            var autoSavedPostFiles = Directory.GetFiles(autoSaveDir, "*.wpost");
            if (autoSavedPostFiles.Length > 0)
            {
                splashScreen?.Dispose();

                var result = AutoRecoverPrompt(null, autoSavedPostFiles.Length);

                switch (result)
                {
                    case AutoRecoverPromptResult.Recover:
                        foreach (var autoSavedPost in autoSavedPostFiles)
                        {
                            ExecutePostEditorFile(autoSavedPost, splashScreen);
                        }

                        return true;
                    case AutoRecoverPromptResult.Discard:
                        foreach (var autoSavedPost in autoSavedPostFiles)
                        {
                            File.Delete(autoSavedPost);
                        }

                        return false;
                    case AutoRecoverPromptResult.AskLater:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Enum AutoRecoverPromptResult
        /// </summary>
        private enum AutoRecoverPromptResult
        {
            /// <summary>
            /// The recover
            /// </summary>
            Recover,

            /// <summary>
            /// The discard
            /// </summary>
            Discard,

            /// <summary>
            /// The ask later
            /// </summary>
            AskLater
        }

        /// <summary>
        /// Automatics the recover prompt.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="count">The count.</param>
        /// <returns>AutoRecoverPromptResult.</returns>
        private static AutoRecoverPromptResult AutoRecoverPrompt(IWin32Window window, int count)
        {
            const int ID_RECOVER = 100; // same as ID_CANCEL
            const int ID_DISCARD = 101; // same as ID_CANCEL
            const int ID_ASKLATER = 2; // same as ID_CANCEL

            while (true)
            {
                var td = new TaskDialog
                             {
                                 WindowTitle = ApplicationEnvironment.ProductNameQualified,
                                 MainInstruction =
                                     string.Format(
                                         CultureInfo.CurrentCulture,
                                         Res.Get(StringId.AutoRecoverDialogInstruction),
                                         ApplicationEnvironment.ProductNameQualified),
                                 Content =
                                     string.Format(
                                         CultureInfo.CurrentCulture,
                                         Res.Get(StringId.AutoRecoverDialogContent),
                                         ApplicationEnvironment.ProductNameQualified),

                                 // td.MainIcon = TaskDialogIcon.Warning;
                                 AllowDialogCancellation = true,
                                 UseCommandLinks = true
                             };
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
                        if (DialogResult.Yes
                            == DisplayMessage.Show(
                                MessageId.AutoRecoverPromptDiscardConfirm,
                                Win32WindowImpl.ActiveWin32Window))
                        {
                            return AutoRecoverPromptResult.Discard;
                        }

                        continue;
                    case ID_ASKLATER:
                        return AutoRecoverPromptResult.AskLater;
                    default:
                        Debug.Fail("Unknown ID " + result);
                        return AutoRecoverPromptResult.AskLater;
                }
            }
        }

        /// <summary>
        /// Needses the expiration warning.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool NeedsExpirationWarning()
        {
            if (DateTime.Now > ExpirationSettings.Expires)
            {
                return true;
            }

            var days = new[] { 1, 2, 3, 4, 5, 15, 30 };
            var index = Array.BinarySearch(days, ExpirationSettings.DaysRemaining);

            if (index < 0)
            {
                index = ~index;
            }

            if (index >= days.Length)
            {
                return false;
            }

            var bucket = days[index];
            if (ExpirationSettings.LastWarnDays == bucket)
            {
                return false;
            }

            ExpirationSettings.LastWarnDays = bucket;
            return true;
        }

        /// <summary>
        /// Executes the show preferences.
        /// </summary>
        /// <param name="panelName">Name of the panel.</param>
        private static void ExecuteShowPreferences(string panelName)
        {
            PreferencesHandler.Instance.ShowPreferences(Win32WindowImpl.DesktopWin32Window, panelName);
        }

        /// <summary>
        /// Executes the open post.
        /// </summary>
        private static void ExecuteOpenPost()
        {
            using (var openPostForm = new OpenPostForm())
            {
                if (openPostForm.ShowDialog(Win32WindowImpl.DesktopWin32Window) == DialogResult.OK)
                {
                    var editingContext = openPostForm.BlogPostEditingContext;
                    PostEditorForm.Launch(editingContext, true);
                }
            }
        }

        /// <summary>
        /// Executes the post editor file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="splashScreen">The splash screen.</param>
        private static void ExecutePostEditorFile(string filename, [CanBeNull] IDisposable splashScreen)
        {
            if (VerifyPostEditorFileIsEditable(filename))
            {
                // load the contents of the file
                var postEditorFile = PostEditorFile.GetExisting(new FileInfo(filename));
                var editingContext = postEditorFile.Load();

                // launch the editing form (request post synchronization)
                PostEditorForm.Launch(editingContext, true, splashScreen);
            }
            else
            {
                splashScreen?.Dispose();
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

        private static void ExecuteNewPost([CanBeNull] IDisposable splashScreen, string switchToBlog)
        {
            PostEditorForm.Launch(switchToBlog, splashScreen);
        }

        /// <summary>
        /// Ensures the blogging configured.
        /// </summary>
        /// <param name="splashScreen">The splash screen.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool EnsureBloggingConfigured([CanBeNull] IDisposable splashScreen)
        {
            // see if the user needs to configure their blog first
            if (!BloggingConfigured || ApplicationDiagnostics.SimulateFirstRun)
            {
                // create a new profile
                return CreateInitialProfile(splashScreen);
            }

            return true;
        }

        /// <summary>
        /// Creates the initial profile.
        /// </summary>
        /// <param name="splashScreen">The splash screen.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool CreateInitialProfile([CanBeNull]IDisposable splashScreen)
        {
            using (new WaitCursor())
            {
                splashScreen?.Dispose();

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

        /// <summary>
        /// Gets a value indicating whether [blogging configured].
        /// </summary>
        /// <value><c>true</c> if [blogging configured]; otherwise, <c>false</c>.</value>
        private static bool BloggingConfigured => BlogSettings.DefaultBlogId != string.Empty;
    }
}
