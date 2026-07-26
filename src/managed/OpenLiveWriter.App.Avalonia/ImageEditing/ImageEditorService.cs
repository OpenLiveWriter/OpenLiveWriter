// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using SkiaSharp;

namespace OpenLiveWriter.App.Avalonia.ImageEditing
{
    /// <summary>Rotation direction for <see cref="ImageEditorService.Rotate90"/>.</summary>
    public enum ImageRotation
    {
        /// <summary>90 degrees clockwise ("rotate right").</summary>
        Clockwise,

        /// <summary>90 degrees counter-clockwise ("rotate left").</summary>
        CounterClockwise
    }

    /// <summary>Color effect for <see cref="ImageEditorService.ApplyEffect"/>.</summary>
    public enum ImageEffect
    {
        /// <summary>Black &amp; white (luminance grayscale).</summary>
        Grayscale,

        /// <summary>Warm brown monochrome tint.</summary>
        Sepia
    }

    /// <summary>
    /// How the editor should adjust the selected image's display size after a
    /// baked (pixel-rewritten) replacement: keep it, swap explicit width/height
    /// (90-degree rotation), or set explicit px dimensions (crop/resize).
    /// </summary>
    public enum BakedImageSizeMode
    {
        /// <summary>Leave the display size alone (color effects).</summary>
        Keep,

        /// <summary>Swap explicit width/height when present; otherwise reset to
        /// natural size (the baked image's natural dimensions are swapped).</summary>
        Swap,

        /// <summary>Set explicit px width/height (crop/resize target size).</summary>
        Set
    }

    /// <summary>
    /// Pure, headless pixel operations for Picture Tools: baked 90-degree
    /// rotation, crop, resize, and the Black &amp; White / Sepia effects. Input
    /// is encoded image bytes (any format Skia decodes: PNG/JPEG/GIF/BMP/WebP);
    /// output is always PNG bytes — the honest lossless-safe default, and the
    /// publish pipeline (data-URI newMediaObject upload) accepts PNG as-is.
    /// No WebView or UI dependencies, so every operation is unit-testable.
    /// </summary>
    public static class ImageEditorService
    {
        /// <summary>
        /// Bakes a 90-degree rotation into the pixels. The output dimensions are
        /// swapped (W×H becomes H×W). Throws <see cref="ArgumentException"/> when
        /// the input is not a decodable image.
        /// </summary>
        public static byte[] Rotate90(byte[] imageBytes, ImageRotation direction)
        {
            using SKBitmap source = Decode(imageBytes);
            var rotated = new SKBitmap(source.Height, source.Width);
            using (var canvas = new SKCanvas(rotated))
            {
                if (direction == ImageRotation.Clockwise)
                {
                    canvas.Translate(source.Height, 0);
                    canvas.RotateDegrees(90);
                }
                else
                {
                    canvas.Translate(0, source.Width);
                    canvas.RotateDegrees(-90);
                }
                canvas.DrawBitmap(source, 0, 0);
            }
            return EncodePng(rotated);
        }

        /// <summary>
        /// Crops a pixel rectangle out of the image. The rectangle is clamped to
        /// the image bounds; an empty intersection (or out-of-range origin)
        /// throws <see cref="ArgumentException"/>, as does undecodable input.
        /// </summary>
        public static byte[] Crop(byte[] imageBytes, int x, int y, int width, int height)
        {
            using SKBitmap source = Decode(imageBytes);
            var rect = ClampCrop(x, y, width, height, source.Width, source.Height);
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new ArgumentException("The crop rectangle does not intersect the image.");

            var cropped = new SKBitmap(rect.Width, rect.Height);
            using (var canvas = new SKCanvas(cropped))
                canvas.DrawBitmap(source, -rect.Left, -rect.Top);
            return EncodePng(cropped);
        }

        /// <summary>
        /// Clamps a requested crop rectangle to the image bounds (origin pulled
        /// into range, size reduced to fit). Pure so both the dialog's OK path
        /// and <see cref="Crop"/> share identical semantics.
        /// </summary>
        internal static SKRectI ClampCrop(int x, int y, int width, int height,
            int imageWidth, int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
                return SKRectI.Empty;

            int left = Math.Max(0, Math.Min(x, imageWidth - 1));
            int top = Math.Max(0, Math.Min(y, imageHeight - 1));
            int right = Math.Max(left + 1, Math.Min(x + Math.Max(width, 1), imageWidth));
            int bottom = Math.Max(top + 1, Math.Min(y + Math.Max(height, 1), imageHeight));
            return new SKRectI(left, top, right, bottom);
        }

        /// <summary>
        /// Bakes a resize into the pixels (high-quality cubic sampling). The
        /// display-size presets/spinners stay non-destructive (width/height
        /// attributes); this exists for flows that must rewrite the pixels
        /// themselves. Throws on undecodable input or non-positive dimensions.
        /// </summary>
        public static byte[] Resize(byte[] imageBytes, int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Resize dimensions must be positive.");

            using SKBitmap source = Decode(imageBytes);
            using SKBitmap resized = source.Resize(new SKImageInfo(width, height),
                new SKSamplingOptions(SKCubicResampler.Mitchell));
            if (resized == null)
                throw new ArgumentException("The image could not be resized.");
            return EncodePng(resized);
        }

        /// <summary>Black &amp; white (luminance) conversion, baked into the pixels.</summary>
        public static byte[] Grayscale(byte[] imageBytes) => ApplyEffect(imageBytes, ImageEffect.Grayscale);

        /// <summary>Sepia-tone conversion, baked into the pixels.</summary>
        public static byte[] Sepia(byte[] imageBytes) => ApplyEffect(imageBytes, ImageEffect.Sepia);

        /// <summary>
        /// Applies a color-matrix effect (alpha preserved). Undecodable input
        /// throws <see cref="ArgumentException"/>.
        /// </summary>
        public static byte[] ApplyEffect(byte[] imageBytes, ImageEffect effect)
        {
            using SKBitmap source = Decode(imageBytes);
            var result = new SKBitmap(source.Width, source.Height);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(
                    effect == ImageEffect.Sepia ? SepiaMatrix : GrayscaleMatrix);
                canvas.DrawBitmap(source, 0, 0, paint);
            }
            return EncodePng(result);
        }

        /// <summary>
        /// Reports the pixel dimensions of an encoded image without fully
        /// decoding the pixels. False when the bytes are not a decodable image.
        /// </summary>
        public static bool TryGetDimensions(byte[] imageBytes, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (imageBytes == null || imageBytes.Length == 0)
                return false;

            try
            {
                using var codec = SKCodec.Create(new SKMemoryStream(imageBytes));
                if (codec == null)
                    return false;
                width = codec.Info.Width;
                height = codec.Info.Height;
                return width > 0 && height > 0;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException
                || ex is ObjectDisposedException)
            {
                return false;
            }
        }

        // Rec.601 luma weights; alpha row passes through untouched.
        private static readonly float[] GrayscaleMatrix =
        {
            0.299f, 0.587f, 0.114f, 0, 0,
            0.299f, 0.587f, 0.114f, 0, 0,
            0.299f, 0.587f, 0.114f, 0, 0,
            0,      0,      0,      1, 0
        };

        // Classic sepia tone (same weights Windows Live Writer's sepia used).
        private static readonly float[] SepiaMatrix =
        {
            0.393f, 0.769f, 0.189f, 0, 0,
            0.349f, 0.686f, 0.168f, 0, 0,
            0.272f, 0.534f, 0.131f, 0, 0,
            0,      0,      0,      1, 0
        };

        private static SKBitmap Decode(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("No image bytes supplied.");

            try
            {
                SKBitmap bitmap = SKBitmap.Decode(imageBytes);
                if (bitmap == null)
                    throw new ArgumentException("The bytes are not a decodable image.");
                return bitmap;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException)
            {
                // SkiaSharp 3.x throws (rather than returning null) when no codec
                // recognizes the bytes — normalize to ArgumentException.
                throw new ArgumentException("The bytes are not a decodable image.", ex);
            }
        }

        private static byte[] EncodePng(SKBitmap bitmap)
        {
            using (bitmap)
            using (SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                return data.ToArray();
        }
    }
}
