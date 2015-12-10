// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Color helper.
    /// </summary>
    public class ColorHelper
    {
        /// <summary>
        /// Initializes a new instance of the ColorHelper class.
        /// </summary>
        private ColorHelper()
        {
        }

        public static int GetLuminosity(Color c)
        {
            return (c.R + c.G + c.B) / 3;
            //return (Math.Max(c.R, Math.Max(c.G, c.B)) + Math.Min(c.R, Math.Min(c.G, c.B)))/2;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static Color GetThemeBorderColor(Color substituteColor)
        {
            Int32 color = 0;

            // UxTheme is only supported on Windows XP and Later
            if (Environment.OSVersion.Version.Major >= 5 &&
                Environment.OSVersion.Version.Minor >= 1)
            {
                IntPtr hTheme = Uxtheme.OpenThemeData(IntPtr.Zero, "Edit");
                if (hTheme != IntPtr.Zero)
                {
                    Uxtheme.GetThemeColor(hTheme, Uxtheme.EP.EDITTEXT, Uxtheme.ETS.NORMAL, Uxtheme.TMT.BORDERCOLOR, out color);
                    Uxtheme.CloseThemeData(hTheme);
                }
            }
            return color == 0 ? substituteColor : ColorTranslator.FromWin32(color);
        }


        /// <summary>
        /// Adjusts the brightness of the specified color.  Works like the Photoshop Hue/Saturation
        /// dialog box.
        /// </summary>
        /// <param name="color">Color to adjust the brightness of.</param>
        /// <param name="amount">Amount to adjust the brightness.  A negative value decreases the
        /// brightness of the color.  A positive value increases the brightness of the color.</param>
        /// <returns></returns>
        public static Color AdjustBrightness(Color color, double amount)
        {
            //	Sanity check amount.
            Debug.Assert(amount >= -1.0 && amount <= 1.0, "amount parameter is out of range");
            if (amount < -1.0 || amount > 1.0)
                return color;

            //	Convert the color into an AHSB value.
            double alpha, hue, saturation, brightness;
            ColorToAHSB(color, out alpha, out hue, out saturation, out brightness);

            //	Adjust brightness.
            return AHSBToColor(alpha, hue, saturation, Math.Max(Math.Min(brightness + amount, 1.0), 0.0));
        }

        /// <summary>
        /// Adjusts the saturation of the specified color.  Works like the Photoshop Hue/Saturation
        /// dialog box.
        /// </summary>
        /// <param name="color">Color to adjust the saturation of.</param>
        /// <param name="amount">Amount to adjust the saturation.  A negative value decreases the
        /// saturation of the color.  A positive value increases the saturation of the color.</param>
        /// <returns></returns>
        public static Color AdjustSaturation(Color color, double amount)
        {
            //	Sanity check amount.
            Debug.Assert(amount >= -1.0 && amount <= 1.0, "amount parameter is out of range");
            if (amount < -1.0 || amount > 1.0)
                return color;

            //	Convert the color into an AHSB value.
            double alpha, hue, saturation, brightness;
            ColorToAHSB(color, out alpha, out hue, out saturation, out brightness);

            //	Adjust saturation.
            return AHSBToColor(alpha, hue, Math.Max(Math.Min(saturation + amount, 1.0), 0.0), brightness);
        }

        /// <summary>
        /// Adjusts the saturation and brightness of the specified color.  Works like the Photoshop
        /// Hue/Saturation dialog box.
        /// </summary>
        /// <param name="color">Color to adjust the saturation and brightness of.</param>
        /// <param name="saturationDeltaamount">Amount to adjust the saturation.  A negative value decreases the
        /// saturation of the color.  A positive value increases the saturation of the color.</param>
        /// <returns></returns>
        public static Color AdjustSaturationAndBrightness(Color color, double saturationDelta, double brightnessDelta)
        {
            //	Sanity check amounts.
            Debug.Assert(saturationDelta >= -1.0 && saturationDelta <= 1.0, "saturationDelta parameter is out of range");
            if (saturationDelta < -1.0 || saturationDelta > 1.0)
                return color;
            Debug.Assert(brightnessDelta >= -1.0 && brightnessDelta <= 1.0, "brightnessDelta parameter is out of range");
            if (brightnessDelta < -1.0 || brightnessDelta > 1.0)
                return color;

            //	Convert the color into an AHSB value.
            double alpha, hue, saturation, brightness;
            ColorToAHSB(color, out alpha, out hue, out saturation, out brightness);

            //	Adjust saturation and brightness.
            return AHSBToColor(alpha,
                                hue,
                                Math.Max(Math.Min(saturation + saturationDelta, 1.0), 0.0),
                                Math.Max(Math.Min(brightness + brightnessDelta, 1.0), 0.0));
        }

        /// <summary>
        /// Converts color name or an HTML-like RGB hex string to a .NET Color structure.
        /// </summary>
        /// <param name="colorStr">A hex RGB string (in HEX format: #RRGGBB or #RGB) or well-known color name</param>
        /// <returns></returns>
        public static Color StringToColor(string colorStr)
        {
            Color color;
            if (colorStr.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                if (colorStr.Length == 7)
                {
                    string hexRed = colorStr.Substring(1, 2);
                    string hexGreen = colorStr.Substring(3, 2);
                    string hexBlue = colorStr.Substring(5);

                    int red = Convert.ToInt32(hexRed, 16);
                    int green = Convert.ToInt32(hexGreen, 16);
                    int blue = Convert.ToInt32(hexBlue, 16);
                    color = Color.FromArgb(red, green, blue);
                }
                else if (colorStr.Length == 4)
                {
                    int red = Convert.ToInt32(colorStr[1].ToString(), 16);
                    int green = Convert.ToInt32(colorStr[2].ToString(), 16);
                    int blue = Convert.ToInt32(colorStr[3].ToString(), 16);
                    color = Color.FromArgb(red, green, blue);
                }
                else
                {
                    Trace.Fail("Failed to convert string to color: " + colorStr);
                    color = Color.Black;
                }
            }
            else
            {
                color = Color.FromName(colorStr);
            }
            return color;
        }

        /// <summary>
        /// Convert a Color structure to an RGB string (in HEX format: #RRGGBB)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToString(Color color)
        {
            return String.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        /// <summary>
        /// Color space conversion from a .NET Color structure to AHSB.
        /// </summary>
        /// <param name="color">Color to convert.</param>
        /// <param name="alpha">Alpha.</param>
        /// <param name="hue">Hue.</param>
        /// <param name="saturation">Saturation.</param>
        /// <param name="brightness">Brightness.</param>
        public static void ColorToAHSB(Color color, out double alpha, out double hue, out double saturation, out double brightness)
        {
            //	Alpha.
            alpha = ((double)color.A) / 255.0;

            //	Red, green and blue.
            double red = ((double)color.R) / 255.0;
            double green = ((double)color.G) / 255.0;
            double blue = ((double)color.B) / 255.0;

            //	Compute the max and minimum values, and the delta between them.
            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;

            //	Brightness is the maximum value.
            brightness = max;

            //	Saturation is (max(red, green, blue)-min(red, green, blue))/max(red, green, blue).
            saturation = (max == 0.0) ? 0.0 : delta / max;

            //	Compute hue.
            if (saturation == 0)
                hue = 0;
            else if (red == max)
                hue = (green - blue) / delta;
            else if (green == max)
                hue = 2.0 + (blue - red) / delta;
            else
                hue = 4.0 + (red - green) / delta;
            hue *= 60.0;
            if (hue < 0.0)
                hue += 360.0;
        }

        /// <summary>
        /// Color space conversion from AHSB to a .NET Color structure.
        /// </summary>
        /// <param name="alpha">Alpha component.</param>
        /// <param name="hue">Hue.</param>
        /// <param name="saturation">Saturation.</param>
        /// <param name="brightness">Brightness.</param>
        /// <returns>Color structure..</returns>
        public static Color AHSBToColor(double alpha, double hue, double saturation, double brightness)
        {
            //	If the saturation is 0.0, then the color is greyscale and a shortcut can be taken.
            if (saturation == 0.0)
            {
                int value = (int)(brightness * 255.0);
                return Color.FromArgb((int)(alpha * 255.0), value, value, value);
            }

            //	A hue of 0.0 and 360.0 is the same.  It is red.
            if (hue == 360.0)
                hue = 0;

            //	Recalculate hue as an index into the arcs of the color circle.
            int arc = (int)Math.Floor(hue / 60.0);

            //	Compute the fractional component of hue (position of the color inside the arc).
            double fraction = hue - Math.Floor(hue);

            int a = (int)(alpha * 255.0);

            //	Compute hue.
            int p = (int)((brightness * (1.0 - saturation)) * 255.0);
            int q = (int)((brightness * (1.0 - saturation * fraction)) * 255.0);
            int t = (int)((brightness * (1.0 - saturation * (1.0 - fraction))) * 255.0);
            int v = (int)(brightness * 255.0);
            switch (arc)
            {
                case 0:
                    return Color.FromArgb(a, v, t, p);
                case 1:
                    return Color.FromArgb(a, q, v, p);
                case 2:
                    return Color.FromArgb(a, p, v, t);
                case 3:
                    return Color.FromArgb(a, p, q, v);
                case 4:
                    return Color.FromArgb(a, t, p, v);
                default:
                    return Color.FromArgb(a, v, p, q);
            }
        }

        /// <summary>
        /// Convert MSHTML color to DotNet color
        /// </summary>
        /// <param name="colorValue">mshtml color value</param>
        /// <returns>DotNet color</returns>
        public static Color BGRToColor(int colorValue)
        {
            // extract RGB values from colorValue
            int red = colorValue & 0xFF;
            int green = (colorValue & 0xFF00) >> 8;
            int blue = (colorValue & 0xFF0000) >> 16;

            // return .NET color
            return Color.FromArgb(red, green, blue);
        }

        /// <summary>
        /// Convert DotNet color to Mshtml Color
        /// </summary>
        /// <param name="colorValue">DotNet color</param>
        /// <returns>mshtml color value</returns>
        public static int ColorToBGR(Color color)
        {
            // build colorValue mask from .NET color
            int colorValue = 0;
            colorValue |= color.R;
            colorValue |= ((int)color.G) << 8;
            colorValue |= ((int)color.B) << 16;

            // return color
            return colorValue;
        }

        public static Image CreateColorImage(Color color, int width, int height)
        {
            Image img = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(img))
            {
                using (Brush b = new SolidBrush(color))
                    g.FillRectangle(b, 0, 0, width, height);
            }
            return img;
        }

        public static string ColorsToString(Color[] colors)
        {
            string[] strColors = new string[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                strColors[i] = ColorToString(colors[i]);
            }
            return StringHelper.Join(strColors, ",");
        }

        public static Color[] StringToColors(string strColors)
        {
            string[] astrColors = StringHelper.Split(strColors, ",");
            Color[] colors = new Color[astrColors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = StringToColor(astrColors[i]);
            }
            return colors;
        }

        public static Color Average(Color color1, Color color2)
        {
            return Color.FromArgb(
                (color1.A + color2.A) / 2,
                (color1.R + color2.R) / 2,
                (color1.G + color2.G) / 2,
                (color1.B + color2.B) / 2
                );
        }

        public static int ColorToInt(Color color)
        {
            return (color.R) | (color.G << 8) | (color.B << 16);
        }
    }

    /// <summary>
    /// Convenient wrapper for combining high-contrast color choice
    /// with the regular color choice.
    /// </summary>
    public struct HCColor
    {
        private readonly Color fullColor;
        private readonly Color highContrastColor;

        public HCColor(Color fullColor, Color highContrastColor)
        {
            this.fullColor = fullColor;
            this.highContrastColor = highContrastColor;
        }

        public HCColor(int r, int g, int b, Color highContrastColor)
        {
            this.fullColor = Color.FromArgb(r, g, b);
            this.highContrastColor = highContrastColor;
        }

        public HCColor(int rgb, Color highContrastcolor)
        {
            this.fullColor = Color.FromArgb(
                (rgb & 0xFF0000) >> 16,
                (rgb & 0x00FF00) >> 8,
                (rgb & 0x0000FF));
            this.highContrastColor = highContrastcolor;
        }

        public static implicit operator Color(HCColor color)
        {
            if (SystemInformation.HighContrast)
                return color.highContrastColor;
            else
                return color.fullColor;
        }
    }
}
