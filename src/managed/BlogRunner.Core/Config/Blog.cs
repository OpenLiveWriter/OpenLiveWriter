// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BlogRunner.Core.Config
{
    public class Blog
    {
        private string homepageUrl, username, password, apiUrl, blogId;

        [XmlElement(ElementName = "homepageUrl")]
        public string HomepageUrl
        {
            get { return homepageUrl; }
            set { homepageUrl = value; }
        }

        [XmlElement(ElementName = "username")]
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        [XmlElement(ElementName = "password")]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        [XmlElement(ElementName = "apiUrl")]
        public string ApiUrl
        {
            get { return apiUrl; }
            set { apiUrl = value; }
        }

        [XmlElement(ElementName = "blogId")]
        public string BlogId
        {
            get { return blogId; }
            set { blogId = value; }
        }
    }

    public enum BlogApi { XmlRpc, AtomPub }
}
