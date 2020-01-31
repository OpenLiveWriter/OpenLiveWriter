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
        public StaticSiteConfig Config { get; set; }

        private TemporaryBlogSettings _temporarySettings;

        private StaticSitePreferencesForm formPreferences;
        private GeneralPanel panelGeneral;
        private AuthoringPanel panelAuthoring;
        private FrontMatterPanel panelFrontMatter;
        private BuildPublishPanel panelBuildPublish;

        public StaticSitePreferencesController(TemporaryBlogSettings blogSettings)
        {
            _temporarySettings = blogSettings;
            LoadConfigFromBlogSettings();

            panelGeneral = new GeneralPanel(this);
            panelAuthoring = new AuthoringPanel(this);
            panelFrontMatter = new FrontMatterPanel(this);
            panelBuildPublish = new BuildPublishPanel(this);

            LoadConfigIntoPanels();
        }

        public void LoadConfigFromBlogSettings()
            => Config = StaticSiteConfig.LoadConfigFromBlogSettings(_temporarySettings);

        public void LoadConfigIntoPanels()
        {
            panelGeneral.LoadConfig();
            panelAuthoring.LoadConfig();
            panelFrontMatter.LoadConfig();
            panelBuildPublish.LoadConfig();
        }

        /// <summary>
        /// Save the StaticSiteConfig to an arbitrary TemporaryBlogSettings
        /// </summary>
        /// <param name="settings">the TemporaryBlogSettings object</param>
        private void SaveConfigToBlogSettings(TemporaryBlogSettings settings)
        {
            Config.SaveToCredentials(settings.Credentials);
            settings.BlogName = Config.SiteTitle;
            settings.HomepageUrl = Config.SiteUrl;
        }

        private bool EditWeblogTemporarySettings(IWin32Window owner)
        {
            LoadConfigFromBlogSettings();
            LoadConfigIntoPanels();

            // Show form
            using (formPreferences = new StaticSitePreferencesForm(this))
            {
                using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(formPreferences))
                {
                    // Customize form title and behavior
                    formPreferences.Text = string.Format(Res.Get(StringId.SSGConfigTitle), _temporarySettings.BlogName);
                    formPreferences.HideApplyButton();

                    // Add panels
                    int iPanel = 0;
                    formPreferences.SetEntry(iPanel++, panelGeneral);
                    formPreferences.SetEntry(iPanel++, panelAuthoring);
                    formPreferences.SetEntry(iPanel++, panelFrontMatter);
                    formPreferences.SetEntry(iPanel++, panelBuildPublish);

                    formPreferences.SelectedIndex = 0;
                    
                    // Show the dialog
                    var result = formPreferences.ShowDialog(owner);
                    
                    if(result == DialogResult.OK)
                    {
                        // All panels should be validated by this point, so save the settings
                        
                        SaveConfigToBlogSettings(_temporarySettings);
                        return true;
                    }

                }
            }

            return false;
        }

        public void GeneralPanel_RunAccountWizard()
        {
            var settingsCopy = TemporaryBlogSettings.CreateNew();
            settingsCopy.CopyFrom(_temporarySettings);
            SaveConfigToBlogSettings(settingsCopy);

            var result = WeblogConfigurationWizardController.EditTemporarySettings(formPreferences, settingsCopy);
            
            // If wizard is successful, load the new settings back into the form.
            if(result)
            {
                _temporarySettings.CopyFrom(settingsCopy);
                Config = StaticSiteConfig.LoadConfigFromBlogSettings(_temporarySettings);
                LoadConfigIntoPanels();
            }
        }

        public void GeneralPanel_RunAutoDetect()
        {
            var result = StaticSiteConfigDetector.AttmeptAutoDetect(Config);

            if(result)
            {
                // Successful detection of parameters
                MessageBox.Show(
                    string.Format(Res.Get(StringId.CWStaticSiteConfigDetection), Res.Get(StringId.ProductNameVersioned)),
                    Res.Get(StringId.ProductNameVersioned),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadConfigIntoPanels();
            }
        }

        public static bool EditTemporarySettings(IWin32Window owner, TemporaryBlogSettings settings)
            => new StaticSitePreferencesController(settings).EditWeblogTemporarySettings(owner);
    }
}