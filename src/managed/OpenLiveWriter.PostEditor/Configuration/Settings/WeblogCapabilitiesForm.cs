// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{

    public class WeblogCapabilitiesForm : ApplicationDialog
    {
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColumnHeader columnHeaderCapability;
        private System.Windows.Forms.ColumnHeader columnHeaderValue;
        private System.Windows.Forms.ListView listViewCapabilities;
        private System.Windows.Forms.Label labelCapabilities;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WeblogCapabilitiesForm(IBlogClientOptions clientOptions)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.columnHeaderCapability.Text = Res.Get(StringId.WeblogCapabilitiesCapability);
            this.columnHeaderValue.Text = Res.Get(StringId.WeblogCapabilitiesValue);
            this.labelCapabilities.Text = Res.Get(StringId.WeblogCapabilitiesCaption);
            this.Text = Res.Get(StringId.WeblogCapabilitiesTitle);

            // captions
            labelCapabilities.Text = String.Format(CultureInfo.CurrentCulture, labelCapabilities.Text, ApplicationEnvironment.ProductNameQualified);
            ListViewOptionWriter listViewOptionWriter = new ListViewOptionWriter(listViewCapabilities);
            listViewOptionWriter.Write(clientOptions);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LayoutHelper.NaturalizeHeight(labelCapabilities);
            int newListViewTop = (int)(labelCapabilities.Bottom + DisplayHelper.ScaleY(8));
            int delta = newListViewTop - listViewCapabilities.Top;
            listViewCapabilities.Top = newListViewTop;
            listViewCapabilities.Height -= delta;
        }

        private class ListViewOptionWriter : DisplayableBlogClientOptionWriter
        {
            public ListViewOptionWriter(ListView listView)
            {
                _listView = listView;
            }

            protected override void WriteOption(string name, string value)
            {
                ListViewItem item = new ListViewItem(new string[] { name, value });
                _listView.Items.Add(item);
            }

            private ListView _listView;
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOK = new System.Windows.Forms.Button();
            this.listViewCapabilities = new System.Windows.Forms.ListView();
            this.columnHeaderCapability = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderValue = new System.Windows.Forms.ColumnHeader();
            this.labelCapabilities = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(141, 278);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            //
            // listViewCapabilities
            //
            this.listViewCapabilities.AutoArrange = false;
            this.listViewCapabilities.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                   this.columnHeaderCapability,
                                                                                                   this.columnHeaderValue});
            this.listViewCapabilities.FullRowSelect = true;
            this.listViewCapabilities.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewCapabilities.Location = new System.Drawing.Point(13, 44);
            this.listViewCapabilities.MultiSelect = false;
            this.listViewCapabilities.Name = "listViewCapabilities";
            this.listViewCapabilities.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.listViewCapabilities.Size = new System.Drawing.Size(329, 223);
            this.listViewCapabilities.TabIndex = 1;
            this.listViewCapabilities.View = System.Windows.Forms.View.Details;
            //
            // columnHeaderCapability
            //
            this.columnHeaderCapability.Text = "Capability";
            this.columnHeaderCapability.Width = 151;
            //
            // columnHeaderValue
            //
            this.columnHeaderValue.Text = "Value";
            this.columnHeaderValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderValue.Width = 155;
            //
            // labelCapabilities
            //
            this.labelCapabilities.Location = new System.Drawing.Point(13, 12);
            this.labelCapabilities.Name = "labelCapabilities";
            this.labelCapabilities.Size = new System.Drawing.Size(335, 27);
            this.labelCapabilities.TabIndex = 2;
            this.labelCapabilities.Text = "{0} has detected the following capabilities for your weblog:";
            //
            // WeblogCapabilitiesForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonOK;
            this.ClientSize = new System.Drawing.Size(356, 310);
            this.Controls.Add(this.labelCapabilities);
            this.Controls.Add(this.listViewCapabilities);
            this.Controls.Add(this.buttonOK);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WeblogCapabilitiesForm";
            this.Text = "Weblog Capabilities";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
