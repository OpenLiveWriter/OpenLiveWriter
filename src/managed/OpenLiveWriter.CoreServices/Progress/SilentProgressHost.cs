// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// A no-op IProgressHost singleton that can be used to execute progress-compatible methods
    /// without a real progress host in place.
    /// </summary>
    public class SilentProgressHost : IProgressHost
    {
        private SilentProgressHost()
        {
        }

        /// <summary>
        /// Returns the singleton instance.
        /// </summary>
        public static IProgressHost Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SilentProgressHost();
                return _instance;
            }
        }
        private static SilentProgressHost _instance;

        #region IProgressHost Members

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
                return false;
            }
        }

        public double ProgressCompletionPercentage
        {
            get
            {
                return 0;
            }
        }

        #endregion
    }
}
