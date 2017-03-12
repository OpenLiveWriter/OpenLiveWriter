// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Represent a nestable block of progress in a progress bar.
    /// </summary>
    public class ProgressTick : IProgressHost, IDisposable
    {
        private IProgressHost ParentProgress;
        private double StartPercentage; //the base %complete that this tick adds progress to.
        private int TickAllocation; //the number of ticks allocated to this object
        private int TickTotal; //the total number of ticks that the parent progress contains.
        private double lastCompletionPercentage; //the %complete of this tick.

        /// <summary>
        ///
        /// </summary>
        /// <param name="progress">the parent progress host that this tick is nested within</param>
        /// <param name="tickAllocation">the number of ticks allocated for use by this object</param>
        /// <param name="tickTotal">the total number of ticks in the overall parent progress operation</param>
        public ProgressTick(IProgressHost progress, int tickAllocation, int tickTotal)
        {
            ParentProgress = progress;

            //remember the initial completion percentage of the parent so that this object
            //will always add that percentage when updating progress.
            StartPercentage = ParentProgress.ProgressCompletionPercentage;

            TickAllocation = tickAllocation;
            TickTotal = tickTotal;
        }

        public int TotalTicks
        {
            get
            {
                return TickTotal;
            }
            set
            {
                TickTotal = value;
            }
        }

        public int AllocatedTicks
        {
            get
            {
                return TickAllocation;
            }
            set
            {
                TickAllocation = value;
            }
        }
        #region IProgressHost Members

        public void UpdateProgress(int complete, int total, string message)
        {
            ConvertTicksToParent(ref complete, ref total);
            // If they send back a bogus progress value, just ignore the values
            if (complete < 0)
                ParentProgress.UpdateProgress(message);
            else
                ParentProgress.UpdateProgress(complete, total, message);
        }

        public void UpdateProgress(int complete, int total)
        {
            int relativeComplete = complete;
            int relativeTotal = total;
            ConvertTicksToParent(ref relativeComplete, ref relativeTotal);
            ParentProgress.UpdateProgress(relativeComplete, relativeTotal);
        }

        public void UpdateProgress(string message)
        {
            ParentProgress.UpdateProgress(message);
        }

        /// <summary>
        /// Convert the relative ticks for this object to the number of ticks in the parent.
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="total"></param>
        private void ConvertTicksToParent(ref int complete, ref int total)
        {
            double fractionalComplete = 0;
            if (complete < 0)
                return;

            if (complete != 0)
                fractionalComplete = ((double)complete) / total;

            double allocationTotal = ((double)TickAllocation) / TickTotal;
            if (allocationTotal + StartPercentage > 1.0)
            {
                // Debug.Fail("Rounding error detected.");
                allocationTotal = 1.0 - StartPercentage;
            }
            double relativeCompletionFraction = ((fractionalComplete * allocationTotal) + StartPercentage);

            //update the ticks to make them relative to the parent.
            double doubleComplete = relativeCompletionFraction * TICK_BASE_COUNT;
            complete = (int)doubleComplete;
            total = TICK_BASE_COUNT; //reset the total to be 100000000 based

            Debug.Assert(complete <= total, String.Format(CultureInfo.InvariantCulture, "Progress calculation error occurred: {0} > {1}", complete.ToString(CultureInfo.InvariantCulture), total.ToString(CultureInfo.InvariantCulture)));

            //save the %complete of this tick object
            lastCompletionPercentage = fractionalComplete;
        }
        private const int TICK_BASE_COUNT = 100000000;

        public bool CancelRequested
        {
            get
            {
                return ParentProgress.CancelRequested;
            }
        }

        public double ProgressCompletionPercentage
        {
            get
            {
                return lastCompletionPercentage;
                //return 0;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            UpdateProgress(1, 1);
        }

        #endregion
    }
}
