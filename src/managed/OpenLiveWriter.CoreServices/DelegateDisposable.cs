// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public delegate void Disposer();

    public class DelegateDisposable : IDisposable
    {
        private Disposer disposer;

        public DelegateDisposable(Disposer disposer)
        {
            this.disposer = disposer;
        }

        public void Dispose()
        {
            Disposer temp;

            lock (this)
            {
                temp = disposer;
                disposer = null;
            }

            if (temp != null)
                temp();
        }
    }
}
