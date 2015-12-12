// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using OpenLiveWriter.CoreServices.Diagnostics;

namespace OpenLiveWriter.CoreServices
{

    public class XmlRpcClient
    {
        /// <summary>
        /// Create an XmlRpcClient for the specified host name
        /// </summary>
        /// <param name="hostname"></param>
        public XmlRpcClient(string hostname, string userAgent)
        {
            _hostname = hostname;
            _userAgent = userAgent;
        }

        /// <summary>
        /// Create an XmlRpcClient for the specified host name
        /// </summary>
        /// <param name="hostname"></param>
        public XmlRpcClient(string hostname, string userAgent, HttpRequestFilter filter, string transportEncoding)
        {
            _hostname = hostname;
            _userAgent = userAgent;
            _requestFilter = filter;
            _transportEncoding = transportEncoding;
        }

        /// <summary>
        /// Call an XmlRpc method
        /// </summary>
        /// <param name="methodName">method name</param>
        /// <param name="parameters">variabile length list of parameters</param>
        /// <returns>response from the remote host</returns>
        /// <exception cref="Exception">allows all exceptions to propogate out of the method</exception>
        public XmlRpcMethodResponse CallMethod(string methodName, params XmlRpcValue[] parameters)
        {
            //select the encoding
            Encoding encodingToUse = StringHelper.GetEncoding(_transportEncoding, new UTF8Encoding(false, false));

            // build the XmlRpc packet
            byte[] requestBytes = GetRequestBytes(encodingToUse, methodName, parameters, false);

            if (ApplicationDiagnostics.VerboseLogging)
            {
                LogXmlRpcRequest(encodingToUse, methodName, parameters);
            }

            // send the request
            HttpWebResponse response;
            try
            {
                response = HttpRequestHelper.SendRequest(_hostname, delegate (HttpWebRequest request)
                {
                    request.Method = "POST";
                    request.AllowAutoRedirect = false;
                    request.ContentType = String.Format(CultureInfo.InvariantCulture, "{0};charset={1}", MimeHelper.TEXT_XML, encodingToUse.WebName);
                    if (_requestFilter != null)
                        _requestFilter(request);
                    using (Stream requestStream = request.GetRequestStream())
                        StreamHelper.Transfer(new MemoryStream(requestBytes), requestStream);
                });
            }
            catch
            {
                if (!ApplicationDiagnostics.VerboseLogging)  // if test mode, request has already been logged
                {
                    LogXmlRpcRequest(encodingToUse, methodName, parameters);
                }

                throw;
            }

            // WinLive 616: The response encoding may not necessarily be the same as our request encoding. Attempt to
            // use the encoding specified in the HTTP header.
            string characterSet;
            if (TryGetCharacterSet(response, out characterSet))
            {
                encodingToUse = StringHelper.GetEncoding(characterSet, encodingToUse);
            }

            // return the response
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encodingToUse))
            {
                string xmlRpcString = reader.ReadToEnd();

                if (ApplicationDiagnostics.VerboseLogging)
                {
                    LogXmlRpcResponse(xmlRpcString);
                }

                try
                {
                    XmlRpcMethodResponse xmlRpcResponse = new XmlRpcMethodResponse(xmlRpcString);
                    if (xmlRpcResponse.FaultOccurred)
                    {
                        if (!ApplicationDiagnostics.VerboseLogging)  // if test mode, response has already been logged
                        {
                            LogXmlRpcRequest(encodingToUse, methodName, parameters);
                            LogXmlRpcResponse(xmlRpcString);
                        }
                    }
                    return xmlRpcResponse;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception parsing XML-RPC response:\r\n\r\n" + ex.ToString() + "\r\n\r\n" + xmlRpcString);
                    throw;
                }
            }

        }

        /// <summary>
        /// Gets the character set associated with the WebResponse.
        /// </summary>
        /// <param name="response">The WebResponse to inspect for a character set.</param>
        /// <param name="characterSet">When this method returns, contains the character set associated with the
        /// WebResponse if the character set is explicitly specified; otherwise, null. This parameter is passed
        /// uninitialized.</param>
        /// <returns>true if the WebResponse explicitly specifies a character set; otherwise, false.</returns>
        private bool TryGetCharacterSet(HttpWebResponse response, out string characterSet)
        {
            // A very applicable comment from MSDN on why not to use the HttpWebResponse.CharacterSet property:
            // http://msdn.microsoft.com/en-us/library/system.net.httpwebresponse.characterset(v=VS.80).aspx
            //
            // "As stated in a previous comment, many web servers are poorly configured and don't include the charset
            // in their content type header, e. g. they just return "text/html". In theory, user agents should treat
            // it as ISO-8859-1, as recommended by W3C. This is what the CharacterSet property actually does: It
            // always returns ISO-8859-1 if the charset it not specified, although often the content has a different
            // encoding (which of course HttpWebResponse cannot know).
            // In real life however, in case of a missing charset definition in the HTTP header user agents look into
            // the markup, and usully a meta tag can be found that contains the correct encoding, like "utf-8". To
            // implement this pragmatic approach, it would IMHO be much more convenient if the CharacterSet would
            // default to an empty string, then you know, that the encoding is not specified and you need a workaround
            // to determine the correct encoding to use.
            // The only workaround that I could find for me was to parse ContentType myself to extract the character
            // set, and ignore ContentEncoding and CharacterSet because they are useless."

            string contentType = response.ContentType;
            if (!String.IsNullOrEmpty(contentType))
            {
                IDictionary values = MimeHelper.ParseContentType(contentType, true);

                const string charset = "charset";
                if (values.Contains(charset))
                {
                    characterSet = values[charset] as string;
                    if (!String.IsNullOrEmpty(characterSet))
                    {
                        Debug.Assert(characterSet.Equals(response.CharacterSet, StringComparison.OrdinalIgnoreCase),
                            "CharacterSet was parsed incorrectly!");
                        return true;
                    }
                }
            }

            characterSet = null;
            return false;
        }

        private void LogXmlRpcRequest(Encoding encodingToUse, string methodName, XmlRpcValue[] parameters)
        {
            Trace.WriteLine("XML-RPC request:\r\n" + _hostname + "\r\n" + encodingToUse.GetString(GetRequestBytes(encodingToUse, methodName, parameters, true)));
        }

        private void LogXmlRpcResponse(string xmlRpcString)
        {
            Trace.WriteLine("XML-RPC response:\r\n" + _hostname + "\r\n" + xmlRpcString);
        }

        private static byte[] GetRequestBytes(Encoding encoding, string methodName, XmlRpcValue[] parameters, bool logging)
        {
            MemoryStream request = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(request, encoding);

            // Amazingly, some configs of WordPress complain
            // about malformed XML when uploading large posts/images (greater than
            // 100,000 bytes) if we don't indent. More precisely, there needs to be
            // fewer than 100,000 bytes before the first line break. Let's just
            // indent and be done with it.
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 1;
            writer.IndentChar = ' ';

            writer.WriteStartDocument();
            using (new WriteXmlElement(writer, "methodCall"))
            {
                using (new WriteXmlElement(writer, "methodName"))
                    writer.WriteString(methodName);

                using (new WriteXmlElement(writer, "params"))
                {
                    foreach (XmlRpcValue param in parameters)
                    {
                        using (new WriteXmlElement(writer, "param"))
                            param.Write(writer, logging);
                    }
                }
            }
            writer.WriteEndDocument();
            writer.Flush();

            return request.ToArray();
        }

        //private Encoding _utf8EncodingNoBOM = new UTF8Encoding(false) ;
        private string _hostname;
        private string _userAgent;
        private HttpRequestFilter _requestFilter;
        private string _transportEncoding;
    }

    public abstract class XmlRpcValue
    {
        protected XmlRpcValue(object value)
            : this(value, false)
        { }

        protected XmlRpcValue(object value, bool suppressLog)
        {
            _value = value;
            _suppressLog = suppressLog;
        }

        public void Write(XmlWriter writer)
        {
            Write(writer, false);
        }

        public void Write(XmlWriter writer, bool logging)
        {
            using (new WriteXmlElement(writer, "value"))
            {
                WriteValue(writer, _value, logging);
            }
        }

        protected virtual void WriteValue(XmlWriter writer, object value, bool logging)
        {
            if (!_suppressLog || !logging)
                WriteValue(writer, value);
            else
                writer.WriteString("[removed]");
        }

        protected abstract void WriteValue(XmlWriter writer, object value);

        private object _value;
        private readonly bool _suppressLog;
    }

    public class XmlRpcString : XmlRpcValue
    {
        public XmlRpcString(string value, bool suppressLog)
            : base(value, suppressLog)
        {
        }

        public XmlRpcString(string value)
            : base(value)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "string"))
                writer.WriteString(value as string);
        }
    }

    public class XmlRpcBase64 : XmlRpcValue
    {
        public XmlRpcBase64(byte[] bytes)
            : base(bytes)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value, bool logging)
        {
            byte[] bytes = (byte[])value;
            using (new WriteXmlElement(writer, "base64"))
            {
                if (!logging)
                    writer.WriteBase64(bytes, 0, bytes.Length);
                else
                    writer.WriteString(string.Format(CultureInfo.InvariantCulture, "[{0} bytes]", bytes.Length));
            }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            Trace.Fail("This should never be called");
        }
    }

    public class XmlRpcInt : XmlRpcValue
    {
        public XmlRpcInt(int value)
            : base(value)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "int"))
                writer.WriteString(((int)value).ToString(CultureInfo.InvariantCulture));
        }
    }

    public class XmlRpcBoolean : XmlRpcValue
    {
        public XmlRpcBoolean(bool value)
            : base(value)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "boolean"))
                writer.WriteString((bool)value ? "1" : "0");

        }
    }

    public class BloggerXmlRpcFormatTime : XmlRpcValue
    {
        private string formatString;
        public BloggerXmlRpcFormatTime(DateTime value, string format)
            : base(value)
        {
            if (String.Empty == format)
            {
                formatString = "yyyy-MM-dd'T'HH':'mm':'ss";
            }
            else
            {
                formatString = format;
            }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "dateTime.iso8601"))
                writer.WriteString(((DateTime)value).ToString(formatString, CultureInfo.InvariantCulture));
        }
    }

    public class XmlRpcFormatTime : XmlRpcValue
    {
        private string formatString;
        public XmlRpcFormatTime(DateTime value, string format)
            : base(value)
        {
            if (String.Empty == format)
            {
                formatString = "yyyyMMdd'T'HH':'mm':'ss";
            }
            else
            {
                formatString = format;
            }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "dateTime.iso8601"))
                writer.WriteString(((DateTime)value).ToString(formatString, CultureInfo.InvariantCulture));
        }
    }

    public class XmlRpcArray : XmlRpcValue
    {
        public XmlRpcArray(XmlRpcValue[] values)
            : base(values)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value, bool logging)
        {
            using (new WriteXmlElement(writer, "array"))
            using (new WriteXmlElement(writer, "data"))
                foreach (XmlRpcValue val in (value as XmlRpcValue[]))
                {
                    val.Write(writer, logging);
                }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            Trace.Fail("This should never be called");
        }
    }

    public class XmlRpcStruct : XmlRpcValue
    {
        /// <summary>
        /// A structure is a dictionary of names and values
        /// </summary>
        /// <param name="values"></param>
        public XmlRpcStruct(XmlRpcMember[] members)
            : base(members)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value, bool logging)
        {
            using (new WriteXmlElement(writer, "struct"))
            {
                foreach (XmlRpcMember member in (value as XmlRpcMember[]))
                {
                    using (new WriteXmlElement(writer, "member"))
                    {
                        using (new WriteXmlElement(writer, "name"))
                            writer.WriteString(member.Name);
                        member.Value.Write(writer, logging);
                    }
                }
            }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            Trace.Fail("This should never be called");
        }
    }

    public class XmlRpcMember
    {
        public XmlRpcMember(string name, string value)
            : this(name, new XmlRpcString(value))
        {
        }

        public XmlRpcMember(string name, string value, bool suppressLog)
            : this(name, new XmlRpcString(value, suppressLog))
        {
        }

        public XmlRpcMember(string name, bool value)
            : this(name, new XmlRpcBoolean(value))
        {
        }

        public XmlRpcMember(string name, int value)
            : this(name, new XmlRpcInt(value))
        {
        }

        public XmlRpcMember(string name, XmlRpcMember[] members)
            : this(name, new XmlRpcStruct(members))
        {
        }

        public XmlRpcMember(string name, XmlRpcValue value)
        {
            Name = name;
            _value = value;
        }

        public readonly string Name;
        public XmlRpcValue Value
        {
            get
            {
                return _value;
            }
        }
        private readonly XmlRpcValue _value;
    }

    public class XmlRpcMethodResponse
    {
        internal XmlRpcMethodResponse(string responseText)
        {
            try
            {
                // analyze the response text to determine the content of the response
                XmlDocument document = new XmlDocument();
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
                    // fault occurred
                    _faultOccurred = true;

                    XmlNode errorCode = document.SelectSingleNode("/methodResponse/fault/value/struct/member[name='faultCode']/value");
                    _faultCode = errorCode.InnerText;

                    XmlNode errorString = document.SelectSingleNode("/methodResponse/fault/value/struct/member[name='faultString']/value");
                    _faultString = errorString.InnerText;
                }

            }
            catch (Exception ex)
            {
                throw new XmlRpcClientInvalidResponseException(responseText, ex);
            }
        }

        public XmlNode Response
        {
            get
            {
                return _response;
            }
        }
        private XmlNode _response = null;

        public bool FaultOccurred
        {
            get
            {
                return _faultOccurred;
            }
        }
        private bool _faultOccurred = false;

        public string FaultCode
        {
            get
            {
                return _faultCode;
            }
        }
        private string _faultCode = String.Empty;

        public string FaultString
        {
            get
            {
                return _faultString;
            }
        }
        private string _faultString = String.Empty;

    }

    public class XmlRpcClientInvalidResponseException : ApplicationException
    {
        public XmlRpcClientInvalidResponseException(string response, Exception innerException)
            : base("Invalid response document returned from XmlRpc server", innerException)
        {
            Response = response;
        }

        public readonly string Response;
    }

    /// <summary>
    /// Utility class used to write elements
    /// </summary>
    internal class WriteXmlElement : IDisposable
    {
        public WriteXmlElement(XmlWriter writer, string elName)
        {
            _writer = writer;
            _writer.WriteStartElement(elName);
        }

        public void Dispose()
        {
            _writer.WriteEndElement();
        }

        private XmlWriter _writer;
    }
}
