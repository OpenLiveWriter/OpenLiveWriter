// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Wraps a progress host and stops some or all (or no) messages from getting through.
    /// </summary>
    public class ProgressHostFilter : IProgressHost
    {
        [Flags]
        public enum MessageType : int
        {
            None = 0,
            UpdateCompletion = 1,
            UpdateMessage = 2,
            CancelRequested = 4,
            ProgressCompletionPercentage = 8,

            All = UpdateCompletion | UpdateMessage | CancelRequested | ProgressCompletionPercentage
        }

        private readonly IProgressHost pHost;
        private readonly MessageType filtered;

        /// <summary>
        /// Creates a new filter.
        /// </summary>
        /// <param name="pHost">The underlying progress host.</param>
        /// <param name="filtered">The message types that should be EXCLUDED.</param>
        public ProgressHostFilter(IProgressHost pHost, MessageType filtered)
        {
            this.pHost = pHost;
            this.filtered = filtered;
        }

        public void UpdateProgress(int complete, int total, string message)
        {
            if (Allowed(MessageType.UpdateCompletion) && Allowed(MessageType.UpdateMessage))
                pHost.UpdateProgress(complete, total, message);
            else if (Allowed(MessageType.UpdateCompletion))
                pHost.UpdateProgress(complete, total);
            else if (Allowed(MessageType.UpdateMessage))
                pHost.UpdateProgress(message);
        }

        public void UpdateProgress(int complete, int total)
        {
            if (Allowed(MessageType.UpdateCompletion))
                pHost.UpdateProgress(complete, total);
        }

        public void UpdateProgress(string message)
        {
            if (Allowed(MessageType.UpdateMessage))
                pHost.UpdateProgress(message);
        }

        public bool CancelRequested
        {
            get { return Allowed(MessageType.CancelRequested) ? pHost.CancelRequested : false; }
        }

        public double ProgressCompletionPercentage
        {
            get { return Allowed(MessageType.ProgressCompletionPercentage) ? pHost.ProgressCompletionPercentage : 0f; }
        }

        private bool Allowed(MessageType messageType)
        {
            return (filtered & messageType) == 0;
        }

    }
}
