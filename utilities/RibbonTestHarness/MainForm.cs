using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using RibbonTestHarness.Core;

namespace RibbonTestHarness;

/// <summary>
/// Main form for the Ribbon Test Harness GUI.
/// </summary>
public partial class MainForm : Form
{
    private readonly AppLauncher _launcher;
    private readonly UIAutomationHelper _uiHelper;
    private readonly ImageComparer _comparer;
    private string _outputDir;
    
    // Controls
    private Button _btnLaunchBoth = null!;
    private Button _btnLaunchInstalled = null!;
    private Button _btnLaunchDev = null!;
    private Button _btnCloseBoth = null!;
    private Button _btnCapture = null!;
    private Button _btnCompare = null!;
    private Button _btnRunAllTests = null!;
    private Button _btnOpenOutput = null!;
    private Button _btnPositionWindows = null!;
    
    private ComboBox _cmbTab = null!;
    private NumericUpDown _numRibbonHeight = null!;
    
    private PictureBox _picInstalled = null!;
    private PictureBox _picDev = null!;
    private PictureBox _picDiff = null!;
    
    private TextBox _txtLog = null!;
    private Label _lblStatus = null!;
    private Label _lblSimilarity = null!;
    
    private Bitmap? _installedCapture;
    private Bitmap? _devCapture;
    private Bitmap? _diffCapture;
    
    public MainForm()
    {
        _launcher = new AppLauncher();
        _uiHelper = new UIAutomationHelper();
        _comparer = new ImageComparer();
        _outputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RibbonTestResults",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        
        InitializeComponents();
        UpdateStatus();
    }
    
    private void InitializeComponents()
    {
        Text = "Ribbon Test Harness - Open Live Writer";
        Size = new Size(1400, 900);
        StartPosition = FormStartPosition.CenterScreen;
        
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Controls
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));   // Images
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));   // Log
        
        // === Control Panel ===
        var controlPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        
        // Row 1: Launch controls
        var launchPanel = new FlowLayoutPanel { Height = 35, AutoSize = true };
        
        _btnLaunchBoth = new Button { Text = "Launch Both Apps", Width = 130, Height = 30 };
        _btnLaunchBoth.Click += async (s, e) => await LaunchBothAsync();
        launchPanel.Controls.Add(_btnLaunchBoth);
        
        _btnLaunchInstalled = new Button { Text = "Launch Installed", Width = 110, Height = 30 };
        _btnLaunchInstalled.Click += async (s, e) => await LaunchInstalledAsync();
        launchPanel.Controls.Add(_btnLaunchInstalled);
        
        _btnLaunchDev = new Button { Text = "Launch Dev", Width = 100, Height = 30 };
        _btnLaunchDev.Click += async (s, e) => await LaunchDevAsync();
        launchPanel.Controls.Add(_btnLaunchDev);
        
        _btnCloseBoth = new Button { Text = "Close Both", Width = 90, Height = 30 };
        _btnCloseBoth.Click += (s, e) => CloseBoth();
        launchPanel.Controls.Add(_btnCloseBoth);
        
        _btnPositionWindows = new Button { Text = "Position Side-by-Side", Width = 130, Height = 30 };
        _btnPositionWindows.Click += (s, e) => PositionWindows();
        launchPanel.Controls.Add(_btnPositionWindows);
        
        controlPanel.Controls.Add(launchPanel);
        
        // Row 2: Test controls
        var testPanel = new FlowLayoutPanel { Height = 35, AutoSize = true };
        
        testPanel.Controls.Add(new Label { Text = "Tab:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        _cmbTab = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbTab.Items.AddRange(new[] { "Home", "Insert", "Blog Account" });
        _cmbTab.SelectedIndex = 0;
        testPanel.Controls.Add(_cmbTab);
        
        var btnClickTab = new Button { Text = "Click Tab", Width = 80, Height = 30 };
        btnClickTab.Click += async (s, e) => await ClickTabAsync();
        testPanel.Controls.Add(btnClickTab);
        
        testPanel.Controls.Add(new Label { Text = "Ribbon Height:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        _numRibbonHeight = new NumericUpDown { Width = 60, Minimum = 50, Maximum = 300, Value = 180 };
        testPanel.Controls.Add(_numRibbonHeight);
        
        _btnCapture = new Button { Text = "Capture Ribbons", Width = 110, Height = 30 };
        _btnCapture.Click += (s, e) => CaptureRibbons();
        testPanel.Controls.Add(_btnCapture);
        
        _btnCompare = new Button { Text = "Compare", Width = 80, Height = 30 };
        _btnCompare.Click += (s, e) => CompareCaptures();
        testPanel.Controls.Add(_btnCompare);
        
        controlPanel.Controls.Add(testPanel);
        
        // Row 3: Test run controls
        var runPanel = new FlowLayoutPanel { Height = 35, AutoSize = true };
        
        _btnRunAllTests = new Button { Text = "Run All Tests", Width = 110, Height = 30, BackColor = Color.LightGreen };
        _btnRunAllTests.Click += async (s, e) => await RunAllTestsAsync();
        runPanel.Controls.Add(_btnRunAllTests);
        
        _btnOpenOutput = new Button { Text = "Open Output Folder", Width = 130, Height = 30 };
        _btnOpenOutput.Click += (s, e) => OpenOutputFolder();
        runPanel.Controls.Add(_btnOpenOutput);
        
        _lblStatus = new Label { Text = "Status: Ready", AutoSize = true, Padding = new Padding(20, 6, 0, 0) };
        runPanel.Controls.Add(_lblStatus);
        
        _lblSimilarity = new Label { Text = "", AutoSize = true, Padding = new Padding(20, 6, 0, 0), ForeColor = Color.Blue };
        runPanel.Controls.Add(_lblSimilarity);
        
        controlPanel.Controls.Add(runPanel);
        
        mainLayout.Controls.Add(controlPanel, 0, 0);
        
        // === Image Panel ===
        var imagePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2
        };
        imagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        imagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        imagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        imagePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        imagePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        
        imagePanel.Controls.Add(new Label { Text = "Installed (Original)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        imagePanel.Controls.Add(new Label { Text = "Development (New)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) }, 1, 0);
        imagePanel.Controls.Add(new Label { Text = "Differences", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) }, 2, 0);
        
        _picInstalled = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
        _picDev = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
        _picDiff = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
        
        imagePanel.Controls.Add(_picInstalled, 0, 1);
        imagePanel.Controls.Add(_picDev, 1, 1);
        imagePanel.Controls.Add(_picDiff, 2, 1);
        
        mainLayout.Controls.Add(imagePanel, 0, 1);
        
        // === Log Panel ===
        _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 9),
            ReadOnly = true
        };
        mainLayout.Controls.Add(_txtLog, 0, 2);
        
        Controls.Add(mainLayout);
        
        // Handle form closing
        FormClosing += (s, e) =>
        {
            _launcher.CloseBoth();
            _installedCapture?.Dispose();
            _devCapture?.Dispose();
            _diffCapture?.Dispose();
        };
    }
    
    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message));
            return;
        }
        
        _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        _txtLog.SelectionStart = _txtLog.Text.Length;
        _txtLog.ScrollToCaret();
    }
    
    private void UpdateStatus()
    {
        var installedStatus = _launcher.IsInstalledRunning ? "Running" : "Stopped";
        var devStatus = _launcher.IsDevRunning ? "Running" : "Stopped";
        _lblStatus.Text = $"Installed: {installedStatus} | Dev: {devStatus}";
    }
    
    private async Task LaunchBothAsync()
    {
        try
        {
            _btnLaunchBoth.Enabled = false;
            Log("Launching both applications...");
            
            await _launcher.LaunchBothAsync(5000);
            
            _uiHelper.AttachToInstalled(_launcher.InstalledProcess);
            _uiHelper.AttachToDev(_launcher.DevProcess);
            
            PositionWindows();
            
            Log("Both applications launched successfully");
            UpdateStatus();
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
            MessageBox.Show(ex.Message, "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnLaunchBoth.Enabled = true;
        }
    }
    
    private async Task LaunchInstalledAsync()
    {
        try
        {
            Log("Launching installed version...");
            await _launcher.LaunchInstalledAsync(3000);
            _uiHelper.AttachToInstalled(_launcher.InstalledProcess);
            Log("Installed version launched");
            UpdateStatus();
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
    }
    
    private async Task LaunchDevAsync()
    {
        try
        {
            Log("Launching development version...");
            await _launcher.LaunchDevAsync(3000);
            _uiHelper.AttachToDev(_launcher.DevProcess);
            Log("Development version launched");
            UpdateStatus();
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
    }
    
    private void CloseBoth()
    {
        Log("Closing both applications...");
        _launcher.CloseBoth();
        UpdateStatus();
        Log("Both applications closed");
    }
    
    private void PositionWindows()
    {
        if (_launcher.InstalledProcess != null && _launcher.DevProcess != null)
        {
            _uiHelper.ResizeWindowsToMatch(
                _launcher.InstalledProcess.MainWindowHandle,
                _launcher.DevProcess.MainWindowHandle,
                1200, 800);
            Log("Windows positioned side-by-side");
        }
    }
    
    private async Task ClickTabAsync()
    {
        var tabName = _cmbTab.SelectedItem?.ToString() ?? "Home";
        Log($"Clicking '{tabName}' tab on both apps...");
        
        ScreenCapture.BringToForeground(_launcher.InstalledProcess);
        await _uiHelper.ClickRibbonTabAsync(_uiHelper.InstalledRoot, tabName);
        await Task.Delay(200);
        
        ScreenCapture.BringToForeground(_launcher.DevProcess);
        await _uiHelper.ClickRibbonTabAsync(_uiHelper.DevRoot, tabName);
        await Task.Delay(300);
        
        Log($"Clicked '{tabName}' tab");
        
        // Auto-capture after clicking
        CaptureRibbons();
    }
    
    private void CaptureRibbons()
    {
        Log("Capturing ribbon areas...");
        
        int ribbonHeight = (int)_numRibbonHeight.Value;
        
        _installedCapture?.Dispose();
        _devCapture?.Dispose();
        
        _installedCapture = ScreenCapture.CaptureProcessRibbon(_launcher.InstalledProcess, ribbonHeight);
        _devCapture = ScreenCapture.CaptureProcessRibbon(_launcher.DevProcess, ribbonHeight);
        
        _picInstalled.Image = _installedCapture;
        _picDev.Image = _devCapture;
        
        if (_installedCapture != null && _devCapture != null)
        {
            Log($"Captured: Installed={_installedCapture.Width}x{_installedCapture.Height}, Dev={_devCapture.Width}x{_devCapture.Height}");
            CompareCaptures();
        }
        else
        {
            Log("Warning: Failed to capture one or both ribbons");
        }
    }
    
    private void CompareCaptures()
    {
        if (_installedCapture == null || _devCapture == null)
        {
            Log("Nothing to compare - capture ribbons first");
            return;
        }
        
        Log("Comparing captures...");
        
        var result = _comparer.Compare(_installedCapture, _devCapture);
        
        _diffCapture?.Dispose();
        _diffCapture = result.DiffImage;
        _picDiff.Image = _diffCapture;
        
        _lblSimilarity.Text = result.Summary;
        _lblSimilarity.ForeColor = result.AreVeryClose ? Color.Green : result.AreSimilar ? Color.Orange : Color.Red;
        
        Log($"Comparison: {result.Summary}");
        
        // Save comparison
        Directory.CreateDirectory(_outputDir);
        var timestamp = DateTime.Now.ToString("HHmmss");
        _comparer.SaveComparisonReport(_outputDir, $"Manual_{timestamp}", _installedCapture, _devCapture, result);
        Log($"Saved to: {_outputDir}");
    }
    
    private async Task RunAllTestsAsync()
    {
        if (!_launcher.IsInstalledRunning || !_launcher.IsDevRunning)
        {
            Log("Please launch both applications first");
            MessageBox.Show("Please launch both applications first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        try
        {
            _btnRunAllTests.Enabled = false;
            Log("\n=== Starting Full Test Suite ===\n");
            
            var scenarios = new TestScenarios(_launcher, _uiHelper, _comparer, _outputDir);
            var results = await scenarios.RunAllTestsAsync(Log);
            
            Log("\n" + scenarios.GenerateSummaryReport());
            
            int passed = 0, failed = 0;
            foreach (var r in results)
            {
                if (r.Passed) passed++;
                else failed++;
            }
            
            var summary = $"Tests completed: {passed} passed, {failed} failed";
            _lblSimilarity.Text = summary;
            _lblSimilarity.ForeColor = failed == 0 ? Color.Green : Color.Red;
            
            MessageBox.Show(
                $"{summary}\n\nResults saved to:\n{_outputDir}",
                "Test Complete",
                MessageBoxButtons.OK,
                failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            Log($"Error during tests: {ex.Message}");
            MessageBox.Show(ex.Message, "Test Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnRunAllTests.Enabled = true;
        }
    }
    
    private void OpenOutputFolder()
    {
        try
        {
            Directory.CreateDirectory(_outputDir);
            Process.Start(new ProcessStartInfo
            {
                FileName = _outputDir,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log($"Error opening folder: {ex.Message}");
        }
    }
}
