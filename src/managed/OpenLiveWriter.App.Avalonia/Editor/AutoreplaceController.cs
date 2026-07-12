// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.App.Avalonia.Settings;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Autoreplace toggles mirrored from <see cref="AppPreferences"/> Editing tab.
    /// </summary>
    public sealed class AutoreplaceOptions
    {
        public bool ReplaceHyphens { get; set; } = true;
        public bool ReplaceSmartQuotes { get; set; } = true;
        public bool ReplaceSpecialCharacters { get; set; } = true;
        public bool ReplaceEmoticons { get; set; } = true;

        public bool AnyEnabled =>
            ReplaceHyphens || ReplaceSmartQuotes || ReplaceSpecialCharacters || ReplaceEmoticons;

        public static AutoreplaceOptions FromPreferences(AppPreferences prefs)
        {
            if (prefs == null)
                return new AutoreplaceOptions();

            return new AutoreplaceOptions
            {
                ReplaceHyphens = prefs.ReplaceHyphens,
                ReplaceSmartQuotes = prefs.ReplaceSmartQuotes,
                ReplaceSpecialCharacters = prefs.ReplaceSpecialCharacters,
                ReplaceEmoticons = prefs.ReplaceEmoticons
            };
        }
    }

    /// <summary>
    /// Pure text autoreplace applied on paste (and mirrored in the editor bridge for
    /// live typing). Logic follows the Windows <c>TypographicCharacterHandler</c> subset
    /// that can run without MSHTML.
    /// </summary>
    public static class AutoreplaceTransformer
    {
        private static readonly Regex DashEm = new(
            @"[^\s\u00A0\-]([ \u00A0]?(?:--?)[ \u00A0]?)[^\s\u00A0\-]",
            RegexOptions.Compiled);

        /// <summary>Transforms plain text according to the enabled autoreplace toggles.</summary>
        public static string TransformPlainText(string text, AutoreplaceOptions options)
        {
            if (string.IsNullOrEmpty(text) || options == null || !options.AnyEnabled)
                return text ?? string.Empty;

            string result = text;
            if (options.ReplaceSpecialCharacters)
                result = ReplaceSpecialSequences(result);
            if (options.ReplaceHyphens)
                result = ReplaceHyphenSequences(result);
            if (options.ReplaceSmartQuotes)
                result = ReplaceStraightQuotes(result);
            if (options.ReplaceEmoticons)
                result = ReplaceTextEmoticons(result);
            return result;
        }

        private static string ReplaceSpecialSequences(string text)
        {
            return text
                .Replace("(c)", "\u00A9", StringComparison.OrdinalIgnoreCase)
                .Replace("(r)", "\u00AE", StringComparison.OrdinalIgnoreCase)
                .Replace("(tm)", "\u2122", StringComparison.OrdinalIgnoreCase)
                .Replace("...", "\u2026");
        }

        private static string ReplaceHyphenSequences(string text)
        {
            return DashEm.Replace(text, m =>
            {
                string match = m.Groups[1].Value.Replace('\u00A0', ' ');
                return match switch
                {
                    "--" or "-- " => m.Value.Replace(match, "\u2014"),
                    " --" or " -" => m.Value.Replace(match, "\u00A0\u2013"),
                    " -- " or " - " => m.Value.Replace(match, "\u00A0\u2013 "),
                    _ => m.Value
                };
            });
        }

        private static string ReplaceStraightQuotes(string text)
        {
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c != '"' && c != '\'')
                {
                    sb.Append(c);
                    continue;
                }

                bool isOpen = true;
                if (i > 0)
                {
                    char prev = text[i - 1];
                    switch (prev)
                    {
                        case '-':
                        case '{':
                        case '[':
                        case '(':
                            isOpen = true;
                            break;
                        default:
                            if (!char.IsWhiteSpace(prev))
                                isOpen = false;
                            break;
                    }
                }

                if (c == '\'')
                    sb.Append(isOpen ? '\u2018' : '\u2019');
                else
                    sb.Append(isOpen ? '\u201C' : '\u201D');
            }
            return sb.ToString();
        }

        private static string ReplaceTextEmoticons(string text)
        {
            // Common text emoticons → Unicode emoji (paste path only).
            return text
                .Replace(":-)", "\u263A")
                .Replace(":)", "\u263A")
                .Replace(":-(", "\u2639")
                .Replace(":(", "\u2639");
        }
    }

    /// <summary>
    /// Builds bridge scripts that push autoreplace toggles into the WebView editor.
    /// </summary>
    public static class AutoreplaceController
    {
        public static string BuildSetAutoreplaceScript(AutoreplaceOptions options)
        {
            options ??= new AutoreplaceOptions();
            return string.Format(
                CultureInfo.InvariantCulture,
                "OLWBridge.setAutoreplace({{smartQuotes:{0},hyphens:{1},special:{2},emoticons:{3}}})",
                Bool(options.ReplaceSmartQuotes),
                Bool(options.ReplaceHyphens),
                Bool(options.ReplaceSpecialCharacters),
                Bool(options.ReplaceEmoticons));
        }

        private static string Bool(bool value) => value ? "true" : "false";
    }
}
