// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Tagging
{
    public class TagSmartContentEditor : SmartContentEditor
    {
        public TagSmartContentEditor(IProperties options, TagContentSource.CurrentBlogId getCurrentBlogId) : this()
        {
            _options = options;
            _getCurrentBlogId = getCurrentBlogId;
        }

        public TagSmartContentEditor()
        {
            InitializeComponent();

            this.header.HeaderText = Res.Get(StringId.Options);
            sidebarHeader.HeaderText = Res.Get(StringId.Tags);
            sidebarHeader.LinkText = "";
            sidebarHeader.LinkUrl = "";

            tagEditor.Changed += new EventHandler(TagEditor_Changed);
            tagEditor.SuppressMnemonics();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            tagEditor.NaturalizeLayout();
        }

        private TagContext _context;
        private SectionHeaderControl header;
        private IProperties _options;
        private SidebarHeaderControl sidebarHeader;
        private TagContentSource.CurrentBlogId _getCurrentBlogId;
        private string _providerName = string.Empty;

        private void InitializeComponent()
        {
            this.header = new OpenLiveWriter.ApplicationFramework.SectionHeaderControl();
            this.sidebarHeader = new OpenLiveWriter.ApplicationFramework.SidebarHeaderControl();
            this.tagEditor = new OpenLiveWriter.PostEditor.Tagging.TagEditor();
            this.SuspendLayout();
            //
            // header
            //
            this.header.AccessibleName = "Options";
            this.header.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.header.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.header.HeaderText = "Options";
            this.header.Location = new System.Drawing.Point(6, 88);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(184, 14);
            this.header.TabIndex = 4;
            this.header.TabStop = false;
            //
            // sidebarHeader
            //
            this.sidebarHeader.AccessibleName = "SidebarHeader";
            this.sidebarHeader.Location = new System.Drawing.Point(10, 2);
            this.sidebarHeader.Name = "sidebarHeader";
            this.sidebarHeader.Size = new System.Drawing.Size(184, 89);
            this.sidebarHeader.TabIndex = 5;
            this.sidebarHeader.TabStop = false;
            //
            // tagEditor
            //
            this.tagEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tagEditor.Location = new System.Drawing.Point(10, 112);
            this.tagEditor.Name = "tagEditor";
            this.tagEditor.Size = new System.Drawing.Size(184, 160);
            this.tagEditor.TabIndex = 0;
            this.tagEditor.TagProvider = null;
            this.tagEditor.Tags = new string[0];
            //
            // TagSmartContentEditor
            //
            this.Controls.Add(this.tagEditor);
            this.Controls.Add(this.header);
            this.Controls.Add(this.sidebarHeader);
            this.Name = "TagSmartContentEditor";
            this.ResumeLayout(false);

        }

        private TagEditor tagEditor;

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!Visible)
                _context.AddTagsToHistory(tagEditor.Tags);
            base.OnVisibleChanged(e);
            LayoutHelper.FitControlsBelow(0, sidebarHeader);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            VirtualTransparency.VirtualPaint(this, pevent);
        }

        private void TagEditor_Changed(object sender, EventArgs e)
        {
            _context.Tags = tagEditor.Tags;
            _context.CurrentProvider = tagEditor.TagProvider;

            _providerName = tagEditor.TagProvider.Name;
            if (_providerName != null)
                sidebarHeader.LinkText = _providerName;
            Invalidate();
            OnContentEdited();
            RefreshLayout();

        }

        private void RefreshLayout()
        {
            sidebarHeader.RefreshLayout();
            LayoutHelper.FitControlsBelow(0, sidebarHeader);
        }

        public override ISmartContent SelectedContent
        {
            get { return base.SelectedContent; }
            set
            {
                tagEditor.Changed -= new EventHandler(TagEditor_Changed);
                try
                {
                    base.SelectedContent = value;
                    _context = new TagContext(value, _options, _getCurrentBlogId());
                    tagEditor.SetTagProviders(_context);
                    tagEditor.Tags = _context.Tags;
                    tagEditor.PreviouslyUsedTags = _context.PreviouslyUsedTags;
                    tagEditor.TagProvider = _context.CurrentProvider;
                    _providerName = _context.CurrentProvider.Name;
                    if (_providerName != null)
                        sidebarHeader.LinkText = _providerName;
                    Invalidate();
                    RefreshLayout();
                }
                finally
                {
                    tagEditor.Changed += new EventHandler(TagEditor_Changed);
                }
            }
        }
    }
}
