// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Interface that represents an object that can provide progress, cancel,
    /// failure, and competed notification.
    /// </summary>
    public interface IProgressProvider
    {
        /// <summary>
        /// Cancels the operation
        /// </summary>
        void Cancel();

        /// <summary>
        /// Provides notification that the operation has made progress
        /// </summary>
        event ProgressUpdatedEventHandler ProgressUpdated;

        /// <summary>
        /// Provides notification that the operation had completed.
        /// </summary>
        event EventHandler Completed;

        /// <summary>
        /// Provides notification that the operation has successfully cancelled.
        /// </summary>
        event EventHandler Cancelled;

        /// <summary>
        /// Provides notification that the operation has failed.
        /// </summary>
        event ThreadExceptionEventHandler Failed;
    }
}
