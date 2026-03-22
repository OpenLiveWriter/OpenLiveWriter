using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RibbonTestHarness.Core;

/// <summary>
/// Handles launching and managing both the installed and development versions of Open Live Writer.
/// </summary>
public class AppLauncher : IDisposable
{
    private Process? _installedProcess;
    private Process? _devProcess;

    // Paths to the executables
    public string InstalledAppPath { get; set; } = @"C:\Users\dougr\AppData\Local\OpenLiveWriter\app-0.6.3\OpenLiveWriter.exe";
    public string DevAppPath { get; set; } = @"D:\Code\openlivewriter\src\managed\OpenLiveWriter\bin\Debug\OpenLiveWriter.exe";
    
    // Alternative dev paths
    public string DevAppPath_x64 => @"D:\Code\openlivewriter\src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe";
    public string DevAppPath_i386 => @"D:\Code\openlivewriter\src\managed\bin\Debug\i386\Writer\OpenLiveWriter.exe";
    
    public Process? InstalledProcess => _installedProcess;
    public Process? DevProcess => _devProcess;
    
    public bool IsInstalledRunning => _installedProcess != null && !_installedProcess.HasExited;
    public bool IsDevRunning => _devProcess != null && !_devProcess.HasExited;
    
    /// <summary>
    /// Finds the best available dev build path
    /// </summary>
    public string FindBestDevPath()
    {
        // Check in order of preference
        string[] paths = [DevAppPath, DevAppPath_x64, DevAppPath_i386];
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        return DevAppPath; // Return default even if not found
    }

    /// <summary>
    /// Kills any existing OpenLiveWriter processes to avoid single-instance conflicts.
    /// OLW is a single-instance app, so stale processes prevent new launches.
    /// </summary>
    public static void KillExistingProcesses()
    {
        var processes = Process.GetProcessesByName("OpenLiveWriter")
            .Concat(Process.GetProcessesByName("OpenLiveWriter.exe"));

        foreach (var proc in processes)
        {
            try
            {
                Console.WriteLine($"  Killing existing OLW process: PID={proc.Id}");
                proc.Kill();
                proc.WaitForExit(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Could not kill process {proc.Id}: {ex.Message}");
            }
            finally
            {
                proc.Dispose();
            }
        }
    }

    /// <summary>
    /// Launches the installed version of Open Live Writer
    /// </summary>
    public async Task<Process?> LaunchInstalledAsync(int startupDelayMs = 3000)
    {
        if (!File.Exists(InstalledAppPath))
        {
            throw new FileNotFoundException($"Installed app not found at: {InstalledAppPath}");
        }
        
        Console.WriteLine($"  Launching installed from: {InstalledAppPath}");
        
        _installedProcess = Process.Start(new ProcessStartInfo
        {
            FileName = InstalledAppPath,
            UseShellExecute = true
        });
        
        // Wait for the app to start
        await Task.Delay(startupDelayMs);
        
        // Ensure we have a valid main window handle
        await WaitForMainWindowAsync(_installedProcess, "Installed");
        
        return _installedProcess;
    }
    
    /// <summary>
    /// Launches the development version of Open Live Writer
    /// </summary>
    public async Task<Process?> LaunchDevAsync(int startupDelayMs = 3000)
    {
        var path = FindBestDevPath();
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Dev app not found at: {path}. Please build the project first.");
        }
        
        Console.WriteLine($"  Launching dev from: {path}");
        
        _devProcess = Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
        
        // Wait for the app to start
        await Task.Delay(startupDelayMs);
        
        // Ensure we have a valid main window handle
        await WaitForMainWindowAsync(_devProcess, "Dev");
        
        return _devProcess;
    }
    
    /// <summary>
    /// Waits for the process to have a valid main window handle
    /// </summary>
    private async Task WaitForMainWindowAsync(Process? process, string name, int maxAttempts = 20)
    {
        if (process == null) return;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            process.Refresh(); // Refresh process info to get updated MainWindowHandle
            
            var handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                // Also verify the window is actually visible
                if (IsWindowVisible(handle))
                {
                    Console.WriteLine($"  {name} window handle acquired: 0x{handle:X} (attempt {i + 1})");
                    return;
                }
            }
            
            await Task.Delay(250);
        }
        
        Console.WriteLine($"  WARNING: {name} window handle may not be valid after {maxAttempts} attempts");
        Console.WriteLine($"  Current handle: 0x{process.MainWindowHandle:X}");
    }
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    
    /// <summary>
    /// Launches both versions side by side
    /// </summary>
    public async Task LaunchBothAsync(int startupDelayMs = 3000)
    {
        // Kill any existing instances first (OLW is single-instance)
        KillExistingProcesses();
        await Task.Delay(500); // Wait for processes to fully exit

        // Launch both simultaneously
        var installedTask = LaunchInstalledAsync(startupDelayMs);
        await Task.Delay(500); // Slight offset so they don't overlap initially
        var devTask = LaunchDevAsync(startupDelayMs);
        
        await Task.WhenAll(installedTask, devTask);
    }
    
    /// <summary>
    /// Closes the installed version
    /// </summary>
    public void CloseInstalled()
    {
        try
        {
            if (_installedProcess != null && !_installedProcess.HasExited)
            {
                _installedProcess.CloseMainWindow();
                if (!_installedProcess.WaitForExit(5000))
                {
                    _installedProcess.Kill();
                }
            }
        }
        catch { }
        finally
        {
            _installedProcess?.Dispose();
            _installedProcess = null;
        }
    }
    
    /// <summary>
    /// Closes the development version
    /// </summary>
    public void CloseDev()
    {
        try
        {
            if (_devProcess != null && !_devProcess.HasExited)
            {
                _devProcess.CloseMainWindow();
                if (!_devProcess.WaitForExit(5000))
                {
                    _devProcess.Kill();
                }
            }
        }
        catch { }
        finally
        {
            _devProcess?.Dispose();
            _devProcess = null;
        }
    }
    
    /// <summary>
    /// Closes both versions
    /// </summary>
    public void CloseBoth()
    {
        CloseInstalled();
        CloseDev();
    }
    
    public void Dispose()
    {
        CloseBoth();
    }
}
