// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.PostEditor.Configuration;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageServiceSettingsEditor.
    /// </summary>
    public class BlogPostHtmlEditorSettingsEditor : IBlogSettingsEditor
    {
        public BlogPostHtmlEditorSettingsEditor()
        {
        }

        private BlogPostHtmlEditorSettingsEditorControl HtmlEditorSettingsControl
        {
            get
            {
                if(_htmlEditorSettingsControl == null)
                {
                    _htmlEditorSettingsControl = new BlogPostHtmlEditorSettingsEditorControl();
                    _htmlEditorSettingsControl.SettingsChanged += new EventHandler(_htmlEditorSettingsControl_SettingsChanged);
                }
                return _htmlEditorSettingsControl;
            }
        }
        private BlogPostHtmlEditorSettingsEditorControl _htmlEditorSettingsControl;

        public event EventHandler SettingsChanged;
        protected void OnSettingsChanged(EventArgs evt)
        {
            if(SettingsChanged != null)
            {
                SettingsChanged(this, evt);
            }
        }

        private BlogSettings _settings;
        bool _initing;
        public void Init(BlogSettings settings)
        {
            _initing = true;
            _settings = settings;
            HtmlEditorSettingsControl.LoadSettings(settings);
            _initing = false;
        }

        public Control EditorControl
        {
            get { return HtmlEditorSettingsControl; }
        }

        public void ApplySettings()
        {
            HtmlEditorSettingsControl.ApplySettings(_settings);
        }

        public string Title
        {
            get { return "Editor"; }
        }

        public virtual void Dispose()
        {
            if(_htmlEditorSettingsControl != null)
            {
                _htmlEditorSettingsControl.SettingsChanged -= new EventHandler(_htmlEditorSettingsControl_SettingsChanged);
                _htmlEditorSettingsControl.Dispose();
                _htmlEditorSettingsControl = null;
            }
            _settings = null;
        }

        private void _htmlEditorSettingsControl_SettingsChanged(object sender, EventArgs e)
        {
            if(!_initing)
                OnSettingsChanged(e);
        }
    }
}
