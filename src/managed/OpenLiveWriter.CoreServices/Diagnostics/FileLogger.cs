// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    public class FileLogger : IDisposable
    {
        private readonly object bufferLock = new object();
        private LogEntry head;
        private LogEntry tail;
        private bool canceled = false;

        /// <summary>
        /// The log file name.
        /// </summary>
        private readonly string logFileName;

        /// <summary>
        /// The name the log file should be moved to when it gets too big.
        /// </summary>
        private readonly string archiveLogFileName;

        private readonly int logFileSizeThreshold;

        public FileLogger(string logFileName) : this(logFileName, null, null)
        {
        }

        public FileLogger(string logFileName, string archiveLogFileName, int? logFileSizeThreshold)
        {
            this.logFileName = logFileName;
            this.archiveLogFileName = archiveLogFileName;
            this.logFileSizeThreshold = logFileSizeThreshold ?? int.MaxValue;

            Thread t = ThreadHelper.NewThread(WriteEntriesThreadStart, "Logging", false, false, true);
            t.Priority = ThreadPriority.BelowNormal;
            t.Start();
        }

        public void Dispose()
        {
            lock (bufferLock)
            {
                canceled = true;
                Monitor.Pulse(bufferLock);
            }
            GC.SuppressFinalize(this);
        }

        public void AddEntry(object entry)
        {
            LogEntry logEntry = new LogEntry(entry);

            lock (bufferLock)
            {
                if (head == null)
                    head = logEntry;
                if (tail != null)
                    tail.Next = logEntry;
                tail = logEntry;

                Monitor.Pulse(bufferLock);
            }
        }

        /// <summary>
        /// Block until the logger contains no entries
        /// and is done writing buffered entries
        /// </summary>
        public void Flush()
        {
            while (true)
            {
                lock (bufferLock)
                {
                    if (canceled)
                        throw new InvalidOperationException("Can't flush a disposed FileLogger");

                    if (head == null && !writesPending)
                    {
                        return;
                    }
                    Monitor.Pulse(bufferLock);
                }
                Thread.Sleep(10);
            }
        }

        private bool writesPending = false;

#if DEBUG
        private bool loggingFailed = false;
#endif
        private void WriteEntriesThreadStart()
        {
            FileInfo fileInfo = new FileInfo(logFileName);

            for (;;)
            {
                LogEntry localHead;

                lock (bufferLock)
                {
                    writesPending = false;

                    while (head == null && !canceled)
                    {
                        Monitor.Wait(bufferLock);
                    }

                    localHead = head;
                    head = null;
                    tail = null;

                    if (localHead == null && canceled)
                        return;

                    writesPending = true;
                }

                try
                {
                    // If file is larger than a preset threshold, delete
                    // the existing archive file if present and rename the
                    // file to the archive file.

                    if (archiveLogFileName != null)
                    {
                        fileInfo.Refresh(); // make file info flush its internal data cache
                        if (fileInfo.Length > logFileSizeThreshold)
                        {
                            File.Delete(archiveLogFileName);
                            File.Move(logFileName, archiveLogFileName);
                        }
                    }
                }
                catch
                {
                    // no big deal... maybe we'll have better luck next time.
                }

                try
                {
                    //	Try to write the message.  Incrementally back off, waiting for the file to
                    //  become available.  (The first backoff is 0ms intentionally -- to give up our
                    //  scheduling quantum -- allowing another thread to run.  Subsequent backoffs
                    //  increase linearly at 5ms intervals.)
                    for (int i = 0; ; i++)
                    {
                        try
                        {
                            //	Get a stream writer on the file.
                            using (StreamWriter streamWriter = File.AppendText(logFileName))
                            {
                                for (LogEntry le = localHead; le != null; le = le.Next)
                                {
                                    //	Attempt to write the entry.
                                    streamWriter.Write(le.Value.ToString());
                                }
                            }
                            break; // writes succeeded, don't try again
                        }
                        catch
                        {
                            //	Sleep.  Back off linearly, but not more than 2s.
                            Thread.Sleep(Math.Min(i * 5, 2000));
                        }
                    }
                }
                catch (Exception e)
                {
                    object foo = e;
                    // This should never happen, but if it does, we can't very well log about it.
#if DEBUG
                    if (!loggingFailed)
                        MessageBox.Show("Logging failed!\n\n" + e.ToString(), ApplicationEnvironment.ProductNameQualified);
                    loggingFailed = true;
#endif
                }
            }
        }

        private class LogEntry
        {
            public readonly object Value;
            public LogEntry Next;

            public LogEntry(object value)
            {
                Value = value;
            }
        }
    }

    public class CsvLogEntry
    {
        private string[] words;

        public CsvLogEntry(params string[] words)
        {
            this.words = words;
        }

        public override string ToString()
        {
            string[] escaped = new string[words.Length];
            for (int i = 0; i < escaped.Length; i++)
                escaped[i] = '"' + (words[i] ?? "").Replace("\"", "\"\"") + '"';
            return StringHelper.Join(escaped, ",") + "\r\n";
        }
    }

}
