// Temporary alpha probe - not for commit.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class AlphaProbeTests
    {
        // Exact copy of BridgedCommand.ScaleImage
        private static Image ScaleImage(Image source, int width, int height)
        {
            if (source == null) return null;
            if (source.Width == width && source.Height == height) return source;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Point, wrapMode);
                }
            }

            return destImage;
        }

        [Test]
        public void Probe()
        {
            var sb = new StringBuilder();
            var cmd = new Command(CommandId.FontBackgroundColor);
            var small = cmd.SmallImage;

            // Simulate: app draws the scaled-up large image (small scaled to 32)
            var scaled = (Bitmap)ScaleImage(small, 32, 32);
            // Now draw that onto a light face at 32px (what the renderer would do)
            var face = new Bitmap(96, 96, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(face))
            {
                g.Clear(Color.FromArgb(245, 246, 247));
                g.DrawImage(scaled, new Rectangle(32, 32, 32, 32), new Rectangle(0, 0, 32, 32), GraphicsUnit.Pixel);
            }
            for (int y = 32; y < 64; y += 2)
            {
                var row = new StringBuilder();
                for (int x = 32; x < 64; x += 2)
                {
                    var px = face.GetPixel(x, y);
                    int sum = px.R + px.G + px.B;
                    row.Append(sum > 690 ? '.' : sum < 240 ? '#' : '+');
                }
                sb.AppendLine(row.ToString());
            }
            File.WriteAllText(@"C:\olw-build\alpha-probe.txt", sb.ToString());
        }
    }
}
