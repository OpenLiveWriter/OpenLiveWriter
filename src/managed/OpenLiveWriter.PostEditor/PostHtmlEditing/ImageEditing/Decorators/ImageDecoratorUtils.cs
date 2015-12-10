// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.Drawing;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    /// <summary>
    /// Summary description for ImageDecoratorUtils.
    /// </summary>
    public class ImageDecoratorUtils
    {
        private ImageDecoratorUtils()
        {

        }

        public static RotateFlipType GetFlipTypeRotatedCCW(RotateFlipType currentRotation)
        {
            RotateFlipType newRotation;
            switch (currentRotation)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    newRotation = RotateFlipType.Rotate270FlipNone;
                    break;
                case RotateFlipType.Rotate90FlipNone:
                    newRotation = RotateFlipType.RotateNoneFlipNone;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                case RotateFlipType.RotateNoneFlipY: // legacy images
                    newRotation = RotateFlipType.Rotate90FlipNone;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    newRotation = RotateFlipType.Rotate180FlipNone;
                    break;
                default:
                    Debug.Fail("Unknown RotateFlipType encountered: " + currentRotation.ToString());
                    newRotation = RotateFlipType.RotateNoneFlipNone;
                    break;
            }
            return newRotation;
        }

        public static RotateFlipType GetFlipTypeRotatedCW(RotateFlipType currentRotation)
        {
            RotateFlipType newRotation;
            switch (currentRotation)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    newRotation = RotateFlipType.Rotate90FlipNone;
                    break;
                case RotateFlipType.Rotate90FlipNone:
                    newRotation = RotateFlipType.Rotate180FlipNone;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                case RotateFlipType.RotateNoneFlipY: // legacy images
                    newRotation = RotateFlipType.Rotate270FlipNone;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    newRotation = RotateFlipType.RotateNoneFlipNone;
                    break;
                default:
                    Debug.Fail("Unknown RotateFlipType encountered: " + currentRotation.ToString());
                    newRotation = RotateFlipType.RotateNoneFlipNone;
                    break;
            }
            return newRotation;
        }
    }
}
