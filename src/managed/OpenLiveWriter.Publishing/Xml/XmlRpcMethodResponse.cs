// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Xml;

namespace OpenLiveWriter.Publishing.Xml
{
    /// <summary>
    /// Cross-platform port of <c>OpenLiveWriter.CoreServices.XmlRpcMethodResponse</c>.
    /// Parses an XML-RPC method response, exposing the response value or the fault.
    /// </summary>
    public class XmlRpcMethodResponse
    {
        public XmlRpcMethodResponse(string responseText)
        {
            try
            {
                var document = new XmlDocument();
                if (responseText != null)
                    responseText = responseText.TrimStart(' ', '\t', '\r', '\n');
                document.LoadXml(responseText);

                XmlNode responseValue = document.SelectSingleNode("/methodResponse/params/param/value");
                if (responseValue != null)
                {
                    _response = responseValue;
                }
                else
                {
                    _faultOccurred = true;

                    XmlNode errorCode = document.SelectSingleNode("/methodResponse/fault/value/struct/member[name='faultCode']/value");
                    _faultCode = errorCode?.InnerText ?? string.Empty;

                    XmlNode errorString = document.SelectSingleNode("/methodResponse/fault/value/struct/member[name='faultString']/value");
                    _faultString = errorString?.InnerText ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new XmlRpcClientInvalidResponseException(responseText, ex);
            }
        }

        public XmlNode Response => _response;
        private readonly XmlNode _response;

        public bool FaultOccurred => _faultOccurred;
        private readonly bool _faultOccurred;

        public string FaultCode => _faultCode;
        private readonly string _faultCode = string.Empty;

        public string FaultString => _faultString;
        private readonly string _faultString = string.Empty;
    }

    public class XmlRpcClientInvalidResponseException : Exception
    {
        public XmlRpcClientInvalidResponseException(string response, Exception innerException)
            : base("Invalid response document returned from XmlRpc server", innerException)
        {
            Response = response;
        }

        public readonly string Response;
    }
}
