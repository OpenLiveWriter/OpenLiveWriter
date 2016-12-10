// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    using JetBrains.Annotations;

    /// <summary>
    /// Typed-exception that occurs when an HTTP request times out after the request has been sent, but
    /// before the response is received.
    /// </summary>
    /// <seealso cref="System.Net.WebException" />
    [Serializable]
    public class WebResponseTimeoutException : WebException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseTimeoutException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public WebResponseTimeoutException([CanBeNull] WebException innerException)
            : base(
                  innerException?.Message,
                  innerException,
                  innerException?.Status ?? WebExceptionStatus.UnknownError,
                  innerException?.Response)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseTimeoutException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected WebResponseTimeoutException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
