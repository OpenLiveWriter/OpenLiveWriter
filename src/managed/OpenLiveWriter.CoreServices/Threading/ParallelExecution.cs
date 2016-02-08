// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenLiveWriter.CoreServices.Threading
{
    public delegate void WorkerExceptionHandler(Exception e);

    /// <summary>
    /// Execute the same task multiple times in parallel.
    /// If one thread exits with an exception, all the other
    /// threads will be interrupted.  Use ThreadHelper from
    /// inside the action code to check for interrupted state.
    /// </summary>
    public class ParallelExecution
    {
        private readonly int threadCount;
        private readonly ThreadStart action;
        private readonly Thread[] threads;

        private Exception exception;

        public ParallelExecution(ThreadStart action, int threadCount)
        {
            if (threadCount < 1)
                throw new ArgumentOutOfRangeException("threadCount", "threadCount must be greater than or equal to 1");

            this.action = action;
            this.threadCount = threadCount;
            this.threads = new Thread[threadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = ThreadHelper.NewThread(new ThreadStart(ExceptionHandlingThreadStart), null, true, false, true);
            }
        }

        public void BeginExecute()
        {
            for (int i = 0; i < threads.Length; i++)
                threads[i].Start();
        }

        /// <summary>
        /// Blocks until all threads return, whether due to success,
        /// failure or cancellation.
        /// </summary>
        public void Execute()
        {
            BeginExecute();

            for (int i = 0; i < threads.Length; i++)
            {
                while (!threads[i].Join(100))
                {
                    CheckForException();
                }
            }

            CheckForException();
        }

        private void CheckForException()
        {
            Exception localException;
            lock (this)
                localException = exception;

            if (localException != null)
            {
                Trace.WriteLine("Exception in worker thread: " + localException.ToString());

                try
                {
                    Debug.WriteLine("Interrupting threads");
                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i].Interrupt();
                    }
                }
                catch
                {
                    Debug.Fail("Exception while interrupting threads");
                }

                throw localException;
            }
        }

        private void ExceptionHandlingThreadStart()
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                lock (this)
                {
                    if (exception == null)
                        exception = e;
                }
            }
        }
    }
}
