// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Xml;
using System.Drawing;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public interface IBlogProvider : IBlogProviderDescription
    {
        bool Visible { get; }

        bool IsProviderForHomepageUrl(string homepageUrl);
        bool IsProviderForHomepageContent(string homepageContent);

        BlogAccount DetectAccountFromRsdHomepageLink(RsdServiceDescription rsdServiceDescription);
        BlogAccount DetectAccountFromRsdEngineName(RsdServiceDescription rsdServiceDescription);

        MessageId DisplayMessageForProviderError(string faultCode, string faultString);

        IBlogClientOptions ConstructBlogOptions(IBlogClientOptions defaultOptions);
    }

    public interface IBlogProviderDescription
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string Link { get; }
        string ClientType { get; }
        string PostApiUrl { get; }
        StringId PostApiUrlLabel { get; }
        /// <summary>
        /// This is the appid for a provider to be configured
        /// with the What's New feed.
        /// </summary>
        string AppId { get; }
    }

    /// <summary>
    /// This is a generic way of describing an account(blog, email, etc...)
    /// </summary>
    public interface IEditorAccount : IDisposable
    {
        string HomepageBaseUrl { get; }
        string Id { get; }
        string Name { get; }
        string HomepageUrl { get; }
        string ServiceName { get; }
        string ProviderId { get; }
        IEditorOptions EditorOptions { get; }
    }

    /// <summary>
    /// These are options that are needed by the ContentEditor and other things hanging off of it.
    /// These should be generic enough to apply to blog as well as other html targets.
    /// </summary>
    public interface IEditorOptions
    {
        bool RequiresXHTML { get; }
        int MaxPostTitleLength { get; }
        bool HasRTLFeatures { get; } // Controls whether or not RTL buttons should appear on the toolbar
        bool IsRTLTemplate { get; } // Controls the default direction of text
        SupportsFeature SupportsImageUpload { get; }
        SupportsFeature SupportsScripts { get; }
        SupportsFeature SupportsEmbeds { get; }
        int PostBodyBackgroundColor { get; }
        string DhtmlImageViewer { get; }
    }

    /// <summary>
    /// These are options that specific to blogs and are consumed by BlogPostHtmlEditor
    /// </summary>
    public interface IBlogClientOptions : IEditorOptions
    {
        ///
        /// Weblog
        ///
        string HomepageLinkText { get; }
        string AdminLinkText { get; }
        string AdminUrl { get; }
        string PostEditingUrl { get; }
        string PostEditingUrlPostIdPattern { get; }
        string ServiceName { get; }

        // Atom-specific
        string ImagePostingUrl { get; }

        ///
        /// Capabilities
        ///
        bool SupportsHttps { get; }
        bool SupportsCategories { get; }
        bool SupportsMultipleCategories { get; }
        bool SupportsCategoriesInline { get; }
        bool SupportsHierarchicalCategories { get; }
        bool SupportsNewCategories { get; }
        bool SupportsNewCategoriesInline { get; }
        bool SupportsSuggestCategories { get; }
        string CategoryScheme { get; }
        bool SupportsCommentPolicy { get; }
        bool SupportsPingPolicy { get; }
        bool SupportsTrackbacks { get; }
        bool SupportsKeywords { get; }
        bool SupportsGetKeywords { get; }
        bool SupportsExcerpt { get; }
        bool SupportsExtendedEntries { get; }
        bool SupportsFileUpload { get; }
        bool SupportsPages { get; }
        bool SupportsPageParent { get; }
        bool SupportsPageOrder { get; }
        bool SupportsPageTrackbacks { get; }
        bool SupportsSlug { get; }
        bool SupportsPassword { get; }
        bool SupportsAuthor { get; }
        bool SupportsCustomDate { get; }
        /// <summary>
        /// Some blog providers let you change the custom date when
        /// posting a new entry, but not when you're updating an existing
        /// entry.
        /// </summary>
        bool SupportsCustomDateUpdate { get; }
        bool SupportsPostAsDraft { get; }
        bool SupportsPostSynchronization { get; }
        bool SupportsAutoUpdate { get; }

        /// Fault-matching
        string InvalidPostIdFaultCodePattern { get; }
        string InvalidPostIdFaultStringPattern { get; }

        ///
        /// Hacks
        ///
        string CharacterSet { get; }
        bool UseLocalTime { get; }
        string PostDateFormat { get; }
        string FileUploadNameFormat { get; }
        bool RequiresHtmlTitles { get; }
        bool LinkToSkyDriveSelfPage { get; }
        /// <summary>
        /// This is for CommunityServer who accepts plaintext titles but returns HTML.
        /// Set to SupportsFeature.Unknown to just reflect the behavior of RequiresHtmlTitles.
        /// </summary>
        SupportsFeature ReturnsHtmlTitlesOnGet { get; }
        bool SupportsEmptyTitles { get; }
        string ContentFilter { get; }
        string PermalinkFormat { get; }
        int MaxRecentPosts { get; }
        int MaxCategoryNameLength { get; }
        bool CommentPolicyAsBoolean { get; }
        int AllowPolicyFalseValue { get; }
        TrackbackDelimiter TrackbackDelimiter { get; }
        bool FuturePublishDateWarning { get; }
        bool UseSpacesTemplateHack { get; }
        bool UseSpacesCIDMemberName { get; }
        bool UsePicasaImgMaxAlways { get; }
        bool UsePicasaS1600h { get; }
        bool KeywordsAsTags { get; }

        ///
        ///  Views
        ///
        string DefaultView { get; }
    }

    public enum TrackbackDelimiter
    {
        ArrayElement,
        Space,
        Comma
    }
}

