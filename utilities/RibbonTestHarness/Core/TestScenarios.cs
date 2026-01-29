using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace RibbonTestHarness.Core;

/// <summary>
/// Defines test scenarios for comparing ribbon implementations.
/// </summary>
public class TestScenarios
{
    private readonly AppLauncher _launcher;
    private readonly UIAutomationHelper _uiHelper;
    private readonly ImageComparer _comparer;
    private readonly string _outputDir;
    
    public List<TestResult> Results { get; } = new();
    
    public class TestResult
    {
        public string TestName { get; set; } = "";
        public bool Passed { get; set; }
        public double Similarity { get; set; }
        public string Message { get; set; } = "";
        public string? ScreenshotPath { get; set; }
        public List<string> Differences { get; set; } = new();
    }
    
    public TestScenarios(AppLauncher launcher, UIAutomationHelper uiHelper, ImageComparer comparer, string outputDir)
    {
        _launcher = launcher;
        _uiHelper = uiHelper;
        _comparer = comparer;
        _outputDir = outputDir;
        Directory.CreateDirectory(_outputDir);
    }
    
    /// <summary>
    /// Runs all test scenarios
    /// </summary>
    public async Task<List<TestResult>> RunAllTestsAsync(Action<string>? log = null)
    {
        Results.Clear();
        
        log?.Invoke("Starting all ribbon comparison tests...");
        
        // Test 1: Initial state comparison
        Results.Add(await TestInitialRibbonStateAsync(log));
        
        // Test 2: Home tab comparison
        Results.Add(await TestHomeTabAsync(log));
        
        // Test 3: Insert tab comparison
        Results.Add(await TestInsertTabAsync(log));
        
        // Test 4: Blog Account tab comparison
        Results.Add(await TestBlogAccountTabAsync(log));
        
        // Test 5: Window resize test
        Results.Add(await TestWindowResizeAsync(log));
        
        // Test 6: Button states comparison
        Results.Add(await TestButtonStatesAsync(log));
        
        log?.Invoke($"\nCompleted {Results.Count} tests.");
        
        return Results;
    }
    
    /// <summary>
    /// Test 1: Compare initial ribbon state when apps first launch
    /// </summary>
    public async Task<TestResult> TestInitialRibbonStateAsync(Action<string>? log = null)
    {
        var result = new TestResult { TestName = "Initial Ribbon State" };
        log?.Invoke($"\nRunning: {result.TestName}");
        
        try
        {
            // Give apps time to fully render
            await Task.Delay(1000);
            
            // Capture ribbon areas
            var installedRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, 180);
            var devRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, 180);
            
            if (installedRibbon == null || devRibbon == null)
            {
                result.Message = "Failed to capture ribbon screenshots";
                result.Passed = false;
                return result;
            }
            
            // Compare
            var comparison = _comparer.Compare(installedRibbon, devRibbon);
            _comparer.SaveComparisonReport(_outputDir, "01_InitialState", installedRibbon, devRibbon, comparison);
            
            result.Similarity = comparison.SimilarityPercentage;
            result.Passed = comparison.AreVeryClose;
            result.Message = comparison.Summary;
            result.ScreenshotPath = Path.Combine(_outputDir, "01_InitialState_comparison.png");
            
            // Analyze specific differences
            AnalyzeRibbonDifferences(result, _uiHelper.InstalledRoot, _uiHelper.DevRoot);
            
            installedRibbon.Dispose();
            devRibbon.Dispose();
            
            log?.Invoke($"  Result: {(result.Passed ? "PASS" : "FAIL")} - {comparison.Summary}");
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Message = $"Error: {ex.Message}";
            log?.Invoke($"  Error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Test 2: Compare Home tab
    /// </summary>
    public async Task<TestResult> TestHomeTabAsync(Action<string>? log = null)
    {
        var result = new TestResult { TestName = "Home Tab" };
        log?.Invoke($"\nRunning: {result.TestName}");
        
        try
        {
            // Click Home tab on both apps
            ScreenCapture.BringToForeground(_launcher.InstalledProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.InstalledRoot, "Home");
            await Task.Delay(300);
            
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.DevRoot, "Home");
            await Task.Delay(500);
            
            // Capture and compare
            var installedRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, 180);
            var devRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, 180);
            
            if (installedRibbon == null || devRibbon == null)
            {
                result.Message = "Failed to capture ribbon screenshots";
                result.Passed = false;
                return result;
            }
            
            var comparison = _comparer.Compare(installedRibbon, devRibbon);
            _comparer.SaveComparisonReport(_outputDir, "02_HomeTab", installedRibbon, devRibbon, comparison);
            
            result.Similarity = comparison.SimilarityPercentage;
            result.Passed = comparison.AreVeryClose;
            result.Message = comparison.Summary;
            result.ScreenshotPath = Path.Combine(_outputDir, "02_HomeTab_comparison.png");
            
            installedRibbon.Dispose();
            devRibbon.Dispose();
            
            log?.Invoke($"  Result: {(result.Passed ? "PASS" : "FAIL")} - {comparison.Summary}");
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Message = $"Error: {ex.Message}";
            log?.Invoke($"  Error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Test 3: Compare Insert tab
    /// </summary>
    public async Task<TestResult> TestInsertTabAsync(Action<string>? log = null)
    {
        var result = new TestResult { TestName = "Insert Tab" };
        log?.Invoke($"\nRunning: {result.TestName}");
        
        try
        {
            // Click Insert tab on both apps
            ScreenCapture.BringToForeground(_launcher.InstalledProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.InstalledRoot, "Insert");
            await Task.Delay(300);
            
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.DevRoot, "Insert");
            await Task.Delay(500);
            
            // Capture and compare
            var installedRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, 180);
            var devRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, 180);
            
            if (installedRibbon == null || devRibbon == null)
            {
                result.Message = "Failed to capture ribbon screenshots";
                result.Passed = false;
                return result;
            }
            
            var comparison = _comparer.Compare(installedRibbon, devRibbon);
            _comparer.SaveComparisonReport(_outputDir, "03_InsertTab", installedRibbon, devRibbon, comparison);
            
            result.Similarity = comparison.SimilarityPercentage;
            result.Passed = comparison.AreVeryClose;
            result.Message = comparison.Summary;
            result.ScreenshotPath = Path.Combine(_outputDir, "03_InsertTab_comparison.png");
            
            installedRibbon.Dispose();
            devRibbon.Dispose();
            
            log?.Invoke($"  Result: {(result.Passed ? "PASS" : "FAIL")} - {comparison.Summary}");
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Message = $"Error: {ex.Message}";
            log?.Invoke($"  Error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Test 4: Compare Blog Account tab
    /// </summary>
    public async Task<TestResult> TestBlogAccountTabAsync(Action<string>? log = null)
    {
        var result = new TestResult { TestName = "Blog Account Tab" };
        log?.Invoke($"\nRunning: {result.TestName}");
        
        try
        {
            // Click Blog Account tab on both apps
            ScreenCapture.BringToForeground(_launcher.InstalledProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.InstalledRoot, "Blog Account");
            await Task.Delay(300);
            
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.DevRoot, "Blog Account");
            await Task.Delay(500);
            
            // Capture and compare
            var installedRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, 180);
            var devRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, 180);
            
            if (installedRibbon == null || devRibbon == null)
            {
                result.Message = "Failed to capture ribbon screenshots";
                result.Passed = false;
                return result;
            }
            
            var comparison = _comparer.Compare(installedRibbon, devRibbon);
            _comparer.SaveComparisonReport(_outputDir, "04_BlogAccountTab", installedRibbon, devRibbon, comparison);
            
            result.Similarity = comparison.SimilarityPercentage;
            result.Passed = comparison.AreVeryClose;
            result.Message = comparison.Summary;
            result.ScreenshotPath = Path.Combine(_outputDir, "04_BlogAccountTab_comparison.png");
            
            installedRibbon.Dispose();
            devRibbon.Dispose();
            
            log?.Invoke($"  Result: {(result.Passed ? "PASS" : "FAIL")} - {comparison.Summary}");
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Message = $"Error: {ex.Message}";
            log?.Invoke($"  Error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Test 5: Compare ribbon behavior during window resize
    /// </summary>
    public async Task<TestResult> TestWindowResizeAsync(Action<string>? log = null)
    {
        var result = new TestResult { TestName = "Window Resize" };
        log?.Invoke($"\nRunning: {result.TestName}");
        
        try
        {
            // Test at different sizes
            int[] widths = { 800, 1000, 1200, 1400 };
            double totalSimilarity = 0;
            int testCount = 0;
            
            foreach (var width in widths)
            {
                // Resize both windows
                if (_launcher.InstalledProcess != null && _launcher.DevProcess != null)
                {
                    _uiHelper.ResizeWindowsToMatch(
                        _launcher.InstalledProcess.MainWindowHandle,
                        _launcher.DevProcess.MainWindowHandle,
                        width, 700);
                }
                
                await Task.Delay(500);
                
                // Capture and compare
                var installedRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, 180);
                var devRibbon = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, 180);
                
                if (installedRibbon != null && devRibbon != null)
                {
                    var comparison = _comparer.Compare(installedRibbon, devRibbon);
                    _comparer.SaveComparisonReport(_outputDir, $"05_Resize_{width}px", installedRibbon, devRibbon, comparison);
                    
                    totalSimilarity += comparison.SimilarityPercentage;
                    testCount++;
                    
                    if (comparison.SimilarityPercentage < 90)
                    {
                        result.Differences.Add($"At {width}px width: {comparison.SimilarityPercentage:F1}% similar");
                    }
                    
                    installedRibbon.Dispose();
                    devRibbon.Dispose();
                }
            }
            
            result.Similarity = testCount > 0 ? totalSimilarity / testCount : 0;
            result.Passed = result.Similarity >= 90;
            result.Message = $"Average similarity across {testCount} resize tests: {result.Similarity:F1}%";
            result.ScreenshotPath = Path.Combine(_outputDir, "05_Resize_1200px_comparison.png");
            
            log?.Invoke($"  Result: {(result.Passed ? "PASS" : "FAIL")} - {result.Message}");
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Message = $"Error: {ex.Message}";
            log?.Invoke($"  Error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Test 6: Compare button states and layout
    /// </summary>
    public async Task<TestResult> TestButtonStatesAsync(Action<string>? log = null)
    {
        var result = new TestResult { TestName = "Button States & Layout" };
        log?.Invoke($"\nRunning: {result.TestName}");
        
        try
        {
            // Ensure Home tab is selected
            ScreenCapture.BringToForeground(_launcher.InstalledProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.InstalledRoot, "Home");
            
            ScreenCapture.BringToForeground(_launcher.DevProcess);
            await _uiHelper.ClickRibbonTabAsync(_uiHelper.DevRoot, "Home");
            await Task.Delay(500);
            
            // Get button elements
            var installedButtons = _uiHelper.FindRibbonButtons(_uiHelper.InstalledRoot);
            var devButtons = _uiHelper.FindRibbonButtons(_uiHelper.DevRoot);
            
            // Compare button counts
            result.Differences.Add($"Installed has {installedButtons.Count} buttons, Dev has {devButtons.Count} buttons");
            
            // Get button names for comparison
            var installedButtonNames = new HashSet<string>();
            var devButtonNames = new HashSet<string>();
            
            foreach (var btn in installedButtons)
            {
                try { installedButtonNames.Add(btn.Current.Name); } catch { }
            }
            
            foreach (var btn in devButtons)
            {
                try { devButtonNames.Add(btn.Current.Name); } catch { }
            }
            
            // Find missing buttons
            var missingInDev = new List<string>();
            var extraInDev = new List<string>();
            
            foreach (var name in installedButtonNames)
            {
                if (!string.IsNullOrEmpty(name) && !devButtonNames.Contains(name))
                    missingInDev.Add(name);
            }
            
            foreach (var name in devButtonNames)
            {
                if (!string.IsNullOrEmpty(name) && !installedButtonNames.Contains(name))
                    extraInDev.Add(name);
            }
            
            if (missingInDev.Count > 0)
                result.Differences.Add($"Missing in dev: {string.Join(", ", missingInDev)}");
            if (extraInDev.Count > 0)
                result.Differences.Add($"Extra in dev: {string.Join(", ", extraInDev)}");
            
            // Calculate similarity based on button matching
            int matchingButtons = installedButtonNames.Count - missingInDev.Count;
            int totalButtons = Math.Max(installedButtonNames.Count, devButtonNames.Count);
            result.Similarity = totalButtons > 0 ? 100.0 * matchingButtons / totalButtons : 100;
            
            result.Passed = missingInDev.Count == 0 && result.Similarity >= 90;
            result.Message = $"Button matching: {matchingButtons}/{totalButtons} ({result.Similarity:F1}%)";
            
            log?.Invoke($"  Result: {(result.Passed ? "PASS" : "FAIL")} - {result.Message}");
            foreach (var diff in result.Differences)
            {
                log?.Invoke($"    - {diff}");
            }
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Message = $"Error: {ex.Message}";
            log?.Invoke($"  Error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Analyzes specific ribbon differences between the two versions.
    /// Note: The managed ribbon uses custom-drawn tabs that aren't visible to UI Automation,
    /// so we focus on button/control comparisons instead of tab structure.
    /// </summary>
    private void AnalyzeRibbonDifferences(TestResult result, AutomationElement? installed, AutomationElement? dev)
    {
        try
        {
            // Compare button counts as a proxy for ribbon completeness
            var installedButtons = _uiHelper.FindRibbonButtons(installed);
            var devButtons = _uiHelper.FindRibbonButtons(dev);
            
            // Filter to only buttons in the ribbon area (near top of window)
            var installedWindowRect = installed?.Current.BoundingRectangle;
            var devWindowRect = dev?.Current.BoundingRectangle;
            
            int installedRibbonButtons = 0;
            int devRibbonButtons = 0;
            
            foreach (var btn in installedButtons)
            {
                try
                {
                    var rect = btn.Current.BoundingRectangle;
                    if (installedWindowRect.HasValue && rect.Top < installedWindowRect.Value.Top + 200)
                    {
                        installedRibbonButtons++;
                    }
                }
                catch { }
            }
            
            foreach (var btn in devButtons)
            {
                try
                {
                    var rect = btn.Current.BoundingRectangle;
                    if (devWindowRect.HasValue && rect.Top < devWindowRect.Value.Top + 200)
                    {
                        devRibbonButtons++;
                    }
                }
                catch { }
            }
            
            if (installedRibbonButtons != devRibbonButtons)
            {
                result.Differences.Add($"Ribbon button count: Installed has ~{installedRibbonButtons}, Dev has ~{devRibbonButtons}");
            }
            
            // Note: Tab name comparison removed because managed ribbon tabs are custom-drawn
            // and not accessible via UI Automation. Use visual comparison instead.
        }
        catch { }
    }
    
    /// <summary>
    /// Generates a summary report
    /// </summary>
    public string GenerateSummaryReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Ribbon Comparison Test Summary ===");
        report.AppendLine($"Date: {DateTime.Now}");
        report.AppendLine($"Output: {_outputDir}");
        report.AppendLine();
        
        int passed = 0, failed = 0;
        
        foreach (var result in Results)
        {
            var status = result.Passed ? "PASS" : "FAIL";
            report.AppendLine($"[{status}] {result.TestName}");
            report.AppendLine($"       Similarity: {result.Similarity:F1}%");
            report.AppendLine($"       {result.Message}");
            
            foreach (var diff in result.Differences)
            {
                report.AppendLine($"       - {diff}");
            }
            
            if (result.Passed) passed++;
            else failed++;
            
            report.AppendLine();
        }
        
        report.AppendLine("=== Summary ===");
        report.AppendLine($"Passed: {passed}/{Results.Count}");
        report.AppendLine($"Failed: {failed}/{Results.Count}");
        
        // Save the report
        var reportPath = Path.Combine(_outputDir, "TestSummary.txt");
        File.WriteAllText(reportPath, report.ToString());
        
        return report.ToString();
    }
}
