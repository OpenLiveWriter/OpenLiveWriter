// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.ImageInsertion.WebImages;
using OpenLiveWriter.PostEditor.LiveClipboard;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.PostEditor.Tagging;
using OpenLiveWriter.PostEditor.Video;
using OpenLiveWriter.InternalWriterPlugin;
using mshtml;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    internal interface IContentSourceSite : IPublishingContext
    {
        IWin32Window DialogOwner { get; }

        IExtensionData CreateExtensionData(string id);

        bool InsertCommandsEnabled { get; }

        string SelectedHtml { get; }

        void Focus();

        void InsertContent(string content, bool select);
        void InsertContent(string contentSourceId, string content, IExtensionData extensionData);
        void InsertContent(string contentSourceId, string content, IExtensionData extensionData, HtmlInsertionOptions insertionOptions);

        /// <summary>
        /// Given a list of contentIds the IContentSourceSite will find the Ids still in use and
        /// tell the SmartContentSource to update those smart content elements in the post.
        /// </summary>
        /// <param name="extensionDataList">List of contentIds that will be updated, this list can contain nulls</param>
        /// <returns>It returns a list of extensiondata that was no updated because it wasnt found in the editor</returns>
        IExtensionData[] UpdateContent(IExtensionData[] extensionDataList);
    }

    public class SmartContentItem
    {
        public SmartContentItem(string contentSourceId, IExtensionData extensionData)
            : this(contentSourceId, extensionData.Id, new SmartContent(extensionData))
        {
        }

        public SmartContentItem(string contentSourceId, string contentBlockId, SmartContent smartContent)
        {
            _contentSourceId = contentSourceId;
            _contentBlockId = contentBlockId;
            _smartContent = smartContent;
        }

        public string ContainingElementId { get { return ContentSourceManager.MakeContainingElementId(ContentSourceId, ContentBlockId); } }
        public string ContentSourceId { get { return _contentSourceId; } }
        public string ContentBlockId { get { return _contentBlockId; } }
        public SmartContent SmartContent { get { return _smartContent; } }

        private string _contentSourceId;
        private string _contentBlockId;
        private SmartContent _smartContent;
    }

    public class ContentSourceInfo
    {
        private ResourceManager _resMan;

        public ContentSourceInfo(Type pluginType, bool showErrors)
        {
            // save a refernce to the type
            _pluginType = pluginType;
            _resMan = new ResourceManager(_pluginType);

            // initialize list of live clipboard attributes
            ArrayList liveClipboardFormats = new ArrayList();

            // initialize our metadata from the Type's attributes
            object[] attributes = pluginType.GetCustomAttributes(true);
            foreach (object attribute in attributes)
            {
                if (attribute is WriterPluginAttribute)
                {
                    _writerPlugin = attribute as WriterPluginAttribute;
                    VerifyPluginBitmap(showErrors);
                }
                else if (attribute is InsertableContentSourceAttribute)
                {
                    _insertableContentSource = attribute as InsertableContentSourceAttribute;
                }
                else if (attribute is UrlContentSourceAttribute)
                {
                    _urlContentSource = attribute as UrlContentSourceAttribute;
                }
                else if (attribute is LiveClipboardContentSourceAttribute)
                {
                    LiveClipboardContentSourceAttribute liveClipboardContentSourceAttribute = attribute as LiveClipboardContentSourceAttribute;
                    liveClipboardFormats.Add(new LiveClipboardFormatHandler(liveClipboardContentSourceAttribute, this));
                }
                else if (attribute is CustomLocalizedPluginAttribute)
                {
                    _customLocalizedPlugin = attribute as CustomLocalizedPluginAttribute;
                }
            }

            // copy array of lc attributes
            _liveClipboardFormatHandlers = liveClipboardFormats.ToArray(typeof(LiveClipboardFormatHandler)) as LiveClipboardFormatHandler[];

            if (_writerPlugin == null)
                throw new WriterContentPluginAttributeMissingException(pluginType);

            if (!CanCreateNew && !CanCreateFromUrl && (LiveClipboardFormatHandlers.Length == 0) &&
                !(typeof(PublishNotificationHook).IsAssignableFrom(pluginType)
                  || typeof(HeaderFooterSource).IsAssignableFrom(pluginType)))
                throw new WriterContentPluginAttributeMissingException(pluginType);

            // initialize settings
            _settings = _parentSettings.GetSubSettings(Id);
        }

        ~ContentSourceInfo()
        {
            // Explicit dispose of managed objects not necessary from finalizer
            // try { _settings.Dispose(); }catch{}
        }

        // plugin type
        public Type Type { get { return _pluginType; } }

        // convenience accessors for most frequently requested properties
        public string Id { get { return _writerPlugin.Id; } }
        public string Name
        {
            get
            {
                string name = GetLocalizedString("WriterPlugin.Name", _writerPlugin.Name);
                int nameLength = name.Length;
                if (name != null)
                    name = name.Substring(0, Math.Min(name.Length, MAX_NAME_LENGTH)).Trim();
                return name;
            }
        }
        private const int MAX_NAME_LENGTH = 256;

        public Bitmap Image
        {
            get
            {
                Bitmap bitmap;

                Debug.Assert(_pluginType.Assembly.ManifestModule.Name != "OpenLiveWriter.WriterPlugin.dll" && _pluginType.Assembly.ManifestModule.Name != "OpenLiveWriter.PostEditor.dll", "1st Party plugins should not have their image loaded.  It should come from the ribbon");

                if (_writerPlugin.ImagePath != null)
                    bitmap = ResourceHelper.LoadAssemblyResourceBitmap(_pluginType.Assembly, _pluginType.Namespace, _writerPlugin.ImagePath, false);
                else
                    bitmap = ResourceHelper.LoadAssemblyResourceBitmap("ContentSources.Images.DefaultPluginImage.png");

                if (bitmap.Width != IMAGE_WIDTH || bitmap.Height != IMAGE_HEIGHT)
                {
                    if (HasTransparentBorder(bitmap))
                        bitmap = ImageHelper2.CropBitmap(bitmap, GetCenterRectangle(bitmap));
                    else
                        bitmap = ImageHelper2.CreateResizedBitmap(bitmap, IMAGE_WIDTH, IMAGE_HEIGHT, bitmap.RawFormat);
                }
                return bitmap;
            }
        }

        private Rectangle GetCenterRectangle(Bitmap bitmap)
        {
            return new Rectangle(
                (bitmap.Width - IMAGE_WIDTH) / 2,
                (bitmap.Height - IMAGE_HEIGHT) / 2,
                IMAGE_WIDTH,
                IMAGE_HEIGHT
                );
        }

        private bool HasTransparentBorder(Bitmap bitmap)
        {
            if (bitmap.Width < IMAGE_WIDTH || bitmap.Height < IMAGE_HEIGHT)
                return false;

            Rectangle rect = GetCenterRectangle(bitmap);
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    if (!rect.Contains(x, y) && bitmap.GetPixel(x, y).A > 0)
                        return false;
            return true;
        }

        private string GetLocalizedString(string name, string defaultValue)
        {
            if (_customLocalizedPlugin != null)
            {
                string fullName = "Plugin." + _customLocalizedPlugin.Name + "." + name;
                fullName = fullName.Replace('.', '_');
                string customValue = Res.Get(fullName);
                return (customValue != null) ? customValue : defaultValue;
            }

            if (_resMan == null)
                return defaultValue;

            try
            {
                string result = _resMan.GetString(name);
                if (result == null)
                    return defaultValue;
                return result;
            }
            catch (MissingManifestResourceException)
            {
                _resMan = null;
                return defaultValue;
            }
        }

        private CustomLocalizedPluginAttribute _customLocalizedPlugin;

        // core plugin attributes
        private WriterPluginAttribute _writerPlugin;
        public bool WriterPluginHasEditableOptions { get { return _writerPlugin.HasEditableOptions; } }
        public string WriterPluginPublisherUrl { get { return _writerPlugin.PublisherUrl; } }
        public string WriterPluginDescription { get { return GetLocalizedString("WriterPlugin.Description", _writerPlugin.Description); } }

        // insertable content source
        private InsertableContentSourceAttribute _insertableContentSource;
        public bool CanCreateNew { get { return _insertableContentSource != null; } }
        public string InsertableContentSourceMenuText
        {
            get
            {
                string menuText = GetLocalizedString("InsertableContentSource.MenuText", _insertableContentSource.MenuText);
                if (menuText != null)
                    menuText = menuText.Substring(0, Math.Min(menuText.Length, MAX_MENU_TEXT_LENGTH)).Trim();
                return menuText;
            }
        }
        private const int MAX_MENU_TEXT_LENGTH = 50;

        public string InsertableContentSourceSidebarText
        {
            get
            {
                string sidebarText = GetLocalizedString("InsertableContentSource.SidebarText", _insertableContentSource.SidebarText);
                if (sidebarText != null)
                    sidebarText = sidebarText.Substring(0, Math.Min(sidebarText.Length, MAX_SIDEBAR_TEXT_LENGTH)).Trim();
                return sidebarText;
            }
        }
        private const int MAX_SIDEBAR_TEXT_LENGTH = 20;

        // url content source
        private UrlContentSourceAttribute _urlContentSource;
        public bool CanCreateFromUrl { get { return _urlContentSource != null; } }
        public string UrlContentSourceUrlPattern { get { return _urlContentSource.UrlPattern; } }
        public bool UrlContentSourceRequiresProgress { get { return _urlContentSource != null && _urlContentSource.RequiresProgress; } }
        public string UrlContentSourceProgressCaption { get { return GetLocalizedString("UrlContentSource.ProgressCaption", _urlContentSource.ProgressCaption); } }
        public string UrlContentSourceProgressMessage { get { return GetLocalizedString("UrlContentSource.ProgressMessage", _urlContentSource.ProgressMessage); } }

        // live clipboard content source metadata
        internal LiveClipboardFormatHandler[] LiveClipboardFormatHandlers { get { return _liveClipboardFormatHandlers; } }
        private LiveClipboardFormatHandler[] _liveClipboardFormatHandlers;

        public bool Enabled
        {
            get { return _settings.GetBoolean(ENABLED, true); }
            set { _settings.SetBoolean(ENABLED, value); }
        }
        private const string ENABLED = "Enabled";

        public DateTime LastUse
        {
            get { return _settings.GetDateTime(LAST_USE, DateTime.Now); }
            set { _settings.SetDateTime(LAST_USE, value); }
        }
        private const string LAST_USE = "LastUse";

        public WriterPlugin Instance
        {
            get
            {
                lock (this)
                {
                    if (_plugin == null)
                    {
                        _plugin = (WriterPlugin)Activator.CreateInstance(_pluginType);
                        _plugin.Initialize(new PluginSettingsAdaptor(_settings));
                    }

                    return _plugin;
                }
            }
        }

        public override bool Equals(object obj)
        {
            return (obj as ContentSourceInfo).Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        internal class LastUseComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return -(x as ContentSourceInfo).LastUse.CompareTo((y as ContentSourceInfo).LastUse);
            }
        }

        internal class NameComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x as ContentSourceInfo).Name.CompareTo((y as ContentSourceInfo).Name);
            }
        }

        internal const int IMAGE_WIDTH = 16;
        internal const int IMAGE_HEIGHT = 16;

        private string VerifyAttributeValue(Type pluginType, object attribute, string attributeField, string attributeValue)
        {
            if (attributeValue != null && attributeValue != String.Empty)
                return attributeValue;
            else
                throw new PluginAttributeFieldMissingException(pluginType, attribute.GetType(), attributeField);
        }

        private void VerifyPluginBitmap(bool showErrors)
        {
            if (_writerPlugin.ImagePath != null)
            {
                try
                {
                    // try to load bitmap
                    using (Bitmap bitmap = new Bitmap(_pluginType, _writerPlugin.ImagePath))
                    {
                        // verify bitmap size
                        if ((bitmap.Width != IMAGE_WIDTH || bitmap.Height != IMAGE_HEIGHT) && (bitmap.Width != 20 || bitmap.Height != 18))
                        {
                            string errorText = String.Format(CultureInfo.CurrentCulture, "Warning: The bitmap for plugin {0} is not the correct size (it should be 16x16 or 20x18).", _pluginType.Name);
                            Trace.Fail(errorText);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to load plugin bitmap: " + e.ToString());
                    if (showErrors)
                        DisplayMessage.Show(MessageId.PluginBitmapLoadError, _pluginType.Name, _writerPlugin.ImagePath);
                    // set image path to null so that the "empty" image is used
                    _writerPlugin.ImagePath = null;
                }
            }
        }

        private Regex VerifyRegex(Type pluginType, string regex)
        {
            try
            {
                return new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }
            catch
            {
                throw new PluginAttributeInvalidRegexException(pluginType, regex);
            }
        }

        private Type _pluginType;
        private WriterPlugin _plugin;
        private SettingsPersisterHelper _settings;
        private static SettingsPersisterHelper _parentSettings = PostEditorSettings.SettingsKey.GetSubSettings("ContentSources");
    }

    internal sealed class ContentSourceManager
    {
        /// <summary>
        /// Method that can force early initialization of the content-source manager
        /// so that plugin load errors occur prior to the load of the PostEditorForm
        /// </summary>
        public static void Initialize()
        {
            Initialize(null);
        }

        private static bool _loaded;

        public static void Initialize(bool? enablePlugins)
        {
            if (_loaded)
            {
                Debug.Fail("ContentSourceManager should not be initialized more then once per process.");
                return;
            }

            _loaded = true;
            _pluginsEnabledOverride = enablePlugins;

            try
            {
                if (PluginsEnabled)
                {
                    PostEditorPluginManager.Init();
                }

                // initialize content sources
                RefreshContentSourceLists(true);

                if (PluginsEnabled)
                {
                    // subscribe to changed event for plugin list
                    PostEditorPluginManager.Instance.PluginListChanged += new EventHandler(Instance_PluginListChanged);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexptected exception initializing content-sources: " + ex.ToString());
            }

            ContentSourceInfo[] contentSources = PluginContentSources;
        }

        internal static event EventHandler GlobalContentSourceListChanged;

        private static void OnGlobalContentSourceListChanged()
        {
            if (GlobalContentSourceListChanged != null)
                GlobalContentSourceListChanged(null, EventArgs.Empty);
        }

        private static void RefreshContentSourceLists(bool showErrors)
        {
            lock (_contentSourceListLock)
            {
                // list of built-in content sources
                ArrayList builtInContentSources = new ArrayList();
                if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.VideoProviders))
                    AddContentSource(builtInContentSources, typeof(VideoContentSource), showErrors);
                if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.Maps))
                    AddContentSource(builtInContentSources, typeof(MapContentSource), showErrors);
                if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.TagProviders))
                    AddContentSource(builtInContentSources, typeof(TagContentSource), showErrors);

                AddContentSource(builtInContentSources, typeof(WebImageContentSource), showErrors);

                builtInContentSources.Sort(new ContentSourceInfo.NameComparer());
                _builtInContentSources = builtInContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];

                // list of plugin content sources
                ArrayList pluginContentSources = new ArrayList();
                if (PluginsEnabled)
                {
                    Type[] pluginTypes = PostEditorPluginManager.Instance.GetPlugins(typeof(WriterPlugin));
                    foreach (Type type in pluginTypes)
                        AddContentSource(pluginContentSources, type, showErrors);
                    pluginContentSources.Sort(new ContentSourceInfo.NameComparer());
                }
                _pluginContentSources = pluginContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];

                // list of all installed content sources
                ArrayList installedContentSources = new ArrayList();
                installedContentSources.AddRange(_builtInContentSources);
                installedContentSources.AddRange(_pluginContentSources);
                _installedContentSources = installedContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];

                // list of active content-sources
                ArrayList activeContentSources = new ArrayList();
                foreach (ContentSourceInfo contentSourceInfo in _installedContentSources)
                    if (contentSourceInfo.Enabled)
                        activeContentSources.Add(contentSourceInfo);
                _activeContentSources = activeContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];

                // list of built-in insertable content-sources
                ArrayList builtInInsertableContentSources = new ArrayList();
                foreach (ContentSourceInfo contentSourceInfo in _builtInContentSources)
                    if (contentSourceInfo.Enabled && contentSourceInfo.CanCreateNew)
                        builtInInsertableContentSources.Add(contentSourceInfo);
                _builtInInsertableContentSources = builtInInsertableContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];

                // list of plugin insertable content-sources
                ArrayList pluginInsertableContentSources = new ArrayList();
                foreach (ContentSourceInfo contentSourceInfo in _pluginContentSources)
                    if (contentSourceInfo.Enabled && contentSourceInfo.CanCreateNew)
                        pluginInsertableContentSources.Add(contentSourceInfo);
                _pluginInsertableContentSources = pluginInsertableContentSources.ToArray(typeof(ContentSourceInfo)) as ContentSourceInfo[];
            }

            // notify listeners
            OnGlobalContentSourceListChanged();
        }
        private readonly static object _contentSourceListLock = new object();

        private static void Instance_PluginListChanged(object sender, EventArgs e)
        {
            RefreshContentSourceLists(false);
        }

        public static IDynamicCommandMenuContext CreateDynamicCommandMenuContext(DynamicCommandMenuOptions options, CommandManager commandManager, IContentSourceSite sourceSite)
        {
            return new ContentSourceCommandMenuContext(options, commandManager, sourceSite);
        }

        public static ContentSourceInfo[] BuiltInContentSources
        {
            get
            {
                lock (_contentSourceListLock)
                    return _builtInContentSources;
            }
        }

        public static ContentSourceInfo[] PluginContentSources
        {
            get
            {
                lock (_contentSourceListLock)
                    return _pluginContentSources;
            }
        }

        public static ContentSourceInfo[] InstalledContentSources
        {
            get
            {
                lock (_contentSourceListLock)
                    return _installedContentSources;
            }
        }

        public static ContentSourceInfo[] ActiveContentSources
        {
            get
            {
                lock (_contentSourceListLock)
                    return _activeContentSources;
            }
        }

        public static ContentSourceInfo[] BuiltInInsertableContentSources
        {
            get
            {
                lock (_contentSourceListLock)
                    return _builtInInsertableContentSources;
            }
        }

        public static ContentSourceInfo[] PluginInsertableContentSources
        {
            get
            {
                lock (_contentSourceListLock)
                    return _pluginInsertableContentSources;
            }
        }

        public static IEnumerable<ContentSourceInfo> EnabledPublishNotificationPlugins
        {
            get { return GetEnabledPlugins(typeof(PublishNotificationHook)); }
        }

        /// <summary>
        /// Gets the publish notification plugins that are enabled FOR THIS BLOG and
        /// also enabled globally.
        /// If there are any plugins that are not currently explicitly enabled/disabled,
        /// and owner is not null, the user will be prompted whether or not to enable a
        /// plugin. If not explicitly set but owner is null, the plugin will be considered
        /// disabled.
        /// </summary>
        public static IEnumerable<ContentSourceInfo> GetActivePublishNotificationPlugins(IWin32Window owner, string blogId)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(blogId))
                return GetActivePlugins(owner, EnabledPublishNotificationPlugins, blogSettings, DEFAULT_ENABLED);
        }

        public static IEnumerable<ContentSourceInfo> EnabledHeaderFooterPlugins
        {
            get { return GetEnabledPlugins(typeof(HeaderFooterSource)); }
        }

        public static IEnumerable<ContentSourceInfo> GetActiveHeaderFooterPlugins(IWin32Window owner, string blogId)
        {
            using (BlogSettings blogSettings = BlogSettings.ForBlogId(blogId))
                return GetActivePlugins(owner, EnabledHeaderFooterPlugins, blogSettings, DEFAULT_ENABLED);
        }

        private const bool DEFAULT_ENABLED = false;

        public static Comparison<ContentSourceInfo> CreateComparison(BlogPublishingPluginSettings settings)
        {
            return delegate (ContentSourceInfo a, ContentSourceInfo b)
                       {
                           int orderA = settings.GetOrder(a.Id) ?? 100000;
                           int orderB = settings.GetOrder(b.Id) ?? 100000;

                           if (orderA != orderB)
                               return orderA - orderB;
                           else
                               return a.LastUse.CompareTo(b.LastUse);
                       };
        }

        private static IEnumerable<ContentSourceInfo> GetEnabledPlugins(Type type)
        {
            ContentSourceInfo[] csis = ActiveContentSources;
            foreach (ContentSourceInfo csi in csis)
            {
                if (type.IsAssignableFrom(csi.Type))
                {
                    yield return csi;
                }
            }
        }

        private static IEnumerable<ContentSourceInfo> GetActivePlugins(IWin32Window owner, IEnumerable<ContentSourceInfo> plugins, BlogSettings blogSettings, bool defaultEnabled)
        {
            BlogPublishingPluginSettings settings = blogSettings.PublishingPluginSettings;
            List<ContentSourceInfo> pluginList = new List<ContentSourceInfo>(plugins);

            // Sort the plugins according to their determined order, and for those
            // without an order, sort by last use
            pluginList.Sort(CreateComparison(settings));

            // Filter out plugins that aren't enabled for this blog.
            // Do this after sorting, so that if we need to prompt, we
            // will persist the correct order
            pluginList = pluginList.FindAll(delegate (ContentSourceInfo csi) { return PluginIsEnabled(owner, settings, csi, defaultEnabled); });

            return pluginList;
        }

        // TODO: Instead of prompting for each one, show one consolidated dialog
        private static bool PluginIsEnabled(IWin32Window owner, BlogPublishingPluginSettings settings, ContentSourceInfo plugin, bool defaultEnabled)
        {
            bool? alreadyEnabled = settings.IsEnabled(plugin.Id);
            if (alreadyEnabled != null)
                return alreadyEnabled.Value;

            // Got here? then we haven't seen this plugin before

            if (owner == null)
                return defaultEnabled;

            bool enabled = DisplayMessage.Show(MessageId.ShouldUsePlugin, owner, plugin.Name) == DialogResult.Yes;
            settings.Set(plugin.Id, enabled, settings.KnownPluginIds.Length);
            return enabled;
        }

        public static ContentSourceInfo FindContentSource(Type type)
        {
            lock (_contentSourceListLock)
            {
                foreach (ContentSourceInfo contentSourceInfo in ActiveContentSources)
                    if (contentSourceInfo.Type == type)
                        return contentSourceInfo;

                // none found
                return null;
            }
        }

        public static ContentSourceInfo FindContentSource(string contentSourceId)
        {
            lock (_contentSourceListLock)
            {
                foreach (ContentSourceInfo contentSourceInfo in ActiveContentSources)
                    if (contentSourceInfo.Id == contentSourceId)
                        return contentSourceInfo;

                // none found
                return null;
            }
        }

        public static ContentSourceInfo FindContentSourceForUrl(string url)
        {
            lock (_contentSourceListLock)
            {
                foreach (ContentSourceInfo contentSourceInfo in ActiveContentSources)
                {
                    if (contentSourceInfo.CanCreateFromUrl)
                    {
                        if (CanHandleUrl(contentSourceInfo, url))
                            return contentSourceInfo;
                    }
                }

                // didn't find one
                return null;
            }
        }

        public static bool ContentSourceIsPlugin(string contentSourceId)
        {
            lock (_contentSourceListLock)
            {
                // search for a plugin with this id
                foreach (ContentSourceInfo contentSourceInfo in PluginContentSources)
                    if (contentSourceInfo.Id == contentSourceId)
                        return true;

                // no love
                return false;
            }
        }

        public static string MakeContainingElementId(string sourceId, string contentBlockId)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", SMART_CONTENT_ID_PREFIX, sourceId, contentBlockId);
        }

        public static void ParseContainingElementId(string containingElementId, out string sourceId, out string contentBlockId)
        {
            if (string.IsNullOrEmpty(containingElementId))
            {
                throw new ArgumentException("Invalid containing element id.");
            }

            string[] contentIds = containingElementId.Split(':');
            if (contentIds.Length == 3 && contentIds[0] == SMART_CONTENT_ID_PREFIX)
            {
                sourceId = contentIds[1];
                contentBlockId = contentIds[2];
            }
            else if (contentIds.Length == 2)
            {
                // for legacy versions of Writer
                sourceId = contentIds[0];
                contentBlockId = contentIds[1];
            }
            else
            {
                throw new ArgumentException("Invalid containing element id: " + containingElementId);
            }
        }

        public static void PerformInsertion(IContentSourceSite sourceSite, ContentSourceInfo contentSource)
        {
            // record use of content-source (used to list source in MRU order on the sidebar)
            RecordContentSourceUsage(contentSource.Id);

            try
            {
                if (contentSource.Instance is SmartContentSource)
                {
                    SmartContentSource scSource = (SmartContentSource)contentSource.Instance;

                    IExtensionData extensionData = sourceSite.CreateExtensionData(Guid.NewGuid().ToString());

                    // SmartContentSource implementations *must* be stateless (see WinLive 126969), so we wrap up the
                    // internal smart content context and pass it in as a parameter to the CreateContent call.
                    ISmartContent sContent;
                    if (scSource is IInternalSmartContentSource)
                    {
                        sContent = new InternalSmartContent(extensionData, sourceSite as IInternalSmartContentContextSource, contentSource.Id);
                    }
                    else
                    {
                        sContent = new SmartContent(extensionData);
                    }

                    if (scSource.CreateContent(sourceSite.DialogOwner, sContent) == DialogResult.OK)
                    {
                        string content = scSource.GenerateEditorHtml(sContent, sourceSite);
                        if (content != null)
                        {
                            sourceSite.InsertContent(contentSource.Id, content, extensionData);
                            sourceSite.Focus();

                            if (ApplicationPerformance.ContainsEvent(MediaInsertForm.EventName))
                                ApplicationPerformance.EndEvent(MediaInsertForm.EventName);
                        }
                    }
                }
                else if (contentSource.Instance is ContentSource)
                {
                    ContentSource sSource = (ContentSource)contentSource.Instance;
                    string newContent = String.Empty; // default
                    try { if (sourceSite.SelectedHtml != null) newContent = sourceSite.SelectedHtml; }
                    catch { } // safely try to provide selected html
                    if (sSource.CreateContent(sourceSite.DialogOwner, ref newContent) == DialogResult.OK)
                    {
                        sourceSite.InsertContent(newContent, contentSource.Id == WebImageContentSource.ID);
                        sourceSite.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayContentRetreivalError(sourceSite.DialogOwner, ex, contentSource);
            }
        }

        /// <summary>
        /// Returns true if the element className is a structured block.
        /// </summary>
        /// <param name="elementClassName"></param>
        /// <returns></returns>
        public static bool IsSmartContentClass(string elementClassName)
        {
            if (elementClassName == SMART_CONTENT || elementClassName == EDITABLE_SMART_CONTENT)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the element exists in a structured block.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsSmartContent(IHTMLElement element)
        {
            while (element != null)
            {
                if (IsSmartContentClass(element.className))
                {
                    return true;
                }
                element = element.parentElement;
            }
            return false;
        }

        public static IHTMLElement GetContainingSmartContent(IHTMLElement element)
        {
            while (element != null)
            {
                if (IsSmartContentClass(element.className))
                    return element;

                element = element.parentElement;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the element is a structured block.
        /// </summary>
        public static bool IsSmartContentContainer(IHTMLElement element)
        {
            if (element == null)
                return false;

            return IsSmartContentClass(element.className);
        }

        public static IHTMLElement GetContainingSmartContentElement(MarkupRange range)
        {
            IHTMLElement containingSmartContent = range.ParentElement(IsSmartContentContainer);
            if (containingSmartContent != null)
            {
                return containingSmartContent;
            }
            else
            {
                IHTMLElement[] elements = range.GetTopLevelElements(MarkupRange.FilterNone);
                if (elements.Length == 1 && IsSmartContent(elements[0]))
                {
                    return elements[0];
                }
            }
            return null;
        }

        public static string[] GetSmartContentIds(MarkupRange range)
        {
            ArrayList ids = new ArrayList();
            foreach (IHTMLElement el in range.GetElements(new IHTMLElementFilter(IsSmartContentContainer), false))
            {
                if (el.id != null)
                    ids.Add(el.id);
            }
            return (string[])ids.ToArray(typeof(string));
        }

        public static bool ContainsSmartContentFromSource(string contentSourceId, MarkupRange range)
        {
            string[] contentIds = GetSmartContentIds(range);
            foreach (string contentId in contentIds)
            {
                string sourceId;
                string contentBlockId;
                ParseContainingElementId(contentId, out sourceId, out contentBlockId);
                if (sourceId == contentSourceId)
                    return true;
            }

            // if we got this far then there are no tags
            return false;
        }

        public class SmartContentPredicate : IElementPredicate
        {
            public bool IsMatch(Element e)
            {
                BeginTag bt = e as BeginTag;
                if (bt == null)
                    return false;
                return IsSmartContentClass(bt.GetAttributeValue("class"));
            }
        }

        public static void DisplayContentRetreivalError(IWin32Window dialogOwner, Exception ex, ContentSourceInfo info)
        {
            if (ex is ContentCreationException)
            {
                ContentCreationException ccEx = ex as ContentCreationException;
                DisplayableExceptionDisplayForm.Show(dialogOwner, new DisplayableException(ccEx.Title, ccEx.Description));
            }
            else if (ex is NotImplementedException)
            {
                DisplayableExceptionDisplayForm.Show(dialogOwner, new DisplayableException(
                    Res.Get(StringId.MethodNotImplemented), String.Format(CultureInfo.InvariantCulture, Res.Get(StringId.MethodNotImplementedDetail), info.Name, info.WriterPluginPublisherUrl, ex.Message)));
            }
            else
            {
                DisplayableExceptionDisplayForm.Show(dialogOwner, ex);
            }
        }

        //note that these strings are also used within the BlogPostRegionLocatorStrategy class. Changes
        // here should be made there as well (used for detecting smart content when updating weblog style)
        public const string SMART_CONTENT = "wlWriterSmartContent";
        public const string EDITABLE_SMART_CONTENT = "wlWriterEditableSmartContent";
        public const string HEADERS_FOOTERS = "wlWriterHeaderFooter";
        public const string SMART_CONTENT_ID_PREFIX = "scid";
        public const string SMART_CONTENT_CONTAINER = "wlwScContainer";

        public class SmartContentElementFilter
        {
            public bool Filter(IHTMLElement e)
            {
                return (e is IHTMLDivElement) &&
                       ((e.className == ContentSourceManager.SMART_CONTENT) ||
                       (e.className == ContentSourceManager.EDITABLE_SMART_CONTENT));
            }
        }

        public static IHTMLElementFilter CreateSmartContentElementFilter()
        {
            return new IHTMLElementFilter(new SmartContentElementFilter().Filter);
        }

        public static void RemoveSmartContentAttributes(BeginTag beginTag)
        {
            if (beginTag == null)
                throw new ArgumentNullException("beginTag");

            Attr classAttr = beginTag.GetAttribute("class");

            // Remove the SmartContent classes.
            if (classAttr != null)
            {
                classAttr.Value = classAttr.Value.Replace(EDITABLE_SMART_CONTENT, string.Empty);
                classAttr.Value = classAttr.Value.Replace(SMART_CONTENT, string.Empty);
            }

            // Remove contentEditable=true so that the user can edit it manually.
            beginTag.RemoveAttribute("contentEditable");
        }

        private static void AddContentSource(ArrayList contentSourceList, Type contentSourceType, bool showErrors)
        {
            try
            {
                contentSourceList.Add(new ContentSourceInfo(contentSourceType, showErrors));
            }
            catch (WriterContentPluginAttributeMissingException)
            {
                // this exception is OK (allows for ContentSource base-class which doesn't implement an actual plugin)
            }
            catch (ArgumentException ex)
            {
                if (showErrors)
                    DisplayMessage.Show(MessageId.PluginInvalidAttribute, contentSourceType.Name, ex.ParamName, ex.Message);
            }
            catch (Exception ex)
            {
                if (showErrors)
                    DisplayMessage.Show(MessageId.PluginUnexpectedLoadError, contentSourceType.Name, ex.Message);
            }
        }

        private static bool CanHandleUrl(ContentSourceInfo contentSource, string url)
        {
            try
            {
                if (typeof(IHandlesMultipleUrls).IsAssignableFrom(contentSource.Type))
                {
                    IHandlesMultipleUrls plugin = (IHandlesMultipleUrls)Activator.CreateInstance(contentSource.Type);
                    return plugin.HasUrlMatch(url);
                }
                Regex regex = new Regex(contentSource.UrlContentSourceUrlPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                return regex.IsMatch(url);
            }
            catch
            {
                return false;
            }
        }

        private static bool? _pluginsEnabledOverride;
        private static bool PluginsEnabled
        {
            get
            {
                if (_pluginsEnabledOverride != null)
                    return _pluginsEnabledOverride.Value;

                string commandLine = Environment.CommandLine;
                if (commandLine != null)
                    return commandLine.ToLower(CultureInfo.InvariantCulture).IndexOf("/noplugins") == -1;

                return true;
            }
        }

        private static void RecordContentSourceUsage(string sourceId)
        {
            foreach (ContentSourceInfo contentSourceId in ActiveContentSources)
            {
                if (contentSourceId.Id == sourceId)
                {
                    contentSourceId.LastUse = DateTime.Now;
                    return;
                }
            }
        }

        private static ContentSourceInfo[] _builtInContentSources;
        private static ContentSourceInfo[] _pluginContentSources;
        private static ContentSourceInfo[] _installedContentSources;
        private static ContentSourceInfo[] _activeContentSources;
        private static ContentSourceInfo[] _builtInInsertableContentSources;
        private static ContentSourceInfo[] _pluginInsertableContentSources;

        internal static ContentSourceInfo GetContentSourceInfoById(string Id)
        {
            foreach (ContentSourceInfo contentSourceInfo in _installedContentSources)
            {
                if (contentSourceInfo.Id == Id)
                {
                    return contentSourceInfo;
                }
            }
            return null;
        }
    }

    internal class ContentSourceCommand : Command, IMenuCommandObject
    {
        public ContentSourceCommand(IContentSourceSite sourceSite, ContentSourceInfo contentSourceInfo, bool isBuiltInPlugin)
        {
            // copy references
            _insertionSite = sourceSite;
            _contentSourceInfo = contentSourceInfo;

            // tie this command to the content-source for execution
            // (we don't initialize other properties b/c this Command
            // is only use for decoupled lookup & execution not for
            // UI display. If we actually want to display this command
            // on a command bar, etc. we should fill in the other properties.
            this.Identifier = contentSourceInfo.Id;

            // For built in plugins, we will get these values from the ribbon
            if (contentSourceInfo.CanCreateNew && !isBuiltInPlugin)
            {
                this.MenuText = ((IMenuCommandObject)this).Caption;
                this.CommandBarButtonBitmapEnabled = contentSourceInfo.Image;
            }
        }

        Bitmap IMenuCommandObject.Image { get { return _contentSourceInfo.Image; } }

        string IMenuCommandObject.Caption
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WithEllipses), _contentSourceInfo.InsertableContentSourceMenuText);
            }
        }

        string IMenuCommandObject.CaptionNoMnemonic
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WithEllipses), _contentSourceInfo.InsertableContentSourceSidebarText);
            }
        }

        bool IMenuCommandObject.Enabled { get { return _insertionSite.InsertCommandsEnabled; } }
        bool IMenuCommandObject.Latched { get { return false; } }

        protected override void OnExecute(EventArgs e)
        {
            base.OnExecute(e);

            if (_insertionSite.InsertCommandsEnabled)
            {
                ContentSourceManager.PerformInsertion(_insertionSite, _contentSourceInfo);
            }
            else
            {
                DisplayMessage.Show(MessageId.CannotInsert, _insertionSite.DialogOwner, _contentSourceInfo.InsertableContentSourceSidebarText);
            }
        }

        private IContentSourceSite _insertionSite;
        private ContentSourceInfo _contentSourceInfo;
    }

    internal class ContentSourceCommandMenuContext : IDynamicCommandMenuContext
    {
        public ContentSourceCommandMenuContext(DynamicCommandMenuOptions options, CommandManager commandManager, IContentSourceSite sourceSite)
        {
            _options = options;
            _commandManager = commandManager;
            _insertionSite = sourceSite;
        }

        public DynamicCommandMenuOptions Options
        {
            get { return _options; }
        }
        private DynamicCommandMenuOptions _options;

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }
        private CommandManager _commandManager;

        public IMenuCommandObject[] GetMenuCommandObjects()
        {
            ArrayList menuCommandObjects = new ArrayList();
            // list built-in sources first
            foreach (ContentSourceInfo _contentSourceInfo in ContentSourceManager.BuiltInInsertableContentSources)
            {
                if (_contentSourceInfo.Id == WebImageContentSource.ID)
                    continue;
                Command command = CommandManager.Get(_contentSourceInfo.Id);
                if (command != null)
                    menuCommandObjects.Add(command);
            }

            // then plugin sources
            foreach (ContentSourceInfo _contentSourceInfo in ContentSourceManager.PluginInsertableContentSources)
            {
                Command command = CommandManager.Get(_contentSourceInfo.Id);
                if (command != null)
                    menuCommandObjects.Add(command);
            }
            return menuCommandObjects.ToArray(typeof(IMenuCommandObject)) as IMenuCommandObject[];
        }

        public void CommandExecuted(IMenuCommandObject menuCommandObject)
        {
            (menuCommandObject as ContentSourceCommand).PerformExecute();
        }

        private IContentSourceSite _insertionSite;
    }

    internal class PluginAttributeException : ApplicationException
    {
    }

    internal class PluginAttributeFieldMissingException : PluginAttributeException
    {
        public PluginAttributeFieldMissingException(Type pluginType, Type attributeType, string attributeFieldName)
        {
            _pluginType = pluginType;
            _attributeType = attributeType;
            _attributeFieldName = attributeFieldName;
        }

        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Plugin {0} is missing the \"{1}\" field of the {2}.", _pluginType.Name, _attributeFieldName, _attributeType.Name);
            }
        }

        private Type _pluginType;
        private Type _attributeType;
        private string _attributeFieldName;
    }

    internal abstract class PluginAttributeImageResourceException : PluginAttributeException
    {
        public PluginAttributeImageResourceException(Type pluginType, string imageResourcePath)
        {
            _pluginType = pluginType;
            _imageResourcePath = imageResourcePath;
        }

        protected Type _pluginType;
        protected string _imageResourcePath;
    }

    internal class PluginAttributeImageResourceMissingException : PluginAttributeImageResourceException
    {
        public PluginAttributeImageResourceMissingException(Type pluginType, string imageResourcePath)
            : base(pluginType, imageResourcePath)
        {
        }

        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Unable to load image resource {0} for Plugin {1}.", _imageResourcePath, _pluginType.Name);
            }
        }
    }

    internal class PluginAttributeImageResourceWrongSizeException : PluginAttributeImageResourceException
    {
        public PluginAttributeImageResourceWrongSizeException(Type pluginType, string imageResourcePath)
            : base(pluginType, imageResourcePath)
        {
        }

        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Image resource {0} for Plugin {1} is the wrong size (Plugin images must be {2}x{3}).", _imageResourcePath, _pluginType.Name, ContentSourceInfo.IMAGE_WIDTH, ContentSourceInfo.IMAGE_HEIGHT);
            }
        }
    }

    internal class PluginAttributeInvalidRegexException : PluginAttributeException
    {
        public PluginAttributeInvalidRegexException(Type pluginType, string regex)
        {
            _pluginType = pluginType;
            _regex = regex;
        }

        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "Invalid regular expression for Plugin {0} ({1}).", _pluginType.Name, _regex);
            }
        }

        private Type _pluginType;
        private string _regex;

    }

    internal class WriterContentPluginAttributeMissingException : PluginAttributeException
    {
        public WriterContentPluginAttributeMissingException(Type pluginType)
        {
            _pluginType = pluginType;
        }

        public override string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "The Plugin {0} does not have the required attributes. Content source plugins must include the WriterPlugin attribute as well as one or more of the InsertableContentSource, UrlContentSource, or LiveClipbaordContentSource attributes.", _pluginType.Name);
            }
        }

        private Type _pluginType;
    }

}
