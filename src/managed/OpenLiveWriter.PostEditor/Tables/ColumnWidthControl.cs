// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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

    public class ColumnWidthControl : System.Windows.Forms.UserControl
    {
        private OpenLiveWriter.Controls.NumericTextBox textBoxWidth;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelPixels;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ColumnWidthControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.labelWidth.Text = Res.Get(StringId.WidthLabel);
            this.labelPixels.Text = Res.Get(StringId.pixels);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right, false))
            {
                DisplayHelper.AutoFitSystemLabel(labelWidth, 0, int.MaxValue);
                DisplayHelper.AutoFitSystemLabel(labelPixels, 0, int.MaxValue);
                LayoutHelper.DistributeHorizontally(8, labelWidth, textBoxWidth, labelPixels);
            }
        }

        public int ColumnWidth
        {
            get
            {
                if (textBoxWidth.Text != String.Empty)
                {
                    try
                    {
                        return int.Parse(textBoxWidth.Text, CultureInfo.CurrentCulture);
                    }
                    catch
                    {
                        return 0;
                    }
                }
                else
                    return 0;
            }
            set
            {
                if (value != 0)
                    textBoxWidth.Text = value.ToString(CultureInfo.CurrentCulture);
                else
                    textBoxWidth.Text = String.Empty;
            }
        }

        public bool ValidateInput()
        {
            return ValidateInput(0);
        }

        public bool ValidateInput(int maxValue)
        {
            int result;
            if (textBoxWidth.Text == String.Empty || !Int32.TryParse(textBoxWidth.Text, out result))
            {
                DisplayMessage.Show(MessageId.UnspecifiedValue, this, Res.Get(StringId.Width));
                textBoxWidth.Focus();
                return false;
            }
            else if (ColumnWidth <= 0)
            {
                DisplayMessage.Show(MessageId.InvalidNumberPositiveOnly, FindForm(), Res.Get(StringId.Width));
                textBoxWidth.Focus();
                return false;
            }
            else if (maxValue > 0 && ColumnWidth >= maxValue)
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxWidth = new OpenLiveWriter.Controls.NumericTextBox();
            this.labelWidth = new System.Windows.Forms.Label();
            this.labelPixels = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // textBoxWidth
            //
            this.textBoxWidth.Location = new System.Drawing.Point(72, 0);
            this.textBoxWidth.MaxLength = 9;
            this.textBoxWidth.Name = "textBoxWidth";
            this.textBoxWidth.Size = new System.Drawing.Size(46, 20);
            this.textBoxWidth.TabIndex = 2;
            this.textBoxWidth.Text = "";
            //
            // labelWidth
            //
            this.labelWidth.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWidth.Location = new System.Drawing.Point(0, 3);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(64, 15);
            this.labelWidth.TabIndex = 0;
            this.labelWidth.Text = "&Width:";
            //
            // labelPixels
            //
            this.labelPixels.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPixels.Location = new System.Drawing.Point(120, 3);
            this.labelPixels.Name = "labelPixels";
            this.labelPixels.Size = new System.Drawing.Size(63, 15);
            this.labelPixels.TabIndex = 3;
            this.labelPixels.Text = "pixels";
            //
            // ColumnWidthControl
            //
            this.Controls.Add(this.labelPixels);
            this.Controls.Add(this.textBoxWidth);
            this.Controls.Add(this.labelWidth);
            this.Name = "ColumnWidthControl";
            this.Size = new System.Drawing.Size(348, 23);
            this.ResumeLayout(false);

        }
        #endregion

    }


}
