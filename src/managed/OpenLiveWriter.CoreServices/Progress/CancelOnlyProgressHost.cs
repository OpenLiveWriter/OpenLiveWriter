// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Progress host that only forwards cancel requests.
    /// </summary>
    public class CancelOnlyProgressHost : IProgressHost
    {
        private readonly IProgressHost parent;

        public CancelOnlyProgressHost(IProgressHost parent)
        {
            this.parent = parent;
        }

        public void UpdateProgress(int complete, int total, string message)
        {
        }

        void IProgressHost.UpdateProgress(int complete, int total)
        {
        }

        void IProgressHost.UpdateProgress(string message)
        {
        }

        public bool CancelRequested
        {
            get
            {
                return parent.CancelRequested;
            }
        }

        public double ProgressCompletionPercentage
        {
            get
            {
                return 0;
            }
        }

    }
}
