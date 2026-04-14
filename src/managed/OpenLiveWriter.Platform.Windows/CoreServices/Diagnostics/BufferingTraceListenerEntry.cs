// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// BufferingTraceListenerEntry.
    /// </summary>
    public class BufferingTraceListenerEntry
    {
        /// <summary>
        /// The sequence number of the entry.
        /// </summary>
        private int sequenceNumber;

        /// <summary>
        /// The DateTime the entry was created.
        /// </summary>
        private DateTime dateTime;

        /// <summary>
        /// The category of the entry.
        /// </summary>
        private string category;

        /// <summary>
        /// The text of the entry.
        /// </summary>
        private string text;

        /// <summary>
        /// The stack trace of the entry.
        /// </summary>
        private string stackTrace;

        /// <summary>
        /// Initializes a new instance of the BufferingTraceListenerEntry class.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the entry.</param>
        /// <param name="category">The category of the entry.</param>
        /// <param name="text">The text of the entry.</param>
        /// <param name="stackTrace">The stack trace of the entry.</param>
        public BufferingTraceListenerEntry(int sequenceNumber, string category, string text, string stackTrace)
        {
            this.sequenceNumber = sequenceNumber;
            this.dateTime = DateTime.Now;
            this.category = category == null ? String.Empty : category;
            this.text = text == null ? String.Empty : text;
            this.stackTrace = stackTrace == null ? String.Empty : stackTrace;
        }

        /// <summary>
        /// Gets the sequence number.
        /// </summary>
        public int SequenceNumber
        {
            get
            {
                return sequenceNumber;
            }
        }

        /// <summary>
        /// Gets the sequence number as a string.
        /// </summary>
        public string SequenceNumberString
        {
            get
            {
                return SequenceNumber.ToString("00000", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the DateTime the entry was created.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return dateTime;
            }
        }

        /// <summary>
        /// Gets the DateTime the entry was created as a string.
        /// </summary>
        public string DateTimeString
        {
            get
            {
                return DateTime.ToString("hh:mm:ss:ff tt", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the category of the entry.
        /// </summary>
        public string Category
        {
            get
            {
                return category;
            }
        }

        /// <summary>
        /// Gets the text of the entry.
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
        }

        /// <summary>
        /// Gets the stack trace of the entry.
        /// </summary>
        public string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
        public override string ToString()
        {
            if (StackTrace.Length == 0)
                return String.Format(CultureInfo.InvariantCulture,
                                        "{0} at {1} ({2})\r\n{3}",
                                        SequenceNumberString,
                                        DateTimeString,
                                        Category,
                                        Text);
            else
                return String.Format(CultureInfo.InvariantCulture,
                                        "{0} at {1} ({2})\r\n{3}\r\nStack Trace:\r\n{4}",
                                        SequenceNumberString,
                                        DateTimeString,
                                        Category,
                                        Text,
                                        StackTrace);
        }
    }
}
