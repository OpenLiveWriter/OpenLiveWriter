using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLiveWriter.BlogClient.Clients
{
    public class StaticSiteConfig
    {
        // The credential keys where the configuration is stored.
        private const string CONFIG_POSTS_PATH = "SSGPostsPath";
        private const string CONFIG_PAGES_ENABLED = "SSGPagesEnabled";
        private const string CONFIG_PAGES_PATH = "SSGPagesPath";
        private const string CONFIG_DRAFTS_ENABLED = "SSGDraftsEnabled";
        private const string CONFIG_DRAFTS_PATH = "SSGDraftsPath";
        private const string CONFIG_IMAGES_ENABLED = "SSGImagesEnabled";
        private const string CONFIG_IMAGES_PATH = "SSGImagesPath";
        private const string CONFIG_BUILD_COMMAND = "SSGBuildCommand";
        private const string CONFIG_BUILDING_ENABLED = "SSGBuildingEnabled";
        private const string CONFIG_PUBLISH_COMMAND = "SSGPublishCommand";
        private const string CONFIG_POST_URL_FORMAT = "SSGPostUrlFormat";
        private const string CONFIG_INITIALISED = "SSGInitialised";

        // Public Site Url is stored in the blog's BlogConfig. Loading is handled in this class, but saving is handled from the WizardController.
        // This is done to avoid referencing PostEditor from this project.

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
        /// Post URL format, appended to end of Site URL, automatically opened on publish completion.
        /// Supports %y for four-digit year, %m and %d for two-digit months and days, %f for post filename.
        /// Default is Jekyll format: "%y/%m/%d/%f"
        /// </summary>
        public string PostUrlFormat { get; set; } = "%y/%m/%d/%f";

        /// <summary>
        /// Used to determine if parameter detection has occurred, default false.
        /// </summary>
        public bool Initialised { get; set; } = false;

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
            BuildCommand = creds.GetCustomValue(CONFIG_BUILD_COMMAND);

            PublishCommand = creds.GetCustomValue(CONFIG_PUBLISH_COMMAND);
            PostUrlFormat = creds.GetCustomValue(CONFIG_POST_URL_FORMAT);
            Initialised = creds.GetCustomValue(CONFIG_INITIALISED) == "1";
        }

        /// <summary>
        /// Loads site configuration from blog settings
        /// </summary>
        /// <param name="blogCredentials">An IBlogSettingsAccessor</param>
        public void LoadFromBlogSettings(IBlogSettingsAccessor blogSettings)
        {
            SiteUrl = blogSettings.HomepageUrl;
            LoadFromCredentials(blogSettings.Credentials);
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
            creds.SetCustomValue(CONFIG_BUILD_COMMAND, BuildCommand);

            creds.SetCustomValue(CONFIG_PUBLISH_COMMAND, PublishCommand);
            creds.SetCustomValue(CONFIG_POST_URL_FORMAT, PostUrlFormat);
            creds.SetCustomValue(CONFIG_INITIALISED, Initialised ? "1" : "0");
        }

        /// <summary>
        /// Attempt detection of parameters based on LocalSitePath
        /// </summary>
        /// <returns>True if detection successful</returns>
        public bool AttemptConfigDetection()
        {
            // TODO Implement
            Initialised = true;
            return true;
        }

        public void SaveToCredentials(IBlogCredentials blogCredentials)
            => SaveToCredentials(new BlogCredentialsAccessor("", blogCredentials));

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
}
