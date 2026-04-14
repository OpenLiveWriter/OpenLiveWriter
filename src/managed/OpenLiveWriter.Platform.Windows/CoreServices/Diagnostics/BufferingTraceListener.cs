// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// A TraceListener that buffers trace events.
    /// </summary>
    public class BufferingTraceListener : TraceListener
    {
        #region Static & Constant Declarations

        #endregion

        #region Private Member Variables

        /// <summary>
        /// The sequence number.
        /// </summary>
        private int sequenceNumber;

        /// <summary>
        /// The entry list.
        /// </summary>
        private ArrayList entryList = new ArrayList();

        /// <summary>
        /// The changed event.
        /// </summary>
        public event EventHandler Changed;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the BufferingTraceListener class.
        /// </summary>
        public BufferingTraceListener()
        {
        }

        #endregion Class Initialization & Termination

        #region Public Methods

        /// <summary>
        /// Gets an array of BufferingTraceListenerEntry objects starting at the specified index.
        /// </summary>
        /// <param name="index">The index at which to return BufferingTraceListenerEntry objects.  This value is updated.</param>
        /// <returns>An array of BufferingTraceListenerEntry objects, or null.</returns>
        public BufferingTraceListenerEntry[] GetEntries(ref int index)
        {
            //	Return what was asked for.
            lock (this)
            {
                //	Validate startIndex.
                if (index < 0 || index > entryList.Count)
                    throw new ArgumentOutOfRangeException("index", index, "Index out of range.");

                //	If there are no new entries to return, return null.
                if (index != entryList.Count)
                {
                    int returnCount = entryList.Count - index;
                    BufferingTraceListenerEntry[] bufferingTraceListenerEntryArray = new BufferingTraceListenerEntry[returnCount];
                    entryList.CopyTo(index, bufferingTraceListenerEntryArray, 0, returnCount);
                    index = entryList.Count;
                    return bufferingTraceListenerEntryArray;
                }
            }

            //	Done.
            return new BufferingTraceListenerEntry[0];
        }

        /// <summary>
        /// Emits the specified error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        public override void Fail(string message)
        {
            //	Add the message.
            AddEntry(message, ErrText.FailText, Environment.StackTrace);
        }

        /// <summary>
        /// Emits an error message, and a detailed error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        public override void Fail(string message, string detailMessage)
        {
            //	Add the message.
            if (detailMessage != null && detailMessage.Length != 0)
                AddEntry(String.Format(CultureInfo.InvariantCulture, "{0} {1}", message, detailMessage), ErrText.FailText, Environment.StackTrace);
            else
                AddEntry(message, ErrText.FailText, Environment.StackTrace);
        }

        /// <summary>
        /// Writes the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        public override void Write(object o)
        {
            AddEntry(o.ToString());
        }

        /// <summary>
        /// Writes a message to the listener.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            AddEntry(message);
        }

        /// <summary>
        /// Writes a category name and the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(object o, string category)
        {
            AddEntry(o.ToString(), category);
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(string message, string category)
        {
            AddEntry(message, category);
        }

        /// <summary>
        /// Writes the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        public override void WriteLine(object o)
        {
            AddEntry(o.ToString());
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            AddEntry(message);
        }

        /// <summary>
        /// Writes a category name and the value of the object's ToString method.
        /// </summary>
        /// <param name="o">An Object whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(object o, string category)
        {
            AddEntry(o.ToString(), category);
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(string message, string category)
        {
            AddEntry(message, category);
        }

        #endregion Public Methods

        #region Protected Events

        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        #endregion Protected Events

        #region Private Methods

        /// <summary>
        /// Adds an entry.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        private void AddEntry(string message)
        {
            AddEntry(message, null, null);
        }

        /// <summary>
        /// Adds an entry.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        /// <param name="category">The category of the entry.</param>
        private void AddEntry(string message, string category)
        {
            AddEntry(message, category, null);
        }

        /// <summary>
        /// Adds an entry.
        /// </summary>
        /// <param name="message">The message of the entry.</param>
        /// <param name="category">The category of the entry.</param>
        /// <param name="stackTrace">The stack trace of the entry.</param>
        private void AddEntry(string message, string category, string stackTrace)
        {
            //	Add the entry.
            lock (this)
                entryList.Add(new BufferingTraceListenerEntry(sequenceNumber++, category, message, stackTrace));

            //	Raise the Changed event.
            OnChanged(EventArgs.Empty);
        }

        #endregion Private Methods
    }
}
