using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RibbonTestHarness.Core;

/// <summary>
/// Provides screenshot capture functionality for windows and specific regions.
/// </summary>
public static class ScreenCapture
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    
    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
    
    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
    
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
    
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd); // Is minimized

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const int MDT_EFFECTIVE_DPI = 0;
    
    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;
    
    /// <summary>
    /// Captures a screenshot of the entire window
    /// </summary>
    public static Bitmap? CaptureWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("  CaptureWindow: Invalid window handle (Zero)");
            return null;
        }
        
        // Check if window is minimized and restore it
        if (IsIconic(hWnd))
        {
            Console.WriteLine("  CaptureWindow: Window is minimized, restoring...");
            ShowWindow(hWnd, SW_RESTORE);
            System.Threading.Thread.Sleep(300);
        }
        
        // Verify window is visible
        if (!IsWindowVisible(hWnd))
        {
            Console.WriteLine("  CaptureWindow: Window not visible, attempting to show...");
            ShowWindow(hWnd, SW_SHOW);
            System.Threading.Thread.Sleep(200);
        }
            
        if (!GetWindowRect(hWnd, out RECT rect))
        {
            Console.WriteLine("  CaptureWindow: Failed to get window rect");
            return null;
        }
            
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        
        Console.WriteLine($"  CaptureWindow: Handle=0x{hWnd:X}, Rect=({rect.Left},{rect.Top}) to ({rect.Right},{rect.Bottom}), Size={width}x{height}");
        
        if (width <= 0 || height <= 0)
        {
            Console.WriteLine("  CaptureWindow: Invalid dimensions");
            return null;
        }
        
        // Bring window to foreground and wait
        SetForegroundWindow(hWnd);
        System.Threading.Thread.Sleep(200); // Longer wait to ensure window is rendered
        
        // First try: CopyFromScreen (fastest and most compatible)
        try
        {
            // Re-fetch rect in case window moved
            GetWindowRect(hWnd, out rect);
            
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
            }
            
            // Verify the bitmap isn't completely black
            if (!IsBitmapAllBlack(bitmap))
            {
                return bitmap;
            }
            
            Console.WriteLine("  CaptureWindow: CopyFromScreen produced black image, trying PrintWindow...");
            bitmap.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  CaptureWindow: CopyFromScreen failed: {ex.Message}");
        }
        
        // Second try: PrintWindow with PW_RENDERFULLCONTENT
        try
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var hdc = graphics.GetHdc();
                // PW_RENDERFULLCONTENT = 2 (captures even when window is occluded)
                bool success = PrintWindow(hWnd, hdc, 2);
                graphics.ReleaseHdc(hdc);
                
                if (!success)
                {
                    Console.WriteLine("  CaptureWindow: PrintWindow(2) failed, trying without flag...");
                    bitmap.Dispose();
                    
                    // Third try: PrintWindow without flags
                    var bitmap2 = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    using (var graphics2 = Graphics.FromImage(bitmap2))
                    {
                        var hdc2 = graphics2.GetHdc();
                        PrintWindow(hWnd, hdc2, 0);
                        graphics2.ReleaseHdc(hdc2);
                    }
                    return bitmap2;
                }
            }
            return bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  CaptureWindow: PrintWindow failed: {ex.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Checks if a bitmap is completely black (or nearly so)
    /// </summary>
    private static bool IsBitmapAllBlack(Bitmap bitmap, int threshold = 10)
    {
        // Sample a few pixels to check if the image is all black
        int sampleCount = 0;
        int blackCount = 0;
        
        for (int x = 0; x < bitmap.Width; x += bitmap.Width / 10)
        {
            for (int y = 0; y < bitmap.Height; y += bitmap.Height / 10)
            {
                if (x < bitmap.Width && y < bitmap.Height)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    sampleCount++;
                    if (pixel.R < threshold && pixel.G < threshold && pixel.B < threshold)
                    {
                        blackCount++;
                    }
                }
            }
        }
        
        // If more than 95% of sampled pixels are black, consider it all black
        return sampleCount > 0 && (double)blackCount / sampleCount > 0.95;
    }
    
    /// <summary>
    /// Captures a screenshot of the window's client area
    /// </summary>
    public static Bitmap? CaptureClientArea(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return null;
            
        if (!GetClientRect(hWnd, out RECT clientRect))
            return null;
            
        POINT clientOrigin = new POINT { X = 0, Y = 0 };
        ClientToScreen(hWnd, ref clientOrigin);
        
        int width = clientRect.Right;
        int height = clientRect.Bottom;
        
        if (width <= 0 || height <= 0)
            return null;
            
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(clientOrigin.X, clientOrigin.Y, 0, 0, new Size(width, height));
        }
        
        return bitmap;
    }
    
    /// <summary>
    /// Captures a specific region of a window (e.g., the ribbon area)
    /// </summary>
    public static Bitmap? CaptureWindowRegion(IntPtr hWnd, Rectangle region)
    {
        var fullCapture = CaptureWindow(hWnd);
        if (fullCapture == null)
            return null;
            
        // Ensure region is within bounds
        region.Intersect(new Rectangle(0, 0, fullCapture.Width, fullCapture.Height));
        
        if (region.Width <= 0 || region.Height <= 0)
        {
            fullCapture.Dispose();
            return null;
        }
        
        var regionBitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(regionBitmap))
        {
            graphics.DrawImage(fullCapture, 
                new Rectangle(0, 0, region.Width, region.Height),
                region,
                GraphicsUnit.Pixel);
        }
        
        fullCapture.Dispose();
        return regionBitmap;
    }
    
    /// <summary>
    /// Captures the ribbon area of a window (top portion)
    /// </summary>
    public static Bitmap? CaptureRibbonArea(IntPtr hWnd, int ribbonHeight = 150)
    {
        if (!GetWindowRect(hWnd, out RECT rect))
            return null;
            
        int width = rect.Right - rect.Left;
        
        // The ribbon is typically in the top portion of the window
        // Include title bar + ribbon tabs + ribbon content
        return CaptureWindowRegion(hWnd, new Rectangle(0, 0, width, ribbonHeight));
    }
    
    /// <summary>
    /// Captures a window from a process
    /// </summary>
    public static Bitmap? CaptureProcess(Process? process)
    {
        if (process == null)
        {
            Console.WriteLine("  CaptureProcess: Process is null");
            return null;
        }
        
        if (process.HasExited)
        {
            Console.WriteLine("  CaptureProcess: Process has exited");
            return null;
        }
        
        // Refresh to ensure we have the latest window handle
        process.Refresh();
        
        var handle = process.MainWindowHandle;
        Console.WriteLine($"  CaptureProcess: ProcessId={process.Id}, ProcessName={process.ProcessName}, Handle=0x{handle:X}");
        
        if (handle == IntPtr.Zero)
        {
            Console.WriteLine("  CaptureProcess: MainWindowHandle is Zero - trying to find window...");
            // Try to find any visible window for this process
            handle = FindMainWindowForProcess(process.Id);
            if (handle != IntPtr.Zero)
            {
                Console.WriteLine($"  CaptureProcess: Found alternative window handle: 0x{handle:X}");
            }
        }
            
        return CaptureWindow(handle);
    }
    
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    
    /// <summary>
    /// Finds the main window for a process by enumerating all windows
    /// </summary>
    private static IntPtr FindMainWindowForProcess(int processId)
    {
        IntPtr foundHandle = IntPtr.Zero;
        
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint windowProcessId);
            if (windowProcessId == processId && IsWindowVisible(hWnd))
            {
                // Get window dimensions to find the largest visible window
                if (GetWindowRect(hWnd, out RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    
                    if (width > 100 && height > 100) // Minimum size for a main window
                    {
                        foundHandle = hWnd;
                        return false; // Stop enumeration
                    }
                }
            }
            return true; // Continue enumeration
        }, IntPtr.Zero);
        
        return foundHandle;
    }
    
    /// <summary>
    /// Gets the DPI scale factor for a window's monitor.
    /// </summary>
    public static double GetDpiScaleForWindow(IntPtr hwnd)
    {
        try
        {
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                int result = GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint _);
                if (result == 0) // S_OK
                    return dpiX / 96.0;
            }
        }
        catch { }

        // Fallback
        try
        {
            using var g = Graphics.FromHwnd(hwnd);
            return g.DpiX / 96.0;
        }
        catch { return 1.0; }
    }

    /// <summary>
    /// Calculates the DPI-aware ribbon capture height.
    /// Ribbon layout at 96 DPI: titlebar(31) + tabheight(24) + content(92) + grouplabel(16) = 163px.
    /// Add a small margin (17px) for borders/rounding = 180px base.
    /// </summary>
    public static int CalculateRibbonCaptureHeight(double dpiScale)
    {
        const int BASE_RIBBON_HEIGHT = 180; // at 100% DPI
        return (int)Math.Ceiling(BASE_RIBBON_HEIGHT * dpiScale);
    }

    /// <summary>
    /// Captures the ribbon area from a process with DPI-aware height calculation.
    /// </summary>
    public static Bitmap? CaptureProcessRibbon(Process? process, int ribbonHeight = 0)
    {
        if (process == null || process.HasExited)
            return null;

        // Refresh to ensure we have the latest window handle
        process.Refresh();

        var handle = process.MainWindowHandle;
        if (handle == IntPtr.Zero)
        {
            Console.WriteLine("  CaptureProcessRibbon: MainWindowHandle is Zero - trying to find window...");
            handle = FindMainWindowForProcess(process.Id);
            if (handle != IntPtr.Zero)
            {
                Console.WriteLine($"  CaptureProcessRibbon: Found alternative window handle: 0x{handle:X}");
            }
        }

        // Auto-calculate ribbon height based on DPI if not explicitly specified
        if (ribbonHeight <= 0)
        {
            var dpiScale = GetDpiScaleForWindow(handle);
            ribbonHeight = CalculateRibbonCaptureHeight(dpiScale);
            Console.WriteLine($"  CaptureProcessRibbon: DPI scale={dpiScale:F2}, calculated ribbon height={ribbonHeight}px");
        }

        return CaptureRibbonArea(handle, ribbonHeight);
    }
    
    /// <summary>
    /// Saves a bitmap to a file
    /// </summary>
    public static void SaveCapture(Bitmap bitmap, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var format = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".gif" => ImageFormat.Gif,
            ".bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Png
        };
        
        bitmap.Save(path, format);
    }
    
    /// <summary>
    /// Brings a window to the foreground
    /// </summary>
    public static void BringToForeground(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            SetForegroundWindow(hWnd);
        }
    }
    
    /// <summary>
    /// Brings a process window to the foreground
    /// </summary>
    public static void BringToForeground(Process? process)
    {
        if (process != null && !process.HasExited)
        {
            BringToForeground(process.MainWindowHandle);
        }
    }
    
    /// <summary>
    /// Gets the window rectangle
    /// </summary>
    public static Rectangle GetWindowBounds(IntPtr hWnd)
    {
        if (GetWindowRect(hWnd, out RECT rect))
        {
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
        return Rectangle.Empty;
    }
}
