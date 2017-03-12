// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    /// <summary>
    /// Summary description for WebProxyPreferences.
    /// </summary>
    public class WebProxyPreferences : Preferences
    {
        public WebProxyPreferences() : base("WebProxy")
        {
        }

        public bool ProxyEnabled
        {
            get { return _proxyEnabled; }
            set { _proxyEnabled = value; Modified(); }
        }
        private bool _proxyEnabled;

        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; Modified(); }
        }
        private string _hostname;

        public int Port
        {
            get { return _port; }
            set { _port = value; Modified(); }
        }
        private int _port;

        public string Username
        {
            get { return _username; }
            set { _username = value; Modified(); }
        }
        private string _username;

        public string Password
        {
            get { return _password; }
            set { _password = value; Modified(); }
        }
        private string _password;

        protected override void LoadPreferences()
        {
            ProxyEnabled = WebProxySettings.ProxyEnabled;
            Hostname = WebProxySettings.Hostname;
            Port = WebProxySettings.Port;
            Username = WebProxySettings.Username;
            Password = WebProxySettings.Password;
        }

        protected override void SavePreferences()
        {
            WebProxySettings.ProxyEnabled = ProxyEnabled;
            WebProxySettings.Hostname = Hostname;
            WebProxySettings.Port = Port;
            WebProxySettings.Username = Username;
            WebProxySettings.Password = Password;
        }
    }
}
