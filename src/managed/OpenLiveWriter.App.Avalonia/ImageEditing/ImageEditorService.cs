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

    /// <summary>Pixel effect for <see cref="ImageEditorService.ApplyEffect"/>.</summary>
    public enum ImageEffect
    {
        /// <summary>Black &amp; white (luminance grayscale).</summary>
        Grayscale,

        /// <summary>Warm brown monochrome tint.</summary>
        Sepia,

        /// <summary>Unsharp 3x3 convolution (Windows kernel: 0/-2/11, factor 3).</summary>
        Sharpen,

        /// <summary>Soft 3x3 blur convolution (Windows kernel: 1/2/6, factor 18).</summary>
        Blur,

        /// <summary>Edge-relief 3x3 convolution with a mid-gray bias (Windows kernel).</summary>
        Emboss
    }

    /// <summary>Anchor for <see cref="ImageEditorService.AddTextWatermark"/>.</summary>
    public enum WatermarkPosition
    {
        /// <summary>Top-left corner.</summary>
        TopLeft,

        /// <summary>Top-right corner.</summary>
        TopRight,

        /// <summary>Bottom-left corner.</summary>
        BottomLeft,

        /// <summary>Bottom-right corner (Windows default).</summary>
        BottomRight,

        /// <summary>Horizontally and vertically centered.</summary>
        Center
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
    /// rotation, crop, resize, the Black &amp; White / Sepia color effects,
    /// contrast adjustment, the Sharpen / Blur / Emboss convolutions, and text
    /// watermarks. Input
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

        /// <summary>Sharpening convolution, baked into the pixels.</summary>
        public static byte[] Sharpen(byte[] imageBytes) => ApplyEffect(imageBytes, ImageEffect.Sharpen);

        /// <summary>Soft blur convolution, baked into the pixels.</summary>
        public static byte[] Blur(byte[] imageBytes) => ApplyEffect(imageBytes, ImageEffect.Blur);

        /// <summary>Emboss (edge relief) convolution, baked into the pixels.</summary>
        public static byte[] Emboss(byte[] imageBytes) => ApplyEffect(imageBytes, ImageEffect.Emboss);

        /// <summary>
        /// Applies a pixel effect (alpha preserved). Grayscale/Sepia run as color
        /// matrices; Sharpen/Blur/Emboss run as 3x3 matrix convolutions with the
        /// same kernels Windows Live Writer's decorators used. Undecodable input
        /// throws <see cref="ArgumentException"/>.
        /// </summary>
        public static byte[] ApplyEffect(byte[] imageBytes, ImageEffect effect)
        {
            switch (effect)
            {
                case ImageEffect.Sharpen:
                    return ApplyConvolution(imageBytes, SharpenKernel, 1f / 3f, 0f);
                case ImageEffect.Blur:
                    return ApplyConvolution(imageBytes, BlurKernel, 1f / 18f, 0f);
                case ImageEffect.Emboss:
                    return ApplyConvolution(imageBytes, EmbossKernel, 1f, 127f);
            }

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
        /// Adjusts contrast by <paramref name="percent"/> (-100..100; 0 is the
        /// identity), baked into the pixels. Uses the classic
        /// 259(c+255)/255(259-c) factor around a 128 midpoint, so mid-gray is
        /// invariant and each application compounds on the current pixels.
        /// </summary>
        public static byte[] AdjustContrast(byte[] imageBytes, int percent)
        {
            if (percent < -100 || percent > 100)
                throw new ArgumentOutOfRangeException(nameof(percent),
                    "Contrast must be between -100 and 100 percent.");

            using SKBitmap source = Decode(imageBytes);
            double c = percent * 255.0 / 100.0;
            float factor = (float)((259.0 * (c + 255.0)) / (255.0 * (259.0 - c)));
            // Skia color matrices operate on normalized (0..1) channels, so the
            // 0..255-space translation is scaled down.
            float t = (128f - factor * 128f) / 255f;
            float[] matrix =
            {
                factor, 0, 0, 0, t,
                0, factor, 0, 0, t,
                0, 0, factor, 0, t,
                0, 0, 0,      1, 0
            };

            var result = new SKBitmap(source.Width, source.Height);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);
                canvas.DrawBitmap(source, 0, 0, paint);
            }
            return EncodePng(result);
        }

        /// <summary>
        /// Draws a text watermark into the pixels: white text with a 1px dark
        /// drop-shadow below-right (Windows Live Writer's legibility style), at
        /// the chosen anchor with a small margin. <paramref name="opacity01"/>
        /// (0..1) is the text alpha; <paramref name="sizePx"/> is the font size
        /// in image pixels. Empty text or undecodable input throws
        /// <see cref="ArgumentException"/>.
        /// </summary>
        public static byte[] AddTextWatermark(byte[] imageBytes, string text, float sizePx,
            float opacity01, WatermarkPosition position)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Watermark text must not be empty.", nameof(text));
            if (sizePx <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizePx), "Watermark size must be positive.");

            byte alpha = (byte)(Math.Clamp(opacity01, 0f, 1f) * 255f);
            using SKBitmap source = Decode(imageBytes);
            var result = new SKBitmap(source.Width, source.Height);
            using (var canvas = new SKCanvas(result))
            {
                canvas.DrawBitmap(source, 0, 0);

                using var font = new SKFont(SKTypeface.Default, sizePx)
                {
                    Edging = SKFontEdging.Antialias
                };
                using var paint = new SKPaint();
                float textWidth = font.MeasureText(text, paint);
                font.GetFontMetrics(out SKFontMetrics metrics);
                float margin = Math.Max(4f, sizePx * 0.25f);

                float x = position == WatermarkPosition.TopLeft || position == WatermarkPosition.BottomLeft
                    ? margin
                    : position == WatermarkPosition.Center
                        ? (source.Width - textWidth) / 2f
                        : source.Width - textWidth - margin;
                // Baseline: the ascent sits above it, the descent below.
                float baseline = position == WatermarkPosition.TopLeft || position == WatermarkPosition.TopRight
                    ? margin - metrics.Ascent
                    : position == WatermarkPosition.Center
                        ? (source.Height - metrics.Ascent - metrics.Descent) / 2f
                        : source.Height - margin - metrics.Descent;
                x = Math.Max(0, x);
                baseline = Math.Max(-metrics.Ascent, baseline);

                paint.Color = new SKColor(0, 0, 0, alpha);
                canvas.DrawText(text, x + 1, baseline + 1, SKTextAlign.Left, font, paint);
                paint.Color = new SKColor(255, 255, 255, alpha);
                canvas.DrawText(text, x, baseline, SKTextAlign.Left, font, paint);
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

        // The 3x3 kernels Windows Live Writer's sharpen/blur/emboss decorators
        // used (TransformMatrix corner/edge/middle + factor, applied above as
        // the convolution gain; emboss adds a mid-gray bias).
        private static readonly float[] SharpenKernel =
        {
             0, -2,  0,
            -2, 11, -2,
             0, -2,  0
        };

        private static readonly float[] BlurKernel =
        {
            1, 2, 1,
            2, 6, 2,
            1, 2, 1
        };

        private static readonly float[] EmbossKernel =
        {
            -1, -1, -1,
            -1,  8, -1,
            -1, -1, -1
        };

        // Runs a 3x3 matrix convolution (alpha passes through unconvolved).
        // Skia's convolution filter samples out-of-bounds pixels as transparent
        // black, so the image is first padded with a 1px duplicate border —
        // Windows Conv3x3's edge convention — and cropped back afterwards.
        private static byte[] ApplyConvolution(byte[] imageBytes, float[] kernel, float gain, float bias)
        {
            using SKBitmap source = Decode(imageBytes);
            int w = source.Width, h = source.Height;

            var padded = new SKBitmap(w + 2, h + 2);
            using (var canvas = new SKCanvas(padded))
            {
                canvas.DrawBitmap(source, 1, 1);
                DrawStrip(canvas, source, new SKRect(0, 0, w, 1), new SKRect(1, 0, w + 1, 1));
                DrawStrip(canvas, source, new SKRect(0, h - 1, w, h), new SKRect(1, h + 1, w + 1, h + 2));
                DrawStrip(canvas, source, new SKRect(0, 0, 1, h), new SKRect(0, 1, 1, h + 1));
                DrawStrip(canvas, source, new SKRect(w - 1, 0, w, h), new SKRect(w + 1, 1, w + 2, h + 1));
                DrawStrip(canvas, source, new SKRect(0, 0, 1, 1), new SKRect(0, 0, 1, 1));
                DrawStrip(canvas, source, new SKRect(w - 1, 0, w, 1), new SKRect(w + 1, 0, w + 2, 1));
                DrawStrip(canvas, source, new SKRect(0, h - 1, 1, h), new SKRect(0, h + 1, 1, h + 2));
                DrawStrip(canvas, source, new SKRect(w - 1, h - 1, w, h), new SKRect(w + 1, h + 1, w + 2, h + 2));
            }

            using SKImage image = SKImage.FromBitmap(padded);
            using SKImageFilter filter = SKImageFilter.CreateMatrixConvolution(
                new SKSizeI(3, 3), kernel, gain, bias, new SKPointI(1, 1),
                SKShaderTileMode.Clamp, false, null);
            var info = new SKImageInfo(w + 2, h + 2);
            using SKSurface surface = SKSurface.Create(info);
            using (var paint = new SKPaint { ImageFilter = filter })
                surface.Canvas.DrawImage(image, 0, 0, paint);
            surface.Canvas.Flush();
            using SKImage cropped = surface.Snapshot(new SKRectI(1, 1, w + 1, h + 1));
            using SKData data = cropped.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static void DrawStrip(SKCanvas canvas, SKBitmap source, SKRect src, SKRect dest) =>
            canvas.DrawBitmap(source, src, dest);

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
