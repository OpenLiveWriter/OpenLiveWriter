// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Api;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient
{

    /// <summary>
    /// Class which provides post-formatting instructions to a blog-client. Note that
    /// the information conveyed in this class should only be optional overrides of
    /// default behavior, since in many cases blog-clients are created without the
    /// context required to pass them PostFormatOptions (such as during detection).
    /// In this case the client is passed PostFormatOptions.Unknown (see below).
    /// Properties within this class should always default to null or String.Empty
    /// and only contain values to override standard behavior.
    ///
    /// The expected source for optional behavioral overrides is the updatable
    /// BlogProviders.xml file. We therefore provide a convenience constructor
    /// for the class which takes an IBlogProviderDescription.
    /// </summary>
    public class PostFormatOptions
    {
        public static PostFormatOptions Unknown = new PostFormatOptions();

        private PostFormatOptions()
        {
        }

        public PostFormatOptions(IBlogProviderOptions blogProviderOptions)
        {
            DateFormatOverride = blogProviderOptions.PostDateFormat ;
            SupportsCustomDate = blogProviderOptions.SupportsCustomDate ;
            UseLocalTime = blogProviderOptions.UseLocalTime ;
            UnescapedTitles = blogProviderOptions.UnescapedTitles ;
            AllowPostAsDraft = blogProviderOptions.SupportsPostAsDraft != SupportsFeature.No;
            ContentFilter = blogProviderOptions.ContentFilter;
        }

        public string DateFormatOverride
        {
            get { return _dateFormatOverride; }
            set { _dateFormatOverride = value; }
        }
        private string _dateFormatOverride = String.Empty;

        public SupportsFeature SupportsCustomDate
        {
            get { return _supportsCustomDate; }
            set { _supportsCustomDate = value; }
        }
        private SupportsFeature _supportsCustomDate = SupportsFeature.Unknown;

        public bool AllowPostAsDraft
        {
            get { return _allowPostAsDraft; }
            set { _allowPostAsDraft = value; }
        }
        private bool _allowPostAsDraft = true;

        public bool UseLocalTime
        {
            get { return _useLocalTime; }
            set { _useLocalTime = value; }
        }
        private bool _useLocalTime = false;

        public bool UnescapedTitles
        {
            get { return _unescapedTitles; }
            set { _unescapedTitles = value; }
        }
        private bool _unescapedTitles = false;

        public string ContentFilter
        {
            get { return _contentFilter; }
            set { _contentFilter = value; }
        }
        private string _contentFilter = String.Empty;
    }
}
