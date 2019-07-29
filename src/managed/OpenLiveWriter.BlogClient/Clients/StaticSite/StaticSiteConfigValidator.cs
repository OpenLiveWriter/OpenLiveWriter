using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    public class StaticSiteConfigValidator
    {
        private StaticSiteConfig _config;

        public StaticSiteConfigValidator(StaticSiteConfig config)
        {
            _config = config;
        }

        public StaticSiteConfigValidator ValidateAll()
            => this
            .ValidateLocalSitePath()
            .ValidatePostsPath()
            .ValidatePagesPath()
            .ValidateDraftsPath()
            .ValidateImagesPath()
            .ValidateOutputPath()
            .ValidatePostUrlFormat()
            .ValidateBuildCommand()
            .ValidatePublishCommand();

        // TODO replace errors with strings from resources

        #region Path Validation

        public StaticSiteConfigValidator ValidateLocalSitePath()
        {
            if(!Directory.Exists(_config.LocalSitePath))
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Local Site Path '{0}' does not exist.",
                    _config.LocalSitePath);

            return this;
        }

        public StaticSiteConfigValidator ValidatePostsPath()
        {
            var postsPathFull = $"{_config.LocalSitePath}\\{_config.PostsPath}";

            // If the Posts path is empty, display an error
            if (_config.PostsPath.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Posts path is empty.");

            // If the Posts path doesn't exist, display an error
            if (!Directory.Exists(postsPathFull))
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Posts path '{0}' does not exist.",
                    postsPathFull);

            return this;
        }

        public StaticSiteConfigValidator ValidatePagesPath()
        {
            if (!_config.PagesEnabled) return this; // Don't validate if pages aren't enabled

            var pagesPathFull = $"{_config.LocalSitePath}\\{_config.PagesPath}";

            // If the Pages path is empty, display an error
            if (_config.PagesPath.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Pages path is empty.");

            // If the path doesn't exist, display an error
            if (!Directory.Exists(pagesPathFull))
                throw new StaticSiteConfigValidationException(
                        "Folder not found",
                        "Pages path '{0}' does not exist.",
                        pagesPathFull);

            return this;
        }

        public StaticSiteConfigValidator ValidateDraftsPath()
        {
            if (!_config.DraftsEnabled) return this; // Don't validate if drafts aren't enabled

            var draftsPathFull = $"{_config.LocalSitePath}\\{_config.DraftsPath}";

            // If the Drafts path is empty, display an error
            if (_config.DraftsPath.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Drafts path is empty.");

            // If the path doesn't exist, display an error
            if (!Directory.Exists(draftsPathFull))
                throw new StaticSiteConfigValidationException(
                        "Folder not found",
                        "Drafts path '{0}' does not exist.",
                        draftsPathFull);

            return this;
        }

        public StaticSiteConfigValidator ValidateImagesPath()
        {
            if (!_config.ImagesEnabled) return this; // Don't validate if images aren't enabled

            var imagesPathFull = $"{_config.LocalSitePath}\\{_config.ImagesPath}";

            // If the Images path is empty, display an error
            if (_config.ImagesPath.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Images path is empty.");

            // If the path doesn't exist, display an error
            if (!Directory.Exists(imagesPathFull))
                throw new StaticSiteConfigValidationException(
                        "Folder not found",
                        "Images path '{0}' does not exist.",
                        imagesPathFull);

            return this;
        }

        public StaticSiteConfigValidator ValidateOutputPath()
        {
            if (!_config.BuildingEnabled) return this; // Don't validate if building isn't enabled

            var outputPathFull = $"{_config.LocalSitePath}\\{_config.OutputPath}";

            // If the Output path is empty, display an error
            if (_config.OutputPath.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                    "Folder not found",
                    "Output path is empty.");

            // If the path doesn't exist, display an error
            if (!Directory.Exists(outputPathFull))
                throw new StaticSiteConfigValidationException(
                        "Folder not found",
                        "Output path '{0}' does not exist.",
                        outputPathFull);

            return this;
        }

        #endregion

        public StaticSiteConfigValidator ValidatePostUrlFormat()
        {
            if (!_config.PostUrlFormat.Contains("%f"))
                throw new StaticSiteConfigValidationException(
                            "Invalid Post URL format",
                            "Post URL format does not contain filename variable (%f)");

            return this;
        }

        public StaticSiteConfigValidator ValidateBuildCommand()
        {
            if (!_config.BuildingEnabled) return this; // Don't validate if building isn't enabled

            if (_config.BuildCommand.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                "Build command empty",
                "A build command is required when local site building is enabled.");

            return this;
        }

        public StaticSiteConfigValidator ValidatePublishCommand()
        {
            if (_config.PublishCommand.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                "Publish command empty",
                "A publish command is required.");

            return this;
        }
    }

    public class StaticSiteConfigValidationException : BlogClientException
    {
        public StaticSiteConfigValidationException(string title, string text, params object[] textFormatArgs) : base(title, text, textFormatArgs)
        {

        }
    }
}
