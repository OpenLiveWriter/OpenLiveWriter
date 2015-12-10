// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Extensibility.BlogClient
{

    public class BlogAccount
    {
        public BlogAccount(string serviceName, string clientType, string postApiUrl, string blogId)
        {
            Init(serviceName, clientType, postApiUrl, blogId);
        }

        protected BlogAccount()
        {
        }

        protected void Init(string serviceName, string clientType, string postApiUrl, string blogId)
        {
            _serviceName = serviceName;
            _clientType = clientType;
            _postApiUrl = postApiUrl;
            _blogId = blogId;
        }

        public string ServiceName
        {
            get { return _serviceName; }
        }
        private string _serviceName;

        public string ClientType
        {
            get { return _clientType; }
        }
        private string _clientType;

        public string PostApiUrl
        {
            get { return _postApiUrl; }
        }
        private string _postApiUrl;

        public string BlogId
        {
            get { return _blogId; }
        }
        private string _blogId;

    }
}
