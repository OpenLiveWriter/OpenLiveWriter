using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

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
            .ValidateBuildCommand()
            .ValidatePublishCommand();

        #region Path Validation

        public StaticSiteConfigValidator ValidateLocalSitePath()
        {
            if(!Directory.Exists(_config.LocalSitePath))
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathLocalSitePathNotFound),
                    _config.LocalSitePath);

            return this;
        }

        public StaticSiteConfigValidator ValidatePostsPath()
        {
            var postsPathFull = $"{_config.LocalSitePath}\\{_config.PostsPath}";

            // If the Posts path is empty, display an error
            if (_config.PostsPath.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPostsEmpty));

            // If the Posts path doesn't exist, display an error
            if (!Directory.Exists(postsPathFull))
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPostsNotFound),
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
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPagesEmpty));

            // If the path doesn't exist, display an error
            if (!Directory.Exists(pagesPathFull))
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPagesNotFound),
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
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathDraftsEmpty));

            // If the path doesn't exist, display an error
            if (!Directory.Exists(draftsPathFull))
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathDraftsNotFound),
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
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathImagesEmpty));

            // If the path doesn't exist, display an error
            if (!Directory.Exists(imagesPathFull))
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathImagesNotFound),
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
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathOutputEmpty));

            // If the path doesn't exist, display an error
            if (!Directory.Exists(outputPathFull))
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathOutputNotFound),
                    outputPathFull);

            return this;
        }

        #endregion

        public StaticSiteConfigValidator ValidateBuildCommand()
        {
            if (!_config.BuildingEnabled) return this; // Don't validate if building isn't enabled

            if (_config.BuildCommand.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                Res.Get(StringId.SSGErrorBuildCommandEmptyTitle),
                Res.Get(StringId.SSGErrorBuildCommandEmptyText));

            return this;
        }

        public StaticSiteConfigValidator ValidatePublishCommand()
        {
            if (_config.PublishCommand.Trim() == string.Empty)
                throw new StaticSiteConfigValidationException(
                Res.Get(StringId.SSGErrorPublishCommandEmptyTitle),
                Res.Get(StringId.SSGErrorPublishCommandEmptyText));

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
