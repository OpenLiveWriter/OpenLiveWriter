// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients
{
    public enum CredentialsPromptResult { Cancel, Abort, SaveUsername, SaveUsernameAndPassword };
    public class CredentialsHelper
    {
        public static IDisposable ShowWaitCursor()
        {
            IBlogClientUIContext uiContext = BlogClientUIContext.ContextForCurrentThread;
            if (uiContext != null && !uiContext.InvokeRequired)
                return new WaitCursor();
            else
                return null;
        }
        public static CredentialsPromptResult PromptForCredentials(ref string username, ref string password, ICredentialsDomain domain)
        {
            CredentialsPromptResult result;

            if (BlogClientUIContext.SilentModeForCurrentThread)
                return CredentialsPromptResult.Abort;

            IBlogClientUIContext uiContext = BlogClientUIContext.ContextForCurrentThread;
            if (uiContext != null)
            {
                PromptHelper promptHelper = new PromptHelper(uiContext, username, password, domain);
                if (uiContext.InvokeRequired)
                    uiContext.Invoke(new InvokeInUIThreadDelegate(promptHelper.ShowPrompt), new object[0]);
                else
                {
                    promptHelper.ShowPrompt();

                    //force a UI loop so that the dialog closes without hanging while post-dialog logic executes
                    Application.DoEvents();
                }

                result = promptHelper.Result;
                if (result != CredentialsPromptResult.Cancel)
                {
                    username = promptHelper.Username;
                    password = promptHelper.Password;
                }
            }
            else
            {
                result = CredentialsPromptResult.Abort;
            }
            return result;
        }

        private class PromptHelper
        {
            private IWin32Window _owner;
            private string _username;
            private string _password;
            private CredentialsPromptResult _result;
            private ICredentialsDomain _domain;
            public PromptHelper(IWin32Window owner, string username, string password, ICredentialsDomain domain)
            {
                _owner = owner;
                _username = username;
                _password = password;
                _domain = domain;
            }

            public void ShowPrompt()
            {
                using (BlogClientLoginDialog form = new BlogClientLoginDialog())
                {
                    if (_username != null)
                        form.UserName = _username;
                    if (_password != null)
                        form.Password = _password;
                    if (_domain != null)
                    {
                        form.Domain = _domain;
                        form.Text = form.Text + " - " + _domain.Name;
                    }

                    DialogResult dialogResult = form.ShowDialog(_owner);
                    if (dialogResult == DialogResult.OK)
                    {
                        _username = form.UserName;
                        _password = form.Password;
                        _result = form.SavePassword
                                    ? CredentialsPromptResult.SaveUsernameAndPassword
                                    : CredentialsPromptResult.SaveUsername;
                    }
                    else
                        _result = CredentialsPromptResult.Cancel;
                }
            }

            public string Username
            {
                get { return _username; }
            }

            public string Password
            {
                get { return _password; }
            }

            public CredentialsPromptResult Result
            {
                get { return _result; }
            }
        }
    }

#if false
    /// <summary>
    /// Summary description for CredentialsHelper.
    /// </summary>
    public class CredentialsHelper
    {
        private CredentialsHelper()
        {
        }

        public static BlogCredentialsRefreshCallback GetRefreshCallback()
        {
            return new BlogCredentialsRefreshCallback(RefreshCredentials);
        }

        private static BlogCredentialsRefreshResult RefreshCredentials(IBlogClientUIContext owner, ref string username, ref string password, ref object authToken)
        {
            BlogCredentialsRefreshResult refreshResult;
            if(authToken != null ||
                (username != String.Empty && username != null && password != String.Empty && password != null))
            {
                refreshResult = BlogCredentialsRefreshResult.OK;
            }
            else
            {
                //Some of the required credential values are  missing, so prompt for them if there is
                //a UI context available
                if(owner == null)
                    refreshResult = BlogCredentialsRefreshResult.Abort;
                else
                {
                    PromptHelper promptHelper = new PromptHelper(owner, ref username, ref password);
                    if(owner.InvokeRequired)
                        owner.Invoke(new InvokeInUIThreadDelegate(promptHelper.ShowPrompt), new object[0]);
                    else
                        promptHelper.ShowPrompt();

                    username = promptHelper.Username;
                    password = promptHelper.Password;
                    refreshResult = promptHelper.Result;
                }
            }

            if(refreshResult != BlogCredentialsRefreshResult.Cancel && refreshResult != BlogCredentialsRefreshResult.Abort)
            {
                //save the login time in the authtoken
                authToken = DateTime.Now;
            }
            return refreshResult;
        }

        public class PromptHelper
        {
            private IWin32Window owner;
            private string _username;
            private string _password;
            private BlogCredentialsRefreshResult _result;
            public PromptHelper(IWin32Window owner, ref string username, ref string password)
            {
                this.owner = owner;
                _username = username;
                _password = password;
            }

            public void ShowPrompt()
            {
                BlogCredentialsRefreshResult refreshResult;
                using (BlogClientLoginDialog form = new BlogClientLoginDialog())
                {
                    if(_username != null)
                        form.UserName = _username;
                    if(_password != null)
                        form.Password = _password;

                    DialogResult dialogResult = form.ShowDialog(owner) ;
                    if (dialogResult == DialogResult.OK)
                    {
                        _username = form.UserName;
                        _password = form.Password;
                        refreshResult = form.SavePassword
                            ? BlogCredentialsRefreshResult.SaveUsernameAndPassword
                            : BlogCredentialsRefreshResult.SaveUsername;
                    }
                    else
                        refreshResult = BlogCredentialsRefreshResult.Cancel;
                }
                _result = refreshResult;
            }

            public string Username
            {
                get { return _username; }
            }

            public string Password
            {
                get { return _password; }
            }

            public BlogCredentialsRefreshResult Result
            {
                get { return _result; }
            }
        }
    }
#endif
}
