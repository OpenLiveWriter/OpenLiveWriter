// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.Layout
{
    public class GdiTextHelper
    {
        public static int MeasureString(Control context, Font font, string text, int width)
        {
            Rectangle textBounds = new Rectangle(0, 0, width, 0);
            return MeasureString(context, font, text, textBounds).Height;
        }

        public static Rectangle MeasureString(Control context, Font font, string text, Rectangle textBounds)
        {
            IntPtr hdc = User32.GetDC(context.Handle);
            try
            {
                IntPtr hFont = font.ToHfont();
                try
                {
                    IntPtr hPrevFont = Gdi32.SelectObject(hdc, hFont);
                    try
                    {
                        StringBuilder sb = new StringBuilder(text);
                        RECT rect = textBounds;
                        rect.bottom = rect.top; // set height to 0
                        User32.DRAWTEXTPARAMS dtparams = new User32.DRAWTEXTPARAMS();
                        dtparams.cbSize = (uint)Marshal.SizeOf(typeof(User32.DRAWTEXTPARAMS));
                        if (0 == User32.DrawTextEx(hdc, sb, sb.Length, ref rect, User32.DT.CALCRECT | User32.DT.WORDBREAK | User32.DT.END_ELLIPSIS, ref dtparams))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        return rect;
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
            finally
            {
                User32.ReleaseDC(context.Handle, hdc);
            }

        }

        public static void DrawString(Control context, Font font, string text, Rectangle textBounds, bool useMnemonics, GdiTextDrawMode textMode)
        {
            IntPtr hdc = User32.GetDC(context.Handle);
            try
            {
                Gdi32.SetBkColor(hdc, Gdi32.ToColorRef(context.BackColor));
                Gdi32.SetTextColor(hdc, Gdi32.ToColorRef(context.ForeColor));

                try
                {
                    IntPtr hFont = font.ToHfont();
                    try
                    {
                        IntPtr hPrevFont = Gdi32.SelectObject(hdc, hFont);
                        try
                        {
                            StringBuilder sb = new StringBuilder(text);
                            RECT rect = textBounds;
                            User32.DRAWTEXTPARAMS dtparams = new User32.DRAWTEXTPARAMS();
                            dtparams.cbSize = (uint)Marshal.SizeOf(typeof(User32.DRAWTEXTPARAMS));
                            User32.DT flags =
                                textMode == GdiTextDrawMode.EndEllipsis ? User32.DT.END_ELLIPSIS
                                : textMode == GdiTextDrawMode.WordBreak ? User32.DT.WORDBREAK | User32.DT.END_ELLIPSIS
                                : User32.DT.WORDBREAK;
                            if (!useMnemonics)
                                flags |= User32.DT.NOPREFIX;

                            if (0 == User32.DrawTextEx(hdc, sb, sb.Length, ref rect, flags, ref dtparams))
                                throw new Win32Exception(Marshal.GetLastWin32Error());
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
                finally
                {
                }
            }
            finally
            {
                User32.ReleaseDC(context.Handle, hdc);
            }

        }
    }

    public enum GdiTextDrawMode
    {
        WordBreak,
        EndEllipsis
    }
}
