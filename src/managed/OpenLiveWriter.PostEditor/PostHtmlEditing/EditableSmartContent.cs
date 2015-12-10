// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.Video;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    class EditableSmartContent : ISmartContent, IDisposable, IInternalContent
    {
        private ISmartContent _smartContent;
        private string _contentSourceId;
        private String _smartContentId;
        private IContentSourceSidebarContext _contentSourceContext;
        private IHTMLElement _smartContentElement;
        private SmartContentSource _contentSource;
        private EditableRootProperties _properties;
        private EditableLayoutStyle _layout;
        EditableSupportingFiles _supportingFiles;
        private IExtensionData _extensionData;
        public EditableSmartContent(IContentSourceSidebarContext context, SmartContentSource contentSource, IHTMLElement smartContentElement)
        {
            GC.SuppressFinalize(this);
            _contentSourceContext = context;
            _contentSource = contentSource;
            _smartContentElement = smartContentElement;

            ContentSourceManager.ParseContainingElementId(_smartContentElement.id, out _contentSourceId, out _smartContentId);

            _smartContent = _contentSourceContext.FindSmartContent(_smartContentId);
            _extensionData = _contentSourceContext.FindExtentsionData(_smartContentId);

            _properties = new EditableRootProperties();
            _layout = new EditableLayoutStyle();
            _supportingFiles = new EditableSupportingFiles();
            MakeSmartContentEditable();
        }

        ~EditableSmartContent()
        {
            Debug.Fail("EditableSmartContent was not disposed");
        }

        public string Id
        {
            get
            {
                return _smartContentId;
            }
        }

        public DateTime? RefreshCallback
        {
            get
            {
                return _extensionData.RefreshCallBack;
            }
            set
            {
                _extensionData.RefreshCallBack = value;
            }
        }

        public Object ObjectState
        {
            get
            {
                return _extensionData.ObjectState;
            }
            set
            {
                _extensionData.ObjectState = value;
            }
        }

        public void SaveEditedSmartContent()
        {
            //update the element ID with the new smart content id (this preserves undo-ability)
            _smartContentElement.id = ContentSourceManager.MakeContainingElementId(_contentSourceId, _smartContentId);
            SmartContentInsertionHelper.InsertEditorHtmlIntoElement(_contentSourceContext, _contentSource, _smartContent, _smartContentElement);

            //reinit the smart content so it is re-cloned
            MakeSmartContentEditable();
        }

        private void MakeSmartContentEditable()
        {
            String newContentId = Guid.NewGuid().ToString();
            _smartContent = _contentSourceContext.CloneSmartContent(_smartContentId, newContentId);
            _smartContentId = newContentId;
            Debug.Assert(_smartContent != null);
            _properties.SmartContentRaw = _smartContent;
            _supportingFiles.SmartContentRaw = _smartContent;
            _layout.SmartContentRaw = _smartContent;
        }

        public void Dispose()
        {
            //delete the current smart content since no edits were ever made that required saving it.
            _contentSourceContext.RemoveSmartContent(_smartContentId);
        }

        public IProperties Properties
        {
            get { return _properties; }
        }

        public ISupportingFiles Files
        {
            get { return _smartContent.Files; }
        }

        public ILayoutStyle Layout
        {
            get { return _smartContent.Layout; }
        }

        class EditableSupportingFiles : ISupportingFiles
        {
            private ISmartContent _smartContentRaw;
            public ISmartContent SmartContentRaw
            {
                get
                {
                    Debug.Assert(_smartContentRaw != null);
                    return _smartContentRaw;
                }
                set { _smartContentRaw = value; }
            }

            private ISupportingFiles SupportingFilesRaw
            {
                get { return SmartContentRaw.Files; }
            }

            public Stream Open(string fileName)
            {
                return SupportingFilesRaw.Open(fileName);
            }

            public Stream Open(string fileName, bool create)
            {
                return SupportingFilesRaw.Open(fileName, create);
            }

            public void AddFile(string fileName, string sourceFilePath)
            {
                Add(fileName, sourceFilePath);
            }

            public void Add(string fileName, string sourceFilePath)
            {
                SupportingFilesRaw.Add(fileName, sourceFilePath);
            }

            public void AddImage(string fileName, Image image, ImageFormat imageFormat)
            {
                SupportingFilesRaw.AddImage(fileName, image, imageFormat);
            }

            public void Remove(string fileName)
            {
                SupportingFilesRaw.Remove(fileName);
            }

            public void RemoveAll()
            {
                SupportingFilesRaw.RemoveAll();
            }

            public Uri GetUri(string fileName)
            {
                return SupportingFilesRaw.GetUri(fileName);
            }

            public bool Contains(string fileName)
            {
                return SupportingFilesRaw.Contains(fileName);
            }

            public string[] Filenames
            {
                get { return SupportingFilesRaw.Filenames; }
            }
        }

        class EditableLayoutStyle : ILayoutStyle
        {
            private ISmartContent _smartContentRaw;
            public ISmartContent SmartContentRaw
            {
                get
                {
                    Debug.Assert(_smartContentRaw != null);
                    return _smartContentRaw;
                }
                set { _smartContentRaw = value; }
            }

            public ILayoutStyle LayoutStyleRaw
            {
                get
                {

                    return SmartContentRaw.Layout;
                }
            }

            public Alignment Alignment
            {
                get { return LayoutStyleRaw.Alignment; }
                set { LayoutStyleRaw.Alignment = value; }
            }

            public int TopMargin
            {
                get { return LayoutStyleRaw.TopMargin; }
                set { LayoutStyleRaw.TopMargin = value; }
            }

            public int RightMargin
            {
                get { return LayoutStyleRaw.RightMargin; }
                set { LayoutStyleRaw.RightMargin = value; }
            }

            public int BottomMargin
            {
                get { return LayoutStyleRaw.BottomMargin; }
                set { LayoutStyleRaw.BottomMargin = value; }
            }

            public int LeftMargin
            {
                get { return LayoutStyleRaw.LeftMargin; }
                set { LayoutStyleRaw.LeftMargin = value; }
            }

        }

        class EditableRootProperties : EditableProperties
        {
            private ISmartContent _smartContentRaw;
            public ISmartContent SmartContentRaw
            {
                get
                {
                    Debug.Assert(_smartContentRaw != null);
                    return _smartContentRaw;
                }
                set { _smartContentRaw = value; }
            }

            public override IProperties CurrProperties
            {
                get { return SmartContentRaw.Properties; }
            }

            public override IProperties GetSubProperties(string name)
            {
                return new EditableSubProperties(name, this);
            }
        }

        class EditableSubProperties : EditableProperties
        {
            String _name;
            EditableProperties _parent;

            public EditableSubProperties(String name, EditableProperties parent)
            {
                _name = name;
                _parent = parent;
            }

            public override IProperties CurrProperties
            {
                get { return _parent.CurrProperties.GetSubProperties(_name); }
            }

            public override IProperties GetSubProperties(string name)
            {
                return new EditableSubProperties(name, this);
            }
        }

        abstract class EditableProperties : IProperties
        {
            public abstract IProperties CurrProperties { get; }

            public string GetString(string name, string defaultValue)
            {
                return CurrProperties.GetString(name, defaultValue);
            }

            public void SetString(string name, string value)
            {
                CurrProperties.SetString(name, value);
            }

            public int GetInt(string name, int defaultValue)
            {
                return CurrProperties.GetInt(name, defaultValue);
            }

            public void SetInt(string name, int value)
            {
                CurrProperties.SetInt(name, value);
            }

            public bool GetBoolean(string name, bool defaultValue)
            {
                return CurrProperties.GetBoolean(name, defaultValue);
            }

            public void SetBoolean(string name, bool value)
            {
                CurrProperties.SetBoolean(name, value);
            }

            public float GetFloat(string name, float defaultValue)
            {
                return CurrProperties.GetFloat(name, defaultValue);
            }

            public void SetFloat(string name, float value)
            {
                CurrProperties.SetFloat(name, value);
            }

            public decimal GetDecimal(string name, decimal defaultValue)
            {
                return CurrProperties.GetDecimal(name, defaultValue);
            }

            public void SetDecimal(string name, decimal value)
            {
                CurrProperties.SetDecimal(name, value);
            }

            public void Remove(string name)
            {
                CurrProperties.Remove(name);
            }

            public bool Contains(string name)
            {
                return CurrProperties.Contains(name);
            }

            public virtual IProperties GetSubProperties(string name)
            {
                return CurrProperties.GetSubProperties(name);
            }

            public virtual void RemoveSubProperties(string name)
            {
                CurrProperties.RemoveSubProperties(name);
            }

            public virtual bool ContainsSubProperties(string name)
            {
                return CurrProperties.ContainsSubProperties(name);
            }

            public void RemoveAll()
            {
                CurrProperties.RemoveAll();
            }

            public string this[string name]
            {
                get { return CurrProperties[name]; }
                set { CurrProperties[name] = value; }
            }

            public string[] Names
            {
                get { return CurrProperties.Names; }
            }

            public virtual string[] SubPropertyNames
            {
                get { return CurrProperties.SubPropertyNames; }
            }
        }
    }
}
