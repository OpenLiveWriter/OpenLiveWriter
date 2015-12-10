// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenLiveWriter.CoreServices.Threading
{
    /// <summary>
    /// Delegate for the logic that needs to be executed at a
    /// specified time.
    ///
    /// Note to implementers:
    /// if the job needs to be run repeatedly, you are responsible
    /// for scheduling the next run on the queue.  Be careful
    /// that exceptions during execution do not prevent the next
    /// run from being queued up (unless of course you want that
    /// behavior).
    /// </summary>
    public delegate void ProcessingQueueJob(ProcessingQueue queue);

    /// <summary>
    /// Schedule jobs for future execution.
    ///
    /// Each ProcessingQueue wraps a single Thread.  All jobs will
    /// execute on this thread.
    /// </summary>
    public class ProcessingQueue
    {
        private readonly SimpleHeap jobHeap = new SimpleHeap();
        private readonly Thread thread;

        public ProcessingQueue(string debugName)
        {
            thread = ThreadHelper.NewThread(new ThreadStart(ThreadStart), "ProcessingQueue_" + debugName, true, false, true);
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        public void ScheduleJob(ProcessingQueueJob job, int desiredExecutionDelay)
        {
            ScheduleJob(job, DateTime.Now.AddMilliseconds(desiredExecutionDelay));
        }

        public void ScheduleJob(ProcessingQueueJob job, DateTime desiredExecutionTime)
        {
            lock (jobHeap)
            {
                jobHeap.Push(new PQEntry(job, desiredExecutionTime));
                Monitor.PulseAll(jobHeap);
            }
        }

        private void ThreadStart()
        {
            TimeSpan maxWaitTime = TimeSpan.FromSeconds(30.0);

            try
            {
                while (true)
                {
                    ProcessingQueueJob job;
                    lock (jobHeap)
                    {
                        // this loop breaks only when a job is ready to execute
                        while (true)
                        {
                            if (jobHeap.Count == 0)
                            {
                                // job heap is empty.  it will be pulsed when something is added
                                Monitor.Wait(jobHeap);
                            }
                            else
                            {
                                // job heap has one or more jobs.  see if the latest is ready
                                DateTime nextTime = ((PQEntry)jobHeap.Peek()).ScheduledTime;
                                DateTime now = DateTime.Now;
                                if (now >= nextTime)
                                {
                                    // it's ready, let's go!
                                    job = ((PQEntry)jobHeap.Pop()).Job;
                                    break;
                                }
                                // job is not ready.  wait until it's ready, or until another
                                // job is added, whichever happens first.
                                TimeSpan delta = nextTime.Subtract(now);
                                if (delta > TimeSpan.Zero)
                                {
                                    // If someone messes with the clock, we could wait forever.
                                    // So make sure we don't wait a too-unreasonable amount of time.
                                    // However, as a mitigating factor, we always wake up when
                                    // the next processing event is added.
                                    if (delta > maxWaitTime)
                                        delta = maxWaitTime;

                                    Monitor.Wait(jobHeap, delta);
                                }
                            }
                        }
                    }

                    try
                    {
                        job(this);
                    }
                    catch (Exception e)
                    {
                        Trace.Fail("Exception in processing queue job: " + e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Trace.Fail("Unexpected exception in processing queue outer loop: " + e.ToString());
            }
        }
    }

    internal class PQEntry : IComparable
    {
        internal readonly ProcessingQueueJob Job;
        internal readonly DateTime ScheduledTime;

        public PQEntry(ProcessingQueueJob job, DateTime scheduledTime)
        {
            Job = job;
            ScheduledTime = scheduledTime;
        }

        public int CompareTo(object obj)
        {
            return ScheduledTime.CompareTo(((PQEntry)obj).ScheduledTime);
        }
    }
}
