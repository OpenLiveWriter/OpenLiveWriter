// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for FileItemFromFile.
    /// </summary>
    public class FileItemFromFileInfo : FileItem
    {
        public FileItemFromFileInfo(FileInfo fileInfo) : base(fileInfo.Name)
        {
            m_fileInfo = fileInfo;
        }

        private FileInfo m_fileInfo;

        public override FileItem[] Children
        {
            get
            {
                if (IsDirectory)
                    return CreateFileItemsFromFiles(Directory.GetFileSystemEntries(m_fileInfo.FullName));
                else
                    return base.Children;
            }
        }
        private static FileItem[] CreateFileItemsFromFiles(string[] paths)
        {
            ArrayList fileItems = new ArrayList();
            foreach (string path in paths)
                fileItems.Add(new FileItemFromFileInfo(new FileInfo(path)));
            return (FileItem[])fileItems.ToArray(typeof(FileItem));
        }

        public override string ContentsPath
        {
            get
            {
                return m_fileInfo.FullName;
            }
        }

        public override bool IsDirectory
        {
            get
            {
                return PathHelper.IsDirectory(m_fileInfo.FullName);
            }
        }
    }
}
