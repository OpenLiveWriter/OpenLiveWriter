using System;
using System.Diagnostics;
using System.Globalization;

namespace OpenLiveWriter.PostEditor.Tables
{
    public enum PixelPercentUnits
    {
        Undefined,
        Pixels,
        Percentage
    }

    /// <summary>
    /// Represents a value that can be a number of pixels or a percentage
    /// </summary>
    [DebuggerDisplay("{Value} {Units}")]
    public struct PixelPercent
    {
        public PixelPercent(int value, PixelPercentUnits units) : this()
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "value must be greater than zero");

            if (value > 100 && units == PixelPercentUnits.Percentage)
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "value when percent must be less or equal to 100");

            SetValues(value.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, units);
        }

        public PixelPercent(string text, IFormatProvider provider, PixelPercentUnits units) : this()
        {
            SetValues(text, provider, units);
        }

        public PixelPercent(string text, IFormatProvider provider) : this(text, provider, PixelPercentUnits.Pixels)
        {
        }

        private void SetValues(string text, IFormatProvider provider, PixelPercentUnits units)
        {
            if (string.IsNullOrEmpty(text))
            {
                Value = 0;
                Units = PixelPercentUnits.Undefined;
            }
            else
            {
                var s = text.Trim();

                if (s.EndsWith("%"))
                {
                    units = PixelPercentUnits.Percentage;
                    s = s.TrimEnd('%');
                }

                int value;
                if (int.TryParse(s, NumberStyles.Integer, provider, out value))
                {
                    Value = value;
                    Units = units;
                }
                else
                {
                    Units = PixelPercentUnits.Undefined;
                    Value = 0;
                }
            }
        }

        public static bool IsAcceptableWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            var s = text.Trim();

            if (s.EndsWith("%"))
            {
                s = s.TrimEnd('%');
            }

            int value;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public int Value { get; private set; }

        public PixelPercentUnits Units { get; private set; }

        public static PixelPercent operator /(PixelPercent left, int right)
        {
            if (right <= 0)
                throw new InvalidOperationException("Can't divide PixelPercent by zero or negative numbers");

            if (left.Units == PixelPercentUnits.Undefined)
                throw new InvalidOperationException("Can't divide PixelPercent as it doesn't have an assigned value");

            return new PixelPercent(left.Value / right, left.Units);
        }

        public static PixelPercent operator *(PixelPercent left, int right)
        {
            throw new NotImplementedException();
        }

        public static implicit operator int(PixelPercent value)
        {
            return value.Value;
        }

        public static implicit operator PixelPercent(int value)
        {
            return new PixelPercent(value, PixelPercentUnits.Pixels);
        }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            switch (Units)
            {
                case PixelPercentUnits.Percentage:
                    return String.Format(provider, "{0}%", Value);
                case PixelPercentUnits.Pixels:
                    return String.Format(provider, "{0}", Value);
                default:
                    return String.Empty;
            }

        }
    }
}