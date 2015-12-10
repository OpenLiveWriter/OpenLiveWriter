// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for TransientDirectory.
    /// </summary>
    public class TransientDirectory : TransientFileSystemItem
    {
        private string name;
        private TransientFileSystemItem[] children;

        public TransientDirectory(string name, params TransientFileSystemItem[] children)
        {
            this.name = name;
            this.children = children;
        }

        #region TransientFileSystemItem Members

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
            inputLen = inputLen - name.Length - 1;

            int maxEls = 0;
            foreach (TransientFileSystemItem child in children)
                maxEls = Math.Max(maxEls, child.MaxElementsOfPathLongerThan(inputLen));

            // if we've already hit the limit by reaching here,
            // or if one of our child paths hit the limit (and
            // thus caused maxEls to be >0), return maxEls + 1
            // (the +1 is to count this directory).
            if (inputLen < 0 || maxEls > 0)
                return (maxEls + 1);
            else
                return -1;
        }

        /*
        public int MaxChildPath(StringBuilder buffer)
        {
            buffer.Append(Path.DirectorySeparatorChar);
            buffer.Append(name);

            int startPos = buffer.Length;
            int currLen = 0;
            int depth = 0;

            StringBuilder maxChildPath = new StringBuilder();
            StringBuilder currChildPath = new StringBuilder();
            foreach (TransientFileSystemItem child in children)
            {
                int tmpDepth = child.MaxChildPath(currChildPath);
                if (currChildPath.Length > currLen && tmpDepth > depth)
                {
                    if (currLen != 0)
                    {
                        buffer.Remove(startPos, currLen);
                    }
                    buffer.Append(currChildPath.ToString());
                    currLen = currChildPath.Length;
                    depth = tmpDepth;
                }
                else
                {
                    currChildPath.Remove(0, currChildPath.Length);
                }
                Debug.Assert(currChildPath.Length == 0, "Programming error: currChildPath != 0");
            }
            if (maxChildPath.Length > 0)
            {
                buffer.Append(maxChildPath.ToString());
            }

            return ++depth;
        }
        */

        public FileSystemInfo Create(DirectoryInfo destination)
        {
            // TODO use constant instead of literal 260
            int charsLeft = 247 - destination.FullName.Length;
            int maxElements = MaxElementsOfPathLongerThan(charsLeft);

            if (charsLeft <= 0)
                throw new PathTooLongException();

            string truncatedName;
            if (maxElements > 0)
            {
                int charsForMe;
                if (maxElements > 1)
                    charsForMe = Math.Max(1, (int)Math.Floor(((double)(charsLeft - 6) / (maxElements - 1))));
                else
                    charsForMe = Math.Max(1, (int)Math.Floor(((double)(charsLeft - 1) / maxElements)));

                if (charsForMe >= name.Length)
                    truncatedName = name;
                else
                    truncatedName = name.Substring(0, charsForMe);
            }
            else
            {
                truncatedName = name;
            }

            string fullPath = Path.Combine(destination.FullName, truncatedName);
            Debug.WriteLine("TransientDirectory: Want to create " + fullPath);
            string createdPath = TempFileManager.CreateNewFile(destination.FullName, truncatedName, true);

            DirectoryInfo dirInfo = new DirectoryInfo(createdPath);
            if (children != null)
            {
                foreach (TransientFileSystemItem item in children)
                {
                    if (item != null)
                        item.Create(dirInfo);
                }
            }
            return dirInfo;
        }

        #endregion
    }
}
