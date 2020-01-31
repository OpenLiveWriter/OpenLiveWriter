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
    public class StaticSitePreferencesPanel : PreferencesPanel
    {
        private System.ComponentModel.Container components = null;

        protected StaticSitePreferencesController _controller;

        public StaticSitePreferencesPanel() : base()
        {
            // This code should never be called at runtime, is only used for the designer 
            _controller = null;
        }

        public StaticSitePreferencesPanel(StaticSitePreferencesController controller) : base()
        {
            _controller = controller;
        }

        public virtual void LoadConfig() { }
        public virtual void ValidateConfig() { }

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

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
