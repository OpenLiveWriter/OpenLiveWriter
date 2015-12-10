// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.ContentSources.Common
{
    public interface IAuth
    {
        bool IsLoggedIn { get; }

        string Username { get; }
        string AuthToken { get; }

        bool AllowSavePassword { get; }
        bool PasswordRequired(string username);

        bool Login(bool showUI, IWin32Window parent);
        bool Login(string userName, string passWord, bool savePassword, bool ignoreSavedPassword, IWin32Window parent);
        void Logout();

        event EventHandler LoginStatusChanged;

        Bitmap LoginLogo { get; }
        string LoginText { get; }
        string LoginUsernameLabel { get; }
        string LoginPasswordLabel { get; }
        string LoginExampleText { get; }
        string LoginSavePasswordText { get; }
        string ServiceUrl { get; }

    }

    #region YouTubeAuth : IAuth
    public class YouTubeAuth : IAuth
    {
        [ThreadStatic]
        private static YouTubeAuth _this;

        private GDataCredentials _credentials;
        private string _username;
        private string _password;

        private YouTubeAuth()
        {

        }

        public static YouTubeAuth Instance
        {
            get
            {
                if (_this == null)
                    _this = new YouTubeAuth();

                return _this;
            }
        }

        public string Username
        {
            get
            {
                Debug.Assert(IsLoggedIn, "Should not try to get Username, if user is not logged in.");
                return _credentials.GetUserName(_username, _password, GDataCredentials.YOUTUBE_SERVICE_NAME); ;
            }
        }

        public string AuthToken
        {
            get
            {
                Debug.Assert(IsLoggedIn, "Should not try to get AuthToken, if user is not logged in.");
                return _credentials.GetCredentialsIfValid(_username, _password, GDataCredentials.YOUTUBE_SERVICE_NAME);
            }
        }

        public bool AllowSavePassword
        {
            get { return false; }
        }

        public bool PasswordRequired(string username)
        {
            return true;
        }

        public bool AttemptAutoLogin(IWin32Window parent)
        {
            return false;
        }

        public bool IsLoggedIn
        {
            get
            {
                return _credentials != null && _credentials.IsValid(_username, _password, GDataCredentials.YOUTUBE_SERVICE_NAME);
            }
        }

        public bool Login(string username, string password, bool savePassword, bool ignoreSavedPassword, IWin32Window parent)
        {
            return Login(username, password);
        }

        public bool Login(string username, string password)
        {
            using (new WaitCursor())
            {
                try
                {
                    _username = username;
                    _password = password;
                    TransientCredentials creds = new TransientCredentials(username, password, null);
                    _credentials = GDataCredentials.FromCredentials(creds);
                    _credentials.EnsureLoggedIn(username, password, GDataCredentials.YOUTUBE_SERVICE_NAME, true, GDataCredentials.YOUTUBE_CLIENT_LOGIN_URL);
                }
                catch (Exception)
                {
                    _credentials = null;
                }
            }

            OnLoginStatusChanged();
            return IsLoggedIn;
        }

        public bool Login(bool showUI, IWin32Window parent)
        {
            if (!showUI) return false;

            BlogClientLoginDialog d = new BlogClientLoginDialog();
            d.Domain = new CredentialsDomain(Res.Get(StringId.Plugin_Video_Youtube_Publish_Name), String.Empty, null, ImageHelper.GetBitmapBytes(ResourceHelper.LoadAssemblyResourceBitmap("Video.YouTube.Images.YouTubeTab.png")), false);
            d.Closing += delegate (object sender, CancelEventArgs e)
                             {
                                 if (d.DialogResult == DialogResult.OK)
                                 {
                                     if (string.IsNullOrEmpty(d.UserName) || string.IsNullOrEmpty(d.Password))
                                     {
                                         DisplayMessage.Show(MessageId.UsernameAndPasswordRequired, this, null);
                                         e.Cancel = true;
                                     }
                                     else if (!Login(d.UserName, d.Password))
                                     {
                                         e.Cancel = true;
                                     }
                                 }

                             };

            d.ShowDialog(parent);

            OnLoginStatusChanged();
            return IsLoggedIn;
        }

        public void Logout()
        {
            _username = null;
            _password = null;
            _credentials = null;
            OnLoginStatusChanged();
        }

        public void SavePassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        public event EventHandler LoginStatusChanged;

        public Bitmap LoginLogo
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("Video.YouTube.Images.LoginLogo.png");
            }
        }

        public string LoginText
        {
            get { return ""; }
        }

        public string LoginUsernameLabel
        {
            get { return Res.Get(StringId.UsernameLabel); }
        }

        public string LoginPasswordLabel
        {
            get { return Res.Get(StringId.PasswordLabel); }
        }

        public string LoginExampleText
        {
            get { return ""; }
        }

        public string LoginSavePasswordText
        {
            get { return Res.Get(StringId.RememberPassword); }
        }

        protected virtual void OnLoginStatusChanged()
        {
            if (LoginStatusChanged != null)
                LoginStatusChanged(this, EventArgs.Empty);
        }

        public string ServiceUrl
        {
            get { return "http://www.youtube.com"; }
        }
    }
    #endregion

}
