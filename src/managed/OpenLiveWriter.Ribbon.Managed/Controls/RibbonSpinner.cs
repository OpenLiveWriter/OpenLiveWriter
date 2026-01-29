// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Ribbon spinner (numeric up/down) control.
    /// </summary>
    public class RibbonSpinner : RibbonControlBase
    {
        private readonly NumericUpDown _innerSpinner;
        private readonly Label _labelControl;
        private string _label;
        private int _minValue = 0;
        private int _maxValue = 100;
        private int _increment = 1;
        private string _formatString = "{0}";
        private string _representativeString = "9999";

        /// <summary>
        /// Gets or sets the label displayed above the spinner.
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                if (_labelControl != null)
                {
                    _labelControl.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public int MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                _innerSpinner.Minimum = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                _innerSpinner.Maximum = value;
            }
        }

        /// <summary>
        /// Gets or sets the increment amount.
        /// </summary>
        public int Increment
        {
            get => _increment;
            set
            {
                _increment = value;
                _innerSpinner.Increment = value;
            }
        }

        /// <summary>
        /// Gets or sets the format string for displaying values.
        /// </summary>
        public string FormatString
        {
            get => _formatString;
            set => _formatString = value ?? "{0}";
        }

        /// <summary>
        /// Gets or sets the representative string for sizing.
        /// </summary>
        public string RepresentativeString
        {
            get => _representativeString;
            set
            {
                _representativeString = value;
                UpdateSize();
            }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public decimal Value
        {
            get => _innerSpinner.Value;
            set
            {
                value = Math.Max(_minValue, Math.Min(_maxValue, value));
                _innerSpinner.Value = value;
            }
        }

        /// <summary>
        /// Occurs when the value changes.
        /// </summary>
        public event EventHandler ValueChanged;

        public RibbonSpinner()
        {
            Size = new Size(70, 44);

            // Label
            _labelControl = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 14),
                Font = new Font(SystemFonts.MenuFont.FontFamily, 7.5f),
                ForeColor = RibbonColors.Current.GroupLabelText,
                TextAlign = ContentAlignment.BottomLeft
            };
            Controls.Add(_labelControl);

            // NumericUpDown
            _innerSpinner = new NumericUpDown
            {
                Location = new Point(0, 16),
                Size = new Size(Width, 23),
                Font = SystemFonts.MenuFont,
                Minimum = _minValue,
                Maximum = _maxValue,
                Increment = _increment,
                TextAlign = HorizontalAlignment.Right
            };

            _innerSpinner.ValueChanged += (s, e) =>
            {
                ValueChanged?.Invoke(this, e);
                ExecuteCommand();
            };

            Controls.Add(_innerSpinner);
        }

        /// <summary>
        /// Override to fill entire bounds before child controls render.
        /// This prevents black showing through gaps between label and spinner.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Fill entire bounds with opaque background to prevent black in gaps
            e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
            base.OnPaint(e);
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            // Calculate width based on representative string
            int width;
            using (var g = CreateGraphics())
            {
                var size = g.MeasureString(_representativeString, SystemFonts.MenuFont);
                width = (int)size.Width + 30; // Extra space for spinner buttons
            }

            switch (CurrentSize)
            {
                case RibbonGroupSize.Large:
                case RibbonGroupSize.Medium:
                    Size = new Size(width, 44);
                    _labelControl.Visible = true;
                    _innerSpinner.Location = new Point(0, 16);
                    break;
                case RibbonGroupSize.Small:
                    Size = new Size(width, 24);
                    _labelControl.Visible = false;
                    _innerSpinner.Location = new Point(0, 0);
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_labelControl != null)
            {
                _labelControl.Width = Width;
            }

            if (_innerSpinner != null)
            {
                _innerSpinner.Width = Width;
            }
        }

        protected override void UpdateFromCommand()
        {
            base.UpdateFromCommand();

            if (_innerSpinner != null)
            {
                _innerSpinner.Enabled = CommandEnabled;
            }
        }
    }
}
