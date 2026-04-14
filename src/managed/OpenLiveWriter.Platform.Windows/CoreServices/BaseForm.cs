// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.CoreServices
{
    public class BaseForm : Form
    {
        private bool allowFontChange = true;
        private bool suppressAutoRtlFixup = false;
        public BaseForm()
        {
#pragma warning disable 612, 618
            AutoScale = false;
#pragma warning restore 612, 618
            this.Font = Res.DefaultFont;
            allowFontChange = false;

            //support for bidi languages
            if (BidiHelper.IsRightToLeft)
            {
                //this one is making the 3rd toolbar shorter/missing buttons and preventing the html editor from loading
                RightToLeft = RightToLeft.Yes;
                RightToLeftLayout = true;
                //the two of them together make the words in the status bar and start screen go backwards
                //  and the 2nd toolbar go black.
            }
            else
            {
                RightToLeft = RightToLeft.No;
                RightToLeftLayout = false;
            }
        }

        public bool SuppressAutoRtlFixup
        {
            get { return suppressAutoRtlFixup; }
            set { suppressAutoRtlFixup = value; }
        }

        private bool scaled = false;
        protected override void SetVisibleCore(bool value)
        {
            if (value && !scaled)
            {
                scaled = true;

                Debug.Assert(AutoScaleMode != AutoScaleMode.Font,
                             "Don't use AutoScaleMode.Font, since we change fonts at runtime");

                if (AutoScaleMode == AutoScaleMode.Dpi || AutoScaleMode == AutoScaleMode.Inherit)
                    DisplayHelper.Scale(this);
            }

            base.SetVisibleCore(value);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!suppressAutoRtlFixup)
                BidiHelper.RtlLayoutFixup(this);

            if (SystemInformation.HighContrast)
            {
                ControlHelper.Walk(
                    delegate (Control c, object state)
                        {
                            LinkLabel link = c as LinkLabel;
                            if (link != null)
                            {
                                if (link.LinkColor == SystemColors.HotTrack)
                                    link.LinkColor = SystemColors.ControlText;
                                if (link.LinkBehavior != LinkBehavior.AlwaysUnderline)
                                    link.LinkBehavior = LinkBehavior.AlwaysUnderline;
                            }
                        },
                        this,
                        null);
            }
        }

        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                Trace.Assert(allowFontChange, "Font changed!");
                base.Font = value;
            }
        }
    }
}
