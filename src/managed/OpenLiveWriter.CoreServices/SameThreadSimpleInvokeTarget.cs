// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Invoke target used to just callback on the same thread
    /// </summary>
    public class SameThreadSimpleInvokeTarget : ISynchronizeInvoke
    {
        public bool InvokeRequired
        {
            get
            {
                return false;
            }
        }

        public object Invoke(Delegate method, object[] args)
        {
            return method.DynamicInvoke(args);
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            string error = "Cannot call BeginInvoke on SameThreadSimpleInvokeTarget";
            Debug.Fail(error);
            throw new InvalidOperationException(error);
        }

        public object EndInvoke(IAsyncResult result)
        {
            string error = "Cannot call EndInvoke on SameThreadSimpleInvokeTarget";
            Debug.Fail(error);
            throw new InvalidOperationException(error);
        }
    }
}
