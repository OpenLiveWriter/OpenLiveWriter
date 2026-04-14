// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.Platform.Windows.BlogClient
{
    /// <summary>
    /// Windows/WinForms implementation of IDialogService.
    /// Uses delegate-based callbacks for DisplayMessage and exception display,
    /// which are registered at startup by the application layer (which has access
    /// to OpenLiveWriter.Controls).
    /// </summary>
    public class WindowsDialogService : IDialogService
    {
        private readonly Func<string, IntPtr, object[], DialogResultValue> _showMessageFunc;
        private readonly Action<IntPtr, Exception> _showExceptionFunc;

        /// <summary>
        /// Creates a WindowsDialogService with callbacks for showing messages and exceptions.
        /// </summary>
        /// <param name="showMessageFunc">Callback to show a message by messageId string. Returns DialogResultValue.</param>
        /// <param name="showExceptionFunc">Callback to show an exception dialog.</param>
        public WindowsDialogService(
            Func<string, IntPtr, object[], DialogResultValue> showMessageFunc,
            Action<IntPtr, Exception> showExceptionFunc)
        {
            _showMessageFunc = showMessageFunc ?? throw new ArgumentNullException(nameof(showMessageFunc));
            _showExceptionFunc = showExceptionFunc ?? throw new ArgumentNullException(nameof(showExceptionFunc));
        }

        public DialogResultValue ShowMessage(string messageId, IntPtr ownerHandle, params object[] parameters)
        {
            return _showMessageFunc(messageId, ownerHandle, parameters);
        }

        public void ShowException(IntPtr ownerHandle, Exception ex)
        {
            _showExceptionFunc(ownerHandle, ex);
        }

        public IDisposable ShowWaitCursor()
        {
            return new WaitCursor();
        }
    }
}
