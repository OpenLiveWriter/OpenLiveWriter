// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.ImageEditing;
using OpenLiveWriter.Localization;
using SkiaSharp;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group Z — Picture Tools pixel baking, second wave: text watermark,
    /// contrast adjustment, and the Sharpen / Blur / Emboss convolutions in
    /// <see cref="ImageEditorService"/> (same kernels as the Windows
    /// decorators), plus the command registration for the newly enabled
    /// commands. Pure SKBitmap assertions in the style of Group Y.
    /// </summary>
    [TestFixture]
    [Category("GroupZ")]
    public class GroupZ_ImageEffectsTests
    {
        private static readonly SKColor MidGray = new SKColor(128, 128, 128);

        private static byte[] CreatePng(int width, int height, Func<int, int, SKColor> pixel)
        {
            using var bitmap = new SKBitmap(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    bitmap.SetPixel(x, y, pixel(x, y));
            using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static double RedVariance(SKBitmap bitmap)
        {
            double sum = 0, sumSq = 0;
            int n = bitmap.Width * bitmap.Height;
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    double v = bitmap.GetPixel(x, y).Red;
                    sum += v;
                    sumSq += v * v;
                }
            double mean = sum / n;
            return sumSq / n - mean * mean;
        }

        // ---- Watermark ----

        [Test]
        public void Watermark_BottomRight_DrawsOnlyInBottomRightQuadrant()
        {
            byte[] png = CreatePng(200, 200, (x, y) => MidGray);

            using SKBitmap result = SKBitmap.Decode(
                ImageEditorService.AddTextWatermark(png, "TEST", 20f, 1f, WatermarkPosition.BottomRight));

            bool bottomRightTouched = false;
            for (int y = 150; y < 200 && !bottomRightTouched; y++)
                for (int x = 100; x < 200 && !bottomRightTouched; x++)
                    bottomRightTouched = result.GetPixel(x, y) != MidGray;
            Assert.That(bottomRightTouched, Is.True, "the watermark must draw non-background pixels");

            for (int y = 0; y < 60; y++)
                for (int x = 0; x < 60; x++)
                    Assert.That(result.GetPixel(x, y), Is.EqualTo(MidGray),
                        $"top-left pixel ({x},{y}) must be untouched by a bottom-right watermark");

            Assert.That(result.GetPixel(190, 190).Alpha, Is.EqualTo(255), "alpha passes through");
        }

        [Test]
        public void Watermark_TopLeft_DrawsOnlyInTopLeftQuadrant()
        {
            byte[] png = CreatePng(200, 200, (x, y) => MidGray);

            using SKBitmap result = SKBitmap.Decode(
                ImageEditorService.AddTextWatermark(png, "TEST", 20f, 1f, WatermarkPosition.TopLeft));

            bool topLeftTouched = false;
            for (int y = 0; y < 60 && !topLeftTouched; y++)
                for (int x = 0; x < 100 && !topLeftTouched; x++)
                    topLeftTouched = result.GetPixel(x, y) != MidGray;
            Assert.That(topLeftTouched, Is.True, "the watermark must draw non-background pixels");

            for (int y = 150; y < 200; y++)
                for (int x = 150; x < 200; x++)
                    Assert.That(result.GetPixel(x, y), Is.EqualTo(MidGray),
                        $"bottom-right pixel ({x},{y}) must be untouched by a top-left watermark");
        }

        [Test]
        public void Watermark_ZeroOpacity_LeavesPixelsUnchanged()
        {
            byte[] png = CreatePng(120, 80, (x, y) =>
                new SKColor((byte)(x * 2), (byte)(y * 3), (byte)(x + y)));

            using SKBitmap original = SKBitmap.Decode(png);
            using SKBitmap result = SKBitmap.Decode(
                ImageEditorService.AddTextWatermark(png, "INVISIBLE", 16f, 0f, WatermarkPosition.Center));

            for (int y = 0; y < original.Height; y++)
                for (int x = 0; x < original.Width; x++)
                    Assert.That(result.GetPixel(x, y), Is.EqualTo(original.GetPixel(x, y)),
                        $"pixel ({x},{y}) must survive a fully transparent watermark");
        }

        [Test]
        public void Watermark_InvalidInput_Throws()
        {
            byte[] png = CreatePng(4, 4, (x, y) => MidGray);
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(
                    () => ImageEditorService.AddTextWatermark(png, "  ", 12f, 0.5f, WatermarkPosition.Center));
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => ImageEditorService.AddTextWatermark(png, "X", 0f, 0.5f, WatermarkPosition.Center));
                Assert.Throws<ArgumentException>(
                    () => ImageEditorService.AddTextWatermark(new byte[] { 1, 2, 3 }, "X", 12f, 0.5f,
                        WatermarkPosition.Center));
            });
        }

        // ---- Contrast ----

        [Test]
        public void Contrast_ZeroPercent_IsIdentity()
        {
            byte[] png = CreatePng(8, 8, (x, y) =>
                new SKColor((byte)(30 * x), (byte)(30 * y), (byte)(15 * (x + y))));

            using SKBitmap original = SKBitmap.Decode(png);
            using SKBitmap result = SKBitmap.Decode(ImageEditorService.AdjustContrast(png, 0));

            for (int y = 0; y < original.Height; y++)
                for (int x = 0; x < original.Width; x++)
                    Assert.That(result.GetPixel(x, y), Is.EqualTo(original.GetPixel(x, y)),
                        $"pixel ({x},{y}) must be unchanged at 0% contrast");
        }

        [Test]
        public void Contrast_MidGray_IsInvariant()
        {
            byte[] png = CreatePng(4, 4, (x, y) => MidGray);

            using SKBitmap up = SKBitmap.Decode(ImageEditorService.AdjustContrast(png, 60));
            using SKBitmap down = SKBitmap.Decode(ImageEditorService.AdjustContrast(png, -60));
            Assert.Multiple(() =>
            {
                Assert.That(up.GetPixel(0, 0).Red, Is.InRange(126, 130), "+60% keeps mid-gray");
                Assert.That(down.GetPixel(0, 0).Red, Is.InRange(126, 130), "-60% keeps mid-gray");
            });
        }

        [Test]
        public void Contrast_Positive_PushesExtremesApart()
        {
            // Left half dark (64), right half bright (192).
            byte[] png = CreatePng(8, 4, (x, y) =>
                new SKColor((byte)(x < 4 ? 64 : 192), 0, 0));

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.AdjustContrast(png, 50));
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPixel(0, 0).Red, Is.LessThan(64), "dark gets darker");
                Assert.That(result.GetPixel(7, 0).Red, Is.GreaterThan(192), "bright gets brighter");
            });
        }

        [Test]
        public void Contrast_Negative_PullsTowardMidGray()
        {
            byte[] png = CreatePng(8, 4, (x, y) =>
                new SKColor((byte)(x < 4 ? 64 : 192), 0, 0));

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.AdjustContrast(png, -50));
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPixel(0, 0).Red, Is.GreaterThan(64), "dark lifts toward mid-gray");
                Assert.That(result.GetPixel(7, 0).Red, Is.LessThan(192), "bright sinks toward mid-gray");
            });
        }

        [Test]
        public void Contrast_InvalidInput_Throws()
        {
            byte[] png = CreatePng(2, 2, (x, y) => MidGray);
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => ImageEditorService.AdjustContrast(png, 101));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImageEditorService.AdjustContrast(png, -101));
                Assert.Throws<ArgumentException>(() => ImageEditorService.AdjustContrast(new byte[] { 9 }, 20));
            });
        }

        // ---- Sharpen ----

        [Test]
        public void Sharpen_FlatImage_StaysFlat()
        {
            byte[] png = CreatePng(6, 6, (x, y) => MidGray);
            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Sharpen(png));
            Assert.That(result.GetPixel(3, 3).Red, Is.InRange(126, 130));
        }

        [Test]
        public void Sharpen_Edge_IncreasesLocalContrast()
        {
            // Vertical step: left half 100, right half 156 — no clamping, so the
            // Windows kernel (0/-2/11, factor 3) pushes the two sides apart.
            byte[] png = CreatePng(8, 8, (x, y) =>
                new SKColor((byte)(x < 4 ? 100 : 156), (byte)(x < 4 ? 100 : 156), (byte)(x < 4 ? 100 : 156)));

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Sharpen(png));
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPixel(3, 4).Red, Is.LessThan(100),
                    "dark side of the edge gets darker");
                Assert.That(result.GetPixel(4, 4).Red, Is.GreaterThan(156),
                    "bright side of the edge gets brighter");
                Assert.That(result.GetPixel(0, 4).Red, Is.InRange(98, 102),
                    "far from the edge the flat region is unchanged");
            });
        }

        // ---- Blur ----

        [Test]
        public void Blur_FlatImage_StaysFlat()
        {
            byte[] png = CreatePng(6, 6, (x, y) => MidGray);
            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Blur(png));
            Assert.That(result.GetPixel(3, 3).Red, Is.InRange(126, 130));
        }

        [Test]
        public void Blur_NoisyImage_ReducesVariance()
        {
            // Deterministic high-frequency noise (alternating 0/255 blocks).
            byte[] png = CreatePng(16, 16, (x, y) =>
                (x + y) % 2 == 0 ? new SKColor(0, 0, 0) : new SKColor(255, 255, 255));

            using SKBitmap original = SKBitmap.Decode(png);
            using SKBitmap blurred = SKBitmap.Decode(ImageEditorService.Blur(png));
            Assert.That(RedVariance(blurred), Is.LessThan(RedVariance(original) * 0.5),
                "blur must smooth high-frequency noise");
        }

        // ---- Emboss ----

        [Test]
        public void Emboss_FlatImage_BecomesMidGray()
        {
            byte[] png = CreatePng(6, 6, (x, y) => new SKColor(200, 200, 200));
            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Emboss(png));
            SKColor c = result.GetPixel(3, 3);
            Assert.Multiple(() =>
            {
                Assert.That(c.Red, Is.InRange(120, 135), "kernel sums to zero; the bias dominates");
                Assert.That(c.Alpha, Is.EqualTo(255), "alpha passes through");
            });
        }

        [Test]
        public void Emboss_Edge_ProducesReliefContrast()
        {
            // Vertical step 64 → 192: emboss lights one side of the edge and
            // darkens the other around the mid-gray bias.
            byte[] png = CreatePng(8, 8, (x, y) =>
            {
                byte v = (byte)(x < 4 ? 64 : 192);
                return new SKColor(v, v, v);
            });

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Emboss(png));
            int edgeDark = result.GetPixel(3, 4).Red;
            int edgeBright = result.GetPixel(4, 4).Red;
            int flat = result.GetPixel(0, 4).Red;
            Assert.Multiple(() =>
            {
                Assert.That(flat, Is.InRange(120, 135), "flat regions sit at the mid-gray bias");
                Assert.That(Math.Abs(edgeDark - flat), Is.GreaterThan(30),
                    "the edge deviates from the bias (relief)");
                Assert.That(edgeDark, Is.Not.EqualTo(edgeBright), "the two sides of the edge differ");
            });
        }

        [Test]
        public void Convolutions_InvalidInput_ThrowArgumentException()
        {
            byte[] garbage = { 1, 2, 3, 4, 5 };
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => ImageEditorService.Sharpen(garbage));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Blur(garbage));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Emboss(garbage));
                Assert.Throws<ArgumentException>(
                    () => ImageEditorService.ApplyEffect(garbage, ImageEffect.Sharpen));
            });
        }

        // ---- Command registration ----

        [Test]
        public void Registry_NewPixelCommands_AreHandled()
        {
            Assert.Multiple(() =>
            {
                Assert.That(HandledCommands.IsHandled(CommandId.Watermark), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageContrast), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectSharpen), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectGaussianBlur), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectEmboss), Is.True);
            });
        }

        [Test]
        public void Registry_TiltAndRecolor_StayDisabled()
        {
            Assert.Multiple(() =>
            {
                Assert.That(HandledCommands.IsHandled(CommandId.ImageTilt), Is.False,
                    "tilt is a perspective transform with little value — stays dead");
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectsRecolorGallery), Is.False,
                    "recolor needs the temperature/tint UX — stays dead");
            });
        }
    }
}
