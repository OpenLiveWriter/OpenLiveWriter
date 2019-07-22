using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenLiveWriter.PostEditor.Configuration;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSite
{
    public class StaticSiteAdvancedConfigController
    {
        private TemporaryBlogSettings _temporarySettings;
        private StaticSiteAdvancedConfigForm _form;

        public bool SettingsModified { get; set; } = false;

        public StaticSiteAdvancedConfigController(TemporaryBlogSettings blogSettings)
        {
            _temporarySettings = blogSettings;

            _form = new StaticSiteAdvancedConfigForm(this);
            _form.AddTabPage("test1", new StaticSiteAdvancedConfigPanel());
            _form.AddTabPage("test2", new StaticSiteAdvancedConfigPanel());
        }

        private bool EditWeblogTemporarySettings(IWin32Window owner)
        {
            _form.ShowDialog(owner);
            return SettingsModified;
        }

        public static bool EditTemporarySettings(IWin32Window owner, TemporaryBlogSettings settings)
            => new StaticSiteAdvancedConfigController(settings).EditWeblogTemporarySettings(owner);
    }
}