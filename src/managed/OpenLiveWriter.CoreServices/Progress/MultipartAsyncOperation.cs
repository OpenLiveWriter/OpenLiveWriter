// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;

namespace OpenLiveWriter.CoreServices.Progress
{

    /// <summary>
    /// An AsyncOperation that serially executes a set of ProgressOperations.
    /// </summary>
    public class MultipartAsyncOperation : ProgressWorkerHostAyncOperation, IProgressCategoryProvider
    {
        private ArrayList workers;
        private int totalProgressTicks;

        /// <summary>
        /// Creates a new MultipartAsyncOperation.
        /// </summary>
        /// <param name="target"></param>
        public MultipartAsyncOperation(ISynchronizeInvoke target) : base(target)
        {
            workers = new ArrayList();
        }

        /// <summary>
        /// Adds a new ProgressOperation to the list of work to perform and assigns the operation
        /// the number of ticks that it should take up in the overall operation.
        /// </summary>
        /// <param name="operation">the progress-compatible method that will do some work</param>
        /// <param name="tickSize">an arbitrary number that should reflect the percentage of the work that will be done by this operation.
        /// Note: longer running operations should have a larger tick count than fast executing operations.</param>
        public void AddProgressOperation(ProgressOperation operation, int tickSize)
        {
            AddProgressOperation(operation, null, null, tickSize);
        }

        /// <summary>
        /// Adds a new ProgressOperation to the list of work to perform and assigns the operation
        /// the number of ticks that it should take up in the overall operation.
        /// </summary>
        /// <param name="operation">the progress-compatible method that will do some work</param>
        /// <param name="tickSize">an arbitrary number that should reflect the percentage of the work that will be done by this operation.
        /// Note: longer running operations should have a larger tick count than fast executing operations.</param>
        public void AddProgressOperation(ProgressOperation operation, ProgressCategory progressCategory, int tickSize)
        {
            AddProgressOperation(operation, progressCategory, null, tickSize);
        }

        /// <summary>
        /// Adds a new ProgressOperation to the list of work to perform and assigns the operation
        /// the number of ticks that it should take up in the overall operation.
        /// </summary>
        /// <param name="operation">the progress-compatible method that will do some work</param>
        /// <param name="completed">method called when the operation is completed</param>
        /// <param name="tickSize">an arbitrary number that should reflect the percentage of the work that will be done by this operation.
        /// Note: longer running operations should have a larger tick count than fast executing operations.</param>
        public void AddProgressOperation(ProgressOperation operation, ProgressOperationCompleted completed, int tickSize)
        {
            AddProgressOperation(operation, null, completed, tickSize);
        }

        /// <summary>
        /// Adds a new ProgressOperation to the list of work to perform and assigns the operation
        /// the number of ticks that it should take up in the overall operation.
        /// </summary>
        /// <param name="operation">the progress-compatible method that will do some work</param>
        /// <param name="completed">method called when the operation is completed</param>
        /// <param name="tickSize">an arbitrary number that should reflect the percentage of the work that will be done by this operation.
        /// Note: longer running operations should have a larger tick count than fast executing operations.</param>
        public void AddProgressOperation(ProgressOperation operation, ProgressCategory category, ProgressOperationCompleted completed, int tickSize)
        {
            // if a category is being specified then the connected UI should know that
            // it can and should show categories
            if (category != null)
                showCategories = true;

            //wrap the operation with a ProgressWorker that can manage its execution
            ProgressWorker worker = new ProgressWorker(operation, category, completed, tickSize, 100, this);

            //add the worker to the list
            workers.Add(worker);

            //add the ticks assigned to this task to the overall tick count
            totalProgressTicks += tickSize;
        }

        /// <summary>
        /// How many operations are being managed by the multipart async-op
        /// </summary>
        public int OperationCount
        {
            get
            {
                return workers.Count;
            }
        }

        /// <summary>
        ///Returns the result of the progress operation at given index.
        /// </summary>
        /// <param name="index">the index of the progress operation.</param>
        /// <returns>the result of the progress operation at given index</returns>
        public object GetProgressOperationResult(int index)
        {
            return (workers[index] as ProgressWorker).ProgressOperationResult;
        }

        protected int TotalProgressTicks
        {
            get
            {
                return totalProgressTicks;
            }
        }

        protected override void DoWork()
        {
            try
            {
                //loop over all of the progress operations and execute them
                //since each operation reports its own progress,
                foreach (ProgressWorker worker in workers)
                {
                    //update the worker's total progress tick so that progress will
                    //flow smoothly
                    worker.TotalProgressTicks = TotalProgressTicks;

                    // set the category if there is one
                    if (worker.Category != null)
                        SetCurrentCategory(worker.Category);

                    // do the work
                    worker.DoWork();
                }
            }
            catch (OperationCancelledException)
            {
                if (!this.CancelRequested)
                {
                    this.Cancel();
                }
                this.AcknowledgeCancel();
            }
        }

        #region IProgressCategoryProvider implementation

        /// <summary>
        /// Should the user-interface show categories?
        /// </summary>
        public bool ShowCategories
        {
            get
            {
                return showCategories;
            }
            set
            {
                showCategories = value;
            }
        }
        private bool showCategories = false;

        /// <summary>
        /// The name of the current category (optional)
        /// </summary>
        public ProgressCategory CurrentCategory
        {
            get
            {
                return progressCategory;
            }
        }
        private ProgressCategory progressCategory;

        /// <summary>
        /// The category being worked on by the operation changed
        /// </summary>
        public event EventHandler ProgressCategoryChanged;

        /// <summary>
        /// Helper to set the current progress category
        /// </summary>
        /// <param name="category"></param>
        private void SetCurrentCategory(ProgressCategory category)
        {
            if (progressCategory != category)
            {
                progressCategory = category;

                if (ProgressCategoryChanged != null)
                    ProgressCategoryChanged(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
