// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;
    using System.Diagnostics;

    using JetBrains.Annotations;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;

    /// <summary>
    /// Provides diagnostic services (logging and error display) to plugins.
    /// </summary>
    public static class PluginDiagnostics
    {
        /// <summary>
        /// Notify the user that an error has occurred.
        /// </summary>
        /// <param name="title">Error title (used as the error caption).</param>
        /// <param name="description">Error description (displayed within a scrolling
        /// text-box so can be longer and/or display diagnostic information).</param>
        public static void DisplayError([NotNull] string title, [NotNull] string description)
        {
            PluginDiagnostics.LogError($"{title}: {description}");

            var displayableException = new DisplayableException(title, description);
            using (var form = new DisplayableExceptionDisplayForm(displayableException))
            {
                form.ShowDialog(Win32WindowImpl.ForegroundWin32Window);
            }
        }

        /// <summary>
        /// Notify the user that an unexpected exception has occurred. In addition to
        /// displaying an error message to the user this method also automatically logs
        /// the unexpected exception in the Open Live Writer log file.
        /// </summary>
        /// <param name="ex">Unexpected exception.</param>
        public static void DisplayUnexpectedException([NotNull] Exception ex)
        {
            PluginDiagnostics.LogException(ex);

            UnexpectedErrorMessage.Show(ex);
        }

        /// <summary>
        /// Log an error that has occurred (writes to the Open Live Writer log file
        /// (located at C:\Documents and Settings\%USER%\Application Data\Open Live Writer).
        /// </summary>
        /// <param name="message">Error message to log.</param>
        public static void LogError([NotNull] string message)
        {
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Log an exception that has occurred (writes to the Open Live Writer log file
        /// (located at C:\Documents and Settings\%USER%\Application Data\Open Live Writer)
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public static void LogException([NotNull] Exception ex)
        {
            PluginDiagnostics.LogException(ex, string.Empty);
        }

        /// <summary>
        /// Log an exception that has occurred (writes to the Open Live Writer log file
        /// (located at C:\Documents and Settings\%USER%\Application Data\Open Live Writer)
        /// </summary>
        /// <param name="ex">Exception to log</param>
        /// <param name="context">Additional context on the circumstances of the exception.</param>
        public static void LogException([NotNull] Exception ex, [NotNull] string context)
        {
            context = context != string.Empty ? $" ({context})" : string.Empty;
            Trace.WriteLine($"Exception{context}: {ex.ToString()}");
        }

    }
}
