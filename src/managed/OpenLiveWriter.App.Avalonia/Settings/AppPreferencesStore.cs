// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.App.Avalonia.Settings
{
    /// <summary>
    /// Loads and saves <see cref="AppPreferences"/> through the platform
    /// <see cref="ISettingsPersister"/> seam (JSON files on macOS via
    /// <c>FileSettingsPersister</c>). The host resolves the storage root via
    /// <see cref="IPlatformServices.GetApplicationDataDirectory"/> — never hardcoded.
    /// </summary>
    public sealed class AppPreferencesStore
    {
        private const string PreferencesRootKey = "Preferences";

        private readonly Func<ISettingsPersister> _createRoot;

        public AppPreferencesStore(Func<ISettingsPersister> createRoot)
        {
            _createRoot = createRoot ?? throw new ArgumentNullException(nameof(createRoot));
        }

        /// <summary>Uses the initialized platform context to resolve the settings root.</summary>
        public static AppPreferencesStore CreateDefault()
        {
            PlatformContext.EnsureInitialized();
            return new AppPreferencesStore(() =>
                PlatformContext.Services.CreateUserSettingsPersister(PreferencesRootKey));
        }

        /// <summary>Test seam: persist under a caller-supplied settings factory (temp dir).</summary>
        public static AppPreferencesStore ForPersisterFactory(Func<ISettingsPersister> createRoot)
            => new AppPreferencesStore(createRoot);

        public AppPreferences Load()
        {
            using ISettingsPersister root = _createRoot();
            var prefs = AppPreferences.CreateDefault();

            using ISettingsPersister postEditor = root.GetSubSettings("PostEditor");
            prefs.PostWindowBehavior = ReadEnum(
                postEditor, "PostWindowBehavior", prefs.PostWindowBehavior);
            prefs.ViewPostAfterPublish = (bool)postEditor.Get(
                "ViewPostAfterPublish", typeof(bool), prefs.ViewPostAfterPublish);
            prefs.CloseWindowOnPublish = (bool)postEditor.Get(
                "CloseWindowOnPublish", typeof(bool), prefs.CloseWindowOnPublish);
            prefs.TitleReminder = (bool)postEditor.Get(
                "TitleReminder", typeof(bool), prefs.TitleReminder);
            prefs.CategoryReminder = (bool)postEditor.Get(
                "CategoryReminder", typeof(bool), prefs.CategoryReminder);
            prefs.TagReminder = (bool)postEditor.Get(
                "TagReminder", typeof(bool), prefs.TagReminder);
            prefs.AutoSaveDrafts = (bool)postEditor.Get(
                "AutoRecover", typeof(bool), prefs.AutoSaveDrafts);
            prefs.AutoSaveMinutes = (int)postEditor.Get(
                "AutoSaveMinutes", typeof(int), prefs.AutoSaveMinutes);
            prefs.FormatHtml = (bool)postEditor.Get(
                "FormatHTML", typeof(bool), prefs.FormatHtml);
            prefs.UseParagraphTags = (bool)postEditor.Get(
                "UseParagraphTags", typeof(bool), prefs.UseParagraphTags);

            using ISettingsPersister wordCount = postEditor.GetSubSettings("WordCount");
            prefs.ShowRealTimeWordCount = (bool)wordCount.Get(
                "ShowWordCount", typeof(bool), prefs.ShowRealTimeWordCount);

            using ISettingsPersister autoreplace = postEditor.GetSubSettings("Autoreplace");
            prefs.ReplaceHyphens = (bool)autoreplace.Get("Hyphens", typeof(bool), prefs.ReplaceHyphens);
            prefs.ReplaceSmartQuotes = (bool)autoreplace.Get("SmartQuotes", typeof(bool), prefs.ReplaceSmartQuotes);
            prefs.ReplaceSpecialCharacters = (bool)autoreplace.Get(
                "OtherSpecialCharacters", typeof(bool), prefs.ReplaceSpecialCharacters);
            prefs.ReplaceEmoticons = (bool)autoreplace.Get("Emoticons", typeof(bool), prefs.ReplaceEmoticons);

            using ISettingsPersister spelling = root.GetSubSettings("Spelling");
            prefs.SpellcheckEnabled = (bool)spelling.Get(
                "SpellcheckEnabled", typeof(bool), prefs.SpellcheckEnabled);
            prefs.CheckSpellingBeforePublishing = (bool)spelling.Get(
                "CheckSpellingBeforePublishing", typeof(bool), prefs.CheckSpellingBeforePublishing);

            using ISettingsPersister proxy = root.GetSubSettings("WebProxy");
            prefs.ProxyEnabled = (bool)proxy.Get("Enabled", typeof(bool), prefs.ProxyEnabled);
            prefs.ProxyHostname = (string)proxy.Get("Hostname", typeof(string), prefs.ProxyHostname);
            prefs.ProxyPort = (int)proxy.Get("Port", typeof(int), prefs.ProxyPort);
            prefs.ProxyUsername = (string)proxy.Get("Username", typeof(string), prefs.ProxyUsername);
            prefs.ProxyPassword = (string)proxy.Get("Password", typeof(string), prefs.ProxyPassword);

            return prefs;
        }

        public void Save(AppPreferences prefs)
        {
            if (prefs == null)
                throw new ArgumentNullException(nameof(prefs));

            using ISettingsPersister root = _createRoot();
            using (root.BatchUpdate())
            {
                using ISettingsPersister postEditor = root.GetSubSettings("PostEditor");
                postEditor.Set("PostWindowBehavior", prefs.PostWindowBehavior.ToString());
                postEditor.Set("ViewPostAfterPublish", prefs.ViewPostAfterPublish);
                postEditor.Set("CloseWindowOnPublish", prefs.CloseWindowOnPublish);
                postEditor.Set("TitleReminder", prefs.TitleReminder);
                postEditor.Set("CategoryReminder", prefs.CategoryReminder);
                postEditor.Set("TagReminder", prefs.TagReminder);
                postEditor.Set("AutoRecover", prefs.AutoSaveDrafts);
                postEditor.Set("AutoSaveMinutes", prefs.AutoSaveMinutes);
                postEditor.Set("FormatHTML", prefs.FormatHtml);
                postEditor.Set("UseParagraphTags", prefs.UseParagraphTags);

                using ISettingsPersister wordCount = postEditor.GetSubSettings("WordCount");
                wordCount.Set("ShowWordCount", prefs.ShowRealTimeWordCount);

                using ISettingsPersister autoreplace = postEditor.GetSubSettings("Autoreplace");
                autoreplace.Set("Hyphens", prefs.ReplaceHyphens);
                autoreplace.Set("SmartQuotes", prefs.ReplaceSmartQuotes);
                autoreplace.Set("OtherSpecialCharacters", prefs.ReplaceSpecialCharacters);
                autoreplace.Set("Emoticons", prefs.ReplaceEmoticons);

                using ISettingsPersister spelling = root.GetSubSettings("Spelling");
                spelling.Set("SpellcheckEnabled", prefs.SpellcheckEnabled);
                spelling.Set("CheckSpellingBeforePublishing", prefs.CheckSpellingBeforePublishing);

                using ISettingsPersister proxy = root.GetSubSettings("WebProxy");
                proxy.Set("Enabled", prefs.ProxyEnabled);
                proxy.Set("Hostname", prefs.ProxyHostname ?? string.Empty);
                proxy.Set("Port", prefs.ProxyPort);
                proxy.Set("Username", prefs.ProxyUsername ?? string.Empty);
                if (string.IsNullOrEmpty(prefs.ProxyPassword))
                    proxy.Unset("Password");
                else
                    proxy.Set("Password", prefs.ProxyPassword);
            }
        }

        /// <summary>Loads persisted main-window geometry (defaults when missing).</summary>
        public WindowLayout LoadWindowLayout()
        {
            using ISettingsPersister root = _createRoot();
            var layout = WindowLayout.CreateDefault();

            using ISettingsPersister bounds = root.GetSubSettings("WindowBounds");
            layout.Width = ReadDouble(bounds, "Width", layout.Width);
            layout.Height = ReadDouble(bounds, "Height", layout.Height);
            layout.X = (int)bounds.Get("X", typeof(int), layout.X);
            layout.Y = (int)bounds.Get("Y", typeof(int), layout.Y);
            layout.Maximized = (bool)bounds.Get("Maximized", typeof(bool), layout.Maximized);

            // Clamp to sensible minimums even if a corrupt value was stored.
            if (layout.Width < WindowLayout.MinWidth)
                layout.Width = WindowLayout.MinWidth;
            if (layout.Height < WindowLayout.MinHeight)
                layout.Height = WindowLayout.MinHeight;

            return layout;
        }

        /// <summary>Persists main-window geometry under the Preferences settings root.</summary>
        public void SaveWindowLayout(WindowLayout layout)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            using ISettingsPersister root = _createRoot();
            using (root.BatchUpdate())
            {
                using ISettingsPersister bounds = root.GetSubSettings("WindowBounds");
                bounds.Set("Width", layout.Width);
                bounds.Set("Height", layout.Height);
                bounds.Set("X", layout.X);
                bounds.Set("Y", layout.Y);
                bounds.Set("Maximized", layout.Maximized);
            }
        }

        private static double ReadDouble(ISettingsPersister persister, string key, double defaultValue)
        {
            object raw = persister.Get(key, typeof(double), defaultValue);
            if (raw is double d)
                return d;
            if (raw is int i)
                return i;
            if (raw is long l)
                return l;
            if (raw is float f)
                return f;
            if (raw is string s && double.TryParse(s, out double parsed))
                return parsed;
            return defaultValue;
        }

        private static PostWindowBehavior ReadEnum(
            ISettingsPersister persister, string key, PostWindowBehavior defaultValue)
        {
            object raw = persister.Get(key, typeof(string), defaultValue.ToString());
            if (raw is string s && Enum.TryParse(s, ignoreCase: true, out PostWindowBehavior parsed))
                return parsed;
            return defaultValue;
        }
    }
}
