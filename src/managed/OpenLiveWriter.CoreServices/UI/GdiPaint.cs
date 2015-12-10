// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.UI
{
    public class GdiPaint
    {
        public static void BitBlt(Graphics g, IntPtr hObject, Rectangle srcRect, Point destPoint)
        {
            IntPtr pTarget = g.GetHdc();
            try
            {
                IntPtr pSource = Gdi32.CreateCompatibleDC(pTarget);
                try
                {
                    IntPtr pOrig = Gdi32.SelectObject(pSource, hObject);
                    try
                    {
                        Gdi32.BitBlt(pTarget, destPoint.X, destPoint.Y, srcRect.Width, srcRect.Height, pSource, srcRect.X, srcRect.Y,
                                     Gdi32.TernaryRasterOperations.SRCCOPY);
                    }
                    finally
                    {
                        Gdi32.SelectObject(pSource, pOrig);
                    }
                }
                finally
                {
                    Gdi32.DeleteDC(pSource);
                }
            }
            finally
            {
                g.ReleaseHdc(pTarget);
            }

        }
    }
}
