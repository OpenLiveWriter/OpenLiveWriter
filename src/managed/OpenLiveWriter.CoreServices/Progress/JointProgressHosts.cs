// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Produces an array of linked ProgressTick-like hosts that work
    /// together to fill an allocation of ticks.  This is necessary
    /// (as opposed to creating individual ProgressTick objects)
    /// when an operation has multiple parts that proceed in parallel.
    ///
    /// This design assumes that all the parts of the operation
    /// represent an equal slice of the total allocation.
    /// </summary>
    public class JointProgressHosts
    {
        private IProgressHost pHost;
        private readonly LinkedProgressTick[] ticks;
        private readonly int tickAllocation;
        private readonly int tickTotal;
        private readonly double startPercentage;

        public JointProgressHosts(IProgressHost parentProgressHost, int slices, int tickAllocation, int tickTotal)
        {
            this.pHost = parentProgressHost;
            this.tickAllocation = tickAllocation;
            this.tickTotal = tickTotal;
            this.startPercentage = parentProgressHost.ProgressCompletionPercentage;

            this.ticks = new LinkedProgressTick[slices];
            for (int i = 0; i < ticks.Length; i++)
                ticks[i] = new LinkedProgressTick(this);
        }

        /// <summary>
        /// Returns the progress hosts that work in parallel.
        /// </summary>
        public IProgressHost[] ProgressHosts
        {
            get { return ticks; }
        }

        private void ProgressUpdated()
        {
            // This logic more or less ported from ProgressTick.ConvertTicksToParent().
            // TODO: Factor out the common code.

            double totalComplete = 0f;

            for (int i = 0; i < ticks.Length; i++)
            {
                LinkedProgressTick tick = ticks[i];
                if (tick.total != 0)
                    totalComplete += Math.Max(0f, ((double)tick.complete) / ((double)tick.total));
            }

            // percent complete over all contained parts
            double totalCompletePercentage = totalComplete / (double)ticks.Length;
            // normalize percentage to fit our allocation vs. parent total
            double scaledTotal = totalCompletePercentage * (((double)tickAllocation) / ((double)tickTotal));
            // offset percentage to reflect work already done before we started
            double offsetAndScaledTotal = scaledTotal + startPercentage;

            // convert to ints, but keep a reasonable amount of precision
            const int total = 100000000;
            int complete = (int)(offsetAndScaledTotal * total);

            // update parent
            pHost.UpdateProgress(complete, total);
        }

        private class LinkedProgressTick : IProgressHost
        {
            private JointProgressHosts parent;
            internal int complete = 0;
            internal int total = 1;

            public LinkedProgressTick(JointProgressHosts parent)
            {
                this.parent = parent;
            }

            public void UpdateProgress(int complete, int total, string message)
            {
                UpdateProgress(complete, total);
                UpdateProgress(message);
            }

            public void UpdateProgress(int complete, int total)
            {
                Debug.Assert(complete <= total, "Progress completed has exceeded total!");
                // Don't let completed progress exceed the total
                this.complete = Math.Min(complete, total);
                this.total = total;
                parent.ProgressUpdated();
            }

            public void UpdateProgress(string message)
            {
                parent.pHost.UpdateProgress(message);
            }

            public bool CancelRequested
            {
                get { return parent.pHost.CancelRequested; }
            }

            public double ProgressCompletionPercentage
            {
                get { return (double)complete / (double)total; }
            }

        }
    }
}
