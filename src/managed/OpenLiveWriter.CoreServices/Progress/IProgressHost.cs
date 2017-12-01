// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Interface used by progress-compatible methods to report their progress back to the host operation.
    /// </summary>
    public interface IProgressHost
    {
        /// <summary>
        /// Notifies the host that progress has been made.
        /// </summary>
        /// <param name="complete">a number indicating what fraction of the total expected progress has been made
        /// (note: this number should always be less than the total)</param>
        /// <param name="total">the total expected progress that the operation will perform.</param>
        /// <param name="message">a message describing the current progress</param>
        void UpdateProgress(int complete, int total, string message);

        /// <summary>
        /// Notifies the host that progress has been made.
        /// </summary>
        /// <param name="complete">a number indicating what fraction of the total expected progress has been made</param>
        /// <param name="total">the total expected progress that the operation will perform.</param>
        void UpdateProgress(int complete, int total);

        /// <summary>
        /// Notifies the host that progress has been made.
        /// </summary>
        /// <param name="message">a message describing the current progress</param>
        void UpdateProgress(string message);

        /// <summary>
        /// Gets the operation's cancel status.  IProgressOperations should query this property frequently to determine
        /// if this operation should be aborted.  If this property returns true, the IProgressOperation should abort as
        /// quickly as possible.
        /// </summary>
        bool CancelRequested { get; }

        double ProgressCompletionPercentage { get; }
    }

    /// <summary>
    /// Delegate signature implemented by methods that can be used as part of a progress operation.
    /// The IProgressHost object can be used to report/query progress information while the operation
    /// is executing.
    /// </summary>
    public delegate object ProgressOperation(IProgressHost progress);

    /// <summary>
    /// Delegate used for notifying controlling code that an individual progress operation
    /// has completed -- the return object from the ProgressOperation is passed as a parameter
    /// </summary>
    public delegate void ProgressOperationCompleted(object result);
}
