// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.Localization
{
    /// <summary>
    /// Windows-specific CultureHelper methods that require WinForms or P/Invoke.
    /// These were extracted from CultureHelper to allow the Localization project
    /// to target net10.0 (cross-platform).
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class CultureHelperWindows
    {
        public static void FixupTextboxForNumber(TextBox textBox)
        {
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant() == "HE")
            {
                textBox.RightToLeft = RightToLeft.No;
                textBox.TextAlign = HorizontalAlignment.Right;
            }
        }

        [Obsolete("NOT FULLY TESTED")]
        public static bool IsImeActive(IntPtr windowHandle)
        {
            bool isActive = false;

            try
            {
                IntPtr handle = Imm32.ImmGetContext(windowHandle);

                if (handle == IntPtr.Zero)
                    return false;

                try
                {
                    isActive = Imm32.ImmGetOpenStatus(handle);
                }
                finally
                {
                    Imm32.ImmReleaseContext(windowHandle, handle);
                }

                return isActive;
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to check if IME is active: " + ex);
                return isActive;
            }
        }

        [Obsolete("NOT FULLY TESTED")]
        public static class Imm32
        {
            [DllImport("imm32.dll")]
            public static extern IntPtr ImmGetContext(IntPtr hWnd);

            [DllImport("imm32.dll")]
            public static extern bool ImmGetOpenStatus(IntPtr hIMC);

            [DllImport("imm32.dll")]
            public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        }
    }
}
