// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.Controls
{
    public partial class ChoiceDialog : BaseForm
    {
        public static Color BlueText = SystemInformation.HighContrast ? SystemColors.MenuText : Color.FromArgb(0, 51, 153);

        public readonly string Heading;
        public readonly string SubHeading;

        public ChoiceDialog()
        {
            InitializeComponent();
        }

        private List<ChoiceOption> _options;
        public ChoiceDialog(string heading, string subheading, string title, List<ChoiceOption> options)
        {
            InitializeComponent();

            Heading = heading;
            SubHeading = subheading;
            labelSubheading.Text = subheading;
            labelHeading.Text = heading;
            Text = title;
            _options = options;
            buttonCancel.Text = Res.Get(StringId.CancelButton);
        }

        private ChoiceOption _selectedItem;
        public ChoiceOption SelectedItem
        {
            get
            {
                return _selectedItem;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set the font size and color for the heading
            labelHeading.Font = Res.GetFont(FontSize.XLarge, FontStyle.Regular);
            labelHeading.ForeColor = BlueText;

            // Layout the heading and subheading
            LayoutHelper.NaturalizeHeightAndDistribute(3, labelHeading, labelSubheading);
            // Move the options panel below the subheading
            panelOptions.Top = labelSubheading.Bottom + (int)DisplayHelper.ScaleY(10);

            int width = 0;
            int height = 0;
            foreach (ChoiceOption co in _options)
            {
                co.AdjustSize();
                width = Math.Max(co.Width, width);
                panelOptions.Controls.Add(co);
                co.Top = height;
                height += co.Size.Height + (int)DisplayHelper.ScaleY(5);
                co.Click += new EventHandler(co_Click);
            }

            // Set the panel width and height
            panelOptions.Width = width + 3;
            panelOptions.Height = height;

            // Set the windows width and height
            int panelHeight = panelOptions.Top + (panelOptions.Top / 2) + panelOptions.Height;
            this.Height = panelHeight + (int)DisplayHelper.ScaleY(12) + buttonCancel.Height + (int)DisplayHelper.ScaleY(12);
            this.Width = panelOptions.Width + panelOptions.Left + (Width - buttonCancel.Right - 1);

            // Update all the options with the max width
            foreach (ChoiceOption co in _options)
            {
                co.Width = width;
            }

            DisplayHelper.AutoFitSystemButton(buttonCancel, buttonCancel.Width, int.MaxValue);
        }

        void co_Click(object sender, EventArgs e)
        {
            _selectedItem = (ChoiceOption)sender;
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool ret = base.ProcessCmdKey(ref msg, keyData);
            Invalidate(true);
            return ret;

        }
    }
}
