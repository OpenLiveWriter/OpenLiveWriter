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

namespace OpenLiveWriter.BlogClient.Clients
{
    [BlogClient("MovableType", "Movable Type")]
    public class MovableTypeClient : MetaweblogClient
    {
        public MovableTypeClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
        }

        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsCategoryIds = true;
            clientOptions.SupportsCategoriesInline = false;
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsCommentPolicy = true;
            clientOptions.SupportsPingPolicy = true;
            clientOptions.SupportsTrackbacks = true;
            clientOptions.SupportsKeywords = true;
            clientOptions.SupportsExcerpt = true;
            clientOptions.SupportsExtendedEntries = true;
        }

    }
}

