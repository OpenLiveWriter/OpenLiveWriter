// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    public delegate ProcessKeepalive ProcessKeepaliveFactoryMethod(bool taskHasNonBackgroundThread);

    /// <summary>
    /// The existence of a non-disposed ProcessKeepalive object will ensure
    /// that a process stays alive.  This is essential in two situations:
    ///
    ///   1) When Internet Explorer hosts the CLR, it does not wait for
    ///      .NET threads to exit before closing the process.  So even
    ///      non-background threads are not safe from abrupt death unless
    ///      they use a ProcessKeepalive.
    ///   2) Even in a CLR process, sometimes we get into a situation where
    ///      all non-background threads have exited but we still need the
    ///      process to stay around, perhaps to listen for an event we're
    ///      expecting.  (e.g. FileMonitor)
    ///
    /// BE SURE that any ProcessKeepalive objects you obtain eventually get
    /// disposed, even in the face of exceptions in the code it's protecting.
    ///
    /// Example:
    ///
    /// ProcessKeepalive keepAlive = ProcessKeepalive.Open();
    /// // time passes...
    /// keepAlive.Dispose();
    ///
    /// or
    ///
    /// using (ProcessKeepalive keepAlive = ProcessKeepalive.Open())
    /// {
    ///   // protected code
    /// }
    ///
    /// or
    ///
    /// class MyForm : Form
    /// {
    ///   private ProcessKeepalive keepAlive;
    ///
    ///   public MyForm()
    ///   {
    ///     keepAlive = ProcessKeepalive.Open();
    ///     try
    ///     {
    ///       InitializeComponents();
    ///       // whatever other constructor stuff
    ///     }
    ///     catch { keepAlive.Dispose(); }
    ///   }
    ///   public override void Dispose(bool disposing)
    ///   {
    ///     if (disposing)
    ///     {
    ///       try { // whatever other dispose stuff }
    ///       finally { keepAlive.Dispose(); }
    ///     }
    ///   }
    /// }
    /// </summary>
    public abstract class ProcessKeepalive : IDisposable
    {
        private static ProcessKeepaliveFactoryMethod factory = new ProcessKeepaliveFactoryMethod(DefaultFactory);

        public static ProcessKeepalive Open()
        {
            return Open(!Thread.CurrentThread.IsBackground);
        }

        /// <summary>
        /// Factory method for getting an ExplorerKeepalive object, which should
        /// generally be called with the using() construct.  The using block will
        /// execute without the world shutting down.
        /// </summary>
        /// <param name="taskHasNonBackgroundThread">
        /// Callers should pass in true if they have a living non-background thread
        /// that mirrors the lifetime of the keepalive.  Living non-background
        /// threads can sometimes be enough to keep the process alive by themselves.
        /// In the case of IE, this is not true, but in the case of any CLR-managed
        /// process, it's true, so we can skip all the work involved in keepalive
        /// and simply return a NullKeepalive() which is just a placebo.
        ///
        /// (If in doubt, pass false; it's always safe to do so.)
        /// </param>
        public static ProcessKeepalive Open(bool taskHasNonBackgroundThread)
        {
            return factory(taskHasNonBackgroundThread);
        }

        public static void SetFactory(ProcessKeepaliveFactoryMethod factoryImpl)
        {
            factory = factoryImpl;
        }

        private static ProcessKeepalive DefaultFactory(bool taskHasNonBackgroundThread)
        {
            IntPtr ptr;
            if (ExplorerKeepalive.Acquire(out ptr))
            {
                return new ExplorerKeepalive(ptr);
            }
            else
            {
                if (taskHasNonBackgroundThread)
                    return new NullKeepalive();  // keepalive is not necessary.
                else
                    return new DotNetKeepalive();
            }
        }

        ~ProcessKeepalive()
        {
            Trace.Fail("ProcessKeepalive was not disposed!");
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }

    /// <summary>
    /// Does nothing.  In some situations, we know that a ProcessKeepalive
    /// is redundant, but the calling code still expects it.
    /// </summary>
    public class NullKeepalive : ProcessKeepalive
    {
        public NullKeepalive() { }
        protected override void Dispose(bool disposing) { }
    }

    /// <summary>
    /// The ProcessKeepalive implementation that is appropriate for
    /// CLR-managed processes.  It works by creating a non-background
    /// thread on demand, and keeping it around until all DotNetKeepalive
    /// instances have been disposed.
    /// </summary>
    public class DotNetKeepalive : ProcessKeepalive
    {
        private static int refcount;
        private static object classLock = new object();
        private static Thread keepaliveThread;

        private bool disposed = false;
#if DEBUG
        private StackTrace stackTrace;
#endif
        internal DotNetKeepalive()
        {
            lock (classLock)
            {
                refcount++;
                if (keepaliveThread == null)
                {
                    keepaliveThread =
                        ThreadHelper.NewThread(new ThreadStart(KeepaliveThreadStart), "ProcessKeepalive", false, false, false);
                    keepaliveThread.Priority = ThreadPriority.Lowest;
                    keepaliveThread.Start();
                }
            }
#if DEBUG
            stackTrace = new StackTrace();
#endif
        }

        protected override void Dispose(bool disposing)
        {
            lock (classLock)
            {
                if (!disposed)
                {
                    disposed = true;
                    refcount--;
                    Monitor.PulseAll(classLock);
                }
            }
        }

        private void KeepaliveThreadStart()
        {
            lock (classLock)
            {
                while (refcount > 0)
                    Monitor.Wait(classLock);

                // we'll only get here when there are
                // no more keepalives
                keepaliveThread = null;
            }
        }
    }

    /// <summary>
    /// An implementation of ProcessKeepalive that is appropriate for
    /// components hosted within a Internet Explorer or Windows Explorer
    /// process.  Uses the SHGetInstanceExplorer ref counting mechanism.
    /// </summary>
    public class ExplorerKeepalive : ProcessKeepalive
    {
        /// <summary>
        /// Manually increase the refcount on keeping the explorer process alive.
        /// Returns false if we're not in an explorer process.
        /// </summary>
        internal static bool Acquire(out IntPtr ptr)
        {
            if (definitelyNotExplorer)
            {
                ptr = IntPtr.Zero;
                return false;
            }

            int hresult = Shell32.SHGetInstanceExplorer(out ptr);
            bool success = (hresult == HRESULT.S_OK);
            definitelyNotExplorer = !success;
            Debug.WriteLineIf(success, "ExplorerKeepalive: Acquired pointer " + ptr.ToString());
            return success;
        }
        private static volatile bool definitelyNotExplorer = false;

        /// <summary>
        /// Manually reduce the refcount on keeping the explorer process alive.
        /// If resulting refcount is 0, no embedded components are keeping the
        /// process alive.  Results are undefined if Acquire return false (i.e.
        /// we're not in an explorer process).
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        internal static int Release(IntPtr ptr)
        {
            Debug.WriteLine("ExplorerKeepalive: Manually releasing pointer " + ptr.ToString());
            return Marshal.Release(ptr);
        }

        // Wrapper implementation.

        private IntPtr ptr;

        internal ExplorerKeepalive(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        protected override void Dispose(bool disposing)
        {
            Release();
        }

        public bool Release()
        {
            IntPtr local;
            lock (this)
            {
                local = ptr;
                ptr = IntPtr.Zero;
            }
            if (local != IntPtr.Zero)
            {
                Marshal.Release(local);
                GC.SuppressFinalize(this);
                Debug.WriteLine("ExplorerKeepalive: Released pointer on instance method " + local.ToString());
                return true;
            }
            else
                return false;
        }
    }

    public class ManualKeepalive : ProcessKeepalive
    {
        public ManualKeepalive()
        {
            Increment();
        }

        protected override void Dispose(bool disposing)
        {
            Decrement();
        }

        public static ProcessKeepalive Factory(bool hasNonBackgroundThread)
        {
            return new ManualKeepalive();
        }

        internal static void Increment()
        {
            lock (__formCountLock)
            {
                if (__shutdown)
                    throw new ManualKeepaliveOperationException("Shutting down");
                ++__formCount;
            }
        }

        internal static void Decrement()
        {
            lock (__formCountLock)
            {
                if (--__formCount == 0)
                    Monitor.PulseAll(__formCountLock);
            }
        }

        /// <summary>
        /// Wait until no threads are running.
        /// </summary>
        public static void Wait(bool doShutdown)
        {
            lock (__formCountLock)
            {
                while (__formCount > 0)
                    Monitor.Wait(__formCountLock);
                if (doShutdown)
                    __shutdown = true;
            }
        }

        private static int __formCount = 0;
        private static readonly object __formCountLock = new object();
        private static bool __shutdown = false;
    }

    [Serializable]
    public class ManualKeepaliveOperationException : Exception
    {
        public ManualKeepaliveOperationException(string msg) : base(msg)
        {
        }
    }
}
