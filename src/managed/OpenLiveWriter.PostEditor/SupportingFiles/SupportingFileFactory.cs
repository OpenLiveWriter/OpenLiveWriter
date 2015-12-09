// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.SupportingFiles
{
    /// <summary>
    /// Factory for managing multiple versions of a supporting file.
    /// </summary>
    internal class SupportingFileFactory
    {
        private SupportingFileService _fileService;
        private Hashtable _uploadInfos = new Hashtable();
        private Hashtable _versionTable = new Hashtable();

        internal SupportingFileFactory(SupportingFileService supportingFileService, string fileId, string fileName, int nextVersion)
        {
            _fileService = supportingFileService;
            _fileId = fileId;
            _fileName = fileName;
            _nextVersion = nextVersion;
            _uploadInfos = new Hashtable();
        }

        public string FileId
        {
            get { return _fileId; }
        }
        private string _fileId;

        public string FileName
        {
            get { return _fileName; }
        }
        private string _fileName;

        public int NextVersion
        {
            get { return _nextVersion; }
        }
        public int _nextVersion = 1;

        internal void AddUploadInfo(string uploadContext, int uploadedFileVersion, Uri uploadUri, BlogPostSettingsBag settings)
        {
            SupportingFileUploadInfo uploadInfo = new SupportingFileUploadInfo(uploadedFileVersion, uploadUri, settings);
            _uploadInfos[uploadContext] = uploadInfo;
        }

        internal ISupportingFileUploadInfo GetUploadInfo(string uploadContext)
        {
            SupportingFileUploadInfo uploadInfo = (SupportingFileUploadInfo)_uploadInfos[uploadContext];
            if (uploadInfo == null)
            {
                uploadInfo = new SupportingFileUploadInfo(-1, null, new BlogPostSettingsBag());
                _uploadInfos[uploadContext] = uploadInfo;
            }
            return uploadInfo;
        }

        internal void MarkUploaded(string uploadContextId, int fileVersion, Uri uploadUri, BlogPostSettingsBag uploadSettings)
        {
            SupportingFileUploadInfo uploadInfo = (SupportingFileUploadInfo)_uploadInfos[uploadContextId];
            if (uploadInfo != null)
            {
                //update the existing info
                uploadInfo.UploadedFileVersion = fileVersion;
                uploadInfo.UploadUri = uploadUri;
                uploadInfo.UploadSettings = uploadSettings;
            }
            else
            {
                _uploadInfos[uploadContextId] = new SupportingFileUploadInfo(fileVersion, uploadUri, uploadSettings);
            }
        }

        internal string[] GetAllUploadContexts()
        {
            ArrayList list = new ArrayList();
            foreach (string uploadContext in _uploadInfos.Keys)
            {
                list.Add(uploadContext);
            }
            return (string[])list.ToArray(typeof(string));
        }

        internal ISupportingFile[] GetVersionedFiles()
        {
            ArrayList list = new ArrayList();
            foreach (ISupportingFile file in _versionTable.Values)
            {
                list.Add(file);
            }
            return (ISupportingFile[])list.ToArray(typeof(ISupportingFile));
        }

        internal ISupportingFile CreateSupportingFile(Uri uri, string fileName, string fileNameUniqueToken, bool embedded)
        {
            Debug.Assert(fileNameUniqueToken != null); //bulletproofing for old post files

            return
                RegisterSupportingFile(
                    new VersionedSupportingFile(this, _nextVersion++, uri, fileName, fileNameUniqueToken, embedded, new BlogPostSettingsBag()));
        }

        internal ISupportingFile CreateSupportingFile(Uri uri, int version, string fileName, string fileNameUniqueToken, bool embedded, BlogPostSettingsBag settings)
        {
            Debug.Assert(fileNameUniqueToken != null); //bulletproofing for old post files

            return RegisterSupportingFile(new VersionedSupportingFile(this, version, uri, fileName, fileNameUniqueToken, embedded, settings));
        }

        private ISupportingFile RegisterSupportingFile(VersionedSupportingFile file)
        {
            _versionTable[file.FileVersion] = file;
            _fileService.RegisterSupportingFile(file);
            return file;
        }

        internal ISupportingFile UpdateFile(VersionedSupportingFile file, Stream newContents, string fileName)
        {
            //update the file contents
            string newStoragePath = _fileService.FileStorage.CreateFile(fileName);
            using (Stream outStream = new FileStream(newStoragePath, FileMode.OpenOrCreate, FileAccess.Write))
                StreamHelper.Transfer(newContents, outStream);

            //if the filename is changing, assign the file a new unique filename token
            string fileNameUniqueToken = fileName == file.FileName
                                             ? file.FileNameUniqueToken
                                             : _fileService.CreateUniqueFileNameToken(fileName);

            //Clone the file so that its old version can still be referenced by its old URL.
            //Note: this is required since editor undo operations may allow the post to rollback
            //to referencing the old version of the file.
            VersionedSupportingFile newFile = new VersionedSupportingFile(this, _nextVersion++, new Uri(UrlHelper.CreateUrlFromPath(newStoragePath)), fileName, fileNameUniqueToken, true, (BlogPostSettingsBag)file.Settings.Clone()); ;
            newFile.StoragePath = newStoragePath;

            RegisterSupportingFile(newFile);
            return newFile;
        }

        private void Delete(VersionedSupportingFile file)
        {
            _fileService.DeregisterSupportingFile(file);
            _versionTable.Remove(file.FileVersion);
        }

        internal class VersionedSupportingFile : ISupportingFile
        {
            private SupportingFileFactory _fileFactory;
            internal VersionedSupportingFile(SupportingFileFactory fileFactory, int version, Uri fileUri, string fileName, string fileNameUniqueToken, bool embedded, BlogPostSettingsBag fileProperties)
            {
                _fileFactory = fileFactory;
                _fileVersion = version;
                _fileUri = fileUri;
                _fileName = fileName;
                _embedded = embedded;
                _properties = fileProperties;
                _fileNameUniqueToken = fileNameUniqueToken;
            }

            public SupportingFileFactory SupportingFileFactory
            {
                get { return _fileFactory; }
            }

            public string FileId
            {
                get { return _fileFactory.FileId + "@" + FileVersion; }
            }

            public bool Embedded
            {
                get { return _embedded; }
                set { _embedded = value; }
            }
            private bool _embedded;

            public string FileName
            {
                get { return _fileName; }
            }
            private string _fileName;

            public string FileNameUniqueToken
            {
                get { return _fileNameUniqueToken.ToString(); }
            }
            private string _fileNameUniqueToken;

            public string StoragePath
            {
                set { FileUri = new Uri(UrlHelper.CreateUrlFromPath(value)); }
            }

            public Uri FileUri
            {
                get { return _fileUri; }
                set { _fileUri = value; }
            }
            private Uri _fileUri;

            public ISupportingFile UpdateFile(Stream newContents)
            {
                return UpdateFile(newContents, FileName);
            }

            public ISupportingFile UpdateFile(Stream newContents, string fileName)
            {
                return _fileFactory.UpdateFile(this, newContents, fileName);
            }

            public int FileVersion
            {
                get { return _fileVersion; }
            }
            private int _fileVersion;

            public int NextFileVersion
            {
                get { return _fileFactory.NextVersion; }
            }

            public ISupportingFileUploadInfo GetUploadInfo(string uploadContextId)
            {
                return _fileFactory.GetUploadInfo(uploadContextId);
            }

            public bool IsUploaded(string uploadContextId)
            {
                ISupportingFileUploadInfo uploadInfo = GetUploadInfo(uploadContextId);
                return FileVersion == uploadInfo.UploadedFileVersion;
            }

            public void MarkUploaded(string uploadContextId, Uri uploadUri)
            {
                ISupportingFileUploadInfo uploadInfo = GetUploadInfo(uploadContextId);
                _fileFactory.MarkUploaded(uploadContextId, _fileVersion, uploadUri, uploadInfo.UploadSettings);
                _fileFactory._fileService.NotifyFileChanged(this);
            }

            internal string[] GetAllUploadContexts()
            {
                return _fileFactory.GetAllUploadContexts();
            }

            public BlogPostSettingsBag Settings
            {
                get { return _properties; }
            }
            private BlogPostSettingsBag _properties;

            public void Delete()
            {
                _fileFactory.Delete(this);
            }

            public override string ToString()
            {
                return FileId;
            }
        }

        private class SupportingFileUploadInfo : ISupportingFileUploadInfo
        {
            private BlogPostSettingsBag _uploadSettings;
            private Uri _uploadUri;
            private int _uploadedFileVersion;

            public SupportingFileUploadInfo(int uploadedFileVersion, Uri uploadUri, BlogPostSettingsBag settings)
            {
                _uploadUri = uploadUri;
                _uploadSettings = settings;
                _uploadedFileVersion = uploadedFileVersion;
            }

            public int UploadedFileVersion
            {
                get { return _uploadedFileVersion; }
                set { _uploadedFileVersion = value; }
            }

            public Uri UploadUri
            {
                get { return _uploadUri; }
                set { _uploadUri = value; }
            }

            public BlogPostSettingsBag UploadSettings
            {
                get { return _uploadSettings; }
                set { _uploadSettings = value; }
            }
        }
    }
}
