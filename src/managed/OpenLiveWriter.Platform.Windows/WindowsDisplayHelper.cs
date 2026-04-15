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
        private float _cachedDpiX;
        private float _cachedDpiY;
        private bool? _compositionEnabled;

        public int DefaultDpi => DEFAULT_DPI;

        private void EnsureDpiCached()
        {
            if (_cachedDpiX == 0)
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    _cachedDpiX = g.DpiX;
                    _cachedDpiY = g.DpiY;
                }
            }
        }

        public float TwipsToPixelsX(int twips)
        {
            EnsureDpiCached();
            return (float)twips * _cachedDpiX / TWIPS_PER_INCH;
        }

        public float TwipsToPixelsY(int twips)
        {
            EnsureDpiCached();
            return (float)twips * _cachedDpiY / TWIPS_PER_INCH;
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
            catch (DllNotFoundException)
            {
                _compositionEnabled = false;
            }

            return _compositionEnabled.Value;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);
    }
}
