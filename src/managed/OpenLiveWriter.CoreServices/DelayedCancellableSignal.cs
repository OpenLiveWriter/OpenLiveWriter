// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for DelayedCancellableSignal.
    /// </summary>
    public class DelayedCancellableSignal
    {
        private object lockObj = new object();
        private long signalTime = long.MaxValue;
        private double delay;

        public DelayedCancellableSignal(double millisecondsDelayed)
        {
            this.delay = millisecondsDelayed;
        }

        public void WaitForQuit()
        {
            lock (lockObj)
            {
                long nowTicks;
                while (signalTime - (nowTicks = DateTime.Now.Ticks) > 0)
                {
                    Trace.Assert(signalTime - nowTicks > 0, "Surprising...");
                    // time to wait
                    long millisToWait = (this.signalTime - nowTicks) / 10000L;
                    millisToWait = Math.Min(millisToWait, (long)Int32.MaxValue);
                    Trace.Assert(millisToWait >= 0, "It couldn't possibly!");

                    Monitor.Wait(lockObj, checked((int)millisToWait));
                }
            }
        }

        /// <summary>
        /// Returns how much time remains before the next signal.
        /// If this value is less than or equal to 0, the signal
        /// time has passed.
        /// </summary>
        public long TimeToWait
        {
            get
            {
                lock (lockObj)
                {
                    return signalTime - DateTime.Now.Ticks;
                }
            }
        }

        protected void DoSignal(double delay)
        {
            lock (lockObj)
            {
                this.signalTime = DateTime.Now.AddMilliseconds(delay).Ticks;
                Monitor.PulseAll(lockObj);
            }
        }

        public void SignalLater()
        {
            Trace.WriteLine("Process Management: Will shut down in " + (new TimeSpan((long)(delay * 10000)).ToString()) + " unless new clients arrive");
            this.DoSignal(this.delay);
        }

        public void SignalNow()
        {
            Trace.WriteLine("Shutting down immediately");
            this.DoSignal(0.0);
        }

        public void CancelAllSignals()
        {
            lock (lockObj)
            {

                Trace.WriteLine("New client arrived");
                this.signalTime = long.MaxValue;
            }
        }

        #region deprecated
        /*
        private DateTime lastCancellation = DateTime.MinValue;
        private ManualResetEvent theLock = new ManualResetEvent(false);
        private LinkedList timers = new LinkedList();

        public DelayedCancellableSignal()
        {
        }

        public void Wait()
        {
            theLock.WaitOne();
        }

        public void SignalLater(int milliseconds)
        {
            Timer t = new Timer(new TimerCallback(MaybeSignal), DateTime.Now, milliseconds, -1);
            timers.Add(t);
        }

        public void SignalNow()
        {
            MaybeSignal(DateTime.Now);
        }

        private void MaybeSignal(object state)
        {
            Timer t = (Timer)state;
            DateTime dateTimeOriginated = (DateTime)state;
            if (dateTimeOriginated.CompareTo(lastCancellation) > 0)
                DoSignal();
        }

        private void DoSignal()
        {
            theLock.Set();
        }

        public void CancelAllSignals()
        {
            lastCancellation = DateTime.Now;
        }
        */
        #endregion
    }
}
