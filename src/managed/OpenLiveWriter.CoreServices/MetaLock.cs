// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A facility that lets you conjure up short-lived, non-reentrant
    /// lock objects on demand.  Locks are identified by a hashtable
    /// key (so make sure that any lockKey objects you pass in have the
    /// appropriate .Equals() and .GetHashCode() semantics).
    ///
    /// For example, you could do the following:
    ///
    /// public void SomeMethod(int n)
    /// {
    ///		using (MetaLock.Lock("joe's lock " + n))
    ///		{
    ///			// protected code
    ///		}
    ///	}
    ///
    ///	This will make sure that method invocations with the same param
    ///	"n" will be blocked from executing concurrently.
    ///
    ///	Note that these locks are NOT REENTRANT.  That means the following
    ///	code will hang forever, EVERY TIME.
    ///
    ///	using (MetaLock.Lock("abc123"))
    ///	{
    ///		using (MetaLock.Lock("abc123"))
    ///		{
    ///			// this point will never be reached
    ///		}
    ///	}
    ///
    ///	We could add reentrancy if it's necessary, but if not, we get higher
    ///	performance by staying away from thread-static lock counts.
    ///
    ///	Note that this class gets more expensive as the number of threads
    ///	waiting in Enter() increases (even if they are not waiting for
    ///	the same lock).
    /// </summary>
    public class MetaLock
    {
        private static readonly TimeSpan INITIAL_TIMEOUT = TimeSpan.FromSeconds(1);
        private object metaLock = new object();
        private ISet locks = new HashSet();

        public MetaLock()
        {
        }

        public IDisposable Lock(object lockKey)
        {
            return new LockImpl(this, lockKey);
        }

        internal void Enter(object obj)
        {
            lock (metaLock)
            {
                DateTime start = DateTime.Now;
                TimeSpan timeout = INITIAL_TIMEOUT;

                while (locks.Contains(obj))
                {
                    bool signalled = Monitor.Wait(metaLock, timeout);
                    if (!signalled)
                    {
                        // The wait timed out.  Log about it.
                        Trace.WriteLine("Meta lock " + obj.ToString() + " not acquired for " + DateTime.Now.Subtract(start).ToString());
                        // Wait longer next time
                        if (timeout.TotalSeconds < 30)
                            timeout = timeout.Add(timeout);
                    }
                }

                locks.Add(obj);
            }
        }

        internal void Exit(object obj)
        {
            lock (metaLock)
            {
                locks.Remove(obj);
                Monitor.PulseAll(metaLock);
            }
        }

        class LockImpl : IDisposable
        {
            private readonly MetaLock metaLock;
            private readonly object lockId;
#if DEBUG
            private bool disposed = false;
#endif

            public LockImpl(MetaLock metaLock, object lockId)
            {
                this.metaLock = metaLock;
                this.lockId = lockId;
                metaLock.Enter(lockId);
            }

            public void Dispose()
            {
#if DEBUG
                Debug.Assert(!disposed, "LockImpl disposed twice!!!");
                disposed = true;
#endif
                metaLock.Exit(lockId);
            }

        }

    }
}
