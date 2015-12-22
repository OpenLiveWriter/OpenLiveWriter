// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.FileDestinations
{

    /// <summary>
    /// LoginException Exception Class
    /// </summary>
    public class LoginException : SiteDestinationException
    {
        /// <summary>
        /// LoginException constructor
        /// </summary>
        /// <param name="innerException">any caught exception to be kept in the exception chain</param>
        /// <param name="arguments">Any exception type specific arguments</param>
        public LoginException(Exception innerException, params object[] arguments)
            : base(innerException, SiteDestinationException.LoginException, arguments)
        {
        }
    }
}
