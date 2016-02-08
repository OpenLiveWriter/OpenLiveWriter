// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter
{
    public class WriterCommandLineOptions
    {
        private readonly CommandLineOptions _options;
        private const string CULTURE = "culture";
        private const string OPTIONS = "options";
        private const string OPENPOST = "openpost";
        private const string NOPLUGINS = "noplugins";
        private const string TESTMODE = "testmode";
        private const string VERBOSELOGGING = "verbose";
        private const string ALLOWUNSAFECERTIFICATES = "allowunsafecertificates";
        private const string PREFERATOM = "preferatom";
        private const string SUPPRESSBACKGROUNDREQUESTS = "suppressbackgroundrequests";
        private const string PROXY = "proxy";
        private const string PERFLOG = "perflog";
        private const string AUTOMATIONMODE = "automation";
        private const string ATTACHDEBUGGER = "attach";
        private const string FIRSTRUN = "firstrun";
        private const string LOCSPY = "locspy";
        private const string INTAPIHOST = "intapihost";
        private const string ADDBLOG = "addblog";

        private WriterCommandLineOptions(CommandLineOptions options)
        {
            _options = options;
        }

        public void ApplyOptions()
        {
            try
            {
                if (_options.IsArgPresent(ATTACHDEBUGGER))
                    Debugger.Launch();

                if (_options.IsArgPresent(CULTURE))
                {
                    string culture = _options.GetValue(CULTURE, null) as string;
                    if (culture != null)
                    {
                        CultureHelper.ApplyUICulture(culture);
                    }
                }

#if DEBUG
                if (_options.IsArgPresent(TESTMODE))
                    ApplicationDiagnostics.TestMode = _options.GetFlagValue(TESTMODE, ApplicationDiagnostics.TestMode);

                if (_options.IsArgPresent(VERBOSELOGGING))
                    ApplicationDiagnostics.VerboseLogging = _options.GetFlagValue(VERBOSELOGGING, ApplicationDiagnostics.VerboseLogging);

                if (_options.IsArgPresent(ALLOWUNSAFECERTIFICATES))
                    ApplicationDiagnostics.AllowUnsafeCertificates = _options.GetFlagValue(ALLOWUNSAFECERTIFICATES, ApplicationDiagnostics.AllowUnsafeCertificates);

                if (_options.IsArgPresent(PREFERATOM))
                    ApplicationDiagnostics.PreferAtom = _options.GetFlagValue(PREFERATOM, ApplicationDiagnostics.PreferAtom);

                if (_options.IsArgPresent(SUPPRESSBACKGROUNDREQUESTS))
                    ApplicationDiagnostics.SuppressBackgroundRequests = _options.GetFlagValue(SUPPRESSBACKGROUNDREQUESTS, ApplicationDiagnostics.SuppressBackgroundRequests);

                if (_options.IsArgPresent(PROXY))
                    ApplicationDiagnostics.ProxySettingsOverride = (string)_options.GetValue(PROXY, ApplicationDiagnostics.ProxySettingsOverride);

                if (_options.IsArgPresent(PERFLOG))
                    ApplicationPerformance.SetLogFilePath((string)_options.GetValue(PERFLOG, null));

                if (_options.IsArgPresent(AUTOMATIONMODE))
                    ApplicationDiagnostics.AutomationMode = true;

                if (_options.IsArgPresent(FIRSTRUN))
                    ApplicationDiagnostics.SimulateFirstRun = true;

                if (_options.IsArgPresent(INTAPIHOST))
                    ApplicationDiagnostics.IntServerOverride = (string)_options.GetValue(INTAPIHOST, null);
#endif

#if !SIGNED
                if (_options.IsArgPresent(LOCSPY))
                    Res.DebugMode = true;
#endif
            }
            catch (Exception e)
            {
                Debug.Fail("Unable to apply culture:\r\n\r\n" + e.ToString());
            }
        }

        public static WriterCommandLineOptions Create(string[] args)
        {
            CommandLineOptions options = new CommandLineOptions(false, 0, int.MaxValue,
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

            bool success = options.Parse(args, false);
            if (!success)
            {
                MessageBox.Show(options.ErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, (BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0));
                return null;
            }
            else
                return new WriterCommandLineOptions(options);
        }

        public bool IsShowPreferences
        {
            get { return _options.IsArgPresent(OPTIONS); }
        }

        public string PreferencesPage
        {
            get { return (string)_options.GetValue(OPTIONS, null); }
        }

        public bool IsOpenPost
        {
            get { return _options.IsArgPresent(OPENPOST); }
        }

        public bool IsPostEditorFile
        {
            get
            {
                return _options.UnnamedArgCount > 0
                       && PostEditorFile.IsValid(_options.GetUnnamedArg(0, null));
            }
        }

        public string PostEditorFileName
        {
            get { return _options.GetUnnamedArg(0, null); }
        }

        public string CultureOverride
        {
            get { return _options.GetValue(CULTURE, null) as string; }
        }

        public bool AddBlogFlagPresent
        {
            get { return _options.IsArgPresent(ADDBLOG); }
        }

        public string AddBlog
        {
            get { return _options.GetValue(ADDBLOG, null) as string; }
        }
    }
}
