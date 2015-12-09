// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Progress
{

    public class TickableProgressTick
    {
        private IProgressHost _progress;
        private int _ticks;
        private int _currentTicks;

        public TickableProgressTick(IProgressHost progress,
            int ticks)
        {
            _progress = progress;
            _ticks = ticks;
            _currentTicks = 0;
        }

        public void Tick()
        {
            if (_currentTicks < _ticks)

                _progress.UpdateProgress(++_currentTicks, _ticks);

        }

        public bool CancelRequested
        {
            get
            {
                return _progress.CancelRequested;
            }
        }

        public void Message(string message)
        {
            _progress.UpdateProgress(message);
        }
    }
}
