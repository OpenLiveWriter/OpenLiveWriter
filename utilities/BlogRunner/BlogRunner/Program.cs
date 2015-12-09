using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using BlogRunner.Core;
using BlogRunner.Core.Tests;
using OpenLiveWriter.CoreServices;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using OpenLiveWriter.CoreServices.Diagnostics;
using System.Xml;

namespace BlogRunner
{
    class Program
    {
        private static void AddTests(List<Test> tests)
        {
            // New tests go here

            tests.Add(new SupportsMultipleCategoriesTest());
            tests.Add(new SupportsPostAsDraftTest());
            tests.Add(new SupportsFuturePostTest());
            tests.Add(new SupportsEmptyTitlesTest());
            tests.Add(new CompositePostTest(
                    new TitleEncodingTest(),
                    new SupportsEmbedsTest(),
                    new SupportsScriptsTest()
                ));
        }

        static int Main(string[] args)
        {
            try
            {
                ChangeErrorColors(ConsoleColor.Red);

                BlogRunnerCommandLineOptions options = new BlogRunnerCommandLineOptions();
                options.Parse(args, true);

                try
                {

                    if (options.GetFlagValue(BlogRunnerCommandLineOptions.OPTION_VERBOSE, false))
                        Debug.Listeners.Add(new ConsoleTraceListener(true));

                    string providersPath = Path.GetFullPath((string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_PROVIDERS, null));
                    string configPath = Path.GetFullPath((string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_CONFIG, null));
                    string outputPath = Path.GetFullPath((string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_OUTPUT, providersPath));
                    List<string> providerIds = new List<string>(options.UnnamedArgs);
                    string errorLogPath = (string)options.GetValue(BlogRunnerCommandLineOptions.OPTION_ERRORLOG, null);
                    if (errorLogPath != null)
                    {
                        errorLogPath = Path.GetFullPath(errorLogPath);
                        Console.SetError(new CompositeTextWriter(
                            Console.Error,
                            File.CreateText(errorLogPath)));
                    }

                    ApplicationEnvironment.Initialize(Assembly.GetExecutingAssembly(),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"\Windows Live\Writer\"));
                    ApplicationDiagnostics.VerboseLogging = true;

                    var config = Config.Load(configPath, providersPath);
                    var providers = new XmlDocument();
                    providers.Load(providersPath);


                    foreach (XmlElement provider in providers.SelectNodes("/providers/provider"))
                    {
                        string providerId = provider.SelectSingleNode("id/text()").Value;
                        string clientType = provider.SelectSingleNode("clientType/text()").Value;

                        if (providerIds.Count > 0 && !providerIds.Contains(providerId))
                            continue;

                        Provider p = config.GetProviderById(providerId);
                        if (p == null)
                            continue;

                        p.ClientType = clientType;

                        TestResultImpl results = new TestResultImpl();

                        Blog b = p.Blog;
                        if (b != null)
                        {
                            Console.Write(provider.SelectSingleNode("name/text()").Value);
                            Console.Write(" (");
                            Console.Write(b.HomepageUrl);
                            Console.WriteLine(")");

                            List<Test> tests = new List<Test>();
                            AddTests(tests);
                            TestRunner tr = new TestRunner(tests);
                            tr.RunTests(p, b, provider);
                        }
                    }

                    using (XmlTextWriter xw = new XmlTextWriter(outputPath, Encoding.UTF8))
                    {
                        xw.Formatting = Formatting.Indented;
                        xw.Indentation = 1;
                        xw.IndentChar = '\t';
                        providers.WriteTo(xw);
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                    return 1;
                }
                finally
                {
                    if (options.GetFlagValue(BlogRunnerCommandLineOptions.OPTION_PAUSE, false))
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.Write("Press any key to continue...");
                        Console.ReadKey(true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return 1;
            }
        }

        private static void ChangeErrorColors(ConsoleColor color)
        {
            Console.SetError(new ColorChangeTextWriter(Console.Error, color));
        }

        private class ColorChangeTextWriter : TextWriter
        {
            private readonly TextWriter tw;
            private readonly ConsoleColor color;

            public ColorChangeTextWriter(TextWriter tw, ConsoleColor color)
            {
                this.tw = tw;
                this.color = color;
            }

            public override System.Text.Encoding Encoding
            {
                get { return tw.Encoding; }
            }

            public override void Write(char value)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                try
                {
                    tw.Write(value);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
            }

            public override void Write(char[] buffer, int index, int count)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                try
                {
                    tw.Write(buffer, index, count);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        private class CompositeTextWriter : TextWriter
        {
            private readonly TextWriter[] writers;

            public CompositeTextWriter(params TextWriter[] writers)
            {
                this.writers = writers;
            }

            public override Encoding Encoding
            {
                get { return Encoding.Unicode; }
            }

            public override void Write(char value)
            {
                foreach (TextWriter writer in writers)
                {
                    writer.Write(value);
                    writer.Flush();
                }
            }

            public override void Write(char[] buffer, int index, int count)
            {
                foreach (TextWriter writer in writers)
                {
                    writer.Write(buffer, index, count);
                    writer.Flush();
                }
            }
        }
    }
}
