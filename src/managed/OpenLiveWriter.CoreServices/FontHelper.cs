// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for FontHelper.
    /// </summary>
    public sealed class FontHelper
    {
        private FontHelper()
        {
        }

        /// <summary>
        /// Measures and returns the width of a space character.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="font">Font to perform the measurement with.</param>
        /// <returns></returns>
        public static int WidthOfSpace(Graphics graphics, Font font)
        {
            return graphics.MeasureString(" ", font).ToSize().Width;
        }
    }
}
