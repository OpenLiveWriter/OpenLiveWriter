using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;

namespace RibbonTestHarness.Core;

/// <summary>
/// Provides UI Automation functionality for interacting with Open Live Writer.
/// Uses FlaUI for DPI-aware mouse/keyboard operations.
/// </summary>
public class UIAutomationHelper
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    
    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    
    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);
    
    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const int MDT_EFFECTIVE_DPI = 0;
    
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    private AutomationElement? _installedRoot;
    private AutomationElement? _devRoot;
    private double _dpiScale = 1.0;
    
    public AutomationElement? InstalledRoot => _installedRoot;
    public AutomationElement? DevRoot => _devRoot;
    public double DpiScale => _dpiScale;
    
    /// <summary>
    /// Attaches to the installed app window
    /// </summary>
    public void AttachToInstalled(Process? process)
    {
        if (process == null || process.HasExited)
            return;
            
        process.WaitForInputIdle(5000);
        _installedRoot = AutomationElement.FromHandle(process.MainWindowHandle);
    }
    
    /// <summary>
    /// Attaches to the dev app window
    /// </summary>
    public void AttachToDev(Process? process)
    {
        if (process == null || process.HasExited)
            return;
            
        process.WaitForInputIdle(5000);
        _devRoot = AutomationElement.FromHandle(process.MainWindowHandle);
        
        // Detect DPI scaling for this window
        _dpiScale = GetDpiScaleForWindow(process.MainWindowHandle);
        Console.WriteLine($"  Detected DPI scale: {_dpiScale:F2} ({_dpiScale * 100:F0}%)");
    }
    
    /// <summary>
    /// Gets the DPI scale factor for a window
    /// </summary>
    private double GetDpiScaleForWindow(IntPtr hwnd)
    {
        try
        {
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                int result = GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
                if (result == 0) // S_OK
                {
                    return dpiX / 96.0; // 96 DPI is 100% scale
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Could not detect DPI: {ex.Message}");
        }
        
        // Fallback: try to detect from Graphics
        try
        {
            using var g = Graphics.FromHwnd(hwnd);
            return g.DpiX / 96.0;
        }
        catch
        {
            return 1.0; // Default to 100%
        }
    }
    
    /// <summary>
    /// Positions two windows side by side on the screen
    /// </summary>
    public void PositionWindowsSideBySide(IntPtr installed, IntPtr dev)
    {
        var screenBounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
        int halfWidth = screenBounds.Width / 2;
        int height = screenBounds.Height - 50; // Leave room for taskbar
        
        // Position installed on the left
        MoveWindow(installed, 0, 0, halfWidth, height, true);
        
        // Position dev on the right
        MoveWindow(dev, halfWidth, 0, halfWidth, height, true);
    }
    
    /// <summary>
    /// Resizes both windows to the same size for accurate comparison
    /// </summary>
    public void ResizeWindowsToMatch(IntPtr installed, IntPtr dev, int width = 1200, int height = 800)
    {
        var screenBounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
        
        // Position installed on the left
        MoveWindow(installed, 50, 50, width, height, true);
        
        // Position dev on the right
        MoveWindow(dev, 50 + width + 20, 50, width, height, true);
        
        // Allow windows to settle
        Thread.Sleep(100);
        
        // Verify and correct if sizes don't match exactly
        var installedSize = GetWindowSize(installed);
        var devSize = GetWindowSize(dev);
        
        if (installedSize != devSize)
        {
            // Force dev window to exactly match installed window's actual size
            // by calculating the offset and adjusting
            int widthDiff = installedSize.Width - devSize.Width;
            int heightDiff = installedSize.Height - devSize.Height;
            
            if (widthDiff != 0 || heightDiff != 0)
            {
                // Resize dev window with the correction
                MoveWindow(dev, 50 + width + 20, 50, width + widthDiff, height + heightDiff, true);
                Thread.Sleep(50);
            }
        }
    }
    
    /// <summary>
    /// Gets the window size
    /// </summary>
    public Size GetWindowSize(IntPtr hWnd)
    {
        if (GetWindowRect(hWnd, out RECT rect))
        {
            return new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
        return Size.Empty;
    }
    
    /// <summary>
    /// Finds a UI element by name
    /// </summary>
    public AutomationElement? FindElementByName(AutomationElement? root, string name)
    {
        if (root == null)
            return null;
            
        var condition = new PropertyCondition(AutomationElement.NameProperty, name);
        return root.FindFirst(TreeScope.Descendants, condition);
    }
    
    /// <summary>
    /// Finds a UI element by automation ID
    /// </summary>
    public AutomationElement? FindElementById(AutomationElement? root, string automationId)
    {
        if (root == null)
            return null;
            
        var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        return root.FindFirst(TreeScope.Descendants, condition);
    }
    
    /// <summary>
    /// Finds all ribbon tabs
    /// </summary>
    public List<AutomationElement> FindRibbonTabs(AutomationElement? root)
    {
        var tabs = new List<AutomationElement>();
        if (root == null)
            return tabs;
            
        // Look for tab items
        var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
        var elements = root.FindAll(TreeScope.Descendants, condition);
        
        foreach (AutomationElement element in elements)
        {
            tabs.Add(element);
        }
        
        return tabs;
    }
    
    /// <summary>
    /// Finds all buttons in the ribbon
    /// </summary>
    public List<AutomationElement> FindRibbonButtons(AutomationElement? root)
    {
        var buttons = new List<AutomationElement>();
        if (root == null)
            return buttons;
            
        var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);
        var elements = root.FindAll(TreeScope.Descendants, condition);
        
        foreach (AutomationElement element in elements)
        {
            buttons.Add(element);
        }
        
        return buttons;
    }
    
    /// <summary>
    /// Clicks an element using the Invoke pattern
    /// </summary>
    public bool ClickElement(AutomationElement? element)
    {
        if (element == null)
            return false;
            
        try
        {
            if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? pattern))
            {
                ((InvokePattern)pattern).Invoke();
                return true;
            }
            
            // Fall back to mouse click
            return ClickElementByMouse(element);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Clicks an element using mouse simulation (FlaUI handles DPI scaling)
    /// </summary>
    public bool ClickElementByMouse(AutomationElement? element)
    {
        if (element == null)
            return false;
            
        try
        {
            var rect = element.Current.BoundingRectangle;
            if (rect.IsEmpty)
                return false;
            
            // Get center of element - FlaUI will handle DPI scaling
            int x = (int)(rect.X + rect.Width / 2);
            int y = (int)(rect.Y + rect.Height / 2);
            
            Console.WriteLine($"  ClickElementByMouse: Clicking at ({x}, {y}) - element bounds ({rect.X:F0},{rect.Y:F0}) {rect.Width:F0}x{rect.Height:F0}");
            ClickAtPosition(x, y);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ClickElementByMouse failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Clicks at a specific screen position using FlaUI (handles DPI scaling)
    /// </summary>
    public void ClickAtPosition(int x, int y)
    {
        // FlaUI's Mouse class handles DPI scaling automatically
        Mouse.MoveTo(x, y);
        Thread.Sleep(50);
        Mouse.Click();
        Thread.Sleep(50);
    }
    
    /// <summary>
    /// Sends the Escape key to close menus/dialogs using FlaUI
    /// </summary>
    public void SendEscapeKey()
    {
        Keyboard.Press(VirtualKeyShort.ESCAPE);
        Thread.Sleep(50);
        Keyboard.Release(VirtualKeyShort.ESCAPE);
    }
    
    /// <summary>
    /// Clicks a ribbon tab by name. For the managed ribbon, tabs are custom-drawn
    /// so we need to click by position rather than UI Automation.
    /// </summary>
    public async Task<bool> ClickRibbonTabAsync(AutomationElement? root, string tabName, int delayAfterClick = 500)
    {
        if (root == null)
            return false;
            
        var windowRect = root.Current.BoundingRectangle;
        Console.WriteLine($"  ClickRibbonTabAsync: Window rect = ({windowRect.Left}, {windowRect.Top}) size {windowRect.Width}x{windowRect.Height}, DPI={_dpiScale:F2}");
        
        // First try to find by UI Automation (works for native ribbon)
        var tab = FindElementByName(root, tabName);
        if (tab != null)
        {
            var rect = tab.Current.BoundingRectangle;
            // Check if this is actually a ribbon tab (near top of window, not in content area)
            // Use DPI-scaled threshold
            var topThreshold = 80 * _dpiScale;
            if (rect.Top < windowRect.Top + topThreshold && rect.Width > 30 && rect.Height > 15)
            {
                Console.WriteLine($"  Found tab '{tabName}' via UI Automation at ({rect.X}, {rect.Y}) size {rect.Width}x{rect.Height}");
                var result = ClickElement(tab);
                if (result)
                {
                    await Task.Delay(delayAfterClick);
                    return true;
                }
            }
        }
        
        // Fall back to clicking by position for managed ribbon
        // The managed ribbon has different layout than native:
        // - Tabs start at X=150 (logical) after File button and QAT
        // - Tab header is inside RibbonPanel which starts at client area top
        // - At 150% DPI, the title bar is ~31*1.5=46 physical pixels
        //
        // Managed ribbon tab positions (in logical pixels from window top-left):
        // - Title bar: ~31 logical (at any DPI, since it's non-client area)
        // - Tab header top: right at client area top, so ~31 from window top
        // - Tab center Y: ~31 + 13 (half of 26px header) = 44 logical
        // - Tab X starts at 150 in control coords, but control left edge is at window left
        //   So Home tab center: 150 + ~30 (half of tab width) = 180 logical
        
        var tabLogicalPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Home", 180 },       // Managed ribbon: tabStartX(150) + half tab width (~30)
            { "Insert", 240 },     // Home(180) + tab width (~50) + spacing (~10)
            { "Blog Account", 330 }, // Insert(240) + tab width (~50) + spacing + wider tab
            { "Debug", 410 }       // Blog Account + tab width
        };
        
        // Tab Y: title bar (~31 logical) + half tab header height (~13 logical) = 44 logical
        const int tabLogicalY = 44;
        
        if (tabLogicalPositions.TryGetValue(tabName, out var tabLogicalX))
        {
            // Scale logical coordinates to physical coordinates
            var clickX = (int)(windowRect.Left + tabLogicalX * _dpiScale);
            var clickY = (int)(windowRect.Top + tabLogicalY * _dpiScale);
            
            // Bring window to foreground first
            var hwnd = new IntPtr(root.Current.NativeWindowHandle);
            SetForegroundWindow(hwnd);
            await Task.Delay(100);
            
            Console.WriteLine($"  Clicking tab '{tabName}' at physical position ({clickX}, {clickY}) [logical: ({tabLogicalX}, {tabLogicalY})]");
            ClickAtPosition(clickX, clickY);
            await Task.Delay(delayAfterClick);
            return true;
        }
        
        // Last resort: try finding TabItem controls but filter to only ones near the top
        var tabs = FindRibbonTabs(root);
        foreach (var t in tabs)
        {
            if (t.Current.Name.Contains(tabName, StringComparison.OrdinalIgnoreCase))
            {
                var rect = t.Current.BoundingRectangle;
                var topThreshold = 80 * _dpiScale;
                if (rect.Top < windowRect.Top + topThreshold)
                {
                    Console.WriteLine($"  Found tab '{tabName}' via UI Automation search at ({rect.X}, {rect.Y})");
                    var result = ClickElement(t);
                    if (result)
                    {
                        await Task.Delay(delayAfterClick);
                        return true;
                    }
                }
            }
        }
        
        Console.WriteLine($"  WARNING: Could not find or click tab '{tabName}'");
        return false;
    }
    
    /// <summary>
    /// Clicks the File menu button on the ribbon.
    /// Always uses mouse click because InvokePattern doesn't properly trigger the ApplicationMenu.
    /// </summary>
    public async Task<bool> ClickFileMenuAsync(AutomationElement? root, int delayAfterClick = 500)
    {
        if (root == null)
            return false;
            
        var windowRect = root.Current.BoundingRectangle;
        Console.WriteLine($"  ClickFileMenuAsync: Window rect = ({windowRect.Left}, {windowRect.Top}) size {windowRect.Width}x{windowRect.Height}, DPI={_dpiScale:F2}");
        
        // Bring window to foreground first
        var hwnd = new IntPtr(root.Current.NativeWindowHandle);
        SetForegroundWindow(hwnd);
        await Task.Delay(100);
        
        // Try to find by name first - use mouse click (not InvokePattern) to properly trigger menu
        var fileBtn = FindElementByName(root, "File");
        if (fileBtn != null)
        {
            var rect = fileBtn.Current.BoundingRectangle;
            // File button should be at the very top left of the ribbon (use DPI-scaled thresholds)
            var topThreshold = 80 * _dpiScale;
            var leftThreshold = 100 * _dpiScale;
            if (rect.Top < windowRect.Top + topThreshold && rect.Left < windowRect.Left + leftThreshold)
            {
                Console.WriteLine($"  Found File button via UI Automation at ({rect.X}, {rect.Y}) size {rect.Width}x{rect.Height}");
                // Always use mouse click for File button - InvokePattern doesn't trigger ApplicationMenu properly
                var result = ClickElementByMouse(fileBtn);
                if (result)
                {
                    await Task.Delay(delayAfterClick);
                    return true;
                }
            }
        }
        
        // Click by position for managed ribbon (using DPI-scaled coordinates)
        // File button in managed ribbon:
        // - Located at X=2, Y=1 within TabHeaderPanel (control coordinates)
        // - Width=54 (PopupWidth), Height=24 (TabHeight - 2)
        // - TabHeaderPanel is at client area origin (0,0)
        // - Window has title bar (~31 logical) + thin border (~1-2 logical)
        // Center of File button from window top-left:
        // - X = border(~1) + 2 + 54/2 = 1 + 2 + 27 = 30 logical
        // - Y = titlebar(~31) + 1 + 24/2 = 31 + 1 + 12 = 44 logical
        const int fileLogicalX = 30;
        const int fileLogicalY = 44;
        
        // Scale logical coordinates to physical coordinates
        var clickX = (int)(windowRect.Left + fileLogicalX * _dpiScale);
        var clickY = (int)(windowRect.Top + fileLogicalY * _dpiScale);
        
        Console.WriteLine($"  Clicking File button at physical position ({clickX}, {clickY}) [logical: ({fileLogicalX}, {fileLogicalY})]");
        ClickAtPosition(clickX, clickY);
        await Task.Delay(delayAfterClick);
        return true;
    }
    
    /// <summary>
    /// Gets all element names in the tree (for debugging)
    /// </summary>
    public List<string> GetAllElementNames(AutomationElement? root)
    {
        var names = new List<string>();
        if (root == null)
            return names;
            
        var walker = TreeWalker.ContentViewWalker;
        CollectNames(walker, root, names, 0);
        return names;
    }
    
    private void CollectNames(TreeWalker walker, AutomationElement element, List<string> names, int depth)
    {
        if (depth > 10) // Prevent infinite recursion
            return;
            
        try
        {
            var name = element.Current.Name;
            var controlType = element.Current.ControlType.ProgrammaticName;
            names.Add($"{new string(' ', depth * 2)}{controlType}: {name}");
            
            var child = walker.GetFirstChild(element);
            while (child != null)
            {
                CollectNames(walker, child, names, depth + 1);
                child = walker.GetNextSibling(child);
            }
        }
        catch { }
    }
    
    /// <summary>
    /// Brings the window to the foreground
    /// </summary>
    public void BringToForeground(AutomationElement? element)
    {
        if (element == null)
            return;
            
        try
        {
            var hwnd = new IntPtr(element.Current.NativeWindowHandle);
            SetForegroundWindow(hwnd);
        }
        catch { }
    }
    
    /// <summary>
    /// Gets the bounding rectangle of an element
    /// </summary>
    public Rectangle GetElementBounds(AutomationElement? element)
    {
        if (element == null)
            return Rectangle.Empty;
            
        try
        {
            var rect = element.Current.BoundingRectangle;
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
        catch
        {
            return Rectangle.Empty;
        }
    }
}
