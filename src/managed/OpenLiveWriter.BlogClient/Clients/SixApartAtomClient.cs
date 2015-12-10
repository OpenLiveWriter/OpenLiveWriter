// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("SixApartAtom")]
    public class SixApartAtomClient : AtomClient
    {
        public SixApartAtomClient(Uri postApiUrl, IBlogCredentialsAccessor credentials, PostFormatOptions postFormatOptions)
            : base(AtomProtocolVersion.V03, postApiUrl, credentials, postFormatOptions)
        {
        }

        public override bool VerifyCredentials()
        {
            // TODO
            return true;
        }

        public override BlogClientCapabilities Capabilities { get { return BlogClientCapabilities.Categories | BlogClientCapabilities.MultipleCategories; } }

        public override BlogPostCategory[] GetCategories(string blogId)
        {
/*
            // get the introspection doc that will lead us to the categories URI
            XmlDocument insDoc = xmlRestRequestHelper.Get("http://www.typepad.com/t/atom/weblog", RequestFilter);
*/
            throw new NotImplementedException();
        }

        protected override HttpRequestFilter RequestFilter
        {
            get
            {
                return new HttpRequestFilter(WsseFilter);
            }
        }

        protected override void Populate(BlogPost post, XmlNode node)
        {
            base.Populate(post, node);
            XmlElement catEl = _atomVer.CreateCategoryElement(node.OwnerDocument, "flimflam");
            node.AppendChild(catEl);
        }

        private void WsseFilter(HttpWebRequest request)
        {
            string username = "joe@unknown.com";//Credentials.Username;
            string password = "abc123";//Credentials.Password;

            string nonce = Guid.NewGuid().ToString("d");
            string created = DateTimeHelper.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo);
            byte[] stringToHash = Encoding.UTF8.GetBytes(nonce + created + password);
            byte[] bytes = SHA1Managed.Create().ComputeHash(stringToHash);
            string digest = Convert.ToBase64String(bytes);

            string headerValue = string.Format("UsernameToken Username=\"{0}\", PasswordDigest=\"{1}\", Created=\"{2}\", Nonce=\"{3}\"",
                username,
                digest,
                created,
                nonce);
            if (headerValue.IndexOfAny(new char[] {'\r', '\n'}) >= 0)
                throw new BlogClientAuthenticationException("ProtocolViolation", "Protocol violation, EOL characters are not allowed in WSSE headers");
            request.Headers.Add("X-WSSE", headerValue);
        }

        protected override void EnsureLoggedIn()
        {
        }

        public override BlogInfo[] GetUsersBlogs()
        {
            throw new NotImplementedException();
        }
    }
}
