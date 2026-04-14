// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Static service locator for platform-specific implementations.
    /// Must be initialized once at application startup.
    /// </summary>
    public static class PlatformContext
    {
        private static bool _initialized;

        public static IPlatformServices Services { get; private set; }
        public static IDisplayHelper Display { get; private set; }
        public static ICredentialStorage Credentials { get; private set; }
        public static IBidiSupport Bidi { get; private set; }
        public static ISpellCheckProvider SpellCheck { get; private set; }
        public static IDialogService DialogService { get; private set; }
        public static ICredentialsPrompter CredentialsPrompter { get; private set; }
        public static ICaptchaHelper CaptchaHelper { get; private set; }

        public static void Initialize(
            IPlatformServices services,
            IDisplayHelper display,
            ICredentialStorage credentials,
            IBidiSupport bidi,
            ISpellCheckProvider spellCheck,
            IDialogService dialogService = null,
            ICredentialsPrompter credentialsPrompter = null,
            ICaptchaHelper captchaHelper = null)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Display = display ?? throw new ArgumentNullException(nameof(display));
            Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            Bidi = bidi ?? throw new ArgumentNullException(nameof(bidi));
            SpellCheck = spellCheck ?? throw new ArgumentNullException(nameof(spellCheck));
            DialogService = dialogService;
            CredentialsPrompter = credentialsPrompter;
            CaptchaHelper = captchaHelper;
            _initialized = true;
        }

        public static bool IsInitialized => _initialized;

        public static void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "PlatformContext has not been initialized. Call PlatformContext.Initialize() at application startup.");
        }

        internal static void Reset()
        {
            Services = null;
            Display = null;
            Credentials = null;
            Bidi = null;
            SpellCheck = null;
            DialogService = null;
            CredentialsPrompter = null;
            CaptchaHelper = null;
            _initialized = false;
        }
    }
}
