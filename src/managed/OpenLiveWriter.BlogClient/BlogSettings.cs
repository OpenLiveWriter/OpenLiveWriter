// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.BlogClient.Detection;

namespace OpenLiveWriter.BlogClient
{

    public class BlogSettings : IBlogSettingsAccessor, IBlogSettingsDetectionContext, IDisposable
    {
        public static string[] GetBlogIds()
        {
            string[] blogIds = SettingsKey.GetSubSettingNames();

            for (int i = 0; i < blogIds.Length; i++)
            {
                if (!BlogIdIsValid(blogIds[i]))
                {
                    blogIds[i] = null;
                }
            }

            return (string[])ArrayHelper.Compact(blogIds);
        }

        public static BlogDescriptor[] GetBlogs(bool sortByName)
        {
            string[] ids = GetBlogIds();
            BlogDescriptor[] blogs = new BlogDescriptor[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                using (BlogSettings settings = BlogSettings.ForBlogId(ids[i]))
                    blogs[i] = new BlogDescriptor(ids[i], settings.BlogName, settings.HomepageUrl);
            }
            if (sortByName)
                Array.Sort(blogs, new BlogDescriptor.Comparer());
            return blogs;
        }

        public static bool BlogIdIsValid(string id)
        {
            BlogSettings blogSettings = null;
            try
            {
                blogSettings = BlogSettings.ForBlogId(id);
                if (!blogSettings.IsValid)
                    return false;
                return BlogClientManager.IsValidClientType(blogSettings.ClientType);

            }
            catch (ArgumentException)
            {
                Trace.WriteLine("Default blog has invalid client type, ignoring blog.");
                return false;
            }
            finally
            {
                if (blogSettings != null)
                    blogSettings.Dispose();
            }
        }
        public static string DefaultBlogId
        {
            get
            {
                // try to get an explicitly set default profile id
                string defaultKey = SettingsKey.GetString(DEFAULT_WEBLOG, String.Empty);

                // if a default is specified and the key exists
                if (BlogIdIsValid(defaultKey))
                {
                    return defaultKey;
                }

                // if one is not specified then get the first one stored (if any)
                // (update the value while doing this so we don't have to repeat
                // this calculation)
                string[] blogIds = GetBlogIds();
                if (blogIds != null && blogIds.Length > 0)
                {
                    DefaultBlogId = blogIds[0];
                    return blogIds[0];
                }
                else
                    return String.Empty;

            }
            set
            {
                SettingsKey.SetString(DEFAULT_WEBLOG, value ?? String.Empty);
            }
        }
        public const string DEFAULT_WEBLOG = "DefaultWeblog";

        public delegate void BlogSettingsListener(string blogId);
        public static event BlogSettingsListener BlogSettingsDeleted;
        private static void OnBlogSettingsDeleted(string blogId)
        {
            if (BlogSettingsDeleted != null)
                BlogSettingsDeleted(blogId);
        }

        public static BlogSettings ForBlogId(string id)
        {
            return new BlogSettings(id);
        }

        private BlogSettings(string id)
        {
            try
            {
                Guid guid = new Guid(id);
                _id = guid.ToString();
            }
            catch (FormatException ex)
            {
                GC.SuppressFinalize(this);
                Trace.WriteLine("Failed to load blog settings for: " + id);
                throw new ArgumentException("Invalid Blog Id " + id, ex);

            }
        }

        /// <summary>
        /// used as a key into settings storage
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
        }
        private string _id;

        public bool IsValid
        {
            get
            {
                return SettingsKey.HasSubSettings(Id);
            }
        }

        public bool IsSpacesBlog
        {
            get { return Settings.GetBoolean(IS_SPACES_BLOG, false); }
            set { Settings.SetBoolean(IS_SPACES_BLOG, value); }
        }
        private const string IS_SPACES_BLOG = "IsSpacesBlog";

        public bool IsSharePointBlog
        {
            get { return Settings.GetBoolean(IS_SHAREPOINT_BLOG, false); }
            set { Settings.SetBoolean(IS_SHAREPOINT_BLOG, value); }
        }
        private const string IS_SHAREPOINT_BLOG = "IsSharePointBlog";

        public bool IsGoogleBloggerBlog
        {
            get { return Settings.GetBoolean(IS_GOOGLE_BLOGGER_BLOG, false); }
            set { Settings.SetBoolean(IS_GOOGLE_BLOGGER_BLOG, value); }
        }
        private const string IS_GOOGLE_BLOGGER_BLOG = "IsGoogleBloggerBlog";

        /// <summary>
        /// Id of the weblog on the host service
        /// </summary>
        public string HostBlogId
        {
            get { return Settings.GetString(BLOG_ID, String.Empty); }
            set { Settings.SetString(BLOG_ID, value); }
        }
        private const string BLOG_ID = "BlogId";

        public string BlogName
        {
            get { return Settings.GetString(BLOG_NAME, String.Empty); }
            set { Settings.SetString(BLOG_NAME, value); }
        }
        private const string BLOG_NAME = "BlogName";

        public string HomepageUrl
        {
            get { return Settings.GetString(HOMEPAGE_URL, String.Empty); }
            set { Settings.SetString(HOMEPAGE_URL, value); }
        }
        private const string HOMEPAGE_URL = "HomepageUrl";

        public bool ForceManualConfig
        {
            get { return Settings.GetBoolean(FORCE_MANUAL_CONFIG, false); }
            set { Settings.SetBoolean(FORCE_MANUAL_CONFIG, value); }
        }
        private const string FORCE_MANUAL_CONFIG = "ForceManualConfig";

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo
        {
            get
            {
                lock (_manifestDownloadInfoLock)
                {
                    using (SettingsPersisterHelper manifestKey = Settings.GetSubSettings(WRITER_MANIFEST))
                    {
                        // at a minimum must have a source-url
                        string sourceUrl = manifestKey.GetString(MANIFEST_SOURCE_URL, String.Empty);
                        if (sourceUrl != String.Empty)
                        {
                            return new WriterEditingManifestDownloadInfo(
                                sourceUrl,
                                manifestKey.GetDateTime(MANIFEST_EXPIRES, DateTime.MinValue),
                                manifestKey.GetDateTime(MANIFEST_LAST_MODIFIED, DateTime.MinValue),
                                manifestKey.GetString(MANIFEST_ETAG, String.Empty));
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            set
            {
                lock (_manifestDownloadInfoLock)
                {
                    if (value != null)
                    {
                        using (SettingsPersisterHelper manifestKey = Settings.GetSubSettings(WRITER_MANIFEST))
                        {
                            manifestKey.SetString(MANIFEST_SOURCE_URL, value.SourceUrl);
                            manifestKey.SetDateTime(MANIFEST_EXPIRES, value.Expires);
                            manifestKey.SetDateTime(MANIFEST_LAST_MODIFIED, value.LastModified);
                            manifestKey.SetString(MANIFEST_ETAG, value.ETag);
                        }
                    }
                    else
                    {
                        if (Settings.HasSubSettings(WRITER_MANIFEST))
                            Settings.UnsetSubsettingTree(WRITER_MANIFEST);
                    }
                }
            }
        }
        private const string WRITER_MANIFEST = "Manifest";
        private const string MANIFEST_SOURCE_URL = "SourceUrl";
        private const string MANIFEST_EXPIRES = "Expires";
        private const string MANIFEST_LAST_MODIFIED = "LastModified";
        private const string MANIFEST_ETAG = "ETag";
        private readonly static object _manifestDownloadInfoLock = new object();

        public string WriterManifestUrl
        {
            get { return Settings.GetString(WRITER_MANIFEST_URL, String.Empty); }
            set { Settings.SetString(WRITER_MANIFEST_URL, value); }
        }
        private const string WRITER_MANIFEST_URL = "ManifestUrl";

        public void SetProvider(string providerId, string serviceName)
        {
            Settings.SetString(PROVIDER_ID, providerId);
            Settings.SetString(SERVICE_NAME, serviceName);
        }

        public string ProviderId
        {
            get
            {
                string providerId = Settings.GetString(PROVIDER_ID, String.Empty);
                if (providerId == "16B3FA3F-DAD7-4c93-A407-81CAE076883E")
                    return "5FD58F3F-A36E-4aaf-8ABE-764248961FA0";
                else
                    return providerId;
            }
        }
        private const string PROVIDER_ID = "ProviderId";

        public string ServiceName
        {
            get { return Settings.GetString(SERVICE_NAME, String.Empty); }
        }
        private const string SERVICE_NAME = "ServiceName";

        public string ClientType
        {
            get
            {
                string clientType = Settings.GetString(CLIENT_TYPE, String.Empty);

                // temporary hack for migration of MovableType blogs
                // TODO: is there a cleaner place to do this?
                if (clientType == "MoveableType")
                    return "MovableType";

                return clientType;
            }
            set
            {
                // TODO:OLW
                // Hack to stop old Spaces configs to be violently/accidentally
                // upgrading to Atom. At time of this writing, this condition gets
                // hit by ServiceUpdateChecker running. This prevents the client
                // type from being changed in the registry from WindowsLiveSpaces
                // to WindowsLiveSpacesAtom; the only practical effect of letting
                // the write go to disk would be that you can't go back to an
                // older build of Writer. We don't have perfect forward compatibility
                // anyway--going through the config wizard with a Spaces blog will
                // also break older builds. But it seems like it's going too far
                // that just starting Writer will make that change.
                // We can take this out, if desired, anytime after Wave 3 goes final.
                if (value == "WindowsLiveSpacesAtom" && Settings.GetString(CLIENT_TYPE, string.Empty) == "WindowsLiveSpaces")
                    return;

                Settings.SetString(CLIENT_TYPE, value);
            }
        }
        private const string CLIENT_TYPE = "ClientType";

        public string PostApiUrl
        {
            get { return Settings.GetString(POST_API_URL, String.Empty); }
            set { Settings.SetString(POST_API_URL, value); }
        }
        private const string POST_API_URL = "PostApiUrl";

        public IDictionary HomePageOverrides
        {
            get
            {
                lock (_homepageOptionOverridesLock)
                {
                    IDictionary homepageOptionOverrides = new Hashtable();
                    // Trying to avoid the creation of this key, so we will know when the service update runs whether we need to build
                    // these settings for the first time.
                    if (Settings.HasSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                    {
                        using (SettingsPersisterHelper homepageOptionOverridesKey = Settings.GetSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                        {
                            foreach (string optionName in homepageOptionOverridesKey.GetNames())
                                homepageOptionOverrides.Add(optionName, homepageOptionOverridesKey.GetString(optionName, String.Empty));
                        }
                    }
                    return homepageOptionOverrides;
                }
            }
            set
            {
                lock (_homepageOptionOverridesLock)
                {
                    // delete existing overrides
                    Settings.UnsetSubsettingTree(HOMEPAGE_OPTION_OVERRIDES);

                    // re-write overrides
                    using (SettingsPersisterHelper homepageOptionOverridesKey = Settings.GetSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                    {
                        foreach (DictionaryEntry entry in value)
                            homepageOptionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                    }
                }
            }
        }
        private const string HOMEPAGE_OPTION_OVERRIDES = "HomepageOptions";
        private readonly static object _homepageOptionOverridesLock = new object();

        public IDictionary OptionOverrides
        {
            get
            {
                lock (_optionOverridesLock)
                {
                    IDictionary optionOverrides = new Hashtable();
                    using (SettingsPersisterHelper optionOverridesKey = Settings.GetSubSettings(OPTION_OVERRIDES))
                    {
                        foreach (string optionName in optionOverridesKey.GetNames())
                            optionOverrides.Add(optionName, optionOverridesKey.GetString(optionName, String.Empty));
                    }
                    return optionOverrides;
                }
            }
            set
            {
                lock (_optionOverridesLock)
                {
                    // safely delete existing overrides
                    Settings.UnsetSubsettingTree(OPTION_OVERRIDES);

                    // re-write overrides
                    using (SettingsPersisterHelper optionOverridesKey = Settings.GetSubSettings(OPTION_OVERRIDES))
                    {
                        foreach (DictionaryEntry entry in value)
                            optionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                    }
                }
            }
        }
        private const string OPTION_OVERRIDES = "ManifestOptions";
        private readonly static object _optionOverridesLock = new object();

        public IDictionary UserOptionOverrides
        {
            get
            {
                lock (_userOptionOverridesLock)
                {
                    IDictionary userOptionOverrides = new Hashtable();
                    using (SettingsPersisterHelper userOptionOverridesKey = Settings.GetSubSettings(USER_OPTION_OVERRIDES))
                    {
                        foreach (string optionName in userOptionOverridesKey.GetNames())
                            userOptionOverrides.Add(optionName, userOptionOverridesKey.GetString(optionName, String.Empty));
                    }
                    return userOptionOverrides;
                }
            }
            set
            {
                lock (_userOptionOverridesLock)
                {
                    // delete existing overrides
                    Settings.UnsetSubsettingTree(USER_OPTION_OVERRIDES);

                    // re-write overrides
                    using (SettingsPersisterHelper userOptionOverridesKey = Settings.GetSubSettings(USER_OPTION_OVERRIDES))
                    {
                        foreach (DictionaryEntry entry in value)
                            userOptionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                    }
                }
            }
        }
        private const string USER_OPTION_OVERRIDES = "UserOptionOverrides";
        private readonly static object _userOptionOverridesLock = new object();

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
                return (this as IBlogSettingsAccessor).Credentials;
            }
        }

        public IBlogCredentials Credentials
        {
            get
            {
                if (_blogCredentials == null)
                {
                    CredentialsDomain credentialsDomain = new CredentialsDomain(ServiceName, BlogName, FavIcon, Image);
                    _blogCredentials = new BlogCredentials(Settings, credentialsDomain);
                }
                return _blogCredentials;
            }
            set
            {
                BlogCredentialsHelper.Copy(value, Credentials);
            }
        }
        private BlogCredentials _blogCredentials;

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get
            {
                lock (_buttonsLock)
                {
                    ArrayList buttonDescriptions = new ArrayList();
                    using (SettingsPersisterHelper providerButtons = Settings.GetSubSettings(BUTTONS_KEY))
                    {
                        foreach (string buttonId in providerButtons.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper buttonKey = providerButtons.GetSubSettings(buttonId))
                                buttonDescriptions.Add(new BlogProviderButtonDescriptionFromSettings(buttonKey));
                        }
                    }
                    return buttonDescriptions.ToArray(typeof(IBlogProviderButtonDescription)) as IBlogProviderButtonDescription[];
                }
            }
            set
            {
                lock (_buttonsLock)
                {
                    // write button descriptions
                    using (SettingsPersisterHelper providerButtons = Settings.GetSubSettings(BUTTONS_KEY))
                    {
                        // track buttons that have been deleted (assume all have been deleted and then
                        // remove deleted buttons from the list as they are referenced)
                        ArrayList deletedButtons = new ArrayList(providerButtons.GetSubSettingNames());

                        // write the descriptions
                        foreach (IBlogProviderButtonDescription buttonDescription in value)
                        {
                            // write
                            using (SettingsPersisterHelper buttonKey = providerButtons.GetSubSettings(buttonDescription.Id))
                                BlogProviderButtonDescriptionFromSettings.SaveFrameButtonDescriptionToSettings(buttonDescription, buttonKey);

                            // note that this button should not be deleted
                            deletedButtons.Remove(buttonDescription.Id);
                        }

                        // execute deletes
                        foreach (string buttonId in deletedButtons)
                            providerButtons.UnsetSubsettingTree(buttonId);
                    }
                }
            }
        }
        private const string BUTTONS_KEY = "CustomButtons";
        private readonly static object _buttonsLock = new object();

        public bool LastPublishFailed
        {
            get { return Settings.GetBoolean(LAST_PUBLISH_FAILED, false); }
            set { Settings.SetBoolean(LAST_PUBLISH_FAILED, value); }
        }
        private const string LAST_PUBLISH_FAILED = "LastPublishFailed";

        public byte[] FavIcon
        {
            get { return Settings.GetByteArray(FAV_ICON, null); }
            set { Settings.SetByteArray(FAV_ICON, value); }
        }
        private const string FAV_ICON = "FavIcon";

        public byte[] Image
        {
            get { return Settings.GetByteArray(IMAGE, null); }
            set
            {
                byte[] imageBytes = value;
                if (imageBytes != null && imageBytes.Length == 0)
                    imageBytes = null;
                Settings.SetByteArray(IMAGE, imageBytes);
            }
        }
        private const string IMAGE = "ImageBytes";

        public byte[] WatermarkImage
        {
            get { return Settings.GetByteArray(WATERMARK_IMAGE, null); }
            set
            {
                byte[] watermarkBytes = value;
                if (watermarkBytes != null && watermarkBytes.Length == 0)
                    watermarkBytes = null;
                Settings.SetByteArray(WATERMARK_IMAGE, watermarkBytes);
            }
        }
        private const string WATERMARK_IMAGE = "WatermarkImageBytes";

        public BlogPostCategory[] Categories
        {
            get
            {
                lock (_categoriesLock)
                {
                    // get the categories
                    ArrayList categories = new ArrayList();
                    using (SettingsPersisterHelper categoriesKey = Settings.GetSubSettings(CATEGORIES))
                    {
                        foreach (string id in categoriesKey.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper categoryKey = categoriesKey.GetSubSettings(id))
                            {
                                string name = categoryKey.GetString(CATEGORY_NAME, id);
                                string parent = categoryKey.GetString(CATEGORY_PARENT, String.Empty);
                                categories.Add(new BlogPostCategory(id, name, parent));
                            }
                        }
                    }

                    if (categories.Count > 0)
                        return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));

                    else // if we got no categories using the new format, try the old format
                        return LegacyCategories;
                }
            }
            set
            {
                lock (_categoriesLock)
                {
                    // delete existing categories
                    SettingsPersisterHelper settings = Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(CATEGORIES);

                        // re-write categories
                        using (SettingsPersisterHelper categoriesKey = settings.GetSubSettings(CATEGORIES))
                        {
                            foreach (BlogPostCategory category in value)
                            {
                                using (SettingsPersisterHelper categoryKey = categoriesKey.GetSubSettings(category.Id))
                                {
                                    categoryKey.SetString(CATEGORY_NAME, category.Name);
                                    categoryKey.SetString(CATEGORY_PARENT, category.Parent);
                                }
                            }
                        }
                    }
                }
            }
        }
        private const string CATEGORIES = "Categories";
        private const string CATEGORY_NAME = "Name";
        private const string CATEGORY_PARENT = "Parent";
        private readonly static object _categoriesLock = new object();

        private static readonly Dictionary<string, XmlSettingsPersister> _keywordPersister = new Dictionary<string, XmlSettingsPersister>();
        /// <summary>
        /// Make sure to own _keywordsLock before calling this property
        /// </summary>
        private XmlSettingsPersister KeywordPersister
        {
            get
            {
                if (!_keywordPersister.ContainsKey(KeywordPath))
                {
                    _keywordPersister.Add(KeywordPath, XmlFileSettingsPersister.Open(KeywordPath));
                }
                return _keywordPersister[KeywordPath];
            }
        }

        public BlogPostKeyword[] Keywords
        {
            get
            {
                lock (_keywordsLock)
                {

                    ArrayList keywords = new ArrayList();
                    // Get all of the keyword subkeys
                    using (XmlSettingsPersister keywordsKey = (XmlSettingsPersister)KeywordPersister.GetSubSettings(KEYWORDS))
                    {
                        // Read the name out of the subkey
                        foreach (string id in keywordsKey.GetSubSettings())
                        {
                            using (ISettingsPersister categoryKey = keywordsKey.GetSubSettings(id))
                            {
                                string name = (string)categoryKey.Get(KEYWORD_NAME, typeof(string), id);
                                keywords.Add(new BlogPostKeyword(name));
                            }
                        }
                    }

                    if (keywords.Count > 0)
                        return (BlogPostKeyword[])keywords.ToArray(typeof(BlogPostKeyword));
                    else
                        return new BlogPostKeyword[0];
                }

            }
            set
            {
                lock (_keywordsLock)
                {
                    // safely delete existing categories
                    XmlSettingsPersister keywordPersister = KeywordPersister;
                    using (keywordPersister.BatchUpdate())
                    {
                        keywordPersister.UnsetSubSettingsTree(KEYWORDS);

                        // re-write keywords
                        using (ISettingsPersister keywordsKey = keywordPersister.GetSubSettings(KEYWORDS))
                        {
                            foreach (BlogPostKeyword keyword in value)
                            {
                                using (ISettingsPersister keywordKey = keywordsKey.GetSubSettings(keyword.Name))
                                {
                                    keywordKey.Set(KEYWORD_NAME, keyword.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        private const string KEYWORDS = "Keywords";
        private const string KEYWORD_NAME = "Name";
        private readonly static object _keywordsLock = new object();
        private string _keywordPath;
        /// <summary>
        /// The path to an xml file in the %APPDATA% folder that contains keywords for the current blog
        /// </summary>
        private string KeywordPath
        {
            get
            {
                if (string.IsNullOrEmpty(_keywordPath))
                {
                    string folderPath = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "Keywords");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    _keywordPath = Path.Combine(folderPath, String.Format(CultureInfo.InvariantCulture, "keywords_{0}.xml", Id));
                }
                return _keywordPath;
            }
        }

        private BlogPostCategory[] LegacyCategories
        {
            get
            {
                ArrayList categories = new ArrayList();
                using (SettingsPersisterHelper categoriesKey = Settings.GetSubSettings(CATEGORIES))
                {
                    foreach (string id in categoriesKey.GetNames())
                        categories.Add(new BlogPostCategory(id, categoriesKey.GetString(id, id)));
                }
                return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
            }
        }

        public AuthorInfo[] Authors
        {
            get
            {
                lock (_authorsLock)
                {
                    // get the authors
                    ArrayList authors = new ArrayList();
                    using (SettingsPersisterHelper authorsKey = Settings.GetSubSettings(AUTHORS))
                    {
                        foreach (string id in authorsKey.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper authorKey = authorsKey.GetSubSettings(id))
                            {
                                string name = authorKey.GetString(AUTHOR_NAME, String.Empty);
                                if (name != String.Empty)
                                    authors.Add(new AuthorInfo(id, name));
                                else
                                    Trace.Fail("Unexpected empty author name for id " + id);
                            }
                        }
                    }

                    return (AuthorInfo[])authors.ToArray(typeof(AuthorInfo));
                }
            }
            set
            {
                lock (_authorsLock)
                {
                    // safely delete existing
                    SettingsPersisterHelper settings = Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(AUTHORS);

                        // re-write
                        using (SettingsPersisterHelper authorsKey = settings.GetSubSettings(AUTHORS))
                        {
                            foreach (AuthorInfo author in value)
                            {
                                using (SettingsPersisterHelper authorKey = authorsKey.GetSubSettings(author.Id))
                                {
                                    authorKey.SetString(AUTHOR_NAME, author.Name);
                                }
                            }
                        }
                    }
                }
            }
        }
        private const string AUTHORS = "Authors";
        private const string AUTHOR_NAME = "Name";
        private readonly static object _authorsLock = new object();

        public PageInfo[] Pages
        {
            get
            {
                lock (_pagesLock)
                {
                    // get the authors
                    ArrayList pages = new ArrayList();
                    using (SettingsPersisterHelper pagesKey = Settings.GetSubSettings(PAGES))
                    {
                        foreach (string id in pagesKey.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper pageKey = pagesKey.GetSubSettings(id))
                            {
                                string title = pageKey.GetString(PAGE_TITLE, String.Empty);
                                DateTime datePublished = pageKey.GetDateTime(PAGE_DATE_PUBLISHED, DateTime.MinValue);
                                string parentId = pageKey.GetString(PAGE_PARENT_ID, String.Empty);
                                pages.Add(new PageInfo(id, title, datePublished, parentId));
                            }
                        }
                    }

                    return (PageInfo[])pages.ToArray(typeof(PageInfo));
                }
            }
            set
            {
                lock (_pagesLock)
                {
                    // safely delete existing
                    SettingsPersisterHelper settings = Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(PAGES);

                        // re-write
                        using (SettingsPersisterHelper pagesKey = settings.GetSubSettings(PAGES))
                        {
                            foreach (PageInfo page in value)
                            {
                                using (SettingsPersisterHelper pageKey = pagesKey.GetSubSettings(page.Id))
                                {
                                    pageKey.SetString(PAGE_TITLE, page.Title);
                                    pageKey.SetDateTime(PAGE_DATE_PUBLISHED, page.DatePublished);
                                    pageKey.SetString(PAGE_PARENT_ID, page.ParentId);
                                }
                            }
                        }
                    }
                }
            }
        }
        private const string PAGES = "Pages";
        private const string PAGE_TITLE = "Name";
        private const string PAGE_DATE_PUBLISHED = "DatePublished";
        private const string PAGE_PARENT_ID = "ParentId";
        private readonly static object _pagesLock = new object();

        public FileUploadSupport FileUploadSupport
        {
            get
            {
                int intVal = Settings.GetInt32(FILE_UPLOAD_SUPPORT, (Int32)FileUploadSupport.Weblog);
                switch (intVal)
                {
                    case (int)FileUploadSupport.FTP:
                        return FileUploadSupport.FTP;
                    case (int)FileUploadSupport.Weblog:
                    default:
                        return FileUploadSupport.Weblog;
                }
            }
            set { Settings.SetInt32(FILE_UPLOAD_SUPPORT, (Int32)value); }
        }
        private const string FILE_UPLOAD_SUPPORT = "FileUploadSupport";

        public IBlogFileUploadSettings FileUploadSettings
        {
            get
            {
                if (_fileUploadSettings == null)
                    _fileUploadSettings = new BlogFileUploadSettings(Settings.GetSubSettings("FileUploadSettings"));
                return _fileUploadSettings;
            }
        }
        private BlogFileUploadSettings _fileUploadSettings;

        public IBlogFileUploadSettings AtomPublishingProtocolSettings
        {
            get
            {
                if (_atomPublishingProtocolSettings == null)
                    _atomPublishingProtocolSettings = new BlogFileUploadSettings(Settings.GetSubSettings("AtomSettings"));
                return _atomPublishingProtocolSettings;
            }
        }
        private BlogFileUploadSettings _atomPublishingProtocolSettings;

        public BlogPublishingPluginSettings PublishingPluginSettings
        {
            get { return new BlogPublishingPluginSettings(Settings.GetSubSettings("PublishingPlugins")); }
        }

        /// <summary>
        /// Delete this profile
        /// </summary>
        public void Delete()
        {
            // dispose the profile
            Dispose();

            using (MetaLock(APPLY_UPDATES_LOCK))
            {
                // delete the underlying settings tree
                SettingsKey.UnsetSubsettingTree(_id);
            }

            // if we are the default profile then set the default to null
            if (_id == DefaultBlogId)
                DefaultBlogId = String.Empty;

            OnBlogSettingsDeleted(_id);
        }

        public void ApplyUpdates(IBlogSettingsDetectionContext settingsContext)
        {
            using (MetaLock(APPLY_UPDATES_LOCK))
            {
                if (BlogSettings.BlogIdIsValid(_id))
                {
                    if (settingsContext.ManifestDownloadInfo != null)
                        ManifestDownloadInfo = settingsContext.ManifestDownloadInfo;

                    if (settingsContext.ClientType != null)
                        ClientType = settingsContext.ClientType;

                    if (settingsContext.FavIcon != null)
                        FavIcon = settingsContext.FavIcon;

                    if (settingsContext.Image != null)
                        Image = settingsContext.Image;

                    if (settingsContext.WatermarkImage != null)
                        WatermarkImage = settingsContext.WatermarkImage;

                    if (settingsContext.Categories != null)
                        Categories = settingsContext.Categories;

                    if (settingsContext.Keywords != null)
                        Keywords = settingsContext.Keywords;

                    if (settingsContext.ButtonDescriptions != null)
                        ButtonDescriptions = settingsContext.ButtonDescriptions;

                    if (settingsContext.OptionOverrides != null)
                        OptionOverrides = settingsContext.OptionOverrides;

                    if (settingsContext.HomePageOverrides != null)
                        HomePageOverrides = settingsContext.HomePageOverrides;
                }
                else
                {
                    throw new InvalidOperationException("Attempted to apply updates to invalid blog-id");
                }
            }
        }

        public static IDisposable ApplyUpdatesLock(string id)
        {
            return _metaLock.Lock(APPLY_UPDATES_LOCK + id);
        }

        private static readonly MetaLock _metaLock = new MetaLock();
        private IDisposable MetaLock(string contextName)
        {
            return _metaLock.Lock(contextName + _id);
        }
        private const string APPLY_UPDATES_LOCK = "ApplyUpdates";

        public void Dispose()
        {
            if (_blogCredentials != null)
            {
                _blogCredentials.Dispose();
                _blogCredentials = null;
            }

            if (_fileUploadSettings != null)
            {
                _fileUploadSettings.Dispose();
                _fileUploadSettings = null;
            }

            if (_atomPublishingProtocolSettings != null)
            {
                _atomPublishingProtocolSettings.Dispose();
                _atomPublishingProtocolSettings = null;
            }

            if (_settings != null)
            {
                _settings.Dispose();
                _settings = null;
            }

            // This block is unsafe because it's easy for a persister
            // to be disposed while it's still being used on another
            // thread.

            // if (_keywordPersister.ContainsKey(KeywordPath))
            // {
            //    _keywordPersister[KeywordPath].Dispose();
            //    _keywordPersister.Remove(KeywordPath);
            // }

            GC.SuppressFinalize(this);
        }

        ~BlogSettings()
        {
            Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Failed to dispose BlogSettings!!! BlogId: {0} // BlogName: {1}", Id, BlogName));
        }

        public IBlogFileUploadSettings FileUpload
        {
            get
            {
                return FileUploadSettings;
            }
        }

        /// <summary>
        /// Key for this weblog
        /// </summary>
        private SettingsPersisterHelper Settings
        {
            get
            {
                if (_settings == null)
                    _settings = GetWeblogSettingsKey(_id);
                return _settings;
            }
        }
        private SettingsPersisterHelper _settings;

        #region Class Configuration (location of settings, etc)

        public static SettingsPersisterHelper GetProviderButtonsSettingsKey(string blogId)
        {
            return GetWeblogSettingsKey(blogId).GetSubSettings(BUTTONS_KEY);
        }

        public static SettingsPersisterHelper GetWeblogSettingsKey(string blogId)
        {
            return SettingsKey.GetSubSettings(blogId);
        }

        public static SettingsPersisterHelper SettingsKey
        {
            get
            {
                return _settingsKey;
            }

        }

        private static SettingsPersisterHelper _settingsKey = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Weblogs");

        #endregion

    }

    public class BlogCredentials : IBlogCredentials, IDisposable
    {
        public BlogCredentials(SettingsPersisterHelper settingsRoot, ICredentialsDomain domain)
        {
            _settingsRoot = settingsRoot;
            _domain = domain;
        }

        public string Username
        {
            get { return GetUsername(); }
            set { CredentialsSettings.SetString(USERNAME, value); }
        }
        private const string USERNAME = "Username";

        public string Password
        {
            get
            {
                return GetPassword() ?? string.Empty;
            }
            set
            {
                // save an encrypted password
                try
                {
                    CredentialsSettings.SetEncryptedString(PASSWORD, value);
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to encrypt weblog password: " + e.Message, e.StackTrace);
                }
            }
        }
        private const string PASSWORD = "Password";

        public string[] CustomValues
        {
            get
            {
                ArrayList customValues = new ArrayList();
                string[] names = CredentialsSettings.GetNames();
                foreach (string name in names)
                    if (name != USERNAME && name != PASSWORD)
                        customValues.Add(name);
                return customValues.ToArray(typeof(string)) as string[];
            }
        }

        public string GetCustomValue(string name)
        {
            return CredentialsSettings.GetString(name, String.Empty);
        }

        public void SetCustomValue(string name, string value)
        {
            CredentialsSettings.SetString(name, value);
        }

        public void Clear()
        {
            Username = String.Empty;
            Password = String.Empty;
            foreach (string name in CredentialsSettings.GetNames())
                CredentialsSettings.SetString(name, null);
        }

        public ICredentialsDomain Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }
        private ICredentialsDomain _domain;

        public void Dispose()
        {
            if (_credentialsSettingsRoot != null)
                _credentialsSettingsRoot.Dispose();
        }

        private SettingsPersisterHelper CredentialsSettings
        {
            get
            {
                if (_credentialsSettingsRoot == null)
                    _credentialsSettingsRoot = _settingsRoot.GetSubSettings("Credentials");
                return _credentialsSettingsRoot;
            }
        }

        /// <summary>
        /// Get Username from either the credentials key or the root key
        /// (seamless migration of accounts that existed prior to us moving
        /// the credentials into their own subkey)
        /// </summary>
        /// <returns></returns>
        private string GetUsername()
        {
            string username = CredentialsSettings.GetString(USERNAME, null);
            if (username != null)
                return username;
            else
                return _settingsRoot.GetString(USERNAME, String.Empty);
        }

        /// <summary>
        /// Get Password from either the credentials key or the root key
        /// (seamless migration of accounts that existed prior to us moving
        /// the credentials into their own subkey)
        /// </summary>
        /// <returns></returns>
        private string GetPassword()
        {
            string password = CredentialsSettings.GetEncryptedString(PASSWORD);
            if (password != null)
                return password;
            else
                return _settingsRoot.GetEncryptedString(PASSWORD);
        }

        private SettingsPersisterHelper _credentialsSettingsRoot;
        private SettingsPersisterHelper _settingsRoot;
    }

    public class BlogFileUploadSettings : IBlogFileUploadSettings, IDisposable
    {
        public BlogFileUploadSettings(SettingsPersisterHelper settings)
        {
            _settings = settings;
        }

        public string GetValue(string name)
        {
            return _settings.GetString(name, String.Empty);
        }

        public void SetValue(string name, string value)
        {
            _settings.SetString(name, value);
        }

        public string[] Names
        {
            get { return _settings.GetNames(); }
        }

        public void Dispose()
        {
            if (_settings != null)
            {
                _settings.Dispose();
                _settings = null;
            }
        }

        private SettingsPersisterHelper _settings;
    }

}
