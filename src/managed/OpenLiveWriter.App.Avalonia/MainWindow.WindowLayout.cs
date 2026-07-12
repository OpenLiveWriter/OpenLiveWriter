// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using OpenLiveWriter.App.Avalonia.Settings;

namespace OpenLiveWriter.App.Avalonia
{
    /// <summary>
    /// Restores and persists main-window size/position via
    /// <see cref="AppPreferencesStore"/> WindowBounds, clamping to the working area.
    /// </summary>
    public partial class MainWindow
    {
        // Last Normal-state geometry — Avalonia has no RestoreBounds, so we track it
        // ourselves and use it when persisting from a Maximized close.
        private double _normalWidth = WindowLayout.DefaultWidth;
        private double _normalHeight = WindowLayout.DefaultHeight;
        private int _normalX = WindowLayout.UnsetCoordinate;
        private int _normalY = WindowLayout.UnsetCoordinate;

        private void InitializeWindowLayout()
        {
            // Explicit system chrome — do not extend client area under the macOS
            // title bar / traffic lights (avoids content colliding with window controls).
            ExtendClientAreaToDecorationsHint = false;
            WindowDecorations = WindowDecorations.Full;

            try
            {
                if (_preferencesStore == null)
                    return;

                WindowLayout layout = _preferencesStore.LoadWindowLayout();
                ApplyWindowLayout(layout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Window] Restore layout failed: {ex.Message}");
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            PropertyChanged += OnWindowLayoutPropertyChanged;
            PositionChanged += (s, e) =>
            {
                if (WindowState == WindowState.Normal)
                    CaptureNormalGeometry();
            };
            Closing += (s, e) => PersistWindowLayout();
            // Re-clamp after the window is realized (screens are reliable then).
            Opened += (s, e) => ClampToScreenWorkingArea();
        }

        private void OnWindowLayoutPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == WindowStateProperty)
            {
                // Capture Normal geometry just before maximize so a Maximized close
                // can still restore the pre-maximize size next launch.
                if (e.OldValue is WindowState oldState && oldState == WindowState.Normal &&
                    e.NewValue is WindowState newState && newState == WindowState.Maximized)
                {
                    CaptureNormalGeometry();
                }
                return;
            }

            if (WindowState != WindowState.Normal)
                return;

            if (e.Property == WidthProperty || e.Property == HeightProperty)
                CaptureNormalGeometry();
        }

        private void CaptureNormalGeometry()
        {
            _normalWidth = Width;
            _normalHeight = Height;
            _normalX = Position.X;
            _normalY = Position.Y;
        }

        private void ApplyWindowLayout(WindowLayout layout)
        {
            if (layout == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }

            Width = Math.Max(layout.Width, MinWidth);
            Height = Math.Max(layout.Height, MinHeight);
            _normalWidth = Width;
            _normalHeight = Height;

            if (layout.HasSavedPosition)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Position = new PixelPoint(layout.X, layout.Y);
                _normalX = layout.X;
                _normalY = layout.Y;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (layout.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void ClampToScreenWorkingArea()
        {
            try
            {
                Screen screen = Screens?.ScreenFromWindow(this) ?? Screens?.Primary;
                if (screen == null)
                    return;

                PixelRect work = screen.WorkingArea;
                double width = Math.Min(Width, work.Width);
                double height = Math.Min(Height, work.Height);
                width = Math.Max(width, MinWidth);
                height = Math.Max(height, MinHeight);
                Width = width;
                Height = height;

                int x = Position.X;
                int y = Position.Y;
                // Keep at least 80px of the title bar visible horizontally, and
                // the full title-bar strip within the working area vertically.
                const int minVisibleX = 80;
                if (x + minVisibleX > work.X + work.Width)
                    x = work.X + work.Width - minVisibleX;
                if (x + Width < work.X + minVisibleX)
                    x = work.X;
                if (y < work.Y)
                    y = work.Y;
                if (y > work.Y + work.Height - 40)
                    y = work.Y + Math.Max(0, work.Height - 40);

                if (x != Position.X || y != Position.Y)
                    Position = new PixelPoint(x, y);

                if (WindowState == WindowState.Normal)
                    CaptureNormalGeometry();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Window] Clamp failed: {ex.Message}");
            }
        }

        private void PersistWindowLayout()
        {
            if (_preferencesStore == null)
                return;

            try
            {
                var layout = new WindowLayout
                {
                    Maximized = WindowState == WindowState.Maximized,
                    Width = _normalWidth,
                    Height = _normalHeight,
                    X = _normalX,
                    Y = _normalY
                };

                // If we never captured Normal geometry (e.g. opened Maximized from
                // a prior session without coords), fall back to current size.
                if (layout.Width < WindowLayout.MinWidth || layout.Height < WindowLayout.MinHeight)
                {
                    layout.Width = Math.Max(Width, WindowLayout.MinWidth);
                    layout.Height = Math.Max(Height, WindowLayout.MinHeight);
                }

                if (!layout.HasSavedPosition && WindowState == WindowState.Normal)
                {
                    layout.X = Position.X;
                    layout.Y = Position.Y;
                }

                _preferencesStore.SaveWindowLayout(layout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLW-Window] Persist layout failed: {ex.Message}");
            }
        }
    }
}
