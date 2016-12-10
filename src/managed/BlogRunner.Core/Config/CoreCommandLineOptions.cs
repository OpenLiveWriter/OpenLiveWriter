// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Class BlogRunnerCommandLineOptions.
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.CommandLineOptions" />
    public class BlogRunnerCommandLineOptions : CommandLineOptions
    {
        /// <summary>
        /// The option providers
        /// </summary>
        public const string OPTION_PROVIDERS = "providers";

        /// <summary>
        /// The option configuration
        /// </summary>
        public const string OPTION_CONFIG = "config";

        /// <summary>
        /// The option output
        /// </summary>
        public const string OPTION_OUTPUT = "output";

        /// <summary>
        /// The option verbose
        /// </summary>
        public const string OPTION_VERBOSE = "verbose";

        /// <summary>
        /// The option pause
        /// </summary>
        public const string OPTION_PAUSE = "pause";

        /// <summary>
        /// The option error log
        /// </summary>
        public const string OPTION_ERRORLOG = "errorlog";

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogRunnerCommandLineOptions"/> class.
        /// </summary>
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
