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
        // Usage: --headless [--single-test <name>]
        if (args.Length > 0 && args[0] == "--headless")
        {
            string? singleTest = null;
            for (int i = 1; i < args.Length - 1; i++)
            {
                if (args[i] == "--single-test")
                {
                    singleTest = args[i + 1];
                    break;
                }
            }

            var runner = new HeadlessTestRunner(singleTest);
            Environment.Exit(runner.RunAllTests() ? 0 : 1);
        }
        else
        {
            Application.Run(new MainForm());
        }
    }
}
