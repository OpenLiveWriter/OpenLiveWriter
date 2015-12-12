// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Net;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public class BlogClientPostUrlNotFoundException : BlogClientException
    {
        public BlogClientPostUrlNotFoundException(string targetUrl, string description)
            : base(StringId.BCEPostUrlNotFoundTitle,
                    StringId.BCEPostUrlNotFoundMessage,
                    targetUrl,
                    description)
        {
        }
    }

    public class BlogClientHttpErrorException : BlogClientException
    {
        private WebException _exception;
        public BlogClientHttpErrorException(string targetUrl, string description, WebException exception)
            : base(StringId.BCEHttpErrorTitle,
                    StringId.BCEHttpErrorMessage,
                    targetUrl,
                    description)
        {
            _exception = exception;
        }

        public WebException Exception
        {
            get
            {
                return _exception;
            }
        }
    }

    public class BlogClientConnectionErrorException : BlogClientException
    {
        public BlogClientConnectionErrorException(string targetUrl, string description)
            : base(StringId.BCEConnectionErrorTitle,
                    StringId.BCEConnectionErrorMessage,
                    targetUrl,
                    description)
        {
            TargetUrl = targetUrl;
            Description = description;
        }

        public readonly string TargetUrl;
        public readonly string Description;
    }

    public class BlogClientInvalidServerResponseException : BlogClientException
    {
        public BlogClientInvalidServerResponseException(string method, string errorMessage, string response)
            : base(StringId.BCEInvalidServerResponseTitle,
                    StringId.BCEInvalidServerResponseMessage,
                    method,
                    errorMessage)
        {
            Method = method;
            ErrorMessage = errorMessage;
            Response = response;
        }

        public readonly string Method;

        public readonly string ErrorMessage = String.Empty;

        public readonly string Response;
    }

    public class BlogClientFileTransferException : BlogClientException
    {
        public BlogClientFileTransferException(string context, string errorCode, string errorMessage)
            : base(StringId.BCEFileTransferTitle,
                    StringId.BCEFileTransferMessage,
                    context,
                    errorCode,
                    errorMessage)
        {
        }

        public BlogClientFileTransferException(FileInfo file, string errorCode, string errorMessage)
            : this(string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.BCEFileTransferTransferringFile), file.Name), errorCode, errorMessage)
        {
        }
    }

    public class BlogClientFileUploadNotSupportedException : BlogClientException
    {
        public BlogClientFileUploadNotSupportedException()
            : base(StringId.BCEFileUploadNotSupportedTitle,
                    StringId.BCEFileUploadNotSupportedMessage)
        {
        }

        public BlogClientFileUploadNotSupportedException(string errorCode, string errorString)
            : this()
        {
            ErrorCode = errorCode;
            ErrorString = errorString;
        }

        public readonly string ErrorCode = String.Empty;
        public readonly string ErrorString = String.Empty;
    }

    public class BlogClientAccessDeniedException : BlogClientAuthenticationException
    {
        public BlogClientAccessDeniedException(string providerErrorCode, string providerErrorString)
            : base(providerErrorCode, providerErrorString)
        {
        }

    }

    public class BlogClientAuthenticationException : BlogClientProviderException
    {
        private WebException _webException;
        public BlogClientAuthenticationException(string providerErrorCode, string providerErrorString) :
        this(providerErrorCode, providerErrorString, null)
        {

        }
        public BlogClientAuthenticationException(string providerErrorCode, string providerErrorString, WebException webException)
            : base(StringId.BCEAuthenticationTitle,
                    StringId.BCEAuthenticationMessage,
                    providerErrorCode,
                    providerErrorString)
        {
            _webException = webException;
        }

        public WebException WebException
        {
            get { return _webException; }
        }
    }

    public class BlogClientProviderException : BlogClientException
    {
        public BlogClientProviderException(string providerErrorCode, string providerErrorString)
            : this(StringId.BCEProviderTitle, StringId.BCEProviderMessage, providerErrorCode, providerErrorString)
        {
        }

        public BlogClientProviderException(StringId titleFormat, StringId textFormat, string providerErrorCode, string providerErrorString)
            : base(titleFormat, textFormat, providerErrorCode, providerErrorString)
        {
            ErrorCode = providerErrorCode;
            ErrorString = providerErrorString;
        }

        public readonly string ErrorCode;
        public readonly string ErrorString;
    }

    public class BlogClientIOException : BlogClientException
    {
        public BlogClientIOException(string context, IOException ioException)
            : base(StringId.BCENetworkIOTitle,
                    StringId.BCENetworkIOMessage,
                    context,
                    ioException.GetType().Name,
                    ioException.Message)
        {
        }

        public BlogClientIOException(FileInfo file, IOException ioException)
            : base(StringId.BCEFileIOTitle,
                    StringId.BCEFileIOMessage,
                    file.Name,
                    ioException.GetType().Name,
                    ioException.Message)
        {
        }
    }

    public class BlogClientOperationCancelledException : BlogClientException
    {
        public BlogClientOperationCancelledException()
            : base(StringId.BCEOperationCancelledTitle, StringId.BCEOperationCancelledMessage)
        {
        }
    }

    public class BlogClientMethodUnsupportedException : BlogClientException
    {
        public BlogClientMethodUnsupportedException(string methodName)
            : base(StringId.BCEMethodUnsupportedTitle, StringId.BCEMethodUnsupportedMessage, methodName)
        {
        }
    }

    public class BlogClientPostAsDraftUnsupportedException : BlogClientException
    {
        public BlogClientPostAsDraftUnsupportedException()
            : base(StringId.BCEPostAsDraftUnsupportedTitle, StringId.BCEPostAsDraftUnsupportedMessage)
        {
        }
    }

    public class BlogClientException : DisplayableException
    {
        public BlogClientException(StringId titleFormat, StringId textFormat, params object[] textFormatArgs)
            : base(titleFormat, textFormat, textFormatArgs)
        {
        }

        public BlogClientException(string title, string text, params object[] textFormatArgs)
            : base(title, text, textFormatArgs)
        {
        }
    }
}

