// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using OpenLiveWriter.Publishing.Xml;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Cross-platform port of the MetaWeblog publish transport from
    /// <c>OpenLiveWriter.BlogClient.Clients.MetaweblogClient</c>, scoped to the
    /// minimal <c>metaWeblog.newPost</c> / <c>metaWeblog.editPost</c> path.
    ///
    /// The XML-RPC payload (title, description=MainContents,
    /// mt_text_more=ExtendedContents, categories, publish flag) is built exactly
    /// like the Windows <c>GeneratePostStruct</c>. Payload building is fully
    /// offline/testable; <see cref="NewPost"/>/<see cref="EditPost"/> transmit over
    /// HTTP where an endpoint is configured.
    /// </summary>
    public class MetaWeblogXmlRpcClient : IBlogClient
    {
        private const string DefaultUserAgent = "OpenLiveWriter";

        private readonly string _endpointUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _userAgent;
        private readonly HttpClient _httpClient;

        public MetaWeblogXmlRpcClient(
            string endpointUrl,
            string username,
            string password,
            IBlogClientOptions options = null,
            string userAgent = null,
            HttpClient httpClient = null)
        {
            _endpointUrl = endpointUrl;
            _username = username ?? string.Empty;
            _password = password ?? string.Empty;
            Options = options ?? BlogClientOptions.Default;
            _userAgent = userAgent ?? DefaultUserAgent;
            _httpClient = httpClient;
        }

        public IBlogClientOptions Options { get; }

        // -----------------------------------------------------------------------
        // Payload building (offline, no network) — faithful to GeneratePostStruct.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the MetaWeblog post struct: title, description/mt_text_more (or a
        /// merged description), and inline categories. Mirrors the Windows
        /// <c>GeneratePostStruct</c> for the minimal publish path.
        /// </summary>
        public XmlRpcStruct GeneratePostStruct(BlogPost post, bool publish)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            var members = new List<XmlRpcMember>
            {
                new XmlRpcMember("title", new XmlRpcString(post.Title ?? string.Empty))
            };

            if (Options.SupportsExtendedEntries && !post.IsPage)
            {
                // set the main and extended contents as separate fields
                members.Add(new XmlRpcMember("description", new XmlRpcString(post.MainContents)));
                members.Add(new XmlRpcMember("mt_text_more", new XmlRpcString(post.ExtendedContents)));
            }
            else
            {
                // merge the main and extended contents into a single field
                string contents = post.MainContents;
                if (!string.IsNullOrEmpty(post.ExtendedContents))
                    contents += post.ExtendedContents;
                members.Add(new XmlRpcMember("description", new XmlRpcString(contents)));
            }

            if (!post.IsPage && Options.SupportsCategoriesInline)
            {
                XmlRpcArray categories = GenerateCategoriesForPost(post);
                if (categories != null)
                    members.Add(new XmlRpcMember("categories", categories));
            }

            return new XmlRpcStruct(members.ToArray());
        }

        private static XmlRpcArray GenerateCategoriesForPost(BlogPost post)
        {
            if (post.Categories == null || post.Categories.Count == 0)
                return null;

            var values = new List<XmlRpcValue>();
            foreach (string category in post.Categories)
                values.Add(new XmlRpcString(category));

            return new XmlRpcArray(values.ToArray());
        }

        /// <summary>Builds the full <c>metaWeblog.newPost</c> method-call XML (no network).</summary>
        public string BuildNewPostXml(string blogId, BlogPost post, bool publish)
        {
            return BuildMethodCallXml("metaWeblog.newPost",
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePostStruct(post, publish),
                new XmlRpcBoolean(publish));
        }

        /// <summary>Builds the full <c>metaWeblog.editPost</c> method-call XML (no network).</summary>
        public string BuildEditPostXml(BlogPost post, bool publish)
        {
            return BuildMethodCallXml("metaWeblog.editPost",
                new XmlRpcString(post.Id),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePostStruct(post, publish),
                new XmlRpcBoolean(publish));
        }

        /// <summary>Serializes an XML-RPC method call to a UTF-8 string.</summary>
        public static string BuildMethodCallXml(string methodName, params XmlRpcValue[] parameters)
        {
            var encoding = new UTF8Encoding(false, false);
            byte[] bytes = GetRequestBytes(encoding, methodName, parameters);
            return encoding.GetString(bytes);
        }

        private static byte[] GetRequestBytes(Encoding encoding, string methodName, XmlRpcValue[] parameters)
        {
            using var request = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Encoding = encoding,
                // Some configs of WordPress complain about malformed XML when uploading
                // large posts/images unless the payload is indented; match the Windows client.
                Indent = true,
                IndentChars = " "
            };

            using (var writer = XmlWriter.Create(request, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("methodCall");

                writer.WriteStartElement("methodName");
                writer.WriteString(methodName);
                writer.WriteEndElement();

                writer.WriteStartElement("params");
                foreach (XmlRpcValue param in parameters)
                {
                    writer.WriteStartElement("param");
                    param.Write(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // params

                writer.WriteEndElement(); // methodCall
                writer.WriteEndDocument();
            }

            return request.ToArray();
        }

        // -----------------------------------------------------------------------
        // Transport — transmits the payload over HTTP.
        // -----------------------------------------------------------------------

        public string NewPost(string blogId, BlogPost post, bool publish)
        {
            XmlRpcMethodResponse response = CallMethod("metaWeblog.newPost",
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePostStruct(post, publish),
                new XmlRpcBoolean(publish));

            string postId = response.Response?.InnerText ?? string.Empty;
            post.Id = postId;
            return postId;
        }

        public void EditPost(string blogId, BlogPost post, bool publish)
        {
            CallMethod("metaWeblog.editPost",
                new XmlRpcString(post.Id),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePostStruct(post, publish),
                new XmlRpcBoolean(publish));
        }

        private XmlRpcMethodResponse CallMethod(string methodName, params XmlRpcValue[] parameters)
        {
            if (string.IsNullOrEmpty(_endpointUrl))
                throw new InvalidOperationException("No XML-RPC endpoint URL was configured for this client.");

            var encoding = new UTF8Encoding(false, false);
            byte[] requestBytes = GetRequestBytes(encoding, methodName, parameters);

            HttpClient client = _httpClient ?? SharedHttpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl);
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            request.Content = new ByteArrayContent(requestBytes);
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml") { CharSet = encoding.WebName };

            HttpResponseMessage response = client.Send(request);
            response.EnsureSuccessStatusCode();

            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream, encoding);
            string responseText = reader.ReadToEnd();

            var xmlRpcResponse = new XmlRpcMethodResponse(responseText);
            if (xmlRpcResponse.FaultOccurred)
            {
                throw new BlogClientPublishException(
                    $"XML-RPC fault {xmlRpcResponse.FaultCode}: {xmlRpcResponse.FaultString}");
            }
            return xmlRpcResponse;
        }

        private static readonly HttpClient SharedHttpClient = new HttpClient();
    }

    /// <summary>Raised when the remote server returns an XML-RPC fault during publish.</summary>
    public class BlogClientPublishException : Exception
    {
        public BlogClientPublishException(string message) : base(message)
        {
        }
    }
}
