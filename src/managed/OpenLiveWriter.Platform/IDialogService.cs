// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Platform
{
    /// <summary>
    /// Result of a dialog operation, matching common dialog semantics.
    /// </summary>
    public enum DialogResultValue
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
    }

    /// <summary>
    /// Platform-agnostic interface for showing messages and error dialogs.
    /// Replaces direct usage of DisplayMessage.Show and DisplayableExceptionDisplayForm.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Show a message dialog identified by messageId. The messageId string corresponds
        /// to the MessageId enum names from the Localization layer.
        /// </summary>
        DialogResultValue ShowMessage(string messageId, IntPtr ownerHandle, params object[] parameters);

        /// <summary>
        /// Show an error/exception dialog.
        /// </summary>
        void ShowException(IntPtr ownerHandle, Exception ex);

        /// <summary>
        /// Show a wait cursor scoped to the returned IDisposable lifetime.
        /// Returns null if not applicable (e.g., background thread or non-GUI platform).
        /// </summary>
        IDisposable ShowWaitCursor();
    }
}
