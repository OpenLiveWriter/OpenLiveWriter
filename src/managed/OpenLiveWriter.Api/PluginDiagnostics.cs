// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Provides diagnostic services (logging and error display) to plugins.
    /// </summary>
    public sealed class PluginDiagnostics
    {
        /// <summary>
        /// Notify the user that an error has occurred.
        /// </summary>
        /// <param name="title">Error title (used as the error caption).</param>
        /// <param name="description">Error description (displayed within a scrolling
        /// text-box so can be longer and/or display diagnostic information).</param>
        public static void DisplayError(string title, string description)
        {
            LogError(String.Format(CultureInfo.CurrentCulture, "{0}: {1}", title, description));

            DisplayableException displayableException = new DisplayableException(title, description);
            using (DisplayableExceptionDisplayForm form = new DisplayableExceptionDisplayForm(displayableException))
                form.ShowDialog(Win32WindowImpl.ForegroundWin32Window);
        }

        /// <summary>
        /// Notify the user that an unexpected exception has occurred. In addition to
        /// displaying an error message to the user this method also automatically logs
        /// the unexpected exception in the Open Live Writer log file.
        /// </summary>
        /// <param name="ex">Unexpected exception.</param>
        public static void DisplayUnexpectedException(Exception ex)
        {
            LogException(ex);

            UnexpectedErrorMessage.Show(ex);
        }

        /// <summary>
        /// Log an error that has occurred (writes to the Open Live Writer log file
        /// (located at C:\Documents and Settings\%USER%\Application Data\Open Live Writer).
        /// </summary>
        /// <param name="message">Error message to log.</param>
        public static void LogError(string message)
        {
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Log an exception that has occurred (writes to the Open Live Writer log file
        /// (located at C:\Documents and Settings\%USER%\Application Data\Open Live Writer)
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public static void LogException(Exception ex)
        {

            LogException(ex, String.Empty);
        }

        /// <summary>
        /// Log an exception that has occurred (writes to the Open Live Writer log file
        /// (located at C:\Documents and Settings\%USER%\Application Data\Open Live Writer)
        /// </summary>
        /// <param name="ex">Exception to log</param>
        /// <param name="context">Additional context on the circumstances of the exception.</param>
        public static void LogException(Exception ex, string context)
        {
            context = context != String.Empty ? String.Format(CultureInfo.InvariantCulture, " ({0})", context) : String.Empty;
            Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Exception{0}: {1}", context, ex.ToString()));
        }

    }
}
