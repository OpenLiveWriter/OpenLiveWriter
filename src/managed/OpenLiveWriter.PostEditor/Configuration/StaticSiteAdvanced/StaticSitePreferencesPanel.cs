using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.BlogClient.Clients.StaticSite;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    public abstract class StaticSitePreferencesPanel : PreferencesPanel
    {
        protected StaticSitePreferencesController _controller;

        public StaticSitePreferencesPanel(StaticSitePreferencesController controller) : base()
        {
            _controller = controller;
        }

        public abstract void LoadConfig();
        public abstract void ValidateConfig();

        public override bool PrepareSave(SwitchToPanel switchToPanel)
        {
            try
            {
                ValidateConfig();
            }
            catch (StaticSiteConfigValidationException ex)
            {
                MessageBox.Show(ex.Text, ex.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                switchToPanel();
                return false;
            }
            return true;
        }
    }
}
