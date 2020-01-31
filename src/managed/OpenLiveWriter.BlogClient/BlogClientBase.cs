// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Net;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient
{
    /// <summary>
    /// Summary description for BlogClientBase.
    /// </summary>
    public abstract class BlogClientBase
    {
        public BlogClientBase(IBlogCredentialsAccessor credentials)
        {
            _credentials = credentials;
        }

        public string ProtocolName
        {
            get
            {
                try
                {
                    return ((BlogClientAttribute)GetType().GetCustomAttributes(typeof(BlogClientAttribute), false)[0]).ProtocolName;
                }
                catch (IndexOutOfRangeException)
                {
                    Trace.Fail("BlogClientAttribute not found on type " + GetType().FullName);
                    return null;
                }
            }
        }

        protected IBlogCredentialsAccessor Credentials
        {
            get { return _credentials; }
        }
        private IBlogCredentialsAccessor _credentials;

        /// <summary>
        /// Forces a synchronous login.
        /// </summary>
        /// <returns>returns true if a valid set of credentials have been established for this client</returns>
        public bool VerifyCredentials()
        {
            try
            {
                return Login() != null;
            }
            catch (BlogClientOperationCancelledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a valid set of credentials for the current blog.
        /// If cached credentials are incomplete or invalid, the user will automatically be prompted for them.
        /// </summary>
        /// <returns></returns>
        protected virtual TransientCredentials Login()
        {
            TransientCredentials transientCredential = Credentials.TransientCredentials as TransientCredentials;
            if (transientCredential == null)
            {
                transientCredential = CreateAuthenticatedCredential();
                Debug.Assert(transientCredential != null, "Transient credential should never be null, throw an exception instead");
                Credentials.TransientCredentials = transientCredential;
            }
            return transientCredential;
        }

        /// <summary>
        /// Hook that allows subclasses to override whether a password must by supplied.
        /// </summary>
        protected virtual bool RequiresPassword
        {
            get { return true; }
        }

        /// <summary>
        /// Creates an authenticated credential that has been verified against the server.
        /// </summary>
        /// <returns></returns>
        protected virtual TransientCredentials CreateAuthenticatedCredential()
        {
            string username = Credentials.Username;
            string password = Credentials.Password;

            //create and store the transient credential since some blog clients rely on it being set to
            //when making requests during validation (like SharePoint and Spaces)
            object oldTransientCredentials = Credentials.TransientCredentials;
            TransientCredentials tc = new TransientCredentials(username, password, null);
            Credentials.TransientCredentials = tc;
            try
            {
                bool promptForPassword = RequiresPassword && (password == null || password == String.Empty);
                Exception verificationException = promptForPassword ? null : VerifyCredentialsReturnException(tc);
                bool verified = !promptForPassword && verificationException == null;
                while (!verified)
                {
                    //if we're in silent mode where prompting isn't allowed, so throw the verification exception
                    if (BlogClientUIContext.SilentModeForCurrentThread)
                    {
                        if (verificationException != null)
                            throw verificationException;
                        else
                            throw new BlogClientAuthenticationException(String.Empty, String.Empty);
                    }

                    //prompt the user for credentials
                    CredentialsPromptResult prompt = CredentialsHelper.PromptForCredentials(ref username, ref password, Credentials.Domain);
                    if (prompt == CredentialsPromptResult.Abort || prompt == CredentialsPromptResult.Cancel)
                        throw new BlogClientOperationCancelledException();

                    //update the credentials, and re-verify them
                    tc = new TransientCredentials(username, password, null);
                    Credentials.TransientCredentials = tc;
                    verificationException = VerifyCredentialsReturnException(tc);
                    verified = verificationException == null;

                    if (verified)
                    {
                        //persist the credentials based on the user's preference from the prompt
                        if (prompt == CredentialsPromptResult.SaveUsernameAndPassword)
                        {
                            Credentials.Username = username;
                            Credentials.Password = password;
                        }
                        else if (prompt == CredentialsPromptResult.SaveUsername)
                        {
                            Credentials.Username = username;
                        }
                    }
                }
            }
            finally
            {
                Credentials.TransientCredentials = oldTransientCredentials;
            }

            return tc;
        }

        private Exception VerifyCredentialsReturnException(TransientCredentials tc)
        {
            try
            {
                using (CredentialsHelper.ShowWaitCursor())
                {
                    VerifyCredentials(tc);
                }
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Hook used by subclasses to validate a newly created set of credentials.
        /// </summary>
        /// <param name="tc"></param>
        protected abstract void VerifyCredentials(TransientCredentials tc);

        /// <summary>
        /// Almost all blogs supported are remote blogs, with few exceptions.
        /// eg. local static sites
        /// </summary>
        public virtual bool RemoteDetectionPossible { get; } = true;
    }
}
