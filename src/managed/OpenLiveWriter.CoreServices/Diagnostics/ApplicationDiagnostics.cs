// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// ApplicationDiagnostics provides services for monitoring the health of an application.
    /// </summary>
    public class ApplicationDiagnostics
    {
        private static bool testMode;
        private static bool automationMode;
        private static bool verboseLogging;
        private static bool allowUnsafeCertificates;
        private static bool preferAtom;
        private static bool simulateFirstRun = false;
        private static bool suppressBackgroundRequests = false;
        private static string proxySettingsOverride;
        private static string intServerOverride;

        static ApplicationDiagnostics()
        {
#if DEBUG
            testMode = true;
            verboseLogging = true;
            allowUnsafeCertificates = true;
#else
            testMode = false;
            verboseLogging = false;
            allowUnsafeCertificates = false;
#endif
        }
        public static bool TestMode
        {
            get { return testMode; }
            set
            {
#if !SIGNED
                if (testMode != value)
                {
                    testMode = value;
                    if (TestModeChanged != null)
                        TestModeChanged(null, EventArgs.Empty);
                }
#endif
            }
        }

#pragma warning disable 0067
        public static event EventHandler TestModeChanged;
#pragma warning restore 0067

        public static bool AutomationMode
        {
            get { return automationMode; }
            set { automationMode = value; }
        }

        public static bool VerboseLogging
        {
            get { return verboseLogging; }
            set { verboseLogging = value; }
        }

        public static bool AllowUnsafeCertificates
        {
            get { return allowUnsafeCertificates; }
            set { allowUnsafeCertificates = value; }
        }

        public static bool PreferAtom
        {
            get { return preferAtom; }
            set { preferAtom = value; }
        }

        public static bool SimulateFirstRun
        {
            get { return simulateFirstRun; }
            set { simulateFirstRun = value; }
        }

        public static bool SuppressBackgroundRequests
        {
            get { return suppressBackgroundRequests; }
            set { suppressBackgroundRequests = value; }
        }

        public static string ProxySettingsOverride
        {
            get { return proxySettingsOverride; }
            set { proxySettingsOverride = value; }
        }

        public static string IntServerOverride
        {
            get { return intServerOverride; }
            set { intServerOverride = value; }
        }

        #region Private Member Variables

        /// <summary>
        /// The LogFileTraceListener.
        /// </summary>
        private LogFileTraceListener logFileTraceListener;

        /// <summary>
        /// The BufferingTraceListener we attach as a listener to Trace and Debug.
        /// </summary>
        private BufferingTraceListener bufferingTraceListener;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the ApplicationDiagnostics class.
        /// </summary>
        public ApplicationDiagnostics(string logFilePath, string logFileFacility)
        {
            Trace.Listeners.Clear();

            //	Instantiate the BufferingTraceListener.
            bufferingTraceListener = new BufferingTraceListener();

            //	Connect the BufferingTraceListener to the Trace event streams.
            Trace.Listeners.Add(bufferingTraceListener);

            //	Instantiate the LogFileTraceListener.
            logFileTraceListener = new LogFileTraceListener(logFilePath, logFileFacility);

            //	Add our LogFileTraceListener to the Trace event stream.
            Trace.Listeners.Add(logFileTraceListener);

            //	Don't include the default Trace listeners in release builds, so that we will not display
            //	trace assertions in the default way.
            if (ApplicationDiagnostics.TestMode)
            {
                Trace.Listeners.Add(new DefaultTraceListener());
            }
        }

        #endregion Class Initialization & Termination

        #region Public Methods

        /// <summary>
        /// Shows the DiagnosticsConsole form.
        /// </summary>
        /// <param name="title">The title of the DiagnosticsConsole.</param>
        public void ShowDiagnosticsConsole(string title)
        {
#if DEBUG
            lock (this)
            {
                DiagnosticsConsole diagnosticsConsole = new DiagnosticsConsole(bufferingTraceListener, title);
                diagnosticsConsole.Run();
            }
#endif
        }

        public BufferingTraceListenerEntry[] GetLogBuffer()
        {
            int count = 0;
            return bufferingTraceListener.GetEntries(ref count);
        }

        public DiagnosticsConsole GetDiagnosticsConsole(string title)
        {
            lock (this)
            {
#if DEBUG
                return new DiagnosticsConsole(bufferingTraceListener, title);
#else
                throw new NotSupportedException("Diagnostic console is only available in debug mode");
#endif
            }
        }

        #endregion Public Methods
    }
}
