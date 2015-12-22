// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A helper object that assists with processing tasks using the .NET ThreadPool without
    /// overloading its 25 thread limit.
    /// </summary>
    public class BackgroundWorkerQueue
    {
        /// <summary>
        /// Creates a new BackgroundWorkerQueue that be restricted in the number of background threads
        /// it will be allowed to consume.
        /// </summary>
        /// <param name="maxConcurrency">the maximum number of threads from the .NET ThreadPool this queue
        /// will use concurrently when processing workers.</param>
        public BackgroundWorkerQueue(int maxConcurrency)
        {
            MaxConcurrency = maxConcurrency;
        }

        /// <summary>
        /// Adds a worker method into the processing queue.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void AddWorker(WaitCallback callback, object state)
        {
            lock (this)
            {
                BackgroundWorker worker = new BackgroundWorker(callback, state);
                queue.Enqueue(worker);
                while (activeWorkerCount < Math.Min(queue.Count, MaxConcurrency))
                    LaunchBackgroundWorker();
            }
        }

        /// <summary>
        /// Returns true if this queue currently has work scheduled and/or executing.
        /// </summary>
        /// <returns></returns>
        public bool HasPendingWork()
        {
            return activeWorkerCount > 0 || queue.Count > 0;
        }

        /// <summary>
        /// Consume a new background worker thread from the .NET thread pool.
        /// </summary>
        private void LaunchBackgroundWorker()
        {
            lock (this)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueue));
                    activeWorkerCount++;
                }
                catch (Exception e)
                {
                    Trace.Assert(activeWorkerCount > 0, "could not launch any background workers: " + e.Message);
                    if (activeWorkerCount == 0)
                    {
                        //no worker background threads were available to process the worker, so launch a
                        //standard thread to process the queue.
                        Thread th = new Thread(new ThreadStart(ProcessQueue));
                        th.IsBackground = true;
                        th.Start();
                    }
                    //else, there's at least one background thread processing the queue, so we'll eventually
                    //get through the work in the queue.
                }
            }
        }

        /// <summary>
        /// A WaitCallback-compatible method for processing the items in the queue.
        /// </summary>
        /// <param name="param"></param>
        private void ProcessQueue(object param)
        {
            ProcessQueue();
        }

        /// <summary>
        /// Loops until all of the workers in the queue have been executed.
        /// </summary>
        private void ProcessQueue()
        {
            //Thread.CurrentThread.Name = QueueName;
            BackgroundWorker worker = DequeueWorker();
            while (worker != null)
            {
                try
                {
                    worker.Callback(worker.State);
                }
                catch (Exception e)
                {
                    Trace.Fail("Background worker threw an exception: " + e.Message, e.StackTrace);
                }
                worker = DequeueWorker();
            }
        }

        /// <summary>
        /// Dequeues the next BackgroundWorker in the queue.
        /// </summary>
        /// <returns></returns>
        private BackgroundWorker DequeueWorker()
        {
            BackgroundWorker worker = null;
            lock (queue)
            {
                if (queue.Count > 0)
                    worker = queue.Dequeue() as BackgroundWorker;
                if (worker == null)
                    activeWorkerCount--;
            }
            return worker;
        }

        Queue queue = new Queue();
        int activeWorkerCount;
        int MaxConcurrency;

        /// <summary>
        /// Utility class for associating a WaitCallback delegate with its assigned state.
        /// </summary>
        internal class BackgroundWorker
        {
            internal BackgroundWorker(WaitCallback callback, Object state)
            {
                Callback = callback;
                State = state;
            }
            internal WaitCallback Callback;
            internal Object State;
        }
    }
}
