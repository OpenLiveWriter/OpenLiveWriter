// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.SupportingFiles
{
    /// <summary>
    /// Manages the set of files attached to a post editor.
    /// </summary>
    internal class SupportingFileService : ISupportingFileService
    {
        private Hashtable _supportingFilesByUri;
        private Hashtable _supportingFilesById;
        private Hashtable _factoriesById;
        private Hashtable _fileNameUniqueTokenTable;
        private BlogPostSupportingFileStorage _fileStorage;
        public SupportingFileService(BlogPostSupportingFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
            _supportingFilesByUri = new Hashtable();
            _supportingFilesById = new Hashtable();
            _factoriesById = new Hashtable();
            _fileNameUniqueTokenTable = new Hashtable();
        }

        internal BlogPostSupportingFileStorage FileStorage
        {
            get { return _fileStorage; }
        }

        public ISupportingFile CreateSupportingFile(string fileName, string fileNameUniqueToken, Stream s)
        {
            string storagePath = _fileStorage.CreateFile(fileName);
            using (Stream outStream = new FileStream(storagePath, FileMode.OpenOrCreate, FileAccess.Write))
                StreamHelper.Transfer(s, outStream);

            SupportingFileFactory fileFactory = new SupportingFileFactory(this, Guid.NewGuid().ToString(), fileName, 1);
            _factoriesById[fileFactory.FileId] = fileFactory;
            ISupportingFile file = fileFactory.CreateSupportingFile(new Uri(UrlHelper.CreateUrlFromPath(storagePath)), fileName, fileNameUniqueToken, true);
            return file;
        }

        public ISupportingFile CreateSupportingFile(string fileName, Stream s)
        {
            return CreateSupportingFile(fileName, CreateUniqueFileNameToken(fileName), s);
        }

        public ISupportingFile AddLinkedSupportingFileReference(Uri sourceUri)
        {
            ISupportingFile file = GetFileByUri(sourceUri);
            if (file == null)
            {
                string fileName = Path.GetFileName(sourceUri.LocalPath);
                SupportingFileFactory fileFactory = new SupportingFileFactory(this, Guid.NewGuid().ToString(), fileName, 1);
                _factoriesById[fileFactory.FileId] = fileFactory;
                file = fileFactory.CreateSupportingFile(sourceUri, fileName, CreateUniqueFileNameToken(fileName + "_linked"), false);
            }
            return file;
        }

        internal ISupportingFile CreateSupportingFileFromStoragePath(string fileName, string storagePath)
        {
            SupportingFileFactory fileFactory = new SupportingFileFactory(this, Guid.NewGuid().ToString(), fileName, 1);
            _factoriesById[fileFactory.FileId] = fileFactory;
            ISupportingFile file = fileFactory.CreateSupportingFile(new Uri(UrlHelper.CreateUrlFromPath(storagePath)), fileName, CreateUniqueFileNameToken(fileName), true);
            return file;
        }

        internal SupportingFileFactory CreateSupportingFile(string id, string fileName, int nextVersion)
        {
            SupportingFileFactory fileFactory = (SupportingFileFactory)_factoriesById[id];
            if (fileFactory == null)
            {
                fileFactory = new SupportingFileFactory(this, id, fileName, nextVersion);
                _factoriesById[fileFactory.FileId] = fileFactory;
            }
            else
            {
                throw new Exception("File already exists");
            }

            return fileFactory;
        }

        internal SupportingFileFactory GetFileFactory(string fileId)
        {
            return (SupportingFileFactory)_factoriesById[fileId];
        }

        internal string[] GetFileFactoryIds()
        {
            ArrayList list = new ArrayList();
            foreach (string fileId in _factoriesById.Keys)
                list.Add(fileId);
            return (string[])list.ToArray(typeof(string));
        }

        internal ISupportingFile[] GetAllSupportingFiles()
        {
            ArrayList list = new ArrayList();
            foreach (ISupportingFile file in _supportingFilesByUri.Values)
                list.Add(file);
            return (ISupportingFile[])list.ToArray(typeof(ISupportingFile));
        }

        public ISupportingFile GetFileByUri(Uri uri)
        {
            return (ISupportingFile)_supportingFilesByUri[uri];
        }

        public ISupportingFile GetFileById(string id)
        {
            return (ISupportingFile)_supportingFilesById[id];
        }

        internal ISupportingFile RegisterSupportingFile(ISupportingFile file)
        {
            Debug.Assert(!_supportingFilesByUri.ContainsKey(file.FileUri), "File already exists.");
            Debug.Assert(!_supportingFilesById.ContainsKey(file.FileId), "File already exists.");
            _supportingFilesByUri[file.FileUri] = file;
            _supportingFilesById[file.FileId] = file;
            RegisterUniqueFileNameToken(file.FileName, file.FileNameUniqueToken);

            OnFileAdded(file);
            return file;
        }

        internal void DeregisterSupportingFile(ISupportingFile file)
        {
            _supportingFilesByUri.Remove(file.FileUri);
            _supportingFilesById.Remove(file.FileId);

            OnFileRemoved(file);
        }

        private void RegisterUniqueFileNameToken(string fileName, string uniqueToken)
        {
            string uniqueFileName = FormatUniqueFilename(fileName, uniqueToken);
            _fileNameUniqueTokenTable[uniqueFileName] = uniqueToken;
        }

        private string FormatUniqueFilename(string fileName, string uniqueToken)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileExtension = Path.GetExtension(fileName);
            string uniqueFileName = String.Format(CultureInfo.InvariantCulture, uniqueFileNameFormat, fileNameWithoutExtension, uniqueToken, fileExtension);
            return uniqueFileName;
        }
        string uniqueFileNameFormat = "{0}_{1}{2}";

        internal string CreateUniqueFileNameToken(string fileName)
        {
            int uniqueToken = 0;
            string uniqueFileName = FormatUniqueFilename(fileName, uniqueToken.ToString(CultureInfo.InvariantCulture));
            while (_fileNameUniqueTokenTable.ContainsKey(uniqueFileName))
            {
                uniqueToken++;
                uniqueFileName = FormatUniqueFilename(fileName, uniqueToken.ToString(CultureInfo.InvariantCulture));
            }

            _fileNameUniqueTokenTable[uniqueFileName] = uniqueToken;
            return uniqueToken.ToString(CultureInfo.InvariantCulture);
        }

        #region Events
        public delegate void AttachedFileHandler(ISupportingFile file);
        public event AttachedFileHandler FileAdded;
        protected virtual void OnFileAdded(ISupportingFile file)
        {
            if (FileAdded != null)
                FileAdded(file);
        }

        public event AttachedFileHandler FileRemoved;
        protected virtual void OnFileRemoved(ISupportingFile file)
        {
            if (FileRemoved != null)
                FileRemoved(file);
        }

        public event AttachedFileHandler FileChanged;
        protected virtual void OnFileChanged(ISupportingFile file)
        {
            if (FileChanged != null)
                FileChanged(file);
        }
        internal void NotifyFileChanged(ISupportingFile file)
        {
            OnFileChanged(file);
        }
        #endregion
    }
}
