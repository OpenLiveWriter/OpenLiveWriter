// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// Summary description for BlogPostContentFilters.
    /// </summary>
    public class BlogPostContentFilters
    {
        private BlogPostContentFilters()
        {
        }

        static BlogPostContentFilters()
        {
            AddContentFilter(typeof(LineBreak2PBRInputFormatter));
            AddContentFilter(typeof(LineBreak2BRInputFormatter));
            AddContentFilter(typeof(WordPressInputFormatter));
        }

        public static IBlogPostContentFilter CreateContentFilter(string filterName)
        {
            ContentFilterTypeDefinition cftDefinition = _contentFilters[filterName] as ContentFilterTypeDefinition;
            return (IBlogPostContentFilter)cftDefinition.Constructor.Invoke(new object[0]);
        }

        public static void AddContentFilter(Type filter)
        {
            try
            {
                ContentFilterTypeDefinition definition = new ContentFilterTypeDefinition(filter);
                _contentFilters.Add(definition.Name, definition);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception adding Content Filter: " + ex.ToString());
            }
        }
        private static Hashtable _contentFilters = new Hashtable();

        private class ContentFilterTypeDefinition
        {
            public ContentFilterTypeDefinition(Type filterType)
            {
                if (!typeof(IBlogPostContentFilter).IsAssignableFrom(filterType))
                    throw new ArgumentException("ContentFilters must implement IBlogPostContentFilter.");

                // determine the name from the custom attribute
                BlogPostContentFilterAttribute[] blogClientAttributes = (BlogPostContentFilterAttribute[])filterType.GetCustomAttributes(typeof(BlogPostContentFilterAttribute), false);
                if (blogClientAttributes.Length != 1)
                    throw new ArgumentException("You must provide a single BlogPostContentFilterAttribute for all registered blog post content filter types.");
                _name = blogClientAttributes[0].TypeName;

                // get the constructor used for creation
                _constructor = filterType.GetConstructor(new Type[] { });
                if (_constructor == null)
                    throw new ArgumentException("You must implement a public constructor with no arguments for all registered blog post content filter types.");

                // record the type
                _type = filterType;
            }

            public string Name
            {
                get { return _name; }
            }
            private string _name;

            public ConstructorInfo Constructor
            {
                get { return _constructor; }
            }
            private ConstructorInfo _constructor;

            public Type Type
            {
                get { return _type; }
            }
            private Type _type;
        }
    }
}
