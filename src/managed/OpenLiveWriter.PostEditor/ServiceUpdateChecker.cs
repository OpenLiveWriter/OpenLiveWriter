// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.PostEditor.Configuration;
using System.Security.AccessControl;

namespace OpenLiveWriter.PostEditor
{

    internal class ServiceUpdateChecker
    {
        public ServiceUpdateChecker(string blogId, WeblogSettingsChangedHandler settingsChangedHandler)
        {
            _blogId = blogId;
            _settingsChangedHandler = settingsChangedHandler;
        }

        public void Start()
        {
            Thread checkerThread = ThreadHelper.NewThread(new ThreadStart(Main), "ServiceUpdateChecker", true, true, true);
            checkerThread.Start();
        }

        private void Main()
        {
            try
            {
                // delay the check for updates
                Thread.Sleep(1000);

                // only run one service-update at a time process wide
                lock (_serviceUpdateLock)
                {
                    // establish settings detection context
                    ServiceUpdateSettingsDetectionContext settingsDetectionContext = new ServiceUpdateSettingsDetectionContext(_blogId);

                    // fire-up a blog settings detector to query for changes
                    BlogSettingsDetector settingsDetector = new BlogSettingsDetector(settingsDetectionContext);
                    settingsDetector.SilentMode = true;
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ApplicationEnvironment.SettingsRootKeyName + @"\Weblogs\" + _blogId + @"\HomepageOptions"))
                    {
                        if (key != null)
                        {
                            settingsDetector.IncludeFavIcon = false;
                            settingsDetector.IncludeCategories = settingsDetectionContext.BlogSupportsCategories;
                            settingsDetector.UseManifestCache = true;
                            settingsDetector.IncludeHomePageSettings = false;
                            settingsDetector.IncludeCategoryScheme = false;
                            settingsDetector.IncludeInsecureOperations = false;
                        }
                    }
                    settingsDetector.IncludeImageEndpoints = false;
                    settingsDetector.DetectSettings(SilentProgressHost.Instance);

                    // write the settings
                    using (ProcessKeepalive.Open())
                    {
                        using (BlogSettings settings = BlogSettings.ForBlogId(_blogId))
                            settings.ApplyUpdates(settingsDetectionContext);
                    }

                    // if changes were made then fire an event to notify the UI
                    if (settingsDetectionContext.HasUpdates)
                    {
                        _settingsChangedHandler(_blogId, false);
                    }
                }

            }
            catch (ManualKeepaliveOperationException)
            {
            }
            catch (BlogClientInvalidServerResponseException ex)
            {
                // Server response errors are expected during service updates (e.g., API changes, server issues)
                // Log without triggering assertion dialogs since this is a background check
                Trace.WriteLine("ServiceUpdateChecker: Server response error (non-fatal): " + ex.Message);
            }
            catch (Exception ex)
            {
                // Log other unexpected exceptions without showing assertion dialogs
                // since this is a background service update check that should not interrupt the user
                Trace.WriteLine("ServiceUpdateChecker: Unexpected exception (non-fatal): " + ex.ToString());
            }
        }

        private string _blogId;
        private WeblogSettingsChangedHandler _settingsChangedHandler;
        private readonly static object _serviceUpdateLock = new object();
    }

    internal class ServiceUpdateSettingsDetectionContext : IBlogSettingsDetectionContextForCategorySchemeHack
    {
        public ServiceUpdateSettingsDetectionContext(string blogId)
        {
            using (BlogSettings settings = BlogSettings.ForBlogId(blogId))
            {
                _blogId = blogId;
                _homepageUrl = settings.HomepageUrl;
                _providerId = settings.ProviderId;
                _manifestDownloadInfo = settings.ManifestDownloadInfo;
                _hostBlogId = settings.HostBlogId;
                _postApiUrl = settings.PostApiUrl;
                _clientType = settings.ClientType;
                _userOptionOverrides = settings.UserOptionOverrides;
                _homepageOptionOverrides = settings.HomePageOverrides;

                _initialCategoryScheme = settings.OptionOverrides != null
                    ? settings.OptionOverrides[BlogClientOptions.CATEGORY_SCHEME] as string
                    : null;

                BlogCredentialsHelper.Copy(settings.Credentials, _credentials);

                using (Blog blog = new Blog(settings))
                    _blogSupportsCategories = blog.ClientOptions.SupportsCategories;

                _initialBlogSettingsContents = GetBlogSettingsContents(settings);
                _initialCategoriesContents = GetCategoriesContents(settings.Categories);

            }
        }

        public bool HasUpdates
        {
            get
            {
                // If all of our manifest fields are null that means we either
                // don't support manifests or we are using a cached manifest.
                // In this case just compare categories.
                if (Image == null && WatermarkImage == null && ButtonDescriptions == null && OptionOverrides == null && HomePageOverrides == null)
                {
                    string updatedCategories = GetCategoriesContents(Categories);
                    return _initialCategoriesContents != updatedCategories;
                }
                else
                {
                    string updatedSettingsContents = GetSettingsContents();
                    return _initialBlogSettingsContents != updatedSettingsContents;
                }
            }
        }

        public bool BlogSupportsCategories
        {
            get { return _blogSupportsCategories; }
        }
        private bool _blogSupportsCategories;

        public string PostApiUrl
        {
            get
            {
                return _postApiUrl;
            }
        }
        private string _postApiUrl;

        public IBlogCredentialsAccessor Credentials
        {
            get
            {
                return new BlogCredentialsAccessor(_blogId, _credentials);
            }
        }
        private IBlogCredentials _credentials = new TemporaryBlogCredentials();

        public string HomepageUrl
        {
            get
            {
                return _homepageUrl;
            }
        }
        private string _homepageUrl;

        public string ProviderId
        {
            get
            {
                return _providerId;
            }
        }
        private string _providerId;

        public string HostBlogId
        {
            get
            {
                return _hostBlogId;
            }
        }
        private string _hostBlogId;

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
        private WriterEditingManifestDownloadInfo _manifestDownloadInfo;

        public string ClientType
        {
            get
            {
                return _clientType;
            }
            set
            {
                _clientType = value;
            }
        }
        private string _clientType;

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
        private byte[] _favIcon;

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
        private byte[] _image;

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
        private byte[] _watermarkImage;

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
        private BlogPostCategory[] _categories;

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
        private BlogPostKeyword[] _keywords;

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get
            {
                return _buttonDescriptions;
            }
            set
            {
                _buttonDescriptions = value;
            }
        }
        private IBlogProviderButtonDescription[] _buttonDescriptions;

        public IDictionary UserOptionOverrides
        {
            get
            {
                return _userOptionOverrides;
            }
        }
        private IDictionary _userOptionOverrides;

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

        public string InitialCategoryScheme
        {
            get
            {
                if (_optionOverrides != null && _optionOverrides.Contains(BlogClientOptions.CATEGORY_SCHEME))
                    return _optionOverrides[BlogClientOptions.CATEGORY_SCHEME] as string;
                return _initialCategoryScheme;
            }
        }

        private IDictionary _homepageOptionOverrides;
        private IDictionary _optionOverrides;

        private string _blogId;

        private string _initialBlogSettingsContents;
        private string _initialCategoriesContents;
        private readonly string _initialCategoryScheme;

        private string GetBlogSettingsContents(BlogSettings blogSettings)
        {
            // return normalized string of settings contents
            StringBuilder settingsContents = new StringBuilder();
            AppendClientType(blogSettings.ClientType, settingsContents);
            AppendImages(blogSettings.Image, blogSettings.WatermarkImage, settingsContents);
            AppendCategories(blogSettings.Categories, settingsContents);
            AppendButtons(blogSettings.ButtonDescriptions, settingsContents);
            AppendOptionOverrides(blogSettings.OptionOverrides, settingsContents);
            AppendOptionOverrides(blogSettings.HomePageOverrides, settingsContents);
            return settingsContents.ToString();
        }

        private string GetSettingsContents()
        {
            StringBuilder settingsContents = new StringBuilder();
            AppendClientType(ClientType, settingsContents);
            AppendImages(Image, WatermarkImage, settingsContents);
            AppendCategories(Categories, settingsContents);
            AppendButtons(ButtonDescriptions, settingsContents);
            AppendOptionOverrides(OptionOverrides, settingsContents);
            AppendOptionOverrides(HomePageOverrides, settingsContents);
            return settingsContents.ToString();
        }

        private void AppendManifestDownloadInfo(WriterEditingManifestDownloadInfo manifestDownloadInfo, StringBuilder settingsContents)
        {
            if (manifestDownloadInfo != null)
            {
                settingsContents.AppendFormat(CultureInfo.InvariantCulture,
                    "ManifestUrl:{0}ManifestExpires:{1}ManifestLastModified:{2}ManifestEtag:{3}",
                    manifestDownloadInfo.SourceUrl,
                    manifestDownloadInfo.Expires,
                    manifestDownloadInfo.LastModified,
                    manifestDownloadInfo.ETag);
            }
        }

        private void AppendClientType(string clientType, StringBuilder settingsContents)
        {
            if (clientType != null)
                settingsContents.AppendFormat("ClientType:{0}", clientType);
        }

        private void AppendImages(byte[] image, byte[] watermarkImage, StringBuilder settingsContents)
        {
            if (image != null && image.Length > 0)
            {
                settingsContents.Append("Image:");
                foreach (byte imageByte in image)
                    settingsContents.Append(imageByte);
            }

            if (watermarkImage != null && watermarkImage.Length > 0)
            {
                settingsContents.Append("WatemarkImage:");
                foreach (byte watermarkImageByte in watermarkImage)
                    settingsContents.Append(watermarkImageByte);
            }
        }

        private void AppendCategories(BlogPostCategory[] categories, StringBuilder settingsContents)
        {
            settingsContents.Append(GetCategoriesContents(categories));
        }

        private string GetCategoriesContents(BlogPostCategory[] categories)
        {
            StringBuilder categoriesBuilder = new StringBuilder();
            if (categories != null)
            {
                Array.Sort(categories, new SortCategoriesComparer());
                foreach (BlogPostCategory category in categories)
                    categoriesBuilder.AppendFormat("Category:{0}/{1}/{2}", category.Id, category.Name, category.Parent);
            }
            return categoriesBuilder.ToString();
        }

        private class SortCategoriesComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x as BlogPostCategory).Id.CompareTo((y as BlogPostCategory).Id);
            }
        }

        private void AppendButtons(IBlogProviderButtonDescription[] buttons, StringBuilder settingsContents)
        {
            if (buttons != null)
            {
                Array.Sort(buttons, new SortButtonsComparer());
                foreach (IBlogProviderButtonDescription button in buttons)
                    settingsContents.AppendFormat(CultureInfo.InvariantCulture, "Button:{0}/{1}/{2}/{3}/{4}/{5}/{6}", button.Id, button.Description, button.ImageUrl, button.ClickUrl, button.ContentUrl, button.ContentDisplaySize, button.NotificationUrl);
            }
        }

        private class SortButtonsComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x as IBlogProviderButtonDescription).Id.CompareTo((y as IBlogProviderButtonDescription).Id);
            }
        }

        private void AppendOptionOverrides(IDictionary optionOverrides, StringBuilder settingsContents)
        {
            if (optionOverrides != null)
            {
                ArrayList optionOverrideList = new ArrayList();
                foreach (DictionaryEntry optionOverride in optionOverrides)
                    optionOverrideList.Add(optionOverride);

                optionOverrideList.Sort(new SortOptionOverridesComparer());

                foreach (DictionaryEntry optionOverride in optionOverrideList)
                    settingsContents.AppendFormat("OptionOverride:{0}/{1}", optionOverride.Key, optionOverride.Value);
            }
        }

        private class SortOptionOverridesComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                string xKey = ((DictionaryEntry)x).Key.ToString();
                string yKey = ((DictionaryEntry)y).Key.ToString();
                return xKey.CompareTo(yKey);
            }
        }

    }

}
