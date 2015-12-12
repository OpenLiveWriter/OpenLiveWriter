// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Detection
{
    public class BlogAccountDetector
    {

        /// <summary>
        /// Detect a specific BlogAccount (or list of blog accounts if a single account could not
        /// be identified).
        /// </summary>
        /// <param name="homepageUrl">Hint to the detector that if there is a list of blogs then the one with this homepageUrl is the one we are seeking</param>
        /// <param name="clientType">Client API type</param>
        /// <param name="postApiUrl">Post API URL</param>
        /// <param name="credential">Credentials</param>
        public BlogAccountDetector(string clientType, string postApiUrl, IBlogCredentialsAccessor credentials)
        {
            _clientType = clientType;
            _postApiUrl = postApiUrl;
            _credentials = credentials;
        }

        public bool ValidateService()
        {
            try
            {
                using (new BlogClientUIContextSilentMode()) //suppress prompting for password if an error occurs
                {
                    // get a list of the user's blogs
                    IBlogClient client = BlogClientManager.CreateClient(_clientType, _postApiUrl, _credentials);
                    _usersBlogs = client.GetUsersBlogs();

                    // we can't continue if there are no blogs
                    if (_usersBlogs.Length == 0)
                        throw new NoAccountsOnServerException();

                    // success
                    return true;
                }
            }
            catch (BlogClientAuthenticationException ex)
            {
                ReportError(ex, MessageId.WeblogAuthenticationError);
                return false;
            }
            catch (BlogClientPostUrlNotFoundException ex)
            {
                ReportError(ex, MessageId.WeblogUrlNotFound, _postApiUrl);
                return false;
            }
            catch (NoAccountsOnServerException ex)
            {
                ReportError(ex, MessageId.WeblogNoAccountsOnServer);
                return false;
            }
            catch (BlogClientOperationCancelledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ReportError(ex, MessageId.WeblogConnectionError, ex.Message);
                return false;
            }
        }

        public BlogInfo DetectAccount(string homepageUrlHint, string blogIdHint)
        {
            // although the request is for all blogs belonging to the user, some
            // sites return only the first blog found for the user (which may not
            // be the correct blog).  Therefore, even if only one weblog is returned
            // we still need to scan the list to see if any of the returned blogs
            // are the correct one (looking for the homepage or blog id hint).
            foreach (BlogInfo blog in _usersBlogs)
            {
                // strip trailing slashes from the urls for comparison
                string url1 = UrlHelper.InsureTrailingSlash(blog.HomepageUrl);
                string url2 = UrlHelper.InsureTrailingSlash(homepageUrlHint);

                // compare the urls and the blog ids
                if (((url1 != String.Empty) && UrlHelper.UrlsAreEqual(url1, url2)) ||
                    blog.Id == blogIdHint)
                {
                    return blog;
                }
            }

            // couldn't find a matching target weblog!
            return null;
        }

        public bool ServiceIncludesBlogId(string blogId)
        {
            // otherwise scan the list of blogs with the passed homepage and
            // blog id hints to see if we have a match
            foreach (BlogInfo blog in _usersBlogs)
            {
                if (blogId.Equals(blog.Id))
                    return true;
            }

            // didn't find a match
            return false;
        }

        public BlogInfo[] UsersBlogs
        {
            get { return _usersBlogs; }
        }

        public bool UserAuthorizationError
        {
            get { return (ErrorMessageType != MessageId.None) && (ErrorMessageType == MessageId.WeblogAuthenticationError); }
        }

        public MessageId ErrorMessageType
        {
            get { return _errorMessageType; }
        }

        public object[] ErrorMessageParams
        {
            get { return _errorMessageParams; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
        private Exception _exception;

        public void ShowLastError(IWin32Window owner)
        {
            if (_errorMessageType != MessageId.None)
            {
                DisplayMessage.Show(_errorMessageType, owner, _errorMessageParams);
            }
            else
            {
                Trace.Fail("Called ShowLastError when no error occurred");
            }
        }

        private void ReportError(Exception ex, MessageId errorMessageType, params object[] errorMessageParams)
        {
            if (ex != null)
            {
                if (ex is WebException)
                    HttpRequestHelper.LogException((WebException)ex);

                if (ApplicationDiagnostics.AutomationMode)
                    Trace.WriteLine(ex.ToString());
                else
                    Trace.Fail(ex.ToString());
            }
            _exception = ex;
            _errorMessageType = errorMessageType;
            _errorMessageParams = errorMessageParams;
        }

        private string _clientType;
        private string _postApiUrl;
        private IBlogCredentialsAccessor _credentials;
        private string _blogId = String.Empty;
        private string _blogName = String.Empty;
        private BlogInfo[] _usersBlogs = new BlogInfo[] { };
        private MessageId _errorMessageType;
        private object[] _errorMessageParams;

        private class NoAccountsOnServerException : ApplicationException
        {
        }

    }
}
