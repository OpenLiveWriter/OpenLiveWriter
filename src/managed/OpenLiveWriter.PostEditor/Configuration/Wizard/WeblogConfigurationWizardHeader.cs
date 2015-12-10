// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    /// <summary>
    /// Summary description for WeblogConfigurationWizardHeader.
    /// </summary>
    public class WeblogConfigurationWizardHeader : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public WeblogConfigurationWizardHeader(StringId label)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.Font = Res.GetFont(FontSize.GiantHeading, FontStyle.Regular);
            TabStop = false;

            _label = Res.Get(label);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            // draw image
            const int HEADER_X_INSET = 0;
            const int HEADER_Y_INSET = 0;
            g.DrawImage(false, _headerImage, HEADER_X_INSET, HEADER_Y_INSET);

            // draw text centered vertically
            const int TEXT_X_INSET = 5;
            Size textSize = g.MeasureText(_label, Font);
            int textY = (Height / 2) - (textSize.Height / 2);
            Color textColor = !SystemInformation.HighContrast ? Color.FromArgb(0, 77, 131) : SystemColors.WindowText;
            Rectangle textRect = new Rectangle(new Point(HEADER_X_INSET + _headerImage.Width + TEXT_X_INSET, textY), textSize);
            g.DrawText(_label, Font, textRect, textColor);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private string _label;
        private Image _headerImage =
            BidiHelper.Mirror(ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Wizard.Images.WeblogWizardHeader.png"));

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // WeblogConfigurationWizardHeader
            //
            this.Font = Res.GetFont(FontSize.GiantHeading, FontStyle.Regular);
            this.Name = "WeblogConfigurationWizardHeader";
            this.Size = new System.Drawing.Size(310, 40);
        }
        #endregion

    }
}
