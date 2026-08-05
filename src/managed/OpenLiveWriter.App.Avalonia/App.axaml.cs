// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Input;
using global::Avalonia.Markup.Xaml;

namespace OpenLiveWriter.App.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;
                SetApplicationMenu(desktop, mainWindow);
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Replaces the macOS application menu, the bold one beside the Apple
        /// logo. Without an application-level NativeMenu, Avalonia supplies its
        /// own default whose first entry is "About Avalonia", which is not
        /// something this app should be advertising. Providing a menu here
        /// replaces that default wholesale, so Quit has to be included: it is
        /// not added back automatically.
        /// </summary>
        private void SetApplicationMenu(IClassicDesktopStyleApplicationLifetime desktop, MainWindow mainWindow)
        {
            var about = new NativeMenuItem("About Open Live Writer");
            about.Click += async (s, e) => await mainWindow.ExecuteCommandAsync(Localization.CommandId.About);

            var checkForUpdates = new NativeMenuItem("Check for Updates\u2026");
            checkForUpdates.Click += async (s, e) =>
                await mainWindow.ExecuteCommandAsync(Localization.CommandId.CheckForUpdates);

            var preferences = new NativeMenuItem("Preferences\u2026") { Gesture = KeyGesture.Parse("Cmd+,") };
            preferences.Click += async (s, e) => await mainWindow.ExecuteCommandAsync(Localization.CommandId.Options);

            var quit = new NativeMenuItem("Quit Open Live Writer") { Gesture = KeyGesture.Parse("Cmd+Q") };
            quit.Click += (s, e) => desktop.Shutdown();

            NativeMenu.SetMenu(this, new NativeMenu
            {
                about,
                checkForUpdates,
                new NativeMenuItemSeparator(),
                preferences,
                new NativeMenuItemSeparator(),
                quit,
            });
        }
    }
}
