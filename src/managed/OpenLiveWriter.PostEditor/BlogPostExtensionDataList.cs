// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.SupportingFiles;
using System.Collections.Generic;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Manages plugin data associated with a blog post.
    /// </summary>
    public class BlogPostExtensionDataList
    {
        private Hashtable _extensionData;
        private ISupportingFileService _fileService;
        internal BlogPostExtensionDataList(ISupportingFileService fileService)
        {
            _extensionData = new Hashtable();
            _fileService = fileService;
        }

        public IExtensionData CreateExtensionData(string id)
        {
            BlogPostExtensionData exData = new BlogPostExtensionData(id, new BlogPostSettingsBag(), _fileService, new Hashtable());
            exData.RefreshCallBackChanged += new EventHandler(exData_RefreshCallBackChanged);
            _extensionData[exData.Id] = exData;
            return exData;
        }

        void exData_RefreshCallBackChanged(object sender, EventArgs e)
        {
            if (RefreshableCallbackTriggered != null)
                RefreshableCallbackTriggered(this, EventArgs.Empty);
        }

        public IExtensionData CloneExtensionData(string id, string newId)
        {
            BlogPostExtensionData exData = (BlogPostExtensionData)GetExtensionData(id);
            if (exData == null)
                return null;
            BlogPostExtensionData exData2 = (BlogPostExtensionData)exData.Clone();
            exData2.Id = newId;
            _extensionData[newId] = exData2;
            exData2.RefreshCallBackChanged += new EventHandler(exData_RefreshCallBackChanged);
            return exData2;
        }

        public void RemoveExtensionData(string id)
        {
            _extensionData.Remove(id);
        }

        public void Clear()
        {
            _extensionData.Clear();
        }

        public IExtensionData GetExtensionData(string id)
        {
            return (IExtensionData)_extensionData[id];
        }

        public IExtensionData[] CalculateReferencedExtensionData(string content)
        {
            Hashtable datas = new Hashtable();
            ContentSourceManager.SmartContentPredicate predicate = new ContentSourceManager.SmartContentPredicate();
            SimpleHtmlParser p = new SimpleHtmlParser(content);
            for (Element el; null != (el = p.Next());)
            {
                if (predicate.IsMatch(el))
                {
                    BeginTag bt = el as BeginTag;
                    Attr idAttr = bt.GetAttribute("id");
                    if (idAttr != null) //Synchronized WP posts will strip ID attrs (bug 488143)
                    {
                        string smartContentSourceId;
                        string smartContentId;
                        string smartContentElementId = idAttr.Value;
                        ContentSourceManager.ParseContainingElementId(smartContentElementId, out smartContentSourceId, out smartContentId);
                        IExtensionData data = GetExtensionData(smartContentId);
                        if (data != null)
                            datas[smartContentId] = data;
                    }
                }
            }
            return (IExtensionData[])ArrayHelper.CollectionToArray(datas.Values, typeof(IExtensionData));
        }

        public void RemoveOrphanedExtensionData(string[] validExtensionIds)
        {
            //create a lookup table for all valid Ids.
            Hashtable validIds = new Hashtable();
            foreach (string validId in validExtensionIds)
                validIds[validId] = validId;

            //mark any ids not in the valid list for removal
            ArrayList removeIds = new ArrayList();
            foreach (string id in _extensionData.Keys)
            {
                if (!validIds.ContainsKey(id))
                    removeIds.Add(id);
            }

            //remove any IDs marked for removal
            foreach (string removeId in removeIds)
                RemoveExtensionData(removeId);
        }

        public IExtensionData GetOrCreateExtensionData(string id)
        {
            return GetExtensionData(id) ?? CreateExtensionData(id);
        }

        public int Count
        {
            get
            {
                return _extensionData.Count;
            }
        }

        public event EventHandler RefreshableCallbackTriggered;

        internal IExtensionData GetExtensionDataWithMinCallback()
        {
            DateTime minCallback = DateTime.MaxValue;
            IExtensionData minCallBackExtensionData = null;

            foreach (IExtensionData extensionData in _extensionData.Values)
            {
                if (extensionData.RefreshCallBack < minCallback)
                {
                    minCallback = (DateTime)extensionData.RefreshCallBack;
                    minCallBackExtensionData = extensionData;
                }
            }

            return minCallBackExtensionData;
        }
    }

    public interface IExtensionData
    {
        BlogPostSettingsBag Settings { get; }
        string Id { get; }
        string AddFile(string fileName, Stream s);
        Uri GetFileUri(string fileId);
        Stream GetFileStream(string fileId);
        Object ObjectState { get; set; }
        void RemoveFile(string fileId);
        DateTime? RefreshCallBack { get; set; }
        event EventHandler RefreshCallBackChanged;
        string[] FileIds { get; }
    }

    class BlogPostExtensionData : IExtensionData, ICloneable
    {
        private BlogPostSettingsBag _settings;
        private string _id;
        private ISupportingFileService _fileService;
        private DateTime? _refreshCallBack;
        private Object _objectContext;

        Hashtable _fileIds;

        internal BlogPostExtensionData(string id, BlogPostSettingsBag settings, ISupportingFileService fileService, Hashtable fileMappings)
        {
            _id = id;
            _settings = settings;
            _fileService = fileService;

            _fileIds = fileMappings;
            _refreshCallBack = null;
            _objectContext = null;
        }

        public event EventHandler RefreshCallBackChanged;
        public DateTime? RefreshCallBack
        {
            get
            {
                return _refreshCallBack;
            }
            set
            {
                _refreshCallBack = value;

                // If the call back is set or even cleared on a extension data
                // object we fire the event which the RefreshableContentManager
                // will listen for and change the intveral on its timer accordingly
                if (RefreshCallBackChanged != null)
                    RefreshCallBackChanged(this, EventArgs.Empty);
            }
        }

        public object ObjectState
        {
            get
            {
                return _objectContext;
            }
            set
            {
                _objectContext = value;
            }
        }

        public BlogPostSettingsBag Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string AddFile(string fileName, Stream s)
        {
            ISupportingFile file = GetSupportingFile(fileName);
            if (file == null)
            {
                file = _fileService.CreateSupportingFile(fileName, s);
            }
            else
            {
                file = file.UpdateFile(s);
            }
            _fileIds[fileName] = file;

            return file.FileId;
        }

        internal string AddStorageFileMapping(string fileName, ISupportingFile file)
        {
            _fileIds[fileName] = file;
            return fileName;
        }

        public Uri GetFileUri(string fileName)
        {
            ISupportingFile file = GetSupportingFile(fileName);
            if (file != null)
                return file.FileUri;
            else
                return null;
        }

        public ISupportingFile GetSupportingFile(string fileName)
        {
            return (ISupportingFile)_fileIds[fileName];
        }

        public Stream GetFileStream(string fileName)
        {
            ISupportingFile file = GetSupportingFile(fileName);
            if (file != null)
            {
                return new FileStream(file.FileUri.LocalPath, FileMode.Open);
            }
            else
                return null;
        }

        public void RemoveFile(string fileName)
        {
            //remove the reference to the file, but keep supporting file alive (for undo/redo)
            _fileIds.Remove(fileName);
        }

        public string[] FileIds
        {
            get
            {
                string[] fileIds = new string[_fileIds.Count];
                int index = 0;
                foreach (string fileId in _fileIds.Keys)
                    fileIds[index++] = fileId;
                return fileIds;
            }
        }
        #region ICloneable Members

        public object Clone()
        {
            BlogPostExtensionData exdata =
                new BlogPostExtensionData(Guid.NewGuid().ToString(), (BlogPostSettingsBag)_settings.Clone(), _fileService, (Hashtable)_fileIds.Clone());
            exdata.RefreshCallBack = RefreshCallBack;
            exdata.ObjectState = ObjectState;
            return exdata;
        }

        #endregion

    }
}

