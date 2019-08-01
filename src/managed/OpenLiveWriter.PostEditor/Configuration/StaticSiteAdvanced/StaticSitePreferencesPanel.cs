using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    public abstract class StaticSitePreferencesPanel : PreferencesPanel
    {
        protected StaticSitePreferencesController _controller;
        protected TemporaryBlogSettings _blogSettings;

        public StaticSitePreferencesPanel(StaticSitePreferencesController controller, TemporaryBlogSettings blogSettings) : base()
        {
            _controller = controller;
            _blogSettings = blogSettings;
        }
    }
}
