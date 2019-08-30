using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    public class StaticSitePreferencesForm : PreferencesForm
    {
        private StaticSitePreferencesController _controller;

        public StaticSitePreferencesForm(StaticSitePreferencesController controller)
        {
            _controller = controller;
        }

        
        protected override bool SavePreferences()
        {
            // We need to save all the results before validating them, as the validity of some settings are dependant on others,
            // eg. PostPath is dependant on LocalSitePath

            // Clone the existing config in-case of validation failure
            var originalConfig = _controller.Config.Clone();

            foreach (StaticSitePreferencesPanel preferencesPanel in preferencesPanelList)
            {
                preferencesPanel.Save();
            }

            // On succesful validation, hand control back to controller
            if (base.SavePreferences()) return true;
            // Otherwise, reset settings to pre-validation state and return false
            _controller.Config = originalConfig;
            return false;
        }
    }
}
