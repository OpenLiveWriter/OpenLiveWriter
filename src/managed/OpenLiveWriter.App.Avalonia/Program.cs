// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using Avalonia;

namespace OpenLiveWriter.App.Avalonia
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Initialize platform services based on OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OpenLiveWriter.Platform.Mac.MacPlatformInitializer.Initialize();
            }
            // Windows and Linux will be added as platform projects are created

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
