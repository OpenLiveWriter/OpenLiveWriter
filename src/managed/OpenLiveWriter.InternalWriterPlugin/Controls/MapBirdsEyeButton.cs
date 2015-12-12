// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{

    internal class MapBirdsEyeButton : MapBitmapButton
    {
        private ControlTheme _theme;
        public MapBirdsEyeButton()
            : base("GoBirdsEye")
        {
            _theme = new ControlTheme(this, true);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);
            g.DrawText(Res.Get(StringId.MapSeeInBirdseye), Font,
                       new Rectangle(ScaleX(TEXT_INSET), 0, Width - ScaleX(TEXT_INSET + 5), Height), Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);

        }

        private class ControlTheme : ControlUITheme
        {
            public Color TextColor;
            public ControlTheme(Control control, bool applyTheme) : base(control, applyTheme)
            {
            }

            protected override void ApplyTheme(bool highContrast)
            {
                TextColor = !highContrast ? Color.FromArgb(GraphicsHelper.Opacity(60), Color.Black) : SystemColors.ControlText;
                base.ApplyTheme(highContrast);
            }
        }

        private const int TEXT_INSET = 58;

    }
}
