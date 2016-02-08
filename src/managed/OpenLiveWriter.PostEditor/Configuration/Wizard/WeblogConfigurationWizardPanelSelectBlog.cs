// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    public class WeblogConfigurationWizardPanelSelectBlog : WeblogConfigurationWizardPanel
    {
        private System.Windows.Forms.Label labelSelectWeblog;
        private System.Windows.Forms.ListBox listBoxSelectWeblog;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WeblogConfigurationWizardPanelSelectBlog()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.labelHeader.Text = Res.Get(StringId.ConfigWizardSelectWeblog);
            this.labelSelectWeblog.Text = Res.Get(StringId.CWSelectBlogText);
            labelSelectWeblog.Font = Res.DefaultFont;
        }

        public string LabelText
        {
            set
            {
                labelSelectWeblog.Text = value;
            }
        }

        public override ConfigPanelId? PanelId
        {
            get
            {
                return (labelSelectWeblog.Text == Res.Get(StringId.CWSelectBlogText)) ? ConfigPanelId.SelectBlog
                    : (labelSelectWeblog.Text == Res.Get(StringId.CWSelectImageEndpointText)) ? ConfigPanelId.SelectImageEndPoint
                    : (ConfigPanelId?)null;
            }
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                MaximizeWidth(labelSelectWeblog);
                LayoutHelper.NaturalizeHeight(labelSelectWeblog);
                LayoutHelper.NaturalizeHeight(labelHeader);
                LayoutHelper.DistributeVertically(10, labelSelectWeblog, listBoxSelectWeblog);
            }
        }

        public void ShowPanel(BlogInfo[] blogs, string selectedBlogId)
        {
            listBoxSelectWeblog.Items.Clear();

            // populate the list
            foreach (BlogInfo blog in blogs)
                listBoxSelectWeblog.Items.Add(blog);

            // default selection
            if (selectedBlogId != null)
            {
                foreach (BlogInfo blog in blogs)
                {
                    if (blog.Id == selectedBlogId)
                    {
                        listBoxSelectWeblog.SelectedItem = blog;
                        break;
                    }
                }
            }

            // if we have no default then select the first weblog
            if (listBoxSelectWeblog.SelectedIndex == -1)
                listBoxSelectWeblog.SelectedIndex = 0;
        }

        public BlogInfo SelectedBlog
        {
            get
            {
                return listBoxSelectWeblog.SelectedItem as BlogInfo;
            }
        }

        public override bool ValidatePanel()
        {
            return SelectedBlog != null;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelSelectWeblog = new System.Windows.Forms.Label();
            this.listBoxSelectWeblog = new System.Windows.Forms.ListBox();
            this.panelMain.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.listBoxSelectWeblog);
            this.panelMain.Controls.Add(this.labelSelectWeblog);
            //
            // labelSelectWeblog
            //
            this.labelSelectWeblog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSelectWeblog.Location = new System.Drawing.Point(20, 0);
            this.labelSelectWeblog.Name = "labelSelectWeblog";
            this.labelSelectWeblog.Size = new System.Drawing.Size(328, 32);
            this.labelSelectWeblog.TabIndex = 0;
            this.labelSelectWeblog.Text = "More than one Weblog was detected. Please select the Weblog that you\'d like to ad" +
                "d from the list below:";
            //
            // listBoxSelectWeblog
            //
            this.listBoxSelectWeblog.Location = new System.Drawing.Point(20, 32);
            this.listBoxSelectWeblog.Name = "listBoxSelectWeblog";
            this.listBoxSelectWeblog.Size = new System.Drawing.Size(280, 121);
            this.listBoxSelectWeblog.TabIndex = 1;
            //
            // WeblogConfigurationWizardPanelSelectBlog
            //
            this.Name = "WeblogConfigurationWizardPanelSelectBlog";
            this.Size = new System.Drawing.Size(432, 244);
            this.panelMain.ResumeLayout(false);

        }
        #endregion
    }
}
