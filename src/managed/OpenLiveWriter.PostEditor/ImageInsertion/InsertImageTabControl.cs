// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.ImageInsertion
{
    /// <summary>
    /// Summary description for InsertImageTabControl.
    /// </summary>
    public class InsertImageTabControl : TabPageControl
    {
        private UITheme _uiTheme;
        public InsertImageTabControl() : base()
        {
            _uiTheme = new UITheme(this);
        }

        public override ApplicationStyle ApplicationStyle
        {
            get
            {
                return _uiTheme._applicationStyle;
            }
        }

        private class UITheme : ControlUITheme
        {
            public ApplicationStyle _applicationStyle;
            public UITheme(Control control) : base(control, false)
            {
                _applicationStyle = new ApplicationStyle();
                ApplyTheme();
            }

            protected override void ApplyTheme(bool highContrast)
            {
                int R = SystemColors.Control.R;
                int G = SystemColors.Control.G;
                int B = SystemColors.Control.B;
                Color dark = !highContrast ? Color.FromArgb((int)(R * 0.9), (int)(G * 0.9), (int)(B * 0.9)) : SystemColors.ControlDark;
                Color darkDark = !highContrast ? Color.FromArgb((int)(R * 0.7), (int)(G * 0.7), (int)(B * 0.7)) : SystemColors.ControlDarkDark;

                _applicationStyle.BorderColor = darkDark;
                _applicationStyle.ActiveTabTopColor = SystemColors.Control;
                _applicationStyle.ActiveTabBottomColor = SystemColors.Control;
                _applicationStyle.InactiveTabTopColor = dark;
                _applicationStyle.InactiveTabBottomColor = dark;
                _applicationStyle.ActiveTabHighlightColor = SystemColors.ControlLight;
                _applicationStyle.InactiveTabHighlightColor = SystemColors.Control;
                _applicationStyle.ActiveTabLowlightColor = dark;
                _applicationStyle.InactiveTabLowlightColor = darkDark;
                _applicationStyle.ActiveTabTextColor = !highContrast ? Color.Black : SystemColors.ControlText;
                _applicationStyle.InactiveTabTextColor = !highContrast ? Color.Black : SystemColors.ControlText;
                base.ApplyTheme(highContrast);
            }
        }
    }
}
