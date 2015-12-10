// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// A progress host that simply writes the progress to the debug output
    /// </summary>
    public class DebugWritingProgressHost : IProgressHost
    {
        public DebugWritingProgressHost()
        {
        }

        public DebugWritingProgressHost(string progressDisplayPrefix)
        {
            this.progressDisplayPrefix = progressDisplayPrefix;
        }
        private string progressDisplayPrefix = string.Empty;
        private double committedProgessPercentage;

        #region IProgressHost Members

        public void UpdateProgress(int complete, int total, string message)
        {
            Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "Debug Writing Progress {0} {1} : {2} / {3}", progressDisplayPrefix, message, complete, total));
            committedProgessPercentage = total == 0 ? 0 : ((double)complete) / total;
        }

        public void UpdateProgress(int complete, int total)
        {
            UpdateProgress(complete, total, "No Message");
        }

        public void UpdateProgress(string message)
        {
            UpdateProgress(0, 0, message);
        }

        public bool CancelRequested
        {
            get
            {
                return false;
            }
        }

        public double ProgressCompletionPercentage
        {
            get
            {
                return committedProgessPercentage;
            }
        }

        #endregion

    }
}
