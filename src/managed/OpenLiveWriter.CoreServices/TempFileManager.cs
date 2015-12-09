// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using Timer = System.Timers.Timer;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Used to create temporary files and directories.
    /// </summary>
    /// <remarks>
    /// This class is used to create temporary files and directories in a way
    /// that makes them relatively cheap to delete with age.
    /// </remarks>
    public class TempFileManager
    {

        /// <summary>
        /// If no pattern is given at all for the creation of a file,
        /// this pattern will be used.
        /// </summary>
        private const string TEMP_FILE_DEFAULT_PATTERN = "p31{0}.tmp";

        /// <summary>
        /// If no pattern is given at all for the creation of a directory,
        /// this pattern will be used.
        /// </summary>
        private const string TEMP_DIR_DEFAULT_PATTERN = "p31{0}";

        #region singleton
        private static TempFileManager singleton;
        static TempFileManager()
        {
            string prefix = ProcessHelper.GetCurrentProcessName();
            int lastDot = prefix.LastIndexOf('.');
            if (lastDot > -1)
            {
                prefix = prefix.Substring(0, lastDot);
            }
            string currentDir = Environment.CurrentDirectory;
            string suffix;
            if (currentDir == null)
                suffix = "";
            else
                suffix = currentDir.GetHashCode().ToString(CultureInfo.InvariantCulture);

            singleton = new TempFileManager(Path.Combine(Path.GetTempPath(), prefix + suffix), true);
        }
        public static TempFileManager Instance
        {
            get
            {
                return singleton;
            }
        }
        #endregion

        /// <summary>
        /// Returns true if the given path lies in the scope of the temp file manager.
        /// </summary>
        public bool IsPathContained(string path)
        {
            if (path == null)
                return false;
            else
            {
                string pattern = "^" + Regex.Escape(tempRoot.FullName) + Regex.Escape(@"\");
                return Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// The root directory where temp dirs/files will be created.
        /// </summary>
        private readonly DirectoryInfo tempRoot;
        private readonly bool deleteOnExit;

        /// <summary>
        /// Singleton.  Use Instance property.
        /// </summary>
        private TempFileManager(string rootPath, bool deleteOnExit)
        {
            this.deleteOnExit = deleteOnExit;
            tempRoot = new DirectoryInfo(rootPath);
            tempRoot.Create();  // does nothing if dir already exists
        }

        /// <summary>
        /// Call when done using this instance of TempFileManager to delete
        /// all temp files (if deleteOnExit == true).
        /// </summary>
        public void Dispose()
        {
            if (deleteOnExit)
            {
                try
                {
                    if (tempRoot.Exists)
                        DeleteAll(tempRoot);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Returns the root temp dir
        /// </summary>
        protected DirectoryInfo TempDir
        {
            get
            {
                DirectoryInfo pathInfo = new DirectoryInfo(tempRoot.FullName);
                pathInfo.Create();
                return pathInfo;
            }
        }

        private void DeleteAll(DirectoryInfo dir)
        {
            try
            {
                dir.Delete(true);
            }
            catch
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    try
                    {
                        file.Attributes = FileAttributes.Normal;
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Unable to delete file " + file.FullName + ": " + e.GetType().FullName + ": " + e.Message);
                    }
                }

                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    DeleteAll(subdir);
                }
            }
        }

        public string CreateTempFile()
        {
            return CreateTempFile(TEMP_FILE_DEFAULT_PATTERN);
        }

        public string CreateTempFile(string pattern)
        {
            // for now, always return exactly the right name by creating a temp dir

            //return CreateNewFile(this.TempDir.FullName, pattern, false);
            return CreateNewFile(CreateTempDir(), pattern, false);
        }

        public string CreateTempDir()
        {
            return CreateTempDir(Guid.NewGuid().ToString().Split('-')[4].ToUpper(CultureInfo.InvariantCulture));
        }

        public string CreateTempDir(string pattern)
        {
            return CreateNewFile(this.TempDir.FullName, pattern, true);
        }

        public string Duplicate(string filePath)
        {
            string newPath = CreateTempFile(Path.GetFileName(filePath));
            File.Copy(filePath, newPath, true);
            // make the copy writable
            File.SetAttributes(newPath, File.GetAttributes(newPath) & ~FileAttributes.ReadOnly);
            try
            {
                File.SetCreationTime(newPath, File.GetCreationTime(filePath));
                File.SetLastWriteTime(newPath, File.GetLastWriteTime(filePath));
            }
            catch (Exception e)
            {
                // not the end of the world
                Debug.WriteLine("Unable to set file creation/modification time: " + e.ToString());
            }
            return newPath;
        }

        /// <summary>
        /// Creates a new file based on a pattern, dealing with
        /// name collisions by using an incrementing number as
        /// part of the file name.  The pattern should be the
        /// filename you are trying to match.
        ///
        /// For example, the pattern "my file.txt" might result in
        /// "my file.txt", "my file[1].txt", or "my file[23].txt".
        /// </summary>
        public static string CreateNewFile(string dir, string pattern, bool asDir)
        {
            // Make sure the pattern isn't too far out of whack
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (pattern.Trim().Length == 0)
                throw new ArgumentException("Temp file pattern cannot be a zero-length string", "pattern");
            if (pattern.IndexOf(Path.DirectorySeparatorChar) != -1 || pattern.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                throw new ArgumentException("Temp file pattern contains invalid path characters", "pattern");

            // If the pattern is just a normal filename, add {0} right before the extension
            pattern = pattern.Replace("{", "{{").Replace("}", "}}");

            DirectoryInfo tempDir = new DirectoryInfo(dir);
            string tempPath = tempDir.FullName;

            string fileName = Path.GetFileNameWithoutExtension(pattern);
            string fileExt = Path.GetExtension(pattern);

            // Fix 597390: Watson: System.IO.PathTooLongException: The specified path, file name, or both are too long
            if ((tempPath.Length + fileName.Length + fileExt.Length + 20 > Kernel32.MAX_PATH) && fileName.Length > 30)
            {
                fileName = StringHelper.RestrictLength(fileName, 30);
            }

            pattern = fileName + "{0}" + fileExt;

            int suffixCounter = 0;

            bool createdDir = false;

            // keep trying until it succeeds or an unhandled exception is thrown
            while (true)
            {
                // If this is our first pass through, use ""; otherwise use "[i]"
                string suffix = suffixCounter == 0 ? "" : "[" + suffixCounter.ToString(CultureInfo.CurrentCulture) + "]";
                suffixCounter++;
                string fullFileName = Path.Combine(tempPath, string.Format(CultureInfo.InvariantCulture, pattern, suffix));

                if (File.Exists(fullFileName) || Directory.Exists(fullFileName))
                {
                    continue;
                }

                if (!asDir)
                {
                    try
                    {
                        // This version of .Open will create the new file if and only if it doesn't
                        // exist already.  If it does exist, an IOException will be thrown.  I'm guessing
                        // that doing it this way will be atomic, which removes the need for me to do
                        // my own synchronization across processes.
                        using (FileStream fs = File.Open(fullFileName, FileMode.CreateNew, FileAccess.Write))
                        {
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (!createdDir && !Directory.Exists(tempPath))
                        {
                            createdDir = true;
                            Directory.CreateDirectory(tempPath);
                            suffixCounter--;
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (IOException)
                    {
                        if (!File.Exists(fullFileName) && !Directory.Exists(fullFileName))
                        {
                            // Exception must have occurred for some reason other than
                            // the file already existing... so rethrow
                            throw;
                        }
                        else
                        {
                            // In the event of the file already existing, we want to keep
                            // trying until we find a number that has not been used
                            continue;
                        }
                    }
                }

                // BUG IN C# COMPILER: http://www.jelovic.com/weblog/e49.htm
                // Using separate "if" (instead of "else") to avoid bug.
                if (asDir)
                {
                    // CreateDirectory succeeds even if a file OR directory already exists with the
                    // name we want to use.  We can't do anything about the case where the directory
                    // has been created since the check in the if() above, but we can at least check
                    // that a *file* creation snuck in (since di.Exists will return false).
                    DirectoryInfo di = Directory.CreateDirectory(fullFileName);
                    if (!di.Exists)
                        continue;
                }

                return fullFileName;
            }
        }
    }
}
