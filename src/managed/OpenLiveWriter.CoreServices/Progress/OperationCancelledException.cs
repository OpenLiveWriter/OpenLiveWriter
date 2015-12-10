// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices.Diagnostics;

namespace OpenLiveWriter.CoreServices.Progress
{
    /// <summary>
    /// Exception for interrupting control flow when the the operation is cancelled.
    /// </summary>
    public class OperationCancelledException : Exception
    {
        static OperationCancelledException()
        {
            DynamicExceptionMessageRegistry.Instance.AddMessage(
                new SimpleDynamicExceptionMessage(typeof(OperationCancelledException), null, true));
        }

        public OperationCancelledException() : base("")
        {
        }

        public OperationCancelledException(string message) : base(message)
        {
        }
    }
}
