// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Imports from uxtheme.dll.  Just enough to get the theme border color for now.
    /// </summary>
    public class Uxtheme
    {
        public static string CLASS_EDIT = "Edit";

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern bool IsAppThemed();

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern Int32 DrawThemeBackground(IntPtr hTheme,
                                                        IntPtr hdc,
                                                        Int32 iPartId,
                                                        Int32 iStateId,
                                                        ref RECT pRect,
                                                        ref RECT pClipRect);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenThemeData(IntPtr hwnd, string classes);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 CloseThemeData(IntPtr htheme);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 GetThemeColor(IntPtr hTheme,
                                                    Int32 partID,
                                                    Int32 stateID,
                                                    Int32 propID,
                                                    out Int32 color);

        /// <summary>
        /// Window Parts.
        /// </summary>
        public struct WP
        {
            public const Int32 CAPTION = 0x00000001;
            public const Int32 FRAMELEFT = 0x00000007;
            public const Int32 FRAMERIGHT = 0x00000008;
            public const Int32 FRAMEBOTTOM = 0x00000009;
        }

        /// <summary>
        /// Caption State.
        /// </summary>
        public struct CS
        {
            public const Int32 ACTIVE = 0x00000001;
            public const Int32 INACTIVE = 0x00000002;
        }

        /// <summary>
        /// Frame State.
        /// </summary>
        public struct FS
        {
            public const Int32 ACTIVE = 0x00000001;
            public const Int32 INACTIVE = 0x00000002;
        }

        /// <summary>
        /// Edit parts.
        /// </summary>
        public struct EP
        {
            public const Int32 EDITTEXT = 0x00000001;
        }

        /// <summary>
        /// EDITTEXT states.
        /// </summary>
        public struct ETS
        {
            public const Int32 NORMAL = 0x00000001;
        }

        /// <summary>
        /// Theme metrics.
        /// </summary>
        public struct TMT
        {
            public const Int32 BORDERCOLOR = 0x00000ED9;
        }
    }
}
