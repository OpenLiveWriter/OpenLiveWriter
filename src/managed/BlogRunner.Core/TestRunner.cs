// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.PostEditor.Configuration;
using Blog = BlogRunner.Core.Config.Blog;
using OpenLiveWriter.Extensibility.BlogClient;
using System.Collections;
using System.Diagnostics;
using System.Xml;

namespace BlogRunner.Core
{
    public class TestRunner
    {
        IEnumerable<Test> tests;

        public TestRunner(IEnumerable<Test> tests)
        {
            this.tests = tests;
        }

        public void RunTests(Provider provider, Blog blog, XmlElement providerEl)
        {
            using (new BlogClientUIContextSilentMode()) //suppress prompting for credentials
            {
                TemporaryBlogCredentials credentials = new TemporaryBlogCredentials();
                credentials.Username = blog.Username;
                credentials.Password = blog.Password;
                BlogCredentialsAccessor credentialsAccessor = new BlogCredentialsAccessor(Guid.NewGuid().ToString(), credentials);
                IBlogClient client = BlogClientManager.CreateClient(provider.ClientType, blog.ApiUrl, credentialsAccessor);

                if (blog.BlogId == null)
                {
                    BlogInfo[] blogs = client.GetUsersBlogs();
                    if (blogs.Length == 1)
                    {
                        blog.BlogId = blogs[0].Id;
                        credentialsAccessor = new BlogCredentialsAccessor(blog.BlogId, credentials);
                        client = BlogClientManager.CreateClient(provider.ClientType, blog.ApiUrl, credentialsAccessor);
                    }
                }

                foreach (Test test in tests)
                {
                    try
                    {
                        Console.WriteLine("Running test " + test.ToString());
                        test.DoTest(blog, client, providerEl);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Error: Test " + test.GetType().Name + " failed for provider " + provider.Name + ":");
                        Console.Error.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
