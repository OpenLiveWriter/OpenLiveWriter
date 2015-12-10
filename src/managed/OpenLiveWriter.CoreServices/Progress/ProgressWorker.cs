// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// An internal utility class for managing the execution of ProgressTicks.
    /// ProgressWorkers understand how to convert the progress reported by the operation
    /// into an allocated size in the overall progress.
    /// </summary>
    public class ProgressWorker
    {
        private ProgressOperationCompleted CompletedMethod;
        private ProgressOperation WorkerMethod;
        private IProgressHost ParentProgress;
        private int ProgressSize;

        public ProgressWorker(ProgressOperation method, int progressSize, int progressTotal, IProgressHost progress)
            : this(method, null, null, progressSize, progressTotal, progress)
        {
        }

        public ProgressWorker(ProgressOperation method, ProgressCategory category, int progressSize, int progressTotal, IProgressHost progress)
            : this(method, category, null, progressSize, progressTotal, progress)
        {
        }

        public ProgressWorker(ProgressOperation method, ProgressOperationCompleted completedMethod, int progressSize, int progressTotal, IProgressHost progress)
            : this(method, null, completedMethod, progressSize, progressTotal, progress)
        {
        }

        public ProgressWorker(ProgressOperation method, ProgressCategory category, ProgressOperationCompleted completedMethod, int progressSize, int progressTotal, IProgressHost progress)
        {
            WorkerMethod = method;
            _category = category;
            CompletedMethod = completedMethod;
            ProgressSize = progressSize;
            ParentProgress = progress;
            TotalProgressTicks = progressTotal;
        }

        public void DoWork()
        {
            ProgressTick progressTick = new ProgressTick(ParentProgress, ProgressSize, TotalProgressTicks);
            if (progressTick.CancelRequested)
                throw new OperationCancelledException();

            _operationResult = WorkerMethod(progressTick);

            if (progressTick.CancelRequested)
                throw new OperationCancelledException();

            progressTick.UpdateProgress(100, 100); //complete progress for the operation
            if (CompletedMethod != null)
                CompletedMethod(_operationResult);
        }

        /// <summary>
        /// The total number of ticks in the progress. This is the number that the progress size value is relative to.
        /// </summary>
        public int TotalProgressTicks
        {
            get
            {
                return totalProgressTicks;
            }
            set
            {
                totalProgressTicks = value;
            }
        }
        int totalProgressTicks = 100;

        public object ProgressOperationResult
        {
            get
            {
                return _operationResult;
            }
        }
        private object _operationResult;

        /// <summary>
        /// Category that progress operation belongs to
        /// </summary>
        public ProgressCategory Category
        {
            get
            {
                return _category;
            }
        }
        private readonly ProgressCategory _category;
    }
}
