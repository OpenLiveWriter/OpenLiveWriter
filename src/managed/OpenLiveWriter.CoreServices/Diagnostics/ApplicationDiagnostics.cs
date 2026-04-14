// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// ApplicationDiagnostics provides services for monitoring the health of an application.
    /// Cross-platform base: provides static diagnostic flags and basic trace initialization.
    /// Platform-specific features (LogFileTraceListener, DiagnosticsConsole, BufferingTraceListener)
    /// are in Platform.Windows.
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

        /// <summary>
        /// Initializes a new instance of the ApplicationDiagnostics class.
        /// Platform-specific trace listeners should be added by the platform initializer.
        /// </summary>
        public ApplicationDiagnostics(string logFilePath, string logFileFacility)
        {
            Trace.Listeners.Clear();

            if (ApplicationDiagnostics.TestMode)
            {
                Trace.Listeners.Add(new DefaultTraceListener());
            }
        }

        /// <summary>
        /// Shows the DiagnosticsConsole form (Windows-only, via Platform.Windows).
        /// Override in platform-specific subclass to provide UI.
        /// </summary>
        public virtual void ShowDiagnosticsConsole(string title)
        {
            // No-op in cross-platform base
        }
    }
}
