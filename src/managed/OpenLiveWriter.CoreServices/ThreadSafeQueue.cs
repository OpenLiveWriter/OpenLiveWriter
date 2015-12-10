// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Threadsafe queue
    /// </summary>
    public class ThreadSafeQueue
    {
        private LinkedList list = new LinkedList();
        private bool terminated = false;

        protected virtual bool ItemsAvailable()
        {
            return list.Count != 0;
        }

        /// <summary>
        /// Stop this ThreadSafeQueue from any further enqueues.
        /// Termination is a one-way operation; there is no way
        /// to un-terminate a terminated queue.
        ///
        /// Termination is a way to indicate to consumer threads
        /// that no more items are forthcoming.
        ///
        /// Note that termination is strictly optional; if you
        /// have some other mechanism for signaling job completion
        /// to your threads (such as Thread.Interrupt()) and
        /// your threads don't use the blocking Dequeue() method,
        /// there's no need to ever terminate.
        /// </summary>
        /// <param name="clearQueueBeforeTerminate">
        /// Clear queue before terminating.  If false, any items
        /// already in the queue will continue to be dequeued
        /// normally.
        /// </param>
        public void Terminate(bool clearQueueBeforeTerminate)
        {
            lock (this)
            {
                if (clearQueueBeforeTerminate)
                    list.Clear();
                terminated = true;
                Pulse();
            }
        }

        public bool Terminated
        {
            get
            {
                lock (this)
                {
                    return terminated;
                }
            }
        }

        /// <summary>
        /// Add an object to the queue
        /// </summary>
        /// <param name="o">The object to add</param>
        /// <exception cref="ThreadSafeQueueTerminatedException">
        /// The queue has been terminated.
        /// </exception>
        public virtual void Enqueue(params object[] objects)
        {
            lock (this)
            {
                if (terminated)
                    throw new ThreadSafeQueueTerminatedException();

                foreach (object o in objects)
                    list.Add(o);
                Pulse();
            }
        }

        /// <summary>
        /// Get an object from the queue if one is available.
        /// Returns immediately either way (unless the queue is under
        /// extremely high contention).
        /// </summary>
        /// <param name="success">
        /// True if item was available.  This makes it possible to
        /// distinguish between null being returned because it was
        /// the next value in the queue, or no values being in the
        /// queue.
        /// </param>
        /// <returns>The object available, or null if none.</returns>
        /// <exception cref="ThreadSafeQueueTerminatedException">
        /// The queue has been terminated.
        /// </exception>
        public virtual object TryDequeue(out bool success)
        {
            lock (this)
            {
                if (!ItemsAvailable())
                {
                    if (terminated)
                        throw new ThreadSafeQueueTerminatedException();
                    else
                    {
                        success = false;
                        return null;
                    }
                }
                else
                {
                    success = true;
                    return DequeueInternal();
                }
            }
        }

        /// <summary>
        /// Gets an object from the queue, blocking until one is available,
        /// or throws ThreadSafeQueueTerminatedException if the thread
        /// is terminated before an item becomes available.
        /// </summary>
        /// <returns>The object</returns>
        /// <exception cref="ThreadSafeQueueTerminatedException">
        /// The queue has been terminated. Note that this can occur
        /// even if the queue was not terminated at the time the
        /// method was called.
        /// </exception>
        public virtual object Dequeue()
        {
            lock (this)
            {
                while (!ItemsAvailable() && !terminated)
                {
                    Monitor.Wait(this);
                }

                if (ItemsAvailable())
                    return DequeueInternal();
                else
                {
                    Debug.Assert(terminated);
                    throw new ThreadSafeQueueTerminatedException();
                }
            }
        }

        private object DequeueInternal()
        {
            object o = list[0];
            list.RemoveAt(0);
            return o;
        }

        /// <summary>
        /// Gets all queued objects from the queue, blocking until at least one is available,
        /// or throws ThreadSafeQueueTerminatedException if the queue is terminated.
        /// </summary>
        /// <returns>All of the objects in the queue</returns>
        /// <exception cref="ThreadSafeQueueTerminatedException">
        /// The queue has been terminated. Note that this can occur
        /// even if the queue was not terminated at the time the
        /// method was called.
        /// </exception>
        public virtual LinkedList DequeueAll()
        {
            lock (this)
            {
                while (!ItemsAvailable() && !terminated)
                {
                    Monitor.Wait(this);
                }

                if (!ItemsAvailable())
                {
                    Debug.Assert(terminated);
                    throw new ThreadSafeQueueTerminatedException();
                }

                LinkedList returnList = list;
                list = new LinkedList();
                return returnList;
            }
        }

        /// <summary>
        /// Clears all work items from the queue.
        /// </summary>
        /// <exception cref="ThreadSafeQueueTerminatedException">
        /// The queue has been terminated.
        /// </exception>
        public virtual void Clear()
        {
            lock (this)
            {
                if (terminated)
                    throw new ThreadSafeQueueTerminatedException();

                list.Clear();
            }
        }

        /// <summary>
        /// Returns the number of objects currently in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Triggers a pulse operation that will allow threads waiting to dequeue objects.
        /// </summary>
        protected virtual void Pulse()
        {
            Monitor.PulseAll(this);
        }
    }

    /// <summary>
    /// Indicates that the ThreadSafeQueue has been Terminated() and
    /// can no longer be used.
    /// </summary>
    [Serializable]
    public class ThreadSafeQueueTerminatedException : Exception
    {
        public ThreadSafeQueueTerminatedException() : base("ThreadSafeQueue has been terminated")
        {
        }
    }
}
