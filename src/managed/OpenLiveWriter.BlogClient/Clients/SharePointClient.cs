// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.FileDestinations;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.BlogClient.Clients
{

    /// <summary>
    /// Summary description for MetaweblogNTLMClient.
    /// </summary>
    [BlogClient("SharePoint", "MetaWeblog")]
    public class SharePointClient : MetaweblogClient
    {
        public SharePointClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
            _postApiUrl = postApiUrl;
        }

        protected override HttpRequestFilter CreateCredentialsFilter(string requestUri)
        {
            return new HttpRequestFilter(BeforeHttpRequest);
        }

        public override bool? DoesFileNeedUpload(IFileUploadContext uploadContext)
        {
            // This is a new post, we want to upload it
            if (String.IsNullOrEmpty(uploadContext.PostId))
            {
                return true;
            }

            // Check to see if we have uploaded this file to this blog with this post id
            string value = uploadContext.Settings.GetString(uploadContext.PostId, String.Empty);

            // We have uploaded this file to this blog with this post id so we should not upload it
            if (value == FILE_ALREADY_UPLOADED)
            {
                return null;
            }

            return true;
        }

        protected override void BeforeHttpRequest(HttpWebRequest request)
        {
            HttpCredentialsProvider credentialsProvider = GetHttpCredentialsProvider();
            if (credentialsProvider != null)
                credentialsProvider.ApplyCredentials(request);

            // WinLive 2734: The word "Mozilla" in our normal UserAgent was triggering ISA/TMG to think we are a web
            // browser and therefore sending us off to Html Forms Authentication when we really wanted to use Basic Auth.
            request.UserAgent = USER_AGENT;

            base.BeforeHttpRequest(request);
        }

        protected override void BlogPostReadFilter(BlogPost blogPost)
        {
            // fix busted permalinks (bug in initial implementation of SharePoint blogging)
            blogPost.Permalink = blogPost.Permalink.Replace("id=", "ID=");
        }

        internal ICredentials GetHttpCredentials()
        {
            return GetHttpCredentialsProvider().GetHttpCredentials();
        }

        protected override bool RequiresPassword
        {
            get { return false; }
        }

        public static bool VerifyCredentialsAndDetectAuthScheme(IBlogCredentials blogCredentials, SharePointClient client)
        {
            //Attempt to execute the GetUsersBlogs() operation using standard HTTP authentication
            //If the server challenges with an HTTP 401, but doesn't include with WWW-authentication
            //header, then the server is configured to use MetaWeblog for authentication, so we
            //re-issue the request using that authentication scheme instead.

            AuthenticationScheme authScheme = AuthenticationScheme.Http;
            AuthenticationScheme requiredAuthScheme = AuthenticationScheme.Unknown;

            while (requiredAuthScheme == AuthenticationScheme.Unknown || authScheme != requiredAuthScheme)
            {
                if (requiredAuthScheme != AuthenticationScheme.Unknown)
                    authScheme = requiredAuthScheme;

                blogCredentials.SetCustomValue(AUTH_SCHEME, authScheme.ToString());
                try
                {
                    TransientCredentials tc = new TransientCredentials(blogCredentials.Username, blogCredentials.Password, null);
                    client.Credentials.TransientCredentials = tc;
                    client.InitTransientCredential(tc);
                    client.GetUsersBlogs();
                    return true;
                }
                catch (WebException e)
                {
                    requiredAuthScheme = GetRequiredAuthScheme(e);
                }
                catch (BlogClientHttpErrorException e)
                {
                    requiredAuthScheme = GetRequiredAuthScheme(e.Exception);
                }
                catch (BlogClientAuthenticationException e)
                {
                    requiredAuthScheme = GetRequiredAuthScheme(e.WebException);
                }
                catch (Exception)
                {
                    throw;
                }
                Debug.Assert(requiredAuthScheme != AuthenticationScheme.Unknown, "Unexpected authscheme"); //this would cause an infinite loop!
            }
            return false;
        }

        private static AuthenticationScheme GetRequiredAuthScheme(WebException e)
        {
            if (e.Response != null && (e.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
            {
                if (e.Response.Headers["WWW-Authenticate"] != null)
                    return AuthenticationScheme.Http;
            }
            return AuthenticationScheme.MetaWeblog;
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            InitTransientCredential(tc);
            base.VerifyCredentials(tc);
        }

        private void InitTransientCredential(TransientCredentials tc)
        {
            HttpCredentialsProvider credentialsProvider = new HttpCredentialsProvider(UrlHelper.SafeToAbsoluteUri(_postApiUrl), Credentials, tc.Username, tc.Password);
            tc.Token = credentialsProvider;

            bool useMetaweblogCredentials = credentialsProvider.GetAuthenticationScheme() == AuthenticationScheme.MetaWeblog;
            string username = useMetaweblogCredentials ? Credentials.Username : "";
            string password = useMetaweblogCredentials ? Credentials.Password : "";
            tc.Username = username;
            tc.Password = password;
        }

        private HttpCredentialsProvider GetHttpCredentialsProvider()
        {
            TransientCredentials tc = Credentials.TransientCredentials as TransientCredentials;
            if (tc != null)
            {
                HttpCredentialsProvider credentialsProvider = tc.Token as HttpCredentialsProvider;
                return credentialsProvider;
            }
            return null;
        }

        protected override string CleanUploadFilename(string filename)
        {
            string path = base.CleanUploadFilename(filename);
            string pathWithoutExtension = Path.GetFileNameWithoutExtension(path);

            if (pathWithoutExtension.Contains("."))
            {
                path = pathWithoutExtension.Replace(".", "_") + "_" + GuidHelper.GetVeryShortGuid() + Path.GetExtension(path);
            }

            return path;
        }

        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {

            FileAttachSettings attachSettings = new FileAttachSettings(uploadContext.Settings);
            if (attachSettings.AttachmentFileName == null)
            {
                string fileName = uploadContext.FormatFileName(CleanUploadFilename(uploadContext.PreferredFileName));
                CalculateUploadVariables(fileName, attachSettings);
            }

            return attachSettings.AttachmentUrl;
        }

        //SharePoint blogIDs are formatted: webguid#listguid
        public static string SharepointBlogIdToListGuid(string blogId)
        {
            int listGuidIndex = blogId.IndexOf("#", StringComparison.OrdinalIgnoreCase);
            if (listGuidIndex == -1)
                return null;
            return blogId.Substring(listGuidIndex + 1);
        }

        public override void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
            FileAttachSettings attachSettings = new FileAttachSettings(uploadContext.Settings);
            if (attachSettings.AttachmentFileName == null)
            {
                CalculateUploadVariables(uploadContext.FormatFileName(uploadContext.PreferredFileName), attachSettings);
            }

            string listGuid = SharepointBlogIdToListGuid(uploadContext.BlogId);
            if (listGuid != null)
            {
                SharePointListsService listsServicesharePointLists = new SharePointListsService(attachSettings.UploadServiceUrl);
                listsServicesharePointLists.Credentials = GetHttpCredentials();

                //The AddAttachment() call will throw an error if the attachment already exists, so we need to delete
                //the attachment first (if it exists).  To delete the attachment, we must construct the attachment URL
                //that is typically generated internally by the server.
                //Sample URL: http://sharepoint/sites/writer/b2/blog/Lists/Posts/Attachments/13/Sunset_thumb1.jpg
                string attachDeleteUrl = String.Format(CultureInfo.InvariantCulture, "{0}{1}/Lists/Posts/Attachments/{2}/{3}", attachSettings.BaseUrl, attachSettings.BlogUrlPart, uploadContext.PostId, attachSettings.AttachmentFileName);
                try
                {
                    listsServicesharePointLists.DeleteAttachment(listGuid, uploadContext.PostId, attachDeleteUrl);
                }
                catch (Exception) { }

                //Add the attachment
                using (Stream fileContents = uploadContext.GetContents())
                    listsServicesharePointLists.AddAttachment(listGuid, uploadContext.PostId, attachSettings.AttachmentFileName, Convert.ToBase64String(StreamHelper.AsBytes(fileContents)));

                uploadContext.Settings.SetString(uploadContext.PostId, FILE_ALREADY_UPLOADED);

                return;
            }
            throw new BlogClientFileUploadNotSupportedException();
        }

        private void CalculateUploadVariables(string fileName, FileAttachSettings attachSettings)
        {
            fileName = FileHelper.GetValidAnsiFileName(fileName, fileName.Length);


            //calculate the ListService endpoint based on the API URL.  Note that the postAPI URL format
            //changed between M1 and M2, so we need try to calculate the API URL using both formats.
            //TODO: Once we ship final and the M1 timebomb expires, we should drop the M1 check.
            Match match = M1_postApiUrlRegex.Match(UrlHelper.SafeToAbsoluteUri(_postApiUrl));
            if (match.Success && match.Groups[1].Success && match.Groups[2].Success)
            {
                attachSettings.BaseUrl = match.Groups[1].Value;
                attachSettings.BlogUrlPart = match.Groups[2].Value;
            }
            else
            {
                match = M2_postApiUrlRegex.Match(UrlHelper.SafeToAbsoluteUri(_postApiUrl));
                if (match.Success && match.Groups[1].Success)
                {
                    attachSettings.BaseUrl = match.Groups[1].Value;
                    attachSettings.BlogUrlPart = String.Empty;
                }
            }

            int fileNameIndex = fileName.LastIndexOf('/');
            if (fileNameIndex != -1)
                attachSettings.AttachmentFileName = fileName.Substring(fileNameIndex + 1);
            else
                attachSettings.AttachmentFileName = fileName;

            attachSettings.AttachmentUrl = String.Format(CultureInfo.InvariantCulture, "{0}{1}/_attach/{2}", attachSettings.BaseUrl, attachSettings.BlogUrlPart, attachSettings.AttachmentFileName);

            string uploadUrl = String.Format(CultureInfo.InvariantCulture, "{0}{1}/_vti_bin/lists.asmx", attachSettings.BaseUrl, attachSettings.BlogUrlPart);
            attachSettings.UploadServiceUrl = uploadUrl;
            return;
        }

        private class FileAttachSettings
        {
            IProperties _settings;
            public FileAttachSettings(IProperties properties)
            {
                _settings = properties;
            }

            public string BaseUrl
            {
                get { return _settings.GetString(BASE_URL, null); }
                set { _settings.SetString(BASE_URL, value); }
            }
            private const string BASE_URL = "sharepoint.baseUrl";

            public string BlogUrlPart
            {
                get { return _settings.GetString(BLOG_URL_PART, null); }
                set { _settings.SetString(BLOG_URL_PART, value); }
            }
            private const string BLOG_URL_PART = "sharepoint.blogUrlPart";

            public string AttachmentFileName
            {
                get { return _settings.GetString(ATTACHMENT_FILE_NAME, null); }
                set { _settings.SetString(ATTACHMENT_FILE_NAME, value); }
            }
            private const string ATTACHMENT_FILE_NAME = "sharepoint.attachmentFileName";

            public string AttachmentUrl
            {
                get { return _settings.GetString(ATTACHMENT_URL, null); }
                set { _settings.SetString(ATTACHMENT_URL, value); }
            }
            private const string ATTACHMENT_URL = "sharepoint.attachmentUrl";

            public string UploadServiceUrl
            {
                get { return _settings.GetString(UPLOAD_SERVICE_URL, null); }
                set { _settings.SetString(UPLOAD_SERVICE_URL, value); }
            }
            private const string UPLOAD_SERVICE_URL = "sharepoint.uploadServiceUrl";
        }

        private const string FILE_ALREADY_UPLOADED = "sharepoint.uploaded";
        private Regex M1_postApiUrlRegex = new Regex(@"(.*)/_layouts/metaweblog\.aspx\?blog=(.*)");
        private Regex M2_postApiUrlRegex = new Regex(@"(.*)/_layouts/metaweblog\.aspx");
        private Uri _postApiUrl;
        internal static readonly string USER_AGENT = ApplicationEnvironment.FormatUserAgentString(ApplicationEnvironment.ProductName, false);

        public enum AuthenticationScheme { Unknown, Http, MetaWeblog };
        internal const string AUTH_SCHEME = "AuthScheme";
        class HttpCredentialsProvider
        {
            ICredentials _credentials = CredentialCache.DefaultCredentials;
            private IBlogCredentialsAccessor _blogCredentials;
            string _postApiUrl;
            public HttpCredentialsProvider(string postApiUrl, IBlogCredentialsAccessor blogCredentials, string username, string password)
            {
                _postApiUrl = postApiUrl;
                _blogCredentials = blogCredentials;

                string baseUrl = UrlHelper.GetBaseUrl(_postApiUrl);
                if ((username != null && username != String.Empty) || (password != null && password != String.Empty))
                {
                    _credentials = HttpRequestHelper.CreateHttpCredentials(username, password, baseUrl);
                }
            }

            public void ApplyCredentials(HttpWebRequest request)
            {
                //NTLM authentication requires keep-alive, so we must force the connection to support keep-alive.
                request.KeepAlive = true;
                request.ProtocolVersion = HttpVersion.Version11;
                request.Credentials = GetHttpCredentials();
            }

            public ICredentials GetHttpCredentials()
            {
                return _credentials;
            }

            internal AuthenticationScheme GetAuthenticationScheme()
            {
                string authVal = _blogCredentials.GetCustomValue(AUTH_SCHEME);
                AuthenticationScheme authSceme = AuthenticationScheme.Http;
                if (authVal != null && authVal.Length > 0)
                {
                    try
                    {
                        authSceme = (AuthenticationScheme)AuthenticationScheme.Parse(typeof(AuthenticationScheme), authVal);
                    }
                    catch (Exception) { }//unknown authscheme detected
                }
                return authSceme;
            }
        }
    }

    /// <summary>
    /// SOAP Wrapper for Sharepoint List webservice based on auto-generated VS Web Service Reference code.
    /// </summary>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "ListsSoap", Namespace = "http://schemas.microsoft.com/sharepoint/soap/")]
    public class SharePointListsService : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        /// <remarks/>
        public SharePointListsService(string url)
        {
            this.Url = url;
            this.UserAgent = SharePointClient.USER_AGENT;
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/AddAttachment", RequestNamespace = "http://schemas.microsoft.com/sharepoint/soap/", ResponseNamespace = "http://schemas.microsoft.com/sharepoint/soap/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string AddAttachment(string listName, string listItemID, string fileName, string attachment)
        {
            object[] results = this.Invoke("AddAttachment", new object[] {
                                                                             listName,
                                                                             listItemID,
                                                                             fileName,
                                                                             attachment});
            return ((string)(results[0]));
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/DeleteAttachment", RequestNamespace = "http://schemas.microsoft.com/sharepoint/soap/", ResponseNamespace = "http://schemas.microsoft.com/sharepoint/soap/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void DeleteAttachment(string listName, string listItemID, string url)
        {
            this.Invoke("DeleteAttachment", new object[] {
                                                             listName,
                                                             listItemID,
                                                             url});
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/GetAttachmentCollection", RequestNamespace = "http://schemas.microsoft.com/sharepoint/soap/", ResponseNamespace = "http://schemas.microsoft.com/sharepoint/soap/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode GetAttachmentCollection(string listName, string listItemID)
        {
            object[] results = this.Invoke("GetAttachmentCollection", new object[] {
                                                                                       listName,
                                                                                       listItemID});
            return ((System.Xml.XmlNode)(results[0]));
        }
    }
}
