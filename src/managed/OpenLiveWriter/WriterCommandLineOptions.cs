// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;
    using OpenLiveWriter.PostEditor;

    /// <summary>
    /// Class WriterCommandLineOptions.
    /// </summary>
    public class WriterCommandLineOptions
    {
        /// <summary>
        /// The options
        /// </summary>
        private readonly CommandLineOptions options;

        /// <summary>
        /// The culture
        /// </summary>
        private const string CULTURE = "culture";

        /// <summary>
        /// The options
        /// </summary>
        private const string OPTIONS = "options";

        /// <summary>
        /// The openpost
        /// </summary>
        private const string OPENPOST = "openpost";

        /// <summary>
        /// The noplugins
        /// </summary>
        private const string NOPLUGINS = "noplugins";

        /// <summary>
        /// The testmode
        /// </summary>
        private const string TESTMODE = "testmode";

        /// <summary>
        /// The verboselogging
        /// </summary>
        private const string VERBOSELOGGING = "verbose";

        /// <summary>
        /// The allowunsafecertificates
        /// </summary>
        private const string ALLOWUNSAFECERTIFICATES = "allowunsafecertificates";

        /// <summary>
        /// The preferatom
        /// </summary>
        private const string PREFERATOM = "preferatom";

        /// <summary>
        /// The suppressbackgroundrequests
        /// </summary>
        private const string SUPPRESSBACKGROUNDREQUESTS = "suppressbackgroundrequests";

        /// <summary>
        /// The proxy
        /// </summary>
        private const string PROXY = "proxy";

        /// <summary>
        /// The perflog
        /// </summary>
        private const string PERFLOG = "perflog";

        /// <summary>
        /// The automationmode
        /// </summary>
        private const string AUTOMATIONMODE = "automation";

        /// <summary>
        /// The attachdebugger
        /// </summary>
        private const string ATTACHDEBUGGER = "attach";

        /// <summary>
        /// The firstrun
        /// </summary>
        private const string FIRSTRUN = "firstrun";

        /// <summary>
        /// The locspy
        /// </summary>
        private const string LOCSPY = "locspy";

        /// <summary>
        /// The intapihost
        /// </summary>
        private const string INTAPIHOST = "intapihost";

        /// <summary>
        /// The addblog
        /// </summary>
        private const string ADDBLOG = "addblog";

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterCommandLineOptions"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        private WriterCommandLineOptions(CommandLineOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Applies the options.
        /// </summary>
        public void ApplyOptions()
        {
            try
            {
                if (this.options.IsArgPresent(ATTACHDEBUGGER))
                {
                    Debugger.Launch();
                }

                if (this.options.IsArgPresent(CULTURE))
                {
                    var culture = this.options.GetValue(CULTURE, null) as string;
                    if (culture != null)
                    {
                        CultureHelper.ApplyUICulture(culture);
                    }
                }

#if DEBUG
                if (this.options.IsArgPresent(TESTMODE))
                    ApplicationDiagnostics.TestMode = this.options.GetFlagValue(TESTMODE, ApplicationDiagnostics.TestMode);

                if (this.options.IsArgPresent(VERBOSELOGGING))
                    ApplicationDiagnostics.VerboseLogging = this.options.GetFlagValue(VERBOSELOGGING, ApplicationDiagnostics.VerboseLogging);

                if (this.options.IsArgPresent(ALLOWUNSAFECERTIFICATES))
                    ApplicationDiagnostics.AllowUnsafeCertificates = this.options.GetFlagValue(ALLOWUNSAFECERTIFICATES, ApplicationDiagnostics.AllowUnsafeCertificates);

                if (this.options.IsArgPresent(PREFERATOM))
                    ApplicationDiagnostics.PreferAtom = this.options.GetFlagValue(PREFERATOM, ApplicationDiagnostics.PreferAtom);

                if (this.options.IsArgPresent(SUPPRESSBACKGROUNDREQUESTS))
                    ApplicationDiagnostics.SuppressBackgroundRequests = this.options.GetFlagValue(SUPPRESSBACKGROUNDREQUESTS, ApplicationDiagnostics.SuppressBackgroundRequests);

                if (this.options.IsArgPresent(PROXY))
                    ApplicationDiagnostics.ProxySettingsOverride = (string)this.options.GetValue(PROXY, ApplicationDiagnostics.ProxySettingsOverride);

                if (this.options.IsArgPresent(PERFLOG))
                    ApplicationPerformance.SetLogFilePath((string)this.options.GetValue(PERFLOG, null));

                if (this.options.IsArgPresent(AUTOMATIONMODE))
                    ApplicationDiagnostics.AutomationMode = true;

                if (this.options.IsArgPresent(FIRSTRUN))
                    ApplicationDiagnostics.SimulateFirstRun = true;

                if (this.options.IsArgPresent(INTAPIHOST))
                    ApplicationDiagnostics.IntServerOverride = (string)this.options.GetValue(INTAPIHOST, null);
#endif

#if !SIGNED
                if (this.options.IsArgPresent(LOCSPY))
                    Res.DebugMode = true;
#endif
            }
            catch (Exception e)
            {
                Debug.Fail("Unable to apply culture:\r\n\r\n" + e.ToString());
            }
        }

        /// <summary>
        /// Creates the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>WriterCommandLineOptions.</returns>
        public static WriterCommandLineOptions Create(string[] args)
        {
            var options = new CommandLineOptions(false, 0, int.MaxValue,
                new ArgSpec(CULTURE, ArgSpec.Options.Default, "The culture to use (e.g. \"en-us\")"),
                new ArgSpec(OPTIONS, ArgSpec.Options.ValueOptional, "Show options"),
                new ArgSpec(OPENPOST, ArgSpec.Options.Flag, "Open post"),
                new ArgSpec(TESTMODE, ArgSpec.Options.Flag | ArgSpec.Options.Unsettable, "Debug mode"),
                new ArgSpec(VERBOSELOGGING, ArgSpec.Options.Flag | ArgSpec.Options.Unsettable, "Enable verbose logging"),
                new ArgSpec(ALLOWUNSAFECERTIFICATES, ArgSpec.Options.Flag | ArgSpec.Options.Unsettable, "Allow all SSL/TLS certificates"),
                new ArgSpec(PREFERATOM, ArgSpec.Options.Flag, "Prefer Atom to RSD during automatic configuration"),
                new ArgSpec(SUPPRESSBACKGROUNDREQUESTS, ArgSpec.Options.Flag, "Suppress background HTTP requests (for testing purposes)"),
                new ArgSpec(PROXY, ArgSpec.Options.Default, "Override proxy settings"),
                new ArgSpec(PERFLOG, ArgSpec.Options.Default, "File path where performance data should be logged. If the file exists, it will be truncated."),
                new ArgSpec(AUTOMATIONMODE, ArgSpec.Options.Flag | ArgSpec.Options.Unsettable, "Turn on automation mode"),
                new ArgSpec(ATTACHDEBUGGER, ArgSpec.Options.Flag, "Attach debugger on launch"),
                new ArgSpec(FIRSTRUN, ArgSpec.Options.Flag, "Show first run wizard"),
                new ArgSpec(INTAPIHOST, ArgSpec.Options.Default, "Use the specified API server hostname for INT testing"),
                new ArgSpec(LOCSPY, ArgSpec.Options.Flag, "Show localization names instead of values"),
                new ArgSpec(NOPLUGINS, ArgSpec.Options.Flag, "Prevents plugins from loading."),
                new ArgSpec(ADDBLOG, ArgSpec.Options.Default, "Adds a blog.")
                );

            var success = options.Parse(args, false);
            if (!success)
            {
                MessageBox.Show(options.ErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0);
                return null;
            }
            else
                return new WriterCommandLineOptions(options);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is show preferences.
        /// </summary>
        /// <value><c>true</c> if this instance is show preferences; otherwise, <c>false</c>.</value>
        public bool IsShowPreferences => this.options.IsArgPresent(OPTIONS);

        /// <summary>
        /// Gets the preferences page.
        /// </summary>
        /// <value>The preferences page.</value>
        public string PreferencesPage => (string)this.options.GetValue(OPTIONS, null);

        /// <summary>
        /// Gets a value indicating whether this instance is open post.
        /// </summary>
        /// <value><c>true</c> if this instance is open post; otherwise, <c>false</c>.</value>
        public bool IsOpenPost => this.options.IsArgPresent(OPENPOST);

        /// <summary>
        /// Gets a value indicating whether this instance is post editor file.
        /// </summary>
        /// <value><c>true</c> if this instance is post editor file; otherwise, <c>false</c>.</value>
        public bool IsPostEditorFile => this.options.UnnamedArgCount > 0
                                        && PostEditorFile.IsValid(this.options.GetUnnamedArg(0, null));

        /// <summary>
        /// Gets the name of the post editor file.
        /// </summary>
        /// <value>The name of the post editor file.</value>
        public string PostEditorFileName => this.options.GetUnnamedArg(0, null);

        /// <summary>
        /// Gets the culture override.
        /// </summary>
        /// <value>The culture override.</value>
        public string CultureOverride => this.options.GetValue(CULTURE, null) as string;

        /// <summary>
        /// Gets a value indicating whether [add blog flag present].
        /// </summary>
        /// <value><c>true</c> if [add blog flag present]; otherwise, <c>false</c>.</value>
        public bool AddBlogFlagPresent => this.options.IsArgPresent(ADDBLOG);

        /// <summary>
        /// Gets the add blog.
        /// </summary>
        /// <value>The add blog.</value>
        public string AddBlog => this.options.GetValue(ADDBLOG, null) as string;
    }
}
