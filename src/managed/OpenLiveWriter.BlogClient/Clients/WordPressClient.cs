// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using System.Net;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using System.Web;

// TODO: WP on outstanding issues
/*
- Trackback arrays now work -- should switch over to using them
- "mt_allow_pings" and "mt_allow_comments" issues w/ use of empty (sending "2" does the trick)
- can't set wp_page_parent_id to 0 because the use of empty causes it to be skipped
- can't set wp_password to nothing after setting one
*/

namespace OpenLiveWriter.BlogClient.Clients
{

    [BlogClient("WordPress", "WordPress")]
    public class WordPressClient : MovableTypeClient
    {
        public WordPressClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
        }

        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            // inherit mt defaults
            base.ConfigureClientOptions(clientOptions);

            // support advanced category features
            clientOptions.SupportsHierarchicalCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsSuggestCategories = true;

            // don't require out-of-band categories
            clientOptions.SupportsCategoriesInline = true;

            // add support for wp-api features
            clientOptions.SupportsKeywords = true;
            clientOptions.SupportsPages = true;
            clientOptions.SupportsPageParent = true;
            clientOptions.SupportsPageOrder = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsPassword = true;
            clientOptions.SupportsAuthor = true;
        }

        public override BlogInfo[] GetUsersBlogs()
        {
            TransientCredentials tc = Login();
            return GetWPUsersBlogs(tc.Username, tc.Password);
        }
        private BlogInfo[] GetWPUsersBlogs(string username, string password)
        {
            // call method
            XmlNode result = CallMethod("wp.getUsersBlogs",
                new XmlRpcString(username),
                new XmlRpcString(password, true));

            try
            {
                // parse results
                ArrayList blogs = new ArrayList();
                XmlNodeList dataValues = result.SelectNodes("array/data");
                foreach (XmlNode dataValue in dataValues)
                {
                    XmlNodeList blogNodes = dataValue.SelectNodes("value/struct");
                    foreach (XmlNode blogNode in blogNodes)
                    {
                        // get node values
                        XmlNode idNode = blogNode.SelectSingleNode("member[name='blogid']/value");
                        XmlNode nameNode = blogNode.SelectSingleNode("member[name='blogName']/value");
                        XmlNode urlNode = blogNode.SelectSingleNode("member[name='url']/value");

                        // add to our list of blogs
                        blogs.Add(new BlogInfo(idNode.InnerText, HttpUtility.HtmlDecode(NodeToText(nameNode)), urlNode.InnerText));
                    }
                }

                // return list of blogs
                return (BlogInfo[])blogs.ToArray(typeof(BlogInfo));
            }
            catch (Exception ex)
            {
                string response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail("Exception occurred while parsing GetUsersBlogs response: " + response + "\r\n" + ex.ToString());
                throw new BlogClientInvalidServerResponseException("wp.getUsersBlogs", ex.Message, response);
            }
        }

    }
}
