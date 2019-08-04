// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients.StaticSite;
using OpenLiveWriter.PostEditor;
using OpenLiveWriter.ApplicationFramework.Preferences;
using OpenLiveWriter.PostEditor.Configuration.Wizard;

using KeyIdentifier = OpenLiveWriter.BlogClient.Clients.StaticSite.StaticSiteConfigFrontMatterKeys.KeyIdentifier;

namespace OpenLiveWriter.PostEditor.Configuration.StaticSiteAdvanced
{
    /// <summary>
    /// Summary description for AccountPanel.
    /// </summary>
    public class FrontMatterPanel : StaticSitePreferencesPanel
    {
        private DataGridView dataGridView;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private DataGridViewTextBoxColumn colProperty;
        private DataGridViewTextBoxColumn colKey;
        private Label labelSubtitle;
        private Button buttonResetDefaults;
        private Dictionary<KeyIdentifier, DataGridViewRow> _keyRowMap = new Dictionary<KeyIdentifier, DataGridViewRow>();

        public FrontMatterPanel() : base()
        {
            InitializeComponent();
            LocalizeStrings();
        }

        public FrontMatterPanel(StaticSitePreferencesController controller)
            : base(controller)
        {
            InitializeComponent();
            LocalizeStrings();
        }

        private void LocalizeStrings()
        {
            PanelName = Res.Get(StringId.SSGConfigFrontMatterTitle);
            labelSubtitle.Text = Res.Get(StringId.SSGConfigFrontMatterSubtitle);

            colKey.HeaderText = Res.Get(StringId.SSGConfigFrontMatterKeyCol);
            colProperty.HeaderText = Res.Get(StringId.SSGConfigFrontMatterPropertyCol);

            buttonResetDefaults.Text = Res.Get(StringId.SSGConfigFrontMatterReset);
        }

        public override void LoadConfig()
        {
            Keys = _controller.Config.FrontMatterKeys;
        }

        public override void ValidateConfig()
        {
            // No validator for FrontMatterKeys yet
        }

        public override void Save()
        {
            _controller.Config.FrontMatterKeys = Keys;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            dataGridView?.AutoResizeRows();
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

        private void AddTableRow(KeyIdentifier keyIdentifier, string prop, string key)
        {
            _keyRowMap[keyIdentifier] = new DataGridViewRow();
            _keyRowMap[keyIdentifier].Cells.Add(new DataGridViewTextBoxCell()
            {
                Value = prop
            });
            _keyRowMap[keyIdentifier].Cells.Add(new DataGridViewTextBoxCell()
            {
                Value = key
            });
            dataGridView.Rows.Add(_keyRowMap[keyIdentifier]);
        }

        private string GetTableRow(KeyIdentifier keyIdentifier)
            => _keyRowMap[keyIdentifier].Cells[1].Value as string;

        public StaticSiteConfigFrontMatterKeys Keys
        {
            get => new StaticSiteConfigFrontMatterKeys()
            {
                IdKey = GetTableRow(KeyIdentifier.Id),
                TitleKey = GetTableRow(KeyIdentifier.Title),
                DateKey = GetTableRow(KeyIdentifier.Date),
                LayoutKey = GetTableRow(KeyIdentifier.Layout),
                TagsKey = GetTableRow(KeyIdentifier.Tags),
                PermalinkKey = GetTableRow(KeyIdentifier.Permalink),
                ParentIdKey = GetTableRow(KeyIdentifier.ParentId)
            };

            set
            {
                _keyRowMap = new Dictionary<KeyIdentifier, DataGridViewRow>();
                dataGridView.Rows.Clear();

                AddTableRow(KeyIdentifier.Id, Res.Get(StringId.SSGFrontMatterId), value.IdKey);
                AddTableRow(KeyIdentifier.Title, Res.Get(StringId.SSGFrontMatterTitle), value.TitleKey);
                AddTableRow(KeyIdentifier.Date, Res.Get(StringId.SSGFrontMatterDate), value.DateKey);
                AddTableRow(KeyIdentifier.Layout, Res.Get(StringId.SSGFrontMatterLayout), value.LayoutKey);
                AddTableRow(KeyIdentifier.Tags, Res.Get(StringId.SSGFrontMatterTags), value.TagsKey);
                AddTableRow(KeyIdentifier.Permalink, Res.Get(StringId.SSGFrontMatterPermalink), value.PermalinkKey);
                AddTableRow(KeyIdentifier.ParentId, Res.Get(StringId.SSGFrontMatterParentId), value.ParentIdKey);

                // Recompute row sizes
                dataGridView?.AutoResizeRows();
            }
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.colProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.buttonResetDefaults = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProperty,
            this.colKey});
            this.dataGridView.Location = new System.Drawing.Point(12, 70);
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.Size = new System.Drawing.Size(350, 323);
            this.dataGridView.TabIndex = 2;
            // 
            // colProperty
            // 
            this.colProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.colProperty.DefaultCellStyle = dataGridViewCellStyle1;
            this.colProperty.HeaderText = "Property";
            this.colProperty.Name = "colProperty";
            this.colProperty.ReadOnly = true;
            this.colProperty.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colKey
            // 
            this.colKey.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colKey.HeaderText = "Front Matter Key";
            this.colKey.Name = "colKey";
            this.colKey.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.Location = new System.Drawing.Point(9, 31);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(353, 36);
            this.labelSubtitle.TabIndex = 1;
            this.labelSubtitle.Text = "Below you can adjust the post front matter keys used to match your static site ge" +
    "nerator.";
            // 
            // buttonResetDefaults
            // 
            this.buttonResetDefaults.Location = new System.Drawing.Point(253, 399);
            this.buttonResetDefaults.Name = "buttonResetDefaults";
            this.buttonResetDefaults.Size = new System.Drawing.Size(109, 23);
            this.buttonResetDefaults.TabIndex = 3;
            this.buttonResetDefaults.Text = "Reset to Defaults";
            this.buttonResetDefaults.UseVisualStyleBackColor = true;
            this.buttonResetDefaults.Click += new System.EventHandler(this.ButtonResetDefaults_Click);
            // 
            // FrontMatterPanel
            // 
            this.AccessibleName = "Front Matter";
            this.Controls.Add(this.buttonResetDefaults);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.dataGridView);
            this.Name = "FrontMatterPanel";
            this.PanelName = "Front Matter";
            this.Size = new System.Drawing.Size(370, 425);
            this.Controls.SetChildIndex(this.dataGridView, 0);
            this.Controls.SetChildIndex(this.labelSubtitle, 0);
            this.Controls.SetChildIndex(this.buttonResetDefaults, 0);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void ButtonResetDefaults_Click(object sender, EventArgs e)
            => Keys = new StaticSiteConfigFrontMatterKeys();
    }
}
