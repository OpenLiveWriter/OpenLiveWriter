// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// A TraceListener that write trace events to a log file.
    /// </summary>
    public class LogFileTraceListener : TraceListener
    {
        #region Static & Constant Declarations

        /// <summary>
        /// Fail text.
        /// </summary>
        private const string FailText = "Fail";

        #endregion

        #region Private Member Variables

        /// <summary>
        /// The sequence number.
        /// </summary>
        private int sequenceNumber;

        /// <summary>
        /// The size at which the log file should be archived and a new one created.
        /// This number is bigger in Debug configuration because we log a lot more messages.
        /// </summary>
#if DEBUG
        private int LOG_FILE_SIZE_THRESHOLD = 5000000;
#else
        private const int LOG_FILE_SIZE_THRESHOLD = 1000000;
#endif

        private FileLogger logger;

        /// <summary>
        /// The facility.
        /// </summary>
        private string facility;

        /// <summary>
        /// The ID of the current process.
        /// </summary>
        private static readonly string processId;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        static LogFileTraceListener()
        {
            int pid;
            using (Process p = Process.GetCurrentProcess())
                pid = p.Id;

            uint sid;
            // ProcessIdToSessionId returns false if session id couldn't be determined
            if (!Kernel32.ProcessIdToSessionId((uint)pid, out sid))
                processId = "??." + pid;
            else if (sid == 0)
                processId = pid.ToString(CultureInfo.InvariantCulture);
            else
                processId = sid + "." + pid;
        }

        /// <summary>
        /// Initializes a new instance of the LogFileTraceListener class.
        /// </summary>
        /// <param name="logFileName">The log file name to write to.</param>
        /// <param name="facility">The facility name.</param>
        public LogFileTraceListener(string logFileName, string facility)
        {
            logger = new FileLogger(logFileName, logFileName + ".old", LOG_FILE_SIZE_THRESHOLD);
            this.facility = facility;
        }

        #endregion Class Initialization & Termination

        #region Public Methods

        /// <summary>
        /// Emits the specified error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        public override void Fail(string message)
        {
            WriteEntry(message, FailText, Environment.StackTrace);
            OnFail();
        }

        /// <summary>
        /// Emits an error message, and a detailed error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        public override void Fail(string message, string detailMessage)
        {
            if (detailMessage != null && detailMessage.Length != 0)
                WriteEntry(String.Format(CultureInfo.InvariantCulture, "{0} {1}", message, detailMessage), FailText, Environment.StackTrace);
            else
                WriteEntry(message, FailText, Environment.StackTrace);
            OnFail();
        }

        private void OnFail()
        {
            if (ApplicationDiagnostics.AutomationMode)
            {
                WriteEntry("Assertion failed, exiting Writer");
                logger.Flush();
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Writes the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        public override void Write(object o)
        {
            WriteEntry(o.ToString());
        }

        /// <summary>
        /// Writes a message to the listener.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            WriteEntry(message);
        }

        /// <summary>
        /// Writes a category name and the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(object o, string category)
        {
            WriteEntry(o.ToString(), category);
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(string message, string category)
        {
            WriteEntry(message, category);
        }

        /// <summary>
        /// Writes the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        public override void WriteLine(object o)
        {
            WriteEntry(o.ToString());
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            WriteEntry(message);
        }

        /// <summary>
        /// Writes a category name and the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(object o, string category)
        {
            WriteEntry(o.ToString(), category);
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(string message, string category)
        {
            WriteEntry(message, category);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Adds an entry.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        private void WriteEntry(string message)
        {
            WriteEntry(message, null, null);
        }

        /// <summary>
        /// Adds an entry.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        /// <param name="category">The category of the entry.</param>
        private void WriteEntry(string message, string category)
        {
            WriteEntry(message, category, null);
        }

        /// <summary>
        /// Adds an entry.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        /// <param name="category">The category of the entry.</param>
        /// <param name="stackTrace">The stack trace of the entry.</param>
        private void WriteEntry(string message, string category, string stackTrace)
        {
            //	Obtain the DateTime the message reached us.
            DateTime dateTime = DateTime.Now;

            //	Default the message, as needed.
            if (message == null || message.Length == 0)
                message = "[No Message]";

            //	Default the category, as needed.
            if (category == null || category.Length == 0)
                category = "None";

            int seqNum = Interlocked.Increment(ref sequenceNumber);

            DebugLogEntry logEntry = new DebugLogEntry(facility, processId, seqNum, dateTime, message, category, stackTrace);

            logger.AddEntry(logEntry);
        }

        #endregion Private Methods

        public class DebugLogEntry
        {
            internal readonly string Facility;
            internal readonly string ProcessId;
            internal readonly int SequenceNumber;
            internal readonly DateTime TimeStamp;
            internal readonly string Message;
            internal readonly string Category;
            internal readonly string StackTrace;

            internal DebugLogEntry(string facility, string processId, int sequenceNumber, DateTime timestamp, string message, string category, string stackTrace)
            {
                this.Facility = facility;
                this.ProcessId = processId;
                this.SequenceNumber = sequenceNumber;
                this.TimeStamp = timestamp;
                this.Message = message;
                this.Category = category;
                this.StackTrace = stackTrace;
            }

            public override string ToString()
            {
                return
                    string.Format(CultureInfo.InvariantCulture,
                                  "{0},{1},{2},{3:00000},{4:dd-MMM-yyyy HH:mm:ss.fff},\"{5}\",\"{6}\"\r\n",
                                  Facility,
                                  ProcessId,
                                  Category,
                                  SequenceNumber,
                                  TimeStamp,
                                  Message.Replace("\"", "\"\""),
                                  StackTrace);
            }
        }

    }
}
