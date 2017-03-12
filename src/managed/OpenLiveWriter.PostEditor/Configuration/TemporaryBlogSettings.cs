// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Globalization;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.FileDestinations;
using OpenLiveWriter.PostEditor.Configuration.Wizard;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor.Configuration
{
    public class TemporaryBlogSettings
        : IBlogSettingsAccessor, IBlogSettingsDetectionContext, ITemporaryBlogSettingsDetectionContext, ICloneable
    {
        public static TemporaryBlogSettings CreateNew()
        {
            return new TemporaryBlogSettings();
        }

        public static TemporaryBlogSettings ForBlogId(string blogId)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(blogId))
            {
                TemporaryBlogSettings tempSettings = new TemporaryBlogSettings(blogId);
                tempSettings.IsNewWeblog = false;
                tempSettings.IsSpacesBlog = blogSettings.IsSpacesBlog;
                tempSettings.IsSharePointBlog = blogSettings.IsSharePointBlog;
                tempSettings.IsGoogleBloggerBlog = blogSettings.IsGoogleBloggerBlog;
                tempSettings.HostBlogId = blogSettings.HostBlogId;
                tempSettings.BlogName = blogSettings.BlogName;
                tempSettings.HomepageUrl = blogSettings.HomepageUrl;
                tempSettings.ForceManualConfig = blogSettings.ForceManualConfig;
                tempSettings.ManifestDownloadInfo = blogSettings.ManifestDownloadInfo;
                tempSettings.SetProvider(blogSettings.ProviderId, blogSettings.ServiceName, blogSettings.PostApiUrl, blogSettings.ClientType);
                tempSettings.Credentials = blogSettings.Credentials;
                tempSettings.LastPublishFailed = blogSettings.LastPublishFailed;
                tempSettings.Categories = blogSettings.Categories;
                tempSettings.Keywords = blogSettings.Keywords;
                tempSettings.Authors = blogSettings.Authors;
                tempSettings.Pages = blogSettings.Pages;
                tempSettings.FavIcon = blogSettings.FavIcon;
                tempSettings.Image = blogSettings.Image;
                tempSettings.WatermarkImage = blogSettings.WatermarkImage;
                tempSettings.OptionOverrides = blogSettings.OptionOverrides;
                tempSettings.UserOptionOverrides = blogSettings.UserOptionOverrides;
                tempSettings.ButtonDescriptions = blogSettings.ButtonDescriptions;
                tempSettings.HomePageOverrides = blogSettings.HomePageOverrides;

                //set the save password flag
                tempSettings.SavePassword = blogSettings.Credentials.Password != null &&
                    blogSettings.Credentials.Password != String.Empty;

                // file upload support
                tempSettings.FileUploadSupport = blogSettings.FileUploadSupport;

                // get ftp settings if necessary
                if (blogSettings.FileUploadSupport == FileUploadSupport.FTP)
                {
                    FtpUploaderSettings.Copy(blogSettings.FileUploadSettings, tempSettings.FileUploadSettings);
                }

                blogSettings.PublishingPluginSettings.CopyTo(tempSettings.PublishingPluginSettings);

                using (PostHtmlEditingSettings editSettings = new PostHtmlEditingSettings(blogId))
                {
                    tempSettings.TemplateFiles = editSettings.EditorTemplateHtmlFiles;
                }
                return tempSettings;
            }
        }

        public void Save(BlogSettings settings)
        {
            settings.HostBlogId = this.HostBlogId;
            settings.IsSpacesBlog = this.IsSpacesBlog;
            settings.IsSharePointBlog = this.IsSharePointBlog;
            settings.IsGoogleBloggerBlog = this.IsGoogleBloggerBlog;
            settings.BlogName = this.BlogName;
            settings.HomepageUrl = this.HomepageUrl;
            settings.ForceManualConfig = this.ForceManualConfig;
            settings.ManifestDownloadInfo = this.ManifestDownloadInfo;
            settings.SetProvider(this.ProviderId, this.ServiceName);
            settings.ClientType = this.ClientType;
            settings.PostApiUrl = this.PostApiUrl;
            if (IsSpacesBlog || !(SavePassword ?? false)) // clear out password so we don't save it
                Credentials.Password = "";

            settings.Credentials = this.Credentials;

            if (Categories != null)
                settings.Categories = this.Categories;

            if (Keywords != null)
                settings.Keywords = this.Keywords;

            settings.Authors = this.Authors;
            settings.Pages = this.Pages;

            settings.FavIcon = this.FavIcon;
            settings.Image = this.Image;
            settings.WatermarkImage = this.WatermarkImage;

            if (OptionOverrides != null)
                settings.OptionOverrides = this.OptionOverrides;

            if (UserOptionOverrides != null)
                settings.UserOptionOverrides = this.UserOptionOverrides;

            if (HomePageOverrides != null)
                settings.HomePageOverrides = this.HomePageOverrides;

            settings.ButtonDescriptions = this.ButtonDescriptions;

            // file upload support
            settings.FileUploadSupport = this.FileUploadSupport;

            // save ftp settings if necessary
            if (FileUploadSupport == FileUploadSupport.FTP)
            {
                FtpUploaderSettings.Copy(FileUploadSettings, settings.FileUploadSettings);
            }

            PublishingPluginSettings.CopyTo(settings.PublishingPluginSettings);

            using (PostHtmlEditingSettings editSettings = new PostHtmlEditingSettings(settings.Id))
            {
                editSettings.EditorTemplateHtmlFiles = TemplateFiles;
            }
        }

        public void SetBlogInfo(BlogInfo blogInfo)
        {
            if (blogInfo.Id != _hostBlogId)
            {
                _blogName = blogInfo.Name;
                _hostBlogId = blogInfo.Id;
                _homePageUrl = blogInfo.HomepageUrl;
                if (!UrlHelper.IsUrl(_homePageUrl))
                {
                    Trace.Assert(!string.IsNullOrEmpty(_homePageUrl), "Homepage URL was null or empty");
                    string baseUrl = UrlHelper.GetBaseUrl(_postApiUrl);
                    _homePageUrl = UrlHelper.UrlCombineIfRelative(baseUrl, _homePageUrl);
                }

                // reset categories, authors, and pages
                Categories = new BlogPostCategory[] { };
                Keywords = new BlogPostKeyword[] { };
                Authors = new AuthorInfo[] { };
                Pages = new PageInfo[] { };

                // reset option overrides
                if (OptionOverrides != null)
                    OptionOverrides.Clear();

                if (UserOptionOverrides != null)
                    UserOptionOverrides.Clear();

                if (HomePageOverrides != null)
                    HomePageOverrides.Clear();

                // reset provider buttons
                if (ButtonDescriptions != null)
                    ButtonDescriptions = new IBlogProviderButtonDescription[0];

                // reset template
                TemplateFiles = new BlogEditingTemplateFile[0];
            }
        }

        public void SetProvider(string providerId, string serviceName, string postApiUrl, string clientType)
        {
            // for dirty states only
            if (ProviderId != providerId ||
                    ServiceName != serviceName ||
                    PostApiUrl != postApiUrl ||
                    ClientType != clientType)
            {
                // reset the provider info
                _providerId = providerId;
                _serviceName = serviceName;
                _postApiUrl = postApiUrl;
                _clientType = clientType;
            }
        }

        public void ClearProvider()
        {
            _providerId = String.Empty;
            _serviceName = String.Empty;
            _postApiUrl = String.Empty;
            _clientType = String.Empty;
            _hostBlogId = String.Empty;
            _manifestDownloadInfo = null;
            _optionOverrides.Clear();
            _templateFiles = new BlogEditingTemplateFile[0];
            _homepageOptionOverrides.Clear();
            _buttonDescriptions = new BlogProviderButtonDescription[0];
            _categories = new BlogPostCategory[0];
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public bool IsSpacesBlog
        {
            get
            {
                return _isSpacesBlog;
            }
            set
            {
                _isSpacesBlog = value;
            }
        }

        public bool? SavePassword
        {
            get
            {
                return _savePassword;
            }
            set
            {
                _savePassword = value;
            }
        }

        public bool IsSharePointBlog
        {
            get
            {
                return _isSharePointBlog;
            }
            set
            {
                _isSharePointBlog = value;
            }
        }

        public bool IsGoogleBloggerBlog
        {
            get
            {
                return _isGoogleBloggerBlog;
            }
            set
            {
                _isGoogleBloggerBlog = value;
            }
        }

        public string HostBlogId
        {
            get
            {
                return _hostBlogId;
            }
            set
            {
                _hostBlogId = value;
            }
        }

        public string BlogName
        {
            get
            {
                return _blogName;
            }
            set
            {
                _blogName = value;
            }
        }

        public string HomepageUrl
        {
            get
            {
                return _homePageUrl;
            }
            set
            {
                _homePageUrl = value;
            }
        }

        public bool ForceManualConfig
        {
            get
            {
                return _forceManualConfig;
            }
            set
            {
                _forceManualConfig = value;
            }
        }

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo
        {
            get
            {
                return _manifestDownloadInfo;
            }
            set
            {
                _manifestDownloadInfo = value;
            }
        }

        public IDictionary OptionOverrides
        {
            get
            {
                return _optionOverrides;
            }
            set
            {
                _optionOverrides = value;
            }
        }

        public IDictionary UserOptionOverrides
        {
            get
            {
                return _userOptionOverrides;
            }
            set
            {
                _userOptionOverrides = value;
            }
        }

        public IDictionary HomePageOverrides
        {
            get
            {
                return _homepageOptionOverrides;
            }
            set
            {
                _homepageOptionOverrides = value;
            }
        }

        public void UpdatePostBodyBackgroundColor(Color color)
        {
            IDictionary dictionary = HomePageOverrides ?? new Hashtable();
            dictionary[BlogClientOptions.POST_BODY_BACKGROUND_COLOR] = color.ToArgb().ToString(CultureInfo.InvariantCulture);
            HomePageOverrides = dictionary;
        }

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get
            {
                return _buttonDescriptions;
            }
            set
            {
                _buttonDescriptions = new BlogProviderButtonDescription[value.Length];
                for (int i = 0; i < value.Length; i++)
                    _buttonDescriptions[i] = new BlogProviderButtonDescription(value[i]);

            }
        }

        public string ProviderId
        {
            get { return _providerId; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
        }

        public string ClientType
        {
            get { return _clientType; }
            set { _clientType = value; }
        }

        public string PostApiUrl
        {
            get { return _postApiUrl; }
        }

        IBlogCredentialsAccessor IBlogSettingsAccessor.Credentials
        {
            get
            {
                return new BlogCredentialsAccessor(Id, Credentials);
            }
        }

        IBlogCredentialsAccessor IBlogSettingsDetectionContext.Credentials
        {
            get
            {
                return new BlogCredentialsAccessor(Id, Credentials);
            }
        }

        public IBlogCredentials Credentials
        {
            get
            {
                return _credentials;
            }
            set
            {
                BlogCredentialsHelper.Copy(value, _credentials);
            }
        }

        public bool LastPublishFailed
        {
            get
            {
                return _lastPublishFailed;
            }
            set
            {
                _lastPublishFailed = value;
            }
        }

        public BlogPostCategory[] Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
            }
        }

        public BlogPostKeyword[] Keywords
        {
            get
            {
                return _keywords;
            }
            set
            {
                _keywords = value;
            }
        }

        public AuthorInfo[] Authors
        {
            get
            {
                return _authors;
            }
            set
            {
                _authors = value;
            }
        }

        public PageInfo[] Pages
        {
            get
            {
                return _pages;
            }
            set
            {
                _pages = value;
            }
        }

        public byte[] FavIcon
        {
            get
            {
                return _favIcon;
            }
            set
            {
                _favIcon = value;
            }
        }

        public byte[] Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
            }
        }

        public byte[] WatermarkImage
        {
            get
            {
                return _watermarkImage;
            }
            set
            {
                _watermarkImage = value;
            }
        }

        public FileUploadSupport FileUploadSupport
        {
            get
            {
                return _fileUploadSupport;
            }
            set
            {
                _fileUploadSupport = value;
            }
        }

        public IBlogFileUploadSettings FileUploadSettings
        {
            get { return _fileUploadSettings; }
        }

        public IBlogFileUploadSettings AtomPublishingProtocolSettings
        {
            get { return _atomPublishingProtocolSettings; }
        }

        public BlogPublishingPluginSettings PublishingPluginSettings
        {
            get { return new BlogPublishingPluginSettings(_pluginSettings); }
        }

        public BlogEditingTemplateFile[] TemplateFiles
        {
            get
            {
                return _templateFiles;
            }
            set
            {
                _templateFiles = value;
            }
        }

        public bool IsNewWeblog
        {
            get { return _isNewWeblog; }
            set { _isNewWeblog = value; }
        }

        public bool SwitchToWeblog
        {
            get { return _switchToWeblog; }
            set { _switchToWeblog = value; }
        }

        public BlogInfo[] HostBlogs
        {
            get
            {
                return _hostBlogs;
            }
            set
            {
                _hostBlogs = value;
            }
        }

        public bool InstrumentationOptIn
        {
            get
            {
                return _instrumentationOptIn;
            }
            set
            {
                _instrumentationOptIn = value;
            }
        }

        public BlogInfo[] AvailableImageEndpoints
        {
            get { return _availableImageEndpoints; }
            set { _availableImageEndpoints = value; }
        }

        //
        // IMPORTANT NOTE: When adding member variables you MUST update the CopyFrom() implementation below!!!!
        //
        private string _id = String.Empty;
        private bool? _savePassword;
        private bool _isSpacesBlog = false;
        private bool _isSharePointBlog = false;
        private bool _isGoogleBloggerBlog = false;
        private string _hostBlogId = String.Empty;
        private string _blogName = String.Empty;
        private string _homePageUrl = String.Empty;
        private bool _forceManualConfig = false;
        private WriterEditingManifestDownloadInfo _manifestDownloadInfo = null;
        private string _providerId = String.Empty;
        private string _serviceName = String.Empty;
        private string _clientType = String.Empty;
        private string _postApiUrl = String.Empty;
        private TemporaryBlogCredentials _credentials = new TemporaryBlogCredentials();
        private bool _lastPublishFailed = false;
        private BlogEditingTemplateFile[] _templateFiles = new BlogEditingTemplateFile[0];
        private bool _isNewWeblog = true;
        private bool _switchToWeblog = false;
        private BlogPostCategory[] _categories = null;
        private BlogPostKeyword[] _keywords = null;
        private AuthorInfo[] _authors = new AuthorInfo[0];
        private PageInfo[] _pages = new PageInfo[0];
        private BlogProviderButtonDescription[] _buttonDescriptions = new BlogProviderButtonDescription[0];
        private byte[] _favIcon = null;
        private byte[] _image = null;
        private byte[] _watermarkImage = null;
        private IDictionary _homepageOptionOverrides = new Hashtable();
        private IDictionary _optionOverrides = new Hashtable();
        private IDictionary _userOptionOverrides = new Hashtable();
        private BlogInfo[] _hostBlogs = new BlogInfo[] { };
        private FileUploadSupport _fileUploadSupport = FileUploadSupport.Weblog;
        private TemporaryFileUploadSettings _fileUploadSettings = new TemporaryFileUploadSettings();
        private TemporaryFileUploadSettings _atomPublishingProtocolSettings = new TemporaryFileUploadSettings();
        private SettingsPersisterHelper _pluginSettings = new SettingsPersisterHelper(new MemorySettingsPersister());
        private BlogInfo[] _availableImageEndpoints;
        private bool _instrumentationOptIn = false;
        //
        // IMPORTANT NOTE: When adding member variables you MUST update the CopyFrom() implementation below!!!!
        //

        private TemporaryBlogSettings()
        {
            Id = Guid.NewGuid().ToString();
        }

        private TemporaryBlogSettings(string id)
        {
            Id = id;
        }

        public void Dispose()
        {

        }

        public void CopyFrom(TemporaryBlogSettings sourceSettings)
        {
            // simple members
            _id = sourceSettings._id;
            _switchToWeblog = sourceSettings._switchToWeblog;
            _isNewWeblog = sourceSettings._isNewWeblog;
            _savePassword = sourceSettings._savePassword;
            _isSpacesBlog = sourceSettings._isSpacesBlog;
            _isSharePointBlog = sourceSettings._isSharePointBlog;
            _isGoogleBloggerBlog = sourceSettings._isGoogleBloggerBlog;
            _hostBlogId = sourceSettings._hostBlogId;
            _blogName = sourceSettings._blogName;
            _homePageUrl = sourceSettings._homePageUrl;
            _manifestDownloadInfo = sourceSettings._manifestDownloadInfo;
            _providerId = sourceSettings._providerId;
            _serviceName = sourceSettings._serviceName;
            _clientType = sourceSettings._clientType;
            _postApiUrl = sourceSettings._postApiUrl;
            _lastPublishFailed = sourceSettings._lastPublishFailed;
            _fileUploadSupport = sourceSettings._fileUploadSupport;
            _instrumentationOptIn = sourceSettings._instrumentationOptIn;

            if (sourceSettings._availableImageEndpoints == null)
            {
                _availableImageEndpoints = null;
            }
            else
            {
                // Good thing BlogInfo is immutable!
                _availableImageEndpoints = (BlogInfo[])sourceSettings._availableImageEndpoints.Clone();
            }

            // credentials
            BlogCredentialsHelper.Copy(sourceSettings._credentials, _credentials);

            // template files
            _templateFiles = new BlogEditingTemplateFile[sourceSettings._templateFiles.Length];
            for (int i = 0; i < sourceSettings._templateFiles.Length; i++)
            {
                BlogEditingTemplateFile sourceFile = sourceSettings._templateFiles[i];
                _templateFiles[i] = new BlogEditingTemplateFile(sourceFile.TemplateType, sourceFile.TemplateFile);
            }

            // option overrides
            if (sourceSettings._optionOverrides != null)
            {
                _optionOverrides.Clear();
                foreach (DictionaryEntry entry in sourceSettings._optionOverrides)
                    _optionOverrides.Add(entry.Key, entry.Value);
            }

            // user option overrides
            if (sourceSettings._userOptionOverrides != null)
            {
                _userOptionOverrides.Clear();
                foreach (DictionaryEntry entry in sourceSettings._userOptionOverrides)
                    _userOptionOverrides.Add(entry.Key, entry.Value);
            }

            // homepage overrides
            if (sourceSettings._homepageOptionOverrides != null)
            {
                _homepageOptionOverrides.Clear();
                foreach (DictionaryEntry entry in sourceSettings._homepageOptionOverrides)
                    _homepageOptionOverrides.Add(entry.Key, entry.Value);
            }

            // categories
            if (sourceSettings._categories != null)
            {
                _categories = new BlogPostCategory[sourceSettings._categories.Length];
                for (int i = 0; i < sourceSettings._categories.Length; i++)
                {
                    BlogPostCategory sourceCategory = sourceSettings._categories[i];
                    _categories[i] = sourceCategory.Clone() as BlogPostCategory;
                }
            }
            else
            {
                _categories = null;
            }

            if (sourceSettings._keywords != null)
            {
                _keywords = new BlogPostKeyword[sourceSettings._keywords.Length];
                for (int i = 0; i < sourceSettings._keywords.Length; i++)
                {
                    BlogPostKeyword sourceKeyword = sourceSettings._keywords[i];
                    _keywords[i] = sourceKeyword.Clone() as BlogPostKeyword;
                }
            }
            else
            {
                _keywords = null;
            }

            // authors and pages
            _authors = sourceSettings._authors.Clone() as AuthorInfo[];
            _pages = sourceSettings._pages.Clone() as PageInfo[];

            // buttons
            if (sourceSettings._buttonDescriptions != null)
            {
                _buttonDescriptions = new BlogProviderButtonDescription[sourceSettings._buttonDescriptions.Length];
                for (int i = 0; i < sourceSettings._buttonDescriptions.Length; i++)
                    _buttonDescriptions[i] = sourceSettings._buttonDescriptions[i].Clone() as BlogProviderButtonDescription;
            }
            else
            {
                _buttonDescriptions = null;
            }

            // favicon
            _favIcon = sourceSettings._favIcon;

            // images
            _image = sourceSettings._image;
            _watermarkImage = sourceSettings._watermarkImage;

            // host blogs
            _hostBlogs = new BlogInfo[sourceSettings._hostBlogs.Length];
            for (int i = 0; i < sourceSettings._hostBlogs.Length; i++)
            {
                BlogInfo sourceBlog = sourceSettings._hostBlogs[i];
                _hostBlogs[i] = new BlogInfo(sourceBlog.Id, sourceBlog.Name, sourceBlog.HomepageUrl);
            }

            // file upload settings
            _fileUploadSettings = sourceSettings._fileUploadSettings.Clone() as TemporaryFileUploadSettings;

            _pluginSettings = new SettingsPersisterHelper(new MemorySettingsPersister());
            _pluginSettings.CopyFrom(sourceSettings._pluginSettings, true, true);
        }

        public object Clone()
        {
            TemporaryBlogSettings newSettings = new TemporaryBlogSettings();
            newSettings.CopyFrom(this);
            return newSettings;
        }

    }

    public class TemporaryBlogCredentials : IBlogCredentials
    {
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        private string _username = String.Empty;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private string _password = String.Empty;

        public string[] CustomValues
        {
            get
            {
                string[] customValues = new string[_values.Count];
                if (_values.Count > 0)
                    _values.Keys.CopyTo(customValues, 0);
                return customValues;
            }
        }

        public string GetCustomValue(string name)
        {
            if (_values.Contains(name))
            {
                return _values[name] as string;
            }
            else
            {
                return String.Empty;
            }
        }

        public void SetCustomValue(string name, string value)
        {
            _values[name] = value;
        }

        public ICredentialsDomain Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }
        private ICredentialsDomain _domain;

        public void Clear()
        {
            _username = String.Empty;
            _password = String.Empty;
            _values.Clear();
        }

        private Hashtable _values = new Hashtable();

    }

    public class TemporaryFileUploadSettings : IBlogFileUploadSettings, ICloneable
    {
        public TemporaryFileUploadSettings()
        {
        }

        public string GetValue(string name)
        {
            if (_values.Contains(name))
            {
                return _values[name] as string;
            }
            else
            {
                return String.Empty;
            }
        }

        public void SetValue(string name, string value)
        {
            _values[name] = value;
        }

        public string[] Names
        {
            get { return (string[])new ArrayList(_values.Keys).ToArray(typeof(string)); }
        }

        public void Clear()
        {
            _values.Clear();
        }

        private Hashtable _values = new Hashtable();

        public object Clone()
        {
            TemporaryFileUploadSettings newSettings = new TemporaryFileUploadSettings();

            foreach (DictionaryEntry entry in _values)
                newSettings.SetValue(entry.Key as string, entry.Value as string);

            return newSettings;
        }

    }

}

