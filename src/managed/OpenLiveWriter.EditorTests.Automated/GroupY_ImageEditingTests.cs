// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Commands;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Avalonia.Controls;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using SkiaSharp;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group Y — Picture Tools pixel baking. Covers the pure SkiaSharp
    /// operations in <see cref="ImageEditorService"/> (rotate 90, crop, resize,
    /// grayscale, sepia) with dimension and pixel spot-check assertions, the
    /// data-URI decode/re-embed round-trip, the selected-image JSON parsing
    /// used by the bake pipeline, and the command registration / ribbon wiring
    /// for the newly enabled crop / rotate / effects commands.
    /// </summary>
    [TestFixture]
    [Category("GroupY")]
    public class GroupY_ImageEditingTests
    {
        private static readonly SKColor Red = new SKColor(255, 0, 0);
        private static readonly SKColor Blue = new SKColor(0, 0, 255);

        private static byte[] CreatePng(int width, int height, Func<int, int, SKColor> pixel)
        {
            using var bitmap = new SKBitmap(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    bitmap.SetPixel(x, y, pixel(x, y));
            using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        // ---- Rotate90 ----

        [Test]
        public void Rotate90_Clockwise_SwapsDimensionsAndPixels()
        {
            byte[] png = CreatePng(2, 1, (x, y) => x == 0 ? Red : Blue);

            byte[] rotated = ImageEditorService.Rotate90(png, ImageRotation.Clockwise);

            using SKBitmap result = SKBitmap.Decode(rotated);
            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(1));
                Assert.That(result.Height, Is.EqualTo(2));
                Assert.That(result.GetPixel(0, 0), Is.EqualTo(Red), "left end moves to the top");
                Assert.That(result.GetPixel(0, 1), Is.EqualTo(Blue), "right end moves to the bottom");
            });
        }

        [Test]
        public void Rotate90_CounterClockwise_SwapsDimensionsAndPixels()
        {
            byte[] png = CreatePng(2, 1, (x, y) => x == 0 ? Red : Blue);

            byte[] rotated = ImageEditorService.Rotate90(png, ImageRotation.CounterClockwise);

            using SKBitmap result = SKBitmap.Decode(rotated);
            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(1));
                Assert.That(result.Height, Is.EqualTo(2));
                Assert.That(result.GetPixel(0, 0), Is.EqualTo(Blue), "right end moves to the top");
                Assert.That(result.GetPixel(0, 1), Is.EqualTo(Red), "left end moves to the bottom");
            });
        }

        [Test]
        public void Rotate90_FourQuarterTurns_ReturnsOriginalPixels()
        {
            byte[] png = CreatePng(3, 2, (x, y) =>
                new SKColor((byte)(40 * x), (byte)(80 * y), (byte)(30 * (x + y))));

            byte[] spun = png;
            for (int i = 0; i < 4; i++)
                spun = ImageEditorService.Rotate90(spun, ImageRotation.Clockwise);

            using SKBitmap original = SKBitmap.Decode(png);
            using SKBitmap result = SKBitmap.Decode(spun);
            Assert.That(result.Width, Is.EqualTo(original.Width));
            Assert.That(result.Height, Is.EqualTo(original.Height));
            for (int y = 0; y < original.Height; y++)
                for (int x = 0; x < original.Width; x++)
                    Assert.That(result.GetPixel(x, y), Is.EqualTo(original.GetPixel(x, y)),
                        $"pixel ({x},{y}) must survive four 90-degree turns");
        }

        // ---- Crop ----

        [Test]
        public void Crop_ReturnsRegionWithExpectedPixels()
        {
            // Unique color per pixel: R encodes x, G encodes y.
            byte[] png = CreatePng(4, 3, (x, y) => new SKColor((byte)(50 * x), (byte)(80 * y), 0));

            byte[] cropped = ImageEditorService.Crop(png, 1, 1, 2, 2);

            using SKBitmap result = SKBitmap.Decode(cropped);
            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(2));
                Assert.That(result.Height, Is.EqualTo(2));
                Assert.That(result.GetPixel(0, 0), Is.EqualTo(new SKColor(50, 80, 0)),
                    "crop origin maps to source pixel (1,1)");
                Assert.That(result.GetPixel(1, 1), Is.EqualTo(new SKColor(100, 160, 0)),
                    "crop bottom-right maps to source pixel (2,2)");
            });
        }

        [Test]
        public void Crop_OutOfBoundsRectangle_IsClampedToImage()
        {
            byte[] png = CreatePng(4, 3, (x, y) => Red);

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Crop(png, 2, 2, 10, 10));
            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(2), "width clamps to the right edge");
                Assert.That(result.Height, Is.EqualTo(1), "height clamps to the bottom edge");
            });

            // A wholly out-of-range origin pulls back to the nearest edge pixel.
            using SKBitmap edge = SKBitmap.Decode(ImageEditorService.Crop(png, 99, 99, 5, 5));
            Assert.Multiple(() =>
            {
                Assert.That(edge.Width, Is.EqualTo(1));
                Assert.That(edge.Height, Is.EqualTo(1));
            });
        }

        // ---- Resize ----

        [Test]
        public void Resize_ProducesExactTargetDimensions()
        {
            byte[] png = CreatePng(4, 2, (x, y) => Red);

            using SKBitmap shrunk = SKBitmap.Decode(ImageEditorService.Resize(png, 2, 1));
            Assert.Multiple(() =>
            {
                Assert.That(shrunk.Width, Is.EqualTo(2));
                Assert.That(shrunk.Height, Is.EqualTo(1));
            });

            using SKBitmap grown = SKBitmap.Decode(ImageEditorService.Resize(png, 8, 4));
            Assert.Multiple(() =>
            {
                Assert.That(grown.Width, Is.EqualTo(8));
                Assert.That(grown.Height, Is.EqualTo(4));
            });
        }

        [Test]
        public void Resize_SolidColor_StaysSolid()
        {
            byte[] png = CreatePng(4, 4, (x, y) => Blue);
            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Resize(png, 2, 2));
            Assert.That(result.GetPixel(0, 0), Is.EqualTo(Blue));
            Assert.That(result.GetPixel(1, 1), Is.EqualTo(Blue));
        }

        // ---- Effects ----

        [Test]
        public void Grayscale_PureRed_BecomesNeutralGray()
        {
            byte[] png = CreatePng(2, 2, (x, y) => Red);

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Grayscale(png));

            SKColor gray = result.GetPixel(0, 0);
            Assert.Multiple(() =>
            {
                Assert.That(gray.Red, Is.EqualTo(gray.Green));
                Assert.That(gray.Green, Is.EqualTo(gray.Blue));
                // Rec.601 luma of pure red: 0.299 * 255 ≈ 76.
                Assert.That(gray.Red, Is.InRange(74, 79));
                Assert.That(gray.Alpha, Is.EqualTo(255), "alpha passes through");
            });
        }

        [Test]
        public void Sepia_PureRed_ProducesWarmTone()
        {
            byte[] png = CreatePng(2, 2, (x, y) => Red);

            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Sepia(png));

            SKColor sepia = result.GetPixel(0, 0);
            Assert.Multiple(() =>
            {
                // Sepia of pure red: (100, 89, 69) with the classic weights.
                Assert.That(sepia.Red, Is.InRange(98, 103));
                Assert.That(sepia.Green, Is.InRange(87, 92));
                Assert.That(sepia.Blue, Is.InRange(67, 72));
                Assert.That(sepia.Red, Is.GreaterThan(sepia.Green));
                Assert.That(sepia.Green, Is.GreaterThan(sepia.Blue));
            });
        }

        [Test]
        public void Sepia_White_StaysNearWhite()
        {
            byte[] png = CreatePng(1, 1, (x, y) => new SKColor(255, 255, 255));
            using SKBitmap result = SKBitmap.Decode(ImageEditorService.Sepia(png));
            SKColor c = result.GetPixel(0, 0);
            Assert.Multiple(() =>
            {
                Assert.That(c.Red, Is.EqualTo(255), "channels clamp at 255");
                Assert.That(c.Green, Is.EqualTo(255));
                Assert.That(c.Blue, Is.InRange(236, 242));
            });
        }

        // ---- Invalid input ----

        [Test]
        public void Ops_UndecodableInput_ThrowArgumentException()
        {
            byte[] garbage = { 1, 2, 3, 4, 5 };
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(
                    () => ImageEditorService.Rotate90(garbage, ImageRotation.Clockwise));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Crop(garbage, 0, 0, 1, 1));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Resize(garbage, 2, 2));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Grayscale(garbage));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Sepia(garbage));
                Assert.Throws<ArgumentException>(() => ImageEditorService.Rotate90(null, ImageRotation.Clockwise));
                Assert.Throws<ArgumentException>(
                    () => ImageEditorService.Resize(CreatePng(2, 2, (x, y) => Red), 0, 2));
            });
        }

        [Test]
        public void TryGetDimensions_ReportsSizeAndRejectsGarbage()
        {
            byte[] png = CreatePng(7, 5, (x, y) => Red);
            Assert.That(ImageEditorService.TryGetDimensions(png, out int w, out int h), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(w, Is.EqualTo(7));
                Assert.That(h, Is.EqualTo(5));
            });

            Assert.That(ImageEditorService.TryGetDimensions(new byte[] { 9, 9, 9 }, out _, out _), Is.False);
            Assert.That(ImageEditorService.TryGetDimensions(null, out _, out _), Is.False);
        }

        // ---- Data-URI round-trip ----

        [Test]
        public void DataUri_DecodeReembedRoundTrip_PreservesBytes()
        {
            byte[] png = CreatePng(3, 3, (x, y) => x == y ? Red : Blue);

            string dataUri = ImageDataUri.BuildPng(png);
            Assert.That(dataUri, Does.StartWith("data:image/png;base64,"));
            Assert.That(ImageDataUri.TryDecode(dataUri, out byte[] decoded), Is.True);
            Assert.That(decoded, Is.EqualTo(png));

            // The full bake pipeline: decode from src, bake, re-embed, decode again.
            byte[] baked = ImageEditorService.Rotate90(decoded, ImageRotation.Clockwise);
            Assert.That(ImageDataUri.TryDecode(ImageDataUri.BuildPng(baked), out byte[] roundTripped), Is.True);
            using SKBitmap result = SKBitmap.Decode(roundTripped);
            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(3));
                Assert.That(result.Height, Is.EqualTo(3));
            });
        }

        [Test]
        public void DataUri_TryDecode_RejectsNonDataAndMalformedUris()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ImageDataUri.TryDecode("https://example.com/p.png", out _), Is.False,
                    "remote URLs must be fetched, not decoded");
                Assert.That(ImageDataUri.TryDecode("data:image/svg+xml,<svg/>", out _), Is.False,
                    "non-base64 payloads are unsupported");
                Assert.That(ImageDataUri.TryDecode("data:image/png;base64,!!!not-base64!!!", out _), Is.False);
                Assert.That(ImageDataUri.TryDecode("", out _), Is.False);
                Assert.That(ImageDataUri.TryDecode(null, out _), Is.False);
            });
        }

        // ---- Selected-image payload parsing (bake pipeline input) ----

        [Test]
        public void ParseSelectedImageJson_ParsesPayloadAndNull()
        {
            string json = "{\"src\":\"data:image/png;base64,AAAA\",\"naturalWidth\":1600," +
                "\"naturalHeight\":900,\"width\":320,\"height\":180,\"alt\":\"\",\"title\":\"\"," +
                "\"alignment\":\"inline\",\"margin\":null,\"rotation\":0,\"borderWidth\":null," +
                "\"borderColor\":null,\"link\":\"\"}";

            ImageFormatState img = WebViewEditor.ParseSelectedImageJson(json);
            Assert.That(img, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(img.NaturalWidth, Is.EqualTo(1600));
                Assert.That(img.Width, Is.EqualTo(320));
                Assert.That(img.HasRemoteSource, Is.False);
            });

            Assert.That(WebViewEditor.ParseSelectedImageJson("null"), Is.Null);
            Assert.That(WebViewEditor.ParseSelectedImageJson(null), Is.Null);
        }

        // ---- Command registration ----

        [Test]
        public void Registry_PixelBakingCommands_AreHandled()
        {
            Assert.Multiple(() =>
            {
                Assert.That(HandledCommands.IsHandled(CommandId.ImageCrop), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageRotateCW), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageRotateCCW), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectsGallery), Is.True,
                    "the effects dropdown parent stays enabled so its menu opens");
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectBlackAndWhite), Is.True);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectSepiaTone), Is.True);
            });
        }

        [Test]
        public void Registry_RemainingPixelCommands_StayDisabled()
        {
            Assert.Multiple(() =>
            {
                Assert.That(HandledCommands.IsHandled(CommandId.ImageTilt), Is.False);
                Assert.That(HandledCommands.IsHandled(CommandId.Watermark), Is.False);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageContrast), Is.False);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectsRecolorGallery), Is.False);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectsSharpenGallery), Is.False);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectsBlurGallery), Is.False);
                Assert.That(HandledCommands.IsHandled(CommandId.ImageEffectsEmbossGallery), Is.False);
            });
        }

        [AvaloniaTest]
        public void PictureTab_EffectsDropdown_EnablesBakedEffectsOnly()
        {
            var ribbon = new AvaloniaRibbonControl { CommandFilter = HandledCommands.IsHandled };
            ribbon.LoadConfiguration(DefaultRibbonConfiguration.Create());
            ribbon.ActivateContextualTabGroup(RibbonContextualTabGroup.ImageTools);

            RibbonButtonControl button = ribbon.GetLogicalDescendants()
                .OfType<RibbonButtonControl>()
                .FirstOrDefault(b => b.CommandId == CommandId.ImageEffectsGallery);
            Assert.That(button, Is.Not.Null);
            Assert.That(button.IsEnabled, Is.True, "the effects dropdown parent stays enabled");
            Assert.That(button.Flyout, Is.Not.Null);

            var menuItems = ((MenuFlyout)button.Flyout).Items.OfType<MenuItem>().ToList();
            Assert.That(menuItems.Count, Is.EqualTo(6),
                "Black & White + Sepia plus the four still-disabled galleries (separator excluded)");

            var fired = new List<CommandId>();
            ribbon.CommandExecuted += (s, id) => fired.Add(id);

            Assert.That(menuItems[0].IsEnabled, Is.True, "Black & White is baked and enabled");
            menuItems[0].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.That(fired, Is.EqualTo(new[] { CommandId.ImageEffectBlackAndWhite }));

            Assert.That(menuItems[1].IsEnabled, Is.True, "Sepia is baked and enabled");
            menuItems[1].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.That(fired, Is.EqualTo(new[]
            {
                CommandId.ImageEffectBlackAndWhite, CommandId.ImageEffectSepiaTone
            }));

            Assert.That(menuItems.Skip(2), Has.All.Property(nameof(MenuItem.IsEnabled)).False,
                "recolor/sharpen/blur/emboss galleries stay disabled");
        }
    }
}
