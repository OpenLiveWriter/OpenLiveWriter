// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for TimerHelper.
    /// </summary>
    public class TimerHelper
    {
        InvokeInUIThreadDelegate _uiInvokeDelegate;
        private TimerHelper(InvokeInUIThreadDelegate uiInvokeDelegate)
        {
            _uiInvokeDelegate = uiInvokeDelegate;
        }

        private Timer Invoke(int delay)
        {
            //set a timer to send focus to the current editor on the next UI loop
            Timer t = new Timer();
            t.Interval = delay;
            t.Tick += new EventHandler(t_Tick);
            t.Start();
            return t;
        }

        private void t_Tick(object sender, EventArgs e)
        {
            Timer t = (Timer)sender;
            t.Tick -= new EventHandler(t_Tick);
            _uiInvokeDelegate();
            t.Stop();
            t.Dispose();
        }

        /// <summary>
        /// Invoke a callback on a delayed timer.
        /// </summary>
        /// <param name="uiInvokeDelegate"></param>
        /// <param name="delayMillis"></param>
        public static Timer CallbackOnDelay(InvokeInUIThreadDelegate uiInvokeDelegate, int delayMillis)
        {
            return (new TimerHelper(uiInvokeDelegate)).Invoke(delayMillis);
        }
    }

    /// <summary>
    /// This class will run multiple "tasks" on the UI thread with a wait time between each one
    /// that will give time for some messages to be processed.  WARNING: using this with a foreach loop or
    /// a for loop that does not explicitly create a new reference to the variables used in the "task" can
    /// result in a unexpected behaviors.
    /// </summary>
    public class MultiTaskHelper : IDisposable
    {
        private Timer t;
        private bool isStarted = false;
        private Queue<InvokeInUIThreadDelegate> workItems = new Queue<InvokeInUIThreadDelegate>();

        public MultiTaskHelper(int callbackTime)
        {
            t = new Timer();
            t.Interval = callbackTime;
            t.Tick += t_Tick;
        }

        void t_Tick(object sender, EventArgs e)
        {
            Start();
        }

        public void AddWork(InvokeInUIThreadDelegate work)
        {
            if (isStarted)
            {
                throw new InvalidOperationException("MultiTaskHelper has already been started");
            }
            workItems.Enqueue(work);
        }

        public void Start()
        {
            isStarted = true;
            t.Stop();
            if (workItems.Count == 0)
            {
                Dispose();
                return;
            }

            InvokeInUIThreadDelegate item = workItems.Dequeue();

            if (item == null)
                return;

            item();
            t.Start();
        }

        public void Dispose()
        {
            if (t != null)
            {
                t.Dispose();
                t = null;
            }
            workItems.Clear();
        }
    }
}
