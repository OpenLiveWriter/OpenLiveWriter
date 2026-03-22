using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RibbonTestHarness.Core;

/// <summary>
/// Additional test scenarios focusing on dropdown menus and interactive elements.
/// </summary>
public class DropdownTestScenarios
{
    private readonly AppLauncher _launcher;
    private readonly UIAutomationHelper _uiHelper;
    private readonly ImageComparer _comparer;
    private readonly string _outputDir;

    public DropdownTestScenarios(AppLauncher launcher, UIAutomationHelper uiHelper, ImageComparer comparer, string outputDir)
    {
        _launcher = launcher;
        _uiHelper = uiHelper;
        _comparer = comparer;
        _outputDir = outputDir;
    }

    /// <summary>
    /// Captures full window screenshots for detailed analysis
    /// </summary>
    public async Task CaptureFullWindowsAsync(Action<string>? log = null)
    {
        log?.Invoke("Capturing full window screenshots...");

        // Ensure windows are positioned
        if (_launcher.InstalledProcess != null && _launcher.DevProcess != null)
        {
            _uiHelper.ResizeWindowsToMatch(
                _launcher.InstalledProcess.MainWindowHandle,
                _launcher.DevProcess.MainWindowHandle,
                1200, 800);
        }
        await Task.Delay(500);

        // Capture installed
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await Task.Delay(200);
        var installedFull = ScreenCapture.CaptureProcess(_launcher.InstalledProcess);

        // Capture dev
        ScreenCapture.BringToForeground(_launcher.DevProcess);
        await Task.Delay(200);
        var devFull = ScreenCapture.CaptureProcess(_launcher.DevProcess);

        if (installedFull != null && devFull != null)
        {
            var timestamp = DateTime.Now.ToString("HHmmss");
            ScreenCapture.SaveCapture(installedFull, Path.Combine(_outputDir, $"FullWindow_{timestamp}_installed.png"));
            ScreenCapture.SaveCapture(devFull, Path.Combine(_outputDir, $"FullWindow_{timestamp}_dev.png"));

            var comparison = _comparer.Compare(installedFull, devFull, 5);
            _comparer.SaveComparisonReport(_outputDir, $"FullWindow_{timestamp}", installedFull, devFull, comparison);
            log?.Invoke($"Full window comparison: {comparison.Summary}");

            installedFull.Dispose();
            devFull.Dispose();
        }
    }

    /// <summary>
    /// Tests the blog selector dropdown
    /// </summary>
    public async Task TestBlogSelectorDropdownAsync(Action<string>? log = null)
    {
        log?.Invoke("\nTesting Blog Selector dropdown...");

        // Click on blog selector in installed version
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await Task.Delay(200);
        
        // Find and click the blog selector dropdown (usually has blog name or "DougRathbone.com")
        var blogSelector = _uiHelper.FindElementByName(_uiHelper.InstalledRoot, "DougRathbone.com");
        if (blogSelector == null)
        {
            // Try finding by control type
            var buttons = _uiHelper.FindRibbonButtons(_uiHelper.InstalledRoot);
            foreach (var btn in buttons)
            {
                try
                {
                    var name = btn.Current.Name;
                    if (name.Contains(".com") || name.Contains("blog", StringComparison.OrdinalIgnoreCase))
                    {
                        blogSelector = btn;
                        break;
                    }
                }
                catch { }
            }
        }

        if (blogSelector != null)
        {
            log?.Invoke("  Found blog selector, clicking...");
            _uiHelper.ClickElement(blogSelector);
            await Task.Delay(800);

            // Capture the dropdown
            var installedDropdown = ScreenCapture.CaptureProcess(_launcher.InstalledProcess);
            
            // Press Escape to close
            _uiHelper.SendEscapeKey();
            await Task.Delay(300);

            // Now do the same for dev version
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await Task.Delay(200);

            var devBlogSelector = _uiHelper.FindElementByName(_uiHelper.DevRoot, "DougRathbone.com");
            if (devBlogSelector == null)
            {
                log?.Invoke("  Did not find 'DougRathbone.com' by name, searching buttons...");
                var buttons = _uiHelper.FindRibbonButtons(_uiHelper.DevRoot);
                foreach (var btn in buttons)
                {
                    try
                    {
                        var name = btn.Current.Name;
                        var controlType = btn.Current.ControlType;
                        log?.Invoke($"    Found button: '{name}' ({controlType.ProgrammaticName})");
                        if (name.Contains(".com") || name.Contains("blog", StringComparison.OrdinalIgnoreCase))
                        {
                            devBlogSelector = btn;
                            log?.Invoke($"  Selected blog selector: '{name}'");
                            break;
                        }
                    }
                    catch { }
                }
            }

            if (devBlogSelector != null)
            {
                // Log what element we found
                try
                {
                    var rect = devBlogSelector.Current.BoundingRectangle;
                    var controlType = devBlogSelector.Current.ControlType;
                    var className = devBlogSelector.Current.ClassName;
                    var name = devBlogSelector.Current.Name;
                    log?.Invoke($"  Dev blog selector element: Name='{name}', Type={controlType.ProgrammaticName}, Class='{className}', Bounds=({rect.X},{rect.Y}) {rect.Width}x{rect.Height}");
                }
                catch (Exception ex)
                {
                    log?.Invoke($"  Error getting dev blog selector info: {ex.Message}");
                }
                
                // Always use mouse click for blog selector (InvokePattern doesn't trigger OnMouseClick)
                _uiHelper.ClickElementByMouse(devBlogSelector);
                await Task.Delay(800);

                var devDropdown = ScreenCapture.CaptureProcess(_launcher.DevProcess);

                _uiHelper.SendEscapeKey();
                await Task.Delay(300);

                if (installedDropdown != null && devDropdown != null)
                {
                    var timestamp = DateTime.Now.ToString("HHmmss");
                    var comparison = _comparer.Compare(installedDropdown, devDropdown, 5);
                    _comparer.SaveComparisonReport(_outputDir, $"BlogSelector_{timestamp}", installedDropdown, devDropdown, comparison);
                    log?.Invoke($"  Blog selector dropdown: {comparison.Summary}");

                    installedDropdown.Dispose();
                    devDropdown.Dispose();
                }
            }
            else
            {
                log?.Invoke("  Could not find blog selector in dev version");
            }
        }
        else
        {
            log?.Invoke("  Could not find blog selector dropdown");
        }
    }

    /// <summary>
    /// Tests the Paste split button dropdown
    /// </summary>
    public async Task TestPasteDropdownAsync(Action<string>? log = null)
    {
        log?.Invoke("\nTesting Paste dropdown...");

        // Click on Paste dropdown arrow in installed version
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await Task.Delay(200);

        var pasteButton = _uiHelper.FindElementByName(_uiHelper.InstalledRoot, "Paste");
        if (pasteButton != null)
        {
            // Click the dropdown part (right side of button)
            var bounds = _uiHelper.GetElementBounds(pasteButton);
            _uiHelper.ClickAtPosition(bounds.Right - 5, bounds.Top + bounds.Height / 2);
            await Task.Delay(600);

            var installedDropdown = ScreenCapture.CaptureProcess(_launcher.InstalledProcess);
            _uiHelper.SendEscapeKey();
            await Task.Delay(300);

            // Same for dev
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await Task.Delay(200);

            var devPasteButton = _uiHelper.FindElementByName(_uiHelper.DevRoot, "Paste");
            if (devPasteButton != null)
            {
                bounds = _uiHelper.GetElementBounds(devPasteButton);
                _uiHelper.ClickAtPosition(bounds.Right - 5, bounds.Top + bounds.Height / 2);
                await Task.Delay(600);

                var devDropdown = ScreenCapture.CaptureProcess(_launcher.DevProcess);
                _uiHelper.SendEscapeKey();
                await Task.Delay(300);

                if (installedDropdown != null && devDropdown != null)
                {
                    var timestamp = DateTime.Now.ToString("HHmmss");
                    var comparison = _comparer.Compare(installedDropdown, devDropdown, 5);
                    _comparer.SaveComparisonReport(_outputDir, $"PasteDropdown_{timestamp}", installedDropdown, devDropdown, comparison);
                    log?.Invoke($"  Paste dropdown: {comparison.Summary}");

                    installedDropdown.Dispose();
                    devDropdown.Dispose();
                }
            }
        }
        else
        {
            log?.Invoke("  Could not find Paste button");
        }
    }

    /// <summary>
    /// Tests the File menu (application menu)
    /// </summary>
    public async Task TestFileMenuAsync(Action<string>? log = null)
    {
        log?.Invoke("\nTesting File menu...");

        // Click on File button in installed version
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await Task.Delay(200);

        // Use the improved ClickFileMenuAsync which handles both native and managed ribbons
        if (await _uiHelper.ClickFileMenuAsync(_uiHelper.InstalledRoot, 800))
        {
            var installedMenu = ScreenCapture.CaptureProcess(_launcher.InstalledProcess);
            _uiHelper.SendEscapeKey();
            await Task.Delay(300);

            // Same for dev
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await Task.Delay(200);

            if (await _uiHelper.ClickFileMenuAsync(_uiHelper.DevRoot, 800))
            {
                var devMenu = ScreenCapture.CaptureProcess(_launcher.DevProcess);
                _uiHelper.SendEscapeKey();
                await Task.Delay(300);

                if (installedMenu != null && devMenu != null)
                {
                    var timestamp = DateTime.Now.ToString("HHmmss");
                    var comparison = _comparer.Compare(installedMenu, devMenu, 5);
                    _comparer.SaveComparisonReport(_outputDir, $"FileMenu_{timestamp}", installedMenu, devMenu, comparison);
                    log?.Invoke($"  File menu: {comparison.Summary}");

                    installedMenu.Dispose();
                    devMenu.Dispose();
                }
            }
            else
            {
                log?.Invoke("  Could not click File button in dev version");
            }
        }
        else
        {
            log?.Invoke("  Could not click File button in installed version");
        }
    }

    /// <summary>
    /// Captures close-up of text styling in different areas
    /// </summary>
    public async Task CaptureTextStylingAsync(Action<string>? log = null)
    {
        log?.Invoke("\nCapturing text styling close-ups...");

        // Capture tab area
        var tabHeight = 30;
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await Task.Delay(200);
        var installedTabs = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, tabHeight);

        ScreenCapture.BringToForeground(_launcher.DevProcess);
        await Task.Delay(200);
        var devTabs = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, tabHeight);

        if (installedTabs != null && devTabs != null)
        {
            var timestamp = DateTime.Now.ToString("HHmmss");
            var comparison = _comparer.Compare(installedTabs, devTabs, 3);
            _comparer.SaveComparisonReport(_outputDir, $"TabStrip_{timestamp}", installedTabs, devTabs, comparison);
            log?.Invoke($"  Tab strip: {comparison.Summary}");

            installedTabs.Dispose();
            devTabs.Dispose();
        }

        // Capture button labels area (30-100px from top)
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await Task.Delay(200);
        var installedButtons = ScreenCapture.CaptureWindowRegion(
            _launcher.InstalledProcess!.MainWindowHandle,
            new Rectangle(0, 30, 1200, 80));

        ScreenCapture.BringToForeground(_launcher.DevProcess);
        await Task.Delay(200);
        var devButtons = ScreenCapture.CaptureWindowRegion(
            _launcher.DevProcess!.MainWindowHandle,
            new Rectangle(0, 30, 1200, 80));

        if (installedButtons != null && devButtons != null)
        {
            var timestamp = DateTime.Now.ToString("HHmmss");
            var comparison = _comparer.Compare(installedButtons, devButtons, 3);
            _comparer.SaveComparisonReport(_outputDir, $"ButtonLabels_{timestamp}", installedButtons, devButtons, comparison);
            log?.Invoke($"  Button labels: {comparison.Summary}");

            installedButtons.Dispose();
            devButtons.Dispose();
        }
    }

    /// <summary>
    /// Runs all dropdown and interactive tests
    /// </summary>
    public async Task RunAllAsync(Action<string>? log = null)
    {
        log?.Invoke("=== Running Dropdown and Interactive Tests ===\n");

        await CaptureFullWindowsAsync(log);
        await CaptureTextStylingAsync(log);
        await TestFileMenuAsync(log);
        await TestBlogSelectorDropdownAsync(log);
        await TestPasteDropdownAsync(log);

        log?.Invoke("\n=== Dropdown Tests Complete ===");
    }
}
