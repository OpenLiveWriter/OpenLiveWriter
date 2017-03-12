// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;

namespace OpenLiveWriter.CoreServices.Threading
{
    public delegate void ThreadStartWithParamsDelegate(object[] parameters);

    public class ThreadStartWithParams
    {
        private readonly ThreadStartWithParamsDelegate _delegate;
        private readonly object[] _params;

        private ThreadStartWithParams(ThreadStartWithParamsDelegate ts, object[] parameters)
        {
            _delegate = ts;
            _params = parameters;
        }

        [STAThread]
        private void Run()
        {
            _delegate(_params);
        }

        public static ThreadStart Create(ThreadStartWithParamsDelegate ts, params object[] parameters)
        {
            return new ThreadStart(new ThreadStartWithParams(new ThreadStartWithParamsDelegate(ts), parameters).Run);
        }
    }
}
