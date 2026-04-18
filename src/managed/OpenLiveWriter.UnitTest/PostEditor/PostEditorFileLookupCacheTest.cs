// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestFixture]
    public class PostEditorFileLookupCacheTest
    {
        private DirectoryInfo tempDir;
        private string _installDir;
        private string blogId1 = Guid.NewGuid().ToString();
        private string blogId2 = Guid.NewGuid().ToString();

        [SetUp]
        public void SetUp()
        {
            var installDir = Path.Combine(Path.GetTempPath(), "OLWTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(installDir);
            var userData = Path.Combine(installDir, "UserData");
            Directory.CreateDirectory(userData);
            Directory.CreateDirectory(Path.Combine(userData, "AppData", "Roaming"));
            Directory.CreateDirectory(Path.Combine(userData, "AppData", "Local"));
            _installDir = installDir;

            ApplicationEnvironment.Initialize(
                Assembly.GetExecutingAssembly(),
                installDir);

            tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            CreateBlogPost(blogId1, "1", "foo");
            CreateBlogPost(blogId1, "2", "bar");
            CreateBlogPost(blogId2, "1", "baz");
            CreateBlogPost(blogId2, "", "boo");
        }

        private void CreateBlogPost(string blogId, string postId, string title)
        {
            BlogPost post = new BlogPost();
            if (postId != null)
                post.Id = postId;
            post.Title = title;
            BlogPostEditingContext ctx = new BlogPostEditingContext(blogId, post);
            PostEditorFile file = PostEditorFile.CreateNew(tempDir);
            file.SaveBlogPost(ctx);
        }

        private FileInfo Lookup(string blogId, string postId)
        {
            return PostEditorFileLookupCache.Lookup(tempDir, "cache.xml", PostEditorFile.ReadFile, "*.wpost", blogId, postId);
        }

        [Test]
        public void ExistingPost()
        {
            Assert.AreEqual(Lookup(blogId1, "1").Name, "foo.wpost");
            Assert.AreEqual(Lookup(blogId1, "1").Name, "foo.wpost");
            Assert.AreEqual(Lookup(blogId1, "1").Name, "foo.wpost");
            Assert.AreEqual(Lookup(blogId1, "2").Name, "bar.wpost");
            Assert.AreEqual(Lookup(blogId2, "1").Name, "baz.wpost");
        }

        [Test]
        public void NonExistentPost()
        {
            Assert.IsNull(Lookup(blogId1, "99999"));
        }

        [Test]
        public void PostLifeCycle()
        {
            Assert.IsNull(Lookup(blogId1, "501"));

            CreateBlogPost(blogId1, "501", "newPost");
            Assert.AreEqual(Lookup(blogId1, "501").Name, "newPost.wpost");

            File.Delete(Path.Combine(tempDir.FullName, "newPost.wpost"));
            Assert.IsNull(Lookup(blogId1, "501"));
        }

        [Test]
        public void CorruptedCache()
        {
            string cachePath = Path.Combine(tempDir.FullName, "cache.xml");
            using (FileStream fs = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.None))
                fs.WriteByte(0);
            ExistingPost();
        }

        [TearDown]
        public void TearDown()
        {
            try { Directory.Delete(tempDir.FullName, true); }
            catch (IOException) { }

            try
            {
                if (_installDir != null && Directory.Exists(_installDir))
                    Directory.Delete(_installDir, true);
            }
            catch (IOException)
            {
                // XmlFileSettingsPersister may still hold file handles; temp dirs
                // will be cleaned up by the OS.
            }
        }

    }
}
