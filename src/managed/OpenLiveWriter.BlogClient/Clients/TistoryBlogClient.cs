// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Net;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.HtmlParser.Parser;
using System.Web;
using OpenLiveWriter.HtmlParser.Parser.FormAgent;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("TistoryBlogClient", "OAuth2.0")]
    public class TistoryBlogClient : BloggerCompatibleClient
    {
        private TistoryCredential _tistoryCredential = null;
        private BlogPostCategory[] _tistoryCategory_list = null;
        private string postInput;
        private string _user_key;
        private string _redirect_url;
        private string blog_info_url = "https://www.tistory.com/apis/blog/info?access_token=";// b36360eb9775ea226eccf16b63faf5fb_5be9d7aa3d594f495e56849075cad5fb&url=manggsoft"
        private string auth_url = "https://www.tistory.com/oauth/authorize";//?client_id=abcdefghijklmnopqrstuvwxyz&redirect_uri=http://client.redirect.url&response_type=token";
        private string category_url = "https://www.tistory.com/apis/category/list?access_token=";
        private string recent_post_url = "https://www.tistory.com/apis/post/list?access_token=";
        private string write_post_url = "https://www.tistory.com/apis/post/write";
        private string delete_post_url = "https://www.tistory.com/apis/post/delete";
        private string edit_post_url = "https://www.tistory.com/apis/post/modify";
        private string get_post_url = "https://www.tistory.com/apis/post/read";
        private string upload_url = "https://www.tistory.com/apis/post/attach";

        public TistoryBlogClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
            postInput = postApiUrl.AbsoluteUri;
            _tistoryCredential = (TistoryCredential)credentials.TransientCredentials;
            //_user_key = postApiUrl;
        }

        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsFileUpload = true;
        }
        
        protected override TransientCredentials Login()
        {
            if(_tistoryCredential == null )
            {
                NameValueCollection parameter = HttpUtility.ParseQueryString(postInput);

                //postInput = postInput.Replace("https://www.tistory.com/oauth/authorize?client_id=", "");
                //int startIndex = postInput.IndexOf("&response_type");
                //postInput = postInput.Remove(startIndex);

                //string[] namesArray = postInput.Split(',');

                //_user_key = namesArray[0];
                //_redirect_url = namesArray[1];

                _user_key = parameter["https://www.tistory.com/oauth/authorize?client_id"];
                _redirect_url = parameter["redirect_uri"];
                _tistoryCredential = new TistoryCredential(Credentials.Username, Credentials.Password, _user_key, null);

                VerifyCredentials(_tistoryCredential);
            }

            return _tistoryCredential;
        }
        
        
        protected override void VerifyCredentials(TransientCredentials tc)
        {
            TistoryCredential tistoryCredential = tc as TistoryCredential;
            //InitTransientCredential(tc);
            //base.VerifyCredentials(tc);
            VerifyAndRefreshCredentials(tistoryCredential);
        }

        /*
        private HttpRequestFilter CreateAuthorizationFilter()
        {
            var transientCredentials = Login();
            var userCredential = (UserCredential)transientCredentials.Token;
            var accessToken = userCredential.Token.AccessToken;

            return (HttpWebRequest request) =>
            {
                // OAuth uses a Bearer token in the HTTP Authorization header.
                request.Headers.Add(
                    HttpRequestHeader.Authorization,
                    string.Format(CultureInfo.InvariantCulture, "Bearer {0}", accessToken));
            };
        }
        */


        private XmlDocument WebPost(string uri, FormData formData)
        {
            try
            {
                byte[] byteformParams = UTF8Encoding.UTF8.GetBytes(formData.ToString());

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.ContentLength = byteformParams.Length;
                request.UserAgent = "Mozilla / 4.0(compatible; MSIE 7.0; Windows NT 5.1)";
                request.CookieContainer = new CookieContainer();


                Stream stDataParams = request.GetRequestStream();
                stDataParams.Write(byteformParams, 0, byteformParams.Length);
                stDataParams.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return ParseXmlResponse(response);
            }
            catch(WebException web_ex)
            {
                Console.WriteLine(web_ex.Message);
            }

            return null;
        }

        private XmlDocument WebPostFileUpload(string uri, Stream filestream, string filename, string content_type, NameValueCollection parameter)
        {
            try
            {

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.KeepAlive = true;
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                
                request.UserAgent = "Mozilla / 4.0(compatible; MSIE 7.0; Windows NT 5.1)";
                request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                request.CookieContainer = new CookieContainer();

                // file multi part
                Stream memStream = new System.IO.MemoryStream();

                string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

                foreach (string key in parameter.Keys)
                {
                    string formitem = string.Format(formdataTemplate, key, parameter[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                }


                memStream.Write(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"uploadedfile\"; filename=\"{0}\"\r\n Content-Type: {1}\r\n\r\n";
                string header = string.Format(headerTemplate, filename, content_type);
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                memStream.Write(headerbytes, 0, headerbytes.Length);

                byte[] buffer = new byte[1024];

                int bytesRead = 0;

                while ((bytesRead = filestream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memStream.Write(buffer, 0, bytesRead);
                }

                memStream.Write(boundarybytes, 0, boundarybytes.Length);


                /*
                for (int i = 0; i < files.Length; i++)
                {

                    string header = string.Format(headerTemplate, "file" + i, files[i]);
                    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    memStream.Write(headerbytes, 0, headerbytes.Length);


                    FileStream fileStream = new FileStream(files[i], FileMode.Open,
                    FileAccess.Read);
                    byte[] buffer = new byte[1024];

                    int bytesRead = 0;

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }


                    memStream.Write(boundarybytes, 0, boundarybytes.Length);


                    fileStream.Close();
                }
                */
                request.ContentLength = memStream.Length;
                Stream requestStream = request.GetRequestStream();

                memStream.Position = 0;
                byte[] tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();
                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return ParseXmlResponse(response);
            }
            catch (WebException web_ex)
            {
                Console.WriteLine(web_ex.Message);
            }

            return null;
        }

        private bool LoginWithWeb(string url, string username, string password, string redirect_url, ref string access_key)
        {
            try
            {
                FormData formData = new FormData(false,
                    "loginId", username,
                    "password", password,
                    "redirectUrl", redirect_url);

                byte[] byteformParams = UTF8Encoding.UTF8.GetBytes(formData.ToString());

                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ////System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                //ServicePointManager.ServerCertificateValidationCallback = delegate
                //{
                //    return true;
                //};

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.tistory.com/auth/login");
                request.Method = "POST";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";//"application/x-www-form-urlencoded";
                request.ContentLength = byteformParams.Length;
                request.UserAgent = "Mozilla / 4.0(compatible; MSIE 7.0; Windows NT 5.1)";
                request.CookieContainer = new CookieContainer();


                Stream stDataParams = request.GetRequestStream();
                stDataParams.Write(byteformParams, 0, byteformParams.Length);
                stDataParams.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream stReadData = response.GetResponseStream();
                StreamReader srReadData = new StreamReader(stReadData, Encoding.UTF8);

                string strResult = srReadData.ReadToEnd();

                response.Close();


                string url_check = response.ResponseUri.ToString();

                if (url_check.Contains("#access_token="))
                {
                    int index = url_check.IndexOf("#access_token=");

                    access_key = url_check.Substring(index + 14);
                    index = access_key.IndexOf("&");
                    access_key = access_key.Substring(0, index);

                    return true;
                }
            }
            catch(WebException web_ex)
            {
                Console.WriteLine(web_ex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private bool VerifyCredentialsReturn(string username, string password, ref string access_key)
        {
            try
            {
                string access_token_url = String.Format("{0}?client_id={1}&redirect_uri={2}&response_type=token", auth_url, _user_key, _redirect_url);

                return LoginWithWeb(access_token_url, username, password, access_token_url, ref access_key);
            }
            catch (Exception e)
            {
                string s = e.Message;
            }

            return false;
        }

        private void VerifyAndRefreshCredentials(TistoryCredential tc)
        {
            string username = tc.Username;
            string password = tc.Password;

            bool promptForpassword = ((username == null || username == String.Empty) || (password == null || password == String.Empty));

            if (tc.Token != null)
            {
                if (tc.IsExpired(blog_info_url) == false)
                    return;
            }

            string access_token_url = String.Format("{0}?client_id={1}&redirect_uri={2}&response_type=token", auth_url, _user_key, _redirect_url);
            string access_key = "";

            if (VerifyCredentialsReturn(username, password, ref access_key) == false)
            { 
                if( promptForpassword )
                {
                    while (promptForpassword)
                    {

                        //if we're in silent mode where prompting isn't allowed, so throw the verification exception
                        if (BlogClientUIContext.SilentModeForCurrentThread)
                        {
                            throw new BlogClientAuthenticationException(String.Empty, String.Empty);
                        }

                        CredentialsPromptResult prompt = CredentialsHelper.PromptForCredentials(ref username, ref password, Credentials.Domain);
                        if (prompt == CredentialsPromptResult.Abort || prompt == CredentialsPromptResult.Cancel)
                            throw new BlogClientOperationCancelledException();

                        if (VerifyCredentialsReturn(username, password, ref access_key))
                            promptForpassword = false;
                    }
                }
                else
                    throw new BlogClientMethodUnsupportedException("Login exception");
            }
            tc.Username = username;
            tc.Password = password;
            if (tc.Token == null)
            {
                tc.Token = new TokenResponse();
            }
            TokenResponse tistory_token = (TokenResponse)tc.Token;
            tistory_token.AccessToken = access_key;
            tc.Token = tistory_token;
            Credentials.TransientCredentials = tc;
        }

        
        public override BlogInfo[] GetUsersBlogs()
        {
            TistoryCredential tc = (TistoryCredential)Login();

            return GetUsersBlogs(tc);
        }

        private BlogInfo[] GetUsersBlogs(TistoryCredential tc)
        {

            XmlRestRequestHelper Blog = new XmlRestRequestHelper();
            TokenResponse tistory_token =(TokenResponse) tc.Token;
            string blog_info_uri = String.Format("{0}{1}&url={2}", blog_info_url, tistory_token.AccessToken, tc.Username);
            Uri blog_info = new Uri(blog_info_uri);
            XmlDocument blog_xml = Blog.Get(ref blog_info, null);

            try
            {
                ArrayList blogs = new ArrayList();


                XmlNodeList blogNodes = blog_xml.SelectNodes("tistory/item/blogs");
                foreach (XmlNode blogNode in blogNodes)
                {
                    XmlNode idNode = blogNode.SelectSingleNode("blog/name");
                    XmlNode nameNode = blogNode.SelectSingleNode("blog/title");
                    XmlNode urlNode = blogNode.SelectSingleNode("blog/url");

                    blogs.Add(new BlogInfo(idNode.InnerText, nameNode.InnerText, urlNode.InnerText));
                }

                // return list of blogs
                return (BlogInfo[])blogs.ToArray(typeof(BlogInfo));
            }
            catch (Exception ex)
            {
                string response = blog_xml != null ? blog_xml.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing GetUsersBlogs response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException("GetUsersBlogs", ex.Message, response);
            }
            
        }

        public override BlogPostCategory[] GetCategories(string blogId)
        {
            if (Options.SupportsHierarchicalCategories)
            {
                return MetaweblogGetCategories(blogId);// WordPressGetCategories(blogId);
            }
            else if (Options.SupportsCategoriesInline)
            {
                return MetaweblogGetCategories(blogId);
            }
            else
            {
                return MetaweblogGetCategories(blogId);
            }
        }

        public override BlogPostKeyword[] GetKeywords(string blogId)
        {
            /*
            if (Options.SupportsGetKeywords)
            {
                return WordPressGetKeywords(blogId);
            }
            */
            return new BlogPostKeyword[] { };
        }

        private BlogPostKeyword[] WordPressGetKeywords(string blogId)
        {
            
            // call the method
            XmlNode result = CallMethod("wp.getTags",
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            ArrayList keywords = new ArrayList();
            try
            {
                XmlNodeList keywordNodes = result.SelectNodes("array/data/value/struct");
                foreach (XmlNode keywordNode in keywordNodes)
                {
                    string keywordTitle = NodeText(keywordNode.SelectSingleNode("member[name='name']/value"));
                    if (string.IsNullOrEmpty(keywordTitle))
                        keywordTitle = NodeText(keywordNode.SelectSingleNode("member[name='title']/value"));

                    // fix bug 722918: Server Side Tags: Tag with ampersand in retrieved post shows as &amp;
                    keywordTitle = HtmlUtils.UnEscapeEntities(keywordTitle, HtmlUtils.UnEscapeMode.Default);

                    keywords.Add(new BlogPostKeyword(keywordTitle));
                }
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing GetKeywords response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException("wp.getKeywords", ex.Message, response);
            }

            // return list of categories
            return (BlogPostKeyword[])keywords.ToArray(typeof(BlogPostKeyword));
            
        }

        private BlogPostCategory[] MetaweblogGetCategories(string blogId)
        {
            BlogPostCategory[] result_category = null;
            XmlRestRequestHelper Blog = new XmlRestRequestHelper();
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            string categori_info_uri = String.Format("{0}{1}&blogName={2}", category_url, tistory_token.AccessToken, blogId);
            Uri blog_info = new Uri(categori_info_uri);
            XmlDocument blog_xml = Blog.Get(ref blog_info, null);

            //// call the method
            //XmlNode result = CallMethod("metaWeblog.getCategories",
            //    new XmlRpcString(blogId),
            //    new XmlRpcString(Username),
            //    new XmlRpcString(Password, true));

            //category_url = "";

            result_category = ParseCategories(blog_xml, "OAuth.getCategories");
            if (_tistoryCategory_list == null &&
                result_category != null)
            {
                _tistoryCategory_list = new BlogPostCategory[result_category.Length];
                Array.Copy(result_category, _tistoryCategory_list, result_category.Length);
            }

            return result_category;
        }

        private BlogPostCategory[] EnhancedGetCategories(string methodName, string blogId, bool allowParent)
        {
            
            // call the method
            XmlNode result = CallMethod(methodName,
                new XmlRpcString(blogId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            // parse the results
            ArrayList categories = new ArrayList();
            try
            {
                XmlNodeList categoryNodes = result.SelectNodes("array/data/value/struct");
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    // parse out the category name
                    string categoryId = NodeText(categoryNode.SelectSingleNode("member[name='categoryId']/value"));
                    string categoryTitle = NodeText(categoryNode.SelectSingleNode("member[name='categoryName']/value"));

                    if (allowParent)
                    {
                        string categoryParent = NodeText(categoryNode.SelectSingleNode("member[name='parentId']/value"));
                        categories.Add(new BlogPostCategory(categoryId, categoryTitle, categoryParent));
                    }
                    else
                    {
                        categories.Add(new BlogPostCategory(categoryId, categoryTitle));
                    }
                }
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing GetCategories response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, response);
            }

            
            // return list of categories
            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
            
        }

        public virtual BlogPostCategory ParseCategory(XmlNode categoryNode)
        {
            // parse out the category name
            //note: spec calls for description, htmlURL, rssURL only
            //but tested metaweblog implementers seem to all have title, and at least MSDN blogs has category id
            //thus, the twisted logic here
            String catID, catName;
            catID = catName = GetNodeValue(categoryNode, "name");// member[name='name']/value");
            String categoryTitle = GetNodeValue(categoryNode, "name");// member[name='name']/value");
            if (categoryTitle != String.Empty && categoryTitle != null)
            {
                catID = catName = categoryTitle;
            }

            // look for various permutations of category-id
            string categoryId = GetNodeValue(categoryNode, "id");
            if (categoryId == null)
                categoryId = GetNodeValue(categoryNode, "id");

            // set if we got it
            if (categoryId != String.Empty && categoryId != null)
            {
                catID = categoryId;
            }

            // Some weblog providers (such as Drupal) actually return the mt category struct for
            // metaWeblog.getCategories. In this case the parsing above would have failed to
            // extract either a name or an id. Parse out the values here if necesssary

            // populate the name field if we haven't gotten it another way
            if (catName == null)
            {
                catName = GetNodeValue(categoryNode, "name");

                // our failsafe for category-id is always category name
                if (catID == null)
                    catID = catName;
            }

            // optional parent specifier
            string parent = GetNodeValue(categoryNode, "parent");// member[name='parent']/value");
            if (parent == null)
                parent = String.Empty;

            // validate (a null category name downstream will result in unexpected error
            // dialogs -- better to show the user an error here indicating that the
            // response was malformed)
            if (catName == null)
                throw new ArgumentException("Category Name Not Specified");

            // add the category
            return new BlogPostCategory(catID, catName, parent);
        }

        private string GetNodeValue(XmlNode xmlNode, string xpath)
        {
            XmlNode nodeValue = xmlNode.SelectSingleNode(xpath);
            if (nodeValue != null)
                return nodeValue.InnerText;
            return null;
        }

        public override BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            XmlRestRequestHelper Blog = new XmlRestRequestHelper();
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            string recent_post_uri = String.Format("{0}{1}&blogName={2}&count={3}&sort=date", recent_post_url, tistory_token.AccessToken, blogId, maxPosts);
            Uri blog_info = new Uri(recent_post_uri);
            XmlDocument blog_xml = Blog.Get(ref blog_info, null);
            //Blog.Post()

            // parse results
            return ParseRecentPosts(blog_xml, "metaWeblog.getRecentPosts", includeCategories, now);
        }

        public override string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            //bool addCategoriesOutOfBand = AddCategoriesIfNecessary(blogId, post, newCategoryContext);

            string result;
            if (Options.SupportsCategoriesInline)
            {
                result = MetaweblogNewPost(blogId, post, publish);
                if (Options.SupportsHierarchicalCategories)
                {
                    MetaweblogNewPost(blogId, post, publish);
                    //MovableTypeSetPostCategories(result, ArrayHelper.Concat(post.Categories, post.NewCategories));
                }
            }
            else
            {
                result = MetaweblogNewPost(blogId, post, publish);
            }

            // if we succeeded then note addition of categories if appropriate
            //if (!addCategoriesOutOfBand)
            //{
            //    foreach (BlogPostCategory category in post.NewCategories)
            //        newCategoryContext.NewCategoryAdded(category);
            //}

            return result;
        }

        private string MetaweblogNewPost(string blogId, BlogPost post, bool publish)
        {
            XmlRestRequestHelper Blog = new XmlRestRequestHelper();
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            FormData paramemter = new FormData();
            paramemter.Add("access_token", tistory_token.AccessToken);
            paramemter.Add("blogName", blogId);
            paramemter.Add("title", post.Title);
            if(publish )
                paramemter.Add("visibility", "2");
            else
                paramemter.Add("visibility", "0");
            if ( post.Categories.Length > 0 )
            paramemter.Add("category", post.Categories[0].Id);
            paramemter.Add("content", post.Contents);
            //paramemter.Add("category", post.Categories[0].Id);


            XmlDocument NewPostXml = WebPost(write_post_url, paramemter);
            //string recent_post_uri = String.Format("{0}{1}&blogName={2}&title={3}", write_post_url, tistory_token.AccessToken, blogId, maxPosts);


            XmlNode postId = NewPostXml.SelectSingleNode("tistory/postId");
            // return the blog id
            return postId.InnerText;
        }

        private string MovableTypeNewPost(string blogId, BlogPost post, bool publish)
        {
            // See http://www.jayallen.org/journey/2003/10/tip_xmlrpc_and_movable_type
            // for explanation of MT's weird publish flag behavior and why it's
            // a good idea to do the song and dance below.

            // create the post (don't publish it yet because
            // we need to add the categories)
            string postId = MetaweblogNewPost(blogId, post, false);

            // add the categories to the post
            //MovableTypeSetPostCategories(postId, post.Categories);

            if (publish)
            {
                // call the metaweblog edit post for publishing
                post.Id = postId;
                MetaweblogEditPost(blogId, post, publish);
            }

            // return the post-id
            return postId;
        }

        public override void DeletePost(string blogId, string postId, bool publish)
        {
            XmlRestRequestHelper Blog = new XmlRestRequestHelper();
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            FormData paramemter = new FormData();
            paramemter.Add("access_token", tistory_token.AccessToken);
            paramemter.Add("blogName", blogId);
            paramemter.Add("postId", postId);
            
            XmlDocument DeleteXML = WebPost(delete_post_url, paramemter);


            XmlNode statusID = DeleteXML.SelectSingleNode("tistory/status");
            
            return;
        }

        private void MovableTypeSetPostCategories(string postId, BlogPostCategory[] categories)
        {
            // call method
            XmlNode result = CallMethod("mt.setPostCategories",
                new XmlRpcString(postId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true),
                GetCategoriesArray(categories));

            // confirm we had a successful return value
            Trace.Assert(result.InnerText == "1", "Unexpected error return value from mt.setPostCategories");
        }

        private XmlRpcArray GetCategoriesArray(BlogPostCategory[] categories)
        {
            ArrayList categoryValues = new ArrayList();
            foreach (BlogPostCategory category in categories)
            {
                // create the struct for the category
                XmlRpcStruct categoryStruct = new XmlRpcStruct(
                    new XmlRpcMember[] { new XmlRpcMember("categoryId", category.Id) });

                // add it to the list
                categoryValues.Add(categoryStruct);
            }

            // return the array
            return new XmlRpcArray((XmlRpcValue[])categoryValues.ToArray(typeof(XmlRpcValue)));
        }

        /// <summary>
        /// Edit an existing entry
        /// </summary>
        /// <param name="blog">blog</param>
        /// <param name="postId">post id</param>
        /// <param name="account">account to post to</param>
        /// <param name="entry">entry to post</param>
        /// <param name="publish">publish now?</param>
        /// <returns>was the entry successfully edited</returns>
        public override bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish)
        {
            if (!publish && !Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            //bool addCategoriesOutOfBand = AddCategoriesIfNecessary(blogId, post, newCategoryContext);

            bool result;
            if (Options.SupportsCategoriesInline)
            {
                result = MetaweblogEditPost(blogId, post, publish);
                //if (Options.SupportsHierarchicalCategories)
                //    MovableTypeSetPostCategories(post.Id, ArrayHelper.Concat(post.Categories, post.NewCategories));
            }
            else
            {
                result = MetaweblogEditPost(blogId, post, publish);
                //result = MovableTypeEditPost(blogId, post, publish);
            }

            //// if we succeeded then note addition of categories if appropriate
            //if (!addCategoriesOutOfBand)
            //{
            //    foreach (BlogPostCategory category in post.NewCategories)
            //        newCategoryContext.NewCategoryAdded(category);
            //}

            return result;
        }

        private bool MetaweblogEditPost(string blogId, BlogPost post, bool publish)
        {
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            FormData paramemter = new FormData();
            paramemter.Add("access_token", tistory_token.AccessToken);
            paramemter.Add("blogName", blogId);
            paramemter.Add("title", post.Title);
            paramemter.Add("postId", post.Id);
            if (post.Categories.Length > 0)
                paramemter.Add("category", post.Categories[0].Id);
            paramemter.Add("content", post.Contents);

            XmlDocument EditXml = WebPost(edit_post_url, paramemter);


            XmlNode result = EditXml.SelectSingleNode("tistory/status");

            return (result.InnerText == "200");
        }

        private bool MovableTypeEditPost(string blogId, BlogPost post, bool publish)
        {
            // add the categories to the post
            MovableTypeSetPostCategories(post.Id, post.Categories);

            // call the base to do the rest of the edit
            return MetaweblogEditPost(blogId, post, publish);
        }

        public override BlogPost GetPost(string blogId, string postId)
        {
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            FormData paramemter = new FormData();
            paramemter.Add("access_token", tistory_token.AccessToken);
            paramemter.Add("blogName", blogId);
            paramemter.Add("postId", postId);

            XmlDocument result = WebPost(get_post_url, paramemter);

            XmlNode postStruct = result.SelectSingleNode("tistory/item");
            if (postStruct != null)
            {
                return ParseBlogPost(postStruct, true);
            }
            else
            {
                throw new BlogClientInvalidServerResponseException("OAuth.GetPost", "No post struct returned from server", result.OuterXml);
            }
        }

        public override BlogPost GetPage(string blogId, string pageId)
        {
            if (Options.SupportsPages)
            {
                // call method
                XmlNode result = CallMethod("wp.getPage",
                    new XmlRpcString(blogId),
                    new XmlRpcString(pageId),
                    new XmlRpcString(Username),
                    new XmlRpcString(Password, true));

                // try get return the post struct
                XmlNode postStruct = result.SelectSingleNode("struct");
                if (postStruct != null)
                {
                    // parse page info (pages don't include categories by design)
                    return ParseBlogPost(postStruct, false);
                }
                else
                {
                    throw new BlogClientInvalidServerResponseException("wp.getPage", "No post struct returned from server", result.OuterXml);
                }
            }
            else
            {
                throw new BlogClientMethodUnsupportedException("GetPage");
            }
        }

        public override BlogPost[] GetPages(string blogId, int maxPages)
        {
            throw new BlogClientMethodUnsupportedException("GetPages");
        }

        public override PageInfo[] GetPageList(string blogId)
        {
            throw new BlogClientMethodUnsupportedException("GetPageList");
        }

        protected override string NewPage(string blogId, BlogPost page, bool publish)
        {
            throw new BlogClientMethodUnsupportedException("NewPage");
        }

        protected override bool EditPage(string blogId, BlogPost page, bool publish)
        {
            throw new BlogClientMethodUnsupportedException("EditPage");
        }

        public override void DeletePage(string blogId, string pageId)
        {
            throw new BlogClientMethodUnsupportedException("DeletePage");
        }

        public override AuthorInfo[] GetAuthors(string blogId)
        {
            throw new BlogClientMethodUnsupportedException("GetAuthors");
        }

        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            string uploadFileName = uploadContext.FormatFileName(CleanUploadFilename(uploadContext.PreferredFileName));
            TokenResponse tistory_token = (TokenResponse)_tistoryCredential.Token;
            FormData paramemter = new FormData();
            paramemter.Add("access_token", tistory_token.AccessToken);
            paramemter.Add("blogName", uploadContext.BlogId);

            XmlDocument result;

            using (Stream fileContents = uploadContext.GetContents())
            {
                NameValueCollection parameters = new NameValueCollection();
                parameters.Add("access_token", tistory_token.AccessToken);
                parameters.Add("blogName", uploadContext.BlogId);

                result = WebPostFileUpload(upload_url, fileContents, uploadFileName,
                    MimeHelper.GetContentType(Path.GetExtension(uploadContext.PreferredFileName), MimeHelper.APP_OCTET_STREAM), parameters);

            }
            /*

            // call the method
            XmlNode result;
            using (Stream fileContents = uploadContext.GetContents())
            {
                upload_url = "";
                result = CallMethod("metaWeblog.newMediaObject",
                                             new XmlRpcString(uploadContext.BlogId),
                                             new XmlRpcString(Username),
                                             new XmlRpcString(Password, true),
                                             new XmlRpcStruct(new XmlRpcMember[]
                                                                   {
                                                                       new XmlRpcMember( "name", uploadFileName ),
                                                                       new XmlRpcMember( "type", MimeHelper.GetContentType(Path.GetExtension(uploadContext.PreferredFileName),MimeHelper.APP_OCTET_STREAM )),
                                                                       new XmlRpcMember( "bits", new XmlRpcBase64( StreamHelper.AsBytes(fileContents) ) ),
                                                                   }
                                                ));
            }
            */
            // return the url to the file on the server
            XmlNode urlNode = result.SelectSingleNode("tistory/url");
            if (urlNode != null)
            {
                return urlNode.InnerText;
            }
            else
            {
                throw new BlogClientInvalidServerResponseException("OAuth.newMediaObject", "No URL returned from server", result.OuterXml);
            }
        }

        protected virtual string CleanUploadFilename(string filename)
        {
            string cleanFilename = filename.Replace("#", "_"); //avoids bug 494107
            return cleanFilename;
        }

        public override string AddCategory(string blogId, BlogPostCategory category)
        {
            throw new BlogClientMethodUnsupportedException("AddCategory");
        }

        private static int ParseCategoryParent(string parent)
        {
            int val = 0;
            if (!string.IsNullOrEmpty(parent))
            {
                if (!int.TryParse(parent, out val))
                {
                    Trace.Fail("Unexpected non-integer category parent ID: " + parent);
                }
            }
            return val;
        }

        public override BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName)
        {
            throw new BlogClientMethodUnsupportedException("SuggestCategories");
        }

        private BlogPostCategory[] ParseCategories(XmlNode result, string methodName)
        {
            // parse the results
            ArrayList categories = new ArrayList();
            try
            {
                XmlNodeList categoryNodes = result.SelectNodes("tistory/item/categories/category");
                foreach (XmlNode categoryNode in categoryNodes)
                    categories.Add(ParseCategory(categoryNode));
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing " + methodName + " response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, response);
            }

            // return list of categories
            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
        }

        protected override BlogClientProviderException ExceptionForFault(string faultCode, string faultString)
        {
            if (faultCode.IndexOf("403", StringComparison.OrdinalIgnoreCase) != -1)
                return new BlogClientAuthenticationException(faultCode, faultString);
            else if (faultCode.IndexOf("3001", StringComparison.OrdinalIgnoreCase) != -1)
                return new BlogClientAccessDeniedException(faultCode, faultString);
            else
                return null;
        }

        private BlogPost[] ParseRecentPosts(XmlNode result, string methodName, bool includeCategories, DateTime? now)
        {
            // parse results
            ArrayList posts = new ArrayList();
            try
            {
                XmlNodeList postNodes = result.SelectNodes("tistory/item/posts/post");
                foreach (XmlNode postNode in postNodes)
                {
                    BlogPost blogPost = ParseRecentBlogPost(postNode, includeCategories);

                    // If FuturePublishDateWarning is set, then there may be already published (e.g. live) posts that
                    // have a future date, so we will include those as well.
                    if (!now.HasValue || Options.FuturePublishDateWarning || blogPost.DatePublished.CompareTo(now.Value) < 0)
                        posts.Add(blogPost);
                }

            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing " + methodName + " response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException(methodName, ex.Message, response);
            }

            // return list of posts
            return (BlogPost[])posts.ToArray(typeof(BlogPost));
        }

        private BlogPost ParseRecentBlogPost(XmlNode postNode, bool includeCategories)
        {
            // create blog post
            BlogPost blogPost = new BlogPost();

            // get node values
            blogPost.Id = NodeText(postNode.SelectSingleNode("id"));// member[name='id']/value"));
            if (blogPost.Id == String.Empty)
                blogPost.Id = NodeText(postNode.SelectSingleNode("id"));// "member[name='id']/value"));

            SetPostTitleFromXmlValue(blogPost, NodeText(postNode.SelectSingleNode("title")));// member[name='title']/value")));

            // date published
            XmlNode dateCreatedNode = postNode.SelectSingleNode("date");// member[name='date']/value");
            if (dateCreatedNode == null)
                dateCreatedNode = postNode.SelectSingleNode("date");// member[name='date']/value");
            string date = NodeText(dateCreatedNode);
            blogPost.DatePublished = DateTime.Parse(date);//ParseBlogDate(dateCreatedNode);

            // extract comment field
            if (Options.SupportsCommentPolicy)
            {
                string commentPolicy = NodeText(postNode.SelectSingleNode("comments"));// member[name='comments']/value"));
                if (commentPolicy != String.Empty)
                {
                    switch (commentPolicy)
                    {
                        case "0":
                            blogPost.CommentPolicy = BlogCommentPolicy.None;
                            break;
                        case "1":
                            blogPost.CommentPolicy = BlogCommentPolicy.Open;
                            break;
                        case "2":
                            blogPost.CommentPolicy = BlogCommentPolicy.Closed;
                            break;
                        default:
                            Trace.Fail("Unexpected value for mt_allow_comments: " + commentPolicy);
                            break;
                    }
                }
            }

            // extract trackback field
            if (Options.SupportsPingPolicy)
            {
                string allowTrackbacks = NodeText(postNode.SelectSingleNode("trackbacks"));// member[name='trackbacks']/value"));
                if (allowTrackbacks != String.Empty)
                    blogPost.TrackbackPolicy = (allowTrackbacks == "1") ? BlogTrackbackPolicy.Allow : BlogTrackbackPolicy.Deny;
            }

            /*
            // extract keywords field
            if (Options.SupportsKeywords)
            {
                string keywords = NodeText(postNode.SelectSingleNode("member[name='mt_keywords']/value"));
                blogPost.Keywords = HtmlUtils.UnEscapeEntities(keywords, HtmlUtils.UnEscapeMode.Default);
            }
            */

            /*
            // extract excerpt field
            if (Options.SupportsExcerpt)
            {
                blogPost.Excerpt = NodeText(postNode.SelectSingleNode("member[name='mt_excerpt']/value"));
            }
            */

            /*
            // extract ping url array
            if (Options.SupportsTrackbacks)
            {

                XmlNode pingUrlsNode = postNode.SelectSingleNode("member[name='postUrl']/value");
                if (pingUrlsNode != null)
                {
                    ArrayList sentPingUrls = new ArrayList();
                    if (Options.TrackbackDelimiter == TrackbackDelimiter.ArrayElement)
                    {
                        XmlNodeList pingUrlNodes = pingUrlsNode.SelectNodes("array/data/value");
                        foreach (XmlNode node in pingUrlNodes)
                            sentPingUrls.Add(node.InnerText.Trim());
                    }
                    else
                    {
                        string delimiter = null;
                        if (Options.TrackbackDelimiter == TrackbackDelimiter.Space)
                            delimiter = " ";
                        else if (Options.TrackbackDelimiter == TrackbackDelimiter.Comma)
                            delimiter = ",";
                        sentPingUrls.AddRange(StringHelper.Split(pingUrlsNode.InnerText, delimiter));
                    }

                    blogPost.PingUrlsSent = sentPingUrls.ToArray(typeof(string)) as string[];
                }
            }

            // extract slug
            if (Options.SupportsSlug)
            {
                blogPost.Slug = NodeText(postNode.SelectSingleNode("member[name='wp_slug']/value"));
                if (blogPost.Slug == String.Empty)
                    blogPost.Slug = NodeText(postNode.SelectSingleNode("member[name='mt_basename']/value"));
            }

            // extract password
            if (Options.SupportsPassword)
            {
                blogPost.Password = NodeText(postNode.SelectSingleNode("member[name='wp_password']/value"));
            }

            // extract author
            if (Options.SupportsAuthor)
            {
                string authorId = NodeText(postNode.SelectSingleNode("member[name='wp_author_id']/value"));

                // account for different display name syntax in various calls
                string authorName = NodeText(postNode.SelectSingleNode("member[name='wp_author_display_name']/value"));

                blogPost.Author = new PostIdAndNameField(authorId, authorName);
            }

            // extract page parent
            if (Options.SupportsPageParent)
            {
                string pageParentId = NodeText(postNode.SelectSingleNode("member[name='wp_page_parent_id']/value"));
                // convert 0 to empty string for parent-id
                if (pageParentId == "0")
                    pageParentId = String.Empty;

                string pageParentTitle = NodeText(postNode.SelectSingleNode("member[name='wp_page_parent_title']/value"));
                blogPost.PageParent = new PostIdAndNameField(pageParentId, pageParentTitle);
            }

            // extract page order
            if (Options.SupportsPageOrder)
            {
                blogPost.PageOrder = NodeText(postNode.SelectSingleNode("member[name='wp_page_order']/value"));
            }
            */

            
            // extract categories
            if (includeCategories && Options.SupportsCategories)
            {
                if (Options.SupportsCategoriesInline && !Options.SupportsHierarchicalCategories)
                {
                    blogPost.Categories = MetaweblogExtractCategories(blogPost.Id, postNode);
                }
                else
                {
                    blogPost.Categories = MetaweblogExtractCategories(blogPost.Id, postNode);
                }
            }
            

            // allow post-processing of extracted blog post
            BlogPostReadFilter(blogPost);

            // return the post
            return blogPost;
        }

        private BlogPost ParseBlogPost(XmlNode postNode, bool includeCategories)
        {
            // create blog post
            BlogPost blogPost = new BlogPost();

            // get node values
            blogPost.Id = NodeText(postNode.SelectSingleNode("id"));// member[name='id']/value"));
            if (blogPost.Id == String.Empty)
                blogPost.Id = NodeText(postNode.SelectSingleNode("id"));// "member[name='id']/value"));

            SetPostTitleFromXmlValue(blogPost, NodeText(postNode.SelectSingleNode("title")));// member[name='title']/value")));

            /*
            // attempt to discover the permalink
            blogPost.Permalink = NodeText(postNode.SelectSingleNode("member[name='permaLink']/value"));
            if (blogPost.Permalink == String.Empty)
                blogPost.Permalink = NodeText(postNode.SelectSingleNode("member[name='permalink']/value"));
            if (blogPost.Permalink == String.Empty)
                blogPost.Permalink = NodeText(postNode.SelectSingleNode("member[name='link']/value"));
            */

            // contents and extended contents
            string mainContents = NodeText(postNode.SelectSingleNode("content"));// member[name='description']/value"));
            //string extendedContents = NodeText(postNode.SelectSingleNode("member[name='mt_text_more']/value"));
            blogPost.SetContents(mainContents, "");
            

            // date published
            XmlNode dateCreatedNode = postNode.SelectSingleNode("date");// member[name='date']/value");
            if (dateCreatedNode == null)
                dateCreatedNode = postNode.SelectSingleNode("date");// member[name='date']/value");
            string date = NodeText(dateCreatedNode);
            blogPost.DatePublished = DateTime.Parse(date);//ParseBlogDate(dateCreatedNode);

            // extract comment field
            if (Options.SupportsCommentPolicy)
            {
                string commentPolicy = NodeText(postNode.SelectSingleNode("comments"));// member[name='comments']/value"));
                if (commentPolicy != String.Empty)
                {
                    switch (commentPolicy)
                    {
                        case "0":
                            blogPost.CommentPolicy = BlogCommentPolicy.None;
                            break;
                        case "1":
                            blogPost.CommentPolicy = BlogCommentPolicy.Open;
                            break;
                        case "2":
                            blogPost.CommentPolicy = BlogCommentPolicy.Closed;
                            break;
                        default:
                            Trace.Fail("Unexpected value for mt_allow_comments: " + commentPolicy);
                            break;
                    }
                }
            }

            // extract trackback field
            if (Options.SupportsPingPolicy)
            {
                string allowTrackbacks = NodeText(postNode.SelectSingleNode("trackbacks"));// member[name='trackbacks']/value"));
                if (allowTrackbacks != String.Empty)
                    blogPost.TrackbackPolicy = (allowTrackbacks == "1") ? BlogTrackbackPolicy.Allow : BlogTrackbackPolicy.Deny;
            }

            /*
            // extract keywords field
            if (Options.SupportsKeywords)
            {
                string keywords = NodeText(postNode.SelectSingleNode("member[name='mt_keywords']/value"));
                blogPost.Keywords = HtmlUtils.UnEscapeEntities(keywords, HtmlUtils.UnEscapeMode.Default);
            }
            */

            /*
            // extract excerpt field
            if (Options.SupportsExcerpt)
            {
                blogPost.Excerpt = NodeText(postNode.SelectSingleNode("member[name='mt_excerpt']/value"));
            }
            */

            /*
            // extract ping url array
            if (Options.SupportsTrackbacks)
            {

                XmlNode pingUrlsNode = postNode.SelectSingleNode("member[name='postUrl']/value");
                if (pingUrlsNode != null)
                {
                    ArrayList sentPingUrls = new ArrayList();
                    if (Options.TrackbackDelimiter == TrackbackDelimiter.ArrayElement)
                    {
                        XmlNodeList pingUrlNodes = pingUrlsNode.SelectNodes("array/data/value");
                        foreach (XmlNode node in pingUrlNodes)
                            sentPingUrls.Add(node.InnerText.Trim());
                    }
                    else
                    {
                        string delimiter = null;
                        if (Options.TrackbackDelimiter == TrackbackDelimiter.Space)
                            delimiter = " ";
                        else if (Options.TrackbackDelimiter == TrackbackDelimiter.Comma)
                            delimiter = ",";
                        sentPingUrls.AddRange(StringHelper.Split(pingUrlsNode.InnerText, delimiter));
                    }

                    blogPost.PingUrlsSent = sentPingUrls.ToArray(typeof(string)) as string[];
                }
            }

            // extract slug
            if (Options.SupportsSlug)
            {
                blogPost.Slug = NodeText(postNode.SelectSingleNode("member[name='wp_slug']/value"));
                if (blogPost.Slug == String.Empty)
                    blogPost.Slug = NodeText(postNode.SelectSingleNode("member[name='mt_basename']/value"));
            }

            // extract password
            if (Options.SupportsPassword)
            {
                blogPost.Password = NodeText(postNode.SelectSingleNode("member[name='wp_password']/value"));
            }

            // extract author
            if (Options.SupportsAuthor)
            {
                string authorId = NodeText(postNode.SelectSingleNode("member[name='wp_author_id']/value"));

                // account for different display name syntax in various calls
                string authorName = NodeText(postNode.SelectSingleNode("member[name='wp_author_display_name']/value"));

                blogPost.Author = new PostIdAndNameField(authorId, authorName);
            }

            // extract page parent
            if (Options.SupportsPageParent)
            {
                string pageParentId = NodeText(postNode.SelectSingleNode("member[name='wp_page_parent_id']/value"));
                // convert 0 to empty string for parent-id
                if (pageParentId == "0")
                    pageParentId = String.Empty;

                string pageParentTitle = NodeText(postNode.SelectSingleNode("member[name='wp_page_parent_title']/value"));
                blogPost.PageParent = new PostIdAndNameField(pageParentId, pageParentTitle);
            }

            // extract page order
            if (Options.SupportsPageOrder)
            {
                blogPost.PageOrder = NodeText(postNode.SelectSingleNode("member[name='wp_page_order']/value"));
            }
            */

            
            // extract categories
            if (includeCategories && Options.SupportsCategories)
            {
                if (Options.SupportsCategoriesInline && !Options.SupportsHierarchicalCategories)
                {
                    blogPost.Categories = MetaweblogExtractCategories(blogPost.Id, postNode);
                }
                else
                {
                    blogPost.Categories = MovableTypeExtractCategories(blogPost.Id, postNode);
                }
            }
            
            // allow post-processing of extracted blog post
            BlogPostReadFilter(blogPost);

            // return the post
            return blogPost;
        }

        private BlogPostCategory[] MetaweblogExtractCategories(string postId, XmlNode postNode)
        {
            // extract categories from the post
            ArrayList categories = new ArrayList();
            XmlNode categoriesNode = postNode.SelectSingleNode("categoryId");
            if (categoriesNode != null)
            {
                if( _tistoryCategory_list != null )
                {
                    foreach( BlogPostCategory category in _tistoryCategory_list)
                    {
                        if( category.Id == categoriesNode.InnerText )
                        {
                            categories.Add(
                                new BlogPostCategory(category.Id, category.Name));
                            break;
                        }
                    }
                }
            }

            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
        }

        /*
        private BlogPostCategory[] MetaweblogExtractCategories(string postId, XmlNode postNode)
        {
            // extract categories from the post
            ArrayList categories = new ArrayList();
            XmlNode categoriesNode = postNode.SelectSingleNode("categoryId");
            if (categoriesNode != null)
            {
                XmlNodeList categoryNodes = categoriesNode.SelectNodes("categoryId");
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    categories.Add(
                        new BlogPostCategory(categoryNode.InnerText, categoryNode.InnerText));
                }
            }

            return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
        }
        */

        private BlogPostCategory[] MovableTypeExtractCategories(string postId, XmlNode postNode)
        {
            // call the method
            XmlNode result = CallMethod("mt.getPostCategories",
                new XmlRpcString(postId),
                new XmlRpcString(Username),
                new XmlRpcString(Password, true));

            // parse out the data
            ArrayList categories = new ArrayList();
            XmlNodeList categoryNodes = result.SelectNodes("array/data/value/struct");
            if (categoryNodes != null)
            {
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    // parse out the category name
                    XmlNode categoryId = categoryNode.SelectSingleNode("member[name='categoryId']/value");
                    XmlNode categoryTitle = categoryNode.SelectSingleNode("member[name='categoryName']/value");

                    // add the category
                    categories.Add(new BlogPostCategory(categoryId.InnerText, categoryTitle.InnerText));
                }

                return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
            }
            else
            {
                throw new BlogClientInvalidServerResponseException("mt.getPostCategories", "No categories returned from server", result.OuterXml);
            }
        }


        protected virtual void BlogPostReadFilter(BlogPost blogPost)
        {
        }

        private void AddTrackbacks(BlogPost post, ArrayList members)
        {
            if (post.PingUrlsPending.Length > 0)
            {
                if (Options.TrackbackDelimiter == TrackbackDelimiter.ArrayElement)
                {
                    members.Add(new XmlRpcMember("mt_tb_ping_urls", ArrayFromStrings(post.PingUrlsPending)));
                }
                else
                {
                    char delimiter;
                    switch (Options.TrackbackDelimiter)
                    {
                        case TrackbackDelimiter.Space:
                            delimiter = ' ';
                            break;
                        case TrackbackDelimiter.Comma:
                        default:
                            delimiter = ',';
                            break;
                    }
                    StringBuilder trackbackBuilder = new StringBuilder();
                    foreach (string pingUrl in post.PingUrlsPending)
                        trackbackBuilder.AppendFormat("{0}{1}", pingUrl, delimiter);
                    string trackbacks = trackbackBuilder.ToString().TrimEnd(delimiter);

                    members.Add(new XmlRpcMember("mt_tb_ping_urls", trackbacks));
                }
            }
        }
        
        static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        protected XmlDocument ParseXmlResponse(HttpWebResponse response)
        {
            MemoryStream ms = new MemoryStream();
            using (Stream s = response.GetResponseStream())
            {
                StreamHelper.Transfer(s, ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            string text = new StreamReader(ms, Encoding.UTF8).ReadToEnd();

            ms.Seek(0, SeekOrigin.Begin);
            if (ms.Length == 0)
                return null;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ms);
                XmlHelper.ApplyBaseUri(xmlDoc, response.ResponseUri);

                return xmlDoc;
            }
            catch (Exception e)
            {
                Trace.Fail("Malformed XML document: " + e.ToString());
                return null;
            }
        }

        public class TokenResponse
        {
            private string token;

            public TokenResponse()
            {
                token = "";
            }

            public string AccessToken
            {
                get { return this.token; }
                set { this.token = value; }
            }
        }
        public class TistoryCredential: TransientCredentials
        {
            private TokenResponse _token = null;
            private string _user_key = "";

            public TistoryCredential(string username, string password, string user_key, object token)
                :base(username, password, token)
            {
                _token = token as TokenResponse;
                _user_key = user_key;
            }
            public string UserKey
            {
                get { return _user_key; }
            }

            public bool IsExpired(string expire_check_url)
            {
                try
                {
                    if(_token != null )
                    {
                        string request_uri;
                        request_uri = String.Format("{0}{1}&url={2}", expire_check_url, _token.AccessToken, Username);
                        Uri expire_uri = new Uri(request_uri);

                        XmlRestRequestHelper expire_request = new XmlRestRequestHelper();
                        XmlDocument expire_xml = expire_request.Get(ref expire_uri, null);
                        XmlNode statusNode;

                        statusNode = expire_xml.SelectSingleNode("tistory/status");

                        if (statusNode.InnerText.Contains("200"))
                            return false;                       
                    }
                }
                catch(Exception e)
                {
                    string erro = e.Message;
                }

                return true;
            }
        }
    }


}
