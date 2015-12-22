// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient
{
    public interface IBlogSettingsAccessor : IDisposable
    {
        string Id { get; }

        bool IsSpacesBlog { get; }
        bool IsSharePointBlog { get; }
        bool IsGoogleBloggerBlog { get; }

        string HostBlogId { get; }
        string BlogName { get; }

        string HomepageUrl { get; }
        bool ForceManualConfig { get; }
        WriterEditingManifestDownloadInfo ManifestDownloadInfo { get; }

        string ProviderId { get; }
        string ServiceName { get; }

        string ClientType { get; }
        string PostApiUrl { get; }

        IDictionary OptionOverrides { get; }
        IDictionary UserOptionOverrides { get; }
        IDictionary HomePageOverrides { get; }

        IBlogCredentialsAccessor Credentials { get; }

        IBlogProviderButtonDescription[] ButtonDescriptions { get; }

        bool LastPublishFailed { get; set; }

        byte[] FavIcon { get; }
        byte[] Image { get; }
        byte[] WatermarkImage { get; }

        PageInfo[] Pages { get; set; }
        AuthorInfo[] Authors { get; set; }

        BlogPostCategory[] Categories { get; set; }
        BlogPostKeyword[] Keywords { get; set; }

        FileUploadSupport FileUploadSupport { get; }
        IBlogFileUploadSettings FileUploadSettings { get; }

        IBlogFileUploadSettings AtomPublishingProtocolSettings { get; }

        BlogPublishingPluginSettings PublishingPluginSettings { get; }
    }

    public class BlogPublishingPluginSettings
    {
        private readonly SettingsPersisterHelper _settings;

        public BlogPublishingPluginSettings(SettingsPersisterHelper settings)
        {
            _settings = settings;
        }

        public void ClearOrder()
        {
            foreach (string name in _settings.GetNames())
                _settings.Unset(name);
        }

        public void ClearOrderAndSettings()
        {
            foreach (string name in _settings.GetNames())
                _settings.Unset(name);
            foreach (string name in _settings.GetSubSettingNames())
                _settings.UnsetSubsettingTree(name);
        }

        public string[] KnownPluginIds
        {
            get
            {
                return _settings.GetNames();
            }
        }

        public bool? IsEnabled(string pluginId)
        {
            switch (GetStringValue(pluginId, 0))
            {
                case "1": return true;
                case "0": return false;
                default: return null;
            }
        }

        public int? GetOrder(string pluginId)
        {
            int order;
            if (int.TryParse(GetStringValue(pluginId, 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out order))
                return order;
            return null;
        }

        public void Set(string pluginId, bool enabled, int order)
        {
            _settings.SetString(pluginId,
                string.Format(CultureInfo.InvariantCulture, "{0},{1}", enabled ? "1" : "0", order));
        }

        private string GetStringValue(string pluginId, int index)
        {
            string strVal = _settings.GetString(pluginId, null);
            if (strVal == null)
                return null;
            string[] chunks = strVal.Split(',');
            if (chunks.Length < index + 1)
                return null;
            return chunks[index];
        }

        public void CopyTo(BlogPublishingPluginSettings settings)
        {
            settings.ClearOrderAndSettings();
            settings._settings.CopyFrom(_settings, true, true);
        }
    }

    public enum FileUploadSupport
    {
        Weblog = 1,  // Weblog=1 for backcompat reasons, we used to have None=0
        FTP,
    };

    public interface IBlogFileUploadSettings
    {
        string GetValue(string name);
        void SetValue(string name, string value);
        string[] Names { get; }
    }

    public class WriterEditingManifestDownloadInfo
    {
        public WriterEditingManifestDownloadInfo(string sourceUrl)
            : this(sourceUrl, DateTime.MinValue, DateTime.MinValue, String.Empty)
        {
        }

        public WriterEditingManifestDownloadInfo(string sourceUrl, DateTime expires, DateTime lastModified, string eTag)
        {
            _sourceUrl = sourceUrl;
            _expires = expires;
            _lastModified = lastModified;
            _eTag = eTag;
        }

        public string SourceUrl
        {
            get { return _sourceUrl; }
        }
        private readonly string _sourceUrl;

        public DateTime Expires
        {
            get { return _expires; }
        }
        private readonly DateTime _expires;

        public DateTime LastModified
        {
            get { return _lastModified; }
        }
        private readonly DateTime _lastModified;

        public string ETag
        {
            get { return _eTag; }
        }
        private readonly string _eTag;
    }

}
