// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tagging
{
    public class TagEditor : UserControl
    {
        public TagEditor()
        {
            InitializeComponent();

            labelTechTags.Text = Res.Get(StringId.TagsTagsLabel);
            labelTag.Text = Res.Get(StringId.TagsTagProviderLabel);

            textBoxTags.AutoCompleteSeparator = "";
            SinkEvents();
        }

        public void SuppressMnemonics()
        {
            labelTag.Text = labelTag.Text.Replace("&", "");
            labelTechTags.Text = labelTechTags.Text.Replace("&", "");
        }

        public void NaturalizeLayout()
        {
            using (new AutoGrow(this, AnchorStyles.Bottom | AnchorStyles.Right, false))
            {
                LayoutHelper.NaturalizeHeight(labelTechTags);
                LayoutHelper.FitControlsBelow(2, labelTechTags);

                LayoutHelper.NaturalizeHeight(labelTag);
                LayoutHelper.FitControlsBelow(2, labelTag);

                labelTechTags.Width = Width;
                labelTag.Width = Width;
                labelTag.Left = labelTechTags.Left = 0;
            }
        }

        public string[] Tags
        {
            get
            {
                if (textBoxTags.Text != null && textBoxTags.Text != string.Empty)
                {
                    string normalizedList = ListHelper.NormalizeList(textBoxTags.Text.Trim(), Res.Comma + "");
                    return StringHelper.Split(normalizedList, Res.Comma + "");

                }
                else
                    return new string[0];
            }
            set
            {
                using (new SuppressChangeEvent(this))
                {
                    textBoxTags.Text = StringHelper.Join(value, Res.ListSeparator);
                }
            }
        }

        public TagProvider TagProvider
        {
            get { return tagProviderComboBox.SelectedTagProvider; }
            set
            {
                if (value != null)
                {
                    tagProviderComboBox.SelectedTagProvider = value;
                }
            }
        }

        public void SetTagProviders(TagContext context)
        {
            _context = context;
            tagProviderComboBox.SetTagProviders(new TagProviderManager(_context.PluginSettings).TagProviders);
        }
        private TagContext _context;

        public string[] PreviouslyUsedTags
        {
            set { textBoxTags.AutoCompleteWords = value; }
        }

        public event EventHandler Changed;

        protected void OnChanged()
        {
            if (Changed != null && !suppressChangeEvent)
                Changed(this, new EventArgs());
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // force the list that is displayed to be normalized
            using (new SuppressChangeEvent(this))
            {
                Tags = StringHelper.Split(ListHelper.NormalizeList(textBoxTags.Text.Trim(), Res.Comma + ""), Res.Comma + "");
            }
        }

        private Label labelTag;
        private TagProviderComboBox tagProviderComboBox;

        private class SuppressChangeEvent : IDisposable
        {
            public SuppressChangeEvent(TagEditor editor)
            {
                _editor = editor;
                _editor.suppressChangeEvent = true;
            }

            private TagEditor _editor;

            public void Dispose()
            {
                _editor.suppressChangeEvent = false;
            }
        }

        private bool suppressChangeEvent = false;

        protected override void Dispose(bool disposing)
        {
            UnSinkEvents();
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
            this.textBoxTags = new OpenLiveWriter.PostEditor.Tagging.TextBoxWithAutoComplete();
            this.labelTechTags = new System.Windows.Forms.Label();
            this.labelTag = new System.Windows.Forms.Label();
            this.tagProviderComboBox = new OpenLiveWriter.PostEditor.Tagging.TagProviderComboBox();
            this.SuspendLayout();
            //
            // textBoxTags
            //
            this.textBoxTags.AcceptsReturn = true;
            this.textBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTags.Location = new System.Drawing.Point(0, 16);
            this.textBoxTags.Multiline = true;
            this.textBoxTags.Name = "textBoxTags";
            this.textBoxTags.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTags.Size = new System.Drawing.Size(320, 140);
            this.textBoxTags.TabIndex = 5;
            //
            // labelTechTags
            //
            this.labelTechTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTechTags.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTechTags.Location = new System.Drawing.Point(0, 0);
            this.labelTechTags.Name = "labelTechTags";
            this.labelTechTags.Size = new System.Drawing.Size(319, 13);
            this.labelTechTags.TabIndex = 0;
            this.labelTechTags.Text = "&Tags (comma separated):";
            //
            // labelTag
            //
            this.labelTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTag.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTag.Location = new System.Drawing.Point(0, 164);
            this.labelTag.Name = "labelTag";
            this.labelTag.Size = new System.Drawing.Size(319, 13);
            this.labelTag.TabIndex = 10;
            this.labelTag.Text = "Tag &Provider:";
            //
            // tagProviderComboBox
            //
            this.tagProviderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tagProviderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagProviderComboBox.Location = new System.Drawing.Point(0, 180);
            this.tagProviderComboBox.Name = "tagProviderComboBox";
            this.tagProviderComboBox.SelectedTagProvider = null;
            this.tagProviderComboBox.Size = new System.Drawing.Size(320, 21);
            this.tagProviderComboBox.TabIndex = 15;
            //
            // TagEditor
            //
            this.Controls.Add(this.tagProviderComboBox);
            this.Controls.Add(this.labelTag);
            this.Controls.Add(this.labelTechTags);
            this.Controls.Add(this.textBoxTags);
            this.Name = "TagEditor";
            this.Size = new System.Drawing.Size(320, 208);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBoxWithAutoComplete textBoxTags;
        private Label labelTechTags;
        private Container components = null;

        private void textBoxTags_TextChanged(object sender, EventArgs e)
        {
            OnChanged();
        }

        private void tagProviderComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnChanged();
        }

        private void tagProviderComboBox_ManageProviders(object sender, EventArgs e)
        {
            TagOptions optionsForm = new TagOptions();
            TagProviderManager manager = new TagProviderManager(_context.PluginSettings);

            optionsForm.Initialize(manager);
            optionsForm.SetContext(_context);
            optionsForm.ShowDialog(this);

            tagProviderComboBox.SetTagProviders(manager.TagProviders);
            tagProviderComboBox.SelectedTagProvider = _context.CurrentProvider;
        }

        private void SinkEvents()
        {
            textBoxTags.TextChanged += new EventHandler(textBoxTags_TextChanged);
            tagProviderComboBox.SelectionChangeCommitted += new EventHandler(tagProviderComboBox_SelectionChangeCommitted);
            tagProviderComboBox.ManageProviders += new EventHandler(tagProviderComboBox_ManageProviders);
        }

        private void UnSinkEvents()
        {
            textBoxTags.TextChanged -= new EventHandler(textBoxTags_TextChanged);
            tagProviderComboBox.SelectionChangeCommitted -= new EventHandler(tagProviderComboBox_SelectionChangeCommitted);
            tagProviderComboBox.ManageProviders -= new EventHandler(tagProviderComboBox_ManageProviders);
        }
    }
}
