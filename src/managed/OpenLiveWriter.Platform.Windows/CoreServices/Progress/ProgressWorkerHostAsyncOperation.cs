// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// AsyncOperation class that can host ProgressWorker objects.
    /// </summary>
    public abstract class ProgressWorkerHostAyncOperation : AsyncOperation, IProgressHost
    {
        public ProgressWorkerHostAyncOperation(ISynchronizeInvoke target) : base(target)
        {
        }

        /// <summary>
        /// Allows operations to to move the progress indicator backwards.  By default, this is not allowed.
        /// </summary>
        public bool AllowBackwardsProgress
        {
            get
            {
                return allowBackwardsProgress;
            }
            set
            {
                allowBackwardsProgress = value;
            }
        }
        private bool allowBackwardsProgress = false;

        private int lastComplete;
        private int lastTotal;
        private string lastMessage = "";

        public bool GetCancelRequested()
        {
            return this.CancelRequested;
        }

        #region IProgressHost Members

        void IProgressHost.UpdateProgress(int complete, int total, string message)
        {
            ValidateProgressUpdate(ref complete, ref total);
            base.UpdateProgress(complete, total, message);
            lastComplete = complete;
            lastTotal = total;
            lastMessage = message;
        }

        void IProgressHost.UpdateProgress(int complete, int total)
        {
            ValidateProgressUpdate(ref complete, ref total);
            base.UpdateProgress(complete, total, lastMessage);
            lastComplete = complete;
            lastTotal = total;
        }

        void IProgressHost.UpdateProgress(string message)
        {
            base.UpdateProgress(lastComplete, lastTotal, message);
            lastMessage = message;
        }

        private void ValidateProgressUpdate(ref int complete, ref int total)
        {
            //In most cases, backwards moving progress indicates a bug with progress
            //operations, so report a bug if backwards moving progress is not explicitly
            //enabled.
#if DEBUG //Note: we #if DEBUG this to avoid .NET compiler issue (see bug #2864 for details) that causes very flakey behavior down the road

            if (!AllowBackwardsProgress)
            {
                //verify that progress is not slipping backwards.
                double lastProgressCompletion = (this as IProgressHost).ProgressCompletionPercentage;
                double currProgressCompletion = total == 0 ? 0 : ((double)complete) / total;
                if (lastProgressCompletion > currProgressCompletion)
                    Debug.WriteLine("Progress backtracked - set AllowBackwardsProgress=true to allow backwards progress");
            }

            if (complete > total)
            {
                //this condition indicates that the progress was sloppy, so fix it up
                Debug.Fail("progress exceeded 100%");
                complete = total;
            }
#endif
        }

        double IProgressHost.ProgressCompletionPercentage
        {
            get
            {
                if (lastTotal == 0 || lastComplete == 0)
                    return 0;
                else
                    return ((double)lastComplete) / lastTotal;
            }
        }

        bool IProgressHost.CancelRequested
        {
            get
            {
                return base.CancelRequested;
            }
        }

        #endregion
    }
}
