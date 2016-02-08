// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// This class helps store an ordered list of files in a SmartContent object.
    /// The file contents can optionally be stored in the SmartContent.
    /// </summary>
    class SmartContentPersistedFileList
    {
        private readonly ISmartContent smartContent;
        private readonly string listId;

        private const char INTERNAL_EXTERNAL_SEPARATOR = '|';
        private const string FILE_SEPARATOR = "?";

        public SmartContentPersistedFileList(ISmartContent smartContent, string listId)
        {
            this.smartContent = smartContent;
            this.listId = listId;
        }

        public List<string> Files
        {
            get
            {
                LazyLoader<string> tempDir = new LazyLoader<string>(
                    delegate
                    {
                        return TempFileManager.Instance.CreateTempDir();
                    });

                List<string> results = new List<string>();

                foreach (PersistedFile pf in GetPersistedFileList())
                {
                    // If the file was not persisted in the wpost, or the
                    // file still exists at the original path, use that.

                    if (File.Exists(pf.Path))
                    {
                        results.Add(pf.Path);
                    }
                    else if (pf.PersistToPostFile)
                    {
                        using (Stream inStream = smartContent.Files.Open(pf.SmartContentName, false))
                        {
                            Trace.WriteLineIf(inStream == null, string.Format(CultureInfo.InvariantCulture, "Failed to find smartcontent persisted file, {0}", pf.SmartContentName));
                            if (inStream != null)
                            {
                                string filePath = TempFileManager.CreateNewFile(tempDir, pf.SmartContentName, false);
                                using (Stream outStream = File.OpenWrite(filePath))
                                    StreamHelper.Transfer(inStream, outStream);
                                results.Add(filePath);
                            }
                        }
                    }
                }

                return results;
            }
        }

        public void ClearFiles()
        {
            foreach (PersistedFile pf in GetPersistedFileList())
            {
                if (pf.PersistToPostFile)
                    smartContent.Files.Remove(pf.SmartContentName);
            }
            smartContent.Properties.Remove(listId);
        }

        public void SetFiles(IEnumerable<PersistedFile> files)
        {
            StringBuilder result = new StringBuilder();

            ClearFiles();
            int i = 0;
            foreach (PersistedFile file in files)
            {
                if (file.PersistToPostFile)
                {
                    string name = (i++).ToString(CultureInfo.InvariantCulture) + Path.GetExtension(file.Path);
                    smartContent.Files.Add(name, file.Path);
                    result.AppendFormat(CultureInfo.InvariantCulture, "I" + INTERNAL_EXTERNAL_SEPARATOR + "{0}" + INTERNAL_EXTERNAL_SEPARATOR + "{1}" + FILE_SEPARATOR, name, file.Path);
                }
                else
                {
                    result.Append("E" + INTERNAL_EXTERNAL_SEPARATOR + file.Path + FILE_SEPARATOR);
                }
            }

            smartContent.Properties.SetString(listId, result.ToString());
        }

        private List<PersistedFile> GetPersistedFileList()
        {
            string str = smartContent.Properties.GetString(listId, null);
            if (string.IsNullOrEmpty(str))
                return new List<PersistedFile>();

            List<PersistedFile> results = new List<PersistedFile>();

            string[] files = StringHelper.Split(str, FILE_SEPARATOR);
            foreach (string file in files)
            {
                string[] chunks = file.Split(INTERNAL_EXTERNAL_SEPARATOR);
                switch (chunks[0])
                {
                    case "I":
                        PersistedFile persistedFile = new PersistedFile(true, chunks[2]);
                        persistedFile.SmartContentName = chunks[1];
                        results.Add(persistedFile);
                        break;
                    case "E":
                        results.Add(new PersistedFile(false, chunks[1]));
                        break;
                }
            }

            return results;
        }
    }

    internal class PersistedFile
    {
        /// <summary>
        /// If true, save the file to the smart content and
        /// recreate when necessary.
        /// </summary>
        public readonly bool PersistToPostFile;
        public readonly string Path;
        internal string SmartContentName;

        public PersistedFile(bool shouldPersistToPostFile, string path)
        {
            PersistToPostFile = shouldPersistToPostFile;
            Path = path;
        }
    }
}
