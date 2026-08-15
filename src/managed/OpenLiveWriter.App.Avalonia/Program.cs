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
            // Must run before anything else, Avalonia included: after an update
            // Velopack relaunches the app with hook arguments it expects to
            // handle and exit on, so starting a UI first would flash a window.
            AppUpdater.RunStartupHooks();

            // Initialize platform services based on OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OpenLiveWriter.Platform.Mac.MacPlatformInitializer.Initialize();
            }
            // Windows and Linux will be added as platform projects are created

            // Stages any newer release in the background; it applies on the
            // next launch. Never blocks or fails startup.
            AppUpdater.CheckInBackground();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
