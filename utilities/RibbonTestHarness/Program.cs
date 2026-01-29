using System;
using System.Windows.Forms;

namespace RibbonTestHarness;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        
        // Parse command line arguments for headless mode
        if (args.Length > 0 && args[0] == "--headless")
        {
            var runner = new HeadlessTestRunner();
            Environment.Exit(runner.RunAllTests() ? 0 : 1);
        }
        else
        {
            Application.Run(new MainForm());
        }
    }
}
