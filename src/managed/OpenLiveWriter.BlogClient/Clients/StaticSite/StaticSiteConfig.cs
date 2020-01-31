using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSiteConfig
    {
        // The credential keys where the configuration is stored.
        private const string CONFIG_POSTS_PATH = "PostsPath";
        private const string CONFIG_PAGES_ENABLED = "PagesEnabled";
        private const string CONFIG_PAGES_PATH = "PagesPath";
        private const string CONFIG_DRAFTS_ENABLED = "DraftsEnabled";
        private const string CONFIG_DRAFTS_PATH = "DraftsPath";
        private const string CONFIG_IMAGES_ENABLED = "ImagesEnabled";
        private const string CONFIG_IMAGES_PATH = "ImagesPath";
        private const string CONFIG_BUILDING_ENABLED = "BuildingEnabled";
        private const string CONFIG_OUTPUT_PATH = "OutputPath";
        private const string CONFIG_BUILD_COMMAND = "BuildCommand";
        private const string CONFIG_PUBLISH_COMMAND = "PublishCommand";
        private const string CONFIG_SITE_URL = "SiteUrl"; // Store Site Url in credentials as well, for acccess by StaticSiteClient
        private const string CONFIG_SHOW_CMD_WINDOWS = "ShowCmdWindows";
        private const string CONFIG_CMD_TIMEOUT_MS = "CmdTimeoutMs";
        private const string CONFIG_INITIALISED = "Initialised";

        public static int DEFAULT_CMD_TIMEOUT = 60000;

        // Public Site Url is stored in the blog's BlogConfig. Loading is handled in this class, but saving is handled from the WizardController.
        // This is done to avoid referencing PostEditor from this project.

        // NOTE: When setting default config values below, also make sure to alter LoadFromCredentials to not overwrite defaults if a key was not found.

        /// <summary>
        /// The full path to the local static site 'project' directory
        /// </summary>
        public string LocalSitePath { get; set; } = "";

        /// <summary>
        /// Path to Posts directory, relative to LocalSitePath
        /// </summary>
        public string PostsPath { get; set; } = "";

        /// <summary>
        /// True if Pages can be posted to this blog.
        /// </summary>
        public bool PagesEnabled { get; set; } = false;

        /// <summary>
        /// Path to Pages directory, relative to LocalSitePath.
        /// </summary>
        public string PagesPath { get; set; } = "";

        /// <summary>
        /// True if Drafts can be saved to this blog.
        /// </summary>
        public bool DraftsEnabled { get; set; } = false;

        /// <summary>
        /// Path to Drafts directory, relative to LocalSitePath.
        /// </summary>
        public string DraftsPath { get; set; } = "";

        /// <summary>
        /// True if Images can be uploaded to this blog.
        /// </summary>
        public bool ImagesEnabled { get; set; } = false;

        /// <summary>
        /// Path to Images directory, relative to LocalSitePath.
        /// </summary>
        public string ImagesPath { get; set; } = "";

        /// <summary>
        /// True if site is locally built.
        /// </summary>
        public bool BuildingEnabled { get; set; } = false;

        /// <summary>
        /// Path to Output directory, relative to LocalSitePath. Can be possibly used in future for preset publishing routines.
        /// </summary>
        public string OutputPath { get; set; } = "";

        /// <summary>
        /// Build command, executed by system command interpreter with LocalSitePath working directory
        /// </summary>
        public string BuildCommand { get; set; } = "";

        /// <summary>
        /// Publish command, executed by system command interpreter with LocalSitePath working directory
        /// </summary>
        public string PublishCommand { get; set; } = "";

        /// <summary>
        /// Public site URL
        /// </summary>
        public string SiteUrl { get; set; } = "";

        /// <summary>
        /// Site title
        /// </summary>
        public string SiteTitle { get; set; } = "";

        /// <summary>
        /// Show CMD windows. Useful for debugging. Default is false.
        /// </summary>
        public bool ShowCmdWindows { get; set; } = false;

        /// <summary>
        /// Timeout for commands. Default is 60k MS (60 seconds).
        /// </summary>
        public int CmdTimeoutMs { get; set; } = DEFAULT_CMD_TIMEOUT;

        /// <summary>
        /// Used to determine if parameter detection has occurred, default false.
        /// </summary>
        public bool Initialised { get; set; } = false;

        public StaticSiteConfigFrontMatterKeys FrontMatterKeys { get; set; } = new StaticSiteConfigFrontMatterKeys();

        public StaticSiteConfigValidator Validator => new StaticSiteConfigValidator(this);

        /// <summary>
        /// Load site configuration from blog credentials
        /// </summary>
        /// <param name="creds">An IBlogCredentialsAccessor</param>
        public void LoadFromCredentials(IBlogCredentialsAccessor creds)
        {
            LocalSitePath = creds.Username;
            PostsPath = creds.GetCustomValue(CONFIG_POSTS_PATH);

            PagesEnabled = creds.GetCustomValue(CONFIG_PAGES_ENABLED) == "1";
            PagesPath = creds.GetCustomValue(CONFIG_PAGES_PATH);

            DraftsEnabled = creds.GetCustomValue(CONFIG_DRAFTS_ENABLED) == "1";
            DraftsPath = creds.GetCustomValue(CONFIG_DRAFTS_PATH);

            ImagesEnabled = creds.GetCustomValue(CONFIG_IMAGES_ENABLED) == "1";
            ImagesPath = creds.GetCustomValue(CONFIG_IMAGES_PATH);

            BuildingEnabled = creds.GetCustomValue(CONFIG_BUILDING_ENABLED) == "1";
            OutputPath = creds.GetCustomValue(CONFIG_OUTPUT_PATH);
            BuildCommand = creds.GetCustomValue(CONFIG_BUILD_COMMAND);

            PublishCommand = creds.GetCustomValue(CONFIG_PUBLISH_COMMAND);

            SiteUrl = creds.GetCustomValue(CONFIG_SITE_URL); // This will be overidden in LoadFromBlogSettings, HomepageUrl is considered a more accurate source of truth

            ShowCmdWindows = creds.GetCustomValue(CONFIG_SHOW_CMD_WINDOWS) == "1";
            if (creds.GetCustomValue(CONFIG_CMD_TIMEOUT_MS) != string.Empty) CmdTimeoutMs = int.Parse(creds.GetCustomValue(CONFIG_CMD_TIMEOUT_MS));
            Initialised = creds.GetCustomValue(CONFIG_INITIALISED) == "1";

            // Load FrontMatterKeys
            FrontMatterKeys = StaticSiteConfigFrontMatterKeys.LoadKeysFromCredentials(creds);
        }

        /// <summary>
        /// Loads site configuration from blog settings
        /// </summary>
        /// <param name="blogCredentials">An IBlogSettingsAccessor</param>
        public void LoadFromBlogSettings(IBlogSettingsAccessor blogSettings)
        {
            LoadFromCredentials(blogSettings.Credentials);

            SiteUrl = blogSettings.HomepageUrl;
            SiteTitle = blogSettings.BlogName;
        }

        /// <summary>
        /// Saves site configuration to blog credentials
        /// </summary>
        public void SaveToCredentials(IBlogCredentialsAccessor creds)
        {
            // Set username to Local Site Path
            creds.Username = LocalSitePath;
            creds.SetCustomValue(CONFIG_POSTS_PATH, PostsPath);

            creds.SetCustomValue(CONFIG_PAGES_ENABLED, PagesEnabled ? "1" : "0");
            creds.SetCustomValue(CONFIG_PAGES_PATH, PagesPath);

            creds.SetCustomValue(CONFIG_DRAFTS_ENABLED, DraftsEnabled ? "1" : "0");
            creds.SetCustomValue(CONFIG_DRAFTS_PATH, DraftsPath);

            creds.SetCustomValue(CONFIG_IMAGES_ENABLED, ImagesEnabled ? "1" : "0");
            creds.SetCustomValue(CONFIG_IMAGES_PATH, ImagesPath);

            creds.SetCustomValue(CONFIG_BUILDING_ENABLED, BuildingEnabled ? "1" : "0");
            creds.SetCustomValue(CONFIG_OUTPUT_PATH, OutputPath);
            creds.SetCustomValue(CONFIG_BUILD_COMMAND, BuildCommand);

            creds.SetCustomValue(CONFIG_PUBLISH_COMMAND, PublishCommand);
            creds.SetCustomValue(CONFIG_SITE_URL, SiteUrl);

            creds.SetCustomValue(CONFIG_SHOW_CMD_WINDOWS, ShowCmdWindows ? "1" : "0");
            creds.SetCustomValue(CONFIG_CMD_TIMEOUT_MS, CmdTimeoutMs.ToString());
            creds.SetCustomValue(CONFIG_INITIALISED, Initialised ? "1" : "0");

            // Save FrontMatterKeys
            FrontMatterKeys.SaveToCredentials(creds);
        }

        public void SaveToCredentials(IBlogCredentials blogCredentials)
            => SaveToCredentials(new BlogCredentialsAccessor("", blogCredentials));

        public StaticSiteConfig Clone()
            => new StaticSiteConfig()
            {
                LocalSitePath = LocalSitePath,
                PostsPath = PostsPath,
                PagesEnabled = PagesEnabled,
                PagesPath = PagesPath,
                DraftsEnabled = DraftsEnabled,
                DraftsPath = DraftsPath,
                ImagesEnabled = ImagesEnabled,
                ImagesPath = ImagesPath,
                BuildingEnabled = BuildingEnabled,
                OutputPath = OutputPath,
                BuildCommand = BuildCommand,
                PublishCommand = PublishCommand,
                SiteUrl = SiteUrl,
                SiteTitle = SiteTitle,
                ShowCmdWindows = ShowCmdWindows,
                CmdTimeoutMs = CmdTimeoutMs,
                Initialised = Initialised,
                
                FrontMatterKeys = FrontMatterKeys.Clone()
            };

        /// <summary>
        /// Create a new StaticSiteConfig instance and load site configuration from blog credentials
        /// </summary>
        /// <param name="blogCredentials">An IBlogCredentialsAccessor</param>
        public static StaticSiteConfig LoadConfigFromCredentials(IBlogCredentialsAccessor blogCredentials)
        {
            var config = new StaticSiteConfig();
            config.LoadFromCredentials(blogCredentials);
            return config;
        }

        public static StaticSiteConfig LoadConfigFromCredentials(IBlogCredentials blogCredentials)
            => LoadConfigFromCredentials(new BlogCredentialsAccessor("", blogCredentials));

        /// <summary>
        /// Create a new StaticSiteConfig instance and loads site configuration from blog settings
        /// </summary>
        /// <param name="blogCredentials">An IBlogSettingsAccessor</param>
        public static StaticSiteConfig LoadConfigFromBlogSettings(IBlogSettingsAccessor blogSettings)
        {
            var config = new StaticSiteConfig();
            config.LoadFromBlogSettings(blogSettings);
            return config;
        }
    }

    /// <summary>
    /// Represents the YAML keys used for each of these properties in the front matter
    /// </summary>
    public class StaticSiteConfigFrontMatterKeys
    {
        private const string CONFIG_ID_KEY = "FrontMatterKey.Id";
        private const string CONFIG_TITLE_KEY = "FrontMatterKey.Title";
        private const string CONFIG_DATE_KEY = "FrontMatterKey.Date";
        private const string CONFIG_LAYOUT_KEY = "FrontMatterKey.Layout";
        private const string CONFIG_TAGS_KEY = "FrontMatterKey.Tags";
        private const string CONFIG_PARENT_ID_KEY = "FrontMatterKey.ParentId";
        private const string CONFIG_PERMALINK_KEY = "FrontMatterKey.Permalink";

        public enum KeyIdentifier
        {
            Id,
            Title,
            Date,
            Layout,
            Tags,
            ParentId,
            Permalink
        }

        public string IdKey { get; set; } = "id";
        public string TitleKey { get; set; } = "title";
        public string DateKey { get; set; } = "date";
        public string LayoutKey { get; set; } = "layout";
        public string TagsKey { get; set; } = "tags";
        public string ParentIdKey { get; set; } = "parent_id";
        public string PermalinkKey { get; set; } = "permalink";

        public StaticSiteConfigFrontMatterKeys Clone()
            => new StaticSiteConfigFrontMatterKeys()
            {
                IdKey = IdKey,
                TitleKey = TitleKey,
                DateKey = DateKey,
                LayoutKey = LayoutKey,
                TagsKey = TagsKey,
                ParentIdKey = ParentIdKey,
                PermalinkKey = PermalinkKey
            };

        /// <summary>
        /// Load front matter keys configuration from blog credentials
        /// </summary>
        /// <param name="creds">An IBlogCredentialsAccessor</param>
        public void LoadFromCredentials(IBlogCredentialsAccessor creds)
        {
            if (creds.GetCustomValue(CONFIG_ID_KEY) != string.Empty) IdKey = creds.GetCustomValue(CONFIG_ID_KEY);
            if (creds.GetCustomValue(CONFIG_TITLE_KEY) != string.Empty) TitleKey = creds.GetCustomValue(CONFIG_TITLE_KEY);
            if (creds.GetCustomValue(CONFIG_DATE_KEY) != string.Empty) DateKey = creds.GetCustomValue(CONFIG_DATE_KEY);
            if (creds.GetCustomValue(CONFIG_LAYOUT_KEY) != string.Empty) LayoutKey = creds.GetCustomValue(CONFIG_LAYOUT_KEY);
            if (creds.GetCustomValue(CONFIG_TAGS_KEY) != string.Empty) TagsKey = creds.GetCustomValue(CONFIG_TAGS_KEY);
            if (creds.GetCustomValue(CONFIG_PARENT_ID_KEY) != string.Empty) ParentIdKey = creds.GetCustomValue(CONFIG_PARENT_ID_KEY);
            if (creds.GetCustomValue(CONFIG_PERMALINK_KEY) != string.Empty) PermalinkKey = creds.GetCustomValue(CONFIG_PERMALINK_KEY);
        }

        /// <summary>
        /// Save front matter keys configuration to blog credentials
        /// </summary>
        /// <param name="creds">An IBlogCredentialsAccessor</param>
        public void SaveToCredentials(IBlogCredentialsAccessor creds)
        {
            creds.SetCustomValue(CONFIG_ID_KEY, IdKey);
            creds.SetCustomValue(CONFIG_TITLE_KEY, TitleKey);
            creds.SetCustomValue(CONFIG_DATE_KEY, DateKey);
            creds.SetCustomValue(CONFIG_LAYOUT_KEY, LayoutKey);
            creds.SetCustomValue(CONFIG_TAGS_KEY, TagsKey);
            creds.SetCustomValue(CONFIG_PARENT_ID_KEY, ParentIdKey);
            creds.SetCustomValue(CONFIG_PERMALINK_KEY, PermalinkKey);
        }

        /// <summary>
        /// Create a new StaticSiteConfigFrontMatterKeys instance and load configuration from blog credentials
        /// </summary>
        /// <param name="blogCredentials">An IBlogCredentialsAccessor</param>
        public static StaticSiteConfigFrontMatterKeys LoadKeysFromCredentials(IBlogCredentialsAccessor blogCredentials)
        {
            var frontMatterKeys = new StaticSiteConfigFrontMatterKeys();
            frontMatterKeys.LoadFromCredentials(blogCredentials);
            return frontMatterKeys;
        }
    }
}