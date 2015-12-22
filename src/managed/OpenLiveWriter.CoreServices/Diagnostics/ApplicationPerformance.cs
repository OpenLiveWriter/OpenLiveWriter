// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// Handles logging of performance tracing events to a text file.
    ///
    /// Example usage:
    ///
    /// using (ApplicationPerformance.LogEvent("ApplyImageEffects"))
    /// {
    ///     // do work here
    ///     ApplyEffects(image);
    /// }
    /// </summary>
    public class ApplicationPerformance
    {
        private static FileLogger logger;
        [ThreadStatic]
        private static Dictionary<string, IDisposable> openEvents = new Dictionary<string, IDisposable>();

        private static Dictionary<string, IDisposable> OpenEvents
        {
            get
            {
                if (openEvents == null)
                    openEvents = new Dictionary<string, IDisposable>();

                return openEvents;
            }
        }

        public static void SetLogFilePath(string logFilePath)
        {
            if (logger != null)
            {
                IDisposable disp = logger;
                logger = null;
                disp.Dispose();
            }

            if (logFilePath != null)
            {
                logFilePath = Path.GetFullPath(logFilePath);
                if (File.Exists(logFilePath))
                    File.Delete(logFilePath);
                FileLogger newLogger = new FileLogger(logFilePath);
                newLogger.AddEntry(new CsvLogEntry("Name", "ElapsedMillis"));
                logger = newLogger;
            }
        }

        public static void FlushLogFile()
        {
            if (logger == null)
                throw new InvalidOperationException("Log file path has not been set");

            logger.Flush();
        }

        public static bool IsEnabled { get { return logger != null; } }

        public static void WriteEvent(string name, long millis)
        {
            if (logger != null)
                logger.AddEntry(new CsvLogEntry(name, millis.ToString(CultureInfo.InvariantCulture)));
        }

        public static IDisposable LogEvent(string name)
        {
            return IsEnabled ? new PerfLogger(name) : null;
        }

        private class PerfLogger : IDisposable
        {
            private readonly string name;
            private readonly Stopwatch stopWatch;

            public PerfLogger(string name)
            {
                this.name = name;
                stopWatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                stopWatch.Stop();
                WriteEvent(name, stopWatch.ElapsedMilliseconds);
            }
        }

        public static void StartEvent(string name)
        {
            if (IsEnabled)
            {
                // Commented out because it breaks automation
                //Debug.Assert(!OpenEvents.ContainsKey(name), "2 open events with the same name.");
                OpenEvents.Add(name, LogEvent(name));
            }

        }

        public static void EndEvent(string name)
        {
            if (IsEnabled)
            {
                Debug.Assert(OpenEvents.ContainsKey(name), "Trying to end an event that is not open.");
                OpenEvents[name].Dispose();
                OpenEvents.Remove(name);
            }
        }

        public static bool ContainsEvent(string name)
        {
            if (!IsEnabled)
            {
                return false;
            }

            return OpenEvents.ContainsKey(name);
        }

        public static void ClearEvent(string name)
        {
            if (IsEnabled)
            {
                OpenEvents.Remove(name);
            }
        }
    }
}
