using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        public Bitmap? DiffImage { get; set; }
        public bool AreIdentical => SimilarityPercentage >= 99.9;
        public bool AreVeryClose => SimilarityPercentage >= 95.0;
        public bool AreSimilar => SimilarityPercentage >= 90.0;
        
        public string Summary => $"Similarity: {SimilarityPercentage:F2}% ({DifferentPixels:N0} of {TotalPixels:N0} pixels differ)";
    }
    
    /// <summary>
    /// Compares two bitmaps and returns the similarity percentage and diff image
    /// </summary>
    public ComparisonResult Compare(Bitmap? image1, Bitmap? image2, int colorTolerance = 10)
    {
        var result = new ComparisonResult();
        
        if (image1 == null || image2 == null)
        {
            result.SimilarityPercentage = image1 == image2 ? 100 : 0;
            return result;
        }
        
        // Resize images to match if needed
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
        
        // Lock bits for faster access
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
                    int idx1 = x * 4;
                    int idx2 = x * 4;
                    int idxDiff = x * 4;
                    
                    int b1 = row1[idx1], g1 = row1[idx1 + 1], r1 = row1[idx1 + 2];
                    int b2 = row2[idx2], g2 = row2[idx2 + 1], r2 = row2[idx2 + 2];
                    
                    int diffR = Math.Abs(r1 - r2);
                    int diffG = Math.Abs(g1 - g2);
                    int diffB = Math.Abs(b1 - b2);
                    
                    bool isDifferent = diffR > colorTolerance || diffG > colorTolerance || diffB > colorTolerance;
                    
                    if (isDifferent)
                    {
                        differentPixels++;
                        // Highlight differences in red with the actual diff intensity
                        int intensity = Math.Max(diffR, Math.Max(diffG, diffB));
                        rowDiff[idxDiff] = 0; // B
                        rowDiff[idxDiff + 1] = 0; // G
                        rowDiff[idxDiff + 2] = (byte)Math.Min(255, 128 + intensity); // R
                        rowDiff[idxDiff + 3] = 255; // A
                    }
                    else
                    {
                        // Show grayscale version of original
                        int gray = (r1 + g1 + b1) / 3;
                        rowDiff[idxDiff] = (byte)gray;
                        rowDiff[idxDiff + 1] = (byte)gray;
                        rowDiff[idxDiff + 2] = (byte)gray;
                        rowDiff[idxDiff + 3] = 255;
                    }
                }
            }
        }
        
        image1.UnlockBits(bmpData1);
        image2.UnlockBits(bmpData2);
        diffBitmap.UnlockBits(diffData);
        
        result.TotalPixels = totalPixels;
        result.DifferentPixels = differentPixels;
        result.SimilarityPercentage = 100.0 * (totalPixels - differentPixels) / totalPixels;
        result.DiffImage = diffBitmap;
        
        return result;
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
");
    }
}
