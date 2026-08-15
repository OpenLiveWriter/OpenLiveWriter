// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Tests.WebView2Editor
{
    /// <summary>
    /// Guards live WebView2 editor tests so they only attempt to create a
    /// WebView2 environment in an interactive window station. Under a service
    /// session (session 0, e.g. SYSTEM test runs) the loader cannot initialize
    /// and, when the user data folder is not writable for the Edge processes,
    /// pops a "can't read and write to its data directory" dialog on the
    /// interactive desktop. Skipping up front avoids both the dialog and the
    /// 30-second ready timeouts.
    /// </summary>
    internal static class WebView2TestSession
    {
        public static void RequireInteractiveSession()
        {
            if (!System.Environment.UserInteractive)
            {
                Assert.Ignore(
                    "WebView2 editor tests require an interactive window station; " +
                    "run them as the logged-on user (OLW_VM_TEST_USER=current).");
            }
        }
    }
}
