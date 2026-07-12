// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Thin, UI-agnostic surface over the platform <see cref="ISpellCheckProvider"/>
    /// (on macOS, <c>MacSpellCheckProvider</c>). It exposes availability, a status
    /// message and suggestions so the shell can report spelling state without touching
    /// the platform singleton directly, and so it is testable with a fake provider.
    /// </summary>
    public class SpellCheckService
    {
        /// <summary>Default checking language when the caller doesn't specify one.</summary>
        public const string DefaultLanguage = "en";

        private readonly ISpellCheckProvider _provider;

        public SpellCheckService(ISpellCheckProvider provider)
        {
            _provider = provider;
        }

        /// <summary>Builds a service over the initialized platform spell provider (may be null).</summary>
        public static SpellCheckService CreateDefault() =>
            new SpellCheckService(PlatformContext.IsInitialized ? PlatformContext.SpellCheck : null);

        /// <summary>True when the platform provides a system dictionary for the language.</summary>
        public bool IsAvailable(string language = DefaultLanguage) =>
            _provider != null && _provider.IsAvailable(language);

        /// <summary>Reports the user-facing spelling status for the current toggle state.</summary>
        public string StatusMessage(bool enabled, string language = DefaultLanguage) =>
            SpellCheckController.DescribeStatus(_provider, language, enabled);

        /// <summary>Returns suggestions for a word (empty when no provider is available).</summary>
        public IReadOnlyList<string> GetSuggestions(string word, string language = DefaultLanguage) =>
            _provider?.GetSuggestions(word, language) ?? Array.Empty<string>();
    }
}
