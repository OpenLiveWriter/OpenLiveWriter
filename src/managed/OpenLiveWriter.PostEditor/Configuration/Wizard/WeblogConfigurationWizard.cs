// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls.Wizard;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{

    public class WeblogConfigurationWizard : WizardForm
    {
        private LinkLabel proxyLabel;

        public WeblogConfigurationWizard(WizardController controller) : base(controller)
        {
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(51, 51, 51);
            this.ClientSize = new System.Drawing.Size(440, 348);
            this.Text = Res.Get(StringId.CWTitle);
            this.Icon = ApplicationEnvironment.ProductIcon;
            this.SizeChanged += new EventHandler(WeblogConfigurationWizard_SizeChanged);

            this.Height = Res.WizardHeight;

            proxyLabel = new LinkLabel();
            proxyLabel.Text = Res.Get(StringId.EditProxySettings);
            proxyLabel.AutoSize = true;
            proxyLabel.Location = new Point(19, 16);
            proxyLabel.LinkBehavior = LinkBehavior.HoverUnderline;
            proxyLabel.Name = "linkLabelProxy";
            proxyLabel.LinkColor = SystemInformation.HighContrast ? SystemColors.HotTrack : Color.FromArgb(0, 102, 204);
            proxyLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(proxyLabel_LinkClicked);
            panelFooter.Controls.Add(proxyLabel);
            proxyLabel.Visible = false;
            proxyLabel.TabIndex = 0;
        }

        void proxyLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PreferencesHandler.Instance.ShowWebProxyPreferences(FindForm());
        }

        void WeblogConfigurationWizard_SizeChanged(object sender, EventArgs e)
        {
            this.Text = Text;
        }

        protected override void SetWizardBody(Control control)
        {
            base.SetWizardBody(control);

            WeblogConfigurationWizardPanel wizardPanel = control as WeblogConfigurationWizardPanel;
            proxyLabel.Visible = wizardPanel.ShowProxySettingsLink;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int delta = proxyLabel.Right + 10 - buttonBack.Left;
            if (delta > 0)
            {
                proxyLabel.AutoSize = false;
                proxyLabel.Width = buttonBack.Left - 10 - proxyLabel.Left;
                LayoutHelper.NaturalizeHeight(proxyLabel);
                proxyLabel.Top = buttonCancel.Top - ((proxyLabel.Height - buttonCancel.Height) / 2);
            }
        }

        protected override void OnAddControl(System.Windows.Forms.Control c)
        {
            base.OnAddControl(c);

            WeblogConfigurationWizardPanel wizardPanel = c as WeblogConfigurationWizardPanel;
            c.Size = c.Parent.Size;
            if (wizardPanel != null)
                wizardPanel.PrepareForAdd();
        }
    }
}
