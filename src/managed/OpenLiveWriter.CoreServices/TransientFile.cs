// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A TransientFile represents a file without a location; it is composed
    /// entirely of filename and data.
    /// </summary>
    public class TransientFile : TransientFileSystemItem
    {
        private string name;
        private Stream data;

        public TransientFile(string name, Stream data, bool allowUnicodeName)
        {
            if (allowUnicodeName)
                this.name = FileHelper.GetValidFileName(name);
            else
                this.name = FileHelper.GetValidAnsiFileName(name);
            this.data = data;
        }

        public TransientFile(string name, byte[] data, bool allowUnicodeName) : this(name, new MemoryStream(data, false), allowUnicodeName)
        {
        }

        public TransientFile(FileInfo file, bool allowUnicodeName) : this(file.Name, new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), allowUnicodeName)
        {
        }

        public TransientFile(string name, FileInfo file, bool allowUnicodeName) : this(name, new FileStream(file.FullName, FileMode.Open, FileAccess.Read), allowUnicodeName)
        {
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public int MaxElementsOfPathLongerThan(int inputLen)
        {
            if (name.Length + 1 > inputLen)
                return 1;
            else
                return -1;
        }

        public FileSystemInfo Create(DirectoryInfo destination)
        {
            string truncatedName;

            // TODO: use constant instead of literal 260
            int charsLeft = 247 - destination.FullName.Length;
            if (charsLeft < name.Length + 1)
            {
                truncatedName = Path.GetExtension(name);

                if (truncatedName == null)
                    truncatedName = string.Empty;

                int charsForName = charsLeft - truncatedName.Length/*extension*/ - 1/*path separator*/;
                if (charsForName < 1)
                    throw new PathTooLongException();
                truncatedName = Path.GetFileNameWithoutExtension(name).Substring(0, charsForName) + truncatedName;
            }
            else
            {
                truncatedName = name;
            }

            // create the file
            Debug.WriteLine("TransientFile: Want to create " + Path.Combine(destination.FullName, truncatedName));
            string newFile = TempFileManager.CreateNewFile(destination.FullName, truncatedName, false);

            lock (this)
            {
                using (data)
                {
                    using (FileStream fs = File.Create(newFile))
                    {
                        int b;
                        while ((b = data.ReadByte()) != -1)
                            fs.WriteByte((byte)b);
                    }
                }
            }

            return new FileInfo(newFile);
        }
    }
}
