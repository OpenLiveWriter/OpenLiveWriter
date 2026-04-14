// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{

    [SupportedOSPlatform("windows")]
    public sealed class DisplayHelper
    {
        const int DEFAULT_DPI = 96;
        const int TWIPS_PER_INCH = 1440;

        public static float TwipsToPixelsX(int twips)
        {
            return TwipsToPixels(twips, PixelsPerLogicalInchX);
        }

        public static float TwipsToPixelsY(int twips)
        {
            return TwipsToPixels(twips, PixelsPerLogicalInchY);
        }

        public static float TwipsToPixels(int twips, int pixelsPerInch)
        {
            if (twips < 0)
            {
                throw new ArgumentOutOfRangeException("twips");
            }

            if (pixelsPerInch <= 0)
            {
                throw new ArgumentOutOfRangeException("pixelsPerInch");
            }

            return (pixelsPerInch * twips) / (float)TWIPS_PER_INCH;
        }

        public static bool IsCompositionEnabled(bool flushCachedValue)
        {
            return DwmHelper.GetCompositionEnabled(flushCachedValue);
        }

        private static int? _pixelsPerLogicalInchX;
        public static int PixelsPerLogicalInchX
        {
            get
            {
                if (_pixelsPerLogicalInchX == null)
                {
                    IntPtr hWndDesktop = User32.GetDesktopWindow();
                    IntPtr hDCDesktop = User32.GetDC(hWndDesktop);
                    _pixelsPerLogicalInchX = Gdi32.GetDeviceCaps(hDCDesktop, DEVICECAPS.LOGPIXELSX);
                    User32.ReleaseDC(hWndDesktop, hDCDesktop);
                }
                return _pixelsPerLogicalInchX.Value;
            }
        }

        public static int? _pixelsPerLogicalInchY;
        public static int PixelsPerLogicalInchY
        {
            get
            {
                if (_pixelsPerLogicalInchY == null)
                {
                    IntPtr hWndDesktop = User32.GetDesktopWindow();
                    IntPtr hDCDesktop = User32.GetDC(hWndDesktop);
                    _pixelsPerLogicalInchY = Gdi32.GetDeviceCaps(hDCDesktop, DEVICECAPS.LOGPIXELSY);
                    User32.ReleaseDC(hWndDesktop, hDCDesktop);
                }
                return _pixelsPerLogicalInchY.Value;
            }
        }

        public static float ScalingFactorX
        {
            get
            {
                return (float)PixelsPerLogicalInchX / (float)DEFAULT_DPI;
            }
        }

        public static float ScalingFactorY
        {
            get
            {
                return (float)PixelsPerLogicalInchY / (float)DEFAULT_DPI;
            }
        }

        public static SizeF ScalingFactor
        {
            get
            {
                return new SizeF(ScalingFactorX, ScalingFactorY);
            }
        }

        public static float ScaleX(float x) => x * ScalingFactorX;
        public static float ScaleY(float y) => y * ScalingFactorY;

        public static int ScaleXCeil(float x) => (int)Math.Ceiling(x * ScalingFactorX);
        public static int ScaleYCeil(float y) => (int)Math.Ceiling(y * ScalingFactorY);

        /// <summary>
        /// Scales a control from 96dpi to the actual screen dpi.
        /// </summary>
        public static void Scale(Control c)
        {
            c.Scale(ScalingFactor);
        }

        /// <summary>
        /// Scales a bitmap from 96dpi to the actual screen dpi.
        /// </summary>
        public static Bitmap ScaleBitmap(Bitmap original)
        {
            return new Bitmap(original, ScaleSize(original.Size));
        }

        /// <summary>
        /// Scales up a 96-dpi Size object to the actual screen DPI.
        /// </summary>
        public static Size ScaleSize(Size original)
        {
            return new Size(ScaleXCeil(original.Width), ScaleYCeil(original.Height));
        }

        /// <summary>
        /// Scales down a screen-dpi size to 96-dpi.
        /// </summary>
        public static Size UnscaleSize(Size original)
        {
            return new Size(
                (int)Math.Ceiling(original.Width / ScalingFactorX),
                (int)Math.Ceiling(original.Height / ScalingFactorY)
                );
        }

        /// <summary>
        /// When PMingLiU is rendered by GDI+ with StringFormat.LineAlignment == StringAlignment.Center
        /// with at least one Chinese character, it ends up 2-3 pixels higher than it should be.
        /// I couldn't find a better fix than to just move it down a couple of pixels.
        ///
        /// Note that the stringFormat and textRect arguments will both be mutated!
        /// </summary>
        public static void FixupGdiPlusLineCentering(Graphics g, Font font, string text, ref StringFormat stringFormat, ref Rectangle textRect)
        {
            if (CultureHelper.GdiPlusLineCenteringBroken && stringFormat.LineAlignment == StringAlignment.Center)
            {
                // only fixup if double-byte chars exist
                bool hasDoubleByte = false;
                foreach (char c in text)
                    if (c >= 0xFF)
                        hasDoubleByte = true;
                if (!hasDoubleByte)
                    return;

                stringFormat.FormatFlags |= StringFormatFlags.NoClip;
                textRect.Offset(0, (int)ScaleY(2));
            }
        }

        public static Size MeasureStringGDI(IntPtr hdc, string str, Font font)
        {
            IntPtr hFont = font.ToHfont();
            try
            {
                IntPtr hPrevFont = Gdi32.SelectObject(hdc, hFont);
                try
                {
                    SIZE size;
                    if (!Gdi32.GetTextExtentPoint32(hdc, str, str.Length, out size))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    POINT p;
                    p.x = size.cx;
                    p.y = size.cy;
                    if (!Gdi32.LPtoDP(hdc, new POINT[] { p }, 1))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    return new Size((int)Math.Ceiling(ScaleX(p.x)), (int)Math.Ceiling(ScaleY(p.y)));
                }
                finally
                {
                    Gdi32.SelectObject(hdc, hPrevFont);
                }
            }
            finally
            {
                Gdi32.DeleteObject(hFont);
            }
        }

        public static int AutoFitSystemButton(Button button)
        {
            return AutoFitSystemButton(button, 0, int.MaxValue);
        }

        public static int MeasureButton(Button button)
        {
            return MeasureString(button, button.Text).Width + MakeEvenInt(ScaleX(10));
        }

        public static int GetMaxDesiredButtonWidth(bool visibleOnly, params Button[] buttons)
        {
            int max = 0;
            foreach (Button button in buttons)
            {
                if (!visibleOnly || button.Visible)
                    max = Math.Max(max, MeasureButton(button));
            }
            return max;
        }

        public static int AutoFitSystemButton(Button button, int minWidth, int maxWidth)
        {
            Debug.Assert(button.FlatStyle == FlatStyle.System, "AutoFitSystemButton only works on buttons with FlatStyle.System; " + button.Name + " is " + button.FlatStyle);
            return FitText(button, button.Text, MakeEvenInt(ScaleX(10)), minWidth, maxWidth);
        }

        public static int AutoFitSystemRadioButton(RadioButton button, int minWidth, int maxWidth)
        {
            Debug.Assert(button.FlatStyle == FlatStyle.System, "AutoFitSystemRadioButton only works on radio buttons with FlatStyle.System; " + button.Name + " is " + button.FlatStyle);
            return FitText(button, button.Text, (int)Math.Ceiling(ScaleX(22)), minWidth, maxWidth);
        }

        public static int AutoFitSystemCheckBox(CheckBox button, int minWidth, int maxWidth)
        {
            Debug.Assert(button.FlatStyle == FlatStyle.System, "AutoFitSystemCheckBox only works on check boxes with FlatStyle.System; " + button.Name + " is " + button.FlatStyle);
            return FitText(button, button.Text, (int)Math.Ceiling(ScaleX(22)), minWidth, maxWidth);
        }

        public static int AutoFitSystemLabel(Label label, int minWidth, int maxWidth)
        {
            Debug.Assert(label.FlatStyle == FlatStyle.System, "AutoFitSystemLabel only works on labels with FlatStyle.System; " + label.Name + " is " + label.FlatStyle);
            return FitText(label, GetMeasurableText(label), 0, minWidth, maxWidth);
        }

        public static int AutoFitSystemCombo(ComboBox comboBox, int minWidth, int maxWidth, int indexToFit)
        {
            if (indexToFit >= 0 && indexToFit < comboBox.Items.Count)
                return SizeComboTo(comboBox, minWidth, maxWidth, MeasureString(comboBox, comboBox.Items[indexToFit].ToString()).Width);
            else
                return AutoFitSystemCombo(comboBox, minWidth, maxWidth, true);
        }

        public static int AutoFitSystemCombo(ComboBox comboBox, int minWidth, int maxWidth, string stringToFit)
        {
            return SizeComboTo(comboBox, minWidth, maxWidth, MeasureString(comboBox, stringToFit).Width);
        }

        public static int AutoFitSystemCombo(ComboBox comboBox, int minWidth, int maxWidth, bool fitCurrentValueOnly)
        {
            int width = GetPreferredWidth(comboBox, fitCurrentValueOnly);
            return SizeComboTo(comboBox, minWidth, maxWidth, width);
        }

        private static int GetPreferredWidth(ComboBox comboBox, bool fitCurrentValueOnly)
        {
            int width = MeasureString(comboBox, comboBox.Text).Width;
            if (!fitCurrentValueOnly)
            {
                foreach (object o in comboBox.Items)
                {
                    if (o != null)
                        width = Math.Max(width, MeasureString(comboBox, o.ToString()).Width);
                }
            }
            return width;
        }

        public static int AutoFitSystemComboDropDown(ComboBox comboBox)
        {
            int width = GetPreferredWidth(comboBox, false);
            comboBox.DropDownWidth = (width);
            return comboBox.DropDownWidth;
        }

        private static int SizeComboTo(ComboBox comboBox, int minWidth, int maxWidth, int width)
        {
            int origWidth = comboBox.Width;
            int hpadding = 34;
            if (comboBox.DrawMode == DrawMode.Normal)
                hpadding -= 3;
            comboBox.Width = Math.Max(minWidth,
                                      Math.Min(maxWidth,
                                               width + (int)Math.Ceiling(ScaleX(hpadding))));
            int deltaX = comboBox.Width - origWidth;
            if ((comboBox.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) == AnchorStyles.Right)
            {
                comboBox.Left -= deltaX;
            }
            return comboBox.Width;
        }

        private static int FitText(Control c, string text, int padding, int minWidth, int maxWidth)
        {
            IntPtr hdc = User32.GetDC(c.Handle);
            try
            {
                int buttonWidth = c.Width;
                int newButtonWidth = c.Width =
                    Math.Min(maxWidth, Math.Max(minWidth,
                    MeasureString(c, text).Width + padding));
                if ((c.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) == AnchorStyles.Right)
                {
                    c.Left += buttonWidth - newButtonWidth;
                }
                return newButtonWidth;
            }
            finally
            {
                User32.ReleaseDC(c.Handle, hdc);
            }
        }

        public static Size MeasureString(Control c, string text)
        {
            return TextRenderer.MeasureText(text, c.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoClipping | TextFormatFlags.PreserveGraphicsClipping);
        }

        private static string GetMeasurableText(Label c)
        {
            if (!c.UseMnemonic || c.Text.IndexOf('&') < 0)
                return c.Text;
            else
            {
                StringBuilder sb = new StringBuilder(c.Text);
                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == '&')
                    {
                        if (sb.Length == i + 1)
                            continue;
                        if (sb[i + 1] == '&')
                        {
                            i++;
                            continue;
                        }

                        sb.Remove(i, 1);
                        i--;
                    }
                }
                return sb.ToString();
            }
        }

        private static int MakeEvenInt(float floatValue)
        {
            int intValue;
            if ((intValue = (int)Math.Ceiling(floatValue)) % 2 == 0)
                return intValue;
            if ((intValue = (int)Math.Floor(floatValue)) % 2 == 0)
                return intValue;
            return (int)floatValue + 1;
        }
    }

    internal class DwmHelper
    {
        private enum State
        {
            None,
            LibraryNotFound,
            LibraryFound
        }

        private static readonly object lockObj = new object();
        private static State state;
        private static bool isCompositionEnabled;

        static DwmHelper()
        {
            Refresh();
        }

        static void Refresh()
        {
            lock (lockObj)
            {
                if (state == State.None)
                {
                    if (Environment.OSVersion.Version.Major < 6)
                        state = State.LibraryNotFound;
                    state = Kernel32.LoadLibrary("dwmapi.dll") != IntPtr.Zero ? State.LibraryFound : State.LibraryNotFound;
                }

                if (state == State.LibraryNotFound)
                    isCompositionEnabled = false;
                else
                {
                    try
                    {
                        bool tmp;
                        if (0 == DwmIsCompositionEnabled(out tmp))
                            isCompositionEnabled = tmp;
                        else
                            isCompositionEnabled = false;
                        return;
                    }
                    catch (Exception e)
                    {
                        isCompositionEnabled = false;
                        Debug.Fail(e.ToString());
                    }
                }
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool isEnabled);

        public static bool GetCompositionEnabled(bool flush)
        {
            if (flush)
                Refresh();
            return isCompositionEnabled;
        }
    }
}
