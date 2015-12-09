// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLiveWriter.CoreServices
{
    public class ConditionVariable
    {
        public void Signal()
        {
            lock (this)
            {
                _signaled = true;
                Monitor.PulseAll(this);
            }
        }

        public bool WaitForSignal(int timeoutMs)
        {
            long endTicks = DateTime.UtcNow.Ticks + timeoutMs;
            lock (this)
            {
                while (!_signaled)
                {
                    long waitTimeOutMs = endTicks - DateTime.UtcNow.Ticks;
                    if (waitTimeOutMs < 0)
                        return false; // timeout b/c we're not signaled

                    if (!Monitor.Wait(this, (int)waitTimeOutMs))
                        return false;
                }
                return true;
            }
        }

        private bool _signaled = false;
    }
}
