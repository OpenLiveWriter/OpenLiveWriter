using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using RibbonTestHarness.Core;

namespace RibbonTestHarness;

/// <summary>
/// Runs ribbon comparison tests in headless mode (no GUI).
/// </summary>
public class HeadlessTestRunner
{
    private readonly string _outputDir;
    private readonly string? _singleTest;

    public HeadlessTestRunner(string? singleTest = null)
    {
        _singleTest = singleTest;
        _outputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RibbonTestResults",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public bool RunAllTests()
    {
        Console.WriteLine("=== Ribbon Test Harness - Headless Mode ===");
        Console.WriteLine($"Output directory: {_outputDir}");
        if (_singleTest != null)
            Console.WriteLine($"Single test: {_singleTest}");
        Console.WriteLine();

        var task = RunAllTestsAsync();
        task.Wait();
        return task.Result;
    }
    
    private async Task<bool> RunAllTestsAsync()
    {
        using var launcher = new AppLauncher();
        var uiHelper = new UIAutomationHelper();
        var comparer = new ImageComparer();
        
        try
        {
            // Verify paths exist
            Console.WriteLine("Checking application paths...");
            
            if (!File.Exists(launcher.InstalledAppPath))
            {
                Console.WriteLine($"ERROR: Installed app not found at: {launcher.InstalledAppPath}");
                return false;
            }
            
            var devPath = launcher.FindBestDevPath();
            if (!File.Exists(devPath))
            {
                Console.WriteLine($"ERROR: Dev app not found at: {devPath}");
                Console.WriteLine("Please build the project first using: dotnet build");
                return false;
            }
            
            Console.WriteLine($"  Installed: {launcher.InstalledAppPath}");
            Console.WriteLine($"  Dev: {devPath}");
            Console.WriteLine();
            
            // Launch both applications
            Console.WriteLine("Launching applications...");
            await launcher.LaunchBothAsync(5000);
            
            if (!launcher.IsInstalledRunning || !launcher.IsDevRunning)
            {
                Console.WriteLine("ERROR: Failed to launch one or both applications");
                return false;
            }
            
            Console.WriteLine("  Both applications launched successfully");
            
            // Attach UI automation
            Console.WriteLine("Attaching UI automation...");
            uiHelper.AttachToInstalled(launcher.InstalledProcess);
            uiHelper.AttachToDev(launcher.DevProcess);
            
            // Use exact same size for both windows to ensure fair comparison
            const int testWidth = 1200;
            const int testHeight = 800;

            // Wait for apps to fully settle (Squirrel updates, window restoration, etc.)
            await Task.Delay(2000);

            // Refresh process info to get the latest window handles
            // (OLW may spawn new windows during startup, e.g., Squirrel update)
            launcher.InstalledProcess?.Refresh();
            launcher.DevProcess?.Refresh();

            // Re-attach UI automation with fresh handles
            Console.WriteLine("Re-attaching UI automation with fresh handles...");
            uiHelper.AttachToInstalled(launcher.InstalledProcess);
            uiHelper.AttachToDev(launcher.DevProcess);

            // Position windows side by side with identical sizes
            Console.WriteLine("Positioning windows with identical sizes...");
            ResizeWindows(launcher, uiHelper, testWidth, testHeight);
            await Task.Delay(1000);

            // Ensure Home tab is selected on BOTH apps before starting tests
            Console.WriteLine("Ensuring Home tab is selected on both apps...");
            ScreenCapture.BringToForeground(launcher.InstalledProcess);
            await uiHelper.ClickRibbonTabAsync(uiHelper.InstalledRoot, "Home");
            await Task.Delay(300);

            ScreenCapture.BringToForeground(launcher.DevProcess);
            await uiHelper.ClickRibbonTabAsync(uiHelper.DevRoot, "Home");
            await Task.Delay(500);

            // Final resize - apps may have moved/resized during tab clicks
            Console.WriteLine("Final window resize verification...");
            ResizeWindows(launcher, uiHelper, testWidth, testHeight);
            await Task.Delay(500);
            
            // Run tests
            Console.WriteLine("\n=== Running Tests ===\n");

            var scenarios = new TestScenarios(launcher, uiHelper, comparer, _outputDir);
            List<TestScenarios.TestResult> results;

            if (_singleTest != null)
            {
                // Run a single test by name for faster iteration
                var testResult = await RunSingleTestAsync(scenarios, _singleTest, Console.WriteLine);
                results = new List<TestScenarios.TestResult> { testResult };
            }
            else
            {
                results = await scenarios.RunAllTestsAsync(Console.WriteLine);

                // Run dropdown/interactive tests
                Console.WriteLine("\n=== Running Dropdown Tests ===\n");
                var dropdownTests = new DropdownTestScenarios(launcher, uiHelper, comparer, _outputDir);
                await dropdownTests.RunAllAsync(Console.WriteLine);
            }

            // Generate summary
            Console.WriteLine("\n" + scenarios.GenerateSummaryReport());

            // Write summary.json
            WriteSummaryJson(results, _outputDir);

            // Return success if all tests passed
            bool allPassed = true;
            foreach (var result in results)
            {
                if (!result.Passed)
                {
                    allPassed = false;
                    break;
                }
            }

            return allPassed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nFATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return false;
        }
        finally
        {
            Console.WriteLine("\nClosing applications...");
            launcher.CloseBoth();
        }
    }

    /// <summary>
    /// Resizes both windows to the target size and verifies they match.
    /// Refreshes handles to account for windows that may have been recreated.
    /// </summary>
    private static void ResizeWindows(AppLauncher launcher, UIAutomationHelper uiHelper, int width, int height)
    {
        if (launcher.InstalledProcess == null || launcher.DevProcess == null)
            return;

        // Refresh to get latest handles
        launcher.InstalledProcess.Refresh();
        launcher.DevProcess.Refresh();

        var installedHandle = launcher.InstalledProcess.MainWindowHandle;
        var devHandle = launcher.DevProcess.MainWindowHandle;

        Console.WriteLine($"  Installed handle: 0x{installedHandle:X}, Dev handle: 0x{devHandle:X}");

        uiHelper.ResizeWindowsToMatch(installedHandle, devHandle, width, height);

        var installedSize = uiHelper.GetWindowSize(installedHandle);
        var devSize = uiHelper.GetWindowSize(devHandle);
        Console.WriteLine($"  Installed: {installedSize.Width}x{installedSize.Height}, Dev: {devSize.Width}x{devSize.Height}");
    }

    /// <summary>
    /// Runs a single test by name (case-insensitive partial match).
    /// </summary>
    private static async Task<TestScenarios.TestResult> RunSingleTestAsync(
        TestScenarios scenarios, string testName, Action<string>? log = null)
    {
        var nameMap = new Dictionary<string, Func<Action<string>?, Task<TestScenarios.TestResult>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["initial"]      = scenarios.TestInitialRibbonStateAsync,
            ["home"]         = scenarios.TestHomeTabAsync,
            ["insert"]       = scenarios.TestInsertTabAsync,
            ["blogaccount"]  = scenarios.TestBlogAccountTabAsync,
            ["blog"]         = scenarios.TestBlogAccountTabAsync,
            ["resize"]       = scenarios.TestWindowResizeAsync,
            ["fullscreen"]   = scenarios.TestFullscreenAsync,
            ["button"]       = scenarios.TestButtonStatesAsync,
            ["region"]       = scenarios.TestRibbonRegionsAsync,
        };

        foreach (var (key, testFunc) in nameMap)
        {
            if (testName.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                var result = await testFunc(log);
                scenarios.Results.Add(result);
                return result;
            }
        }

        log?.Invoke($"Unknown test name: '{testName}'. Available: initial, home, insert, blogaccount, resize, button, region");
        return new TestScenarios.TestResult { TestName = testName, Passed = false, Message = "Unknown test" };
    }

    private void WriteSummaryJson(List<TestScenarios.TestResult> results, string outputDir)
    {
        try
        {
            var testEntries = new List<object>();
            int passed = 0, failed = 0;

            foreach (var r in results)
            {
                var entry = new Dictionary<string, object>
                {
                    ["testName"] = r.TestName,
                    ["passed"] = r.Passed,
                    ["similarity"] = Math.Round(r.Similarity, 2),
                    ["message"] = r.Message,
                    ["differences"] = r.Differences,
                };
                if (r.ScreenshotPath != null)
                    entry["screenshotPath"] = r.ScreenshotPath;
                testEntries.Add(entry);

                if (r.Passed) passed++;
                else failed++;
            }

            var summary = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.Now.ToString("o"),
                ["outputDir"] = outputDir,
                ["totalTests"] = results.Count,
                ["passed"] = passed,
                ["failed"] = failed,
                ["allPassed"] = failed == 0,
                ["tests"] = testEntries,
            };

            var jsonPath = Path.Combine(outputDir, "summary.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(summary, options));
            Console.WriteLine($"\nJSON summary written to: {jsonPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  WARNING: Failed to write summary.json: {ex.Message}");
        }
    }
}
