// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public struct ColorPickerColor
    {
        public ColorPickerColor(Color color, StringId id)
        {
            Color = color;
            StringId = id;
        }

        public Color Color;
        public StringId StringId;
    }

    public interface IColorPickerCommand
    {
        string[] StandardColorsTooltips { get; }
        uint[] StandardColors { get; }
        uint NumStandardColorsRows { get; }
        uint NumColumns { get; }
        Color SelectedColor { get; }
        int SelectedColorAsBGR { get; }
        SwatchColorType SelectedColorType { get; }
        bool Automatic { get; set; }
        void SetSelectedColor(Color color, SwatchColorType type);
    }
}
