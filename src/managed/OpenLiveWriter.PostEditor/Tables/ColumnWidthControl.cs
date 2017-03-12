// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    /* Why is percent based column sizing not allowed? We want to in all cases let the
     * table flow to occupy the width of its container. This provides for both robust
     * behavior accross blogs/templates/normal mode and also allows the browser table
     * rendering logic to automatically "balance" columns based on their content and
     * preferred widths. Percent mode requires that the table be given a fixed width
     * (otherwise you end up with a table of essentially zero size). Note that users
     * are essentially able to do percent based sizing by providing a set of
     * preferred widths to their columns (if the preferred widths exceed the available
     * width in the table's parent block then they become relative guidelines for
     * sizing the columns).
     */

    public partial class ColumnWidthControl : System.Windows.Forms.UserControl
    {
        public ColumnWidthControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.labelWidth.Text = Res.Get(StringId.WidthLabel);
            this.rbPixels.Text = Res.Get(StringId.Pixels);
            this.rbPercent.Text = Res.Get(StringId.Percent);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right, false))
            {
                DisplayHelper.AutoFitSystemLabel(labelWidth, 0, int.MaxValue);
                DisplayHelper.AutoFitSystemRadioButton(rbPixels, 0, int.MaxValue);
                DisplayHelper.AutoFitSystemRadioButton(rbPercent, 0, int.MaxValue);
                LayoutHelper.DistributeHorizontally(8, labelWidth, textBoxWidth, rbPixels, rbPercent);
            }
        }

        public PixelPercent ColumnWidth
        {
            get
            {
                if (textBoxWidth.Text != String.Empty)
                {
                    try
                    {
                        var units = rbPercent.Checked ? PixelPercentUnits.Percentage : PixelPercentUnits.Pixels;
                        return new PixelPercent(textBoxWidth.Text, CultureInfo.CurrentCulture, units);
                    }
                    catch
                    {
                        return new PixelPercent();
                    }
                }
                else
                    return new PixelPercent();
            }
            set
            {
                if (value.Units != PixelPercentUnits.Undefined)
                {
                    textBoxWidth.Text = value.Value.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    textBoxWidth.Text = String.Empty;
                }

                if (value.Units == PixelPercentUnits.Percentage)
                {
                    rbPercent.Checked = true;
                }
                else
                {
                    rbPixels.Checked = true;
                }
            }
        }

        public bool ValidateInput()
        {
            return ValidateInput(0);
        }

        public bool ValidateInput(int maxValue)
        {
            if (!PixelPercent.IsAcceptableWidth(textBoxWidth.Text))
            {
                DisplayMessage.Show(MessageId.UnspecifiedValue, this, Res.Get(StringId.Width));
                textBoxWidth.Focus();
                return false;
            }
            else if (ColumnWidth.Units != PixelPercentUnits.Undefined && ColumnWidth.Value <= 0)
            {
                DisplayMessage.Show(MessageId.InvalidNumberPositiveOnly, FindForm(), Res.Get(StringId.Width));
                textBoxWidth.Focus();
                return false;
            }
            else if (maxValue > 0 && ColumnWidth.Units != PixelPercentUnits.Undefined && ColumnWidth.Value >= maxValue)
            {
                DisplayMessage.Show(MessageId.ValueExceedsMaximum, FindForm(), maxValue, Res.Get(StringId.Width));
                textBoxWidth.Focus();
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
