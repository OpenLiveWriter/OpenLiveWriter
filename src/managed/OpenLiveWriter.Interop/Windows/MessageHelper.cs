// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Windows message parameter conversion methods.
    /// </summary>
    public class MessageHelper
    {
        /// <summary>
        ///	HIWORD retrieves the high-order word from the given 32-bit value.
        /// </summary>
        /// <param name="value">Specifies the value to be converted.</param>
        /// <returns>The return value is the high-order word of the specified value.</returns>
        public static int HIWORDToInt32(IntPtr value)
        {
            return (int)(short)(ushort)(((uint)value.ToInt32() & 0xFFFF0000) >> 16);
        }

        /// <summary>
        /// LOWORD retrieves the low-order word from the specified value.
        /// </summary>
        /// <param name="value">Specifies the value to be converted.</param>
        /// <returns>The return value is the low-order word of the specified value.</returns>
        public static int LOWORDToInt32(IntPtr value)
        {
            return (int)(short)(ushort)((uint)value.ToInt32() & 0x0000FFFF);
        }

        /// <summary>
        /// MAKELONG creates a LONG value by concatenating the specified values.
        /// </summary>
        /// <param name="loword">Specifies the low-order word of the new value.</param>
        /// <param name="hiword">Specifies the high-order word of the new value.</param>
        /// <returns>The return value is a LONG value.</returns>
        public static IntPtr MAKELONG(int loword, int hiword)
        {
            uint lo = (uint)(ushort)(short)loword;
            uint hi = (uint)(ushort)(short)hiword;
            return new IntPtr((int)((hi << 16) | lo));
        }
    }
}
