// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.PostEditor.BlogProviderButtons;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{

    internal class WeblogConfigurationWizardPanelBlogType : WeblogConfigurationWizardPanel
    {
        private System.Windows.Forms.Label labelWelcomeText;
        private System.Windows.Forms.Panel panelRadioButtons;
        private System.Windows.Forms.RadioButton radioButtonSharePoint;
        private System.Windows.Forms.RadioButton radioButtonBlogger;
        private System.Windows.Forms.RadioButton radioButtonOther;
        private System.Windows.Forms.Label labelOtherDesc;
        private System.Windows.Forms.RadioButton radioButtonWordpress;
        private System.Windows.Forms.Panel panelComboBox;
        private System.Windows.Forms.ComboBox comboBoxSelectWeblogType;
        private LinkLabel privacyPolicyLabel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WeblogConfigurationWizardPanelBlogType()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            labelOtherDesc.Visible = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant() == "en";

            labelHeader.Text = Res.Get(StringId.WizardBlogTypeWhatBlogType);
            labelWelcomeText.Text = Res.Get(StringId.WizardBlogTypeWelcome);
            radioButtonSharePoint.Text = Res.Get(StringId.WizardBlogTypeSharePoint);
            radioButtonBlogger.Text = Res.Get(StringId.WizardBlogTypeGoogleBlogger);
            radioButtonOther.Text = Res.Get(StringId.WizardBlogTypeOther);
            radioButtonWordpress.Text = Res.Get(StringId.CWWelcomeWP);

            labelOtherDesc.Text = Res.Get(StringId.BlogServiceNames);

            labelWelcomeText.Text = string.Format(CultureInfo.CurrentCulture, labelWelcomeText.Text, ApplicationEnvironment.ProductNameQualified);

            // is there at least one provider wizard definition?
            // (if so then we override the choose account ui to bury the ms-specific blog providers)
            if (BlogProviderAccountWizard.InstalledAccountWizards.Length > 0)
            {
                // toggle visibility of UI
                panelRadioButtons.Visible = false;
                panelComboBox.Visible = true;

                comboBoxSelectWeblogType.Items.Add(new WeblogType(radioButtonWordpress));

                // add standard selections
                comboBoxSelectWeblogType.Items.Add(new WeblogType(radioButtonSharePoint));

                // add all known provider customizations
                foreach (IBlogProviderAccountWizardDescription accountWizard in BlogProviderAccountWizard.InstalledAccountWizards)
                    comboBoxSelectWeblogType.Items.Add(new WeblogType(accountWizard));

                // add other weblog types which have been added on this machine
                foreach (string serviceName in WeblogConfigurationWizardSettings.ServiceNamesUsed)
                {
                    if (!SelectWeblogComboContainsServiceName(serviceName))
                        comboBoxSelectWeblogType.Items.Add(new WeblogType(serviceName));
                }

                comboBoxSelectWeblogType.Items.Add(new WeblogType(radioButtonBlogger));

                // add "another weblog type" entry
                comboBoxSelectWeblogType.Items.Add(new WeblogType(radioButtonOther));

                // select first item by default
                comboBoxSelectWeblogType.SelectedIndex = 0;

                // improve on the default selection
                if (WeblogConfigurationWizardSettings.LastServiceName != String.Empty)
                {
                    SelectServiceName(WeblogConfigurationWizardSettings.LastServiceName);
                }
                else // no last added (first run), use provider customization
                {
                    SelectServiceName(BlogProviderAccountWizard.InstalledAccountWizards[0].ServiceName);
                }
            }

            // track changing of selection (used to clear credentials)
            comboBoxSelectWeblogType.SelectedIndexChanged += new EventHandler(UserChangedSelectionHandler);
            radioButtonWordpress.CheckedChanged += new EventHandler(UserChangedSelectionHandler);
            radioButtonSharePoint.CheckedChanged += new EventHandler(UserChangedSelectionHandler);
            radioButtonBlogger.CheckedChanged += new EventHandler(UserChangedSelectionHandler);
            radioButtonOther.CheckedChanged += new EventHandler(UserChangedSelectionHandler);
        }

        public override void NaturalizeLayout()
        {
            if (!DesignMode)
            {
                //labelWelcomeText.Visible = false;
                //panelRadioButtons.Top = 0;

                MaximizeWidth(labelWelcomeText);
                LayoutHelper.NaturalizeHeightAndDistribute(10, labelWelcomeText, panelRadioButtons, panelComboBox);

                MaximizeWidth(panelRadioButtons);

                MaximizeWidth(radioButtonWordpress);
                MaximizeWidth(radioButtonSharePoint);
                MaximizeWidth(radioButtonBlogger);
                MaximizeWidth(radioButtonOther);
                MaximizeWidth(labelOtherDesc);

                using (new AutoGrow(panelRadioButtons, AnchorStyles.Bottom, true))
                {
                    LayoutHelper.NaturalizeHeightAndDistribute(3, radioButtonWordpress, radioButtonSharePoint, radioButtonBlogger, radioButtonOther);
                    labelOtherDesc.Top = radioButtonOther.Bottom;
                }
            }
        }

        public override ConfigPanelId? PanelId
        {
            get { return ConfigPanelId.BlogType; }
        }

        public void OnDisplayPanel()
        {
            _userChangedSelection = false;
        }

        public bool UserChangedSelection
        {
            get
            {
                return _userChangedSelection;
            }
        }
        private bool _userChangedSelection;

        public bool IsSharePointBlog
        {
            get
            {
                if (panelRadioButtons.Visible)
                {
                    return radioButtonSharePoint.Checked;
                }
                else
                {
                    return SelectedWeblog.RadioButton == radioButtonSharePoint;
                }
            }
        }

        public bool IsGoogleBloggerBlog
        {
            get
            {
                if (panelRadioButtons.Visible)
                {
                    return radioButtonBlogger.Checked;
                }
                else
                {
                    return SelectedWeblog.RadioButton == radioButtonBlogger;
                }
            }
        }

        public IBlogProviderAccountWizardDescription ProviderAccountWizard
        {
            get
            {
                if (panelRadioButtons.Visible)
                {
                    return null;
                }
                else
                {
                    return SelectedWeblog.ProviderAccountWizard;
                }
            }
        }

        private WeblogType SelectedWeblog
        {
            get
            {
                return comboBoxSelectWeblogType.SelectedItem as WeblogType;
            }
        }

        private void UserChangedSelectionHandler(object sender, EventArgs ea)
        {
            _userChangedSelection = true;
        }

        private bool SelectWeblogComboContainsServiceName(string serviceName)
        {
            // scan for service name
            foreach (WeblogType weblogType in comboBoxSelectWeblogType.Items)
                if (weblogType.ServiceName.Equals(serviceName))
                    return true;

            // didn't find it
            return false;
        }

        private void SelectServiceName(string serviceName)
        {
            foreach (WeblogType weblogType in comboBoxSelectWeblogType.Items)
            {
                if (weblogType.ServiceName.Equals(serviceName))
                {
                    comboBoxSelectWeblogType.SelectedItem = weblogType;
                    break;
                }
            }
        }

        private class WeblogType
        {
            public WeblogType(IBlogProviderAccountWizardDescription providerAccountWizard)
            {
                _providerAccountWizard = providerAccountWizard;
                _name = providerAccountWizard.ServiceName;
            }

            public WeblogType(RadioButton radioButton)
            {
                _radioButton = radioButton;
                _name = radioButton.Text.Replace("&", String.Empty);
            }

            public WeblogType(string name)
            {
                _name = name;
            }

            public string ServiceName
            {
                get
                {
                    // remove numonics
                    return _name;
                }
            }

            public override string ToString()
            {
                return ServiceName;
            }

            public RadioButton RadioButton
            {
                get
                {
                    return _radioButton;
                }
            }

            public IBlogProviderAccountWizardDescription ProviderAccountWizard
            {
                get
                {
                    return _providerAccountWizard;
                }
            }

            private string _name;
            private RadioButton _radioButton;
            private IBlogProviderAccountWizardDescription _providerAccountWizard;
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
            this.labelWelcomeText = new System.Windows.Forms.Label();
            this.panelRadioButtons = new System.Windows.Forms.Panel();
            this.radioButtonWordpress = new System.Windows.Forms.RadioButton();
            this.radioButtonSharePoint = new System.Windows.Forms.RadioButton();
            this.radioButtonBlogger = new System.Windows.Forms.RadioButton();
            this.radioButtonOther = new System.Windows.Forms.RadioButton();
            this.labelOtherDesc = new System.Windows.Forms.Label();
            this.panelComboBox = new System.Windows.Forms.Panel();
            this.comboBoxSelectWeblogType = new System.Windows.Forms.ComboBox();
            this.privacyPolicyLabel = new System.Windows.Forms.LinkLabel();
            this.panelMain.SuspendLayout();
            this.panelRadioButtons.SuspendLayout();
            this.panelComboBox.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.Controls.Add(this.privacyPolicyLabel);
            this.panelMain.Controls.Add(this.panelRadioButtons);
            this.panelMain.Controls.Add(this.labelWelcomeText);
            this.panelMain.Controls.Add(this.panelComboBox);
            //
            // labelWelcomeText
            //
            this.labelWelcomeText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelWelcomeText.Location = new System.Drawing.Point(20, 0);
            this.labelWelcomeText.Name = "labelWelcomeText";
            this.labelWelcomeText.Size = new System.Drawing.Size(332, 40);
            this.labelWelcomeText.TabIndex = 2;
            this.labelWelcomeText.Text = "{0} can create and post entries on your blog, and works with a wide variety of we" +
    "blog services. Select the type of weblog service to continue.";
            //
            // panelRadioButtons
            //
            this.panelRadioButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRadioButtons.Controls.Add(this.radioButtonWordpress);
            this.panelRadioButtons.Controls.Add(this.radioButtonSharePoint);
            this.panelRadioButtons.Controls.Add(this.radioButtonBlogger);
            this.panelRadioButtons.Controls.Add(this.radioButtonOther);
            this.panelRadioButtons.Controls.Add(this.labelOtherDesc);
            this.panelRadioButtons.Location = new System.Drawing.Point(20, 88);
            this.panelRadioButtons.Name = "panelRadioButtons";
            this.panelRadioButtons.Size = new System.Drawing.Size(388, 72);
            this.panelRadioButtons.TabIndex = 4;
            //
            // radioButtonWordpress
            //
            this.radioButtonWordpress.Location = new System.Drawing.Point(0, 0);
            this.radioButtonWordpress.Name = "radioButtonWordpress";
            this.radioButtonWordpress.Size = new System.Drawing.Size(104, 24);
            this.radioButtonWordpress.TabIndex = 1;
            this.radioButtonWordpress.TabStop = true;
            this.radioButtonWordpress.Text = "Wordpress";
            //
            // radioButtonSharePoint
            //
            this.radioButtonSharePoint.Location = new System.Drawing.Point(0, 24);
            this.radioButtonSharePoint.Name = "radioButtonSharePoint";
            this.radioButtonSharePoint.Size = new System.Drawing.Size(104, 24);
            this.radioButtonSharePoint.TabIndex = 2;
            this.radioButtonSharePoint.Text = "Share&Point weblog";
            // 
            // radioButtonBlogger
            // 
            this.radioButtonBlogger.Location = new System.Drawing.Point(0, 72);
            this.radioButtonBlogger.Name = "radioButtonBlogger";
            this.radioButtonBlogger.Size = new System.Drawing.Size(104, 24);
            this.radioButtonBlogger.TabIndex = 3;
            this.radioButtonBlogger.Text = "&Google Blogger";
            // 
            // radioButtonOther
            //
            this.radioButtonOther.Location = new System.Drawing.Point(0, 48);
            this.radioButtonOther.Name = "radioButtonOther";
            this.radioButtonOther.Size = new System.Drawing.Size(104, 24);
            this.radioButtonOther.TabIndex = 4;
            this.radioButtonOther.Text = "Another &weblog service";
            //
            // labelOtherDesc
            //
            this.labelOtherDesc.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelOtherDesc.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelOtherDesc.Location = new System.Drawing.Point(18, 0);
            this.labelOtherDesc.Name = "labelOtherDesc";
            this.labelOtherDesc.Size = new System.Drawing.Size(332, 40);
            this.labelOtherDesc.TabIndex = 5;
            this.labelOtherDesc.Text = "TypePad and others";
            // 
            // panelComboBox
            //
            this.panelComboBox.Controls.Add(this.comboBoxSelectWeblogType);
            this.panelComboBox.Location = new System.Drawing.Point(20, 88);
            this.panelComboBox.Name = "panelComboBox";
            this.panelComboBox.Size = new System.Drawing.Size(328, 21);
            this.panelComboBox.TabIndex = 5;
            this.panelComboBox.Visible = false;
            //
            // comboBoxSelectWeblogType
            //
            this.comboBoxSelectWeblogType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSelectWeblogType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSelectWeblogType.Location = new System.Drawing.Point(0, 0);
            this.comboBoxSelectWeblogType.Name = "comboBoxSelectWeblogType";
            this.comboBoxSelectWeblogType.Size = new System.Drawing.Size(328, 21);
            this.comboBoxSelectWeblogType.TabIndex = 0;
            //
            // privacyPolicyLabel
            //
            this.privacyPolicyLabel.AutoSize = true;
            this.privacyPolicyLabel.Location = new System.Drawing.Point(23, 167);
            this.privacyPolicyLabel.Name = "privacyPolicyLabel";
            this.privacyPolicyLabel.Size = new System.Drawing.Size(256, 13);
            this.privacyPolicyLabel.TabIndex = 6;
            this.privacyPolicyLabel.TabStop = true;
            this.privacyPolicyLabel.Text = "We follow the privacy policy of the .NET Foundation.";
            this.privacyPolicyLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.privacyPolicyLabel_LinkClicked);
            //
            // WeblogConfigurationWizardPanelBlogType
            //
            this.Name = "WeblogConfigurationWizardPanelBlogType";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.panelRadioButtons.ResumeLayout(false);
            this.panelComboBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void privacyPolicyLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellHelper.LaunchUrl("http://www.dotnetfoundation.org/privacy-policy");
        }
    }
}

