// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            if (!post.IsPage && Options.SupportsKeywords && !string.IsNullOrEmpty(post.Keywords))
                members.Add(new XmlRpcMember("mt_keywords", new XmlRpcString(post.Keywords)));

            if (post.DateCreatedUtc.HasValue)
                members.Add(new XmlRpcMember("dateCreated", new XmlRpcDateTime(post.DateCreatedUtc.Value)));

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
        // Transport — transmits the payload over HTTP (async end-to-end).
        // -----------------------------------------------------------------------

        public async Task<string> NewPostAsync(string blogId, BlogPost post, bool publish)
        {
            XmlRpcMethodResponse response = await CallMethodAsync("metaWeblog.newPost",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePostStruct(post, publish),
                new XmlRpcBoolean(publish)).ConfigureAwait(false);

            string postId = response.Response?.InnerText ?? string.Empty;
            post.Id = postId;
            return postId;
        }

        public Task EditPostAsync(string blogId, BlogPost post, bool publish)
        {
            return CallMethodAsync("metaWeblog.editPost",
                CancellationToken.None,
                new XmlRpcString(post.Id),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePostStruct(post, publish),
                new XmlRpcBoolean(publish));
        }

        /// <summary>
        /// Verifies the configured endpoint/credentials with a lightweight
        /// <c>blogger.getUsersBlogs</c> call (supported by MetaWeblog-compatible
        /// endpoints such as WordPress). Completes normally on success; throws
        /// <see cref="BlogClientPublishException"/> on an XML-RPC fault (e.g. bad
        /// credentials) and lets transport errors bubble up.
        /// </summary>
        public Task VerifyCredentialsAsync(CancellationToken cancellationToken = default)
        {
            return CallMethodAsync("blogger.getUsersBlogs",
                cancellationToken,
                new XmlRpcString(string.Empty), // appkey — unused by MetaWeblog providers
                new XmlRpcString(_username),
                new XmlRpcString(_password, true));
        }

        /// <summary>
        /// Uploads a media object via <c>metaWeblog.newMediaObject</c> and returns the
        /// hosted URL from the response struct's <c>url</c> member. Faithful to the
        /// Windows <c>DoBeforePublishUploadWork</c> upload struct (name/type/bits).
        /// </summary>
        public async Task<string> NewMediaObjectAsync(string blogId, string fileName, string mimeType, byte[] bits)
        {
            if (bits == null) throw new ArgumentNullException(nameof(bits));

            XmlRpcMethodResponse response = await CallMethodAsync("metaWeblog.newMediaObject",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                new XmlRpcStruct(new[]
                {
                    new XmlRpcMember("name", new XmlRpcString(CleanUploadFilename(fileName))),
                    new XmlRpcMember("type", new XmlRpcString(mimeType ?? "application/octet-stream")),
                    new XmlRpcMember("bits", new XmlRpcBase64(bits)),
                })).ConfigureAwait(false);

            XmlNode urlNode = response.Response?.SelectSingleNode("struct/member[name='url']/value");
            if (urlNode == null || string.IsNullOrEmpty(urlNode.InnerText))
            {
                throw new BlogClientPublishException(
                    "metaWeblog.newMediaObject returned no URL for the uploaded media object.");
            }
            return urlNode.InnerText;
        }

        /// <summary>Sanitizes an upload filename (avoids a WordPress '#' bug; see Windows client).</summary>
        private static string CleanUploadFilename(string filename) =>
            (filename ?? string.Empty).Replace("#", "_");

        /// <summary>
        /// Fetches the blog's categories via <c>metaWeblog.getCategories</c> and parses the
        /// returned array of category structs. Faithful to the Windows
        /// <c>MetaweblogGetCategories</c>/<c>ParseCategory</c> path.
        /// </summary>
        public async Task<IReadOnlyList<BlogPostCategory>> GetCategoriesAsync(string blogId)
        {
            XmlRpcMethodResponse response = await CallMethodAsync("metaWeblog.getCategories",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true)).ConfigureAwait(false);

            return ParseCategories(response.Response);
        }

        /// <summary>
        /// Parses a <c>metaWeblog.getCategories</c> response value node into categories.
        /// Pure/offline so it can be fixture-tested against sample XML. Tolerant of the
        /// common member permutations (description/title/categoryName for the name;
        /// categoryid/categoryId for the id) exactly like the Windows client.
        /// </summary>
        public static IReadOnlyList<BlogPostCategory> ParseCategories(XmlNode responseValue)
        {
            var categories = new List<BlogPostCategory>();
            if (responseValue == null)
                return categories;

            XmlNodeList categoryNodes = responseValue.SelectNodes("array/data/value/struct");
            if (categoryNodes == null)
                return categories;

            foreach (XmlNode node in categoryNodes)
            {
                string name = GetNodeValue(node, "member[name='description']/value");

                string title = GetNodeValue(node, "member[name='title']/value");
                if (!string.IsNullOrEmpty(title))
                    name = title;

                if (string.IsNullOrEmpty(name))
                    name = GetNodeValue(node, "member[name='categoryName']/value");

                string id = GetNodeValue(node, "member[name='categoryid']/value")
                    ?? GetNodeValue(node, "member[name='categoryId']/value");
                if (string.IsNullOrEmpty(id))
                    id = name;

                string parent = GetNodeValue(node, "member[name='parentId']/value") ?? string.Empty;

                if (string.IsNullOrEmpty(name))
                    continue; // malformed entry — skip rather than surface a null name

                categories.Add(new BlogPostCategory(id, name, parent));
            }

            return categories;
        }

        /// <summary>
        /// Parses a <c>metaWeblog.getCategories</c> XML-RPC method-response document string
        /// into categories. Convenience wrapper over <see cref="ParseCategories(XmlNode)"/>
        /// for fixture-based tests.
        /// </summary>
        public static IReadOnlyList<BlogPostCategory> ParseCategoriesResponse(string responseXml)
        {
            var response = new XmlRpcMethodResponse(responseXml);
            return ParseCategories(response.Response);
        }

        // -----------------------------------------------------------------------
        // Server fetch — reading posts/pages back from the blog.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Lists the most recent posts via <c>metaWeblog.getRecentPosts</c>. Faithful to
        /// the Windows <c>RecentPostSynchronizer</c> fetch: the returned structs carry the
        /// full body, so opening a listed post needs no second round-trip.
        /// </summary>
        public async Task<IReadOnlyList<ServerPost>> GetRecentPostsAsync(string blogId, int count)
        {
            XmlRpcMethodResponse response = await CallMethodAsync("metaWeblog.getRecentPosts",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                new XmlRpcInt(count)).ConfigureAwait(false);

            return ParseServerPosts(response.Response, isPage: false);
        }

        /// <summary>Fetches a single post in full via <c>metaWeblog.getPost</c>.</summary>
        public async Task<ServerPost> GetPostAsync(string postId)
        {
            XmlRpcMethodResponse response = await CallMethodAsync("metaWeblog.getPost",
                CancellationToken.None,
                new XmlRpcString(postId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true)).ConfigureAwait(false);

            return ParseServerPostStruct(response.Response?.SelectSingleNode("struct"), isPage: false);
        }

        /// <summary>
        /// Lists the blog's pages via <c>wp.getPages</c>. WordPress page structs carry the
        /// same members as post structs with page-flavored names (page_id / page_title /
        /// page_status); entries are marked <see cref="ServerPostInfo.IsPage"/>.
        /// </summary>
        public async Task<IReadOnlyList<ServerPost>> GetPagesAsync(string blogId)
        {
            XmlRpcMethodResponse response = await CallMethodAsync("wp.getPages",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true)).ConfigureAwait(false);

            return ParseServerPosts(response.Response, isPage: true);
        }

        /// <summary>
        /// Parses a <c>metaWeblog.getRecentPosts</c>/<c>wp.getPages</c> XML-RPC
        /// method-response document string into posts. Pure/offline for fixture tests.
        /// </summary>
        public static IReadOnlyList<ServerPost> ParseServerPostsResponse(string responseXml, bool isPage = false)
        {
            var response = new XmlRpcMethodResponse(responseXml);
            return ParseServerPosts(response.Response, isPage);
        }

        /// <summary>
        /// Parses a <c>metaWeblog.getPost</c> XML-RPC method-response document string into
        /// a single post. Pure/offline for fixture tests.
        /// </summary>
        public static ServerPost ParseGetPostResponse(string responseXml)
        {
            var response = new XmlRpcMethodResponse(responseXml);
            return ParseServerPostStruct(response.Response?.SelectSingleNode("struct"), isPage: false);
        }

        /// <summary>
        /// Parses the array-of-structs value returned by getRecentPosts/getPages.
        /// Tolerant like <see cref="ParseCategories"/>: missing/unknown members degrade
        /// to defaults rather than failing the whole list.
        /// </summary>
        private static IReadOnlyList<ServerPost> ParseServerPosts(XmlNode responseValue, bool isPage)
        {
            var posts = new List<ServerPost>();
            XmlNodeList structNodes = responseValue?.SelectNodes("array/data/value/struct");
            if (structNodes == null)
                return posts;

            foreach (XmlNode node in structNodes)
                posts.Add(ParseServerPostStruct(node, isPage));

            return posts;
        }

        /// <summary>
        /// Parses one post/page struct. Post member names (postid/title/post_status) and
        /// page member names (page_id/page_title/page_status) are both accepted so the
        /// same parser serves getRecentPosts, getPost, and wp.getPages.
        /// </summary>
        private static ServerPost ParseServerPostStruct(XmlNode structNode, bool isPage)
        {
            var post = new ServerPost { IsPage = isPage };
            if (structNode == null)
                return post;

            post.PostId = GetNodeValue(structNode, "member[name='postid']/value")
                ?? GetNodeValue(structNode, "member[name='page_id']/value")
                ?? string.Empty;
            post.Title = GetNodeValue(structNode, "member[name='title']/value")
                ?? GetNodeValue(structNode, "member[name='page_title']/value")
                ?? string.Empty;
            post.Description = GetNodeValue(structNode, "member[name='description']/value") ?? string.Empty;
            post.TextMore = GetNodeValue(structNode, "member[name='mt_text_more']/value") ?? string.Empty;
            post.Keywords = GetNodeValue(structNode, "member[name='mt_keywords']/value") ?? string.Empty;
            post.Permalink = GetNodeValue(structNode, "member[name='permalink']/value") ?? string.Empty;
            post.Status = GetNodeValue(structNode, "member[name='post_status']/value")
                ?? GetNodeValue(structNode, "member[name='page_status']/value")
                ?? string.Empty;
            post.Categories = ParseInlineCategories(structNode);

            string dateCreated = GetNodeValue(structNode, "member[name='dateCreated']/value");
            post.DateCreatedUtc = ParseIso8601Date(dateCreated);

            return post;
        }

        private static IReadOnlyList<string> ParseInlineCategories(XmlNode structNode)
        {
            var categories = new List<string>();
            XmlNodeList values = structNode.SelectNodes(
                "member[name='categories']/value/array/data/value");
            if (values != null)
            {
                foreach (XmlNode value in values)
                {
                    string name = value.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(name))
                        categories.Add(name);
                }
            }
            return categories;
        }

        /// <summary>
        /// Parses the XML-RPC <c>dateTime.iso8601</c> format (e.g. 20240310T14:22:31),
        /// tolerating the trailing-Z/offset variants some servers emit. Returns null when
        /// the value is missing or unparseable — a bad date must not fail the fetch.
        /// </summary>
        private static DateTime? ParseIso8601Date(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string[] formats =
            {
                "yyyyMMdd'T'HH:mm:ss",
                "yyyyMMdd'T'HH:mm:ss'Z'",
                "yyyyMMdd'T'HH:mm:sszzz"
            };
            if (DateTime.TryParseExact(value.Trim(), formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal |
                    System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out DateTime parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value.Trim(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal |
                    System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out parsed))
            {
                return parsed;
            }

            return null;
        }

        // -----------------------------------------------------------------------
        // Pages — wp.newPage / wp.editPage.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the WordPress page struct: title + description/mt_text_more. Unlike
        /// posts, pages carry no categories/keywords; the publish flag travels as the
        /// trailing method parameter rather than a struct member.
        /// </summary>
        public XmlRpcStruct GeneratePageStruct(BlogPost post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            var members = new List<XmlRpcMember>
            {
                new XmlRpcMember("title", new XmlRpcString(post.Title ?? string.Empty)),
                new XmlRpcMember("description", new XmlRpcString(post.MainContents)),
                new XmlRpcMember("mt_text_more", new XmlRpcString(post.ExtendedContents)),
            };

            if (post.DateCreatedUtc.HasValue)
                members.Add(new XmlRpcMember("dateCreated", new XmlRpcDateTime(post.DateCreatedUtc.Value)));

            return new XmlRpcStruct(members.ToArray());
        }

        /// <summary>Builds the full <c>wp.newPage</c> method-call XML (no network).</summary>
        public string BuildNewPageXml(string blogId, BlogPost post, bool publish)
        {
            return BuildMethodCallXml("wp.newPage",
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePageStruct(post),
                new XmlRpcBoolean(publish));
        }

        /// <summary>Builds the full <c>wp.editPage</c> method-call XML (no network).</summary>
        public string BuildEditPageXml(string blogId, BlogPost post, bool publish)
        {
            return BuildMethodCallXml("wp.editPage",
                new XmlRpcString(blogId),
                new XmlRpcString(post.Id),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePageStruct(post),
                new XmlRpcBoolean(publish));
        }

        /// <summary>
        /// Creates a page via <c>wp.newPage</c> and returns the server-assigned page id.
        /// </summary>
        public async Task<string> NewPageAsync(string blogId, BlogPost post, bool publish)
        {
            XmlRpcMethodResponse response = await CallMethodAsync("wp.newPage",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePageStruct(post),
                new XmlRpcBoolean(publish)).ConfigureAwait(false);

            string pageId = response.Response?.InnerText ?? string.Empty;
            post.Id = pageId;
            return pageId;
        }

        /// <summary>Edits an existing page via <c>wp.editPage</c>.</summary>
        public Task EditPageAsync(string blogId, BlogPost post, bool publish)
        {
            return CallMethodAsync("wp.editPage",
                CancellationToken.None,
                new XmlRpcString(blogId),
                new XmlRpcString(post.Id),
                new XmlRpcString(_username),
                new XmlRpcString(_password, true),
                GeneratePageStruct(post),
                new XmlRpcBoolean(publish));
        }

        private static string GetNodeValue(XmlNode node, string xpath)
        {
            XmlNode found = node.SelectSingleNode(xpath);
            // Prefer the typed child text so indentation whitespace between value/child
            // elements is not folded into the returned value; fall back to InnerText.
            XmlNode typed = found?.SelectSingleNode("string") ?? found?.FirstChild;
            string text = (typed != null && typed.NodeType != XmlNodeType.Text)
                ? typed.InnerText
                : found?.InnerText;
            return string.IsNullOrEmpty(text) ? text : text.Trim();
        }

        private async Task<XmlRpcMethodResponse> CallMethodAsync(
            string methodName, CancellationToken cancellationToken, params XmlRpcValue[] parameters)
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

            // Async end-to-end: the publish path must never block the caller's thread
            // (the UI thread in the shell) on the network round-trip.
            HttpResponseMessage response = await client.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw await BlogClientHttpException.CreateAsync(response, _endpointUrl)
                    .ConfigureAwait(false);

            string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

    /// <summary>
    /// Raised when the XML-RPC endpoint answers with a non-success HTTP status (e.g.
    /// 401/403 from a host-level auth rule, security plugin, or an application-password
    /// requirement). Carries the status code and a bounded snippet of the response body
    /// so the user can see *what* rejected the call (a Basic-auth realm, a WAF block
    /// page, a WordPress error, …) instead of a bare "401 (Unauthorized)".
    /// </summary>
    public class BlogClientHttpException : BlogClientPublishException
    {
        private const int MaxBodySnippet = 300;

        public BlogClientHttpException(int statusCode, string reason, string bodySnippet, string endpointUrl)
            : base(BuildMessage(statusCode, reason, bodySnippet, endpointUrl))
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }

        public static async Task<BlogClientHttpException> CreateAsync(
            HttpResponseMessage response, string endpointUrl)
        {
            string snippet = string.Empty;
            try
            {
                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                // Collapse whitespace so an HTML error page reads as one tidy line.
                snippet = System.Text.RegularExpressions.Regex.Replace(body ?? string.Empty, @"\s+", " ").Trim();
                if (snippet.Length > MaxBodySnippet)
                    snippet = snippet.Substring(0, MaxBodySnippet) + "\u2026";
            }
            catch
            {
                // A body that can't be read must never mask the status itself.
            }

            return new BlogClientHttpException(
                (int)response.StatusCode, response.ReasonPhrase, snippet, endpointUrl);
        }

        private static string BuildMessage(int statusCode, string reason, string bodySnippet, string endpointUrl)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("The blog server rejected the request (HTTP ").Append(statusCode);
            if (!string.IsNullOrEmpty(reason))
                sb.Append(' ').Append(reason);
            sb.Append(").");

            if (statusCode == 401 || statusCode == 403)
            {
                sb.Append(" The endpoint refused authentication. Check the username and password — ")
                  .Append("many WordPress hosts now require an *application password* for XML-RPC ")
                  .Append("instead of the account password — or whether a security plugin or host ")
                  .Append("rule is blocking XML-RPC.");
            }

            if (!string.IsNullOrEmpty(bodySnippet))
                sb.Append(" Server said: ").Append(bodySnippet);

            return sb.ToString();
        }
    }
}
