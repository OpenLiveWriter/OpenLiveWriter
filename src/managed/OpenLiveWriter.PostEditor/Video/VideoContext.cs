// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using mshtml;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for VideoContext.
    /// </summary>
    public class VideoContext
    {
        private SupportsFeature _embeddable = SupportsFeature.Unknown;
        private SupportsFeature _scriptable = SupportsFeature.Unknown;
        private string _blogProviderId = String.Empty;
        private IPublishingContext _context;

        public VideoContext()
        {

        }

        public VideoContext(IPublishingContext context)
        {
            _embeddable = context.SupportsEmbeds;
            _scriptable = context.SupportsScripts;
            _blogProviderId = ((BlogPostHtmlEditor)context).ProviderId;
            _context = context;
        }

        public IHTMLElement GetElement(string contentId)
        {
            return ((BlogPostHtmlEditor)_context).GetSmartContentElement(VideoContentSource.ID, contentId);
        }

        public string BlogProviderId { get { return _blogProviderId; } }

        //logic for choosing the player style based on blog capabilities
        public VideoPlayerStyle DetermineAppropriatePlayer(bool haveLink)
        {
            if (!haveLink)
                return VideoPlayerStyle.EmbeddedInPage;
            if (True(_embeddable) & True(_scriptable))
                return VideoPlayerStyle.Automatic;
            if (True(_embeddable))
                return VideoPlayerStyle.EmbeddedInPage;
            if (False(_scriptable))
                return VideoPlayerStyle.PreviewWithLink;
            return VideoPlayerStyle.Automatic;
        }

        private bool True(SupportsFeature test)
        {
            return test == SupportsFeature.Yes;
        }

        private bool False(SupportsFeature test)
        {
            return test == SupportsFeature.No;
        }
    }
}
