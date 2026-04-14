// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenLiveWriter.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsDisplayHelper : IDisplayHelper
    {
        private const int DEFAULT_DPI = 96;
        private const int TWIPS_PER_INCH = 1440;
        private bool? _compositionEnabled;

        public int DefaultDpi => DEFAULT_DPI;

        public float TwipsToPixelsX(int twips)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return TwipsToPixels(twips, (int)g.DpiX);
            }
        }

        public float TwipsToPixelsY(int twips)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return TwipsToPixels(twips, (int)g.DpiY);
            }
        }

        private static float TwipsToPixels(int twips, int pixelsPerInch)
        {
            return (float)twips * pixelsPerInch / TWIPS_PER_INCH;
        }

        public bool IsCompositionEnabled()
        {
            if (_compositionEnabled.HasValue)
                return _compositionEnabled.Value;

            try
            {
                int result = DwmIsCompositionEnabled(out bool enabled);
                _compositionEnabled = result == 0 && enabled;
            }
            catch
            {
                _compositionEnabled = false;
            }

            return _compositionEnabled.Value;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);
    }
}
