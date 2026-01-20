// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Bitmap operation helper.
    /// </summary>
    public class BitmapHelper
    {
        /// <summary>
        /// Initializes a new instance of the BitmapHelper class.
        /// </summary>
        private BitmapHelper()
        {
        }

        /// <summary>
        /// Converts the input bitmap into a grayscale bitmap.  The input bitmap is not modified.
        /// </summary>
        /// <param name="input">Input bitmap.</param>
        /// <returns>Grayscale version of the input bitmap.</returns>
        public static Bitmap ToGrayscale(Bitmap input)
        {
            //	Create the output bitmap.
            Bitmap output = new Bitmap(input.Width, input.Height);

            //	Convert the pixels.
            for (int x = 0; x < output.Width; x++)
                for (int y = 0; y < output.Height; y++)
                {
                    //	Get the input pixel color.
                    Color color = input.GetPixel(x, y);

                    //	Compute the grayscale value as the average of the RGB values.
                    byte grayscale = (byte)(((int)color.R + (int)color.G + (int)color.B)/3);

                    //	Set the output pixel.
                    output.SetPixel(x, y, Color.FromArgb(color.A, grayscale, grayscale, grayscale));
                }

            //	Done.  Return the output bitmap.
            return output;
        }
    }
}
