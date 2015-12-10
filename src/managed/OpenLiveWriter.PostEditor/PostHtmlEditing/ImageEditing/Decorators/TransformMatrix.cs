// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{

    public class TransformMatrix
    {
        private const int RED_OFFSET = 2;
        private const int GREEN_OFFSET = 1;
        private const int BLUE_OFFSET = 0;
        private int TopLeft, TopMid, TopRight;
        private int MidLeft, Pixel, MidRight;
        private int BottomLeft, BottomMid, BottomRight;
        private int Factor, Offset;

        //standard identity matrix values
        public TransformMatrix()
        {
            TopLeft = TopMid = TopRight = MidLeft = 0;
            Pixel = 1;
            MidRight = BottomLeft = BottomMid = BottomRight = 0;
            Factor = 1;
            Offset = 0;
        }

        public TransformMatrix(int topLeft, int topMid, int topRight, int midLeft, int pixel, int midRight, int bottomLeft, int bottomMid, int bottomRight, int factor, int offset)
        {
            if (factor == 0)
                throw new ArgumentException("Factor cannot be zero", "factor");

            TopLeft = topLeft;
            TopMid = topMid;
            TopRight = topRight;
            MidLeft = midLeft;
            Pixel = pixel;
            MidRight = midRight;
            BottomLeft = bottomLeft;
            BottomMid = bottomMid;
            BottomRight = bottomRight;
            Factor = factor;
            Offset = offset;
        }

        public TransformMatrix(int corner, int edge, int middle, int factor, int offset)
        {
            if (factor == 0)
                throw new ArgumentException("Factor cannot be zero", "factor");

            TopLeft = TopRight = BottomLeft = BottomRight = corner;
            TopMid = MidLeft = MidRight = BottomMid = edge;
            Pixel = middle;
            Factor = factor;
            Offset = offset;
        }

        public Bitmap Conv3x3(Bitmap image)
        {
            //check for error condition--will lead to divide by 0
            if (0 == Factor)
                throw new InvalidOperationException("Factor cannot be zero");

            // NOTE: GDI returns BGR, NOT RGB.
            //make image with 1 pixel border
            using (Bitmap source = new Bitmap(image.Width + 2, image.Height + 2))
            {
                using (Graphics g = Graphics.FromImage(source))
                {
                    g.Clear(Color.White);
                    g.DrawImage(image, new Rectangle(1, 1, image.Width, image.Height), 0, 0, image.Width, image.Height,
                                GraphicsUnit.Pixel);
                }
                //make edges duplicates
                for (int x = 1; x < source.Width - 1; x++)
                {
                    source.SetPixel(x, 0, source.GetPixel(x, 1));
                    source.SetPixel(x, source.Height - 1, source.GetPixel(x, source.Height - 2));
                }
                for (int y = 1; y < source.Height - 1; y++)
                {
                    source.SetPixel(0, y, source.GetPixel(1, y));
                    source.SetPixel(source.Width - 1, y, source.GetPixel(source.Width - 2, y));
                }

                Bitmap transformed = new Bitmap(image.Width, image.Height);
                // NOTE: only getting RGB values. Alpha is ignored/disposed and will not be in transformed image
                BitmapData bmTransformed = transformed.LockBits(new Rectangle(0, 0, transformed.Width, transformed.Height),
                                                          ImageLockMode.WriteOnly,
                                                          PixelFormat.Format24bppRgb);
                try
                {
                    BitmapData bmSource = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                                                          ImageLockMode.ReadOnly,
                                                          PixelFormat.Format24bppRgb);
                    try
                    {
                        int scanWidth = bmSource.Stride;
                        int scanWidthDouble = scanWidth * 2;
                        int scanWidthNew = bmTransformed.Stride;

                        System.IntPtr Scan0 = bmTransformed.Scan0;
                        System.IntPtr SrcScan0 = bmSource.Scan0;

                        unsafe
                        {
                            //note: the pointers are at the upper left hand corner pixel, not the middle (target) pixel
                            // for the source image (the copy) and at the actual pixel for the original (transformed)
                            const int bytesPerPixel = 3;

                            int newPixel;

                            for (int y = 0; y < transformed.Height; y++)
                            {
                                byte* ptrNewImage = (byte*)Scan0 + (y * scanWidthNew);
                                byte* ptrSource = (byte*)SrcScan0 + (y * scanWidth);

                                for (int x = 0; x < transformed.Width; x++)
                                {
                                    int currentPixel = x * bytesPerPixel;

                                    //calculate the new red value
                                    newPixel = ((((ptrSource[2] * TopLeft) +
                                                  (ptrSource[5] * TopMid) +
                                                  (ptrSource[8] * TopRight) +
                                                  (ptrSource[2 + scanWidth] * MidLeft) +
                                                  (ptrSource[5 + scanWidth] * Pixel) +
                                                  (ptrSource[8 + scanWidth] * MidRight) +
                                                  (ptrSource[2 + scanWidthDouble] * BottomLeft) +
                                                  (ptrSource[5 + scanWidthDouble] * BottomMid) +
                                                  (ptrSource[8 + scanWidthDouble] * BottomRight))
                                                 / Factor) + Offset);

                                    if (newPixel < 0) newPixel = 0;
                                    if (newPixel > 255) newPixel = 255;
                                    ptrNewImage[currentPixel + RED_OFFSET] = (byte)newPixel;

                                    //calculate the new green value
                                    newPixel = ((((ptrSource[1] * TopLeft) +
                                                  (ptrSource[4] * TopMid) +
                                                  (ptrSource[7] * TopRight) +
                                                  (ptrSource[1 + scanWidth] * MidLeft) +
                                                  (ptrSource[4 + scanWidth] * Pixel) +
                                                  (ptrSource[7 + scanWidth] * MidRight) +
                                                  (ptrSource[1 + scanWidthDouble] * BottomLeft) +
                                                  (ptrSource[4 + scanWidthDouble] * BottomMid) +
                                                  (ptrSource[7 + scanWidthDouble] * BottomRight))
                                                 / Factor) + Offset);

                                    if (newPixel < 0) newPixel = 0;
                                    if (newPixel > 255) newPixel = 255;
                                    ptrNewImage[currentPixel + GREEN_OFFSET] = (byte)newPixel;

                                    //calculate the new blue value
                                    newPixel = ((((ptrSource[0] * TopLeft) +
                                                  (ptrSource[3] * TopMid) +
                                                  (ptrSource[6] * TopRight) +
                                                  (ptrSource[0 + scanWidth] * MidLeft) +
                                                  (ptrSource[3 + scanWidth] * Pixel) +
                                                  (ptrSource[6 + scanWidth] * MidRight) +
                                                  (ptrSource[0 + scanWidthDouble] * BottomLeft) +
                                                  (ptrSource[3 + scanWidthDouble] * BottomMid) +
                                                  (ptrSource[6 + scanWidthDouble] * BottomRight))
                                                 / Factor) + Offset);

                                    if (newPixel < 0) newPixel = 0;
                                    if (newPixel > 255) newPixel = 255;
                                    ptrNewImage[currentPixel + BLUE_OFFSET] = (byte)newPixel;

                                    ptrSource += bytesPerPixel;
                                }
                            }
                        }
                    }
                    finally
                    {
                        source.UnlockBits(bmSource);
                    }
                }
                finally
                {
                    transformed.UnlockBits(bmTransformed);
                }
                return transformed;
            }
        }
    }
}
