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
        private const string CONFIG_PAGES_PATH = "SSGPagesPath";
        private const string CONFIG_BUILD_COMMAND = "SSGBuildCommand";
        private const string CONFIG_PUBLISH_COMMAND = "SSGPublishCommand";
        private const string CONFIG_INITIALISED = "SSGInitialised";

        /// <summary>
        /// The full path to the local static site 'project' directory
        /// </summary>
        public string LocalSitePath { get; set; } = "";

        /// <summary>
        /// Path to Posts directory, relative to LocalSitePath
        /// </summary>
        public string PostsPath { get; set; } = "";

        /// <summary>
        /// Path to Pages directory, relative to LocalSitePath
        /// </summary>
        public string PagesPath { get; set; } = "";

        /// <summary>
        /// Build command, executed by system command interpreter with LocalSitePath working directory
        /// </summary>
        public string BuildCommand { get; set; } = "";

        /// <summary>
        /// Publish command, executed by system command interpreter with LocalSitePath working directory
        /// </summary>
        public string PublishCommand { get; set; } = "";

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
            LocalSitePath  = creds.Username;
            PostsPath      = creds.GetCustomValue(CONFIG_POSTS_PATH);
            PagesPath      = creds.GetCustomValue(CONFIG_PAGES_PATH);
            BuildCommand   = creds.GetCustomValue(CONFIG_BUILD_COMMAND);
            PublishCommand = creds.GetCustomValue(CONFIG_PUBLISH_COMMAND);
            Initialised    = creds.GetCustomValue(CONFIG_INITIALISED) == "1";
        }

        /// <summary>
        /// Saves site configuration to blog credentials
        /// </summary>
        public void SaveToCredentials(IBlogCredentialsAccessor creds)
        {
            // Set username to Local Site Path
            creds.Username = LocalSitePath;
            creds.SetCustomValue(CONFIG_POSTS_PATH,      PostsPath);
            creds.SetCustomValue(CONFIG_PAGES_PATH,      PagesPath);
            creds.SetCustomValue(CONFIG_BUILD_COMMAND,   BuildCommand);
            creds.SetCustomValue(CONFIG_PUBLISH_COMMAND, PublishCommand);
            creds.SetCustomValue(CONFIG_INITIALISED,     Initialised ? "1" : "0");
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
    }
}
