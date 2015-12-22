// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using OpenLiveWriter.HtmlParser.Parser;
//using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor
{
    public class BlogPostSupportingFileStorage
    {
        public BlogPostSupportingFileStorage()
        {
            _storageDirectory = new DirectoryInfo(TempFileManager.Instance.CreateTempDir("supfiles" + Environment.TickCount.ToString("X", CultureInfo.InvariantCulture)));
            _storageDirectory.Create();
        }

        /// <summary>
        /// Path to supporting file storage
        /// </summary>
        public string StoragePath
        {
            get { return _storageDirectory.FullName; }
        }

        /// <summary>
        /// add a file to the list (managing filename uniqueness) and return a file
        /// URI to the full path of the added file
        /// </summary>
        /// <param name="requestedFileName"></param>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        public string AddFile(string requestedFileName, Stream fileContents)
        {
            string fileName = TempFileManager.CreateNewFile(_storageDirectory.FullName, CleanPathForServer(requestedFileName), false);

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                StreamHelper.Transfer(fileContents, fileStream);

            return UrlHelper.GetLocalFileUrl(fileName);
        }

        /// <summary>
        /// Create a unique file placeholder.
        /// </summary>
        /// <param name="requestedFileName"></param>
        /// <returns></returns>
        public string CreateFile(string requestedFileName)
        {
            if (!FileHelper.IsValidFileName(requestedFileName))
                requestedFileName = FileHelper.GetPrettyFileName(requestedFileName);

            string fileName = TempFileManager.CreateNewFile(_storageDirectory.FullName, requestedFileName, false);
            return fileName;
        }

        public bool ContainsFile(string filePath)
        {
            // get cannonical path names
            string normalizedLocalFilePath = Kernel32.GetLongPathName(filePath);
            if (normalizedLocalFilePath == null) // if the file doesn't exist this will be the case
                return false;
            else
                normalizedLocalFilePath = normalizedLocalFilePath.ToLower(CultureInfo.CurrentCulture);

            string normalizedSupportingFilesPath = Kernel32.GetLongPathName(StoragePath).ToLower(CultureInfo.CurrentCulture);

            // do the comparison
            return normalizedLocalFilePath.StartsWith(normalizedSupportingFilesPath);
        }

        public string[] GetSupportingFilesInPost(string postContents)
        {
            _supportingFileScratchList.Clear();
            HtmlReferenceFixer.FixLocalFileReferences(postContents, new ReferenceFixer(EnumerateLocalFileReference));
            return (string[])_supportingFileScratchList.ToArray(typeof(string));
        }
        private string EnumerateLocalFileReference(BeginTag tag, string reference)
        {
            // check if the local file is contained in the supporting file directory
            string localFilePath = new Uri(reference).LocalPath;
            if (this.ContainsFile(localFilePath))
                _supportingFileScratchList.Add(localFilePath);

            // don't transform the url
            return reference;
        }
        private ArrayList _supportingFileScratchList = new ArrayList();

        public string CleanPathForServer(string path)
        {
            // make ftp friendly, trim if > 40, replace spaces
            string cleanPath = FileHelper.GetValidAnsiFileName(path, 40);
            // Fix bug 581102: Post title with leading dot causes FTP upload error
            if (cleanPath.StartsWith("."))
                cleanPath = FileHelper.GetValidAnsiFileName(cleanPath.Trim('.'), 40);
            cleanPath = cleanPath.Replace(' ', '_');
            return cleanPath;
        }

        public string SpellingContextDirectory
        {
            get
            {
                if (_spellingContextDirectory == null)
                {
                    _spellingContextDirectory = Path.Combine(StoragePath, SPELLING_CONTEXT);
                    if (!Directory.Exists(_spellingContextDirectory))
                        Directory.CreateDirectory(_spellingContextDirectory);
                }
                return _spellingContextDirectory;
            }
        }
        private string _spellingContextDirectory;
        private const string SPELLING_CONTEXT = "SpellingContext";

        public void WriteSpellingContextDictionary(Stream dictionaryContents)
        {
            string contextDictionaryPath = Path.Combine(SpellingContextDirectory, CONTEXT_DICTIONARY_FILE);
            using (FileStream dictionaryFile = new FileStream(contextDictionaryPath, FileMode.Create))
            {
                StreamHelper.Transfer(dictionaryContents, dictionaryFile);
            }
        }

        public Stream ReadSpellingContextDictionary()
        {
            string contextDictionaryPath = Path.Combine(SpellingContextDirectory, CONTEXT_DICTIONARY_FILE);
            if (File.Exists(contextDictionaryPath))
                return new FileStream(contextDictionaryPath, FileMode.Open);
            else
                return null;
        }

        //ToDo: OLW Spell Checker
        // NOTE: hardcode to sentry spelling engine -- need to unroll this if we switch engines
        private static readonly string CONTEXT_DICTIONARY_FILE = "context.tlx";

        private DirectoryInfo _storageDirectory;
    }

}
