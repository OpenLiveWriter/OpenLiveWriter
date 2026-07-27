// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

// Registers the headless Avalonia application used by every [AvaloniaTest].
// This spins up a real Avalonia app/dispatcher without a display, which lets us
// construct and drive Avalonia controls (dialogs, panels) in-process. It does
// NOT provide a WKWebView backend — tests that need a live WebView are marked
// [Explicit] + [Category("WebView")] (see WebViewCategories).
[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace OpenLiveWriter.EditorTests.Automated.Infrastructure
{
    public sealed class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<TestApp>()
                // Real Skia backend so UiReview can CaptureRenderedFrame / RenderTargetBitmap
                // to PNG. UseHeadlessDrawing=false is required for IPlatformRenderInterface.
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions
                {
                    UseHeadlessDrawing = false
                });
    }

    /// <summary>
    /// Minimal Avalonia application with the Fluent theme so standard controls
    /// (TextBox, Button, CheckBox) initialize their templates during tests.
    /// Also loads the production AppStyles.axaml (macOS-style buttons) so
    /// UiReview captures match the real app.
    /// </summary>
    public sealed class TestApp : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
            Styles.Add(new global::Avalonia.Markup.Xaml.Styling.StyleInclude((System.Uri)null)
            {
                Source = new System.Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml")
            });
            Styles.Add(new global::Avalonia.Markup.Xaml.Styling.StyleInclude((System.Uri)null)
            {
                Source = new System.Uri("avares://OpenLiveWriter.App.Avalonia/AppStyles.axaml")
            });
        }
    }
}
