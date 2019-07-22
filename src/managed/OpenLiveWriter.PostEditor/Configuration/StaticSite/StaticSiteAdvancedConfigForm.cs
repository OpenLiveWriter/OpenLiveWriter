using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSite
{
    public partial class StaticSiteAdvancedConfigForm : Form
    {
        private StaticSiteAdvancedConfigController _controller;

        public StaticSiteAdvancedConfigForm(StaticSiteAdvancedConfigController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        public void AddTabPage(string title, Control tabPageControl)
        {
            var page = new TabPage(title);
            page.Controls.Add(tabPageControl);
            tabControl.TabPages.Add(page);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            _controller.SettingsModified = true;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            _controller.SettingsModified = true;
        }
    }
}
