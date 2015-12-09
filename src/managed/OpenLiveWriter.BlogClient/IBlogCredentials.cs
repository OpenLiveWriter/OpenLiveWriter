// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;

namespace OpenLiveWriter.BlogClient
{
    public interface IBlogCredentials
    {
        string Username { get; set; }
        string Password { get; set; }

        ICredentialsDomain Domain { get; set; }

        string[] CustomValues { get; }
        string GetCustomValue(string name);
        void SetCustomValue(string name, string value);
        void Clear();
    }

    public interface IBlogCredentialsAccessor
    {
        string Username { get; set; }
        string Password { get; set; }
        string[] CustomValues { get; }
        string GetCustomValue(string name);
        void SetCustomValue(string name, string value);

        ICredentialsDomain Domain { get; }

        /// <summary>
        /// Transient credentials (credentials that the user has instructed
        /// us not to save but which we nevertheless need in-memory access to).
        /// This object can be of any type and provides unique, process-wide
        /// storage for each configured account in the system.
        /// The most common application of this property would be to store a
        /// password that the user  has provided for future use within the
        /// lifetime of the process. Note that because these credentials are
        /// shared process-wide the underlying object must be threadsafe.
        /// </summary>
        object TransientCredentials { get; set; }
    }

    public class BlogCredentialsAccessor : IBlogCredentialsAccessor
    {
        public BlogCredentialsAccessor(string accountId, IBlogCredentials credentials)
        {
            _accountId = accountId;
            _credentials = credentials;
        }

        public string Username
        {
            get
            {
                return _credentials.Username;
            }
            set
            {
                _credentials.Username = value;
            }
        }

        public string Password
        {
            get
            {
                return _credentials.Password;
            }
            set
            {
                _credentials.Password = value;
            }
        }

        public string[] CustomValues
        {
            get
            {
                return _credentials.CustomValues;
            }
        }

        public string GetCustomValue(string name)
        {
            return _credentials.GetCustomValue(name);
        }

        public void SetCustomValue(string name, string value)
        {
            _credentials.SetCustomValue(name, value);
        }

        public object TransientCredentials
        {
            get
            {
                lock (_transientCredentials)
                {
                    return _transientCredentials[_accountId];
                }
            }
            set
            {
                lock (_transientCredentials)
                {
                    if (value == null)
                        _transientCredentials.Remove(_accountId);
                    else
                        _transientCredentials[_accountId] = value;
                }
            }
        }

        public ICredentialsDomain Domain
        {
            get { return _credentials.Domain; }
        }

        private string _accountId;
        private IBlogCredentials _credentials;

        private static Hashtable _transientCredentials = new Hashtable();
    }

    public class TransientCredentials
    {
        private string _username;
        private string _password;
        private object _token;

        public TransientCredentials(string username, string password, object token)
        {
            _username = username;
            _password = password;
            _token = token;
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public object Token
        {
            get { return _token; }
            set { _token = value; }
        }
    }

    public sealed class BlogCredentialsHelper
    {
        public static void Copy(IBlogCredentials sourceCredentials, IBlogCredentials destCredentials)
        {
            destCredentials.Clear();
            destCredentials.Username = sourceCredentials.Username;
            destCredentials.Password = sourceCredentials.Password;
            foreach (string customValue in sourceCredentials.CustomValues)
                destCredentials.SetCustomValue(customValue, sourceCredentials.GetCustomValue(customValue));

            destCredentials.Domain = sourceCredentials.Domain;
        }

        public static bool CredentialsAreEqual(IBlogCredentials c1, IBlogCredentials c2)
        {
            if (c1.Username != c2.Username)
                return false;

            if (c1.Password != c2.Password)
                return false;

            foreach (string customValue in c1.CustomValues)
            {
                string c1Value = c1.GetCustomValue(customValue);
                string c2Value = c2.GetCustomValue(customValue);

                if (c1Value != c2Value)
                    return false;
            }

            // if we got this far then they are equal
            return true;
        }

    }
}

