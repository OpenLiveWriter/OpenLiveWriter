using System;
using System.Collections.Generic;
using System.Data;
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
    public class PreferencesController
    {
        private TemporaryBlogSettings _temporarySettings;
        private PreferencesForm _form;

        private GeneralPanel panelGeneral;
        private AuthoringPanel panelAuthoring;
        private FrontMatterPanel panelFrontMatter;
        private BuildPublishPanel panelBuildPublish;

        public PreferencesController(TemporaryBlogSettings blogSettings)
        {
            _temporarySettings = blogSettings;
            _form = new PreferencesForm();

            panelGeneral = new GeneralPanel(this);
            panelAuthoring = new AuthoringPanel(this);
            panelFrontMatter = new FrontMatterPanel(this);
            panelBuildPublish = new BuildPublishPanel(this);
        }

        private bool EditWeblogTemporarySettings(IWin32Window owner)
        {
            LoadFromStaticSiteConfig(CreateSiteConfig());

            // Show form
            using (var preferencesForm = new StaticSitePreferencesForm())
            {
                using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(preferencesForm))
                {
                    // Customize form title and behavior
                    preferencesForm.Text = $"Static Site Configuration for '{_temporarySettings.BlogName}'"; //String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WeblogSettings), blogSettings.BlogName);

                    // Add panels
                    int iPanel = 0;
                    preferencesForm.SetEntry(iPanel++, panelGeneral);
                    preferencesForm.SetEntry(iPanel++, panelAuthoring);
                    preferencesForm.SetEntry(iPanel++, panelFrontMatter);
                    preferencesForm.SetEntry(iPanel++, panelBuildPublish);

                    preferencesForm.SelectedIndex = 0;
                    
                    // Show the dialog
                    preferencesForm.ShowDialog(owner);
                }
            }

            return false;
        }

        private StaticSiteConfig CreateSiteConfig()
        {
            var ssgConfig = new StaticSiteConfig();
            ssgConfig.LoadFromBlogSettings(_temporarySettings);
            return ssgConfig;
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

        public void GeneralPanel_RunAccountWizard()
        {
            WeblogConfigurationWizardController.EditTemporarySettings(_form, _temporarySettings);
            // Reload the settings into the form
            LoadFromStaticSiteConfig(CreateSiteConfig());
        }

        public void GeneralPanel_RunAutoDetect()
        {
            var ssgConfig = CreateSiteConfig();
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
            => new PreferencesController(settings).EditWeblogTemporarySettings(owner);
    }
}