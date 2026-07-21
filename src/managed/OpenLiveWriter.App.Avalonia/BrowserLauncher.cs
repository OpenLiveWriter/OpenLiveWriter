// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Opens a URL in the system default browser (used by the "View post after
    /// publishing" preference). The <see cref="UrlHandler"/> seam lets headless
    /// tests intercept launches instead of spawning a real browser process.
    /// </summary>
    public static class BrowserLauncher
    {
        /// <summary>Test seam: when set, <see cref="Open"/> redirects here (no browser).</summary>
        public static Action<string> UrlHandler { get; set; }

        /// <summary>
        /// Opens <paramref name="url"/> in the default browser. Returns false when the
        /// URL is blank or the launch fails — publishing itself has already succeeded,
        /// so a launch problem is never surfaced as an error.
        /// </summary>
        public static bool Open(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (UrlHandler != null)
            {
                UrlHandler(url);
                return true;
            }

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
