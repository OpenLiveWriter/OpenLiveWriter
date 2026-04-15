// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Platform.Mac
{
    public static class MacPlatformInitializer
    {
        public static void Initialize()
        {
            PlatformContext.Initialize(
                services: new MacPlatformServices(),
                display: new MacDisplayHelper(),
                credentials: new MacCredentialStorage(),
                bidi: new MacBidiSupport(),
                spellCheck: new MacSpellCheckProvider());
        }
    }
}
