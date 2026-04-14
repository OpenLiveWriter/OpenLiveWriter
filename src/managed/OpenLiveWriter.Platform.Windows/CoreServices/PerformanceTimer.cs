// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Simple performance timer with high-resolution as compared with using DateTime.
    /// </summary>
    public class PerformanceTimer
    {
        /// <summary>
        /// The performance counter frequency.
        /// </summary>
        private static double performanceCounterFrequency;

        /// <summary>
        /// The PerformanceTimer name.
        /// </summary>
        private string name;

        /// <summary>
        /// The performance counter as set by the last call to the Reset method.
        /// </summary>
        private long startPerformanceCounter;

        /// <summary>
        /// The performance counter as set by the last call to the Reset or Split methods.
        /// </summary>
        private long splitPerformanceCounter;

        /// <summary>
        /// Static initialization of the PerformanceCounter class.
        /// </summary>
        static PerformanceTimer()
        {
            //	Obtain the native performance counter frequency (the number of "counts per second"
            //	of the high-resolution performance counter).
            long nativePerformanceCounterFrequency = 0;
            Kernel32.QueryPerformanceFrequency(ref nativePerformanceCounterFrequency);

            //	Convert the native performance counter frequence from "counts per second" to
            //	"counts per millisecond".  This avoids an extra floating point operation each time
            //	 a measurement is made when actually using the PerformanceTimer class.
            performanceCounterFrequency = 1000.0 / nativePerformanceCounterFrequency;
        }

        /// <summary>
        /// Initializes a new instance of the PerformanceTimer class.
        /// </summary>
        public PerformanceTimer()
        {
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the PerformanceTimer class.
        /// </summary>
        public PerformanceTimer(string name)
        {
            this.name = name;
            Reset();
        }

        /// <summary>
        /// Resets the PerformanceTimer, allowing for a new performance measurement to be made
        /// using it.
        /// </summary>
        public void Reset()
        {
            Kernel32.QueryPerformanceCounter(ref startPerformanceCounter);
            splitPerformanceCounter = startPerformanceCounter;
        }

        /// <summary>
        /// Returns the elapsed time, in milliseconds, since the last PerformanceTimer reset.
        /// </summary>
        /// <returns>Elapsed time since the </returns>
        public double ElapsedTime()
        {
            //	Obtain the end performance counter for this measurement.
            long endPerformanceCounter = 0;
            Kernel32.QueryPerformanceCounter(ref endPerformanceCounter);

            //	Return the elapsed time.
            return ((endPerformanceCounter - startPerformanceCounter) * performanceCounterFrequency);
        }

        /// <summary>
        /// Returns the elapsed time, in milliseconds, since the last PerformanceTimer reset or
        /// last call to SplitTime.
        /// </summary>
        /// <returns>Elapsed time since the </returns>
        public double SplitTime()
        {
            //	Obtain the end performance counter for this measurement.
            long endPerformanceCounter = 0;
            Kernel32.QueryPerformanceCounter(ref endPerformanceCounter);

            //	Calculate the split time.
            double splitTime = ((endPerformanceCounter - splitPerformanceCounter) * performanceCounterFrequency);

            //	Set the new split performance counter.
            splitPerformanceCounter = endPerformanceCounter;

            //	Return the split time.
            return splitTime;
        }

        /// <summary>
        /// Logs a split time (DEBUG).
        /// </summary>
        /// <param name="description">The description of the split time.</param>
        [Conditional("DEBUG")]
        public void DebugLogSplitTime(string description)
        {
            Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "*** Performance {0} {1:###,###,##0.00}ms {2}", name, SplitTime(), description));
        }

        /// <summary>
        /// Logs elapsed time (DEBUG).
        /// </summary>
        [Conditional("DEBUG")]
        public void DebugLogElapsedTime()
        {
            Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "*** Performance {0} {1:###,###,##0.00}ms Total", name, ElapsedTime()));
        }
    }
}
