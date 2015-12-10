// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    /// <summary>
    /// Summary description for IBlogPostContentFilter.
    /// </summary>
    public interface IBlogPostContentFilter
    {
        /// <summary>
        /// Filter content being read from a remote blog server.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        string OpenFilter(string content);

        /// <summary>
        /// Filter content being published to a remote blog server.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        string PublishFilter(string content);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BlogPostContentFilterAttribute : Attribute
    {
        public BlogPostContentFilterAttribute(string typeName)
        {
            _typeName = typeName;
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        private string _typeName;
    }
}
