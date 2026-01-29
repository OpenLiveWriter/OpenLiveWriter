using System;
using System.IO;
using System.Threading.Tasks;
using RibbonTestHarness.Core;

namespace RibbonTestHarness;

/// <summary>
/// Runs ribbon comparison tests in headless mode (no GUI).
/// </summary>
public class HeadlessTestRunner
{
    private readonly string _outputDir;
    
    public HeadlessTestRunner()
    {
        _outputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RibbonTestResults",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
    
    public bool RunAllTests()
    {
        Console.WriteLine("=== Ribbon Test Harness - Headless Mode ===");
        Console.WriteLine($"Output directory: {_outputDir}");
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
            
            // Position windows side by side with identical sizes
            Console.WriteLine("Positioning windows with identical sizes...");
            if (launcher.InstalledProcess != null && launcher.DevProcess != null)
            {
                var installedHandle = launcher.InstalledProcess.MainWindowHandle;
                var devHandle = launcher.DevProcess.MainWindowHandle;
                
                // Use exact same size for both windows to ensure fair comparison
                const int testWidth = 1200;
                const int testHeight = 800;
                
                uiHelper.ResizeWindowsToMatch(installedHandle, devHandle, testWidth, testHeight);
                
                // Verify both windows are the same size
                var installedSize = uiHelper.GetWindowSize(installedHandle);
                var devSize = uiHelper.GetWindowSize(devHandle);
                
                Console.WriteLine($"  Installed window: {installedSize.Width}x{installedSize.Height}");
                Console.WriteLine($"  Dev window: {devSize.Width}x{devSize.Height}");
                
                // If sizes don't match, force them again
                if (installedSize != devSize)
                {
                    Console.WriteLine("  Window sizes don't match, forcing resize...");
                    await Task.Delay(500);
                    uiHelper.ResizeWindowsToMatch(installedHandle, devHandle, testWidth, testHeight);
                    await Task.Delay(500);
                    
                    installedSize = uiHelper.GetWindowSize(installedHandle);
                    devSize = uiHelper.GetWindowSize(devHandle);
                    Console.WriteLine($"  After retry - Installed: {installedSize.Width}x{installedSize.Height}, Dev: {devSize.Width}x{devSize.Height}");
                }
            }
            
            await Task.Delay(1000);
            
            // Ensure Home tab is selected on BOTH apps before starting tests
            Console.WriteLine("Ensuring Home tab is selected on both apps...");
            ScreenCapture.BringToForeground(launcher.InstalledProcess);
            await uiHelper.ClickRibbonTabAsync(uiHelper.InstalledRoot, "Home");
            await Task.Delay(300);
            
            ScreenCapture.BringToForeground(launcher.DevProcess);
            await uiHelper.ClickRibbonTabAsync(uiHelper.DevRoot, "Home");
            await Task.Delay(500);
            
            // Run tests
            Console.WriteLine("\n=== Running Tests ===\n");
            
            var scenarios = new TestScenarios(launcher, uiHelper, comparer, _outputDir);
            var results = await scenarios.RunAllTestsAsync(Console.WriteLine);
            
            // Run dropdown/interactive tests
            Console.WriteLine("\n=== Running Dropdown Tests ===\n");
            var dropdownTests = new DropdownTestScenarios(launcher, uiHelper, comparer, _outputDir);
            await dropdownTests.RunAllAsync(Console.WriteLine);
            
            // Generate summary
            Console.WriteLine("\n" + scenarios.GenerateSummaryReport());
            
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
}
