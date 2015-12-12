// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor
{
    public class PingPreferencesPanel : PreferencesPanel
    {
        private System.Windows.Forms.CheckBox chkPing;
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox txtUrls;
        private bool loading;

        public PingPreferencesPanel()
        {
            InitializeComponent();

            chkPing.Text = Res.Get(StringId.PingPrefUrl);
            PanelName = Res.Get(StringId.PingPrefName);

            PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PingPreferences.png");

            DoLoad();
            ManageState();
            this.txtUrls.RightToLeft = RightToLeft.No;
            if (BidiHelper.IsRightToLeft)
                txtUrls.TextAlign = HorizontalAlignment.Right;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                DisplayHelper.AutoFitSystemCheckBox(chkPing, chkPing.Width, chkPing.Width);
                LayoutHelper.NaturalizeHeightAndDistribute(8, Controls);
            }
        }

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
            this.chkPing = new System.Windows.Forms.CheckBox();
            this.txtUrls = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // chkPing
            //
            this.chkPing.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkPing.Location = new System.Drawing.Point(8, 32);
            this.chkPing.Name = "chkPing";
            this.chkPing.Size = new System.Drawing.Size(344, 24);
            this.chkPing.TabIndex = 3;
            this.chkPing.Text = "Send &pings to the URLs below (one URL per line)";
            this.chkPing.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkPing.CheckedChanged += new System.EventHandler(this.chkPing_CheckedChanged);
            //
            // txtUrls
            //
            this.txtUrls.AcceptsReturn = true;
            this.txtUrls.Location = new System.Drawing.Point(8, 56);
            this.txtUrls.Multiline = true;
            this.txtUrls.Name = "txtUrls";
            this.txtUrls.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUrls.Size = new System.Drawing.Size(344, 136);
            this.txtUrls.TabIndex = 4;
            this.txtUrls.Text = "urls";
            this.txtUrls.WordWrap = false;
            this.txtUrls.TextChanged += new System.EventHandler(this.txtUrls_TextChanged);
            //
            // PingPreferencesPanel
            //
            this.Controls.Add(this.txtUrls);
            this.Controls.Add(this.chkPing);
            this.Name = "PingPreferencesPanel";
            this.PanelName = "Ping Servers";
            this.Size = new System.Drawing.Size(416, 424);
            this.Controls.SetChildIndex(this.chkPing, 0);
            this.Controls.SetChildIndex(this.txtUrls, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void DoLoad()
        {
            loading = true;
            try
            {
                if (PostEditorSettings.PingUrls.Length > 0)
                    txtUrls.Text = StringHelper.Join(PostEditorSettings.PingUrls, "\r\n") + "\r\n";
                else
                    txtUrls.Text = string.Empty;
                chkPing.Checked = PostEditorSettings.Ping;
            }
            finally
            {
                loading = false;
            }
        }

        public override bool PrepareSave(SwitchToPanel switchToPanel)
        {
            string urls = txtUrls.Text;

            if (urls.Trim().Length == 0 && chkPing.Checked)
            {
                switchToPanel();
                txtUrls.Focus();
                DisplayMessage.Show(MessageId.PingUrlRequired);
                return false;
            }

            int insertedCharacters = 0;
            for (Match m = Regex.Match(urls, "([^\r\n]+)"); m.Success; m = m.NextMatch())
            {
                string url = m.Value.Trim();
                if (url.Length == 0)
                    continue;
                if (!UrlHelper.IsUrl(url))
                {
                    int realIndex = m.Index + insertedCharacters;

                    const string DEFAULT_SCHEME = "http://";

                    if (UrlHelper.IsUrl(DEFAULT_SCHEME + url))
                    {
                        txtUrls.Text = txtUrls.Text.Substring(0, realIndex) + DEFAULT_SCHEME + txtUrls.Text.Substring(realIndex);
                        insertedCharacters += DEFAULT_SCHEME.Length;
                    }
                    else
                    {
                        switchToPanel();
                        txtUrls.Select(realIndex, m.Length);
                        txtUrls.Focus();
                        txtUrls.HideSelection = false;
                        DisplayMessage.Show(MessageId.InvalidPingUrl);
                        txtUrls.HideSelection = true;
                        return false;
                    }
                }
            }
            return true;
        }

        public override void Save()
        {
            ArrayList urls = new ArrayList();
            foreach (string url in txtUrls.Lines)
            {
                string trimmed = url.Trim();
                if (trimmed.Length > 0 && !urls.Contains(trimmed))
                    urls.Add(trimmed);
            }
            PostEditorSettings.PingUrls = (string[])urls.ToArray(typeof(string));
            PostEditorSettings.Ping = chkPing.Checked;
            DoLoad();
        }

        private void chkPing_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!loading)
                OnModified(EventArgs.Empty);

            ManageState();
        }

        private void txtUrls_TextChanged(object sender, System.EventArgs e)
        {
            if (!loading)
                OnModified(EventArgs.Empty);
        }

        private void ManageState()
        {
            txtUrls.Enabled = chkPing.Checked;
        }
    }
}
