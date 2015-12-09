// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    public class PaddedWaitCursor : IDisposable
    {
        public PaddedWaitCursor(int msPadding)
        {
            _targetEndTime = DateTime.Now.AddMilliseconds(msPadding);
            _previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            // sleep if necessary
            TimeSpan padTime = _targetEndTime.Subtract(DateTime.Now);
            if (padTime.Milliseconds > 0)
                Thread.Sleep(padTime);

            Cursor.Current = _previousCursor;
        }

        Cursor _previousCursor;
        DateTime _targetEndTime;
    }
}
