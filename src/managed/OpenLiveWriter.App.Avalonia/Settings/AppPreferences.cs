// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.App.Avalonia.Settings
{
    /// <summary>How the shell opens posts — mirrors Windows <c>PostWindowBehavior</c>.</summary>
    public enum PostWindowBehavior
    {
        UseSameWindow,
        OpenNewWindow,
        OpenNewWindowIfDirty
    }

    /// <summary>
    /// Cross-platform preference snapshot for the macOS shell. Field names and defaults
    /// follow the Windows Post Editor / Editing / Spelling / Web Proxy panels where
    /// relevant (see <c>testplan/testOptionsDialogBox</c>).
    /// </summary>
    public sealed class AppPreferences
    {
        // ---- General / Post windows (Preferences tab) ----
        public PostWindowBehavior PostWindowBehavior { get; set; } = PostWindowBehavior.UseSameWindow;
        public bool ViewPostAfterPublish { get; set; } = true;
        public bool CloseWindowOnPublish { get; set; }
        public bool TitleReminder { get; set; } = true;
        public bool CategoryReminder { get; set; }
        public bool TagReminder { get; set; }
        public bool AutoSaveDrafts { get; set; } = true;
        public int AutoSaveMinutes { get; set; } = 3;
        public bool ShowRealTimeWordCount { get; set; }
        public bool FormatHtml { get; set; } = true;

        // ---- Editing tab ----
        public bool ReplaceHyphens { get; set; } = true;
        public bool ReplaceSmartQuotes { get; set; } = true;
        public bool ReplaceSpecialCharacters { get; set; } = true;
        public bool ReplaceEmoticons { get; set; } = true;
        public bool UseParagraphTags { get; set; } = true;

        // ---- Spelling tab ----
        public bool SpellcheckEnabled { get; set; } = true;

        // ---- Web Proxy tab ----
        public bool ProxyEnabled { get; set; }
        public string ProxyHostname { get; set; }
        public int ProxyPort { get; set; } = 8080;
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }

        /// <summary>Factory defaults used when no persisted settings exist.</summary>
        public static AppPreferences CreateDefault() => new AppPreferences();

        /// <summary>Deep copy for dialog editing without mutating the live snapshot.</summary>
        public AppPreferences Clone() => new AppPreferences
        {
            PostWindowBehavior = PostWindowBehavior,
            ViewPostAfterPublish = ViewPostAfterPublish,
            CloseWindowOnPublish = CloseWindowOnPublish,
            TitleReminder = TitleReminder,
            CategoryReminder = CategoryReminder,
            TagReminder = TagReminder,
            AutoSaveDrafts = AutoSaveDrafts,
            AutoSaveMinutes = AutoSaveMinutes,
            ShowRealTimeWordCount = ShowRealTimeWordCount,
            FormatHtml = FormatHtml,
            ReplaceHyphens = ReplaceHyphens,
            ReplaceSmartQuotes = ReplaceSmartQuotes,
            ReplaceSpecialCharacters = ReplaceSpecialCharacters,
            ReplaceEmoticons = ReplaceEmoticons,
            UseParagraphTags = UseParagraphTags,
            SpellcheckEnabled = SpellcheckEnabled,
            ProxyEnabled = ProxyEnabled,
            ProxyHostname = ProxyHostname,
            ProxyPort = ProxyPort,
            ProxyUsername = ProxyUsername,
            ProxyPassword = ProxyPassword
        };
    }
}
