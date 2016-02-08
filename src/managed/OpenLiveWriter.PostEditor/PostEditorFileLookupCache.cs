// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Speeds up lookups of wpost files by caching the blogid and postid of each file.
    /// </summary>
    public class PostEditorFileLookupCache
    {
        public delegate PostKey PostKeyGenerator(FileInfo file);

        private readonly DirectoryInfo dir;
        private readonly string cacheFilename;
        private readonly PostKeyGenerator pkg;
        private readonly string pattern;

        protected PostEditorFileLookupCache(DirectoryInfo dir, string cacheFilename, PostKeyGenerator pkg, string pattern)
        {
            this.dir = dir;
            this.cacheFilename = cacheFilename;
            this.pkg = pkg;
            this.pattern = pattern;
        }

        public static FileInfo Lookup(DirectoryInfo dir, string cacheFilename, PostKeyGenerator pkg, string pattern, string blogId, string postId)
        {
            return new PostEditorFileLookupCache(dir, cacheFilename, pkg, pattern).Lookup(blogId, postId);
        }

        protected FileInfo Lookup(string blogId, string postId)
        {
            if (string.IsNullOrEmpty(blogId) || string.IsNullOrEmpty(postId))
            {
                Debug.Fail("Doesn't make sense to lookup blogId or postId that is null/empty");
                return null;
            }

            Dictionary<FileIdentity, PostKey> cache = GetCache();
            foreach (KeyValuePair<FileIdentity, PostKey> pair in cache)
                if (pair.Value.BlogId == blogId && pair.Value.PostId == postId)
                    return new FileInfo(Path.Combine(dir.FullName, pair.Key.Filename));
            return null;
        }

        private Dictionary<FileIdentity, PostKey> GetCache()
        {
            // Ensure that the cache is fully up to date with respect to
            // what's actually on disk.

            Dictionary<FileIdentity, PostKey> filesInCache = LoadCache();
            bool dirty = false;

            // Read all files on disk--this is cheap since we don't actually look inside them
            Dictionary<string, FileIdentity> filesOnDisk = new Dictionary<string, FileIdentity>();
            foreach (FileInfo fi in dir.GetFiles(pattern, SearchOption.TopDirectoryOnly))
                filesOnDisk.Add(fi.Name, new FileIdentity(fi));

            // Remove any entries in the cache that are either no longer on disk or are
            // stale (file on disk has changed, according to size or last modified).
            // For any entries that are on disk and have an up-to-date copy in the cache,
            // remove from filesOnDisk to ensure they get ignored for the next step.
            // This step should also be cheap, no I/O involved.
            foreach (FileIdentity fid in new List<FileIdentity>(filesInCache.Keys))
            {
                FileIdentity fidDisk;
                if (!filesOnDisk.TryGetValue(fid.Filename, out fidDisk))
                    fidDisk = null;

                if (fidDisk == null || !fidDisk.Equals(fid))
                {
                    dirty = true;
                    filesInCache.Remove(fid);
                }
                else
                    filesOnDisk.Remove(fid.Filename);
            }

            // Anything left in filesOnDisk needs to be added to the cache, expensively.
            if (filesOnDisk.Count > 0)
            {
                foreach (FileIdentity fid in filesOnDisk.Values)
                {
                    PostKey postKey = pkg(new FileInfo(Path.Combine(dir.FullName, fid.Filename)));
                    if (postKey != null)
                    {
                        dirty = true;
                        filesInCache[fid] = postKey;
                    }
                }
            }

            // Only persist if changes were made
            if (dirty)
            {
                SaveCache(filesInCache);
            }

            return filesInCache;
        }

        private Dictionary<FileIdentity, PostKey> LoadCache()
        {
            string cacheFile = CacheFile;
            if (File.Exists(cacheFile))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(cacheFile);

                    Dictionary<FileIdentity, PostKey> result = new Dictionary<FileIdentity, PostKey>();
                    foreach (XmlElement el in xmlDoc.SelectNodes("/cache/file"))
                    {
                        string filename = el.GetAttribute("name");
                        DateTime lastModified = new DateTime(long.Parse(el.GetAttribute("ticks"), CultureInfo.InvariantCulture), DateTimeKind.Utc);
                        long size = long.Parse(el.GetAttribute("size"), CultureInfo.InvariantCulture);

                        string blogId = el.GetAttribute("blogId");
                        string postId = el.GetAttribute("postId");

                        if (string.IsNullOrEmpty(filename))
                        {
                            Trace.Fail("Corrupted entry in cache");
                            continue;
                        }

                        result.Add(new FileIdentity(filename, lastModified, size), new PostKey(blogId, postId));
                    }

                    return result;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Failed to load post cache file " + cacheFile + ": " + e.ToString());
                }
            }

            return new Dictionary<FileIdentity, PostKey>();
        }

        private string CacheFile
        {
            get { return Path.Combine(dir.FullName, cacheFilename); }
        }

        private void SaveCache(Dictionary<FileIdentity, PostKey> cache)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement cacheEl = xmlDoc.CreateElement("cache");
            xmlDoc.AppendChild(cacheEl);
            foreach (KeyValuePair<FileIdentity, PostKey> pair in cache)
            {
                XmlElement fileEl = xmlDoc.CreateElement("file");
                fileEl.SetAttribute("name", pair.Key.Filename);
                fileEl.SetAttribute("ticks", pair.Key.LastModified.Ticks.ToString(CultureInfo.InvariantCulture));
                fileEl.SetAttribute("size", pair.Key.Size.ToString(CultureInfo.InvariantCulture));
                fileEl.SetAttribute("blogId", pair.Value.BlogId);
                fileEl.SetAttribute("postId", pair.Value.PostId);
                cacheEl.AppendChild(fileEl);
            }

            xmlDoc.Save(CacheFile);
        }

        public class PostKey : IEquatable<PostKey>
        {
            private readonly string blogId;
            private readonly string postId;

            public PostKey(string blogId, string postId)
            {
                this.blogId = blogId ?? "";
                this.postId = postId ?? "";
            }

            public string BlogId
            {
                get { return blogId; }
            }

            public string PostId
            {
                get { return postId; }
            }

            public bool Equals(PostKey postKey)
            {
                if (postKey == null) return false;
                return Equals(blogId, postKey.blogId) && Equals(postId, postKey.postId);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                return Equals(obj as PostKey);
            }

            public override int GetHashCode()
            {
                return (blogId != null ? blogId.GetHashCode() : 0) + 29 * (postId != null ? postId.GetHashCode() : 0);
            }
        }

        public class FileIdentity : IEquatable<FileIdentity>
        {
            private readonly string filename;
            private readonly DateTime lastModified;
            private readonly long size;

            public FileIdentity(string filename, DateTime lastModified, long size)
            {
                this.filename = filename;
                this.lastModified = lastModified;
                this.size = size;
            }

            public FileIdentity(FileInfo fi)
            {
                filename = fi.Name;
                lastModified = fi.LastWriteTimeUtc;
                size = fi.Length;
            }

            public string Filename
            {
                get { return filename; }
            }

            public DateTime LastModified
            {
                get { return lastModified; }
            }

            public long Size
            {
                get { return size; }
            }

            public bool Equals(FileIdentity fileIdentity)
            {
                if (fileIdentity == null) return false;
                if (!Equals(filename, fileIdentity.filename)) return false;
                if (!Equals(lastModified, fileIdentity.lastModified)) return false;
                if (size != fileIdentity.size) return false;
                return true;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                return Equals(obj as FileIdentity);
            }

            public override int GetHashCode()
            {
                int result = filename.GetHashCode();
                result = 29 * result + lastModified.GetHashCode();
                result = 29 * result + (int)size;
                return result;
            }
        }
    }
}
