// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.BlogClient.Providers
{
    public delegate string OptionReader(string optionName);

    public class BlogClientOptions : IBlogClientOptions
    {
        public BlogClientOptions()
        {
            // accept default options (see private data declaratoins for defaults)
        }

        public bool SupportsHttps
        {
            get { return _supportsHttps; }
            set { _supportsHttps = value; }
        }

        public bool SupportsCategories
        {
            get { return _supportsCategories; }
            set { _supportsCategories = value; }
        }

        public bool SupportsMultipleCategories
        {
            get { return _supportsMultipleCategories; }
            set { _supportsMultipleCategories = value; }
        }

        public bool SupportsHierarchicalCategories
        {
            get { return _supportsHeirarchicalCategories; }
            set { _supportsHeirarchicalCategories = value; }
        }

        public bool SupportsNewCategories
        {
            get { return _supportsNewCategories; }
            set { _supportsNewCategories = value; }
        }

        public bool SupportsNewCategoriesInline
        {
            get { return _supportsNewCategoriesInline; }
            set { _supportsNewCategoriesInline = value; }
        }

        public bool SupportsSuggestCategories
        {
            get { return _supportsSuggestCategories; }
            set { _supportsSuggestCategories = value; }
        }

        public bool SupportsCategoriesInline
        {
            get { return _supportsCategoriesInline; }
            set { _supportsCategoriesInline = value; }
        }

        public bool SupportsCategoryIds
        {
            get { return _supportsCategoryIds; }
            set { _supportsCategoryIds = value; }
        }

        public string CategoryScheme
        {
            get { return _categoryScheme; }
            set { _categoryScheme = value; }
        }

        public bool SupportsCommentPolicy
        {
            get { return _supportsCommentPolicy; }
            set { _supportsCommentPolicy = value; }
        }

        public bool SupportsPingPolicy
        {
            get { return _supportsPingPolicy; }
            set { _supportsPingPolicy = value; }
        }

        public bool SupportsTrackbacks
        {
            get { return _supportsTrackbacks; }
            set { _supportsTrackbacks = value; }
        }

        public bool SupportsKeywords
        {
            get { return _supportsKeywords; }
            set { _supportsKeywords = value; }
        }

        public bool SupportsGetKeywords
        {
            get { return _supportsGetKeywords; }
            set { _supportsGetKeywords = value; }
        }

        public bool SupportsExcerpt
        {
            get { return _supportsExcerpt; }
            set { _supportsExcerpt = value; }
        }

        public bool SupportsExtendedEntries
        {
            get { return _supportsExtendedEntries; }
            set { _supportsExtendedEntries = value; }
        }

        public bool SupportsFileUpload
        {
            get { return _supportsFileUpload; }
            set { _supportsFileUpload = value; }
        }

        public bool SupportsPages
        {
            get { return _supportsPages; }
            set { _supportsPages = value; }
        }

        public bool SupportsPageParent
        {
            get { return _supportsPageParent; }
            set { _supportsPageParent = value; }
        }

        public bool SupportsPageOrder
        {
            get { return _supportsPageOrder; }
            set { _supportsPageOrder = value; }
        }

        public bool SupportsPageTrackbacks
        {
            get { return _supportsPageTrackbacks; }
            set { _supportsPageTrackbacks = value; }
        }

        public bool SupportsSlug
        {
            get { return _supportsSlug; }
            set { _supportsSlug = value; }
        }

        public bool SupportsPassword
        {
            get { return _supportsPassword; }
            set { _supportsPassword = value; }
        }

        public bool SupportsAuthor
        {
            get { return _supportsAuthor; }
            set { _supportsAuthor = value; }
        }

        public SupportsFeature SupportsScripts
        {
            get { return _supportsScripts; }
            set { _supportsScripts = value; }
        }

        public SupportsFeature SupportsEmbeds
        {
            get { return _supportsEmbeds; }
            set { _supportsEmbeds = value; }
        }

        // @SharedCanvas - not used yet, make sure this makes sense when it used, and the value is read from anywhere it needs to
        public SupportsFeature SupportsImageUpload
        {
            get { return _supportsImageUpload; }
            set { _supportsImageUpload = value; }
        }

        public bool SupportsCustomDate
        {
            get { return _supportsCustomDate; }
            set { _supportsCustomDate = value; }
        }

        public bool SupportsCustomDateUpdate
        {
            get { return _supportsCustomDateUpdate; }
            set { _supportsCustomDateUpdate = value; }
        }

        public bool SupportsPostAsDraft
        {
            get { return _supportsPostAsDraft; }
            set { _supportsPostAsDraft = value; }
        }

        public bool CommentPolicyAsBoolean
        {
            get { return _commentPolicyAsBoolean; }
            set { _commentPolicyAsBoolean = value; }
        }

        public int AllowPolicyFalseValue
        {
            get { return _allowPolicyFalseValue; }
            set { _allowPolicyFalseValue = value; }
        }

        public TrackbackDelimiter TrackbackDelimiter
        {
            get { return _trackbackDelimiter; }
            set { _trackbackDelimiter = value; }
        }

        public bool FuturePublishDateWarning
        {
            get { return _futurePublishDateWarning; }
            set { _futurePublishDateWarning = value; }
        }

        public bool UseSpacesTemplateHack
        {
            get { return _useSpacesTemplateHack; }
            set { _useSpacesTemplateHack = value; }
        }

        public bool UseSpacesCIDMemberName
        {
            get { return _supportsSpacesCIDUrls; }
            set { _supportsSpacesCIDUrls = value; }
        }

        public string PostDateFormat
        {
            get { return _postDateFormat; }
            set { _postDateFormat = value; }
        }

        public string FileUploadNameFormat
        {
            get { return _fileUploadFormat; }
            set { _fileUploadFormat = value; }
        }

        public bool UseLocalTime
        {
            get { return _useLocalTime; }
            set { _useLocalTime = value; }
        }

        public string CharacterSet
        {
            get { return _characterSet; }
            set { _characterSet = value; }
        }

        public bool RequiresXHTML
        {
            get { return _requiresXHTML; }
            set { _requiresXHTML = value; }
        }

        public string DhtmlImageViewer
        {
            get { return _dhtmlImageViewer; }
            set { _dhtmlImageViewer = value; }
        }

        public int PostBodyBackgroundColor
        {
            get { return _postBodyBackgroundColor; }
            set { _postBodyBackgroundColor = value; }
        }

        public string ContentFilter
        {
            get { return _contentFilter; }
            set { _contentFilter = value; }
        }

        public string PermalinkFormat
        {
            get { return _permalinkFormat; }
            set { _permalinkFormat = value; }
        }

        public string HomepageLinkText
        {
            get { return _homepageLinkText; }
            set { _homepageLinkText = value; }
        }

        public string AdminLinkText
        {
            get { return _adminLinkText; }
            set { _adminLinkText = value; }
        }

        public string AdminUrl
        {
            get { return _adminUrl; }
            set { _adminUrl = value; }
        }

        public string PostEditingUrl
        {
            get { return _postEditingUrl; }
            set { _postEditingUrl = value; }
        }

        public string PostEditingUrlPostIdPattern
        {
            get { return _postEditingUrlPostIdPattern; }
            set { _postEditingUrlPostIdPattern = value; }
        }

        public string ImagePostingUrl
        {
            get { return _imagePostingUrl; }
            set { _imagePostingUrl = value; }
        }

        public bool SupportsPostSynchronization
        {
            get { return _postSynchronizationEnabled; }
            set { _postSynchronizationEnabled = value; }
        }

        public bool SupportsAutoUpdate
        {
            get { return _supportsAutoUpdate; }
            set { _supportsAutoUpdate = value; }
        }

        public bool HasRTLFeatures
        {
            get { return IsRTLTemplate; }
        }

        public bool IsRTLTemplate
        {
            get { return _templateIsRTL; }
            set { _templateIsRTL = value; }
        }

        public string DefaultView
        {
            get { return _defaultView; }
            set { _defaultView = value; }
        }

        public bool RequiresHtmlTitles
        {
            get { return _requiresHtmlTitles; }
            set { _requiresHtmlTitles = value; }
        }

        public bool LinkToSkyDriveSelfPage
        {
            get { return _linkToSkyDriveSelfPage; }
            set { _linkToSkyDriveSelfPage = value; }
        }

        public SupportsFeature ReturnsHtmlTitlesOnGet
        {
            get { return _returnsHtmlTitlesOnGet; }
            set { _returnsHtmlTitlesOnGet = value; }
        }

        public bool SupportsEmptyTitles
        {
            get { return _supportsEmptyTitles; }
            set { _supportsEmptyTitles = value; }
        }

        public int MaxRecentPosts
        {
            get { return _maxRecentPosts; }
            set { _maxRecentPosts = value; }
        }

        public int MaxCategoryNameLength
        {
            get { return _maxCategoryNameLength; }
            set { _maxCategoryNameLength = value; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        public string InvalidPostIdFaultCodePattern
        {
            get { return _invalidPostIdFaultCodePattern; }
            set { _invalidPostIdFaultCodePattern = value; }
        }

        public string InvalidPostIdFaultStringPattern
        {
            get { return _invalidPostIdFaultStringPattern; }
            set { _invalidPostIdFaultStringPattern = value; }
        }

        public bool UsePicasaImgMaxAlways
        {
            get { return usePicasaImgMaxAlways; }
            set { usePicasaImgMaxAlways = value; }
        }

        public bool UsePicasaS1600h
        {
            get { return usePicasaS1600h; }
            set { usePicasaS1600h = value; }
        }

        public bool KeywordsAsTags
        {
            get { return _keywordsAsTags; }
            set { _keywordsAsTags = value; }
        }

        public int MaxPostTitleLength
        {
            get { return _maxPostTitleLength > 0 ? _maxPostTitleLength : int.MaxValue; }
            set { _maxPostTitleLength = value; }
        }

        public const string CHARACTER_SET = "characterSet";
        public const string SUPPORTS_EMBEDS = "supportsEmbeds";
        public const string SUPPORTS_SCRIPTS = "supportsScripts";
        public const string REQUIRES_XHTML = "requiresXHTML";
        public const string TEMPLATE_IS_RTL = "templateIsRTL";
        public const string DHTML_IMAGE_VIEWER = "dhtmlImageViewer";
        public const string IMAGE_ENDPOINT = "imageEndpoint";
        public const string SUPPORTS_NEW_CATEGORIES = "supportsNewCategories";
        public const string CATEGORY_SCHEME = "categoryScheme";
        public const string SUPPORTS_CATEGORIES = "supportsCategories";
        public const string POST_BODY_BACKGROUND_COLOR = "postBodyBackgroundColor";

        public static IBlogClientOptions ApplyOptionOverrides(OptionReader optionReader, IBlogClientOptions existingOptions)
        {
            return ApplyOptionOverrides(optionReader, existingOptions, true);
        }

        public static IBlogClientOptions ApplyOptionOverrides(OptionReader optionReader, IBlogClientOptions existingOptions, bool includeIrregularities)
        {
            BlogClientOptions clientOptions = new BlogClientOptions();

            // Protocol capabilities
            clientOptions.SupportsPostAsDraft = ReadBool(optionReader("supportsPostAsDraft"), existingOptions.SupportsPostAsDraft);
            clientOptions.SupportsFileUpload = ReadBool(optionReader("supportsFileUpload"), existingOptions.SupportsFileUpload);
            clientOptions.SupportsExtendedEntries = ReadBool(optionReader("supportsExtendedEntries"), existingOptions.SupportsExtendedEntries);
            clientOptions.SupportsCustomDate = ReadBool(optionReader("supportsCustomDate"), existingOptions.SupportsCustomDate);
            clientOptions.SupportsCustomDateUpdate = ReadBool(optionReader("supportsCustomDateUpdate"), existingOptions.SupportsCustomDateUpdate);
            clientOptions.SupportsHttps = ReadBool(optionReader("supportsHttps"), existingOptions.SupportsHttps);
            clientOptions.SupportsCategories = ReadBool(optionReader(SUPPORTS_CATEGORIES), existingOptions.SupportsCategories);
            clientOptions.SupportsCategoriesInline = ReadBool(optionReader("supportsCategoriesInline"), existingOptions.SupportsCategoriesInline);
            clientOptions.SupportsMultipleCategories = ReadBool(optionReader("supportsMultipleCategories"), existingOptions.SupportsMultipleCategories);
            clientOptions.SupportsHierarchicalCategories = ReadBool(optionReader("supportsHierarchicalCategories"), existingOptions.SupportsHierarchicalCategories);
            clientOptions.SupportsNewCategories = ReadBool(optionReader(SUPPORTS_NEW_CATEGORIES), existingOptions.SupportsNewCategories);
            clientOptions.SupportsNewCategoriesInline = ReadBool(optionReader("supportsNewCategoriesInline"), existingOptions.SupportsNewCategoriesInline);
            clientOptions.SupportsSuggestCategories = ReadBool(optionReader("supportsSuggestCategories"), existingOptions.SupportsSuggestCategories);
            clientOptions.CategoryScheme = ReadText(optionReader(CATEGORY_SCHEME), existingOptions.CategoryScheme);
            clientOptions.SupportsKeywords = ReadBool(optionReader("supportsKeywords"), existingOptions.SupportsKeywords);
            clientOptions.SupportsGetKeywords = ReadBool(optionReader("supportsGetTags"), existingOptions.SupportsGetKeywords);
            clientOptions.SupportsCommentPolicy = ReadBool(optionReader("supportsCommentPolicy"), existingOptions.SupportsCommentPolicy);
            clientOptions.SupportsPingPolicy = ReadBool(optionReader("supportsPingPolicy"), existingOptions.SupportsPingPolicy);
            clientOptions.SupportsAuthor = ReadBool(optionReader("supportsAuthor"), existingOptions.SupportsAuthor);
            clientOptions.SupportsSlug = ReadBool(optionReader("supportsSlug"), existingOptions.SupportsSlug);
            clientOptions.SupportsPassword = ReadBool(optionReader("supportsPassword"), existingOptions.SupportsPassword);
            clientOptions.SupportsExcerpt = ReadBool(optionReader("supportsExcerpt"), existingOptions.SupportsExcerpt);
            clientOptions.SupportsTrackbacks = ReadBool(optionReader("supportsTrackbacks"), existingOptions.SupportsTrackbacks);
            clientOptions.SupportsPages = ReadBool(optionReader("supportsPages"), existingOptions.SupportsPages);
            clientOptions.SupportsPageParent = ReadBool(optionReader("supportsPageParent"), existingOptions.SupportsPageParent);
            clientOptions.SupportsPageOrder = ReadBool(optionReader("supportsPageOrder"), existingOptions.SupportsPageOrder);
            clientOptions.SupportsPageTrackbacks = ReadBool(optionReader("supportsPageTrackbacks"), existingOptions.SupportsPageTrackbacks);

            // Writer capabilities
            clientOptions.LinkToSkyDriveSelfPage = ReadBool(optionReader("linkToSkyDriveSelfPage"), existingOptions.LinkToSkyDriveSelfPage);
            clientOptions.RequiresHtmlTitles = ReadBool(optionReader("requiresHtmlTitles"), existingOptions.RequiresHtmlTitles);
            clientOptions.ReturnsHtmlTitlesOnGet = ReadSupportsFeature(optionReader("returnsHtmlTitlesOnGet"), existingOptions.ReturnsHtmlTitlesOnGet);
            clientOptions.SupportsEmptyTitles = ReadBool(optionReader("supportsEmptyTitles"), existingOptions.SupportsEmptyTitles);
            clientOptions.SupportsScripts = ReadSupportsFeature(optionReader(SUPPORTS_SCRIPTS), existingOptions.SupportsScripts);
            clientOptions.SupportsEmbeds = ReadSupportsFeature(optionReader(SUPPORTS_EMBEDS), existingOptions.SupportsEmbeds);
            clientOptions.SupportsImageUpload = ReadSupportsFeature(optionReader("supportsImageUpload"), existingOptions.SupportsImageUpload);
            clientOptions.DefaultView = ReadText(optionReader("defaultView"), existingOptions.DefaultView);
            clientOptions.CharacterSet = ReadText(optionReader(CHARACTER_SET), existingOptions.CharacterSet);
            clientOptions.RequiresXHTML = ReadBool(optionReader(REQUIRES_XHTML), existingOptions.RequiresXHTML);
            clientOptions.DhtmlImageViewer = ReadText(optionReader(DHTML_IMAGE_VIEWER), existingOptions.DhtmlImageViewer);
            clientOptions.PostBodyBackgroundColor = ReadInt(optionReader(POST_BODY_BACKGROUND_COLOR), existingOptions.PostBodyBackgroundColor);
            clientOptions.MaxCategoryNameLength = ReadInt(optionReader("maxCategoryNameLength"), existingOptions.MaxCategoryNameLength);
            clientOptions.SupportsAutoUpdate = ReadBool(optionReader("supportsAutoUpdate"), existingOptions.SupportsAutoUpdate);
            clientOptions.InvalidPostIdFaultCodePattern = ReadText(optionReader("invalidPostIdFaultCodePattern"), existingOptions.InvalidPostIdFaultCodePattern);
            clientOptions.InvalidPostIdFaultStringPattern = ReadText(optionReader("invalidPostIdFaultStringPattern"), existingOptions.InvalidPostIdFaultStringPattern);
            clientOptions.IsRTLTemplate = ReadBool(optionReader("templateIsRTL"), existingOptions.IsRTLTemplate);
            clientOptions.MaxPostTitleLength = ReadInt(optionReader("maxPostTitleLength"), existingOptions.MaxPostTitleLength);

            // Weblog
            clientOptions.HomepageLinkText = ReadText(optionReader("homepageLinkText"), existingOptions.HomepageLinkText);
            clientOptions.AdminLinkText = ReadText(optionReader("adminLinkText"), existingOptions.AdminLinkText);
            clientOptions.AdminUrl = ReadText(optionReader("adminUrl"), existingOptions.AdminUrl);
            clientOptions.PostEditingUrl = ReadText(optionReader("postEditingUrl"), existingOptions.PostEditingUrl);
            clientOptions.PostEditingUrlPostIdPattern = ReadText(optionReader("postEditingUrlPostIdPattern"), existingOptions.PostEditingUrlPostIdPattern);
            clientOptions.ImagePostingUrl = ReadText(optionReader(IMAGE_ENDPOINT), existingOptions.ImagePostingUrl);
            clientOptions.ServiceName = ReadText(optionReader("serviceName"), existingOptions.ServiceName);

            // Irregularities
            if (includeIrregularities)
            {
                clientOptions.CommentPolicyAsBoolean = ReadBool(optionReader("commentPolicyAsBoolean"), existingOptions.CommentPolicyAsBoolean);
                clientOptions.AllowPolicyFalseValue = ReadInt(optionReader("allowPolicyFalseValue"), existingOptions.AllowPolicyFalseValue);
                clientOptions.MaxRecentPosts = ReadInt(optionReader("maxRecentPosts"), existingOptions.MaxRecentPosts);
                clientOptions.ContentFilter = ReadText(optionReader("contentFilter"), existingOptions.ContentFilter);
                clientOptions.PermalinkFormat = ReadText(optionReader("permalinkFormat"), existingOptions.PermalinkFormat);
                clientOptions.PostDateFormat = ReadText(optionReader("postDateFormat"), existingOptions.PostDateFormat);
                clientOptions.FileUploadNameFormat = ReadText(optionReader("fileUploadNameFormat"), existingOptions.FileUploadNameFormat);
                clientOptions.UseLocalTime = ReadBool(optionReader("useLocalTime"), existingOptions.UseLocalTime);
                clientOptions.SupportsPostSynchronization = ReadBool(optionReader("supportsPostSynchronization"), existingOptions.SupportsPostSynchronization);
                clientOptions.TrackbackDelimiter = ReadTrackbackDelimiter(optionReader("trackbackDelimiter"), existingOptions.TrackbackDelimiter);
                clientOptions.FuturePublishDateWarning = ReadBool(optionReader("futurePublishDateWarning"), existingOptions.FuturePublishDateWarning);
                clientOptions.UsePicasaImgMaxAlways = ReadBool(optionReader("usePicasaImgMaxAlways"), existingOptions.UsePicasaImgMaxAlways);
                clientOptions.UsePicasaS1600h = ReadBool(optionReader("usePicasaS1600h"), existingOptions.UsePicasaS1600h);
                clientOptions.KeywordsAsTags = ReadBool(optionReader("keywordsAsTags"), existingOptions.KeywordsAsTags);
            }
            else
            {
                clientOptions.CommentPolicyAsBoolean = existingOptions.CommentPolicyAsBoolean;
                clientOptions.AllowPolicyFalseValue = existingOptions.AllowPolicyFalseValue;
                clientOptions.MaxRecentPosts = existingOptions.MaxRecentPosts;
                clientOptions.ContentFilter = existingOptions.ContentFilter;
                clientOptions.PermalinkFormat = existingOptions.PermalinkFormat;
                clientOptions.PostDateFormat = existingOptions.PostDateFormat;
                clientOptions.FileUploadNameFormat = existingOptions.FileUploadNameFormat;
                clientOptions.UseLocalTime = existingOptions.UseLocalTime;
                clientOptions.SupportsPostSynchronization = existingOptions.SupportsPostSynchronization;
                clientOptions.TrackbackDelimiter = existingOptions.TrackbackDelimiter;
                clientOptions.FuturePublishDateWarning = existingOptions.FuturePublishDateWarning;
                clientOptions.UsePicasaImgMaxAlways = existingOptions.UsePicasaImgMaxAlways;
                clientOptions.UsePicasaS1600h = existingOptions.UsePicasaS1600h;
                clientOptions.KeywordsAsTags = existingOptions.KeywordsAsTags;
            }

            // return options
            return clientOptions;
        }

        public static void ShowInNotepad(IBlogClientOptions options)
        {
            string clientOptionsFile = TempFileManager.Instance.CreateTempFile("BlogClientOptions.txt");
            using (StreamWriter stream = new StreamWriter(clientOptionsFile))
            {
                OptionStreamWriter writer = new OptionStreamWriter(stream);
                writer.Write(options);
            }
            Process.Start(clientOptionsFile);
        }

        private class OptionStreamWriter : DisplayableBlogClientOptionWriter
        {
            public OptionStreamWriter(StreamWriter output)
            {
                _output = output;
            }

            protected override void WriteOption(string name, string value)
            {
                _output.WriteLine("{0,-30}     {1}", name, value);
            }

            private StreamWriter _output;
        }

        private static string ReadText(string textValue, string defaultValue)
        {
            if (textValue != null)
            {
                return textValue.Trim();

            }
            else
                return defaultValue;
        }

        internal static bool ReadBool(string boolValue, bool defaultValue)
        {
            return StringHelper.ToBool(boolValue, defaultValue);
        }

        private static TrackbackDelimiter ReadTrackbackDelimiter(string delimiterValue, TrackbackDelimiter defaultValue)
        {
            if (delimiterValue != null)
            {
                switch (delimiterValue.Trim().ToUpperInvariant())
                {
                    case "ARRAYELEMENT":
                        return TrackbackDelimiter.ArrayElement;
                    case "SPACE":
                        return TrackbackDelimiter.Space;
                    case "COMMA":
                        return TrackbackDelimiter.Comma;
                }
            }
            return defaultValue;
        }

        private static int ReadInt(string intValue, int defaultValue)
        {
            if (intValue != null)
            {
                try
                {
                    return int.Parse(intValue, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Trace.Fail("Error parsing int string \"" + intValue + ": " + e.ToString());
                }
            }
            return defaultValue;
        }

        private static SupportsFeature ReadSupportsFeature(string supportsFeatureValue, SupportsFeature defaultValue)
        {
            if (supportsFeatureValue != null)
            {
                switch (supportsFeatureValue.Trim().ToUpperInvariant())
                {
                    case "UNKNOWN":
                        return SupportsFeature.Unknown;
                    case "YES":
                        return SupportsFeature.Yes;
                    case "NO":
                        return SupportsFeature.No;
                    default:
                        throw new ArgumentException("Invalid value for SupportsFeature: " + supportsFeatureValue);
                }
            }
            else
            {
                return defaultValue;
            }
        }

        private bool _supportsHttps = false;
        private bool _supportsCategories = false;
        private bool _supportsMultipleCategories = false;
        private bool _supportsHeirarchicalCategories = false;
        private bool _supportsNewCategories = false;
        private bool _supportsNewCategoriesInline = false;
        private bool _supportsSuggestCategories = false;
        private bool _supportsCategoriesInline = true;
        private bool _supportsCategoryIds = false;
        private bool _supportsCommentPolicy = false;
        private string _categoryScheme = null;
        private bool _supportsPingPolicy = false;
        private bool _supportsTrackbacks = false;
        private bool _supportsKeywords = false;
        private bool _supportsGetKeywords = false;
        private bool _supportsExcerpt = false;
        private bool _supportsExtendedEntries = false;
        private bool _supportsFileUpload = false;

        private bool _supportsPages = false;
        private bool _supportsPageParent = false;
        private bool _supportsPageOrder = false;
        private bool _supportsPageTrackbacks = false;
        private bool _supportsSlug = false;
        private bool _supportsPassword = false;
        private bool _supportsAuthor = false;
        private bool _supportsCustomDate = true;
        private bool _supportsCustomDateUpdate = true;
        private bool _supportsPostAsDraft = true;

        private SupportsFeature _supportsScripts = SupportsFeature.Unknown;
        private SupportsFeature _supportsEmbeds = SupportsFeature.Unknown;
        private SupportsFeature _supportsImageUpload = SupportsFeature.Yes;

        private bool _supportsAutoUpdate = true;
        private bool _templateIsRTL = false;

        private bool _commentPolicyAsBoolean = false;  // The "none" value for comments is supported by MT and TypePad but not WordPress
        private int _allowPolicyFalseValue = 0;
        private TrackbackDelimiter _trackbackDelimiter = TrackbackDelimiter.ArrayElement;
        private bool _futurePublishDateWarning = false;
        private bool _useSpacesTemplateHack = false;
        private bool _supportsSpacesCIDUrls = false;
        private string _postDateFormat = String.Empty;
        private string _fileUploadFormat = String.Empty;
        private bool _useLocalTime = false;
        private string _characterSet = String.Empty;
        private bool _requiresXHTML = false;
        private string _contentFilter = String.Empty;
        private bool _postSynchronizationEnabled = true;
        private bool _requiresHtmlTitles = true;
        private bool _linkToSkyDriveSelfPage = false;
        private SupportsFeature _returnsHtmlTitlesOnGet = SupportsFeature.Unknown;
        private bool _supportsEmptyTitles = true;
        private string _defaultView = String.Empty;
        private string _permalinkFormat = String.Empty;
        private string _homepageLinkText = String.Empty;
        private string _adminLinkText = String.Empty;
        private string _adminUrl = String.Empty;
        private string _postEditingUrl = String.Empty;
        private string _postEditingUrlPostIdPattern = String.Empty;
        private string _imagePostingUrl = String.Empty;
        private int _maxRecentPosts = -1;
        private int _maxCategoryNameLength = 0;
        private string _serviceName = String.Empty;
        private string _invalidPostIdFaultCodePattern = String.Empty;
        private string _invalidPostIdFaultStringPattern = String.Empty;
        private bool usePicasaImgMaxAlways = true;
        private bool usePicasaS1600h = true;
        private string _dhtmlImageViewer;
        private int _postBodyBackgroundColor = Color.Transparent.ToArgb();
        private bool _keywordsAsTags = false;
        private int _maxPostTitleLength;
    }

    public abstract class DisplayableBlogClientOptionWriter
    {
        public void Write(IBlogClientOptions clientOptions)
        {
            WriteOption(Res.Get(StringId.CapabilityPostDraftToServer), clientOptions.SupportsPostAsDraft);
            WriteOption(Res.Get(StringId.CapabilityFileUpload), clientOptions.SupportsFileUpload);
            WriteOption(Res.Get(StringId.CapabilityExtendedEntries), clientOptions.SupportsExtendedEntries);
            WriteOption(Res.Get(StringId.CapabilityCustomPublishDate), clientOptions.SupportsCustomDate);
            WriteOption(Res.Get(StringId.CapabilityCustomPublishDateUpdate), clientOptions.SupportsCustomDateUpdate);
            WriteOption(Res.Get(StringId.CapabilityCategories), clientOptions.SupportsCategories);
            WriteOption(Res.Get(StringId.CapabilityCategoriesInline), clientOptions.SupportsCategoriesInline);
            WriteOption(Res.Get(StringId.CapabilityMultipleCategories), clientOptions.SupportsMultipleCategories);
            WriteOption(Res.Get(StringId.CapabilityHierarchicalCategories), clientOptions.SupportsHierarchicalCategories);
            WriteOption(Res.Get(StringId.CapabilityNewCategories), clientOptions.SupportsNewCategories);
            WriteOption(Res.Get(StringId.CapabilityNewCategoriesInline), clientOptions.SupportsNewCategoriesInline);
            WriteOption(Res.Get(StringId.CapabilityCategoryScheme), clientOptions.CategoryScheme);
            WriteOption(Res.Get(StringId.CapabilityKeywords), clientOptions.SupportsKeywords);
            WriteOption(Res.Get(StringId.CapabilityKeywordRetrieval), clientOptions.SupportsGetKeywords);
            WriteOption(Res.Get(StringId.CapabilityCommentPolicy), clientOptions.SupportsCommentPolicy);
            WriteOption(Res.Get(StringId.CapabilityTrackbackPolicy), clientOptions.SupportsPingPolicy);
            WriteOption(Res.Get(StringId.CapabilityAuthor), clientOptions.SupportsAuthor);
            WriteOption(Res.Get(StringId.CapabilitySlug), clientOptions.SupportsSlug);
            WriteOption(Res.Get(StringId.CapabilityPassword), clientOptions.SupportsPassword);
            WriteOption(Res.Get(StringId.CapabilityExcerpt), clientOptions.SupportsExcerpt);
            WriteOption(Res.Get(StringId.CapabilitySendTrackbacks), clientOptions.SupportsTrackbacks);
            WriteOption(Res.Get(StringId.CapabilityPages), clientOptions.SupportsPages);
            WriteOption(Res.Get(StringId.CapabilityPageParent), clientOptions.SupportsPageParent);
            WriteOption(Res.Get(StringId.CapabilityPageOrder), clientOptions.SupportsPageOrder);
            WriteOption(Res.Get(StringId.CapabilityHtmlTitles), clientOptions.RequiresHtmlTitles);
            WriteOption(Res.Get(StringId.CapabilityEmptyTitles), clientOptions.SupportsEmptyTitles);
            WriteOption(Res.Get(StringId.CapabilityScripts), clientOptions.SupportsScripts);
            WriteOption(Res.Get(StringId.CapabilityEmbeds), clientOptions.SupportsEmbeds);

            string defaultView;
            switch (clientOptions.DefaultView)
            {
                case "WebLayout":
                    defaultView = Res.Get(StringId.CapabilityValueWebLayout);
                    break;
                case "WebPreview":
                    defaultView = Res.Get(StringId.CapabilityValueWebPreview);
                    break;
                case "Normal":
                case "":
                case null:
                    defaultView = Res.Get(StringId.CapabilityValueNormal);
                    break;
                default:
                    defaultView = clientOptions.DefaultView;
                    break;
            }
            WriteOption(Res.Get(StringId.CapabilityDefaultView), defaultView);

            WriteOption(Res.Get(StringId.CapabilityCharacterSet), clientOptions.CharacterSet != String.Empty ? clientOptions.CharacterSet : "UTF-8");
            WriteOption(Res.Get(StringId.CapabilityRequiresXHTML), clientOptions.RequiresXHTML);
            WriteOption(Res.Get(StringId.CapabilityTemplateIsRTL), clientOptions.IsRTLTemplate);
            WriteOption(Res.Get(StringId.CapabilityCategoryNameLimit), clientOptions.MaxCategoryNameLength != 0 ? clientOptions.MaxCategoryNameLength.ToString(CultureInfo.CurrentCulture) : Res.Get(StringId.CapabilityValueNoLimit));
            WriteOption(Res.Get(StringId.CapabilityAutoUpdate), clientOptions.SupportsAutoUpdate);
            WriteOption(Res.Get(StringId.CapabilityPostTitleLengthLimit), clientOptions.MaxPostTitleLength != int.MaxValue ? clientOptions.MaxPostTitleLength.ToString(CultureInfo.CurrentCulture) : Res.Get(StringId.Unknown));
        }

        private void WriteOption(string name, bool value)
        {
            WriteOption(name, value ? Res.Get(StringId.Yes) : Res.Get(StringId.No));
        }

        private void WriteOption(string name, SupportsFeature value)
        {
            switch (value)
            {
                case SupportsFeature.Yes:
                    WriteOption(name, Res.Get(StringId.Yes));
                    break;
                case SupportsFeature.No:
                    WriteOption(name, Res.Get(StringId.No));
                    break;
                case SupportsFeature.Unknown:
                    WriteOption(name, Res.Get(StringId.Unknown));
                    break;
            }
        }

        protected abstract void WriteOption(string name, string value);
    }

    public class BlogProviderDescription : IBlogProviderDescription
    {
        public BlogProviderDescription(
            string id, string name, string description, string link, string clientType, string postApiUrl, StringId postApiUrlDescription, string appid)
        {
            Init(id, name, description, link, clientType, postApiUrl, postApiUrlDescription, appid);
        }

        protected BlogProviderDescription()
        {
        }

        protected void Init(
            string id, string name, string description, string link, string clientType, string postApiUrl, StringId postApiUrlLabel, string appid)
        {
            _id = id;
            _name = name;
            _description = description;
            _link = link;
            _clientType = clientType;
            _postApiUrl = postApiUrl;
            _postApiUrlLabel = postApiUrlLabel;
            _appid = appid;
        }

        public string Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public string Link
        {
            get { return _link; }
        }

        public string ClientType
        {
            get { return _clientType; }
        }

        public string PostApiUrl
        {
            get { return _postApiUrl; }
        }

        public StringId PostApiUrlLabel
        {
            get { return _postApiUrlLabel; }
        }

        public string AppId
        {
            get { return _appid; }
        }

        private string _id;
        private string _name;
        private string _description;
        private string _link;
        private string _clientType;
        private string _postApiUrl;
        private StringId _postApiUrlLabel;
        private string _appid;
    }

    public class BlogProvider : BlogProviderDescription, IBlogProvider
    {
        protected BlogProvider()
        {
        }

        protected void Init(
            string id, string name, string description, string link, string clientType, string postApiUrl,
            StringId postApiUrlLabel,
            string appid,
            string homepageUrlPattern, string homepageContentPattern,
            RsdClientTypeMapping[] rsdClientTypeMappings, string rsdEngineNamePattern, string rsdHomepageLinkPattern,
            ProviderFault[] providerFaults,
            bool visible)
        {
            base.Init(id, name, description, link, clientType, postApiUrl, postApiUrlLabel, appid);
            _homepageUrlPattern = homepageUrlPattern;
            _homepageContentPattern = homepageContentPattern;
            _rsdClientTypeMappings = rsdClientTypeMappings.Clone() as RsdClientTypeMapping[];
            _rsdEngineNamePattern = rsdEngineNamePattern;
            _rsdHomepageLinkPattern = rsdHomepageLinkPattern;
            _providerFaults = providerFaults;
            _visible = visible;
        }

        public bool Visible
        {
            get { return _visible; }
        }

        virtual public bool IsProviderForHomepageUrl(string homepageUrl)
        {
            return IsMatch(HompepageUrlRegex, homepageUrl);
        }

        virtual public bool IsProviderForHomepageContent(string homepageContent)
        {
            return IsMatch(HomepageContentRegex, homepageContent);
        }

        virtual public MessageId DisplayMessageForProviderError(string faultCode, string faultString)
        {
            foreach (ProviderFault providerFault in _providerFaults)
            {
                if (providerFault.IsMatch(faultCode, faultString))
                    return providerFault.MessageId;
            }
            return MessageId.None;
        }

        virtual public BlogAccount DetectAccountFromRsdHomepageLink(RsdServiceDescription rsdServiceDescription)
        {
            if (IsMatch(RsdHomepageLinkRegex, rsdServiceDescription.HomepageLink))
            {
                return ScanForSupportedRsdApi(rsdServiceDescription);
            }
            else
            {
                return null;
            }
        }

        virtual public BlogAccount DetectAccountFromRsdEngineName(RsdServiceDescription rsdServiceDescription)
        {
            if (IsMatch(RsdEngineNameRegex, rsdServiceDescription.EngineName))
            {
                return ScanForSupportedRsdApi(rsdServiceDescription);
            }
            else
            {
                return null;
            }
        }

        private BlogAccount ScanForSupportedRsdApi(RsdServiceDescription rsdServiceDescription)
        {
            // initialize client type mappings (including mapping "implied" from ClientType itself
            ArrayList rsdClientTypeMappings = new ArrayList(_rsdClientTypeMappings);
            rsdClientTypeMappings.Add(new RsdClientTypeMapping(ClientType, ClientType));

            // scan for a match
            foreach (RsdClientTypeMapping mapping in rsdClientTypeMappings)
            {
                RsdApi rsdApi = rsdServiceDescription.ScanForApi(mapping.RsdClientType);
                if (rsdApi != null)
                {
                    // HACK: the internal spaces.msn-int site has a bug that causes it to return
                    // the production API URL, so force storage.msn.com to storage.msn-int.com
                    string postApiUrl = rsdApi.ApiLink;
                    if (rsdServiceDescription.HomepageLink.IndexOf("msn-int", StringComparison.OrdinalIgnoreCase) != -1)
                        postApiUrl = postApiUrl.Replace("storage.msn.com", "storage.msn-int.com");

                    return new BlogAccount(Name, mapping.ClientType, postApiUrl, rsdApi.BlogId);
                }
            }

            // no match
            return null;
        }

        public virtual IBlogClientOptions ConstructBlogOptions(IBlogClientOptions defaultOptions)
        {
            return defaultOptions;
        }

        private bool IsMatch(Regex regex, string inputText)
        {
            if (regex == null)
                return false;
            else
                return regex.IsMatch(inputText);
        }

        private Regex HompepageUrlRegex
        {
            get
            {
                if (_homepageUrlRegex == null && _homepageUrlPattern != String.Empty)
                {
                    try
                    {
                        _homepageUrlRegex = new Regex(_homepageUrlPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + _homepageUrlPattern);
                    }
                }

                return _homepageUrlRegex;
            }
        }

        private Regex HomepageContentRegex
        {
            get
            {
                if (_homepageContentRegex == null && _homepageContentPattern != String.Empty)
                {
                    try
                    {
                        _homepageContentRegex = new Regex(_homepageContentPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + _homepageContentPattern);
                    }
                }

                return _homepageContentRegex;
            }
        }

        private Regex RsdEngineNameRegex
        {
            get
            {
                if (_rsdEngineNameRegex == null && _rsdEngineNamePattern != String.Empty)
                {
                    try
                    {
                        _rsdEngineNameRegex = new Regex(_rsdEngineNamePattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + _rsdEngineNamePattern);
                    }
                }

                return _rsdEngineNameRegex;
            }
        }

        private Regex RsdHomepageLinkRegex
        {
            get
            {
                if (_rsdHomepageLinkRegex == null && _rsdHomepageLinkPattern != String.Empty)
                {
                    try
                    {
                        _rsdHomepageLinkRegex = new Regex(_rsdHomepageLinkPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + _rsdHomepageLinkPattern);
                    }
                }

                return _rsdHomepageLinkRegex;
            }
        }

        private bool _visible;
        private RsdClientTypeMapping[] _rsdClientTypeMappings;
        private string _rsdEngineNamePattern;
        private Regex _rsdEngineNameRegex;
        private string _rsdHomepageLinkPattern;
        private Regex _rsdHomepageLinkRegex;
        private string _homepageUrlPattern;
        private Regex _homepageUrlRegex;
        private string _homepageContentPattern;
        private Regex _homepageContentRegex;
        private ProviderFault[] _providerFaults;

        private static readonly SettingsPersisterHelper _postEditorSettings = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");
        private static readonly bool WP_ENABLED = _postEditorSettings.GetBoolean("M1Enabled", false);

        protected struct RsdClientTypeMapping
        {
            public RsdClientTypeMapping(string rsdClientType, string clientType)
            {
                RsdClientType = rsdClientType;
                ClientType = clientType;
            }
            public readonly string RsdClientType;
            public readonly string ClientType;

        }

        protected class ProviderFault
        {
            public ProviderFault(string faultCodePattern, string faultStringPattern, string messageId)
            {
                _faultCodePattern = faultCodePattern;
                _faultStringPattern = faultStringPattern;
                _messageId = messageId;

            }

            public bool IsMatch(string faultCode, string faultString)
            {
                if (_faultCodePattern != String.Empty && _faultStringPattern != String.Empty)
                {
                    return FaultCodeMatches(faultCode) && FaultStringMatches(faultString);
                }
                else if (_faultCodePattern != String.Empty)
                {
                    return FaultCodeMatches(faultCode);
                }
                else if (_faultStringPattern != String.Empty)
                {
                    return FaultStringMatches(faultString);
                }
                else
                {
                    return false;
                }
            }

            public MessageId MessageId
            {
                get
                {
                    try
                    {
                        return (MessageId)MessageId.Parse(typeof(MessageId), _messageId, false);
                    }
                    catch (ArgumentException)
                    {
                        return MessageId.None;
                    }
                }
            }

            private bool FaultCodeMatches(string faultCode)
            {
                try // defend against invalid regex in provider or manifest file
                {
                    Regex regex = new Regex(_faultCodePattern, RegexOptions.IgnoreCase);
                    return regex.IsMatch(faultCode);
                }
                catch (ArgumentException e)
                {
                    Trace.Fail("Error processing regular expression: " + e.ToString());
                    return false;
                }
            }

            private bool FaultStringMatches(string faultString)
            {
                try // defend against invalid regex in provider or manifest file
                {
                    Regex regex = new Regex(_faultStringPattern, RegexOptions.IgnoreCase);
                    return regex.IsMatch(faultString);
                }
                catch (ArgumentException e)
                {
                    Trace.Fail("Error processing regular expression: " + e.ToString());
                    return false;
                }
            }

            private string _faultCodePattern;
            private string _faultStringPattern;
            private string _messageId;
        }
    }
}

