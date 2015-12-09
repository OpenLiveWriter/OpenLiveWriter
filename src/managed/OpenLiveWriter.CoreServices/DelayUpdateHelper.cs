// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices
{
    public class DelayUpdateHelper
    {
        public DelayUpdateHelper(ThreadStart start, int delayMs)
        {
            _start = start;
            _delayMs = delayMs;
        }
        private readonly int _delayMs;
        private readonly ThreadStart _start;

        public void StartBackgroundUpdate(string name)
        {
            Thread t = ThreadHelper.NewThread(DelayStart, name, true, false, true);
            t.Start();
        }

        private void DelayStart()
        {
            Thread.Sleep(_delayMs);
            _start();
        }
    }
}
