using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.CoreServices;

namespace BlogRunner.Core.Config
{
    public class BlogRunnerCommandLineOptions : CommandLineOptions
    {
        public const string OPTION_PROVIDERS = "providers";
        public const string OPTION_CONFIG = "config";
        public const string OPTION_OUTPUT = "output";
        public const string OPTION_VERBOSE = "verbose";
        public const string OPTION_PAUSE = "pause";
        public const string OPTION_ERRORLOG = "errorlog";

        public BlogRunnerCommandLineOptions() : base(false, 0, int.MaxValue,
                    new ArgSpec(OPTION_PROVIDERS, ArgSpec.Options.Required, "Path to BlogProviders.xml"),
                    new ArgSpec(OPTION_CONFIG, ArgSpec.Options.Required, "Path to BlogRunner config file"),
                    new ArgSpec(OPTION_OUTPUT, ArgSpec.Options.Required, "Path to output file"),
                    new ArgSpec(OPTION_VERBOSE, ArgSpec.Options.Flag, "Verbose logging"),
                    new ArgSpec(OPTION_ERRORLOG, ArgSpec.Options.Default, "Log errors to specified file"),
                    new ArgSpec(OPTION_PAUSE, ArgSpec.Options.Flag, "Pause before exiting"))
        {
        }
    }
}
