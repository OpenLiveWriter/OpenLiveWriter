// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.App.Avalonia.Settings
{
    /// <summary>
    /// Last-known main window geometry. Persisted separately from user Preferences
    /// so Options UI does not expose chrome layout. Mirrors Windows
    /// <c>PostEditorSettings.PostEditorWindowBounds</c>.
    /// </summary>
    public sealed class WindowLayout
    {
        public const double DefaultWidth = 1200;
        public const double DefaultHeight = 800;
        public const double MinWidth = 800;
        public const double MinHeight = 600;

        /// <summary>Sentinel: position was never saved — center on screen at restore.</summary>
        public const int UnsetCoordinate = int.MinValue;

        public double Width { get; set; } = DefaultWidth;
        public double Height { get; set; } = DefaultHeight;
        public int X { get; set; } = UnsetCoordinate;
        public int Y { get; set; } = UnsetCoordinate;
        public bool Maximized { get; set; }

        public bool HasSavedPosition =>
            X != UnsetCoordinate && Y != UnsetCoordinate;

        public static WindowLayout CreateDefault() => new WindowLayout();

        public WindowLayout Clone() => new WindowLayout
        {
            Width = Width,
            Height = Height,
            X = X,
            Y = Y,
            Maximized = Maximized
        };
    }
}
