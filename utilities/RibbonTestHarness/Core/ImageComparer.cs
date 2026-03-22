using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RibbonTestHarness.Core;

/// <summary>
/// Provides image comparison functionality for detecting visual differences between ribbons.
/// </summary>
public class ImageComparer
{
    /// <summary>
    /// Result of an image comparison
    /// </summary>
    public class ComparisonResult
    {
        public double SimilarityPercentage { get; set; }
        public int DifferentPixels { get; set; }
        public int TotalPixels { get; set; }
        public int MaxChannelDelta { get; set; }
        public double AvgDifferentPixelDelta { get; set; }
        public bool SizeMismatch { get; set; }
        public System.Drawing.Size Image1Size { get; set; }
        public System.Drawing.Size Image2Size { get; set; }
        public Bitmap? DiffImage { get; set; }
        public Dictionary<string, ComparisonResult>? RegionResults { get; set; }
        public bool AreIdentical => SimilarityPercentage >= 99.9;
        public bool AreVeryClose => SimilarityPercentage >= 95.0;
        public bool AreSimilar => SimilarityPercentage >= 90.0;

        public string Summary
        {
            get
            {
                var s = $"Similarity: {SimilarityPercentage:F2}% ({DifferentPixels:N0} of {TotalPixels:N0} pixels differ)";
                if (SizeMismatch)
                    s += $" [SIZE MISMATCH: {Image1Size.Width}x{Image1Size.Height} vs {Image2Size.Width}x{Image2Size.Height}]";
                if (DifferentPixels > 0)
                    s += $" [MaxDelta={MaxChannelDelta}, AvgDelta={AvgDifferentPixelDelta:F1}]";
                return s;
            }
        }
    }
    
    /// <summary>
    /// Compares two bitmaps and returns the similarity percentage and diff image
    /// </summary>
    public ComparisonResult Compare(Bitmap? image1, Bitmap? image2, int colorTolerance = 20)
    {
        var result = new ComparisonResult();

        if (image1 == null || image2 == null)
        {
            result.SimilarityPercentage = image1 == image2 ? 100 : 0;
            return result;
        }

        result.Image1Size = image1.Size;
        result.Image2Size = image2.Size;
        result.SizeMismatch = image1.Width != image2.Width || image1.Height != image2.Height;

        if (result.SizeMismatch)
        {
            Console.WriteLine($"  WARNING: Image size mismatch: {image1.Width}x{image1.Height} vs {image2.Width}x{image2.Height}");
        }

        int width = Math.Min(image1.Width, image2.Width);
        int height = Math.Min(image1.Height, image2.Height);

        if (width == 0 || height == 0)
        {
            result.SimilarityPercentage = 0;
            return result;
        }

        var diffBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        int differentPixels = 0;
        int totalPixels = width * height;
        int maxChannelDelta = 0;
        long totalDelta = 0;

        var rect = new System.Drawing.Rectangle(0, 0, width, height);

        var bmpData1 = image1.LockBits(new System.Drawing.Rectangle(0, 0, image1.Width, image1.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var bmpData2 = image2.LockBits(new System.Drawing.Rectangle(0, 0, image2.Width, image2.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var diffData = diffBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        unsafe
        {
            for (int y = 0; y < height; y++)
            {
                byte* row1 = (byte*)bmpData1.Scan0 + (y * bmpData1.Stride);
                byte* row2 = (byte*)bmpData2.Scan0 + (y * bmpData2.Stride);
                byte* rowDiff = (byte*)diffData.Scan0 + (y * diffData.Stride);

                for (int x = 0; x < width; x++)
                {
                    int idx = x * 4;

                    int b1 = row1[idx], g1 = row1[idx + 1], r1 = row1[idx + 2];
                    int b2 = row2[idx], g2 = row2[idx + 1], r2 = row2[idx + 2];

                    int diffR = Math.Abs(r1 - r2);
                    int diffG = Math.Abs(g1 - g2);
                    int diffB = Math.Abs(b1 - b2);
                    int pixelMax = Math.Max(diffR, Math.Max(diffG, diffB));

                    bool isDifferent = diffR > colorTolerance || diffG > colorTolerance || diffB > colorTolerance;

                    if (isDifferent)
                    {
                        differentPixels++;
                        totalDelta += pixelMax;
                        if (pixelMax > maxChannelDelta)
                            maxChannelDelta = pixelMax;

                        // Highlight differences in red with the actual diff intensity
                        rowDiff[idx] = 0; // B
                        rowDiff[idx + 1] = 0; // G
                        rowDiff[idx + 2] = (byte)Math.Min(255, 128 + pixelMax); // R
                        rowDiff[idx + 3] = 255; // A
                    }
                    else
                    {
                        // Show grayscale version of original
                        int gray = (r1 + g1 + b1) / 3;
                        rowDiff[idx] = (byte)gray;
                        rowDiff[idx + 1] = (byte)gray;
                        rowDiff[idx + 2] = (byte)gray;
                        rowDiff[idx + 3] = 255;
                    }
                }
            }
        }

        image1.UnlockBits(bmpData1);
        image2.UnlockBits(bmpData2);
        diffBitmap.UnlockBits(diffData);

        result.TotalPixels = totalPixels;
        result.DifferentPixels = differentPixels;
        result.MaxChannelDelta = maxChannelDelta;
        result.AvgDifferentPixelDelta = differentPixels > 0 ? (double)totalDelta / differentPixels : 0;
        result.SimilarityPercentage = 100.0 * (totalPixels - differentPixels) / totalPixels;
        result.DiffImage = diffBitmap;

        return result;
    }
    
    /// <summary>
    /// Compares named regions of two bitmaps independently
    /// </summary>
    public ComparisonResult CompareRegions(Bitmap? image1, Bitmap? image2, Dictionary<string, System.Drawing.Rectangle> regions, int colorTolerance = 20)
    {
        // Do the full comparison first
        var fullResult = Compare(image1, image2, colorTolerance);
        fullResult.RegionResults = new Dictionary<string, ComparisonResult>();

        if (image1 == null || image2 == null)
            return fullResult;

        foreach (var (name, region) in regions)
        {
            // Clamp region to both image bounds
            int cropWidth = Math.Min(region.Width, Math.Min(image1.Width, image2.Width) - region.X);
            int cropHeight = Math.Min(region.Height, Math.Min(image1.Height, image2.Height) - region.Y);

            if (cropWidth <= 0 || cropHeight <= 0 || region.X < 0 || region.Y < 0)
            {
                Console.WriteLine($"  Region '{name}' out of bounds, skipping");
                continue;
            }

            var cropRect = new System.Drawing.Rectangle(region.X, region.Y, cropWidth, cropHeight);

            using var crop1 = CropBitmap(image1, cropRect);
            using var crop2 = CropBitmap(image2, cropRect);

            var regionResult = Compare(crop1, crop2, colorTolerance);
            // Don't keep diff bitmaps for regions to save memory
            regionResult.DiffImage?.Dispose();
            regionResult.DiffImage = null;

            fullResult.RegionResults[name] = regionResult;
        }

        return fullResult;
    }

    /// <summary>
    /// Crops a region from a bitmap
    /// </summary>
    private static Bitmap CropBitmap(Bitmap source, System.Drawing.Rectangle region)
    {
        var cropped = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(cropped);
        g.DrawImage(source,
            new System.Drawing.Rectangle(0, 0, region.Width, region.Height),
            region,
            GraphicsUnit.Pixel);
        return cropped;
    }

    /// <summary>
    /// Creates a side-by-side comparison image
    /// </summary>
    public Bitmap CreateSideBySide(Bitmap? image1, Bitmap? image2, string label1 = "Installed", string label2 = "Dev")
    {
        if (image1 == null && image2 == null)
        {
            return new Bitmap(100, 100);
        }
        
        int width1 = image1?.Width ?? 0;
        int width2 = image2?.Width ?? 0;
        int height1 = image1?.Height ?? 0;
        int height2 = image2?.Height ?? 0;
        
        int totalWidth = width1 + width2 + 10; // 10px gap
        int maxHeight = Math.Max(height1, height2) + 30; // Space for labels
        
        var result = new Bitmap(totalWidth, maxHeight, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(System.Drawing.Color.White);
            
            // Draw labels
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(System.Drawing.Color.Black))
            {
                g.DrawString(label1, font, brush, 5, 5);
                g.DrawString(label2, font, brush, width1 + 15, 5);
            }
            
            // Draw images
            if (image1 != null)
            {
                g.DrawImage(image1, 0, 25);
            }
            if (image2 != null)
            {
                g.DrawImage(image2, width1 + 10, 25);
            }
            
            // Draw separator
            using (var pen = new Pen(System.Drawing.Color.Gray, 2))
            {
                g.DrawLine(pen, width1 + 5, 0, width1 + 5, maxHeight);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Creates a three-panel comparison: Installed | Dev | Diff
    /// </summary>
    public Bitmap CreateComparisonPanel(Bitmap? image1, Bitmap? image2, Bitmap? diffImage)
    {
        int width = image1?.Width ?? image2?.Width ?? 100;
        int height = image1?.Height ?? image2?.Height ?? 100;
        
        int totalWidth = width * 3 + 20; // gaps
        int totalHeight = height + 40; // labels
        
        var result = new Bitmap(totalWidth, totalHeight, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(System.Drawing.Color.White);
            
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(System.Drawing.Color.Black))
            {
                g.DrawString("Installed (Original)", font, brush, 5, 5);
                g.DrawString("Development (New)", font, brush, width + 15, 5);
                g.DrawString("Differences", font, brush, width * 2 + 25, 5);
            }
            
            if (image1 != null)
                g.DrawImage(image1, 0, 35);
            if (image2 != null)
                g.DrawImage(image2, width + 10, 35);
            if (diffImage != null)
                g.DrawImage(diffImage, width * 2 + 20, 35);
                
            using (var pen = new Pen(System.Drawing.Color.Gray, 1))
            {
                g.DrawLine(pen, width + 5, 0, width + 5, totalHeight);
                g.DrawLine(pen, width * 2 + 15, 0, width * 2 + 15, totalHeight);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Saves a comparison report with all images
    /// </summary>
    public void SaveComparisonReport(string outputDir, string testName, Bitmap? installed, Bitmap? dev, ComparisonResult result)
    {
        Directory.CreateDirectory(outputDir);
        
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var prefix = $"{testName}_{timestamp}";
        
        if (installed != null)
            ScreenCapture.SaveCapture(installed, Path.Combine(outputDir, $"{prefix}_installed.png"));
        if (dev != null)
            ScreenCapture.SaveCapture(dev, Path.Combine(outputDir, $"{prefix}_dev.png"));
        if (result.DiffImage != null)
            ScreenCapture.SaveCapture(result.DiffImage, Path.Combine(outputDir, $"{prefix}_diff.png"));
            
        // Create side-by-side
        var sideBySide = CreateComparisonPanel(installed, dev, result.DiffImage);
        ScreenCapture.SaveCapture(sideBySide, Path.Combine(outputDir, $"{prefix}_comparison.png"));
        sideBySide.Dispose();
        
        // Write text report
        var reportPath = Path.Combine(outputDir, $"{prefix}_report.txt");
        File.WriteAllText(reportPath, $@"Ribbon Comparison Report
========================
Test: {testName}
Date: {DateTime.Now}

Result: {result.Summary}
Status: {(result.AreIdentical ? "PASS - Identical" : result.AreVeryClose ? "CLOSE - Minor differences" : "FAIL - Significant differences")}

Details:
- Similarity: {result.SimilarityPercentage:F2}%
- Different Pixels: {result.DifferentPixels:N0}
- Total Pixels: {result.TotalPixels:N0}
- Max Channel Delta: {result.MaxChannelDelta}
- Avg Different Pixel Delta: {result.AvgDifferentPixelDelta:F1}
- Size Mismatch: {result.SizeMismatch}
- Image1 Size: {result.Image1Size.Width}x{result.Image1Size.Height}
- Image2 Size: {result.Image2Size.Width}x{result.Image2Size.Height}
");

        // Write JSON report
        var jsonReport = new Dictionary<string, object>
        {
            ["testName"] = testName,
            ["timestamp"] = DateTime.Now.ToString("o"),
            ["similarity"] = Math.Round(result.SimilarityPercentage, 2),
            ["differentPixels"] = result.DifferentPixels,
            ["totalPixels"] = result.TotalPixels,
            ["maxChannelDelta"] = result.MaxChannelDelta,
            ["avgDifferentPixelDelta"] = Math.Round(result.AvgDifferentPixelDelta, 1),
            ["sizeMismatch"] = result.SizeMismatch,
            ["image1Size"] = $"{result.Image1Size.Width}x{result.Image1Size.Height}",
            ["image2Size"] = $"{result.Image2Size.Width}x{result.Image2Size.Height}",
            ["status"] = result.AreIdentical ? "PASS" : result.AreVeryClose ? "CLOSE" : "FAIL",
        };

        if (result.RegionResults != null)
        {
            var regionData = new Dictionary<string, object>();
            foreach (var (regionName, regionResult) in result.RegionResults)
            {
                regionData[regionName] = new Dictionary<string, object>
                {
                    ["similarity"] = Math.Round(regionResult.SimilarityPercentage, 2),
                    ["differentPixels"] = regionResult.DifferentPixels,
                    ["totalPixels"] = regionResult.TotalPixels,
                    ["maxChannelDelta"] = regionResult.MaxChannelDelta,
                    ["avgDifferentPixelDelta"] = Math.Round(regionResult.AvgDifferentPixelDelta, 1),
                    ["status"] = regionResult.SimilarityPercentage >= 95 ? "PASS" : regionResult.SimilarityPercentage >= 90 ? "CLOSE" : "FAIL",
                };
            }
            jsonReport["regions"] = regionData;
        }

        var jsonPath = Path.Combine(outputDir, $"{prefix}_report.json");
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(jsonReport, jsonOptions));
    }
}
