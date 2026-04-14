// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public static class WindowsPlatformInitializer
    {
        public static void Initialize()
        {
            PlatformContext.Initialize(
                services: new WindowsPlatformServices(),
                display: new WindowsDisplayHelper(),
                credentials: new WindowsCredentialStorage(),
                bidi: new WindowsBidiSupport(),
                spellCheck: new WindowsSpellCheckProvider());
        }
    }
}
