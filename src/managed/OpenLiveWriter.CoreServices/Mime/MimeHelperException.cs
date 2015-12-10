// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// MimeDocumentWriter Exception Class
    /// </summary>
    public class MimeHelperException : ApplicationException
    {
        public static MimeHelperException ForEncodingException(Exception innerException)
        {
            string message = "An encoding exception has occurred";
            return new MimeHelperException(message, innerException);
        }

        public static MimeHelperException ForIllegalEncodingCharacter(char illegalChar, string encodingType, Exception innerException)
        {
            string message = String.Format(CultureInfo.CurrentCulture, "An illegal character was encountered, with character code: {0} [{1}]", (int)illegalChar, encodingType);
            return new MimeHelperException(message, innerException);
        }

        public static MimeHelperException ForUnableToSetContentType(string encodingType, Exception innerException)
        {
            string message = String.Format(CultureInfo.CurrentCulture, "Can't set content type: {0}", encodingType);
            return new MimeHelperException(message, innerException);
        }

        /// <summary>
        /// MimeHelperException constructor
        /// </summary>
        /// <param name="innerException">any caught exception to be kept in the exception chain</param>
        /// <param name="exceptionType">The type of Exception- see MimeHelperException members for
        /// exception types</param>
        /// <param name="arguments">Any exception type specific arguments</param>
        private MimeHelperException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

    }
}

