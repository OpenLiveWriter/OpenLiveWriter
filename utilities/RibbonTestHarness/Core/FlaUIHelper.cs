using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace RibbonTestHarness.Core;

/// <summary>
/// FlaUI-based UI automation helper that handles DPI scaling and provides
/// robust element discovery and interaction.
/// </summary>
public class FlaUIHelper : IDisposable
{
    private readonly UIA3Automation _automation;
    private FlaUI.Core.Application? _installedApp;
    private FlaUI.Core.Application? _devApp;
    private Window? _installedWindow;
    private Window? _devWindow;
    private double _dpiScale = 1.0;

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const int MDT_EFFECTIVE_DPI = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    public Window? InstalledWindow => _installedWindow;
    public Window? DevWindow => _devWindow;
    public double DpiScale => _dpiScale;

    public FlaUIHelper()
    {
        _automation = new UIA3Automation();
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
            Console.WriteLine($"  FlaUI: Warning - Could not detect DPI: {ex.Message}");
        }
        return 1.0; // Default to 100%
    }

    /// <summary>
    /// Attaches to the installed app process
    /// </summary>
    public void AttachToInstalled(Process? process)
    {
        if (process == null || process.HasExited)
            return;

        _installedApp = FlaUI.Core.Application.Attach(process);
        _installedWindow = _installedApp.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        Console.WriteLine($"  FlaUI: Attached to installed app, window title: {_installedWindow?.Title}");
    }

    /// <summary>
    /// Attaches to the dev app process
    /// </summary>
    public void AttachToDev(Process? process)
    {
        if (process == null || process.HasExited)
            return;

        _devApp = FlaUI.Core.Application.Attach(process);
        _devWindow = _devApp.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        
        // Detect DPI scaling for this window
        if (_devWindow != null)
        {
            var hwnd = _devWindow.Properties.NativeWindowHandle.Value;
            _dpiScale = GetDpiScaleForWindow(hwnd);
        }
        
        Console.WriteLine($"  FlaUI: Attached to dev app, window title: {_devWindow?.Title}, DPI scale: {_dpiScale:F2} ({_dpiScale * 100:F0}%)");
    }

    /// <summary>
    /// Resizes both windows to the same size for accurate comparison
    /// </summary>
    public void ResizeWindowsToMatch(int width = 1200, int height = 800)
    {
        if (_installedWindow != null)
        {
            var handle = _installedWindow.Properties.NativeWindowHandle.Value;
            MoveWindow(handle, 50, 50, width, height, true);
        }

        if (_devWindow != null)
        {
            var handle = _devWindow.Properties.NativeWindowHandle.Value;
            MoveWindow(handle, 50 + width + 20, 50, width, height, true);
        }

        Thread.Sleep(200); // Allow windows to settle
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
    /// Finds a UI element by name using FlaUI's robust search
    /// </summary>
    public AutomationElement? FindElementByName(Window? window, string name)
    {
        if (window == null) return null;

        var condition = _automation.ConditionFactory.ByName(name);
        return window.FindFirstDescendant(condition);
    }

    /// <summary>
    /// Finds a UI element by automation ID
    /// </summary>
    public AutomationElement? FindElementById(Window? window, string automationId)
    {
        if (window == null) return null;

        var condition = _automation.ConditionFactory.ByAutomationId(automationId);
        return window.FindFirstDescendant(condition);
    }

    /// <summary>
    /// Finds all ribbon tabs (TabItem controls)
    /// </summary>
    public List<AutomationElement> FindRibbonTabs(Window? window)
    {
        if (window == null) return new List<AutomationElement>();

        var condition = _automation.ConditionFactory.ByControlType(ControlType.TabItem);
        var tabs = window.FindAllDescendants(condition);
        
        // Filter to tabs near the top of the window (ribbon area)
        var windowBounds = window.BoundingRectangle;
        return tabs
            .Where(t => t.BoundingRectangle.Top < windowBounds.Top + 100)
            .ToList();
    }

    /// <summary>
    /// Finds all buttons in the window
    /// </summary>
    public List<AutomationElement> FindAllButtons(Window? window)
    {
        if (window == null) return new List<AutomationElement>();

        var condition = _automation.ConditionFactory.ByControlType(ControlType.Button);
        return window.FindAllDescendants(condition).ToList();
    }

    /// <summary>
    /// Finds all buttons in the ribbon area (top portion of window)
    /// </summary>
    public List<AutomationElement> FindRibbonButtons(Window? window)
    {
        if (window == null) return new List<AutomationElement>();

        var buttons = FindAllButtons(window);
        var windowBounds = window.BoundingRectangle;
        
        // Filter to buttons in the ribbon area (within ~150px of top)
        return buttons
            .Where(b => b.BoundingRectangle.Top < windowBounds.Top + 150)
            .ToList();
    }

    /// <summary>
    /// Clicks an element using FlaUI's click (handles DPI scaling)
    /// </summary>
    public bool ClickElement(AutomationElement? element)
    {
        if (element == null) return false;

        try
        {
            // FlaUI's Click() handles DPI scaling automatically
            element.Click();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FlaUI: Click failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clicks at a specific point relative to the window (handles DPI scaling)
    /// </summary>
    public void ClickAtWindowRelativePosition(Window? window, int relativeX, int relativeY)
    {
        if (window == null) return;

        var bounds = window.BoundingRectangle;
        var clickX = (int)bounds.X + relativeX;
        var clickY = (int)bounds.Y + relativeY;
        
        // FlaUI's Mouse class handles DPI scaling
        Mouse.MoveTo(clickX, clickY);
        Mouse.Click();
    }

    /// <summary>
    /// Clicks a ribbon tab by name. First tries UI Automation, then falls back to
    /// DPI-aware coordinate-based clicking for custom-drawn tabs.
    /// </summary>
    public async Task<bool> ClickRibbonTabAsync(Window? window, string tabName, int delayAfterClick = 500)
    {
        if (window == null) return false;

        var windowBounds = window.BoundingRectangle;
        Console.WriteLine($"  FlaUI: Looking for tab '{tabName}' (DPI scale: {_dpiScale:F2})...");

        // Focus the window first
        window.Focus();
        await Task.Delay(100);

        // Try direct name search first
        var tab = FindElementByName(window, tabName);
        if (tab != null)
        {
            var bounds = tab.BoundingRectangle;
            var topThreshold = 100 * _dpiScale;
            
            // Verify it's in the tab area
            if (bounds.Top < windowBounds.Top + topThreshold && bounds.Width > 20)
            {
                Console.WriteLine($"  FlaUI: Found tab '{tabName}' via UI Automation at ({bounds.X}, {bounds.Y}) size {bounds.Width}x{bounds.Height}");
                ClickElement(tab);
                await Task.Delay(delayAfterClick);
                return true;
            }
        }

        // Try finding TabItem controls and matching by name
        var tabs = FindRibbonTabs(window);
        foreach (var t in tabs)
        {
            var name = t.Name ?? "";
            if (name.Equals(tabName, StringComparison.OrdinalIgnoreCase) ||
                name.Contains(tabName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  FlaUI: Found tab via TabItem search: '{name}'");
                ClickElement(t);
                await Task.Delay(delayAfterClick);
                return true;
            }
        }

        // Try finding by accessible name pattern (for custom controls)
        var allElements = window.FindAllDescendants();
        foreach (var elem in allElements)
        {
            try
            {
                var name = elem.Name ?? "";
                var bounds = elem.BoundingRectangle;
                var topThreshold = 80 * _dpiScale;

                // Look for elements with the tab name that are in the tab header area
                if (name.Equals(tabName, StringComparison.OrdinalIgnoreCase) &&
                    bounds.Top < windowBounds.Top + topThreshold &&
                    bounds.Height > 15 && bounds.Height < 50 * _dpiScale &&
                    bounds.Width > 30 && bounds.Width < 200 * _dpiScale)
                {
                    Console.WriteLine($"  FlaUI: Found potential tab element: '{name}' at ({bounds.X}, {bounds.Y})");
                    ClickElement(elem);
                    await Task.Delay(delayAfterClick);
                    return true;
                }
            }
            catch { }
        }

        // Fall back to DPI-aware coordinate-based clicking for custom-drawn managed ribbon tabs
        // Managed ribbon layout:
        // - Tabs start at X=150 (logical) after File button and QAT
        // - Tab Y: title bar (~31 logical) + half tab header height (~13 logical) = 44 logical
        var tabLogicalPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Home", 180 },       // Managed ribbon: tabStartX(150) + half tab width (~30)
            { "Insert", 240 },     // Home + tab width + spacing
            { "Blog Account", 330 }, // Insert + tab width + spacing + wider tab
            { "Debug", 410 }       // Blog Account + tab width
        };
        
        const int tabLogicalY = 44;
        
        if (tabLogicalPositions.TryGetValue(tabName, out var tabLogicalX))
        {
            // Scale logical coordinates to physical coordinates
            var clickX = (int)(windowBounds.X + tabLogicalX * _dpiScale);
            var clickY = (int)(windowBounds.Y + tabLogicalY * _dpiScale);
            
            Console.WriteLine($"  FlaUI: Clicking tab '{tabName}' by position: physical ({clickX}, {clickY}) [logical: ({tabLogicalX}, {tabLogicalY})]");
            Mouse.MoveTo(clickX, clickY);
            Mouse.Click();
            await Task.Delay(delayAfterClick);
            return true;
        }

        Console.WriteLine($"  FlaUI: WARNING - Could not find tab '{tabName}'");
        return false;
    }

    /// <summary>
    /// Clicks the File menu button
    /// </summary>
    public async Task<bool> ClickFileMenuAsync(Window? window, int delayAfterClick = 500)
    {
        if (window == null) return false;

        var windowBounds = window.BoundingRectangle;
        Console.WriteLine($"  FlaUI: Looking for File button (DPI scale: {_dpiScale:F2})...");

        // Focus the window first
        window.Focus();
        await Task.Delay(100);

        // Try finding by name "File"
        var fileBtn = FindElementByName(window, "File");
        if (fileBtn != null)
        {
            var bounds = fileBtn.BoundingRectangle;
            var topThreshold = 100 * _dpiScale;
            var leftThreshold = 100 * _dpiScale;

            // Verify it's in the top-left area
            if (bounds.Top < windowBounds.Top + topThreshold && bounds.Left < windowBounds.Left + leftThreshold)
            {
                Console.WriteLine($"  FlaUI: Found File button via UI Automation at ({bounds.X}, {bounds.Y})");
                ClickElement(fileBtn);
                await Task.Delay(delayAfterClick);
                return true;
            }
        }

        // Try finding buttons in the top-left that might be the File menu
        var buttons = FindAllButtons(window);
        foreach (var btn in buttons)
        {
            var bounds = btn.BoundingRectangle;
            var name = btn.Name ?? "";
            var topThreshold = 80 * _dpiScale;
            var leftThreshold = 80 * _dpiScale;

            // Look for a button in the top-left corner that could be File
            if (bounds.Left < windowBounds.Left + leftThreshold &&
                bounds.Top < windowBounds.Top + topThreshold &&
                (name.Contains("File", StringComparison.OrdinalIgnoreCase) || 
                 string.IsNullOrEmpty(name))) // File button might have no accessible name
            {
                Console.WriteLine($"  FlaUI: Found potential File button: '{name}' at ({bounds.X}, {bounds.Y})");
                ClickElement(btn);
                await Task.Delay(delayAfterClick);
                return true;
            }
        }

        // Fall back to DPI-aware coordinate-based clicking for custom-drawn managed ribbon
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
        var clickX = (int)(windowBounds.X + fileLogicalX * _dpiScale);
        var clickY = (int)(windowBounds.Y + fileLogicalY * _dpiScale);
        
        Console.WriteLine($"  FlaUI: Clicking File button by position: physical ({clickX}, {clickY}) [logical: ({fileLogicalX}, {fileLogicalY})]");
        Mouse.MoveTo(clickX, clickY);
        Mouse.Click();
        await Task.Delay(delayAfterClick);
        return true;
    }

    /// <summary>
    /// Sends the Escape key to close menus/dialogs
    /// </summary>
    public void SendEscapeKey()
    {
        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
        Thread.Sleep(50);
        Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
    }

    /// <summary>
    /// Brings a window to the foreground
    /// </summary>
    public void BringToForeground(Window? window)
    {
        window?.Focus();
    }

    /// <summary>
    /// Gets element bounds
    /// </summary>
    public Rectangle GetElementBounds(AutomationElement? element)
    {
        if (element == null) return Rectangle.Empty;

        var rect = element.BoundingRectangle;
        return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    /// <summary>
    /// Dumps the UI tree for debugging
    /// </summary>
    public void DumpUITree(Window? window, int maxDepth = 3)
    {
        if (window == null) return;

        Console.WriteLine("=== UI Tree Dump ===");
        DumpElement(window, 0, maxDepth);
        Console.WriteLine("=== End UI Tree ===");
    }

    private void DumpElement(AutomationElement element, int depth, int maxDepth)
    {
        if (depth > maxDepth) return;

        var indent = new string(' ', depth * 2);
        try
        {
            var bounds = element.BoundingRectangle;
            Console.WriteLine($"{indent}{element.ControlType}: '{element.Name}' at ({bounds.X:F0},{bounds.Y:F0}) size {bounds.Width:F0}x{bounds.Height:F0}");

            var children = element.FindAllChildren();
            foreach (var child in children)
            {
                DumpElement(child, depth + 1, maxDepth);
            }
        }
        catch { }
    }

    public void Dispose()
    {
        _automation?.Dispose();
    }
}
