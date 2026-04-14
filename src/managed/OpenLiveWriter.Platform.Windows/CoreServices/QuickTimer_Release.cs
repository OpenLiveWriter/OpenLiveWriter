// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A version of QuickTimer that works in both debug and release mode.
    ///
    /// ----
    ///
    /// Easy way to slap a timer around a block of code.  Example:
    ///
    /// using (new QuickTimer("Long running operation"))
    /// {
    ///	// do long running operation
    /// }
    ///
    /// When the using block is exited, timing info will be written
    /// to the Debug output.
    /// </summary>
    public struct QuickTimer_Release : IDisposable
    {
        private readonly string label;
        private readonly PerformanceTimer timer;

        public QuickTimer_Release(string label)
        {
            this.label = label;
            this.timer = new PerformanceTimer();
        }

        public void Dispose()
        {
            double time = timer.ElapsedTime();
            Trace.WriteLine("QuickTimer: [" + label + "] " + time + "ms");
        }
    }
}
