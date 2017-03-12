// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.PostEditor.JumpList;

namespace OpenLiveWriter.PostEditor
{
    public enum PostWindowBehavior
    {
        UseSameWindow,
        OpenNewWindow,
        OpenNewWindowIfDirty
    }

    public class PostEditorSettings
    {
        public static bool ViewPostAfterPublish
        {
            get { return SettingsKey.GetBoolean(VIEW_POST_AFTER_PUBLISH, true); }
            set { SettingsKey.SetBoolean(VIEW_POST_AFTER_PUBLISH, value); }
        }
        private const string VIEW_POST_AFTER_PUBLISH = "ViewPostAfterPublish";

        public static bool CloseWindowOnPublish
        {
            get { return SettingsKey.GetBoolean(CLOSE_WINDOW_ON_PUBLISH, false); }
            set { SettingsKey.SetBoolean(CLOSE_WINDOW_ON_PUBLISH, value); }
        }
        private const string CLOSE_WINDOW_ON_PUBLISH = "CloseWindowOnPublish";

        public static bool TitleReminder
        {
            get { return SettingsKey.GetBoolean(TITLE_REMINDER, true); }
            set { SettingsKey.SetBoolean(TITLE_REMINDER, value); }
        }
        private const string TITLE_REMINDER = "TitleReminder";

        public static bool FuturePublishDateWarning
        {
            get { return SettingsKey.GetBoolean(FUTURE_PUBLISH_DATE_WARNING, true); }
            set { SettingsKey.SetBoolean(FUTURE_PUBLISH_DATE_WARNING, value); }
        }
        private const string FUTURE_PUBLISH_DATE_WARNING = "FuturePublishDateWarning";

        public static bool CategoryReminder
        {
            get { return SettingsKey.GetBoolean(CATEGORY_REMINDER, false); }
            set { SettingsKey.SetBoolean(CATEGORY_REMINDER, value); }
        }
        private const string CATEGORY_REMINDER = "CategoryReminder";

        public static bool TagReminder
        {
            get { return SettingsKey.GetBoolean(TAG_REMINDER, false); }
            set { SettingsKey.SetBoolean(TAG_REMINDER, value); }
        }
        private const string TAG_REMINDER = "TagReminder";

        public static bool Ping
        {
            get { return SettingsKey.GetBoolean(PING, false); }
            set { SettingsKey.SetBoolean(PING, value); }
        }
        private const string PING = "Ping";

        public static string[] PingUrls
        {
            get
            {
                return SettingsKey.GetStrings(PING_URLS, DEFAULT_PING_URLS);
            }
            set
            {
                SettingsKey.SetStrings(PING_URLS, value);
            }
        }

        private static string[] DEFAULT_PING_URLS = { };

        private const string PING_URLS = "PingUrls";

        public static bool AutoSaveDrafts
        {
            get { return SettingsKey.GetBoolean(AUTO_SAVE_DRAFTS, true); }
            set { SettingsKey.SetBoolean(AUTO_SAVE_DRAFTS, value); }
        }
        private const string AUTO_SAVE_DRAFTS = "AutoRecover";

        public static int AutoSaveMinutes
        {
            get { return SettingsKey.GetInt32(AUTO_SAVE_MINUTES, 3); }
            set { SettingsKey.SetInt32(AUTO_SAVE_MINUTES, value); }
        }
        private const string AUTO_SAVE_MINUTES = "AutoSaveMinutes";

        public static string AutoSaveDirectory
        {
            get
            {
                string autoSaveDir = Path.Combine(ApplicationEnvironment.LocalApplicationDataDirectory, "AutoRecover");
                Directory.CreateDirectory(autoSaveDir);
                return autoSaveDir;
            }
        }

        public static PostWindowBehavior PostWindowBehavior
        {
            get { return (PostWindowBehavior)Enum.Parse(typeof(PostWindowBehavior), SettingsKey.GetString(POST_WINDOW_BEHAVIOR, PostWindowBehavior.UseSameWindow.ToString())); }
            set { SettingsKey.SetString(POST_WINDOW_BEHAVIOR, value.ToString()); }
        }
        private const string POST_WINDOW_BEHAVIOR = "PostWindowBehavior";

        public static bool PostEditorWindowMaximized
        {
            get { return SettingsKey.GetBoolean(POST_EDITOR_WINDOW_MAXIMIZED, false); }
            set { SettingsKey.SetBoolean(POST_EDITOR_WINDOW_MAXIMIZED, value); }
        }
        private const string POST_EDITOR_WINDOW_MAXIMIZED = "MainWindowMaximized";

        public static Rectangle PostEditorWindowBounds
        {
            get { return SettingsKey.GetRectangle(POST_EDITOR_WINDOW_BOUNDS, DefaultWindowBounds); }
            set { SettingsKey.SetRectangle(POST_EDITOR_WINDOW_BOUNDS, value); }
        }
        private const string POST_EDITOR_WINDOW_BOUNDS = "MainWindowBounds";
        public static readonly Rectangle DefaultWindowBounds = new Rectangle(0, 0, 800, 650);

        public static Point PostEditorWindowLocation
        {
            get { return SettingsKey.GetPoint(POST_EDITOR_WINDOW_LOCATION, new Point(0, 0)); }
            set { SettingsKey.SetPoint(POST_EDITOR_WINDOW_LOCATION, value); }
        }
        private const string POST_EDITOR_WINDOW_LOCATION = "MainWindowLocation";

        public static SizeF PostEditorWindowScale
        {
            get { return SettingsKey.GetSizeF(POST_EDITOR_WINDOW_SCALE, new SizeF(1f, 1f)); }
            set { SettingsKey.SetSizeF(POST_EDITOR_WINDOW_SCALE, value); }
        }
        private const string POST_EDITOR_WINDOW_SCALE = "MainWindowScale";

        public static Size OpenPostFormSize
        {
            get { return SettingsKey.GetSize(OPEN_POST_FORM_SIZE, new Size(650, 485)); }
            set { SettingsKey.SetSize(OPEN_POST_FORM_SIZE, value); }
        }
        private const string OPEN_POST_FORM_SIZE = "OpenPostFormSize";

        public static bool AllowSettingsAutoUpdate
        {
            get { return SettingsKey.GetBoolean(ALLOW_SETTINGS_AUTO_UPDATE, true); }
            set { SettingsKey.SetBoolean(ALLOW_SETTINGS_AUTO_UPDATE, value); }
        }
        private const string ALLOW_SETTINGS_AUTO_UPDATE = "AllowSettingsAutoUpdate";

        public static bool AllowProviderButtons
        {
            get { return SettingsKey.GetBoolean(ALLOW_PROVIDER_BUTTONS, true); }
            set { SettingsKey.SetBoolean(ALLOW_PROVIDER_BUTTONS, value); }
        }
        private const string ALLOW_PROVIDER_BUTTONS = "AllowProviderButtons";

        public static bool AutomationMode
        {
            get { return SettingsKey.GetBoolean(AUTOMATION_MODE, false); }
            set { SettingsKey.SetBoolean(AUTOMATION_MODE, value); }
        }
        private const string AUTOMATION_MODE = "AutomationMode";

        public static string WeblogPostsFolder
        {
            get { return SettingsKey.GetString(WEBLOG_POSTS_FOLDER, null); }
            set { SettingsKey.SetString(WEBLOG_POSTS_FOLDER, value); }
        }
        private const string WEBLOG_POSTS_FOLDER = "PostsDirectory";

        internal static SettingsPersisterHelper SettingsKey = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");
        public static SettingsPersisterHelper RecentEmoticonsKey = SettingsKey.GetSubSettings("RecentEmoticons");
    }
}
