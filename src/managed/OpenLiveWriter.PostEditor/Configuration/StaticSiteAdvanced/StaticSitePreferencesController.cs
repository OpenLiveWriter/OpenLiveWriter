using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients.StaticSite;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Configuration.Wizard;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    public class StaticSitePreferencesController
    {
        private TemporaryBlogSettings _temporarySettings;
        private PreferencesForm _form;

        private GeneralPanel panelGeneral;
        private AuthoringPanel panelAuthoring;
        private FrontMatterPanel panelFrontMatter;
        private BuildPublishPanel panelBuildPublish;

        public StaticSitePreferencesController(TemporaryBlogSettings blogSettings)
        {
            _temporarySettings = blogSettings;
            _form = new PreferencesForm();

            panelGeneral = new GeneralPanel(this, blogSettings);
            panelAuthoring = new AuthoringPanel(this, blogSettings);
            panelFrontMatter = new FrontMatterPanel(this, blogSettings);
            panelBuildPublish = new BuildPublishPanel(this, blogSettings);
        }

        private bool EditWeblogTemporarySettings(IWin32Window owner)
        {
            LoadFromStaticSiteConfig(StaticSiteConfig.LoadConfigFromBlogSettings(_temporarySettings));

            // Show form
            using (var preferencesForm = new StaticSitePreferencesForm())
            {
                using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(preferencesForm))
                {
                    // Customize form title and behavior
                    preferencesForm.Text = $"Static Site Configuration for '{_temporarySettings.BlogName}'"; //TODO use strings
                    preferencesForm.HideApplyButton();

                    // Add panels
                    int iPanel = 0;
                    preferencesForm.SetEntry(iPanel++, panelGeneral);
                    preferencesForm.SetEntry(iPanel++, panelAuthoring);
                    preferencesForm.SetEntry(iPanel++, panelFrontMatter);
                    preferencesForm.SetEntry(iPanel++, panelBuildPublish);

                    preferencesForm.SelectedIndex = 0;
                    
                    // Show the dialog
                    var result = preferencesForm.ShowDialog(owner);
                    
                    if(result == DialogResult.OK)
                    {
                        // Create a static site config
                        var ssgConfig = StaticSiteConfig.LoadConfigFromBlogSettings(_temporarySettings);
                        SaveToStaticSiteConfig(ssgConfig);
                        // All panels should be validated by this point, so save the settings

                        return true;
                    }

                }
            }

            return false;
        }

        private void LoadFromStaticSiteConfig(StaticSiteConfig config)
        {
            // General
            panelGeneral.SiteTitle = config.SiteTitle;
            panelGeneral.SiteUrl = config.SiteUrl;
            panelGeneral.LocalSitePath = config.LocalSitePath;

            // Authoring
            panelAuthoring.PostsPath = config.PostsPath;
            panelAuthoring.DraftsEnabled = config.DraftsEnabled;
            panelAuthoring.DraftsPath = config.DraftsPath;
            panelAuthoring.PagesEnabled = config.PagesEnabled;
            panelAuthoring.PagesPath = config.PagesPath;
            panelAuthoring.PagesStoredInRoot = config.PagesPath == ".";
            panelAuthoring.ImagesEnabled = config.ImagesEnabled;
            panelAuthoring.ImagesPath = config.ImagesPath;

            // Front Matter
            panelFrontMatter.Keys = config.FrontMatterKeys;

            // Building and Publishing
            panelBuildPublish.ShowCmdWindows = config.ShowCmdWindows;
            panelBuildPublish.CmdTimeoutMs = config.CmdTimeoutMs;
            panelBuildPublish.BuildingEnabled = config.BuildingEnabled;
            panelBuildPublish.BuildCommand = config.BuildCommand;
            panelBuildPublish.OutputPath = config.OutputPath;
            panelBuildPublish.PublishCommand = config.PublishCommand;
        }

        private void SaveToStaticSiteConfig(StaticSiteConfig config)
        {
            // General
            config.SiteTitle = panelGeneral.SiteTitle;
            config.SiteUrl = panelGeneral.SiteUrl;
            config.LocalSitePath = panelGeneral.LocalSitePath;

            // Authoring
            config.PostsPath = panelAuthoring.PostsPath;
            config.DraftsEnabled = panelAuthoring.DraftsEnabled;
            config.DraftsPath = panelAuthoring.DraftsPath;
            config.PagesEnabled = panelAuthoring.PagesEnabled;
            config.PagesPath = panelAuthoring.PagesStoredInRoot ? "." : panelAuthoring.PagesPath;
            config.ImagesEnabled = panelAuthoring.ImagesEnabled;
            config.ImagesPath = panelAuthoring.ImagesPath;

            // Front Matter
            config.FrontMatterKeys = panelFrontMatter.Keys;

            // Building and Publishing
            config.ShowCmdWindows = panelBuildPublish.ShowCmdWindows;
            config.CmdTimeoutMs = panelBuildPublish.CmdTimeoutMs;
            config.BuildingEnabled = panelBuildPublish.BuildingEnabled;
            config.BuildCommand = panelBuildPublish.BuildCommand;
            config.OutputPath = panelBuildPublish.OutputPath;
            config.PublishCommand = panelBuildPublish.PublishCommand;
        }


        public void GeneralPanel_RunAccountWizard()
        {
            WeblogConfigurationWizardController.EditTemporarySettings(_form, _temporarySettings);
            // Reload the settings into the form
            LoadFromStaticSiteConfig(StaticSiteConfig.LoadConfigFromBlogSettings(_temporarySettings));
        }

        public void GeneralPanel_RunAutoDetect()
        {
            var ssgConfig = StaticSiteConfig.LoadConfigFromBlogSettings(_temporarySettings);
            var result = StaticSiteConfigDetector.AttmeptAutoDetect(ssgConfig);

            if(result)
            {
                // Successful detection of parameters
                MessageBox.Show(
                    string.Format(Res.Get(StringId.CWStaticSiteConfigDetection), Res.Get(StringId.ProductNameVersioned)),
                    Res.Get(StringId.ProductNameVersioned),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadFromStaticSiteConfig(ssgConfig);
            }
        }

        public static bool EditTemporarySettings(IWin32Window owner, TemporaryBlogSettings settings)
            => new StaticSitePreferencesController(settings).EditWeblogTemporarySettings(owner);
    }
}