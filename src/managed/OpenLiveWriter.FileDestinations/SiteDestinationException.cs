// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.FileDestinations
{

    /// <summary>
    /// SiteEncoder Exception Class
    /// </summary>
    public class SiteDestinationException : ResourceFileException
    {

        /// <summary>
        /// A general transfer exception
        /// {0} The type of transfer that is being attempted
        /// {1} The inner ErrorCode
        /// </summary>
        public static readonly string TransferException = "TransferException";

        /// <summary>
        /// A exception that occurs while attempting to connect to a destination
        /// {0} The type of transfer that is being attempted
        /// {1} The inner ErrorCode
        /// </summary>
        public static readonly string ConnectionException = "ConnectionException";

        /// <summary>
        /// An exception occurs while attempting to login to a destination
        /// {0} The type of transfer that is being attempted
        /// {1} The inner ErrorCode
        /// </summary>
        public static readonly string LoginException = "LoginException";

        /// <summary>
        /// An unexpected exception has occurred.
        /// {0} The type of transfer that is being attempted
        /// {1} The inner ErrorCode
        /// </summary>
        public static readonly string UnexpectedException = "UnexpectedException";

        /// <summary>
        /// The type of ISiteDestination that is throwing the exception
        /// </summary>
        public string DestinationType;

        /// <summary>
        /// Error code associated with the destination error
        /// </summary>
        public int DestinationErrorCode;

        /// <summary>
        /// Any additional information or extended message associated with this exception
        /// </summary>
        public string DestinationExtendedMessage;

        /// <summary>
        /// SiteDestinationException constructor
        /// </summary>
        /// <param name="innerException">any caught exception to be kept in the exception chain</param>
        /// <param name="exceptionType">The type of Exception- see SiteDestinationException members for
        /// exception types</param>
        /// <param name="arguments">Any exception type specific arguments</param>
        public SiteDestinationException(Exception innerException, string exceptionType,
            params object[] arguments) : base(innerException, typeof(SiteDestinationException), exceptionType, arguments)
        {
        }

    }
}
