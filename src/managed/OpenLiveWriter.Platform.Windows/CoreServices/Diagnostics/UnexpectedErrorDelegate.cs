// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// Top Level exception handling delegate.
    /// provides conveniences for registering default handlers; RegisterWindowsHandler() for
    /// GUI apps, RegisterConsoleHandler()for non-GUI apps.
    /// </summary>
    public class UnexpectedErrorDelegate
    {

        /// <summary>
        /// Registers a listener for top-level GUI event exceptions, redirecting them through the
        /// delegate.
        /// </summary>
        public static void RegisterWindowsHandler()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(WindowsExceptionHandler);
        }

        /// <summary>
        /// Registers a listener for top-level application exceptions, for non-GUI applications.
        /// </summary>
        public static void RegisterConsoleHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ConsoleExceptionHandler);
        }

        /// <summary>
        /// All unhandled exceptions will ultimately end up here, (hopefully) providing a single
        /// point of contact for what action to take when it happens.
        ///
        /// Exceptions are divided into 2 categories. Non-CLR compliant exceptions, (usually the result
        /// of calling outside the .NET framework) CLR compliant exceptions.
        /// </summary>
        /// <param name="anyException"></param>
        public static void HandleUncaughtException(object anyException)
        {
            Exception exception = anyException as Exception;
            if (exception == null) // Non CLR compliant exception
            {
                // Non clr message
                UnexpectedErrorMessage.Show(new Exception("A non CLR exception has occurred and been unhandled."));
                return;
            }
            else
            {

                UnexpectedErrorMessage.Show(new ForeGroundWindow(), exception);
                return;
            }

        }

        // Delegate for GUI (Application) uncaught exceptions.
        private static void WindowsExceptionHandler(object oSender, ThreadExceptionEventArgs oEventArgs)
        {
            HandleUncaughtException(oEventArgs.Exception);
        }

        // Delegate for non-GUI (AppDomain) uncaught exceptions.
        private static void ConsoleExceptionHandler(object oSender, UnhandledExceptionEventArgs oEventArgs)
        {
            HandleUncaughtException(oEventArgs.ExceptionObject);
        }

        private class ForeGroundWindow : IWin32Window
        {
            #region IWin32Window Members

            public IntPtr Handle
            {
                get
                {

                    if (_handle == IntPtr.Zero)
                    {
                        _handle = User32.GetForegroundWindow();
                    }
                    return _handle;
                }
            }
            private IntPtr _handle = IntPtr.Zero;

            #endregion

        }

    }
}
